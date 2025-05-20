# Api Tests

This project contains a API and load tests, written in xunit. Only the API tests are actually run in Github pipelines.

## How to run them

To run the API tests you have to run the backend first.

Squidex needs MongoDB by default. The docker-compose file can be found in this repository: https://github.com/Squidex/squidex-hosting/blob/master/development/docker-compose.yml


### Configuration

But before you can run the backend you have to make 2 adjustments to the app settings: 
https://github.com/Squidex/squidex/blob/master/backend/src/Squidex/appsettings.json

The recommendation is to either create an `appsettings.Development.json` file or to configure the following value with environment variables:

#### Client ID

* Key: `identity:adminClientId`
* Value: `root`
* Environment Variable: `IDENTITY__ADMINCLIENTID`

#### Client Secret

* Key: `identity:adminClientSecret`
* Value: `xeLd6jFxqbXJrfmNLlO2j1apagGGGSyZJhFnIuHp4I0=`
* Environment Variable: `IDENTITY__ADMINCLIENTSECRET`

#### Settings File

`appsettings.Development.json`
```json
{
    "identity": {
        "adminClientId": "root",
        "adminClientSecret": "xeLd6jFxqbXJrfmNLlO2j1apagGGGSyZJhFnIuHp4I0="
    }
}
```

### Run the backend

Just go to the backend folder and run the backend. The full instructions can be found under: https://docs.squidex.io/id-01-getting-started/contributing-and-developing/developing

### Run the tests

The tests are written with xunit. So you can just run them via your IDE or with

```
dotnet test
```