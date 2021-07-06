#!/bin/sh

echo "Waiting 60 seconds for MongoDB ..."
sleep 60

if [ "x$DOMAIN" = "x" ]; then
    export DOMAIN=$RENDER_EXTERNAL_HOSTNAME
fi

export URLS__BASEURL="https://${DOMAIN}"
export EVENTSTORE__MONGODB__CONFIGURATION="mongodb://${MONGO}"
export STORE__MONGODB__CONFIGURATION="mongodb://${MONGO}"

export IDENTITY__GOOGLECLIENT=""
export IDENTITY__GITHUBCLIENT=""
export IDENTITY__MICROSOFTCLIENT=""

dotnet Squidex.dll