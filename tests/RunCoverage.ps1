$ErrorActionPreference = "Stop"

$reportsFolder = ".\_test-output"
$userProfile = $env:USERPROFILE
$workingFolder = Get-Location

Write-Host "Clear up '$reportsFolder' folder"

if (Test-Path $reportsFolder) {
    Remove-Item $reportsFolder -recurse
}

Write-Host "Create new '$reportsFolder' folder"

New-Item -ItemType directory -Path $reportsFolder

&"$userProfile\.nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console.exe" `
-register:user `
-target:"C:\Program Files\dotnet\dotnet.exe" `
-targetargs:"test $workingFolder\Squidex.Infrastructure.Tests\Squidex.Infrastructure.Tests.csproj" `
-filter:"+[Squidex*]*" `
-skipautoprops `
-output:"$workingFolder\$reportsFolder\Infrastructure.xml" `
-oldStyle

&"$userProfile\.nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console.exe" `
-register:user `
-target:"C:\Program Files\dotnet\dotnet.exe" `
-targetargs:"test $workingFolder\Squidex.Domain.Apps.Core.Tests\Squidex.Domain.Apps.Core.Tests.csproj" `
-filter:"+[Squidex*]*" `
-skipautoprops `
-output:"$workingFolder\$reportsFolder\Core.xml" `
-oldStyle

&"$userProfile\.nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console.exe" `
-register:user `
-target:"C:\Program Files\dotnet\dotnet.exe" `
-targetargs:"test $workingFolder\Squidex.Domain.Apps.Read.Tests\Squidex.Domain.Apps.Read.Tests.csproj" `
-filter:"+[Squidex*]*" `
-skipautoprops `
-output:"$workingFolder\$reportsFolder\Read.xml" `
-oldStyle

&"$userProfile\.nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console.exe" `
-register:user `
-target:"C:\Program Files\dotnet\dotnet.exe" `
-targetargs:"test $workingFolder\Squidex.Domain.Apps.Write.Tests\Squidex.Domain.Apps.Write.Tests.csproj" `
-filter:"+[Squidex*]*" `
-skipautoprops `
-output:"$workingFolder\$reportsFolder\Write.xml" `
-oldStyle

&"$userProfile\.nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console.exe" `
-register:user `
-target:"C:\Program Files\dotnet\dotnet.exe" `
-targetargs:"test $workingFolder\Squidex.Domain.Users.Tests\Squidex.Domain.Users.Tests.csproj" `
-filter:"+[Squidex*]*" `
-skipautoprops `
-output:"$workingFolder\$reportsFolder\Users.xml" `
-oldStyle

&"$userProfile\.nuget\packages\ReportGenerator\2.5.11\tools\ReportGenerator.exe" `
-reports:"$workingFolder\$reportsFolder\*.xml" `
-targetdir:"$workingFolder\$reportsFolder\Output"