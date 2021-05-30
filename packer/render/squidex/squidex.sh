#!/bin/sh

echo "Waiting 60 seconds ..."
sleep 60

export URLS__BASEURL="https://${DOMAIN}"
export EVENTSTORE__MONGODB__CONFIGURATION="mongodb://${MONGO}"
export STORE__MONGODB__CONFIGURATION="mongodb://${MONGO}"

dotnet Squidex.dll