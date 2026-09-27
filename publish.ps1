param([switch]$BundleModels, [string]$OutputDirectory)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $PSScriptRoot 'artifacts/publish/win-x64'
} elseif (-not [System.IO.Path]::IsPathRooted($OutputDirectory)) {
    $OutputDirectory = Join-Path $PSScriptRoot $OutputDirectory
}
dotnet publish (Join-Path $PSScriptRoot 'src/VoiceTranslator.App/VoiceTranslator.App.csproj') -c Release -r win-x64 --self-contained true -o $outputDirectory
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'setup-whisper.ps1') -Destination $outputDirectory -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'setup-translation.ps1') -Destination $outputDirectory -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.md') -Destination $outputDirectory -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'LICENSE') -Destination $outputDirectory -Force
$docsTarget = Join-Path $outputDirectory 'docs'
New-Item -ItemType Directory -Force -Path $docsTarget | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'docs/privacy.md') -Destination $docsTarget -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'docs/code-signing-policy.md') -Destination $docsTarget -Force
$licensesTarget = Join-Path $outputDirectory 'licenses'
New-Item -ItemType Directory -Force -Path $licensesTarget | Out-Null
Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'licenses') -File | Copy-Item -Destination $licensesTarget -Force

if ($BundleModels) {
    $modelSource = Join-Path $env:LOCALAPPDATA 'VoiceTranslator/Models'
    $modelTarget = Join-Path $outputDirectory 'Models'
    if (-not (Test-Path -LiteralPath $modelSource)) { throw 'The local model directory is missing. Run setup-whisper.ps1 and setup-translation.ps1 first.' }
    New-Item -ItemType Directory -Force -Path $modelTarget | Out-Null
    foreach ($name in @('ggml-base.bin', 'qwen2.5-0.5b-instruct-q4_k_m.gguf')) {
        $source = Join-Path $modelSource $name
        if (-not (Test-Path -LiteralPath $source)) { throw "Missing model: $source" }
        Copy-Item -LiteralPath $source -Destination $modelTarget -Force
    }
    foreach ($tool in @('whisper', 'llama')) {
        $source = Join-Path $PSScriptRoot ".local/$tool"
        if (-not (Test-Path -LiteralPath $source)) { throw "Missing local runtime: $source" }
        $target = Join-Path $outputDirectory ".local/$tool"
        New-Item -ItemType Directory -Force -Path $target | Out-Null
        Get-ChildItem -LiteralPath $source -Force | Copy-Item -Destination $target -Recurse -Force
    }
}
Write-Host "Publish complete: $outputDirectory"

