@echo off
setlocal
echo ========================================================
echo  Building Classroom Checker (.NET Framework 4.8 x64)
echo ========================================================

if not "%MSBUILD%"=="" goto :check_msbuild

set PF86=%ProgramFiles(x86)%
set VSWHERE=%PF86%\Microsoft Visual Studio\Installer\vswhere.exe
if not exist "%VSWHERE%" goto :check_fallback

for /f "usebackq tokens=*" %%i in (`"%VSWHERE%" -latest -prerelease -products * -find MSBuild\**\Bin\MSBuild.exe 2^>nul`) do (
    if exist "%%i" (
        set MSBUILD=%%i
        goto :check_msbuild
    )
)

:check_fallback
if exist "C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD=C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\MSBuild.exe
    goto :check_msbuild
)
if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe
    goto :check_msbuild
)
if exist "C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD=C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe
    goto :check_msbuild
)
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD=C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe
    goto :check_msbuild
)
if exist "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" (
    set MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe
    goto :check_msbuild
)

:check_msbuild
if "%MSBUILD%"=="" (
    echo [ERROR] MSBuild.exe not found on system.
    exit /b 1
)

echo [INFO] Using MSBuild: "%MSBUILD%"
"%MSBUILD%" "%~dp0ClassroomChecker.sln" -p:Configuration=Release -p:Platform=x64 -p:UseSharedCompilation=false -nr:false

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
