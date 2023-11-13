FROM node:20.9.0-slim

ENV CHROME_BIN=/usr/bin/chromium

# Install latest chrome dev package and fonts to support major charsets (Chinese, Japanese, Arabic, Hebrew, Thai and a few others)
# Note: this installs the necessary libs to make the bundled version of Chromium that Puppeteer
# installs, work.
RUN apt-get update \
 && apt-get update \
 && apt-get install -y chromium fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst fonts-freefont-ttf libxss1 --no-install-recommends \
 && rm -rf /var/lib/apt/lists/*

WORKDIR /puppeteer

# Install puppeteer so it's available in the container.
RUN npm init -y \
 && npm install puppeteer