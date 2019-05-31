param(

    [Parameter(Mandatory=$false)]
    [Int]
    $BuildCounter
)

$TAG_NAME= .\build_tools\generate_gitversion.ps1 -BuildCounter $BuildCounter
docker build . -t $TAG_NAME --pull `
    --build-arg http_proxy=http://outboundproxycha.cha.rbxd.ds:3128 `
    --build-arg https_proxy=http://outboundproxycha.cha.rbxd.ds:3128 `
    -f Dockerfile
docker push "nexus.cha.rbxd.ds:8000/cosmos:" + $TAG_NAME