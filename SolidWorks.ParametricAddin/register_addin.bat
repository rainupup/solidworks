@echo off
REM SolidWorks Parametric Add-in Registration Script
REM Run as Administrator for system-wide installation, or as current user for per-user.
REM
REM Usage:
REM   register_addin.bat          - Per-user registration (default)
REM   register_addin.bat admin    - System-wide registration (requires Admin)

setlocal

set TARGET_DIR=%~dp0bin\Debug
set DLL=%TARGET_DIR%\SolidWorks.ParametricAddin.dll

if not exist "%DLL%" (
    echo ERROR: DLL not found at %DLL%
    echo Please build the project in Visual Studio first.
    exit /b 1
)

echo Registering SolidWorks Parametric Add-in...
echo DLL: %DLL%

REM Find RegAsm.exe
set REGASM=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe

if not exist "%REGASM%" (
    echo ERROR: RegAsm.exe not found. .NET Framework 4.8 required.
    exit /b 1
)

if "%1"=="admin" (
    echo Running as system-wide registration...
    "%REGASM%" /codebase "%DLL%"
) else (
    echo Running as per-user registration...
    "%REGASM%" /codebase "%DLL%" /nologo
)

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Registration successful!
    echo Restart SolidWorks to load the add-in.
    echo.
    echo To unregister, run: "%REGASM%" /unregister "%DLL%"
) else (
    echo.
    echo Registration FAILED. Try running as Administrator:
    echo   register_addin.bat admin
)

endlocal
pause
