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

function AddContributor ($role, $email) {

    $addContributorUrl = "$apiBaseUrl/apps/$appName/contributors"
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }

Write-Host $addContributorUrl
    $body = "{ ""contributorId"": ""$email"", ""role"": ""$role"", ""invite"": true }"

    try{
        Invoke-RestMethod -Method Post -Uri $addContributorUrl -Headers $headers -Body $body -DisableKeepAlive 

        Write-Host "User $email added"
    }
    catch{   
        $result = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($result)
        $responseBody = $reader.ReadToEnd()   
        Write-Host $responseBody -ForegroundColor Red
    }

}

AddContributor -role "Editor" -email "vegatesteditor@cha.rbxd.ds"
AddContributor -role "Editor" -email "vegatestreviewer@cha.rbxd.ds"