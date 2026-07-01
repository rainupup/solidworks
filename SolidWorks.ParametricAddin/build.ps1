<#
.SYNOPSIS
    Build the SolidWorks Parametric Add-in.
.DESCRIPTION
    Builds interop stubs (or uses real SW DLLs), then builds the main project.
.PARAMETER RealSw
    Use real SolidWorks interop DLLs. Set $env:SW_INTEROP_PATH or defaults to
    "C:\Program Files\SOLIDWORKS Corp\SOLIDWORKS\api\redist"
.PARAMETER Clean
    Clean all build outputs.
.EXAMPLE
    .\build.ps1               # Dev build with stub DLLs
    .\build.ps1 -RealSw        # Production build with real SW DLLs
    .\build.ps1 -Clean         # Clean
#>
param([switch]$RealSw, [switch]$Clean)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$libDir = "$root\lib"

# ---- Find dotnet with SDK ----
function Find-Dotnet {
    # Check if default dotnet has SDK
    $defaultDotnet = Get-Command dotnet -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
    if ($defaultDotnet) {
        $sdkList = & $defaultDotnet --list-sdks 2>&1
        if ($sdkList -match '\d+\.\d+\.\d+') {
            return $defaultDotnet
        }
    }
    # Try x86 path (common on Windows with separate x86/x64 installs)
    $x86 = "${env:ProgramFiles(x86)}\dotnet\dotnet.exe"
    if (Test-Path $x86) {
        $sdkList = & $x86 --list-sdks 2>&1
        if ($sdkList -match '\d+\.\d+\.\d+') {
            return $x86
        }
    }
    # Try x64 Program Files
    $x64 = "$env:ProgramFiles\dotnet\dotnet.exe"
    if (Test-Path $x64) {
        return $x64
    }
    throw "dotnet SDK not found. Install from: https://dotnet.microsoft.com/download/dotnet/8.0"
}

$dotnet = Find-Dotnet
Write-Host "Using dotnet: $dotnet" -ForegroundColor Gray

# ---- Clean ----
if ($Clean) {
    Write-Host "Cleaning..." -ForegroundColor Yellow
    @("$root\bin", "$root\obj", "$root\lib\*.dll") | ForEach-Object {
        Remove-Item $_ -Recurse -Force -ErrorAction SilentlyContinue
    }
    Get-ChildItem "$root\stubs" -Directory | ForEach-Object {
        Remove-Item "$_\bin" -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item "$_\obj" -Recurse -Force -ErrorAction SilentlyContinue
    }
    Write-Host "Clean complete." -ForegroundColor Green
    return
}

# ---- Prepare interop DLLs ----
New-Item -ItemType Directory -Force -Path $libDir | Out-Null

if ($RealSw) {
    $swPath = $env:SW_INTEROP_PATH
    if (-not $swPath) {
        $swPath = "${env:ProgramFiles}\SOLIDWORKS Corp\SOLIDWORKS\api\redist"
    }
    if (-not (Test-Path "$swPath\SolidWorks.Interop.sldworks.dll")) {
        Write-Host "ERROR: SW interop DLLs not found at: $swPath" -ForegroundColor Red
        Write-Host "`nSet the path with: `$env:SW_INTEROP_PATH = 'C:\path\to\sw\api\redist'" -ForegroundColor Yellow
        Write-Host "Then run: .\build.ps1 -RealSw" -ForegroundColor Yellow
        return
    }
    Write-Host "Copying real SW interop DLLs from: $swPath" -ForegroundColor Cyan
    Copy-Item "$swPath\SolidWorks.Interop.sldworks.dll" $libDir -Force
    Copy-Item "$swPath\SolidWorks.Interop.swconst.dll" $libDir -Force
    Copy-Item "$swPath\SolidWorks.Interop.swpublished.dll" $libDir -Force
    Write-Host "Using REAL SolidWorks interop DLLs." -ForegroundColor Green
}
else {
    Write-Host "Building stub interop DLLs..." -ForegroundColor Cyan
    $stubs = @(
        "stubs\SolidWorks.Interop.sldworks",
        "stubs\SolidWorks.Interop.swconst",
        "stubs\SolidWorks.Interop.swpublished"
    )
    foreach ($proj in $stubs) {
        Write-Host "  $proj" -ForegroundColor Gray
        & $dotnet build "$root\$proj" -c Debug --nologo -v q 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  FAILED: $proj" -ForegroundColor Red
            & $dotnet build "$root\$proj" -c Debug
            return
        }
        $dll = Get-ChildItem "$root\$proj\bin\Debug\net8.0\*.dll" | Select-Object -First 1
        if ($dll) { Copy-Item $dll.FullName $libDir -Force }
    }
    Write-Host "Stub DLLs ready." -ForegroundColor Green
}

# ---- Build main project ----
Write-Host "`nBuilding SolidWorks.ParametricAddin..." -ForegroundColor Cyan
& $dotnet build "$root" -c Debug --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nMAIN BUILD FAILED" -ForegroundColor Red
    return
}

Write-Host "Main project built." -ForegroundColor Green

# ---- Build COM host project ----
$comHostDir = "$PSScriptRoot\..\SolidWorks.ParametricAddin.ComHost"
if (Test-Path $comHostDir) {
    Write-Host "`nBuilding COM host..." -ForegroundColor Cyan
    & $dotnet build "$comHostDir" -c Debug --nologo

    if ($LASTEXITCODE -eq 0) {
        $comOutput = "$comHostDir\bin\Debug\net8.0-windows\SolidWorks.ParametricAddin.ComHost.dll"
        $comHostDll = "$comHostDir\bin\Debug\net8.0-windows\SolidWorks.ParametricAddin.ComHost.comhost.dll"
        Write-Host "`nBUILD SUCCESS" -ForegroundColor Green
        Write-Host "Add-in DLL: $comOutput" -ForegroundColor Gray
        Write-Host "COM Host:   $comHostDll" -ForegroundColor Gray
        Write-Host "`nTo register: regsvr32 `"$comHostDll`"" -ForegroundColor Yellow
    }
    else {
        Write-Host "`nCOM HOST BUILD FAILED" -ForegroundColor Red
    }
}
else {
    $output = "$root\bin\Debug\net8.0-windows\SolidWorks.ParametricAddin.dll"
    Write-Host "`nBUILD SUCCESS" -ForegroundColor Green
    Write-Host "Output: $output" -ForegroundColor Gray
}
