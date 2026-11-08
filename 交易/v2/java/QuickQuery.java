// 2.0 交易示例（Java + JNA）
// 1) 运行 xdDemo.exe 登录自己的券商或模拟盘一次
// 2) 与 thsapi.dll、login_prefs.json、jna.jar 同目录（位数与 DLL 一致）
// 3) Alloc 结果 UTF-8，用完 FreeApiBuffer
//
//   javac -cp jna.jar QuickQuery.java
//   java  -cp .;jna.jar QuickQuery

import com.sun.jna.Native;
import com.sun.jna.Pointer;
import com.sun.jna.ptr.PointerByReference;
import com.sun.jna.win32.StdCallLibrary;

import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class QuickQuery {
    public interface ThsApi extends StdCallLibrary {
        void OpenTdx();
        void CloseTdx();
        int Logon(String ip, short port, String config, short yybId,
                  String accountNo, String tradeAccount,
                  String jyPassword, String txPassword, byte[] errInfo);
        void Logoff(int clientId);
        int QueryDataAlloc(int clientId, int category, PointerByReference result, PointerByReference errInfo);
        void FreeApiBuffer(Pointer p);
    }

    static String jsonStr(String obj, String key) {
        Matcher m = Pattern.compile("\"" + Pattern.quote(key) + "\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"").matcher(obj);
        if (!m.find()) return "";
        return m.group(1).replace("\\\"", "\"").replace("\\\\", "\\");
    }

    static int jsonInt(String obj, String key, int defv) {
        Matcher m = Pattern.compile("\"" + Pattern.quote(key) + "\"\\s*:\\s*(-?\\d+)").matcher(obj);
        return m.find() ? Integer.parseInt(m.group(1)) : defv;
    }

    static boolean jsonBool(String obj, String key) {
        Matcher m = Pattern.compile("\"" + Pattern.quote(key) + "\"\\s*:\\s*\"?(true|1)", Pattern.CASE_INSENSITIVE).matcher(obj);
        return m.find();
    }

    static String jsonObjRaw(String obj, String key) {
        Matcher m = Pattern.compile("\"" + Pattern.quote(key) + "\"\\s*:\\s*\\{").matcher(obj);
        if (!m.find()) return "";
        int start = m.end() - 1;
        int depth = 0;
        for (int i = start; i < obj.length(); i++) {
            char c = obj.charAt(i);
            if (c == '{') depth++;
            else if (c == '}') {
                depth--;
                if (depth == 0) return obj.substring(start, i + 1);
            }
        }
        return "";
    }

    static String pickAccount(String json) {
        String last = jsonStr(json, "lastAccount");
        int accounts = json.indexOf("\"accounts\"");
        if (accounts < 0) return "";
        if (!last.isEmpty()) {
            String key = "\"" + last + "\"";
            int k = json.indexOf(key, accounts);
            if (k >= 0) {
                int brace = json.indexOf('{', k + key.length());
                if (brace >= 0) {
                    int depth = 0;
                    for (int i = brace; i < json.length(); i++) {
                        char c = json.charAt(i);
                        if (c == '{') depth++;
                        else if (c == '}') {
                            depth--;
                            if (depth == 0) return json.substring(brace, i + 1);
                        }
                    }
                }
            }
        }
        int first = json.indexOf('{', accounts + 10);
        if (first < 0) return "";
        int d = 0;
        for (int i = first; i < json.length(); i++) {
            char c = json.charAt(i);
            if (c == '{') d++;
            else if (c == '}') {
                d--;
                if (d == 0) return json.substring(first, i + 1);
            }
        }
        return "";
    }

    static String ptrUtf8(Pointer p) {
        if (p == null) return "";
        return new String(p.getByteArray(0, (int) p.indexOf(0, (byte) 0)), StandardCharsets.UTF_8);
    }

    public static void main(String[] args) throws Exception {
        // Logon 的账号、密码参数按 GBK 传入
        System.setProperty("jna.encoding", "GBK");
        if (!Files.exists(Paths.get("login_prefs.json"))) {
            System.err.println("找不到 login_prefs.json。请先运行 xdDemo.exe 登录一次。");
            return;
        }
        String json = new String(Files.readAllBytes(Paths.get("login_prefs.json")), StandardCharsets.UTF_8);
        String acc = pickAccount(json);
        String ip = jsonStr(acc, "serverIp");
        short port = (short) jsonInt(acc, "serverPort", 0);
        short yyb = (short) jsonInt(acc, "yybId", 0);
        String account = jsonStr(acc, "account");
        String trade = jsonStr(acc, "tradeAccount");
        String jy = jsonStr(acc, "jyPassword");
        String tx = jsonStr(acc, "txPassword");
        String config = jsonObjRaw(acc, "config");
        if (config.isEmpty()) config = jsonStr(acc, "config");
        if (config.isEmpty()) {
            String qs = jsonStr(acc, "apiQsid");
            if (qs.isEmpty()) qs = String.valueOf(jsonInt(acc, "apiQsid", 0));
            if (qs.equals("0")) qs = String.valueOf(jsonInt(acc, "websiteQsid", 0));
            String at = jsonStr(acc, "accountTypeCode");
            if (at.isEmpty()) at = "k";
            config = "{\"qs_id\":" + qs + ",\"account_type\":\"" + at + "\"}";
        }
        // 信用账户：登录账号前加 X；未单独保存交易账号时同样加 X
        boolean credit = jsonBool(acc, "isCredit");
        boolean tradeGiven = !trade.isEmpty();
        if (!tradeGiven) trade = account;
        if (credit && !account.toUpperCase().startsWith("X")) account = "X" + account;
        if (credit && !tradeGiven && !trade.toUpperCase().startsWith("X")) trade = "X" + trade;

        ThsApi api = Native.load("thsapi.dll", ThsApi.class);
        api.OpenTdx();
        byte[] err = new byte[1024];
        int cid = api.Logon(ip, port, config, yyb, account, trade, jy, tx, err);
        if (cid < 0) {
            System.err.println("登录失败: " + new String(err, "GBK").trim());
            api.CloseTdx();
            return;
        }
        System.out.println("登录成功 ClientID=" + cid);

        PointerByReference result = new PointerByReference();
        PointerByReference errInfo = new PointerByReference();
        api.QueryDataAlloc(cid, 0, result, errInfo); // 0=资金
        String e = ptrUtf8(errInfo.getValue());
        String r = ptrUtf8(result.getValue());
        if (!e.isEmpty()) System.err.println("查询失败: " + e);
        else System.out.println("资金表:\n" + r);
        if (result.getValue() != null) api.FreeApiBuffer(result.getValue());
        if (errInfo.getValue() != null) api.FreeApiBuffer(errInfo.getValue());

        api.Logoff(cid);
        api.CloseTdx();
    }
}
