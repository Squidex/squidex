param(
    [Parameter(Mandatory=$false)]
    [Int]
    $BuildCounter
)


try{
    # Exexute gitversion.exe file to calculate the full semantic version for the build (saved in $env:Version)
    # Format of the version is [semantic version].[branch name].[branch build count]
    $gitVersionPs = "build_tools/generate_gitversion.ps1 -BuildCounter $BuildCounter"
    Invoke-Expression $gitVersionPs

    $repoPath = "nexus.cha.rbxd.ds:8000/cosmos"
    $tagName = $env:Version

    $semanticDockerTag = $repoPath + ":" + $tagName

    Write-Host "Building docker image $semanticDockerTag"
    docker build . -t $semanticDockerTag --pull `
        --build-arg http_proxy=http://outboundproxycha.cha.rbxd.ds:3128 `
        --build-arg https_proxy=http://outboundproxycha.cha.rbxd.ds:3128 `

    Write-Host "Pushing docker image $semanticDockerTag"
    docker push $semanticDockerTag

    Write-Host "Removing Docker Image on Build Agent"
    docker rmi $semanticDockerTag 
}
catch{
    $result = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($result)
    $responseBody = $reader.ReadToEnd()   
    Write-Host $responseBody -ForegroundColor Red
}
