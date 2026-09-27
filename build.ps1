param()
$ErrorActionPreference = 'Stop'
dotnet restore (Join-Path $PSScriptRoot 'VoiceTranslator.sln')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet build (Join-Path $PSScriptRoot 'VoiceTranslator.sln') -c Release --no-restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet test (Join-Path $PSScriptRoot 'VoiceTranslator.sln') -c Release --no-restore
exit $LASTEXITCODE
