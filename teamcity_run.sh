#!/bin/bash

# getting the values from the input arguments
namespace=$1
image_tag=$2

if [[ $namespace == *"systest"* ]]; then
	helm_values=systest_values.yaml
elif [[ $namespace == *"uat"* ]]; then
  	helm_values=uat_values.yaml
else
	helm_values=values.yaml
fi


#sudo -i helm repo update
echo "Deploying Image tag: " $image_tag
echo "---------------------------------"
sudo -i helm upgrade --install --namespace $namespace --set image.tag=$2 cosmos-deploy.image.tag=$2 --recreate-pods cosmos -f charts/cosmos/$helm_values charts/cosmos/

echo "---------- HELM History ----------"
sudo -i helm history cosmos

echo "------------ Pods list ------------"
sudo -i kubectl get pods -n $namespace