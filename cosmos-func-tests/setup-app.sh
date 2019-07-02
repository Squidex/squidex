#!/usr/bin/env sh

# start DB
nohup node ./JSFiles/utils/Db.js &

# dotnet app
nohup dotnet run --project ../src/Squidex &
