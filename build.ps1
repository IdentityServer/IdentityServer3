Param(
	[string]$buildNumber = "0",
	[string]$preRelease = $null
)

if(Test-Path Env:\APPVEYOR_BUILD_NUMBER) {
	$d = Get-Date
	$preRelease = "pre-" + $d.Year.ToString() + $d.Month.ToString() + $d.Day.ToString() + "-" + [int]$Env:APPVEYOR_BUILD_NUMBER
	Write-Host "Using APPVEYOR_BUILD_NUMBER"
}

gci .\source -Recurse "packages.config" |% {
	"Restoring " + $_.FullName
	.\source\.nuget\nuget.exe i $_.FullName -o .\source\packages
}
Import-Module .\source\packages\psake.4.3.2\tools\psake.psm1
Invoke-Psake .\default.ps1 $task -framework "4.0x64" -properties @{ buildNumber=$buildNumber; preRelease=$preRelease }
Remove-Module psake