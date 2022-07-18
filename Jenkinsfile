#!/usr/bin/env groovy
import groovy.time.*

def aws_region = 'us-east-1'
def app = '' /* the docker app build gets stored here */
def upstream_image_name = "squidex/squidex"
def full_image_name = null
def cluster = null
def namespace = null
def helm_data_file = null

pipeline {
  agent any
  options {
    disableConcurrentBuilds()
  }
  parameters {
    string(name: 'tag', description: 'The tag to deploy: ex. 6.7.0', defaultValue: 'latest')
    string(name: 'dbname', description: 'The current mongodb to use (like: homer-squidex-staging or homer-squidex-staging-v2upgrade)', defaultValue: 'none')
    choice(name: 'cluster', choices: ['staging', 'production'], description: 'The Kubernetes Cluster to deploy to')
    choice(name: 'namespace', choices: ['content-dev', 'content-v1', 'content-v2', 'content-v2upgrade'], description: 'The environment to deploy squidex to')
   }
  stages {
    stage('Checkout') {
      steps {
          script {
            git branch:'devops', credentialsId: 'jenkins-aws-user', url: 'https://github.com/LearnWithHomer/squidex'
            full_image_name = "${upstream_image_name}:${tag}"
            cluster = params.cluster
            /* in production the 'content-v1' namespace is named 'squidex',
               so we are overriding it here                                 */
            if (params.namespace == 'content-v1' && cluster == 'production') {
              namespace = 'squidex'
            } else {
              namespace = params.namespace
            }
            helm_data_file = "${cluster}/${namespace}.yaml"
          }
      }
    }

    stage('Create New Environment MongoDB') {
      steps {
        timeout(time: 40, unit:'MINUTES') {
          sh './mongo_snapshot.sh ${cluster} ${dbname} ${tag}'
        }
      }
    }
    stage('Update Kubernetes Deployment yaml'){
      steps {
        script {
          deploymentName = homerKubernetes.getDeploymentName()
          replicas = homerKubernetes.getReplicaCount(cluster, namespace, deploymentName)
          homerKubernetes.updateDeploymentYaml(null, 'k8s/deployment.yaml', full_image_name)
          homerKubernetes.updateReplicaCount(replicas)
        }
      }
    }

    stage('Render and validate the Kubernetes deployment yaml'){
      steps {
        script {
          deploymentName = homerKubernetes.getDeploymentName()
          replicas = homerKubernetes.getReplicaCount(cluster, namespace, deploymentName)
          homerKubernetes.updateDeploymentYaml(null, 'k8s/deployment.yaml',full_image_name)
          homerKubernetes.updateReplicaCount(replicas)
          homerKubernetes.processConfigData(helm_data_file, namespace, cluster)
          homerKubernetes.validateParams()
        }
      }
    }


    stage('Deploy'){
      steps {
        script {
          homerKubernetes.deploy(namespace, cluster, 'output.yaml')
          homerKubernetes.deploy(namespace, cluster)
          homerKubernetes.watchDeploy(namespace, cluster, "jenkins-deployment.yaml") // Watch/wait for deployment
          homerKubernetes.deploy(namespace, cluster, 'k8s/service.yaml') //Create the service for the size
        }
      }
    }
  }
}
