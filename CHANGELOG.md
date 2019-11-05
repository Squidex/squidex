# Changelog

## 3.5.0 - 2019-10-26

**NOTE**: This is the latest release with .NET Core 2.X. Next release will be 3.0 and above. Does not really matter when you use Docker.

### Features

* **Grain**: Fixed grain indices.
* **Content**: Multiple schemas allowed for references.
* **UI**: Inline stars editor.
* **UI**: Get rid of immutable arrays.
* **UI**: Hide date buttons based on settings.
* **UI**: Updated several packages.
* **UI**: Improvement to contributor page.
* **UI**: Better error indicating when saving content.
* **UI**: Warning when changing content status and you have pending changes.
* **UI**: Markdown support for Alerts and Dialogs.
* **UI**: Design improvements.
* **UI**: Custom "Forbidden" page when users access a page he is not allowed to instead of automatic logout.
* **UI**: Migration to angular CDK drag and drop to replace two drag and drop libraries.
* **UI**: Collapse or expand all array items.
* **Migration**: Better cancellation support for migration.
* **Rules**: Custom payload for Webhook and Algolia action.
* **Rules**: Optional names for rules when you have multiple rules with the same actions and triggers.
* **Rules**: Basic statistic summary per rule.
* **Rules**: Filter rule events by rule.
* **Rules**: Added exception details for Algolia.
* **Common**: New diacritic character for slug 

### Bugfixes

* **UI**: Fix references dropdown in query UI for localized values.
* **UI**: Fixed the unique checkbox in schema editor.
* **UI**: Fixed default date handling.
* **UI**: Fixed sorting of fields in schema synchronization endpoint.
* **UI**: Fixed preview button when multiple preview targets where configured.
* **UI**: Fixed TinymCE editor in arrays (Not recommended to use that!)
* **App**: Fix plan settings.
* **App**: Do not store default roles in the database so that we can change them later when new features are added.
* **Logging**: Use explicit thread for logging to console.
* **Logging**: Critical performance improvement.
* **Rules**: Fixed discourse action.

## 3.3.0 - 2019-09-08

### Features

* **UI**: Autosaving for content in local store.
* **UI**: Labels, descriptions and icons for contents.
* **UI**: Bulk import for contributors.
* **UI**: Pagination and search for contributors.
* **UI**: Improve file size for generated javascript bundles.
* **Rules**: Configurable default timeout for rule execution.
* **Assets**: Use asset url with slug when adding assets to rich text or markdown.
* **API**: Client per user.
* **API**: Limits for number of living content grains.

### Bugfixes

* **UI**: Fix for dynamic chunk loading.
* **UI**: Styling fixes for date editor.
* **UI**: Improvement and fixes for checking unsaving changes.
* **API**: Fixes hateaos links for nested schema fields.

## 3.2.2 - 2019-08-20

### Bugfixes

* **API**: Fixed a bug that prevented json response for field endpoint to be serialized correctly.
* **UI**: Layout fix for clients page.

## 3.2.1 - 2019-08-19

### Bugfixes

* **MongoDB**: Fix index creation for Orleans tables.
* **MongoDB**: Fixes in OpenAPI definition.

## 3.2.0 - 2019-08-19

### Features

* **Contents**: Improved reference dropdown selector.
* **API**: Json queries for new query editor.
* **API**: Moved from Swagger2 to OpenAPI for generated documentation.
* **API**: Improved GraphQL error handling.
* **API**: Setting to show PII (Personally Identifiable Information) in logs.
* **UI**: Query editor for json queries.
* **UI**: Horizontal scrolling in UI.
* **Assets**: Pass in time to cache to asset API.
* **Assets**: Shorter asset fields and asset migration.
* **Rules**: Kafka rule action, thanks to https://github.com/sauravvijay
* **MongoDB**: Removed support for CosmosDB and DocumentDB due to high costs.

## 3.1.0 - 2019-07-25

### Features

* **Contents**: Include reference fields to indicate which fields to show when a content item is referenced by another content.
* **Contents**: Resolve references for content list when max items is set to 1.
* **UI**: Scrollbars improved and designed.

### Bugfixes

* **UI**: Multiple fixes for modal dialogs.

## 3.0.0 - 2019-07-11

This version contains many major breaking changes. Please read: https://docs.squidex.io/next/02-api-compatibility

### Features

* **Contents**: Workflow system
* **API**: Hateoas
* **API**: Info endpoint
* **Configuration**: A lot of configuration settings to tweak some aspects of the UI.

## v3.0.0-beta2 - 2019-06-29

### Features

* **Contents**: Editor for custom workflows.

## v2.2.0 - 2019-06-29

### Features

* **Login**: Redirect to authentication provider automatically if only one provider is active.

### Bugfixes

* **GraphQL**: Fix a bug in styles that prevented to autocomplete to show properly

## v3.0.0-beta1 - 2019-06-24

This version contains many major breaking changes. Please read: https://docs.squidex.io/next/02-api-compatibility

