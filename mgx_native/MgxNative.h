#pragma once

namespace MgxNative {

public ref class Parser
{
public:
    static System::String^ ParseFile(System::String^ path);
    static System::String^ ParseBytes(cli::array<unsigned char>^ data, System::String^ filename);
};

}
