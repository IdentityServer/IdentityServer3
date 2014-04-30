properties {
	$base_directory = Resolve-Path . 
	$src_directory = "$base_directory\source"
	$output_directory = "$base_directory\build"
	$dist_directory = "$base_directory\dist"
	$sln_file = "$src_directory\Thinktecture.IdentityServer.v3.sln"
	$target_config = "Release"
	$framework_version = "v4.5"
	$xunit_path = "$src_directory\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe"
	$ilrepack_path = "$src_directory\packages\ILRepack.1.25.0\tools\ILRepack.exe"

	$buildNumber = 0;
	$version = "0.0.0.0"
}

task default -depends Clean, Compile, CreateDistribution

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
			# Not including $output_directory\Autofac.dll as it procuces a reference error: 
			#	Mono.Cecil.AssemblyResolutionException: Failed to resolve assembly: 'System.Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
			# Seems to be th eresult of Autofac being a portable assembly
			if ("$_" -ne "Autofac.dll") {
				$input_dlls = "$input_dlls $output_directory\$_"
			}
	}

	"input_dlls = $input_dlls"

	Invoke-Expression "$ilrepack_path /targetplatform:v4 /internalize /target:library /out:$output_directory\Thinktecture.IdentityServer.dll $output_directory\Thinktecture.IdentityServer.Core.dll $input_dlls"
}

task CreateDistribution -depends ILRepack {
	New-Item $dist_directory -Type Directory
	Copy-Item $output_directory\Thinktecture.IdentityServer.dll $dist_directory
	Copy-Item $output_directory\Autofac.dll $dist_directory
}
