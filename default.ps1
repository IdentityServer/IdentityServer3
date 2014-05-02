properties {
	$base_directory = Resolve-Path . 
	$src_directory = "$base_directory\source"
	$output_directory = "$base_directory\build"
	$dist_directory = "$base_directory\distribution"
	$sln_file = "$src_directory\Thinktecture.IdentityServer.v3.sln"
	$target_config = "Release"
	$framework_version = "v4.5"
	$xunit_path = "$src_directory\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe"
	$ilmerge_path = "$src_directory\packages\ILMerge.2.13.0307\ILMerge.exe"

	$buildNumber = 0;
	$version = "0.0.0.0"
}

task default -depends Clean, Compile, ILRepack

task Clean {
	rmdir $output_directory -ea SilentlyContinue -recurse
	rmdir $dist_directory -ea SilentlyContinue -recurse
	exec { msbuild /nologo /verbosity:quiet $sln_file /p:Configuration=$target_config /t:Clean }
}

task Compile -depends UpdateVersion {
	exec { msbuild /nologo /verbosity:q $sln_file /p:Configuration=$target_config /p:TargetFrameworkVersion=v4.5 }
}

task UpdateVersion {
	$vSplit = $version.Split('.')
	if($vSplit.Length -ne 4)
	{
		throw "Version number is invalid. Must be in the form of 0.0.0.0"
	}
	$major = $vSplit[0]
	$minor = $vSplit[1]
	$patch = $vSplit[2]
	$assemblyFileVersion =  "$major.$minor.$patch.$buildNumber"
	$assemblyVersion = "$major.$minor.0.0"
	$versionAssemblyInfoFile = "$src_directory/VersionAssemblyInfo.cs"
	"using System.Reflection;" > $versionAssemblyInfoFile
	"" >> $versionAssemblyInfoFile
	"[assembly: AssemblyVersion(""$assemblyVersion"")]" >> $versionAssemblyInfoFile
	"[assembly: AssemblyFileVersion(""$assemblyFileVersion"")]" >> $versionAssemblyInfoFile
}

task ILRepack -depends Compile {
	$input_dlls = ""

	Get-ChildItem -Path $output_directory -Filter *.dll |
		foreach-object {
			# Exclude Thinktecture.IdentityServer.Core.dll as that will be the primary assembly
			if ("$_" -ne "Thinktecture.IdentityServer.Core.dll") {
				$input_dlls = "$input_dlls $output_directory\$_"
			}
	}

	New-Item $dist_directory -Type Directory
	Invoke-Expression "$ilmerge_path /targetplatform:v4 /internalize:ilmerge.exclude /allowDup /target:library /out:$dist_directory\Thinktecture.IdentityServer.dll $output_directory\Thinktecture.IdentityServer.Core.dll  $input_dlls"
}
