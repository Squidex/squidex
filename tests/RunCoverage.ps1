Param(
	[switch]$infrastructure,
	[switch]$appsCore,
	[switch]$appsRead,
	[switch]$appsWrite,
	[switch]$users,
	[switch]$all
)

$ErrorActionPreference = "Stop"

$folderReports = ".\_test-output"
$folderHome = $env:USERPROFILE
$folderWorking = Get-Location

if (Test-Path $folderReports) {
    Remove-Item $folderReports -recurse
}

Write-Host "Recreated '$folderReports' folder"

New-Item -ItemType directory -Path $folderReports

if ($all -Or $infrastructure) {
	&"$folderHome\.nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console.exe" `
	-register:user `
	-target:"C:\Program Files\dotnet\dotnet.exe" `
	-targetargs:"test $folderWorking\Squidex.Infrastructure.Tests\Squidex.Infrastructure.Tests.csproj" `
	-filter:"+[Squidex*]*" `
	-skipautoprops `
	-output:"$folderWorking\$folderReports\Infrastructure.xml" `
	-oldStyle
}

if ($all -Or $appsCore) {
	&"$folderHome\.nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console.exe" `
	-register:user `
	-target:"C:\Program Files\dotnet\dotnet.exe" `
	-targetargs:"test $folderWorking\Squidex.Domain.Apps.Core.Tests\Squidex.Domain.Apps.Core.Tests.csproj" `
	-filter:"+[Squidex*]*" `
	-skipautoprops `
	-output:"$folderWorking\$folderReports\Core.xml" `
	-oldStyle
}

if ($all -Or $appsRead) {
	&"$folderHome\.nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console.exe" `
	-register:user `
	-target:"C:\Program Files\dotnet\dotnet.exe" `
	-targetargs:"test $folderWorking\Squidex.Domain.Apps.Read.Tests\Squidex.Domain.Apps.Read.Tests.csproj" `
	-filter:"+[Squidex*]*" `
	-skipautoprops `
	-output:"$folderWorking\$folderReports\Read.xml" `
	-oldStyle
}

if ($all -Or $appsWrite) {
	&"$folderHome\.nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console.exe" `
	-register:user `
	-target:"C:\Program Files\dotnet\dotnet.exe" `
	-targetargs:"test $folderWorking\Squidex.Domain.Apps.Write.Tests\Squidex.Domain.Apps.Write.Tests.csproj" `
	-filter:"+[Squidex*]*" `
	-skipautoprops `
	-output:"$folderWorking\$folderReports\Write.xml" `
	-oldStyle
}

if ($all -Or $users) {
	&"$folderHome\.nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console.exe" `
	-register:user `
	-target:"C:\Program Files\dotnet\dotnet.exe" `
	-targetargs:"test $folderWorking\Squidex.Domain.Users.Tests\Squidex.Domain.Users.Tests.csproj" `
	-filter:"+[Squidex*]*" `
	-skipautoprops `
	-output:"$folderWorking\$folderReports\Users.xml" `
	-oldStyle
}

&"$folderHome\.nuget\packages\ReportGenerator\3.0.0\tools\ReportGenerator.exe" `
-reports:"$folderWorking\$folderReports\*.xml" `
-targetdir:"$folderWorking\$folderReports\Output"