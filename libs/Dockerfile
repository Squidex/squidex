FROM microsoft/aspnetcore-build:2.0.3-jessie

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