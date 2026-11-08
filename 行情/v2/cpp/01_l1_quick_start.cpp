// 2.0 行情示例（C++）
// 1) 运行「行情测试demo.exe」用 L1 连接一次
// 2) 与 HqApi.dll、login_prefs.json 同目录编译运行（位数一致）

#include <Windows.h>
#include <cstdio>
#include <fstream>
#include <iostream>
#include <string>

typedef int(__stdcall* ConnectAllocFn)(const char*, unsigned short, const char*, const char*, char**, char**);
typedef void(__stdcall* DisconnectFn)(int);
typedef bool(__stdcall* QuotesAllocFn)(int, unsigned char*, const char**, unsigned short*, char**, char**);
typedef void(__stdcall* FreeBufFn)(char*);

static std::string ReadFile(const std::string& path) {
    std::ifstream in(path, std::ios::binary);
    if (!in) return {};
    return std::string((std::istreambuf_iterator<char>(in)), std::istreambuf_iterator<char>());
}

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

static std::string PickLaneL1(const std::string& json) {
    size_t lanes = json.find("\"lanes\"");
    if (lanes == std::string::npos) return {};
    size_t k = json.find("\"L1\"", lanes);
    if (k == std::string::npos) return {};
    size_t brace = json.find('{', k);
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
    SetConsoleOutputCP(65001);
    HMODULE dll = LoadLibraryA("HqApi.dll");
    if (!dll) dll = LoadLibraryA("TdxHqApi.dll");
    if (!dll) { std::cerr << "找不到 HqApi.dll\n"; return 1; }

    auto ConnectAlloc = (ConnectAllocFn)GetProcAddress(dll, "TdxHq_ConnectAlloc");
    auto Disconnect = (DisconnectFn)GetProcAddress(dll, "TdxHq_Disconnect");
    auto QuotesAlloc = (QuotesAllocFn)GetProcAddress(dll, "TdxHq_GetSecurityQuotesAlloc");
    auto FreeBuf = (FreeBufFn)GetProcAddress(dll, "TdxHq_FreeBuffer");
    if (!ConnectAlloc || !Disconnect || !QuotesAlloc || !FreeBuf) {
        std::cerr << "导出不完整，请确认是 2.0 版 HqApi.dll\n";
        return 1;
    }

    std::string json = ReadFile("login_prefs.json");
    std::string lane = PickLaneL1(json);
    if (lane.empty()) {
        std::cerr << "login_prefs.json 中没有 L1 通道。请先运行行情测试demo.exe 连接一次。\n";
        return 1;
    }
    std::string ip = JsonStr(lane, "ip");
    unsigned short port = (unsigned short)JsonInt(lane, "port", 7709);
    std::string account = JsonStr(lane, "account");
    std::string password = JsonStr(lane, "password");

    char* result = nullptr;
    char* errInfo = nullptr;
    int cid = ConnectAlloc(ip.c_str(), port, account.c_str(), password.c_str(), &result, &errInfo);
    if (cid < 0) {
        std::cerr << "连接失败: " << (errInfo ? errInfo : "") << "\n";
        if (result) FreeBuf(result);
        if (errInfo) FreeBuf(errInfo);
        return 1;
    }
    if (result) FreeBuf(result);
    if (errInfo) FreeBuf(errInfo);
    std::cout << "连接成功 ConnID=" << cid << "\n";

    unsigned char markets[2] = {1, 0}; // 沪、深
    const char* codes[2] = {"600519", "000001"};
    unsigned short count = 2;
    result = errInfo = nullptr;
    if (!QuotesAlloc(cid, markets, codes, &count, &result, &errInfo)) {
        std::cerr << "行情失败: " << (errInfo ? errInfo : "") << "\n";
    } else if (result) {
        std::cout << result << "\n";
    }
    if (result) FreeBuf(result);
    if (errInfo) FreeBuf(errInfo);

    Disconnect(cid);
    FreeLibrary(dll);
    return 0;
}
