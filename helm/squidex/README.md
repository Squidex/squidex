# Squidex Helm Deployment up to version 7 exlusive

Do not use this Helm chart for version 7 and above.

## TL;DR

```bash
$ helm install my-release squidex
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
$ helm install my-release squidex
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

| Name                      | Description                    | Value             |
| ------------------------- | ------------------------------ | ----------------- |
| `service.type`            | Kubernetes Service type        | `ClusterIP`       |
| `service.port`            | Kubernetes Service port        | `80`              |
| `deployment.replicaCount` | Number of instances.           | `1`               |
| `image.repository`        | Squidex image registry         | `squidex/squidex` |
| `image.tag`               | Squidex image tag              | `""`              |
| `image.pullPolicy`        | Squidex image pull policy      | `IfNotPresent`    |
| `ingress.enabled`         | True to deploy an ingress      | `true`            |
| `ingress.hostName`        | The host name for the ingress. | `squidex.local`   |


### Squidex parameters

| Name                                                 | Description                                                                                                      | Value                        |
| ---------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- | ---------------------------- |
| `env.EVENTSTORE__MONGODB__DATABASE`                  | The name of the database for events                                                                              | `Squidex`                    |
| `env.IDENTITY__ADMINEMAIL`                           | The initial admin email address.                                                                                 | `""`                         |
| `env.IDENTITY__ADMINPASSWORD`                        | The initial admin email address.                                                                                 | `""`                         |
| `env.IDENTITY__ADMINRECREATE`                        | Recreate the admin if it does not exist or the password does not match                                           | `false`                      |
| `env.IDENTITY__ALLOWPASSWORDAUTH`                    | Enable password auth. Set this to false if you want to disable local login, leaving only 3rd party login options | `true`                       |
| `env.IDENTITY__LOCKAUTOMATICALLY`                    | Lock new users automatically, the administrator must unlock them.                                                | `false`                      |
| `env.IDENTITY__PRIVACYURL`                           | The url to you privacy statements.                                                                               | `https://squidex.io/privacy` |
| `env.IDENTITY__SHOWPII`                              | Set to true to show PII (Personally Identifiable Information) in the logs                                        | `true`                       |
| `env.IDENTITY__GOOGLECLIENT`                         | Google client ID (keep empty to disable Google authentication).                                                  | `nil`                        |
| `env.IDENTITY__GOOGLESECRET`                         | Google client secret (keep empty to disable Google authentication).                                              | `nil`                        |
| `env.IDENTITY__GITHUBCLIENT`                         | Github client ID (keep empty to disable Github authentication).                                                  | `nil`                        |
| `env.IDENTITY__GITHUBSECRET`                         | Github client secret (keep empty to disable Github authentication).                                              | `nil`                        |
| `env.IDENTITY__MICROSOFTCLIENT`                      | Microsoft client ID (keep empty to disable Microsoft authentication).                                            | `nil`                        |
| `env.IDENTITY__MICROSOFTSECRET`                      | Microsoft client secret (keep empty to disable Microsoft authentication).                                        | `nil`                        |
| `env.IDENTITY__MICROSOFTTENANT`                      | Optional tenant name for Azure AD.                                                                               | `nil`                        |
| `env.IDENTITY__OIDCAUTHORITY`                        | The URL to the custom OIDC authority.                                                                            | `nil`                        |
| `env.IDENTITY__OIDCCLIENT`                           | The client ID to the authority.                                                                                  | `nil`                        |
| `env.IDENTITY__OIDCSECRET`                           | The client secret to the authority.                                                                              | `nil`                        |
| `env.IDENTITY__OIDCGETCLAIMSFROMUSERINFOENDPOINT`    | True to get claims from the user endpoint.                                                                       | `false`                      |
| `env.IDENTITY__OIDCMETADATAADDRESS`                  | A custom address for OIDC metadata.                                                                              | `nil`                        |
| `env.IDENTITY__OIDCNAME`                             | The name of the OIDC integration or server. Used in the UI                                                       | `nil`                        |
| `env.IDENTITY__OIDCRESPONSETYPE`                     | The type of the response. id_token or code.                                                                      | `nil`                        |
| `env.IDENTITY__OIDCSCOPES`                           | The scopes.                                                                                                      | `[]`                         |
| `env.IDENTITY__OIDCSINGOUTREDIRECTURL`               | The redirect URL for the sign out.                                                                               | `nil`                        |
| `env.LOGGING__APPLICATIONINSIGHTS__ENABLED`          | Enable monitoring via application insights.                                                                      | `falsen`                     |
| `env.LOGGING__APPLICATIONINSIGHTS__CONNECTIONSTRING` | The connection string to application insights.                                                                   | `nil`                        |
| `env.LOGGING__COLORS`                                | Use colors in the console output.                                                                                | `false`                      |
| `env.LOGGING__HUMAN`                                 | Setting the flag to true, enables well formatteds json logs.                                                     | `false`                      |
| `env.LOGGING__LEVEL`                                 | Trace, Debug, Information, Warning, Error, Fatal                                                                 | `INFORMATION`                |
| `env.LOGGING__LOGREQUESTS`                           | Set to false to disable logging of http requests.                                                                | `true`                       |
| `env.LOGGING__OTLP__ENABLED`                         | True, to enable OpenTelemetry Protocol integration                                                               | `false`                      |
| `env.LOGGING__OLTP__ENDPOINT`                        | The endpoint to the agent                                                                                        | `nil`                        |
| `env.LOGGING__STACKDRIVER__ENABLED`                  | True, to enable stackdriver integration.                                                                         | `false`                      |
| `env.LOGGING__STOREENABLED`                          | False to disable the log store for HTTP requests.                                                                | `true`                       |
| `env.LOGGING__STORERETENTIONINDAYS`                  | The number of days request log items will be stored                                                              | `90`                         |
| `env.ORLEANS__CLUSTERING`                            | Enables clustering via Orleans. Set to Development to turn it off.                                               | `MongoDB`                    |
| `env.STORE__MONGODB__DATABASE`                       | The name of the main database.                                                                                   | `Squidex`                    |
| `env.STORE__MONGODB__CONTENTDATABASE`                | The name of the database for content items.                                                                      | `SquidexContent`             |
| `env.URLS__BASEURL`                                  | Set the base url of your application, to generate correct urls in background process.                            | `https://squidex.local/`     |
| `env.URLS__ENFORCEHTTPS`                             | Set it to true to redirect the user from http to https permanently                                               | `false`                      |


### MongoDB parameters

| Name                                               | Description                                    | Value               |
| -------------------------------------------------- | ---------------------------------------------- | ------------------- |
| `mongodb-replicaset.enabled`                       | Uses the custom mongoDB instance.              | `true`              |
| `mongodb-replicaset.replicas`                      | The number of replicas.                        | `3`                 |
| `mongodb-replicaset.persistentVolume.enabled`      | If true, persistent volume claims are created. | `true`              |
| `mongodb-replicaset.persistentVolume.storageClass` | Persistent volume storage class.               | `""`                |
| `mongodb-replicaset.persistentVolume.accessModes`  | Persistent volume access modes.                | `["ReadWriteOnce"]` |
| `mongodb-replicaset.persistentVolume.size`         | Persistent volume size.                        | `10Gi`              |


Parameters are generated with: https://github.com/bitnami-labs/readme-generator-for-helm#configuration-file

## Support

Use the support forum to get help: https://support.squidex.io