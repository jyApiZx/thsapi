// 2.0 交易示例（C++）
// 1) 运行 xdDemo.exe 登录自己的券商或模拟盘一次
// 2) 把本文件与 thsapi.dll、login_prefs.json 放同一目录后编译运行
// 3) 结果为 UTF-8，用完 FreeApiBuffer；无需预分配 1MB Buffer
// 位数必须与 DLL 一致（优先 x64）

#include <Windows.h>
#include <cstdio>
#include <cstring>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

typedef void(__stdcall* OpenTdxFn)();
typedef void(__stdcall* CloseTdxFn)();
typedef int(__stdcall* LogonFn)(const char*, short, const char*, short,
    const char*, const char*, const char*, const char*, char*);
typedef void(__stdcall* LogoffFn)(int);
typedef int(__stdcall* QueryDataAllocFn)(int, int, char**, char**);
typedef void(__stdcall* FreeApiBufferFn)(char*);

static std::string ReadFile(const std::string& path) {
    std::ifstream in(path, std::ios::binary);
    if (!in) return {};
    return std::string((std::istreambuf_iterator<char>(in)), std::istreambuf_iterator<char>());
}

// 极简 JSON 字符串字段提取（仅够读 login_prefs）
static std::string JsonStr(const std::string& obj, const char* key) {
    std::string pat = std::string("\"") + key + "\"";
    size_t p = obj.find(pat);
    if (p == std::string::npos) return {};
    p = obj.find(':', p + pat.size());
    if (p == std::string::npos) return {};
    p = obj.find('"', p + 1);
    if (p == std::string::npos) return {};
    size_t q = p + 1;
    std::string out;
    while (q < obj.size()) {
        char c = obj[q++];
        if (c == '\\' && q < obj.size()) { out.push_back(obj[q++]); continue; }
        if (c == '"') break;
        out.push_back(c);
    }
    return out;
}

static int JsonInt(const std::string& obj, const char* key, int defv = 0) {
    std::string pat = std::string("\"") + key + "\"";
    size_t p = obj.find(pat);
    if (p == std::string::npos) return defv;
    p = obj.find(':', p + pat.size());
    if (p == std::string::npos) return defv;
    return atoi(obj.c_str() + p + 1);
}

static bool JsonBool(const std::string& obj, const char* key) {
    std::string pat = std::string("\"") + key + "\"";
    size_t p = obj.find(pat);
    if (p == std::string::npos) return false;
    p = obj.find(':', p + pat.size());
    if (p == std::string::npos) return false;
    p = obj.find_first_not_of(" \t\r\n\"", p + 1);
    if (p == std::string::npos) return false;
    return obj.compare(p, 4, "true") == 0 || obj[p] == '1';
}

static std::string JsonObjRaw(const std::string& obj, const char* key) {
    std::string pat = std::string("\"") + key + "\"";
    size_t p = obj.find(pat);
    if (p == std::string::npos) return {};
    p = obj.find('{', p + pat.size());
    if (p == std::string::npos) return {};
    int depth = 0;
    for (size_t i = p; i < obj.size(); ++i) {
        if (obj[i] == '{') depth++;
        else if (obj[i] == '}') {
            depth--;
            if (depth == 0) return obj.substr(p, i - p + 1);
        }
    }
    return {};
}

static std::string PickAccountBlock(const std::string& json) {
    std::string last = JsonStr(json, "lastAccount");
    size_t accounts = json.find("\"accounts\"");
    if (accounts == std::string::npos) return {};
    if (!last.empty()) {
        std::string key = std::string("\"") + last + "\"";
        size_t k = json.find(key, accounts);
        if (k != std::string::npos) {
            size_t brace = json.find('{', k + key.size());
            if (brace != std::string::npos) {
                int depth = 0;
                for (size_t i = brace; i < json.size(); ++i) {
                    if (json[i] == '{') depth++;
                    else if (json[i] == '}') {
                        depth--;
                        if (depth == 0) return json.substr(brace, i - brace + 1);
                    }
                }
            }
        }
    }
    // 回退：取 accounts 里第一个对象
    size_t brace = json.find('{', accounts + 10);
    if (brace == std::string::npos) return {};
    int depth = 0;
    for (size_t i = brace; i < json.size(); ++i) {
        if (json[i] == '{') depth++;
        else if (json[i] == '}') {
            depth--;
            if (depth == 0) return json.substr(brace, i - brace + 1);
        }
    }
    return {};
}

