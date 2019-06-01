param(
    [Parameter(Mandatory=$false)]
    [Int]
    $BuildCounter
)

# Exexute gitversion.exe file to calculate the full semantic version for the build (saved in $env:Version)
# Format of the version is [semantic version].[branch name].[branch build count]
$GIT_VERSION_PS = "build_tools/generate_gitversion.ps1 -BuildCounter $BuildCounter"
Invoke-Expression $GIT_VERSION_PS

# Convert the semantic version to lowercase and remove all breaking characters
$TAG_NAME = $env:Version.toLower() -replace "[^a-z0-9_\-]",'.'

# Add the repository to the full semantic version
$DOCKER_TAG =  "nexus.cha.rbxd.ds:8000/cosmos:" + $TAG_NAME

docker build . -t $DOCKER_TAG --pull `
    --build-arg http_proxy=http://outboundproxycha.cha.rbxd.ds:3128 `
    --build-arg https_proxy=http://outboundproxycha.cha.rbxd.ds:3128 `

# docker push $DOCKER_TAG