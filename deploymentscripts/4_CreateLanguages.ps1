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

Function CreateLanguage($language) {
    $createLanguageUrl = "$apiBaseUrl/apps/$appName/languages"
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }

    $body = $language | ConvertTo-Json -Depth 3

    try{
        Write-Host "Create Language"
        Invoke-RestMethod -Method Post -Uri $createLanguageUrl -Headers $headers -Body $body -DisableKeepAlive 
    }
    catch{
        $result = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($result)
        $responseBody = $reader.ReadToEnd()   
        Write-Host $responseBody -ForegroundColor Red
    }

}

Function UpdateLanguage($language) {
    $updateLanguageUrl = "$apiBaseUrl/apps/$appName/languages/" + $language.language
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    $body = $language.settings | ConvertTo-Json -Depth 3

    try{
        Write-Host "Update Language"
        Invoke-RestMethod -Method Put -Uri $updateLanguageUrl -Headers $headers -Body $body -DisableKeepAlive 
    }
    catch{
        $result = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($result)
        $responseBody = $reader.ReadToEnd()   
        Write-Host $responseBody -ForegroundColor Red
    }

}

# Create Languages
$languagesPs = Get-Content './settings/languages.json'| ConvertFrom-Json
$languagesArray = $languagesPs.languages

foreach($language in $languagesArray)
{
    CreateLanguage -language $language
    UpdateLanguage -language $language
}
