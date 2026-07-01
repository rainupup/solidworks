param([string]$GenFile)

$content = Get-Content $GenFile -Raw
$content = $content -replace 'public sealed class GeneratedInternalTypeHelper', '[System.Runtime.InteropServices.ComVisible(false)] public sealed class GeneratedInternalTypeHelper'
Set-Content $GenFile -Value $content -NoNewline
Write-Host "Patched GeneratedInternalTypeHelper for COM hosting compatibility"
