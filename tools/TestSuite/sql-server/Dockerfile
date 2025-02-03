FROM mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04

# Switch to root to perform installation
USER root

ENV ACCEPT_EULA=Y

# Install prerequisites, add Microsoft repository, and install mssql-tools and unixodbc-dev
RUN apt-get update && \
    apt-get install -y curl gnupg2 && \
    curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - && \
    curl https://packages.microsoft.com/config/ubuntu/22.04/prod.list -o /etc/apt/sources.list.d/mssql-release.list && \
    apt-get update && \
    # Install mssql-tools (which includes sqlcmd) and its dependencies
    apt-get install -y mssql-tools unixodbc-dev && \
    # Clean up APT caches to reduce image size
    rm -rf /var/lib/apt/lists/*

# Add the SQL Server tools to the PATH environment variable
ENV PATH="${PATH}:/opt/mssql-tools/bin"

# Revert to the default user for security
USER mssql

# Expose the SQL Server port
EXPOSE 1433

# Start SQL Server when the container launches
CMD ["/opt/mssql/bin/sqlservr"]