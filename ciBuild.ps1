param(

    [Parameter(Mandatory=$false)]
    [Int]
    $BuildCounter
)

$TAG_NAME= .\build_tools\generate_gitversion.ps1 -BuildCounter $BuildCounter
Write-Host $TAG_NAME.ToLower()
docker build . -t $TAG_NAME --pull `
    --build-arg http_proxy=http://outboundproxycha.cha.rbxd.ds:3128 `
    --build-arg https_proxy=http://outboundproxycha.cha.rbxd.ds:3128 `
    -f Dockerfile
docker push $TAG_NAME