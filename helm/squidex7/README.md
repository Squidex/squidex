# Squidex Helm Deployment for version 7 and above

Do not use this Helm chart for version 6.X and lower.

## TL;DR

```bash
$ helm install my-release squidex7
```

## Introduction

This chart installs the following deployments:

* Squidex
* MongoDB
* Ingress

## Prerequisites

- Kubernetes 1.19+
- Helm 3.2.0+
- PV provisioner support in the underlying infrastructure

## Installing the Chart

To install the chart with the release name `my-release`:

```bash
$ helm install my-release squidex7
```

> **Tip**: List all releases using `helm list`

## Uninstalling the Chart

To uninstall/delete the `my-release` deployment:

```bash
$ helm delete my-release
```

The command removes all the Kubernetes components associated with the chart and deletes the release.

## Parameters

### Global parameters

| Name                                               | Description                                                          | Value             |
| -------------------------------------------------- | -------------------------------------------------------------------- | ----------------- |
| `nameOverride`                                     | Override the name of the application.                                | `squidex`         |
| `labels`                                           | Labels to add to the deployment.                                     | `{}`              |
| `service.type`                                     | Kubernetes Service type.                                             | `ClusterIP`       |
| `service.port`                                     | Kubernetes Service port.                                             | `8080`            |
| `deployment.replicaCount`                          | Number of replicas (ignored if autoscaling enabled).                 | `1`               |
| `deployment.revisionHistoryLimit`                  | Number of revision history.                                          | `2`               |
| `deployment.serviceAccountName`                    | Name of the service account to use.                                  | `""`              |
| `deployment.strategy.type`                         | Deployment strategy type.                                            | `RollingUpdate`   |
| `deployment.strategy.rollingUpdate.maxSurge`       | Maximum number of pods that can be created above the desired amount. | `1`               |
| `deployment.strategy.rollingUpdate.maxUnavailable` | Maximum number of unavailable pods during update.                    | `0`               |
| `deployment.restartPolicy`                         | Pod restart policy.                                                  | `Always`          |
| `deployment.annotations`                           | Annotations to add to the deployment.                                | `nil`             |
| `deployment.command`                               | Command to run in the container.                                     | `nil`             |
| `deployment.args`                                  | Arguments to pass to the container.                                  | `nil`             |
| `networkPolicy.enabled`                            | Enable network policies.                                             | `false`            |
| `image.repository`                                 | Squidex image registry.                                              | `squidex/squidex` |
| `image.pullPolicy`                                 | Squidex image pull policy.                                           | `IfNotPresent`    |
| `resources`                                        | Resource requests and limits.                                        | `{}`              |
| `topologySpreadConstraints`                        | Topology spread constraints for pod scheduling.                      | `[]`              |
| `priorityClassName`                                | Priority class name for the pod.                                     | `nil`             |
| `runAsNonRoot`                                     | Run container as non-root user.                                      | `true`            |
| `ingress.enabled`                                  | True to deploy an ingress.                                           | `true`            |
| `ingress.ingressClassName`                         | The ingress class.                                                   | `nginx`           |
| `ingress.annotations`                              | Ingress annotations.                                                 | `{}`              |
| `ingress.hostName`                                 | The host name for the ingress.                                       | `squidex.local`   |

### Squidex parameters

