param()

$ErrorActionPreference = 'Stop'
$releaseUrl = 'https://github.com/ggml-org/llama.cpp/releases/download/b11195/llama-b11195-bin-win-cpu-x64.zip'
$releaseSha256 = '9416e8bdd133a08a749e71a98435d9b59712e39e914b037ed82f79e2fa8a5093'
$modelUrl = 'https://hf-mirror.com/Qwen/Qwen2.5-0.5B-Instruct-GGUF/resolve/main/qwen2.5-0.5b-instruct-q4_k_m.gguf'
$modelSha256 = '74a4da8c9fdbcd15bd1f6d01d621410d31c6fc00986f5eb687824e7b93d7a9db'
$toolDirectory = Join-Path $PSScriptRoot '.local/llama'
$modelDirectory = Join-Path $env:LOCALAPPDATA 'VoiceTranslator/Models'
$settingsDirectory = Join-Path $env:LOCALAPPDATA 'VoiceTranslator'
$settingsPath = Join-Path $settingsDirectory 'settings.json'
$downloadDirectory = Join-Path $env:TEMP ('VoiceTranslator-translation-' + [guid]::NewGuid().ToString('N'))

function Assert-Hash([string]$path, [string]$expected) {
    $actual = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actual -ne $expected) { throw "SHA-256 校验失败：$path (实际 $actual)" }
}

New-Item -ItemType Directory -Force -Path $toolDirectory, $modelDirectory, $downloadDirectory | Out-Null
try {
    $exePath = Join-Path $toolDirectory 'llama-cli.exe'
    if (-not (Test-Path -LiteralPath $exePath)) {
        $zipPath = Join-Path $downloadDirectory 'llama-win-cpu-x64.zip'
        Write-Host '下载 llama.cpp Windows CPU 运行时...'
        Invoke-WebRequest -Uri $releaseUrl -OutFile $zipPath
        Assert-Hash $zipPath $releaseSha256
        $expanded = Join-Path $downloadDirectory 'expanded'
        Expand-Archive -LiteralPath $zipPath -DestinationPath $expanded
        $sourceExe = Get-ChildItem -LiteralPath $expanded -Recurse -File -Filter 'llama-cli.exe' | Select-Object -First 1
        if ($null -eq $sourceExe) { throw '下载包中找不到 llama-cli.exe。' }
        Get-ChildItem -LiteralPath $sourceExe.DirectoryName -Force | Copy-Item -Destination $toolDirectory -Recurse -Force
    }

    $modelPath = Join-Path $modelDirectory 'qwen2.5-0.5b-instruct-q4_k_m.gguf'
    if (-not (Test-Path -LiteralPath $modelPath)) {
        $temporaryModel = Join-Path $downloadDirectory 'qwen.gguf'
        Write-Host '下载 Qwen2.5 0.5B Q4_K_M 模型（约 491 MB）...'
        & curl.exe -fL --retry 3 --connect-timeout 20 --max-time 900 --output $temporaryModel $modelUrl
        if ($LASTEXITCODE -ne 0) { throw "模型下载失败（curl 退出码 $LASTEXITCODE）。" }
        Assert-Hash $temporaryModel $modelSha256
        Move-Item -LiteralPath $temporaryModel -Destination $modelPath
    }
    Assert-Hash $modelPath $modelSha256

    New-Item -ItemType Directory -Force -Path $settingsDirectory | Out-Null
    if (Test-Path -LiteralPath $settingsPath) {
        $settings = Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8 | ConvertFrom-Json
    } else {
        $settings = [pscustomobject]@{}
    }
    $settings | Add-Member -NotePropertyName LlamaExecutablePath -NotePropertyValue $exePath -Force
    $settings | Add-Member -NotePropertyName LlamaModelPath -NotePropertyValue $modelPath -Force
    $settings | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $settingsPath -Encoding UTF8
    Write-Host "llama.cpp: $exePath"
    Write-Host "本地翻译模型: $modelPath"
    Write-Host '配置完成。'
}
finally {
    $resolvedDownload = [IO.Path]::GetFullPath($downloadDirectory)
    $resolvedTemp = [IO.Path]::GetFullPath($env:TEMP).TrimEnd('\') + '\'
    if ($resolvedDownload.StartsWith($resolvedTemp, [StringComparison]::OrdinalIgnoreCase) -and
        [IO.Path]::GetFileName($resolvedDownload) -like 'VoiceTranslator-translation-*' -and
        (Test-Path -LiteralPath $resolvedDownload)) {
        Remove-Item -LiteralPath $resolvedDownload -Recurse -Force
    }
}
