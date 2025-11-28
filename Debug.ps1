$ErrorActionPreference = 'Stop'

$ProfileContainerDir = Join-Path $PSScriptRoot "profiles"
$DebugBuildModFile = Join-Path $PSScriptRoot "CollectCruiserItemCompany\bin\Debug\netstandard2.1\com.aoirint.CollectCruiserItemCompany.dll"

$GameDir = "C:\Program Files (x86)\Steam\steamapps\common\Lethal Company"
$GameExeName = "Lethal Company.exe"

$GameExePath = Join-Path $GameDir $GameExeName

Add-Type @"
using System;
using System.Runtime.InteropServices;

public static class Win32 {
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool SetWindowText(IntPtr hWnd, string lpString);
}
"@

function Set-WindowTitleSafely {
    param(
        [System.Diagnostics.Process]$Proc,
        [string]$Title
    )

    for ($i = 0; $i -lt 30; $i++) {
        $Proc.Refresh()

        if ($Proc.MainWindowHandle -ne 0) {
            [Win32]::SetWindowText($Proc.MainWindowHandle, $Title) | Out-Null
            return $true
        }

        Start-Sleep -Milliseconds 200
    }

    Write-Warning "Failed to obtain window handle for PID $($Proc.Id)"
    return $false
}

# Debug build
dotnet build --configuration Debug

if (-not (Test-Path $DebugBuildModFile)) {
  Write-Error "Debug build mod file not found: $DebugBuildModFile"
  exit 1
}

# Start game processes
$GameProcesses = @()
for ($i = 1; $i -le 2; $i++) {
  $ProfileDir = Join-Path $ProfileContainerDir ("profile_" + $i)

  $BepInExPluginDir = Join-Path $ProfileDir "BepInEx\plugins"

  # Install debug build mod
  Copy-Item -Path $DebugBuildModFile -Destination $BepInExPluginDir -Force

  if ($i -eq 1) {
    Copy-Item -Path (Join-Path $ProfileDir "winhttp.dll") -Destination $GameDir -Force
    Copy-Item -Path (Join-Path $ProfileDir "doorstop_config.ini") -Destination $GameDir -Force
  }

  $BepInExPreloaderDllFile = Join-Path $ProfileDir "BepInEx\core\BepInEx.Preloader.dll"
  if (-not (Test-Path $BepInExPreloaderDllFile)) {
    Write-Error "BepInEx Preloader DLL not found in profile directory: $BepInExPreloaderDllFile"
    exit 1
  }

  $Args = @(
    '-screen-width'
    '1280'
    '-screen-height'
    '720'
    # Doorstop v3 for BepInEx 5.4.21
    '--doorstop-enable'
    'true'
    '--doorstop-target'
    $BepInExPreloaderDllFile
  )

  $DebuggerPort = 55555 + ($i - 1)
  $EnvDict = @{
    "MONO_ENV_OPTIONS" = "--debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:${DebuggerPort},embedding=1,defer=y"
  }

  try {
    $GameProcess = Start-Process `
      -FilePath $GameExePath `
      -ArgumentList $Args `
      -WorkingDirectory $GameDir `
      -Environment $EnvDict `
      -PassThru
  } catch {
    Write-Error "Failed to start game process for profile $_"
    exit 1
  }

  $GameProcesses += $GameProcess

  Set-WindowTitleSafely -Proc $GameProcess -Title ("Lethal Company - Profile $i") | Out-Null

  # Wait to prevent conflicting initial file access
  Sleep -Seconds 1
}

# Wait until all game processes exit
try {
  while ($true) {
    $Alive = $GameProcesses | Where-Object { $_ -and -not $_.HasExited }

    if (-not $Alive) {
      break
    }

    Start-Sleep -Milliseconds 100
  }
}
finally {
  # Stop all game processes on exit
  foreach ($GameProcess in $GameProcesses) {
    if ($null -ne $GameProcess -and -not $GameProcess.HasExited) {
      try {
        $null = $GameProcess.CloseMainWindow()
        if (-not $GameProcess.WaitForExit(2000)) {
          $GameProcess.Kill()
          $null = $GameProcess.WaitForExit(3000)
        }
      } catch {
        # ignore errors
      }
    }
  }
}
