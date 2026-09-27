param()

$ErrorActionPreference = 'Stop'
$releaseUrl = 'https://github.com/ggml-org/whisper.cpp/releases/download/v1.9.2/whisper-bin-x64.zip'
$releaseSha256 = '49dcc16de826f20bd53d44f947a1ae49dfa81f86cad67a64d80820cb192d674a'
$modelUrl = 'https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin'
$modelMirrorUrl = 'https://hf-mirror.com/ggerganov/whisper.cpp/resolve/main/ggml-base.bin'
$modelSha256 = '60ed5bc3dd14eea856493d334349b405782ddcaf0028d4b5df4088345fba2efe'
$toolDirectory = Join-Path $PSScriptRoot '.local/whisper'
$modelDirectory = Join-Path $env:LOCALAPPDATA 'VoiceTranslator/Models'
$settingsDirectory = Join-Path $env:LOCALAPPDATA 'VoiceTranslator'
$settingsPath = Join-Path $settingsDirectory 'settings.json'
$downloadDirectory = Join-Path $env:TEMP ('VoiceTranslator-setup-' + [guid]::NewGuid().ToString('N'))

function Assert-Hash([string]$path, [string]$expected) {
    $actual = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actual -ne $expected) {
        throw "SHA-256 verification failed: $path (actual $actual)"
    }
}

New-Item -ItemType Directory -Force -Path $toolDirectory, $modelDirectory, $downloadDirectory | Out-Null
try {
    $exePath = Join-Path $toolDirectory 'whisper-cli.exe'
    if (-not (Test-Path -LiteralPath $exePath)) {
        $zipPath = Join-Path $downloadDirectory 'whisper-bin-x64.zip'
        Write-Host 'Downloading whisper.cpp Windows x64 binaries...'
        Invoke-WebRequest -Uri $releaseUrl -OutFile $zipPath
        Assert-Hash $zipPath $releaseSha256
        $expanded = Join-Path $downloadDirectory 'expanded'
        Expand-Archive -LiteralPath $zipPath -DestinationPath $expanded
        $sourceExe = Get-ChildItem -LiteralPath $expanded -Recurse -File -Filter 'whisper-cli.exe' | Select-Object -First 1
        if ($null -eq $sourceExe) { throw 'whisper-cli.exe was not found in the downloaded archive.' }
        Get-ChildItem -LiteralPath $sourceExe.DirectoryName -Force |
            Copy-Item -Destination $toolDirectory -Recurse -Force
    }

    $modelPath = Join-Path $modelDirectory 'ggml-base.bin'
    if (-not (Test-Path -LiteralPath $modelPath)) {
        $temporaryModel = Join-Path $downloadDirectory 'ggml-base.bin'
        Write-Host 'Downloading the multilingual Whisper base model (about 148 MB)...'
        & curl.exe -fL --retry 2 --connect-timeout 20 --max-time 900 --output $temporaryModel $modelUrl
        if ($LASTEXITCODE -ne 0) {
            Write-Host 'The primary model source is unavailable. Trying the mirror with the same SHA-256 verification...'
            & curl.exe -fL --retry 2 --connect-timeout 20 --max-time 900 --output $temporaryModel $modelMirrorUrl
            if ($LASTEXITCODE -ne 0) { throw "Whisper model download failed (curl exit code $LASTEXITCODE)." }
        }
        Assert-Hash $temporaryModel $modelSha256
        Move-Item -LiteralPath $temporaryModel -Destination $modelPath
    }
    Assert-Hash $modelPath $modelSha256

    New-Item -ItemType Directory -Force -Path $settingsDirectory | Out-Null
    if (Test-Path -LiteralPath $settingsPath) {
        $settings = Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8 | ConvertFrom-Json
    } else {
        $settings = [pscustomobject]@{ AlwaysOnTop = $true; MicrophoneIndex = 0; WhisperExecutablePath = ''; WhisperModelPath = '' }
    }
    $settings.WhisperExecutablePath = $exePath
    $settings.WhisperModelPath = $modelPath
    $settings | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $settingsPath -Encoding UTF8
    Write-Host "whisper.cpp: $exePath"
    Write-Host "Multilingual model: $modelPath"
    Write-Host 'Setup complete.'
}
finally {
    $resolvedDownload = [IO.Path]::GetFullPath($downloadDirectory)
    $resolvedTemp = [IO.Path]::GetFullPath($env:TEMP).TrimEnd('\') + '\'
    if ($resolvedDownload.StartsWith($resolvedTemp, [StringComparison]::OrdinalIgnoreCase) -and
        [IO.Path]::GetFileName($resolvedDownload) -like 'VoiceTranslator-setup-*' -and
        (Test-Path -LiteralPath $resolvedDownload)) {
        Remove-Item -LiteralPath $resolvedDownload -Recurse -Force
    }
}
