param(
    [Parameter(Mandatory=$true)]
    [string]
    $token,

    [Parameter(Mandatory=$true)]
    [string]
    $apiBaseUrl,

    [Parameter(Mandatory=$true)]
    [string]
    $appName
)

function CreateContent ($schemaName, $content) {

    $createContentUrl = "$apiBaseUrl/content/$appName/$schemaName" + "?publish=true"
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }

    $body = $content | ConvertTo-Json -Depth 3

    try{
        Invoke-RestMethod -Method Post -Uri $createContentUrl -Headers $headers -Body $body -DisableKeepAlive 
    }
    catch{
        $result = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($result)
        $responseBody = $reader.ReadToEnd()   
        Write-Host $responseBody -ForegroundColor Red
    }

}


# Create Commodity Ref-Data
Write-Host "Create Ref-Data - Commodities"
$commoditiesPs = Get-Content './contents/commodities.json' | ConvertFrom-Json
$commoditiesArray = $commoditiesPs.commodities

foreach($commodity in $commoditiesArray)
{
    CreateContent -schemaName 'commodity' -content $commodity
}

# Create Commentary-Types Ref-Data
Write-Host "Create Ref-Data - Commentary Types"
$commentaryTypesPs = Get-Content './contents/commentaryTypes.json' | ConvertFrom-Json
$commentaryTypesArray = $commentaryTypesPs.commentaryTypes

foreach($commentaryType in $commentaryTypesArray)
{
    CreateContent -schemaName 'commentary-type' -content $commentaryType
}
