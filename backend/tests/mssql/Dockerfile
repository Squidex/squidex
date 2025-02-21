# Base OS layer: Latest Ubuntu LTS
FROM --platform=linux/amd64 ubuntu:focal

# Install prerequistes since it is needed to get repo config for SQL server
RUN export DEBIAN_FRONTEND=noninteractive && \
    apt-get update && \
    apt-get install -yq curl apt-transport-https gnupg && \
    # Get official Microsoft repository configuration
    curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - && \
    curl https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb --output packages-microsoft-prod.deb && dpkg -i packages-microsoft-prod.deb && \
    curl https://packages.microsoft.com/config/ubuntu/20.04/mssql-server-2022.list | tee /etc/apt/sources.list.d/mssql-server.list && \
    apt-get update  && \
    # Install SQL Server from apt
    apt-get install -y mssql-server && \
    # Install optional packages
    apt-get install -y mssql-server-fts && \
    ACCEPT_EULA=Y apt-get install -y mssql-tools && \
    # Cleanup the Dockerfile
    apt-get clean && \
    rm -rf /var/lib/apt/lists

# Run SQL Server process
CMD /opt/mssql/bin/sqlservr