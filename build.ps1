# Build the image
docker build . -t build-image -f Dockerfile.build

# Open the image
docker create --name build-cont build-image

# Copy the output to the host file system
docker cp build-cont:/out ./publish

# Cleanup
docker rm build-cont