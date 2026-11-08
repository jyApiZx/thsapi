// 2.0 行情示例（Java + JNA）
// 1) 运行「行情测试demo.exe」用 L1 连接一次
// 2) 与 HqApi.dll、login_prefs.json、jna.jar 同目录
//
//   javac -cp jna.jar L1QuickStart.java
//   java  -cp .;jna.jar L1QuickStart

import com.sun.jna.Memory;
import com.sun.jna.Native;
import com.sun.jna.Pointer;
import com.sun.jna.ptr.PointerByReference;
import com.sun.jna.ptr.ShortByReference;
import com.sun.jna.win32.StdCallLibrary;

import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class L1QuickStart {
    public interface HqApiLib extends StdCallLibrary {
        int TdxHq_ConnectAlloc(String ip, short port, String account, String password,
                               PointerByReference result, PointerByReference errInfo);
        void TdxHq_Disconnect(int connId);
        boolean TdxHq_GetSecurityQuotesAlloc(int connId, byte[] markets, Pointer codes,
                                             ShortByReference count,
                                             PointerByReference result, PointerByReference errInfo);
        void TdxHq_FreeBuffer(Pointer p);
    }

    static String jsonStr(String obj, String key) {
        Matcher m = Pattern.compile("\"" + Pattern.quote(key) + "\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"").matcher(obj);
        return m.find() ? m.group(1) : "";
    }

    static int jsonInt(String obj, String key, int defv) {
        Matcher m = Pattern.compile("\"" + Pattern.quote(key) + "\"\\s*:\\s*(-?\\d+)").matcher(obj);
        return m.find() ? Integer.parseInt(m.group(1)) : defv;
    }

    static String pickLaneL1(String json) {
        int lanes = json.indexOf("\"lanes\"");
        if (lanes < 0) return "";
        int k = json.indexOf("\"L1\"", lanes);
        if (k < 0) return "";
        int brace = json.indexOf('{', k);
        if (brace < 0) return "";
        int depth = 0;
        for (int i = brace; i < json.length(); i++) {
            char c = json.charAt(i);
            if (c == '{') depth++;
            else if (c == '}') {
                depth--;
                if (depth == 0) return json.substring(brace, i + 1);
            }
        }
        return "";
    }

    static String ptrUtf8(Pointer p) {
        if (p == null) return "";
        return new String(p.getByteArray(0, (int) p.indexOf(0, (byte) 0)), StandardCharsets.UTF_8);
    }

    public static void main(String[] args) throws Exception {
        System.setProperty("jna.encoding", "GBK");
        if (!Files.exists(Paths.get("login_prefs.json"))) {
            System.err.println("找不到 login_prefs.json。请先运行行情测试demo.exe 连接一次。");
            return;
        }
        String lane = pickLaneL1(new String(Files.readAllBytes(Paths.get("login_prefs.json")), StandardCharsets.UTF_8));
        String ip = jsonStr(lane, "ip");
        short port = (short) jsonInt(lane, "port", 7709);
        String account = jsonStr(lane, "account");
        String password = jsonStr(lane, "password");

        HqApiLib api = Native.load("HqApi.dll", HqApiLib.class);
        PointerByReference result = new PointerByReference();
        PointerByReference errInfo = new PointerByReference();
        int cid = api.TdxHq_ConnectAlloc(ip, port, account, password, result, errInfo);
        if (cid < 0) {
            System.err.println("连接失败: " + ptrUtf8(errInfo.getValue()));
            if (result.getValue() != null) api.TdxHq_FreeBuffer(result.getValue());
            if (errInfo.getValue() != null) api.TdxHq_FreeBuffer(errInfo.getValue());
            return;
        }
        if (result.getValue() != null) api.TdxHq_FreeBuffer(result.getValue());
        if (errInfo.getValue() != null) api.TdxHq_FreeBuffer(errInfo.getValue());
        System.out.println("连接成功 ConnID=" + cid);

        byte[] markets = new byte[]{1, 0};
        Memory codes = new Memory(Native.POINTER_SIZE * 2L);
        Memory c0 = new Memory(16); c0.setString(0, "600519");
        Memory c1 = new Memory(16); c1.setString(0, "000001");
        codes.setPointer(0, c0);
        codes.setPointer(Native.POINTER_SIZE, c1);
        ShortByReference count = new ShortByReference((short) 2);
        result = new PointerByReference();
        errInfo = new PointerByReference();
        boolean ok = api.TdxHq_GetSecurityQuotesAlloc(cid, markets, codes, count, result, errInfo);
        if (!ok) System.err.println("行情失败: " + ptrUtf8(errInfo.getValue()));
        else System.out.println(ptrUtf8(result.getValue()));
        if (result.getValue() != null) api.TdxHq_FreeBuffer(result.getValue());
        if (errInfo.getValue() != null) api.TdxHq_FreeBuffer(errInfo.getValue());

        api.TdxHq_Disconnect(cid);
    }
}
