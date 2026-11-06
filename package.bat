@echo off
setlocal
echo ========================================================
echo  Packaging Classroom Checker for USB Distribution
echo ========================================================

set MSBUILD="C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\MSBuild.exe"

if not exist %MSBUILD% (
    echo [ERROR] MSBuild not found at: %MSBUILD%
    exit /b 1
)

echo [1/3] Building Release x64 binary...
%MSBUILD% "%~dp0ClassroomChecker.sln" -p:Configuration=Release -p:Platform=x64 -p:UseSharedCompilation=false -nr:false

if %ERRORLEVEL% neq 0 (
    echo.
    echo [ERROR] Build failed with exit code %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

echo.
echo [2/3] Preparing dist folder...
if not exist "%~dp0dist" mkdir "%~dp0dist"
if not exist "%~dp0dist\backgrounds" mkdir "%~dp0dist\backgrounds"
if not exist "%~dp0dist\specs" mkdir "%~dp0dist\specs"

echo [3/3] Copying executable to dist...
copy /Y "%~dp0src\ClassroomChecker\bin\Release\ClassroomChecker.exe" "%~dp0dist\ClassroomChecker.exe"

if %ERRORLEVEL% equ 0 (
    echo.
    echo ========================================================
    echo  PACKAGE CREATED SUCCESSFULLY!
    echo  USB Distribution Directory: %~dp0dist
    echo  Executable: %~dp0dist\ClassroomChecker.exe
    echo ========================================================
) else (
    echo.
    echo [ERROR] Copying executable failed.
)

exit /b %ERRORLEVEL%
