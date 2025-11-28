# Initialize BeplnEx profiles with LcBetterSaves

$ErrorActionPreference = 'Stop'

$ProfileContainerDir = Join-Path $PSScriptRoot "Profiles"

$BepInExVersion = "5.4.21"
$BepInExAssetName = "BepInEx_x64_5.4.21.0.zip"
$BepInExSha256Expected = "2af69fe0aaf821e95c4cd3e4d53860e667c54648f97dca6f971a5bcd3c22aa34"

$LcBetterSavesVersion = "1.7.3"
$LcBetterSavesSha256Expected = "502c75b79c3a89ccce484893df020adcdb8eade9d3a10ea39f74110efe77b5a6"

# Download BepInEx
$BepInExUrl = "https://github.com/BepInEx/BepInEx/releases/download/v${BepInExVersion}/${BepInExAssetName}"
$TempBepInExZipFile = Join-Path $env:TEMP ("BepInex_zip_" + [guid]::NewGuid().ToString() + ".zip")
try {
  Invoke-WebRequest -Uri $BepInExUrl -OutFile $TempBepInExZipFile
} catch {
  Write-Error "Failed to download BepInEx: $_"
  exit 1
}

$BepInExHash = (Get-FileHash -Path $TempBepInExZipFile -Algorithm SHA256).Hash.ToLower()
if ($BepInExHash -ne $BepInExSha256Expected.ToLower()) {
  Write-Error "BepInEx hash mismatch. Expected: $BepInExSha256Expected, Actual: $BepInExHash"
  Remove-Item -Path $TempBepInExZipFile -Force
  exit 1
}

$TempBepInExDir = Join-Path $env:TEMP  ("BepInex_" + [guid]::NewGuid().ToString())
try {
  Expand-Archive -Path $TempBepInExZipFile -DestinationPath $TempBepInExDir -Force
} catch {
  Write-Error "Failed to extract BepInEx: $_"
  exit 1
}
Remove-Item -Path $TempBepInExZipFile -Force

# Initialize profile directories
if (-not (Test-Path $ProfileContainerDir)) {
  New-Item -ItemType Directory -Path $ProfileContainerDir
}
for ($i = 1; $i -le 2; $i++) {
  $ProfileDir = Join-Path $ProfileContainerDir ("profile_" + $i)

  if (Test-Path $ProfileDir) {
    Remove-Item -Path $ProfileDir -Recurse -Force
  }

  New-Item -ItemType Directory -Path $ProfileDir | Out-Null
  Copy-Item -Path (Join-Path $TempBepInExDir "*") -Destination $ProfileDir -Recurse
}
Remove-Item -Path $TempBepInExDir -Recurse -Force

# Download LcBetterSaves mod
$LcBetterSavesUrl = "https://gcdn.thunderstore.io/live/repository/packages/Pooble-LCBetterSaves-${LcBetterSavesVersion}.zip"
$TempLcBetterSavesZipFile = Join-Path $env:TEMP ("lc_better_saves_zip_" + [guid]::NewGuid().ToString() + ".zip")
try {
  Invoke-WebRequest -Uri $LcBetterSavesUrl -OutFile $TempLcBetterSavesZipFile
}
catch {
  Write-Error "Failed to download LcBetterSaves mod: $_"
  exit 1
}

$LcBetterSavesHash = (Get-FileHash -Path $TempLcBetterSavesZipFile -Algorithm SHA256).Hash.ToLower()
if ($LcBetterSavesHash -ne $LcBetterSavesSha256Expected.ToLower()) {
  Write-Error "LcBetterSaves mod hash mismatch. Expected: $LcBetterSavesSha256Expected, Actual: $LcBetterSavesHash"
  Remove-Item -Path $TempLcBetterSavesZipFile -Force
  exit 1
}

$TempLcBetterSavesDir = Join-Path $env:TEMP  ("lc_better_saves_" + [guid]::NewGuid().ToString())
try {
  Expand-Archive -Path $TempLcBetterSavesZipFile -DestinationPath $TempLcBetterSavesDir -Force
} catch {
  Write-Error "Failed to extract LcBetterSaves mod: $_"
  exit 1
}
Remove-Item -Path $TempLcBetterSavesZipFile -Force

# Install LcBetterSaves mod
for ($i = 1; $i -le 2; $i++) {
  $ProfileDir = Join-Path $ProfileContainerDir ("profile_" + $i)
  $PluginsDir = Join-Path $ProfileDir "BepInEx\plugins"

  if (-not (Test-Path $PluginsDir)) {
    New-Item -ItemType Directory -Path ${PluginsDir} | Out-Null
  }

  Copy-Item -Path (Join-Path $TempLcBetterSavesDir "LCBetterSaves.dll") -Destination $PluginsDir -Force
}
Remove-Item -Path $TempLcBetterSavesDir -Recurse -Force
