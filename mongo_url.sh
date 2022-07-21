#!/bin/bash

#setting up some vars for the version especially as a '.' is not allowed in an Atlas DB name
# staging or production
export environment=$1
# cluster we want to use as the source
export cluster=$2
# version we are migrating to
export version=$3
# getting the secrets for atlas
export MONGODB_ATLAS_PUBLIC_API_KEY=$(aws secretsmanager get-secret-value --secret-id squidex_mongo_build --query SecretString --output text --region us-east-1 | jq -r .squidex_atlas_api_key)
export MONGODB_ATLAS_PRIVATE_API_KEY=$(aws secretsmanager get-secret-value --secret-id squidex_mongo_build --query SecretString --output text --region us-east-1 | jq -r .squidex_atlas_private_api_key)

if [ $environment = "staging" ] ; then
  export project="5eea50347b142b17471b8f7d"
elif [ $environment = "production" ] ; then
  export project="5e8503c8f954f12a69977894"
fi

#get secrets from parameter store for Atlas API keys

#definitely a hack for now, but getting atlas for the deployment

if ! command -v atlas &> /dev/null
then
  source ./mongo_deps.sh > /dev/null 2>&1
fi
docker pull imega/jq > /dev/null 2>&1 #We don't want any docker output

atlas cluster describe homer-squidex-${environment}-${version} --projectId ${project} -o json |  docker run --rm -i imega/jq -r '.connectionStrings.standardSrv'
