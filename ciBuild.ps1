param(
    [Parameter(Mandatory=$false)]
    [Int]
    $BuildCounter
)

# Exexute gitversion.exe file to calculate the full semantic version for the build (saved in $env:Version)
# Format of the version is [semantic version].[branch name].[branch build count]
$GIT_VERSION_PS = "build_tools/generate_gitversion.ps1 -BuildCounter $BuildCounter"
Invoke-Expression $GIT_VERSION_PS

$REPO_PATH = "nexus.cha.rbxd.ds:8000/cosmos"

# Convert the semantic version to lowercase and remove all breaking characters
$TAG_NAME = $env:Version.toLower() -replace "[^a-z0-9_\-]",'.'

# Build the docker tags
$SEMANTIC_DOCKER_TAG = $REPO_PATH + ":" + $TAG_NAME
$LATEST_DOCKER_TAG = $REPO_PATH + ":" + "latest"

docker build . -t $LATEST_DOCKER_TAG -t $SEMANTIC_DOCKER_TAG --pull `
    --build-arg http_proxy=http://outboundproxycha.cha.rbxd.ds:3128 `
    --build-arg https_proxy=http://outboundproxycha.cha.rbxd.ds:3128 `

docker push $LATEST_DOCKER_TAG
docker push $SEMANTIC_DOCKER_TAG