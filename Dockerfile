FROM mcr.microsoft.com/dotnet/sdk:8.0 AS test

WORKDIR /app

# FFMPEG for tests
RUN apt-get update \
 && apt-get install -y ffmpeg

# Copy the entire project
COPY . .

# Restore dependencies for backend
RUN cd backend && dotnet restore

# Install ReportGenerator tool for test coverage
RUN dotnet tool install -g dotnet-reportgenerator-globaltool

# Install additional tools for debugging and coverage
RUN dotnet tool install -g dotnet-dump \
 && dotnet tool install -g dotnet-gcdump \
 && dotnet tool install -g coverlet.console

# Add dotnet tools to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Set environment variables for test configuration
ARG SQUIDEX__BUILD__VERSION=7.0.0
ARG SQUIDEX__BUILD__ARGS

# Run backend tests with coverage and diagnostics
CMD ["bash", "-c", "echo .NET VERSION && dotnet --version && echo GLIBC VERSION && ldd --version && echo GLIBC VERSION CHECK && cd backend && dotnet test --filter \"FullyQualifiedName~DefaultAppLogStoreTests\" --collect:\"XPlat Code Coverage\" --results-directory TestResults"]
