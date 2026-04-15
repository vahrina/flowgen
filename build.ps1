param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [switch]$KeepXml
)

$ErrorActionPreference = "Stop"

$pluginRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectDir = Join-Path $pluginRoot "src/Flow.Launcher.Plugin.PassGen"
$projectFile = Join-Path $projectDir "Flow.Launcher.Plugin.PassGen.csproj"
$buildOut = Join-Path $projectDir "bin/$Configuration"

$runtimeFiles = @(
    "flowgen.dll",
    "flowgen.deps.json",
    "Flow.Launcher.Plugin.dll",
    "JetBrains.Annotations.dll",
    "PropertyChanged.dll",
    "plugin.json",
    "icon.png"
)

$legacyFiles = @(
    "Flow.Launcher.Plugin.PassGen.dll",
    "Flow.Launcher.Plugin.PassGen.deps.json",
    "Flow.Launcher.Plugin.PassGen.xml",
    "flowgen.xml"
)

Write-Host "==> build ($Configuration)"
dotnet build "$projectFile" -c $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed"
}

Write-Host "==> clean old artifacts"
foreach ($name in $legacyFiles) {
    $target = Join-Path $pluginRoot $name
    if (Test-Path $target) {
        Remove-Item $target -Force -ErrorAction SilentlyContinue
    }
}

if (-not $KeepXml) {
    $xml = Join-Path $pluginRoot "flowgen.xml"
    if (Test-Path $xml) {
        Remove-Item $xml -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "==> deploy runtime files"
foreach ($name in $runtimeFiles) {
    $src = Join-Path $buildOut $name
    $dst = Join-Path $pluginRoot $name

    if (-not (Test-Path $src)) {
        throw "missing build output: $name"
    }

    try {
        Copy-Item $src $dst -Force
    }
    catch {
        Write-Warning "failed to copy $name (likely locked): $($_.Exception.Message)"
    }
}

Write-Host "==> done"
Write-Host "restart flow launcher if flowgen.dll was locked during copy"
