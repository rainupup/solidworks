$guid = "{B8E7F3D1-A2C4-4E5F-9A1B-3C6D8E0F4A2C}"
$comHost = "D:\code\solidworks\SolidWorks.ParametricAddin.ComHost\bin\Debug\net8.0-windows\SolidWorks.ParametricAddin.ComHost.comhost.dll"
$title = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String("5Y+C5pWw5YyW6K6+6K6h5bel5YW3"))

# COM CLSID
New-Item -Path "HKLM:\SOFTWARE\Classes\CLSID\$guid" -Force | Out-Null
Set-ItemProperty -Path "HKLM:\SOFTWARE\Classes\CLSID\$guid" -Name "(default)" -Value "SolidWorks.ParametricAddin.ComHost.SwAddin" -Force

New-Item -Path "HKLM:\SOFTWARE\Classes\CLSID\$guid\InprocServer32" -Force | Out-Null
Set-ItemProperty -Path "HKLM:\SOFTWARE\Classes\CLSID\$guid\InprocServer32" -Name "(default)" -Value $comHost -Force
Set-ItemProperty -Path "HKLM:\SOFTWARE\Classes\CLSID\$guid\InprocServer32" -Name "ThreadingModel" -Value "Both" -Force

New-Item -Path "HKLM:\SOFTWARE\Classes\CLSID\$guid\ProgId" -Force | Out-Null
Set-ItemProperty -Path "HKLM:\SOFTWARE\Classes\CLSID\$guid\ProgId" -Name "(default)" -Value "SolidWorks.ParametricAddin.ComHost.SwAddin" -Force

# ProgId
New-Item -Path "HKLM:\SOFTWARE\Classes\SolidWorks.ParametricAddin.ComHost.SwAddin\CLSID" -Force | Out-Null
Set-ItemProperty -Path "HKLM:\SOFTWARE\Classes\SolidWorks.ParametricAddin.ComHost.SwAddin\CLSID" -Name "(default)" -Value $guid -Force

# SolidWorks Add-in
New-Item -Path "HKLM:\SOFTWARE\SolidWorks\AddIns\$guid" -Force | Out-Null
Set-ItemProperty -Path "HKLM:\SOFTWARE\SolidWorks\AddIns\$guid" -Name "(default)" -Value 1 -Force
Set-ItemProperty -Path "HKLM:\SOFTWARE\SolidWorks\AddIns\$guid" -Name "Description" -Value "SolidWorks Parametric Design Add-in" -Force
Set-ItemProperty -Path "HKLM:\SOFTWARE\SolidWorks\AddIns\$guid" -Name "Title" -Value $title -Force

Write-Host "OK - Registered under HKLM"