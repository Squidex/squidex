# BACKEND

# Build the image
docker build . -t squidex-build-backend-image -f dockerfile.backend

# Open the image
docker create --name squidex-build-backend-container squidex-build-backend-image

# Copy the output to the host file system
docker cp squidex-build-backend-container:/out ./publish

# Cleanup
docker rm squidex-build-backend-container

# ------------------------

# BACKEND

# Build the image
docker build . -t squidex-build-frontend-image -f dockerfile.frontend

# Open the image
docker create --name squidex-build-frontend-container squidex-build-frontend-image

# Copy the output to the host file system
docker cp squidex-build-frontend-container:/out ./publish/wwwroot/build

# Cleanup
docker rm squidex-build-frontend-container