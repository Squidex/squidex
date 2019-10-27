# Build the image
docker build . -t squidex-build-image

# Open the image
docker create --name squidex-build-container squidex-build-image

# Copy the output to the host file system
docker cp squidex-build-container:/app/. ./publish

# Cleanup
docker rm squidex-build-container
docker rmi squidex-build-image