## v2.1.0 - 2019-06-22

### Features

* **Assets**: Parameter to prevent download in Browser.
* **Assets**: FTP asset store.
* **GraphQL**: Logging for field resolvers
* **GraphQL**: Performance optimizations for asset fields and references with DataLoader.
* **MongoDB**: Performance optimizations.
* **MongoDB**: Support for AWS DocumentDB.
* **Schemas**: Separator field.
* **Schemas**: Setting to prevent duplicate references.
* **UI**: Improved styling of DateTime editor.
* **UI**: Custom Editors: Provide all values.
* **UI**: Custom Editors: Provide context with user information and auth token.
* **UI**: Filter by status.
* **UI**: Dropdown field for references.
* **Users**: Email notifications when contributors is added.

### Bugfixes

* **Contents**: Fix for scheduled publishing.
* **GraphQL**: Fix query parameters for assets.
* **GraphQL**: Fix for duplicate field names in GraphQL.
* **GraphQL**: Fix for invalid field names.
* **Plans**: Fix when plans reset and extra events.
* **UI**: Unify slugify in Frontend and Backend.

## v2.0.5 - 2019-04-21

### Features

* **UI**: Sort content by clicking on the table header.

### Bugfixes

* **UI**: Fix publish button in content context menu.

## v2.0.4 - 2019-04-20

### Features

* **UI**: Link to go from a referenced content to the corresponding edit screen.
* **Contents**: Also query by items in array fields.

You can use the following syntax for array items:

    $filter=data/iv/hobbies/name eq 'Programming'

## v2.0.3 - 2019-04-19

### Bugfixes

* **UI**: Serveral essential bugfixes for radio buttons in Angular forms.

## v2.0.2 - 2019-04-16

### Bugfixes

* **Fulltext**: Workaround for a clustering bug that prevented the text indexer to run properly in cluster mode.
* **Fulltext**: Big performance improvement for text indexer.
* **Identity-Server**: Use special callback path for internal odic to not interfere with external oidc and Orleans Dashboard.

## v2.0.1 - 2019-04-06

### Bugfixes

* **Assets**: Fix the naming of assets that has changed since last version.
* **Assets**: Fix when overriding assets that do now exists.
* **Contents**: Fixed a bug that made the text indexer crash when an content was published that had no text.

### Features

* **Assets**: Introduces slugs for assets and the option to query assets by slugs.
* **Assets**: Dialogs to edit slugs.
* **UI**: Ability to host Squidex in a virtual directory.

### Breaking Changes

* This release sets the clustering mode to 'Development' which means it is turned off. This makes operations simpler for most users.

## v2.0.0 - 2018-04-01

### Features

* **UI**: Automatic generation of UI for rule actions.
* **Contents**: Improved full text engine with `Lucene.NET`.
* **Server**: Plugin system.
* **Server**: Performance improvement for event handling.

The major feature of this release is the improved full text search. Content will be added to separate indices, which gives the following advantages:

* Each language is added to one field with individual stop words.
* Fuzzy search, e.g. `awsome~` to search for `awesome`.
* Search in one language only, e.g. `en:Home`

The full text index is populated in the background and it can therefore take a few seconds until you see the change. As ad admin you can restart the process in the admin section.

## v1.16.2 - 2019-03-16

### Bugfixes

* **UI**: Corrections for auto completions.
* **UI**: Correctly close onboarding tooltips.

## v1.16.1 - 2019-03-08

### Bugfixes

* **UI**: Multiple placeholders for interpolation.
* **UI**: Fix for button activation when adding rules.

## v1.16.0 - 2019-02-23

### Features

* **CLI**: New commands for schema synchronization.
* **UI**: Imroved validation messages.
* **UI**: Integrate CLI documentation to client UI.
* **UI**: Side by side view for content differences.
* **UI**: Drag and drop assets to markdown editor.
* **UI**: Drag and drop assets to rich text editor.
* **UI**: Copy assets from clipboard to asset views.
* **UI**: Copy assets from clipboard to markdown editor.
* **UI**: Copy assets from clipboard to rich text editor.
* **UI**: Performance improvements and refactoring of components.
* **Schemas**: New endpoint to synchronize schemas.
* **Server**: Log all requests for cloud version and provide endpoint to download logs.
* **Server**: Improved logging for http requests.
* **Rules**: Generate event and trigger when the app consumed almost all resources.

### Bugfixes