int main() {
    SetConsoleOutputCP(65001); // UTF-8 控制台
    HMODULE dll = LoadLibraryA("thsapi.dll");
    if (!dll) {
        std::cerr << "找不到 thsapi.dll（请与本程序同目录）\n";
        return 1;
    }

    auto OpenTdx = (OpenTdxFn)GetProcAddress(dll, "OpenTdx");
    auto CloseTdx = (CloseTdxFn)GetProcAddress(dll, "CloseTdx");
    auto Logon = (LogonFn)GetProcAddress(dll, "Logon");
    auto Logoff = (LogoffFn)GetProcAddress(dll, "Logoff");
    auto QueryDataAlloc = (QueryDataAllocFn)GetProcAddress(dll, "QueryDataAlloc");
    auto FreeApiBuffer = (FreeApiBufferFn)GetProcAddress(dll, "FreeApiBuffer");
    if (!OpenTdx || !CloseTdx || !Logon || !Logoff || !QueryDataAlloc || !FreeApiBuffer) {
        std::cerr << "导出函数不完整，请确认是 2.0 版 thsapi.dll\n";
        return 1;
    }

    std::string json = ReadFile("login_prefs.json");
    if (json.empty()) {
        std::cerr << "找不到 login_prefs.json。请先运行 xdDemo.exe 登录一次。\n";
        return 1;
    }
    std::string acc = PickAccountBlock(json);
    std::string ip = JsonStr(acc, "serverIp");
    short port = (short)JsonInt(acc, "serverPort", 0);
    short yyb = (short)JsonInt(acc, "yybId", 0);
    std::string account = JsonStr(acc, "account");
    std::string trade = JsonStr(acc, "tradeAccount");
    std::string jy = JsonStr(acc, "jyPassword");
    std::string tx = JsonStr(acc, "txPassword");
    std::string config = JsonObjRaw(acc, "config");
    if (config.empty()) config = JsonStr(acc, "config");
    if (config.empty()) {
        std::string qs = JsonStr(acc, "apiQsid");
        if (qs.empty()) qs = std::to_string(JsonInt(acc, "apiQsid", 0));
        if (qs == "0") qs = std::to_string(JsonInt(acc, "websiteQsid", 0));
        std::string at = JsonStr(acc, "accountTypeCode");
        if (at.empty()) at = "k";
        config = "{\"qs_id\":" + qs + ",\"account_type\":\"" + at + "\"}";
    }
    // 信用账户：登录账号前加 X；未单独保存交易账号时同样加 X
    bool credit = JsonBool(acc, "isCredit");
    bool tradeGiven = !trade.empty();
    if (!tradeGiven) trade = account;
    if (credit && account.rfind("X", 0) != 0 && account.rfind("x", 0) != 0) account = "X" + account;
    if (credit && !tradeGiven && trade.rfind("X", 0) != 0 && trade.rfind("x", 0) != 0) trade = "X" + trade;

    OpenTdx();
    char err[1024] = {0};
    int cid = Logon(ip.c_str(), port, config.c_str(), yyb,
        account.c_str(), trade.c_str(), jy.c_str(), tx.c_str(), err);
    if (cid < 0) {
        std::cerr << "登录失败: " << err << "\n";
        CloseTdx();
        return 1;
    }
    std::cout << "登录成功 ClientID=" << cid << "\n";

    char* result = nullptr;
    char* errInfo = nullptr;
    int rc = QueryDataAlloc(cid, 0, &result, &errInfo); // 0=资金
    if (errInfo && errInfo[0]) {
        std::cerr << "查询失败: " << errInfo << "\n";
    } else if (result) {
        std::cout << "资金表:\n" << result << "\n";
        (void)rc;
    }
    if (result) FreeApiBuffer(result);
    if (errInfo) FreeApiBuffer(errInfo);

    Logoff(cid);
    CloseTdx();
    FreeLibrary(dll);
    return 0;
}
