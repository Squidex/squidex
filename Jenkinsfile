#!/usr/bin/env groovy
import groovy.time.*

def ecr_repo_east = '597764168253.dkr.ecr.us-east-1.amazonaws.com'
def ecr_repo_south = '597764168253.dkr.ecr.ap-south-1.amazonaws.com'
def ecr_repo_southeast = '597764168253.dkr.ecr.ap-southeast-2.amazonaws.com'
def aws_region = 'us-east-1'
def app = '' /* the docker app build gets stored here */
def homer_image_name = "${ecr_repo_east}/homer-squidex"
def upstream_image_name = "squidex/squidex"
def upstream_image_tag = "4.5.1"
def full_image_name = null
def cluster = null
def namespace = null

pipeline {
  agent any
  options {
    disableConcurrentBuilds()
  }
  parameters {
    string(name: 'tag', description: 'The tag to deploy', defaultValue: 'latest')
    choice(name: 'cluster', choices: ['staging', 'production'], description: 'The Kubernetes Cluster to deploy to')
    choice(name: 'namespace', choices: ['content-dev', 'content-v1', 'content-v2'], description: 'The environment to deploy squidex to')
   }
  stages {
    stage('Checkout') {
      steps {
          script {
            git credentialsId: 'jenkins-aws-user', url: 'https://github.com/LearnWithHomer/squidex'
            if (params.namespace == "content-v1"){
              full_image_name = "${upstream_image_name}:${upstream_image_tag}"
            }
            full_image_name = "${image_name}:${tag}"
            cluster = params.cluster
            namespace = params.namespace
          }
      }
    }

    stage('Update Kubernetes Deployment yaml'){
      steps {
        script {
          deploymentName = homerKubernetes.getDeploymentName()
          replicas = homerKubernetes.getReplicaCount(cluster, namespace, deploymentName)
          homerKubernetes.updateDeploymentYaml(app, 'k8s/deployment.yaml')
          homerKubernetes.updateReplicaCount(replicas)
        }
      }
    }


    stage('Deploy'){
      when {
        expression {deploy}
      }
      steps {
        script {
          homerKubernetes.deploy(namespace, cluster)
          homerKubernetes.deploy(namespace, cluster, 'k8s/service.yaml') //Create the service for the size
        }
      }
    }
  }
}
