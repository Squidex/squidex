#!/usr/bin/env groovy
import groovy.time.*

def aws_region = 'us-east-1'
def app = '' /* the docker app build gets stored here */
def upstream_image_name = "squidex/squidex"
def full_image_name = null
def cluster = null
def namespace = null
def helm_data_file = null
def mongo_url = null
def squidex_version = null //Sanitized version of tag without . in the number

pipeline {
  agent any
  options {
    disableConcurrentBuilds()
  }
  parameters {
    string(name: 'tag', description: 'The tag to deploy: ex. 6.7.0', defaultValue: 'latest')
    string(name: 'dbname', description: 'The current mongodb to use (like: homer-squidex-staging or homer-squidex-staging-v2upgrade)', defaultValue: 'none')
    choice(name: 'cluster', choices: ['staging', 'production'], description: 'The Kubernetes Cluster to deploy to')
    choice(name: 'namespace', choices: ['content-v1', 'content-v2', 'content-v2upgrade'], description: 'The environment to deploy squidex to')
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
            squidex_version = tag.minus(".") //We need a DNS friendly value so need to remove dots
          }
      }
    }

    stage('Create New Environment MongoDB') {
      steps {
        script{
          timeout(time: 40, unit:'MINUTES') {
            sh './mongo_snapshot.sh ${cluster} ${dbname} ${squidex_version}'
            mongo_url = sh(returnStdout:true, script: './mongo_url.sh ${cluster} ${dbname} ${squidex_version}')
          }
        }
      }
    }

    stage('Render and validate the Kubernetes deployment yaml'){
      steps {
        script {
          replicas = homerKubernetes.getReplicaCount(cluster, namespace, deploymentName)
          mongoLogin = sh(returnStdout: true, script:"aws secretsmanager get-secret-value --secret-id squidex_mongo_build --query SecretString --output text | jq -r .${namespace}_login")
          helmArgs = "--set replicaCount=${replicas} --set squidexTag=${tag} --set version=${squidex_version} --set mongoconnectionstring=${mongoLogin} --set mongourl=${mongo_url}"
          homerKubernetes.processConfigData(helm_data_file, namespace, cluster, helmArgs, "./helm-chart-deploy")
          homerKubernetes.validateParams()
        }
      }
    }


    stage('Deploy'){
      steps {
        script {
          homerKubernetes.deploy(namespace, cluster, 'output.yaml')
        }
      }
    }
  }
}
