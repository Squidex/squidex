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

function CreateApp {
    $createAppUrl = "$apiBaseUrl/apps"

    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    $body = @{"name" = $appName} | ConvertTo-Json
    
    try{
        Write-Host "Creating app - $appName"
        Invoke-RestMethod -Method Post -Uri $createAppUrl -Headers $headers -Body $body 
    }
    catch{
        $result = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($result)
        $responseBody = $reader.ReadToEnd()   
        Write-Host $responseBody -ForegroundColor Red
    }    
}

CreateApp