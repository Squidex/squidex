#!/bin/bash
set -x
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
  source ./mongo_deps.sh
fi

#get cluster version
export cluster_version=`atlas clusters describe $cluster --projectId ${project} | grep $2 | awk '{print $3}' | grep -o '^[^.]*\.[0-9]*'`

#creating snapshot of current cluster to use to build the new cluster
export snapshot=`atlas backups snapshots create $cluster --desc test-upgrade-backup --projectId ${project} | awk -F\' '{print $2}'`

#watching the cluster snapshot so when it is done, we can create our new database
atlas backups snapshots watch ${snapshot} --clusterName $cluster --projectId ${project} 

#create new cluster
atlas cluster create homer-squidex-${environment}-${version} --projectId ${project} --provider AWS --region US_EAST_1 --members 3 --tier M10 --mdbVersion ${cluster_version} --diskSizeGB 100

#wait for cluster to be ready
atlas cluster watch homer-squidex-${environment}-${version} --projectId ${project}

#with the snapshot done, time to create the new database
atlas backup restore start automated --clusterName ${cluster} --snapshotId ${snapshot} --targetClusterName homer-squidex-${environment}-${version} --targetProjectId ${project} --projectId ${project}

#wait for cluster restore to be done
atlas cluster watch homer-squidex-${environment}-${version} --projectId ${project}
