param(

    [Parameter(Mandatory=$false)]
    [Int]
    $BuildCounter
)

$TAG_NAME = .\build_tools\generate_gitversion.ps1 -BuildCounter $BuildCounter
$DOCKER_TAG =  $TAG_NAME.toLower() -replace "[^a-z0-9_\-]",'.'

docker build . -t $DOCKER_TAG --pull `
    --build-arg http_proxy=http://outboundproxycha.cha.rbxd.ds:3128 `
    --build-arg https_proxy=http://outboundproxycha.cha.rbxd.ds:3128 `
    -f Dockerfile
docker push $DOCKER_TAG