FROM microsoft/dotnet:2.1-sdk

# Install runtime dependencies
RUN apt-get update \
 && apt-get install -y --no-install-recommends ca-certificates bzip2 libfontconfig \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*
 
# Install official PhantomJS release
RUN set -x  \
 && apt-get update \
 && apt-get install -y --no-install-recommends \
 && mkdir /srv/var \
 && mkdir /tmp/phantomjs \
   # Download Phantom JS
 && curl -L https://bitbucket.org/ariya/phantomjs/downloads/phantomjs-2.1.1-linux-x86_64.tar.bz2 | tar -xj --strip-components=1 -C /tmp/phantomjs \
   # Copy binaries only
 && mv /tmp/phantomjs/bin/phantomjs /usr/local/bin \
   # Create symbol link
   # Clean up
 && apt-get autoremove -y \
 && apt-get clean all \
 && rm -rf /tmp/* /var/lib/apt/lists/*

RUN phantomjs --version

# Install Node
ENV NODE_VERSION 8.9.4
ENV NODE_DOWNLOAD_SHA 21fb4690e349f82d708ae766def01d7fec1b085ce1f5ab30d9bda8ee126ca8fc
RUN curl -SL "https://nodejs.org/dist/v${NODE_VERSION}/node-v${NODE_VERSION}-linux-x64.tar.gz" --output nodejs.tar.gz \
 && echo "$NODE_DOWNLOAD_SHA nodejs.tar.gz" | sha256sum -c - \
 && tar -xzf "nodejs.tar.gz" -C /usr/local --strip-components=1 \
 && rm nodejs.tar.gz \
 && ln -s /usr/local/bin/node /usr/local/bin/nodejs
 
# Install Google Chrome

# See https://crbug.com/795759
RUN apt-get update && apt-get install -yq libgconf-2-4

# Install latest chrome dev package and fonts to support major charsets (Chinese, Japanese, Arabic, Hebrew, Thai and a few others)
RUN apt-get update && apt-get install -y wget --no-install-recommends \
 && wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
 && sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list' \
 && apt-get update \
 && apt-get install -y google-chrome-unstable fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst ttf-freefont \
   --no-install-recommends \
 && rm -rf /var/lib/apt/lists/* \
 && apt-get autoremove -y \
 && rm -rf /src/*.deb

# It's a good idea to use dumb-init to help prevent zombie chrome processes.
ADD https://github.com/Yelp/dumb-init/releases/download/v1.2.0/dumb-init_1.2.0_amd64 /usr/local/bin/dumb-init

RUN chmod +x /usr/local/bin/dumb-init

# Install puppeteer so it's available in the container.
RUN npm i puppeteer