| Name                                                 | Description                                                                                                       | Value                        |
| ---------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- | ---------------------------- |
| `env.EVENTSTORE__MONGODB__DATABASE`                  | The name of the database for events.                                                                              | `Squidex`                    |
| `env.IDENTITY__ADMINEMAIL`                           | The initial admin email address.                                                                                  | `""`                         |
| `env.IDENTITY__ADMINPASSWORD`                        | The initial admin email address.                                                                                  | `""`                         |
| `env.IDENTITY__ADMINRECREATE`                        | Recreate the admin if it does not exist or the password does not match.                                           | `false`                      |
| `env.IDENTITY__ALLOWPASSWORDAUTH`                    | Enable password auth. Set this to false if you want to disable local login, leaving only 3rd party login options. | `true`                       |
| `env.IDENTITY__LOCKAUTOMATICALLY`                    | Lock new users automatically, the administrator must unlock them.                                                 | `false`                      |
| `env.IDENTITY__PRIVACYURL`                           | The url to you privacy statements.                                                                                | `https://squidex.io/privacy` |
| `env.IDENTITY__SHOWPII`                              | Set to true to show PII (Personally Identifiable Information) in the logs.                                        | `true`                       |
| `env.IDENTITY__GOOGLECLIENT`                         | Google client ID (keep empty to disable Google authentication).                                                   | `nil`                        |
| `env.IDENTITY__GOOGLESECRET`                         | Google client secret (keep empty to disable Google authentication).                                               | `nil`                        |
| `env.IDENTITY__GITHUBCLIENT`                         | Github client ID (keep empty to disable Github authentication).                                                   | `nil`                        |
| `env.IDENTITY__GITHUBSECRET`                         | Github client secret (keep empty to disable Github authentication).                                               | `nil`                        |
| `env.IDENTITY__MICROSOFTCLIENT`                      | Microsoft client ID (keep empty to disable Microsoft authentication).                                             | `nil`                        |
| `env.IDENTITY__MICROSOFTSECRET`                      | Microsoft client secret (keep empty to disable Microsoft authentication).                                         | `nil`                        |
| `env.IDENTITY__MICROSOFTTENANT`                      | Optional tenant name for Azure AD.                                                                                | `nil`                        |
| `env.IDENTITY__OIDCAUTHORITY`                        | The URL to the custom OIDC authority.                                                                             | `nil`                        |
| `env.IDENTITY__OIDCCLIENT`                           | The client ID to the authority.                                                                                   | `nil`                        |
| `env.IDENTITY__OIDCSECRET`                           | The client secret to the authority.                                                                               | `nil`                        |
| `env.IDENTITY__OIDCGETCLAIMSFROMUSERINFOENDPOINT`    | True to get claims from the user endpoint.                                                                        | `false`                      |
| `env.IDENTITY__OIDCMETADATAADDRESS`                  | A custom address for OIDC metadata.                                                                               | `nil`                        |
| `env.IDENTITY__OIDCNAME`                             | The name of the OIDC integration or server. Used in the UI.                                                       | `nil`                        |
| `env.IDENTITY__OIDCRESPONSETYPE`                     | The type of the response. id_token or code.                                                                       | `nil`                        |
| `env.IDENTITY__OIDCSCOPES`                           | The scopes.                                                                                                       | `[]`                         |
| `env.IDENTITY__OIDCSINGOUTREDIRECTURL`               | The redirect URL for the sign out.                                                                                | `nil`                        |
| `env.LOGGING__APPLICATIONINSIGHTS__ENABLED`          | Enable monitoring via application insights.                                                                       | `false`                      |
| `env.LOGGING__APPLICATIONINSIGHTS__CONNECTIONSTRING` | The connection string to application insights.                                                                    | `nil`                        |
| `env.LOGGING__COLORS`                                | Use colors in the console output.                                                                                 | `false`                      |
| `env.LOGGING__HUMAN`                                 | Setting the flag to true, enables well formatteds json logs.                                                      | `false`                      |
| `env.LOGGING__LEVEL`                                 | Trace, Debug, Information, Warning, Error, Fatal.                                                                 | `Warning`                    |
| `env.LOGGING__LOGREQUESTS`                           | Set to false to disable logging of http requests.                                                                 | `true`                       |
| `env.LOGGING__OTLP__ENABLED`                         | True, to enable OpenTelemetry Protocol integration.                                                               | `false`                      |
| `env.LOGGING__OLTP__ENDPOINT`                        | The endpoint to the agent.                                                                                        | `nil`                        |
| `env.LOGGING__STACKDRIVER__ENABLED`                  | True, to enable stackdriver integration.                                                                          | `false`                      |
| `env.LOGGING__STOREENABLED`                          | False to disable the log store for HTTP requests.                                                                 | `true`                       |
| `env.LOGGING__STORERETENTIONINDAYS`                  | The number of days request log items will be stored.                                                              | `90`                         |
| `env.STORE__MONGODB__DATABASE`                       | The name of the main database.                                                                                    | `Squidex`                    |
| `env.STORE__MONGODB__CONTENTDATABASE`                | The name of the database for content items.                                                                       | `SquidexContent`             |
| `env.URLS__BASEURL`                                  | Set the base url of your application, to generate correct urls in background process.                             | `https://squidex.local/`     |
| `env.URLS__ENFORCEHTTPS`                             | Set it to true to redirect the user from http to https permanently.                                               | `false`                      |
| `env.ASPNETCORE_URLS`                                | An override to ensure that kestrel starts on a non-privileged port.                                               | `http://+:8080`              |
| `autoscaling.enabled`                                | Enable autoscaling for the deployment.                                                                            | `false`                      |
| `autoscaling.maxReplicas`                            | Maximum number of replicas.                                                                                       | `6`                          |
| `autoscaling.minReplicas`                            | Minimum number of replicas.                                                                                       | `3`                          |
| `autoscaling.targetCPUUtilizationPercentage`         | Target CPU utilization percentage.                                                                                | `85`                         |
| `podDisruptionBudget.minAvailable`                   | Minimum number of available pods.                                                                                 | `1`                          |
| `podDisruptionBudget.unhealthyPodEvictionPolicy`     | Policy for evicting unhealthy pods.                                                                               | `AlwaysAllow`                |

### MongoDB parameters

| Name                               | Description                                                | Value               |
| ---------------------------------- | ---------------------------------------------------------- | ------------------- |
| `mongodb.architecture`             | MongoDB(®) architecture (standalone or replicaset).        | `replicaset`        |
| `mongodb.enabled`                  | Uses the custom mongoDB instance.                          | `true`              |
| `mongodb.replicaCount`             | The number of replicas.                                    | `3`                 |
| `mongodb.auth.enabled`             | Enable authentication for MongoDB.                         | `false`             |
| `mongodb.auth.rootUsername`        | The MongoDB root user name.                                | `""`                |
| `mongodb.auth.rootPassword`        | The MongoDB root password.                                 | `""`                |
| `mongodb.auth.existingSecret`      | The name of the existing secret to use for authentication. | `""`                |
| `mongodb.persistence.enabled`      | If true, persistent volume claims are created.             | `true`              |
| `mongodb.persistence.storageClass` | The storage class for the persistent volume claim.         | `""`                |
| `mongodb.persistence.accessModes`  | Persistent volume access modes.                            | `["ReadWriteOnce"]` |
| `mongodb.persistence.size`         | Persistent volume size.                                    | `10Gi`              |


Parameters are generated with: https://github.com/bitnami-labs/readme-generator-for-helm#configuration-file

## Support

Use the support forum to get help: https://support.squidex.io
