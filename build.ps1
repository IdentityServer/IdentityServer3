Param([string]$buildNumber = "0")

$teamCityBlockName = "Restore Dependencies"
if($env:TEAMCITY_PROJECT_NAME -ne $null){
	"##teamcity[blockOpened name='$teamCityBlockName']"
}

gci .\source -Recurse "packages.config" |% {
	"Restoring " + $_.FullName
	.\source\.nuget\nuget.exe i $_.FullName -o .\source\packages
}

if($env:TEAMCITY_PROJECT_NAME -ne $null){
	"##teamcity[blockClosed name='$teamCityBlockName']"
}

Import-Module .\source\packages\psake.4.3.2\tools\psake.psm1
Invoke-Psake .\default.ps1 $task -framework "4.0x64" -properties @{ buildNumber=$buildNumber }
Remove-Module psake