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

function CreateRole ($roleName) {
    $createRoleUrl = "$apiBaseUrl/apps/$appName/roles"

    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }

    $body = @{"name" = $roleName} | ConvertTo-Json

    try{
        Write-Host "Creating role - $roleName"
        Invoke-RestMethod -Method Post -Uri $createRoleUrl -Headers $headers -Body $body -DisableKeepAlive 
    }
    catch{
        $result = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($result)
        $responseBody = $reader.ReadToEnd()   
        Write-Host $responseBody -ForegroundColor Red
    }
}

function AddPermissionsToRole ($roleName, $body) {
    $updateRoleUrl = "$apiBaseUrl/apps/$appName/roles/$roleName/"

    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
 
    try{
        Write-Host "Adding permissions to role - $roleName"
        Invoke-RestMethod -Method Put -Uri $updateRoleUrl -Headers $headers -Body $body -DisableKeepAlive
    }
    catch{
        $result = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($result)
        $responseBody = $reader.ReadToEnd()   
        Write-Host $responseBody -ForegroundColor Red
    }
}

function CreateRoles {

    $analystPermissions = Get-Content './roles/analyst.json'
    CreateRole "CMS Analyst"
    AddPermissionsToRole "CMS Analyst" $analystPermissions

    $editorPermissions = Get-Content './roles/editor.json'
    CreateRole "CMS Editor"
    AddPermissionsToRole "CMS Editor" $editorPermissions
    
    $copyEditorPermissions = Get-Content './roles/copyEditor.json'
    CreateRole "CMS Copy Editor"
    AddPermissionsToRole "CMS Copy Editor" $copyEditorPermissions

    $managingEditorPermissions = Get-Content './roles/managingEditor.json'
    CreateRole "CMS Managing Editor"
    AddPermissionsToRole "CMS Managing Editor" $managingEditorPermissions

    $managingAnalystJson = Get-Content './roles/managingAnalyst.json'
    CreateRole "CMS Managing Analyst"
    AddPermissionsToRole "CMS Managing Analyst" $managingAnalystJson
    
}

CreateRoles