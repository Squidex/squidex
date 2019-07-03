#!/usr/bin/env sh

# start DB
nohup node ./JSFiles/utils/Db.js &>/dev/null &

# dotnet app
nohup dotnet run --project ../src/Squidex &>/dev/null &

# run deployment scipts
#waitfor https://localhost:5000/ && cd ../deploymentscripts && @powershell -NoProfile -ExecutionPolicy Unrestricted -Command ./Deployment.ps1 -identityServiceBaseUrl 'http://identityservice.systest.tesla.cha.rbxd.ds' -apiBaseUrl 'https://localhost:5000/api' -tokenUser 'CMSDeployer' -tokenPassword 'p@55w0rd' -appName 'commentary' 
