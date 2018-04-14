#
# Stage 1, Prebuild
#
FROM squidex/aspnetcore-build-phantomjs:2.0.3-jessie as builder

# See https://crbug.com/795759
RUN apt-get update && apt-get install -yq libgconf-2-4

# Install latest chrome dev package and fonts to support major charsets (Chinese, Japanese, Arabic, Hebrew, Thai and a few others)
# Note: this installs the necessary libs to make the bundled version of Chromium that Puppeteer
# installs, work.
RUN apt-get update && apt-get install -y wget --no-install-recommends \
    && wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list' \
    && apt-get update \
    && apt-get install -y google-chrome-unstable fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst ttf-freefont \
      --no-install-recommends \
    && rm -rf /var/lib/apt/lists/* \
    && apt-get purge --auto-remove -y curl \
    && rm -rf /src/*.deb

# It's a good idea to use dumb-init to help prevent zombie chrome processes.
ADD https://github.com/Yelp/dumb-init/releases/download/v1.2.0/dumb-init_1.2.0_amd64 /usr/local/bin/dumb-init
RUN chmod +x /usr/local/bin/dumb-init

# Uncomment to skip the chromium download when installing puppeteer. If you do,
# you'll need to launch puppeteer with:
#     browser.launch({executablePath: 'google-chrome-unstable'})
# ENV PUPPETEER_SKIP_CHROMIUM_DOWNLOAD true

# Install puppeteer so it's available in the container.
RUN npm i puppeteer

# Add user so we don't need --no-sandbox.
RUN groupadd -r pptruser && useradd -r -g pptruser -G audio,video pptruser \
    && mkdir -p /home/pptruser/Downloads \
    && chown -R pptruser:pptruser /home/pptruser \
    && chown -R pptruser:pptruser /node_modules

# Run everything after as non-privileged user.
USER pptruser

COPY src/Squidex/package.json /tmp/package.json

# Install Node packages 
RUN cd /tmp && npm install

COPY . .

WORKDIR /

# Build Frontend
RUN cp -a /tmp/node_modules /src/Squidex/ \
 && cd /src/Squidex \
 && npm run test:coverage \
 && npm run build:copy \
 && npm run build
 
# Test Backend
RUN dotnet restore \
 && dotnet test tests/Squidex.Infrastructure.Tests/Squidex.Infrastructure.Tests.csproj \ 
 && dotnet test tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj \ 
 && dotnet test tests/Squidex.Domain.Apps.Entities.Tests/Squidex.Domain.Apps.Entities.Tests.csproj \
 && dotnet test tests/Squidex.Domain.Users.Tests/Squidex.Domain.Users.Tests.csproj \
 && dotnet test tests/Squidex.Tests/Squidex.Tests.csproj

# Publish
RUN dotnet publish src/Squidex/Squidex.csproj --output /out/ --configuration Release

#
# Stage 2, Build runtime
#
FROM microsoft/aspnetcore:2.0.3-jessie

# Default AspNetCore directory
WORKDIR /app

# Copy from nuild stage
COPY --from=builder /out/ .

EXPOSE 80
EXPOSE 33333
EXPOSE 40000

ENTRYPOINT ["dotnet", "Squidex.dll"]