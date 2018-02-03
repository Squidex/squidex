# Changelog

## v1.1.3 - 2018-02-03

### Features

* **Rules**: Trigger when asset has changed
* **Rules**: Action to purge cache items in fastly
* **Rules**: Action to push events to Azure storage queues.

### Bugfixes

* **Rules**: Layout fixes.

### Refactorings

* Freeze action, triggers and field properties using Fody.
* Fetch derived types automatically for Swagger generation.

## v1.1.2 - 2018-01-31

### Features

* **Assets**: OData support, except full text search (`$search`)
* **Rules**: Slack action
* **Rules**: Algolia action

### Bugixes

* **Rules**: Color corrections for actions. 

### Breaking Changes

* Asset structure has changed: Migration will update the ocllection automatically.
* Asset endpoint:
    * `take` query parameter renamed to `$top`  for OData compatibility.
    * `skip` query parameter renamed to `$skip` for OData compatibility.
    * `query` query parameter replaced with OData. Use `$query=contains(fileName, 'MyQuery')` instead.