FROM mcr.microsoft.com/dotnet/sdk:8.0 AS test

WORKDIR /app

# Copy solution and project files for backend
COPY backend/*.sln ./backend/
COPY backend/src/*/*.csproj ./backend/src/*/
COPY backend/tests/*/*.csproj ./backend/tests/*/
COPY backend/extensions/*/*.csproj ./backend/extensions/*/

# Restore dependencies for backend only
RUN cd backend && dotnet restore

# Copy the backend source code
COPY backend/ ./backend/

# Install ReportGenerator tool for test coverage
RUN dotnet tool install -g dotnet-reportgenerator-globaltool

# Add dotnet tools to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Run specific test project like Calculator project - simple and focused
CMD ["bash", "-c", "echo .NET VERSION && dotnet --version && echo GLIBC VERSION && ldd --version && echo GLIBC VERSION CHECK && cd backend && dotnet test tests/Squidex.Domain.Apps.Entities.Tests/Squidex.Domain.Apps.Entities.Tests.csproj --collect:'XPlat Code Coverage' --results-directory TestResults"]
