#!/usr/bin/env groovy
import groovy.time.*

def aws_region = 'us-east-1'
def app = '' /* the docker app build gets stored here */
def upstream_image_name = "squidex/squidex"
def create_db = true
def full_image_name = null
def cluster = null
def dbname = null
def origin = null
def tag = null
def namespace = null
def helm_data_file = null
def mongo_url = null
def squidex_version = null //Sanitized version of tag without . in the number
def env = ["staging": "hmr-kops-staging",
           "production": "hmr-kops-production"]
def environment = null

pipeline {
  agent any
  options {
    disableConcurrentBuilds()
  }
  parameters {
    booleanParam(name: 'create_db', defaultValue: true, description: 'Create New MongoDB Environment')
    string(name: 'tag', description: 'The tag to deploy: ex. 6.7.0', defaultValue: 'latest')
    string(name: 'dbname', description: 'The current mongodb to use (like: homer-squidex-staging or homer-squidex-staging-v2upgrade)', defaultValue: 'none')
    choice(name: 'cluster', choices: ['staging', 'production'], description: 'The Kubernetes Cluster to deploy to') //The names don't quite match but we handle it in a map
    choice(name: 'namespace', choices: ['content-v2', 'content-v1'], description: 'The environment to deploy squidex to')
    choice(name: 'logginglevel', choices: ['WARNING', 'INFORMATION', 'DEBUG'], description: 'Logging level to set Squidex to')
   }
  stages {
    stage('Checkout') {
      steps {
          script {
            environment = params.cluster
            origin = params.dbname
            tag = params.tag
            /* in production the 'content-v1' namespace is named 'squidex',
               so we are overriding it here                                 */
            namespace = params.namespace //Everything on the 2.0 clusters runs this way
            helm_data_file = "${environment}/${namespace}.yaml"
            squidex_version = tag.replaceAll("\\.","") //We need a DNS friendly value so need to remove dots
            println("The sanitized squidex tag is ${squidex_version}")
            cluster = env[environment]
            if(!params.create_db) {
              dbname = params.dbname
            }
          }
      }
    }

    stage('Create New Environment MongoDB') {
      when {
        expression { params.create_db }
      }
      steps {
        script{
          timeout(time: 40, unit:'MINUTES') {
            sh "./mongo_snapshot.sh ${environment} ${origin} ${squidex_version}"
            dbname = "squidex-${environment}-${squidex_version}"
          }
        }
      }
    }

    stage('Get Mongo URL') {
      steps {
        script {
            url = sh(returnStdout:true, script: "./mongo_url.sh ${dbname} ${environment}").trim()
            prefix="mongodb+srv://"
            mongo_url=url.substring(prefix.size())
        }
      }
    }

    stage('Render and validate the Kubernetes deployment yaml'){
      steps {
        script {
          replicas = 1 //Initially the deployment should just be a single pod
          mongoLogin = sh(returnStdout: true, script:"aws secretsmanager get-secret-value --secret-id squidex_mongo_build --query SecretString --output text --region=us-east-1 | jq -r '.\"${environment}_${namespace}_login\"'").trim()
          helmArgs = "--set loggingLevel=${params.logginglevel} --set replicaCount=${replicas} --set imageTag=${tag} --set version=${squidex_version} --set mongoconnectionstring=${mongoLogin} --set mongourl=${mongo_url}"
          homerKubernetes.processConfigData(helm_data_file, namespace, cluster, helmArgs, "./squidex")
        }
      }
    }


    stage('Deploy'){
      steps {
        script {
          def kubectlDockerArgs = " -v ${WORKSPACE}/output.yaml:/deployment.yaml"
          def kubectlArgs = "apply -f deployment.yaml -n ${namespace}"

          homerKubernetes.runKubectl(cluster, kubectlDockerArgs, kubectlArgs, "", "${cluster}.kops.learnwithhomer.com")
        }
      }
    }
  }
}
