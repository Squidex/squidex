param(

    [Parameter(Mandatory=$false)]
    [Int]
    $BuildCounter
)

$GIT_VERSION_PS = "build_tools/generate_gitversion.ps1 -BuildCounter $BuildCounter"
Invoke-Expression $GIT_VERSION_PS

$TAG_NAME = $env:Version.toLower() -replace "[^a-z0-9_\-]",'.'
$DOCKER_TAG =  "nexus.cha.rbxd.ds:8000/cosmos:" + $TAG_NAME

Write-Host $DOCKER_TAG

docker build . -t $DOCKER_TAG --pull `
    --build-arg http_proxy=http://outboundproxycha.cha.rbxd.ds:3128 `
    --build-arg https_proxy=http://outboundproxycha.cha.rbxd.ds:3128 `
    -f Dockerfile
docker push $DOCKER_TAG