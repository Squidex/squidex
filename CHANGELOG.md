# Changelog

## v1.4.1 - 2018-05-02

### Bugfixes

* **Orleans**: Remove orleans dashboard from 8080.

## v1.4.0 - 2018-05-02

### Features

* **UI**: Big refactorings and UI improvements.
* **Actions**: New log formatter with placeholder for user infos.
* **Actions**: Azure Queue action.
* **Actions**: Algolia action.
* **Actions**: Fastly action.
* **Backup**: Backup all your data to an archive.

## v1.3.0 - 2018-02-17

### Features

* **Actions**: ElasticSearch action.

### Refactorings

* **DomainObjects**: Refactored domain objects to be ready for Orleans.

## v1.2.0 - 2018-02-10

### Features

* **EventStore**: Event metadata are stored as json objects in MongoDB now and you cacn query by metadata.
* **Content**: Updated to state can be scheduled, e.g. to publish them.

> This releases will run a migration, which might take a while and also effects the events. We recommend to make a backup first.

## v1.1.7 - 2018-02-06

### Bugfixes

* **UI**: Checkbox style fixed.

## v1.1.6 - 2018-02-06

### Features

* **Rules**: Allow content triggers to catch all content events.
* **Rules**: Ensure that the events for an aggregate are handled sequentially.
* **UI**: History stream in the dashboard.
* **UI**: Better UI for apps overview.
* **Apps**: Added a ready to use blog sample.

### Bugfixes

* **UI**: History UI was throwing an exception when a user was referenced in the message.
* **UI**: A lot of style fixes. 

## v1.1.5 - 2018-02-03

### Features

* **Content**: Slugify function for custom scripts.

### Bugfixes

* **Migration**: Assets and schemas were not removed before recreation.
* **Content**: OData queries only worked for data fields.
* **Assets**: OData queries did not work at all and included too many fields (e.g. AppId, Id).

## v1.1.4 - 2018-02-03

### Features

* **Login**: Consent screen to inform the user about privacy policies.

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