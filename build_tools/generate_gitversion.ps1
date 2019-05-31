param(

    [Parameter(Mandatory=$false)]
    [Int]
    $BuildCounter
)

$mono = if( Get-Command mono) { $(Get-Command mono).Path } else {"" }

$nugetExe = "& $mono $PSScriptRoot/.nuget/nuget.exe"
$nugetGitversionInstallArgs = 'Install', 'GitVersion.CommandLine', '-version', '4.0.0', '-OutputDirectory', "$PSScriptRoot/packages"

Invoke-Expression "$nugetExe $nugetGitversionInstallArgs"

#set up libgit to work correcly with centos
echo @'
<configuration>
    <dllmap os="linux" cpu="x86-64" wordsize="64" dll="git2-15e1193" target="/usr/lib64/libgit2.so.24" />
    <dllmap os="osx" cpu="x86,x86-64" dll="git2-15e1193" target="lib/osx/libgit2-15e1193.dylib" />
</configuration>
'@ > $PSScriptRoot/packages/GitVersion.CommandLine.4.0.0/tools/LibGit2Sharp.dll.config


#test commit
$gitVersionExe = "& $mono $PSScriptRoot/packages/GitVersion.CommandLine.4.0.0/tools/GitVersion.exe -nofetch"
$json = Invoke-Expression $gitVersionExe

$version = $json | out-string | ConvertFrom-Json
if($BuildCounter -ne $null)
{
  $semVer = $version.FullSemVer -replace '\+\d+$', "+$BuildCounter"
}
else
{
  $semVer = $version.FullSemVer
}

Write-Host "Artifact Version: '$semVer'"
$semVer



