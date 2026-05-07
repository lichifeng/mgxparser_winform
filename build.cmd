@echo off
setlocal

set PROJ_DIR=%~dp0

echo ========================================
echo  帝国时代录像管理器 - 完整构建
echo ========================================
echo.

:: Step 1: Build MgxNative.dll (C++/CLI)
echo [1/2] Building MgxNative.dll ...
call "%PROJ_DIR%mgx_native\BuildMgxNative.cmd"
if errorlevel 1 (
    echo *** C++/CLI 构建失败 ***
    exit /b 1
)

:: Step 2: Build C# WinForms project
echo [2/2] Building C# project ...
echo.

msbuild "%PROJ_DIR%mgxparser.csproj" /p:Configuration=Release /p:Platform=x64 /t:Rebuild
if errorlevel 1 (
    echo *** C# 构建失败 ***
    exit /b 1
)

echo.
echo ========================================
echo  构建完成!
echo  输出: %PROJ_DIR%bin\Release\帝国时代录像管理器.exe
echo ========================================

endlocal
