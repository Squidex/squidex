Param(
	[switch]$infrastructure,
	[switch]$appsCore,
	[switch]$appsEntities,
	[switch]$users,
	[switch]$web,
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
	&"$folderHome\.nuget\packages\OpenCover\4.7.922\tools\OpenCover.Console.exe" `
	-register:user `
	-target:"C:\Program Files\dotnet\dotnet.exe" `
	-targetargs:"test --filter Category!=Dependencies $folderWorking\Squidex.Infrastructure.Tests\Squidex.Infrastructure.Tests.csproj" `
	-filter:"+[Squidex.*]* -[Squidex.Infrastructure*]*CodeGen* -[Squidex.Infrastructure*]*OrleansGeneratedCode*" `
	-skipautoprops `
	-output:"$folderWorking\$folderReports\Infrastructure.xml" `
	-oldStyle
}

if ($all -Or $appsCore) {
	&"$folderHome\.nuget\packages\OpenCover\4.7.922\tools\OpenCover.Console.exe" `
	-register:user `
	-target:"C:\Program Files\dotnet\dotnet.exe" `
	-targetargs:"test $folderWorking\Squidex.Domain.Apps.Core.Tests\Squidex.Domain.Apps.Core.Tests.csproj" `
	-filter:"+[Squidex.*]*" `
	-skipautoprops `
	-output:"$folderWorking\$folderReports\Core.xml" `
	-oldStyle
}

if ($all -Or $appsEntities) {
	&"$folderHome\.nuget\packages\OpenCover\4.7.922\tools\OpenCover.Console.exe" `
	-register:user `
	-target:"C:\Program Files\dotnet\dotnet.exe" `
	-targetargs:"test $folderWorking\Squidex.Domain.Apps.Entities.Tests\Squidex.Domain.Apps.Entities.Tests.csproj" `
	-filter:"+[Squidex.*]* -[Squidex.Domain.Apps.Entities*]*CodeGen* -[Squidex.Domain.Apps.Entities*]*OrleansGeneratedCode*" `
	-skipautoprops `
	-output:"$folderWorking\$folderReports\Entities.xml" `
	-oldStyle
}

if ($all -Or $users) {
	&"$folderHome\.nuget\packages\OpenCover\4.7.922\tools\OpenCover.Console.exe" `
	-register:user `
	-target:"C:\Program Files\dotnet\dotnet.exe" `
	-targetargs:"test $folderWorking\Squidex.Domain.Users.Tests\Squidex.Domain.Users.Tests.csproj" `
	-filter:"+[Squidex.*]*" `
	-skipautoprops `
	-output:"$folderWorking\$folderReports\Users.xml" `
	-oldStyle
}

if ($all -Or $web) {
	&"$folderHome\.nuget\packages\OpenCover\4.7.922\tools\OpenCover.Console.exe" `
	-register:user `
	-target:"C:\Program Files\dotnet\dotnet.exe" `
	-targetargs:"test $folderWorking\Squidex.Web.Tests\Squidex.Web.Tests.csproj" `
	-filter:"+[Squidex]Squidex.Web*" `
	-skipautoprops `
	-output:"$folderWorking\$folderReports\Web.xml" `
	-oldStyle
}

&"$folderHome\.nuget\packages\ReportGenerator\4.1.6\tools\net47\ReportGenerator.exe" `
-reports:"$folderWorking\$folderReports\*.xml" `
-targetdir:"$folderWorking\$folderReports\Output"