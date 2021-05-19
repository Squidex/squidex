#!/bin/bash
set -e

cd /opt/squidex

# Prompt user to enter config.

echo "> This script will setup a basic configuration of Squidex"
echo "> using docker-compose that is suitable for the most use cases."
echo "> Please go to https://docs.squidex.io for advanced configuration."
echo
echo "> Please enter the host name. You need a public DNS entry,"
echo "> because Squidex will get a certificate using lets encrypt."
echo

read -p "Enter Host Name (required): " hostName
while [ -z "$hostName" ]; do
	read -p "Enter Host Name (required): " hostName
done

echo
echo "> You can also configure external authentication providers if you want."
echo "> If no external provider is configured you can later setup an account." 
echo

read -p "Enter Google Client ID (optional): " googleClientId
read -p "Enter Google Client Secret (optional): " googleSecret

read -p "Enter Github Client ID (optional): " githubClientId
read -p "Enter Github Client Secret (optional): " githubSecret

read -p "Enter Microsoft Client ID (optional): " microsoftClientId
read -p "Enter Microsoft Client Secret (optional)": microsoftSecret

echo
echo "SUMMARY"

echo "Hostname:        		   $hostName"
echo "Google Client ID:        $googleClientId"
echo "Google Client Secret:    $googleSecret" 
echo "Github Client ID:        $githubClientId" 
echo "Github Client Secret:    $githubSecret"
echo "Microsoft Client ID:     $microsoftClientId"
echo "Microsoft Client Secret: $microsoftSecret"

envFile=".env"

[ -f $envFile ] && rm $envFile

echo "SQUIDEX_DOMAIN=$hostName" >> $envFile
echo "SQUIDEX_ADMINEMAIL=" >> $envFile
echo "SQUIDEX_ADMINPASSWORD=" >> $envFile
echo "SQUIDEX_GOOGLECLIENT=$googleClientId" >> $envFile
echo "SQUIDEX_GOOGLESECRET=$googleSecret" >> $envFile
echo "SQUIDEX_GITHUBCLIENT=$githubClientId" >> $envFile
echo "SQUIDEX_GITHUBSECRET=$githubSecret" >> $envFile
echo "SQUIDEX_MICROSOFTCLIENT=$microsoftClientId" >> $envFile
echo "SQUIDEX_MICROSOFTSECRET=$microsoftSecret" >> $envFile
echo "UI__ONLYADMINSCANCREATEAPPS=true" >> $envFile

echo
echo "Waiting 10 seconds. You may press Ctrl+C now to abort this script."

( set -x; sleep 10 )

docker-compose up -d