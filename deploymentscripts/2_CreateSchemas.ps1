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

function  CreateSchema ($schemaName, $body) {

    $createSchemaUrl = $apiBaseUrl + '/apps/' + $appName + '/schemas'

    $headers = @{
        'Authorization' = 'Bearer ' + $token
        'Content-Type' = 'application/json'
    }

    try{
        Write-Host "Creating schema - $schemaName"
        $response = Invoke-RestMethod -Method Post -Uri $createSchemaUrl -Headers $headers -Body $body -DisableKeepAlive
        return $response
    }
    catch{
        $result = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($result)
        $responseBody = $reader.ReadToEnd()   
        Write-Host $responseBody -ForegroundColor Red
    }

}

function CreateSchemas {

    $commoditySchema = GET-Content './schemas/ref-data/commodity.json'
    $commoditySchemaResponse = CreateSchema 'commodity'  $commoditySchema
    
    $commentaryType = GET-Content './schemas/ref-data/commentaryType.json'
    $commentaryTypeResponse = CreateSchema 'commentary-type'  $commentaryType

    $commentaryObj = GET-Content './schemas/commentary.json' -Raw | ConvertFrom-Json 
    $commentaryObj.fields[0].properties | Add-Member -Name "schemaId" -value $commoditySchemaResponse.id -MemberType NoteProperty
    $commentaryObj.fields[1].properties | Add-Member -Name "schemaId" -value $commentaryTypeResponse.id -MemberType NoteProperty
    
    $commentary = $commentaryObj | ConvertTo-Json -Depth 32
    $commentarySchemaResponse = CreateSchema 'commentary'  $commentary

    $response = @{
        CommoditySchemaId = $commoditySchemaResponse.id
        CommentaryTypeSchemaId = $commentaryTypeResponse.id
        CommentarySchemaId = $commentarySchemaResponse.id
    }

    return New-Object psobject -Property $response
}

CreateSchemas