(Mostly due to UI refactoring :( )

* **UI**: Fixed custom editors.
* **UI**: Fixed disable state of restore button.
* **UI**: Fixes for addition button states.

## v1.15.0 - 2019-01-05

### Features

* **Rules**: Javascript conditions for rule triggers.
* **Rules**: Javascript formatting for rule actions.

## v1.14.0 - 2018-12-23

### Features

* **CLI**: Basic setup
* **CLI**: Export all Content
* **UI**: Edit asset tags and names in asset field.
* **UI**: Preview for SVG assets.
* **UI**: No stacked bars for API performance chart and a checkbox to toggle between stacked and non-stacked bars.
* **Users**: Invite users to an app even if they do not have an account yet.
* **Users**: Github authentication.
* **Client Library**: Integrated autogenerated management library.
* **Contents**: Preview urls for schemas.
* **Contents**: Button  to show all input fields for localized fields.
* **Scripting**: Access to user roles.

### Bugfixes

* **API**: Several bugfixes for the JSON API and Swagger
* **UI**: Fixed dependencies and sortable lists.
* **UI**: Fixed disabled state for custom field editors.
* **Permissions**: Fixed duplicate permissions.

### Refactorings

* *Improved build performance for the Frontend.
* *Migration to Angular7

## v1.13.0 - 2018-12-08

### Features

* **Contents**: Uniqueness validator.
* **Swagger**: Show needed permission in swagger definition.
* **UI**: Array fields: Clone items.
* **UI**: Array fields: Collapsed all items to make sorting measier.
* **UI**: Array fields: Buttons for sorting.
* **UI**: Drag indicators for drag and drop lists.
* **General**: New fine grained permission system.
* **General**: Custom roles.
* **Rules**: Cancel queued events.
* **Rules**: Publication id for medium action.

### Bugfixes

* **Backup**: Always make  the user, who restored an backup an owner.
* **Contents**: Implemented $search for Array fields.
* **UI**: Fixed for Edge browser.

### Refactorings

* Migration to `.NET Core 2.2`
* Migration to `ASP.NET Core 2.2`
* Migration to `ASP.NET Core` Health System
* Performance improvements in logging system.
* Performance improvements for json serialization.
* Unificiation of storage implementations.
* Custom JSON structures to replace `JSON.NET` with faster serializer at later point of time.

## v1.12.0 - 2018-11-06

### Features

* **Contents**: Comments for content items.
* **Contents**: Specify limits for max number of assets via options.
* **Assets**: Specify limits for max number of assets via options
* **UI**: List view for assets.
* **UI**: Reorder contents in references fields.

### Bugfixes

* **GraphQL**: Fix languages with region support.

Various small bugfixes for UI and API.

## v1.11.0 - 2018-09-24

### Features

* **API**: Correct handling of `If-None-Match` header to support caching.
* **Rules**: Major refactoring of action handlers to add new actions with less code.
* **Rules**: Twitter action to post status update.
* **Rules**: Prerender.io action to invalidate cache entries for SPA sites.
* **Contents**: Support IN-queries, like `fileName in ['Logo.jpg', 'Logo.png']`
* **UI**: Cloning content items.
* **UI**: Tag input in header to filter by tags when assigning assets to content.
* **Schemas**: Color picker as additional editor for string fields.
* **Statistics**: Report api usage and performance per client.

### Bugfixes

* **Clustering / Orleans**: Fixed correct serialization of exceptions, e.g. as validation errors.
* **Backups**: Always assign the user who started the restore operation as Owner to the app.
* **UI**: Reset name when a asset or content query is saved.
* **UI**: Disable spellchecking for tag editor.

## v1.10.0 - 2018-08-29

### Featues

* **Contents**: Introduce `X-Unpublished` header to also get unpublished content.
* **UI**: General feature to store UI settings.
* **UI**: Save content queries.
* **UI**: Save assets queries.
* **GraphQL**: Endpoint to run multiple queries in parallel with a single request.

## v1.9.0 - 2018-08-19

### Features

* **Scripting**: Override for the slugify method to use single line characters when replacing diacritics.
* **Docker**: Smaller image size.

## v1.8.0 - 2018-06-30

### Features

* **Schemas**: Singleton schemas (can only have single content)

### Bugfixes

* **UI**: Nested fields got wrong ids and names and could not be saved.
* **Contents**: Ensure that the content api returns content in correct order when querying by ids.

## v1.7.0 - 2018-06-25

* Migration to .NET Core 2.1

## v1.6.2 - 2018-06-23

### Features

* **UI**: Better sortable with improved UX.
* **Migration**: Increased performance.
* **Migration**: Disable event handlers during migration.

### Bugfixes

* **Schemas**: Invariant name handling for field names.

## v1.6.1 - 2018-06-22

### Bugfixes

* **MongoDB**: Fixed date time handling.

## v1.6.0 - 2018-06-07

### Features

* **Schemas**: Nested Schemas.
* **UI**: Migration to RxJS6.
* **UI**: Migration to Angular6.

## v1.5.0 - 2018-05-20

### Bugfixes

* **UI**: Fixed the pattern selector in field editor.

### Features

* **Contents**: Allow to save content updates as draft.
* **Schemas**: Create folders to group schemas.
* **UI**: Increased the search input.
* **UI**: Plugin system for content editors.

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
* **Contents**: Updated to state can be scheduled, e.g. to publish them.

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

* **Contents**: Slugify function for custom scripts.

### Bugfixes

* **Migration**: Assets and schemas were not removed before recreation.
* **Contents**: OData queries only worked for data fields.
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