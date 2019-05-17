param(

    [Parameter(Mandatory=$true)]
    [string]
    $identityServiceBaseUrl,

    [Parameter(Mandatory=$true)]
    [string]
    $tokenUser,

    [Parameter(Mandatory=$true)]
    [string]
    $tokenPassword
)

Function GetToken {
    $IdentityServiceUrl = $identityServiceBaseUrl.TrimEnd('/')+'/connect/token/'
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12   
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $tokenUser,$tokenPassword)))
    $formFields = @{grant_type='client_credentials';scope='all'}
    try{
        $response = Invoke-RestMethod -Uri $IdentityServiceUrl -Method Post -Body $formFields -ContentType "application/x-www-form-urlencoded" -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        $token = $response.access_token
        return $token
    }
    catch{
        Write-Host "Exception:" $_.Exception
    }
}

GetToken