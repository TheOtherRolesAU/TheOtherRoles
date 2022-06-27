#include <cstdio>
#include <direct.h>
#include <stdexcept>
#include <string>
#include <thread>
#include <windows.h>
#include <tlhelp32.h>

#include "include/zip/zip.h"

#pragma comment(lib, "Urlmon.lib")


using namespace std;

string GetArgument(int argc, char* argv[], string match)
{
    for (int i = 0; i < argc; ++i)
    {
        if (argv[i] != match) continue;
        if (i + 1 == argc) continue;
        return argv[i + 1];
    }

    throw invalid_argument("Missing Argument: " + match);
}

int on_extract_entry(const char *_, void *__)
{
    return 0;
}

bool IsProcessRunning(const wchar_t *processName)
{
    bool exists = false;
    PROCESSENTRY32 entry;
    entry.dwSize = sizeof(PROCESSENTRY32);

    HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);

    if (Process32First(snapshot, &entry))
        while (Process32Next(snapshot, &entry))
            if (!_wcsicmp(entry.szExeFile, processName))
                exists = true;

    CloseHandle(snapshot);
    return exists;
}


int main(int argc, char* argv[])
{
    auto gamePath = GetArgument(argc, argv, "--game-path");
    auto zipPath = GetArgument(argc, argv, "--zip");
    const wstring processName( L"Among Us.exe" );
    
    while(IsProcessRunning(processName.c_str()))
    {
        this_thread::sleep_for(200ms);
    }
    
    int argzip = 0;
    zip_extract(zipPath.c_str(), gamePath.c_str(), on_extract_entry, &argzip);

    remove(zipPath.c_str());
    MessageBoxW(NULL, L"An update has been queued, please restart your game!", L"The Other Roles - Updater", MB_ICONINFORMATION | MB_OK);
}
