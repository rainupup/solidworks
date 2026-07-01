@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul 2>&1
set GUID={B8E7F3D1-A2C4-4E5F-9A1B-3C6D8E0F4A2C}
set COMHOST=D:\code\solidworks\SolidWorks.ParametricAddin.ComHost\bin\Debug\net8.0-windows\SolidWorks.ParametricAddin.ComHost.comhost.dll

echo === Registering SolidWorks Parametric Add-in ===
echo.

echo [1/8] CLSID default...
reg add "HKLM\SOFTWARE\Classes\CLSID\%GUID%" /ve /d "SolidWorks.ParametricAddin.ComHost.SwAddin" /f
if errorlevel 1 goto :error

echo [2/8] InprocServer32 default...
reg add "HKLM\SOFTWARE\Classes\CLSID\%GUID%\InprocServer32" /ve /d "%COMHOST%" /f
if errorlevel 1 goto :error

echo [3/8] InprocServer32 ThreadingModel...
reg add "HKLM\SOFTWARE\Classes\CLSID\%GUID%\InprocServer32" /v ThreadingModel /d "Both" /f
if errorlevel 1 goto :error

echo [4/8] ProgId...
reg add "HKLM\SOFTWARE\Classes\CLSID\%GUID%\ProgId" /ve /d "SolidWorks.ParametricAddin.ComHost.SwAddin" /f
if errorlevel 1 goto :error

echo [5/8] ProgId CLSID mapping...
reg add "HKLM\SOFTWARE\Classes\SolidWorks.ParametricAddin.ComHost.SwAddin\CLSID" /ve /d "%GUID%" /f
if errorlevel 1 goto :error

echo [6/8] SW AddIn enabled...
reg add "HKLM\SOFTWARE\SolidWorks\AddIns\%GUID%" /ve /d "1" /f
if errorlevel 1 goto :error

echo [7/8] SW AddIn Description...
reg add "HKLM\SOFTWARE\SolidWorks\AddIns\%GUID%" /v Description /d "SolidWorks Parametric Design Add-in" /f
if errorlevel 1 goto :error

echo [8/8] SW AddIn Title...
reg add "HKLM\SOFTWARE\SolidWorks\AddIns\%GUID%" /v Title /d "ParametricDesignTool" /f
if errorlevel 1 goto :error

echo.
echo === Registration Complete ===
echo Restart SolidWorks, then check: Tools ^> Add-Ins
pause
exit /b 0

:error
echo.
echo === ERROR: Access Denied ===
echo You must run this script as Administrator:
echo   Right-click register_addin.bat ^> Run as Administrator
pause
exit /b 1
