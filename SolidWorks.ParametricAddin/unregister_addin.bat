@echo off
REM Unregister the SolidWorks Parametric Add-in

setlocal

set TARGET_DIR=%~dp0bin\Debug
set DLL=%TARGET_DIR%\SolidWorks.ParametricAddin.dll

if not exist "%DLL%" (
    echo WARNING: DLL not found at %DLL%
    echo Checking Release build...
    set TARGET_DIR=%~dp0bin\Release
    set DLL=%TARGET_DIR%\SolidWorks.ParametricAddin.dll
)

if not exist "%DLL%" (
    echo ERROR: DLL not found. Cannot unregister.
    exit /b 1
)

set REGASM=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe

if not exist "%REGASM%" (
    echo ERROR: RegAsm.exe not found.
    exit /b 1
)

echo Unregistering SolidWorks Parametric Add-in...
"%REGASM%" /unregister "%DLL%" /nologo

if %ERRORLEVEL% EQU 0 (
    echo Unregistration successful.
) else (
    echo Unregistration FAILED. Try running as Administrator.
)

endlocal
pause
