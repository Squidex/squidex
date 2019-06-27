param(
    [Parameter(Mandatory=$true)]
    [string]
    $token,

    [Parameter(Mandatory=$true)]
    [string]
    $apiBaseUrl,

    [Parameter(Mandatory=$true)]
    [string]
    $appName,

    [Parameter(Mandatory=$true)]
    [object]
    $schemaIds
)

Function CreateRule($rule, $schemaId, $schemaName) {
    $createRuleUrl = "$apiBaseUrl/apps/$appName/rules"
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }

    $rule.trigger.schemas[0] | Add-Member -Name "schemaId" -value $schemaId -MemberType NoteProperty
    $rule.action | Add-Member -Name "topicName" -value $schemaName -MemberType NoteProperty

    $body = $rule | ConvertTo-Json -Depth 32 | % { [System.Text.RegularExpressions.Regex]::Unescape($_) }
    Write-Host $body
    try{
        Write-Host "Create Rule $schemaName"
        Invoke-RestMethod -Method Post -Uri $createRuleUrl -Headers $headers -Body $body -DisableKeepAlive 
    }
    catch{
        $result = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($result)
        $responseBody = $reader.ReadToEnd()   
        Write-Host $responseBody -ForegroundColor Red
    }

}


# Create Rules
$rulesPs = Get-Content './settings/kafkaRule.json' -Raw | ConvertFrom-Json
CreateRule -rule $rulesPs -schemaId $schemaIds.CommentaryTypeSchemaId -schemaName "CommentaryType"
$rulesPs = Get-Content './settings/kafkaRule.json' -Raw | ConvertFrom-Json
CreateRule -rule $rulesPs -schemaId $schemaIds.CommentarySchemaId -schemaName "Commentary"
$rulesPs = Get-Content './settings/kafkaRule.json' -Raw | ConvertFrom-Json
CreateRule -rule $rulesPs -schemaId $schemaIds.CommoditySchemaId -schemaName "Commodity"
$rulesPs = Get-Content './settings/kafkaRule.json' -Raw | ConvertFrom-Json
CreateRule -rule $rulesPs -schemaId $schemaIds.RegionSchemaId -schemaName "Region"
