@echo off
setlocal enabledelayedexpansion

set VS_DIR=C:\Program Files\Microsoft Visual Studio\18\Community
set VCVARS="%VS_DIR%\VC\Auxiliary\Build\vcvarsall.bat"

if not exist %VCVARS% (
    echo Error: VS not found at %VCVARS%
    exit /b 1
)

call %VCVARS% x64
if errorlevel 1 (
    echo Failed to set up VS x64 environment
    exit /b 1
)

set PROJ_DIR=%~dp0..
set SRC_DIR=%~dp0

echo === Building MgxNative.dll (C++/CLI, static link mgx.lib) ===
echo.

cl.exe /nologo /clr /LD /MD /O2 /Fo"%PROJ_DIR%\MgxNative.obj" /Fe"%PROJ_DIR%\MgxNative.dll" ^
    "%SRC_DIR%\MgxNative.cpp" ^
    "%PROJ_DIR%\mgx.lib" ^
    ws2_32.lib userenv.lib bcrypt.lib ntdll.lib

if errorlevel 1 (
    echo.
    echo *** Build FAILED ***
    exit /b 1
)

echo.
echo === Build OK: MgxNative.dll ===
echo.

:: Clean up intermediate object file
if exist "%PROJ_DIR%\MgxNative.obj" del "%PROJ_DIR%\MgxNative.obj"
if exist "%PROJ_DIR%\MgxNative.lib" del "%PROJ_DIR%\MgxNative.lib"
if exist "%PROJ_DIR%\MgxNative.exp" del "%PROJ_DIR%\MgxNative.exp"

echo Output: %PROJ_DIR%\MgxNative.dll
