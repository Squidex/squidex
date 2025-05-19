Param(
	[switch]$testInfrastructure,
	[switch]$testAppsCore,
	[switch]$testAppsEntities,
	[switch]$testUsers,
	[switch]$testWeb,
	[switch]$testAll,
    [switch]$noClean
)

$ErrorActionPreference = "Stop"

$folderReports = ".\_test-output"
$folderWorking = Get-Location
$versionOpenCover = "4.7.1221"
$versionReportGenerator = "5.4.1"

if ($testAll) {
    $testInfrastructure = $true
    $testAppsCore = $true
    $testAppsEntities = $true
    $testUsers = $true
    $testWeb = $true
}

Write-Host "Test Infrastructure: $testInfrastructure"
Write-Host "Test Apps Core:      $testAppsCore"
Write-Host "Test Apps Entities:  $testAppsEntities"
Write-Host "Test Users:          $testUsers"
Write-Host "Test Web:            $testWeb"

if (!$noClean) {
    if (Test-Path $folderReports) {
        Remove-Item $folderReports -recurse

        Write-Host "Recreated '$folderReports' folder"
    }
}

if (!(Test-Path $folderReports)) {
    New-Item -ItemType directory -Path $folderReports
}

if ($testInfrastructure) {
    $projectName = "Squidex.Infrastructure.Tests"

    dotnet test "$folderWorking\$projectName\$projectName.csproj" `
        --no-restore `
        --filter "Category!=Dependencies & Category!=TestContainer" `
        --collect "XPlat Code Coverage" `
        --results-directory "$folderReports" `
        --settings "$folderWorking\coverlet.runsettings.xml"
}

if ($testAppsCore) {
    $projectName = "Squidex.Domain.Apps.Core.Tests"

    dotnet test "$folderWorking\$projectName\$projectName.csproj" `
        --no-restore `
        --filter "Category!=Dependencies & Category!=TestContainer" `
        --collect "XPlat Code Coverage" `
        --results-directory "$folderReports" `
        --settings "$folderWorking\coverlet.runsettings.xml"
}

if ($testAppsEntities) {
    $projectName = "Squidex.Domain.Apps.Entities.Tests"

    dotnet test "$folderWorking\$projectName\$projectName.csproj" `
        --no-restore `
        --filter "Category!=Dependencies & Category!=TestContainer" `
        --collect "XPlat Code Coverage" `
        --results-directory "$folderReports" `
        --settings "$folderWorking\coverlet.runsettings.xml"
}

if ($testUsers) {
    $projectName = "Squidex.Domain.Users.Tests"

    dotnet test "$folderWorking\$projectName\$projectName.csproj" `
        --no-restore `
        --filter "Category!=Dependencies & Category!=TestContainer" `
        --collect "XPlat Code Coverage" `
        --results-directory "$folderReports" `
        --settings "$folderWorking\coverlet.runsettings.xml"
}

if ($testWeb) {
    $projectName = "Squidex.Web.Tests"

    dotnet test "$folderWorking\$projectName\$projectName.csproj" `
        --no-restore `
        --filter "Category!=Dependencies & Category!=TestContainer" `
        --collect "XPlat Code Coverage" `
        --results-directory "$folderReports" `
        --settings "$folderWorking\coverlet.runsettings.xml"
}


dotnet tool install -g dotnet-reportgenerator-globaltool

reportgenerator `
    -reports:"$folderReports\**\coverage.cobertura.xml" `
    -targetdir:"$folderReports\report" `
    -reporttypes:Html