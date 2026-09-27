$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$projectPath = Join-Path $root 'src/VoiceTranslator.App/VoiceTranslator.App.csproj'
$scriptPath = Join-Path $root 'installer/VoxMate.iss'
$publishDirectory = Join-Path $root 'artifacts/publish/installer-input'
$outputDirectory = Join-Path $root 'artifacts/installer'

[xml]$project = Get-Content -LiteralPath $projectPath -Raw
$version = [string]$project.Project.PropertyGroup.Version
if ([string]::IsNullOrWhiteSpace($version)) { throw 'The application project has no Version property.' }

$candidates = @(
    (Join-Path $root '.local/tools/InnoSetup7/ISCC.exe'),
    'C:\Program Files\Inno Setup 7\ISCC.exe',
    'C:\Program Files (x86)\Inno Setup 7\ISCC.exe'
)
$compiler = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $compiler) {
    $command = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($command) { $compiler = $command.Source }
}
if (-not $compiler) {
    throw 'Inno Setup 7 compiler ISCC.exe was not found. Install Inno Setup 7 from https://jrsoftware.org/isdl.php or place its portable files in .local/tools/InnoSetup7.'
}

& (Join-Path $root 'build.ps1')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& (Join-Path $root 'publish.ps1') -BundleModels -OutputDirectory $publishDirectory
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

foreach ($relative in @(
    'VoiceTranslator.App.exe',
    'Models/ggml-base.bin',
    'Models/qwen2.5-0.5b-instruct-q4_k_m.gguf',
    '.local/whisper/whisper-cli.exe',
    '.local/llama/llama-cli.exe',
    'LICENSE',
    'docs/privacy.md',
    'docs/code-signing-policy.md',
    'licenses/README.md'
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $publishDirectory $relative))) {
        throw "Installer input is missing: $relative"
    }
}

New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
& $compiler '--quiet-progress' "--define=AppVersion=$version" "--define=PublishDir=$publishDirectory" "--output-dir=$outputDirectory" $scriptPath
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$installer = Join-Path $outputDirectory "VoxMate-Setup-$version-win-x64.exe"
if (-not (Test-Path -LiteralPath $installer)) { throw "Compilation finished but the installer was not found: $installer" }
$hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $installer).Hash.ToLowerInvariant()
$checksumFile = Join-Path $outputDirectory "VoxMate-Setup-$version-win-x64.sha256"
Set-Content -LiteralPath $checksumFile -Value "$hash  $(Split-Path -Path $installer -Leaf)" -Encoding ascii
$releaseNotes = Join-Path $root "docs/release-notes-$version.md"
if (Test-Path -LiteralPath $releaseNotes) {
    Copy-Item -LiteralPath $releaseNotes -Destination (Join-Path $outputDirectory 'RELEASE_NOTES.md') -Force
}
Write-Host "Installer: $installer"
Write-Host "SHA-256：$hash"
