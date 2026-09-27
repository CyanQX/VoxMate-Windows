$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$source = Join-Path $root 'localization/zh-CN'
$destination = Join-Path $root 'artifacts/language-packs'
$archive = Join-Path $destination 'VoxMate-zh-CN-language-pack.zip'

foreach ($relative in @('Locales/zh-CN.json', 'Locales/active-locale.txt')) {
    if (-not (Test-Path -LiteralPath (Join-Path $source $relative))) {
        throw "Language pack is missing: $relative"
    }
}
$marker = (Get-Content -LiteralPath (Join-Path $source 'Locales/active-locale.txt') -Raw).Trim()
if ($marker -ne 'zh-CN') { throw 'The locale marker must contain zh-CN.' }
$strings = Get-Content -LiteralPath (Join-Path $source 'Locales/zh-CN.json') -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
if ($strings.Count -lt 100 -or $strings.ContainsKey('') -or @($strings.Values | Where-Object { [string]::IsNullOrWhiteSpace($_) }).Count -gt 0) {
    throw 'The Chinese language pack is incomplete or contains empty translations.'
}
New-Item -ItemType Directory -Force -Path $destination | Out-Null
if (Test-Path -LiteralPath $archive) { Remove-Item -LiteralPath $archive -Force }
Compress-Archive -Path (Join-Path $source 'Locales') -DestinationPath $archive -CompressionLevel Optimal
Write-Host "Chinese language pack: $archive"
