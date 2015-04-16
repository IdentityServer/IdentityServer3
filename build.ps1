Param(
	[string]$buildNumber = "0",
	[string]$preRelease = $null
)

gci .\source -Recurse "packages.config" |% {
	"Restoring " + $_.FullName
	.\source\.nuget\nuget.exe i $_.FullName -o .\source\packages
    
    if ($LastExitCode -ne 0) {
        exit $LastExitCode
    }
}

Import-Module .\source\packages\psake.4.4.1\tools\psake.psm1

if(Test-Path Env:\APPVEYOR_BUILD_NUMBER){
	$buildNumber = [int]$Env:APPVEYOR_BUILD_NUMBER
	$task = "appVeyor"
    
    Write-Host "Using APPVEYOR_BUILD_NUMBER"
}

"Build number $buildNumber"

Invoke-Psake .\default.ps1 $task -framework "4.0x64" -properties @{ buildNumber=$buildNumber; preRelease=$preRelease }

Remove-Module psake