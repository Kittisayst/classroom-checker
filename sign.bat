@echo off
setlocal
REM ========================================================
REM  sign.bat - sign dist\ClassroomChecker.exe with signtool
REM
REM  Requires your own code-signing certificate (.pfx). See docs\code-signing.md
REM  Never put the password in this file or commit a .pfx to git.
REM
REM  Usage:
REM    set SIGN_PFX=D:\secure\ClassroomChecker.pfx
REM    set SIGN_PASSWORD=your-password
REM    sign.bat
REM
REM  Optional: SIGNTOOL (full path to signtool.exe), SIGN_TIMESTAMP (RFC3161 URL)
REM ========================================================

if "%SIGN_PFX%"=="" goto :nopfx
if not exist "%SIGN_PFX%" goto :nopfxfile
if "%SIGN_PASSWORD%"=="" goto :nopass

set EXE=%~dp0dist\ClassroomChecker.exe
if not exist "%EXE%" goto :noexe

if not "%SIGNTOOL%"=="" goto :havetool
set PF86=%ProgramFiles(x86)%
for /f "delims=" %%i in ('dir /b /s "%PF86%\Windows Kits\10\bin\signtool.exe" 2^>nul ^| findstr /i "\\x64\\"') do set SIGNTOOL=%%i
if "%SIGNTOOL%"=="" goto :notool

:havetool
REM A timestamp keeps the signature valid after the certificate expires.
REM Internet is needed only while signing, not on the classroom machines.
if "%SIGN_TIMESTAMP%"=="" set SIGN_TIMESTAMP=http://timestamp.digicert.com

"%SIGNTOOL%" sign /fd SHA256 /f "%SIGN_PFX%" /p "%SIGN_PASSWORD%" /tr "%SIGN_TIMESTAMP%" /td SHA256 /d "Classroom Checker" "%EXE%"
if errorlevel 1 goto :signfail

"%SIGNTOOL%" verify /pa "%EXE%"
exit /b %ERRORLEVEL%

:nopfx
echo [ERROR] SIGN_PFX is not set. Set it to the path of your .pfx file.
echo         See docs\code-signing.md
exit /b 1

:nopfxfile
echo [ERROR] Certificate file not found: %SIGN_PFX%
exit /b 1

:nopass
echo [ERROR] SIGN_PASSWORD is not set.
exit /b 1

:noexe
echo [ERROR] %EXE% not found. Run package.bat first.
exit /b 1

:notool
echo [ERROR] signtool.exe not found. Install the Windows SDK or set SIGNTOOL.
exit /b 1

:signfail
echo [ERROR] Signing failed.
exit /b 1
