TAG_NAME="nexus.cha.rbxd.ds:8000/cosmos:test-tag"
docker build -t $TAG_NAME --pull \
    --build-arg http_proxy=http://outboundproxycha.cha.rbxd.ds:3128 \
    --build-arg https_proxy=http://outboundproxycha.cha.rbxd.ds:3128 \
    -f Dockerfile .
docker push $TAG_NAME