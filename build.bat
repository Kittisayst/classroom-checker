@echo off
setlocal
echo ========================================================
echo  Building Classroom Checker (.NET Framework 4.8 x64)
echo ========================================================

set MSBUILD="C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\MSBuild.exe"

if not exist %MSBUILD% (
    echo [ERROR] MSBuild not found at: %MSBUILD%
    exit /b 1
)

%MSBUILD% "%~dp0ClassroomChecker.sln" -p:Configuration=Release -p:Platform=x64 -p:UseSharedCompilation=false -nr:false

if %ERRORLEVEL% equ 0 (
    echo.
    echo ========================================================
    echo  BUILD SUCCESSFUL!
    echo  Output: %~dp0src\ClassroomChecker\bin\Release\ClassroomChecker.exe
    echo ========================================================
) else (
    echo.
    echo [ERROR] Build failed with exit code %ERRORLEVEL%
)

exit /b %ERRORLEVEL%
