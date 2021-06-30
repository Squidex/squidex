#!/bin/sh

export URLS__BASEURL="https://${DOMAIN}"

export MONGO_STRIP="$(echo ${MONGO_RS_URL} | sed 's@mongodb://@@g' | sed 's@?replicaSet@Squidex?retryWrites=false\&replicaSet@g')"

export EVENTSTORE__MONGODB__CONFIGURATION="mongodb://${MONGO_USERNAME}:${MONGO_PASSWORD}@${MONGO_STRIP}"
export STORE__MONGODB__CONFIGURATION="mongodb://${MONGO_USERNAME}:${MONGO_PASSWORD}@${MONGO_STRIP}"
export STORE__MONGODB__CONTENTDATABASE="Squidex"

export ASPNETCORE_URLS="http://+:$PORT"

export IDENTITY__GOOGLECLIENT=""
export IDENTITY__GITHUBCLIENT=""
export IDENTITY__MICROSOFTCLIENT=""

dotnet Squidex.dll