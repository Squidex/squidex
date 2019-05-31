param(

    [Parameter(Mandatory=$false)]
    [Int]
    $BuildCounter
)

$TAG_NAME = .\build_tools\generate_gitversion.ps1 -BuildCounter $BuildCounter

Write-Host $TAG_NAME

$DOCKER_TAG =  $TAG_NAME.toLower() -replace "[^a-z0-9_\-]",'.'

Write-Host $DOCKER_TAG

docker build . -t $DOCKER_TAG --pull `
    --build-arg http_proxy=http://outboundproxycha.cha.rbxd.ds:3128 `
    --build-arg https_proxy=http://outboundproxycha.cha.rbxd.ds:3128 `
    -f Dockerfile
docker push $DOCKER_TAG