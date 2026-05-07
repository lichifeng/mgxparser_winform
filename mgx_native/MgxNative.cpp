#include "MgxNative.h"
#include <string.h>

/* C API from mgx.lib (Rust static library) */
/* lang: NULL or "" → Chinese, "en" → English */
extern "C" {
    char* mgx_parse_file(const char* path, const char* lang);
    void  mgx_free_string(char* ptr);
    char* mgx_parse_bytes(const unsigned char* data, size_t len, const char* filename, const char* lang);
}

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Text;

String^ MgxNative::Parser::ParseFile(String^ path)
{
    /* Convert .NET String to UTF-8 (null-terminated) for the C API */
    array<unsigned char>^ pathBytes = Encoding::UTF8->GetBytes(path);
    IntPtr nativePath = Marshal::AllocHGlobal(pathBytes->Length + 1);
    Marshal::Copy(pathBytes, 0, nativePath, pathBytes->Length);
    Marshal::WriteByte(nativePath, pathBytes->Length, 0);

    char* result = mgx_parse_file(static_cast<const char*>(nativePath.ToPointer()), NULL);

    Marshal::FreeHGlobal(nativePath);

    if (result == nullptr)
        return nullptr;

    /* Convert UTF-8 result to .NET String */
    int resultLen = (int)strlen(result);
    array<unsigned char>^ resultBytes = gcnew array<unsigned char>(resultLen);
    Marshal::Copy(IntPtr(result), resultBytes, 0, resultLen);
    String^ json = Encoding::UTF8->GetString(resultBytes);

    mgx_free_string(result);
    return json;
}

String^ MgxNative::Parser::ParseBytes(array<unsigned char>^ data, String^ filename)
{
    /* Pin the managed byte array so the C API can read it */
    pin_ptr<unsigned char> pinnedData = &data[0];
    const unsigned char* nativeData = pinnedData;

    /* Convert filename to UTF-8 */
    array<unsigned char>^ nameBytes = Encoding::UTF8->GetBytes(filename);
    IntPtr nativeName = Marshal::AllocHGlobal(nameBytes->Length + 1);
    Marshal::Copy(nameBytes, 0, nativeName, nameBytes->Length);
    Marshal::WriteByte(nativeName, nameBytes->Length, 0);

    char* result = mgx_parse_bytes(nativeData, data->Length,
        static_cast<const char*>(nativeName.ToPointer()), NULL);

    Marshal::FreeHGlobal(nativeName);

    if (result == nullptr)
        return nullptr;

    /* Convert UTF-8 result to .NET String */
    int resultLen = (int)strlen(result);
    array<unsigned char>^ resultBytes = gcnew array<unsigned char>(resultLen);
    Marshal::Copy(IntPtr(result), resultBytes, 0, resultLen);
    String^ json = Encoding::UTF8->GetString(resultBytes);

    mgx_free_string(result);
    return json;
}
