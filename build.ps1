param(
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe",
    [string]$BuildTarget = "StandaloneWindows64",
    [string]$BuildPath = ""
)

$ErrorActionPreference = "Stop"
$ProjectPath = $PSScriptRoot
$UnityArgs = @(
    "-batchmode",
    "-quit",
    "-projectPath",
    $ProjectPath,
    "-executeMethod",
    "BuildScript.Build",
    "-biTileBuildTarget",
    $BuildTarget
)

if ($BuildPath -ne "") {
    $UnityArgs += @("-buildPath", $BuildPath)
}

& $UnityPath $UnityArgs

exit $LASTEXITCODE
