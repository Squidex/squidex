# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [6.0.0] - 2021-10-14

### Fixed

* **UI**: Improve autosave feature.
* **UI**: Correct styling of content compare view.
* **UI**: Fix for reference dropdown.
* **MongoDB**: Fixed parsing of MongoDB versions for release candidates or beta versions.

### Changed

* **GraphQL**: Do not apply page size when retrieving references to get rid of limitation.
* **API**: New content structure to allow full app deletion.
* **Schemas**: Simplify schema builder.

### Added

* **Assets**: Asset scripts.
* **GraphQL**: Do not return error when content to query is from another schema.
* **Contents**: Uniqueness validation for components and array fields.
* **UI**: Persist collapsed status for array fields in UI.
* **UI**: Hint to run webpack for development mode.

## [5.9.0] - 2021-09-14

### Fixed

* **Contents**: Fixed field rules to also work with components.
* **Notifo**: Improved error handling for Notifo integration.
* **Hosting**: Several fixes to improve URL resolution when Squidex is hosted in a subfolder.

### Changed

* **Assets**: Improved asset folder dropdown.
* **Assets**: Improved and optimized queries for asset folders.
* **Event Processing**: Migration to channels.
* **Contents**: Improved workflow handling for `Save and Publish`.
* **Contents**: Resolve component types with `schemaName` field in the component object.
* **Contents**: Added markdown support to all field hints.
* **Monitoring**: Migration to Open Telemetry traces.
* **Rules**: Improved rule simulator.
* **UI**: New and better design.

### Added

* **Assets**: New metadata provider for azure image recognition.
* **Clustering**: Added support for Kubernetes.
* **Contents**: New tab to inspect the content item and view the structure as pure JSON.
* **Contents**: New calendar view for scheduled content items.
* **Contents**: Added a button to the UI to cancel content scheduling.
* **Contents**: New validation properties for videos.
* **Contents**: Added a text field to the content editor to define a custom ID when creating content items.
* **GraphQL**: Exposed the `newStatus` field in GraphQL.
* **GraphQL**: Exposed the `newStatusColor` field in GraphQL.
* **Logging**: Added more information to the request log.
* **Rules**: New action to trigger SignalR.
* **Rules**: Enable or disable rules over the context menu.
* **Scripting**: New method to make HTTP methods using POST, PUT, PATCH and DELETE verbs.

## [5.8.2] - 2021-07-18

### Fixed

* **GraphQL**: Urgent bugfix for references and components. Please upgrade immediately from 5.8.1.

## [5.8.1] - 2021-07-17

### Fixed

* **API**: Several bugfixes for components, especially around caching of components.
* **GraphQL**: Several bufixes for components.
* **UI**: Several bugfixes for components.
* **UI**: Several fixes to fontsize.
* **UI**: Virtual scrolling for nested fields with a lot of items (> 50) to improve performance.

### Added

* **Assets**: Check SVG files for embedded scripts and block uploads.
* **Assets**: Option to create one folder per app for uploaded assets.
* **Rules**: Add `newStatus` to content changed event.
* **UI**: Define your custom date or date-time format for Date fields.

### Changed

* **API**: Support hosting without https (not recommended).
* **UI**: Load outdated-browser from local files instead of CDN.
* **UI**: Load leaflet from local files instead of CDN.
* **UI**: Load leaflet geocoder from local files instead of CDN.
* **UI**: Load Orleans Dashboard assets from local files instead of CDN.

## [5.8.0] - 2021-06-28

### Fixed

* **API**: Fixes for anonymous write access.
* **API**: Fixes for client access when creating apps.
* **Schemas**: Correct sync of field rules, especially for CLI.
* **UserManagement**: Fix pagination
* **UI**: Encode IDs to allow custom content IDs with slash.
* **UI**: Fixes typos for italian translation.
* **UI**: Allow scrolling when content is disabled.
* **UI**: Fixes references/referencing view for localized content.
* **UI**: Fix confirm click.

### Added

* **API**: Better timeout and cancellation handling.
* **API**: Default timeouts for most important MongoDB calls.
* **API**: Better API tests to improve stability.
* **Assets**: Additional configuration flag to allow one folder per asset.
* **Contents**: Read published contents from secondary MongodB instances for better load distribution.
* **Contents**: Better indexes for improved performance.
* **Rules**: New liquid and javascript extensions to read the asset as text in rules.
* **Rules**: Simpler syntax to resolve assets and contents in liquid templates.
* **Contents**: Array builder when building custom code extension.
* **UI**: Show SVG as images when in contnet overview.
* **UI**: Chinese translation.

## [5.7.1] - 2021-05-21

### Fixed

- **Events**: Read the correct amount of events with event store.

## [5.7.0] - 2021-05-07

### Added
 
- **Assets**: Delete assets permanently.
- **GraphQL**: Updrade to GraphQL.NET 4 for better performance.
- **GraphQL**: Provide detailed user information for contents and assets.
- **Infrastructure**: New option to disable the request log.
- **Infrastructure**: Update of NSWAG to reduce memory footprint.
- **Performance**: Performance optimization for batch operations.
- **Performance**: Performance improvements for restore operations, recovery and repair.
- **Rules**: New rule simulator to test how a rule would have behaved for the latest 100 events within the last 7 days.
- **Rules**: Better rule editor for scripts and templates including intellisense code formatting.
- **Rules**: Http DELETE for webhooks.
- **Scripting**: More variables for scripts.
- **Scripting**: New functions to hash strings with SHA256 and MD5.
- **UI**: Easy cloning of references.
- **UI**: New fullscreen view for rules.
- **UI**: Show confirm and alert dialogs in UI plugins.
- **UI**: Show asset selector in UI plugins.
- **UI**: Show saved queries i nsearch dialog.

### Fixed 

- **OpenAPI**: Fix several endpoint descriptions.
- **Contents**: Correct validation for PUT requests.
- **GraphQL**: Fix GraphQL for empty nested fields.
- **UI**: Url decode role names in API endpoint.
- **UI**: Fix several error handling bugs, especially around content updates.

## [5.6.0] - 2021-02-27

### Added

- **API**: Endpoint to provide JSON schemas for all rule events.
- **API**: Better support for OpenAPI code generators.
- **API**: Fix total calculation in same hot paths.
- **API**: Performance improvements to the GraphQL endpoint.
- **API**: Performance improvements to reduce memory usage.
- **Contents**: Pujabi as new language.
- **Contents**: New scripting methods: `getReferences` and `getAssets`
- **Contents**: New flat data API description.
- **Infrastructure**: Simplified serializers to migrate to System.Text.Json when possible.
- **Permissions**: New permissions to only give access to contents created by the user.
- **Rules**: Custom payloads for AuzureQueue rules.
- **UI**: Editor extensions.
- **UI**: Delay tooltip a little bit.
- **UI**: Drag and drop of multiple files and folders.
- **UI**: Simply autocompletion for scripting.

### Fixed 

- **Assets**: Fix surrogate keys for asset endpoint.
- **Contents**: Null fixesd for Odata.
- **UI**: Show all properties when exporting schemas.
- **UI**: Shortcut for new asset folder fixed.
- **UI**: Max height for language dropdown.
- **UI**: Fix custom editor in nested forms.
- **UI**: Fix reset button for assets.
- **Usages**: Fix usage notifications.

## [5.5.0] - 2021-01-24

### Added

- **Admin**: Better user management in the backend.
- **Admin**: Possible to delete users now.
- **Backend**: Reduce memory allocations in the backend.
- **Backend**: Better tests for replicated caches.
- **Notifo**: Update to newer notifo REST service.
- **Contents**: Geolocation fields also accept GeoJson objects now.
- **Contents**: Support for Geo queries with the full text index. You need to rerun the full text indexer for old geolocation fields.
- **Contents**: Reference specific content version in GraphQL endpoint.
- **UI**: Change the default of date editors to local mode instead of UTC.
- **UI**: Better support for SVG images in the rich text and markdown editor.
- **UI**: Integrated video player for assets.
- **UI**: Integrated document viewer for documents like Word and PowerPoint.
- **UI**: Fixed default value handling.
- **UI**: Better sorting of schemas in custom Roles.

### Fixed 

- **Assets**: Fixed a bug where the wrong version was used when referencing assets.
- **Assets**: Fixed a critical bug in the backend to not deliver protected assets when an old version without that protection was queried.
- **UI**: Fix quick nav after migration to newer Angular version.
- **UI**: Fixed a layer in the OpenStreetMap editor to use https instead of http.
- **UI**: Fixed array sorting in the UI.
- **UI**: Fixed a bug that was causing the folders not to show up properly.
- **Identity**: Fix the config management for external OIDC providers.
- **Events**: Fix event consumers for older mongo installations (< 4.0 ).

## [5.4.0] - 2020-12-28

### Added

- **API**: Update to Net5.
- **API**: Migrate to new Notifo API.
- **Contents**: More default values for other field types like references and assets.
- **Contents**: Optimize DB calls.
- **Contents**: Optionally validate during publish operation.
- **Contents**: Ensure that sorting is always consistent.
- **Contents**: Check referrers before unpulishing a content item.
- **Contents**: More support for bulk actions.
- **Contents**: Update multiple content items in a bulk operation.
- **Rules**: Run rules from snapshots.
- **Translation**: Also support Google Translate.
- **UI**: Update to Angular 11.
- **UI**: Rich dropdown for users when creating content filters.
- **UI**: Quick links to asset folder in asset overview.
- **UI**: Show asset path in asset dialog.
- **UI**: Localize datepicker.
- **UI**: News views to show all references contents and referencing content items for a single content.
- **UI**: Allow to translate all string fields.

### Fixed 

- **Assets**: Fixes parent id (folder id) after migration from 4.X.
- **Assets**: Fixed a bug in usage tracking of asset downloads that was causing it to never decrease.
- **Contents**: GraphQL fallback handling when schema is called `content`.
- **Rules**: Fix rerun logic.
- **Hosting**: Fixes a few problems around hosting and header forwarding (e.g. `X-Proto-For`).
- **UI**: Several drag and drop bugfixes for angular CDK.

## [5.3.0] - 2020-10-30

### Changed

This changes reverts the changes from `5.1.0` and uses a normal handling of forwarded headers again.

Futhermore it fixes some issues with the migration from 4.X versions. You have to run the migrations agains as explained here: https://docs.squidex.io/01-getting-started/installation/troubleshooting-and-support#my-migration-is-broken

### Added

- **Assets**: Upload assets by dropping a folder to the drop area.
- **Assets**: Introduce a new option to fix broken asset files.
- **Backups**: Ignore missing asset files during backup and restore.
- **GraphQL**: Limit the number of parallel requests in GraphQL to keep the load on MongoDB low.
- **GraphQL**: Resolve reverse references in GraphQL.
- **Rules**: Log exceptions in rule handlers (actions).
- **UI**: Provide access to the current language in field editors.

### Fixed 

- **Assets**: Fix parent id for folders. See above.
- **UI**: Several layout fixes in the UI.
- **UI**: Several UI language fixes.

## [4.7.6] - 2020-10-30

### Added

- **Assets**: Upload assets by dropping a folder to the drop area.
- **GraphQL**: Limit the number of parallel requests in GraphQL to keep the load on MongoDB low.
- **GraphQL**: Resolve reverse references in GraphQL.
- **Rules**: Log exceptions in rule handlers (actions).
- **UI**: Provide access to the current language in field editors.

### Fixed 

- **UI**: Several layout fixes in the UI.
- **UI**: Several UI language fixes.

## [5.1.0] - 2020-10-20

### Changed

This version introduces a few small breaking changes. The support for https redirects and `X-FORWARDED-*` headers has been removed. Squidex does not provide a solution for https and it is the responsibility of the reverse proxy like nginx, caddy, IIS or Cloudflare to terminate https requests. Therefore it does not make sense to care about https redirects anyway. The support for `X-FORWARDED-*` headers has been removed because it was possible to solve it with the mandatory `urls:baseUrls` setting in an easier way.

This version also comes with new docker image versioning, each version is now tagged with the concrete version and the major version only, e.g.

- squidex/squidex:5
- squidex/squidex:5.1

Furthermore the docker-compose files are updated with a simpler approach to use [caddy](https://caddyserver.com/) as a reverse proxy for https termination:

https://github.com/Squidex/squidex-docker/blob/master/standalone/docker-compose.yml


### Added

- **Backup**: Check version compatibility.
- **Backup**: Adjust asset urls in strings to new app.
- **Full Text**: Batching operations for Elastic Search
- **Full Text**: Search in field with Elastic Search
- **Full Text**: Search lazy with Elastic Search
- **UI**: Leave an app.
- **UI**: Fullscreen mode for custom field editors.
- **UI**: Language toggle to switch UI language.
- **UI**: Half width fields for field editors.
- **UI**: Make the schema selector dropdown more visible.
- **UI**: Markdown support for field labels.
- **Server**: Remove support for https redirects and X-FORWARDED headers.

### Fixed 

- **UI**: Fixed the route synchronizer.
- **UI**: Fix a few spelling errors.
- **UI**: Fixes checkboxes for custom roles.
- **Assets**: Fix asset urls in GraphQL.

## [5.0.0] - 2020-10-08

This feature adds custom IDs to the system. So far every ID like a content ID is globally unique. This means that you cannot create custom IDs and when a backup is restored you need to assign new ids, because the old IDs might be already in use.

With this version on, every ID is prefixed with the app id, meaning they are only unique within an app. This allows new endpoints for upserting contents and defininig custom ids for assets.

### Deprecated

This version has to migrate a few mongo collections:

- Events
- States_AssetFolders
- States_Assets
- State_Contents_All
- State_Contents_Published

This process will start automatically and can take a while. To be backwards compatible when you experience a bug, new collections are created. The new collection names are

- Events2
- States_AssetFolders2
- States_Assets2
- States_Contents_All2
- States_Contents_Published2

(As you can see the collection names have been streamlined.)

If everything works fine for you, you can delete the old collections.

## [4.7.0] - 2020-09-29

### Added

- **Assets**: Do not cache protected assets when user gets a 403.
- **Assets**: Reduce the number of threads that resize images to improve performance.
- **Assets**: New query string properties to change the format of assets, e.g. from PNG to JPG.
- **Content**: New full text index based on MongoDB.
- **Contents**: Better exception handling in validation.
- **Events**: New event consumers with support for batching. Can process up to 10.000 events / second.
- **Events**: Count the number of processed events to get an understanding of performance.
- **GraphQL**: GraphQL Mutations.
- **General**: Better default configuration.
- **Roles**: Role properties to customize the UI.
- **Rules**: New functions for rules.
- **Rules**: Enable algolia and ElasticSearch rule actions for all events.
- **UI**: Sidebar plugin for contents and content items.
- **UI**: Disable add button for array field when max items is reached.
- **UI**: Italian translation.
- **UI**: Dutch translation.

### Bugixes

- **Backup**: Fix memory usage when downloading backups.
- **Clients**: Fix anonymous access clients.
- **Contents**: Fixes to the angular forms to revert a performance improvements.
- **Contents**: Fixes to empty filter.
- **UI**: Fix confirm button.

## [5.0.0 BETA 2] - 2020-09-02

Includes new features in 4.6.0

## [4.6.0] - 2020-09-02

### Added

- **API**: Client contigent or API calls to protected your API.
- **API**: Special headers to simulate errors in the UI.
- **API**: Allow anonymous access per client.
- **Assets**: Better fallback handling when resizing assets failed.
- **Assets**: Autorotate images with orientation metadata.
- **Contents**: Better word count implementation which also works for CJK languages. (Chinese, Japanese, Korean).
- **Contents**: Text based validation for string fields. You can define the content type now and the validator will extract the plain text from html and markdown to make character or word count validation.
- **Contents**: Filter content by `newStatus` field.
- **Contents / Schemas**: Field rules to disable or hide fields based on conditions.
- **Workflows**: Visualization of the workflow with a readonly diagram.
- **UI**: Button per field to unset a field value.
- **UI**: Custom editors for all field types.
- **UI**: Confirm dialog when removing assets or references.
- **UI**: Open referenced content in new tab.
- **UI**: Show current traffic usage in Dashboard.
- **UI**: Toggle between locale and UTC mode for datetime editors.
- **UI / General**: Support for localized UI and backend with support for `en` and `nl` for now. Italian is coming as well.
- **Performance**: Replicated cache for some high load scenarios.


### Fixed 

- **Contributors**: Ignore casing of email addresses, which was causing Squidex not to invite people with uppercase characters in email addresses.
- **Contents**: Improved content scheduler to handle contents better, when the app or schema has already been deleted.
- **Contents**: Fixes for authentication and GraphQl GET Endpoint.
- **Contents**: Improve javascript error handling for schemas scripts. Some exceptions have been swallowed before.
- **EventSourcing**: Fixed a bug for the event consumer, which was skipping some events in high load scenarios. Need a replica set and Mongo 4.2. to work properly.
- **Algolia**: Fixed Algolia rule action.

## [5.0.0 BETA 1] - 2020-07-06

This version introduces a new way to deal with ids. So far each content element has an id that is unique across all apps. This causes problems, because you cannot define your own ids and the ids have to change when you clone an app via backup and restore.

With this version ids for content items and assets are only unique within an app.

To make this possible, this version rebuilds all content items, assets and asset folders which can take a few minutes.

## [4.5.0] - 2020-07-06

### Added

- **Backups**: Increase download timeout for backups to 60 minutes.
- **Rules**: Change expiration for rule events to be relative from now instead of relative to the original event to make replaying easier.
- **Rules**: Also create rule events when the creation failed to simplify debugging.
- **UI**: CTRL+Click for content items to open them in new tabs. Mimic default browser behavior for links.
- **UI**: Make sections defined by schema separators collapsible.
- **UI**: Customizable dashboard.
- **UI**: Include external dependencies into the build to run Squidex in protected company networks.
- **UI**: Define the preview modes for assets.
- **Notifo**: Notifo integration finalized.

### Fixed 

- **Assets**: Fixed a bug where the wrong permission was checked for protected assets.
- **Assets**: Fixed the wrong calculation of focus points when resizing assets.
- **Assets**: Upraded the image library to a newer version to fix a bug with resizing.
- **Contents**: Minor fixes for flat content.
- **Rules**: Fixed several bugs in the rule runner.
- **Rules**: Fixed avro serialize for union schemas.
- **Rules**: Proper cancellation for kafka.
- **Schemas**: Fixed several minor bugs in the schema synchronizer.

## [4.4.0] - 2020-06-15

### Added

- **Rules**: Liquid support.

### Fixed 

- **Grains**: Fix restart of grains
- **EventStore**: Fix a bug where very old events were not consumed properly.

## [4.4.0 RC] - 2020-05-30

### General

- Many improvements to tests and integration of API tests into CI pipeline.

### Added 

- **Rules**: Defined payload, headers and key in kafka rule action.
- **Rules**: Support for avro serialization in kafka rule action.
- **Rules**: Define fallback values in formatting, e.g. `${CONTENT_DATA.name.iv ? Fallback}`
- **Rules**: Define transformations, e.g. `${CONTENT_DATA.name.iv | upper}`: Upper, Lower, Slugify, EncodeJson, Timestamp_Ms, Timestamp_Seconds
- **Rules**: Resolve reference in formatting, e.g. `${CONTENT_DATA.city.iv.data.name.iv}`
- **Contents**: Use aggregation framework to order large data sets.
- **Contents**: Improvements to the bulk endpoint to also allow deletion and updates.
- **Contents**: Improvements to the enrichment flow of contents when they are queried from the database.
- **Scripts**: Fallback for `oldStatus`.
- **Clustering**: Auto restart background processes.
- **Authentication**: Use local API authentication to bypass the extra call to identity server and to make deployment easier.
- **Authentication**: Improve performance when checking permissions by simple caching.
- **Amazon S3**: Allow to upload assets where the stream has no length.
- **UI**: Additional editor to use checkboxes for references.

### Fixed 

- **Assets**: Fixed a bug where deleting folders using the UI was not working properly.
- **Contents**: Use aggregation framework to order large data sets.
- **Contents**: Fixed a bug where references were cleared in some conditions.
- **UI**: Do not show first value in dropdowns when no value is defined.
- **UI**: Fixes for notifications and show newest notifications first.
- **UI**: Fixed the layout of asset preview in content list.
- **Authentication**: Fixed a bug where invited collaborators were not added to an app correctly.
- **Schemas**: Fix for schema synchronizer were some changes were not discovered correctly.

## [4.3.0] - 2020-04-27

### Added

- **API**: Dedicated health check for event consumers and background processes.
- **Rules**: Integrated a background worker to start rules from beginning.
- **Users**: Custom user properties.
- **Scripting**: Incrementing counters.

### Fixed 

- **API**: Fix in OpenAPI schema to get rid of FieldNames collection that causes problems in code generators.
- **API**: Short header for surrogate keys and custom request header to turn off keys.
- **API**: Better error handling for unsupported ODATA features.
- **UI**: Fix in autocompletion component which was causing issues in role form.
- **UI**: Fixed a layout bug in the role form.
- **UI**: Fixed a layout bug in tag editor.
- **UI**: Time formatting fixed.
- **UI**: Fixed a bug that was showing all assets and not in their folders.
- **API**: Fixed index usage for event store.
- **FullText**: Fixed a small minor in full text index.
- **Rules**: Fixed a bug in email rule which was using email body as sender and recipient address.
- **Rules**: Use default timeout in webhook.

## [4.2.0 Beta 2] - 2020-02-24

This release just contains a lot of bugfixes.

## [4.2.0 Beta 1] - 2020-02-20

The release makes a lot of changed to the content structure, therefore it will run a migration to recreate the contents collections. This can take a while.

### Added

- **UI**: Global search
- **Contents**: Full text search also includes references items.
- **Contents**: Alignment of workflows.
- **Contents**: Improvements to full text index for later support of elastic search.

## [4.1.3] - 2020-02-20

### Fixed 

- **UI**: Several fixes due to wrong references of SCSS mixins.

### Added

- **Assets**: Option to turn on the recursive deleter.

## [4.1.2] - 2020-02-19

### Fixed 

- **UI**: Fix to show all controls for localized assets.
- **UI**: Fix for sorting content when clicking on table headers.
- **UI**: Fixed disable state in tag editor.
- **UI**: Fixed layout issues with modal editor.

### Added

- **Clustering**: Configuration option to define the IP Address manually.
- **UI**: Migration to Angular9 and Ivy.

## [4.1.0] - 2020-02-11

### Fixed 

- **Assets**: Resizing of images when only or height is defined.
- **Contents**: Fixes with nullable property in OpenApi.
- **UI**: Fixes with valueChanges in editors.

## [4.1.0 RC] - 2020-02-05

### Added

- **Assets**: Support for focus points in UI and API.
- **Assets**: Integrated editor / cropper for basic image editing.
- **Contents**: Better and more consistent content cleanup and enrichment.

### Fixed 

- **API**: Faster equals checks.
- **API**: Fixed a critical bug that caused an infinite loop and Out of Memory.
- **API**: Many small bugfixes.

## [4.1.0 Beta 1] - 2020-01-17

### Added

- **Assets**: Folders to organized your assets.
- **Assets**: Asset metadata with built in editor in the management UI.
- **Assets**: Better detection of asset type, including videos and extracting of more metadata.
- **Assets**: Protect assets.
- **Assets**: Amazon S3 support for assets.
- **Assets**: Dedicated permission to upload new version of asset.
- **GraphQL**: Flat data to provide content with default language rules.
- **Logging**: Increased log levels and performance improvements.
- **Logging**: Store request logs in MongoDB for fast download (also in hosted version).
- **Geolocation**: Search by location in OpenStreetMap-Editor.
- **Geolocation**: General UX approvements for Editor.
- **Comments**: Mention other contributors by email address.
- **Comments**: Notification when you get mentioned.
- **Comments**: Markdown support.
- **Comments**: Rule action to create comments for content items.
- **Comments**: Trigger to handle notifications, for example to forward them to slack.
- **References**: Tag editor for references.
- **References**: Added button to open contents view in a new browser tab.
- **UI**: Page size for contents view.
- **UI**: Less forgiving markdown preview.
- **UI**: Video support for rich text editor.
- **UI**: Clearer link to API documentation.
- **Strings**: StockImage editor with photes provided by Unsplash.
- **Performance**: Performance improvements and reduced memory allocations.
- **Performance**: Faster full text index.

### Fixed 

- **Backups**: Fixed several minor bugs in backups and increased the test coverage.
- **Infrastructure**: Fixed a bug when resetting plans (Cloud only).
- **Infrastructure**: Fixed header handling when Squidex is hosted behind a proxy.
- **Content**: Use proper MongoDB indices for default sorting.
- **UI**: Fixed image positioning in Safari in content list.
- **UI**: Fix for autosaving content.
- **Translation**: Fix for deepl translation.
- **Authentication**: Better logout.

## [4.0.3] - 2019-11-18

### Added

- **Login**: Support for Microsoft TenantId. Thanks to [mhilgersom](https://github.com/mhilgersom)

## [4.0.2] - 2019-11-18

### Fixed 

- **API**: Fix parsing of OData queries with required fields.
- **API**: Also add client to contributor index.
- **API**: Fix Asset upload size limit.
- **API**: Fixed required attribute for generated OpenAPI schema.
- **UI**: Add scripts to schema export so that it does not get overwritten on sync.
- **UI**: Field readonly fields in content lists.

## [4.0.1] - 2019-11-14

### Fixed 

- **UI**: Cancel button for new apps should not be disabled after submit.
- **Schema**: Fixed synchronization for list fields and reference fields.

## [4.0.0] - 2019-11-13

### Changed

#### List Fields

This feature contains a major update how reference fields and list fields are managed. In previous versions, schema fields had the properties `IsListField` or `IsReferenceField` to indicate whether a field should be shown in content lists or not. This was hard to manage in the UI and you could not specify the order. With this release schemas contain list of field names that should be used as list fields or reference fields. List field names can also contain meta fields like the content id or the content author. But we have decided not to implement a migration for that. If you use the feature you have to configure these fields again. Please not that the API is not affected and it is very likely not a breaking change for your consuming applications.

#### .NET Core 3.0

Migration to .NET Core 3.0. This also includes some code changes such as cleanup of configuration and proper nullable support.

This version does not use alpine image with self contained image any more. Therefore the final image is larger than before but the Squidex layer itself is smaller, which means a reduced disk size and download size when you update Squidex or when you have multiple versions installed or other .NET Core applications on your servers.

#### Clustering

This version introduces a new storage to communicate cluster members. Therefore it is recommended not to use a rolling deployment and restart the entire cluster instead.

### Added

- **UI**: New approach to manage list fields and schema fields.
- **UI**: Many small UI / UX improvements.
- **UI**: Improvements to the Geolocation editor.
- **UI**: Improved dialog to connect a client.
- **UI**: Improved Rule Wizard dialog.
- **UI**: Integrated cluster monitoring UI.
- **UI**: Improved schema UI.
- **UI**: Confirm dialog before removing contributor.
- **Workflows**: Restrict when a content item can be updated by setting an expression or roles.
- **Workflows**: Define multiple roles for workflows.
- **Rules**: Action to write comments.
- **API**: Migration to .NET Core 3.0
- **API**: Configuratiopn option to recreated the superadmin whe nyou loose the password.
- **GraphQL**: Flat content structure.
- **Clustering**: Clustering improvements.

### Fixed 

- **UI**: Fixed the buttons to change the status of multiple contents.
- **Rules**: Fixed saving of rule names.

## [4.0.0 Beta 1] - 2019-10-27

Migration to .NET Core 3.0. This also includes some code changes such as cleanup of configuration and proper nullable support.

This version does not use alpine image with self contained image any more. Therefore the final image is larger than before but the Squidex layer itself is smaller, which means a reduced disk size and download size when you update Squidex or when you have multiple versions installed or other .NET Core applications on your servers.

## [3.5.0] - 2019-10-26

**NOTE**: This is the latest release with .NET Core 2.X. Next release will be 3.0 and above. Does not really matter when you use Docker.

### Added

- **Grain**: Fixed grain indices.
- **Content**: Multiple schemas allowed for references.
- **UI**: Inline stars editor.
- **UI**: Get rid of immutable arrays.
- **UI**: Hide date buttons based on settings.
- **UI**: Updated several packages.
- **UI**: Improvement to contributor page.
- **UI**: Better error indicating when saving content.
- **UI**: Warning when changing content status and you have pending changes.
- **UI**: Markdown support for Alerts and Dialogs.
- **UI**: Design improvements.
- **UI**: Custom "Forbidden" page when users access a page he is not allowed to instead of automatic logout.
- **UI**: Migration to angular CDK drag and drop to replace two drag and drop libraries.
- **UI**: Collapse or expand all array items.
- **Migration**: Better cancellation support for migration.
- **Rules**: Custom payload for Webhook and Algolia action.
- **Rules**: Optional names for rules when you have multiple rules with the same actions and triggers.
- **Rules**: Basic statistic summary per rule.
- **Rules**: Filter rule events by rule.
- **Rules**: Added exception details for Algolia.
- **Common**: New diacritic character for slug 

### Fixed 

- **UI**: Fix references dropdown in query UI for localized values.
- **UI**: Fixed the unique checkbox in schema editor.
- **UI**: Fixed default date handling.
- **UI**: Fixed sorting of fields in schema synchronization endpoint.
- **UI**: Fixed preview button when multiple preview targets where configured.
- **UI**: Fixed TinymCE editor in arrays (Not recommended to use that!)
- **App**: Fix plan settings.
- **App**: Do not store default roles in the database so that we can change them later when new features are added.
- **Logging**: Use explicit thread for logging to console.
- **Logging**: Critical performance improvement.
- **Rules**: Fixed discourse action.

## [3.3.0] - 2019-09-08

### Added

- **UI**: Autosaving for content in local store.
- **UI**: Labels, descriptions and icons for contents.
- **UI**: Bulk import for contributors.
- **UI**: Pagination and search for contributors.
- **UI**: Improve file size for generated javascript bundles.
- **Rules**: Configurable default timeout for rule execution.
- **Assets**: Use asset url with slug when adding assets to rich text or markdown.
- **API**: Client per user.
- **API**: Limits for number of living content grains.

### Fixed 

- **UI**: Fix for dynamic chunk loading.
- **UI**: Styling fixes for date editor.
- **UI**: Improvement and fixes for checking unsaving changes.
- **API**: Fixes hateaos links for nested schema fields.

## [3.2.2] - 2019-08-20

### Fixed 

- **API**: Fixed a bug that prevented json response for field endpoint to be serialized correctly.
- **UI**: Layout fix for clients page.

## [3.2.1] - 2019-08-19

### Fixed 

- **MongoDB**: Fix index creation for Orleans tables.
- **MongoDB**: Fixes in OpenAPI definition.

## [3.2.0] - 2019-08-19

### Added

- **Contents**: Improved reference dropdown selector.
- **API**: Json queries for new query editor.
- **API**: Moved from Swagger2 to OpenAPI for generated documentation.
- **API**: Improved GraphQL error handling.
- **API**: Setting to show PII (Personally Identifiable Information) in logs.
- **UI**: Query editor for json queries.
- **UI**: Horizontal scrolling in UI.
- **Assets**: Pass in time to cache to asset API.
- **Assets**: Shorter asset fields and asset migration.
- **Rules**: Kafka rule action, thanks to https://github.com/sauravvijay
- **MongoDB**: Removed support for CosmosDB and DocumentDB due to high costs.

## [3.1.0] - 2019-07-25

### Added

- **Contents**: Include reference fields to indicate which fields to show when a content item is referenced by another content.
- **Contents**: Resolve references for content list when max items is set to 1.
- **UI**: Scrollbars improved and designed.

### Fixed 

- **UI**: Multiple fixes for modal dialogs.

## [3.0.0] - 2019-07-11

This version contains many major breaking changes. Please read: https://docs.squidex.io/next/02-api-compatibility

### Added

- **Contents**: Workflow system
- **API**: Hateoas
- **API**: Info endpoint
- **Configuration**: A lot of configuration settings to tweak some aspects of the UI.

## [v3.0.0-beta2] - 2019-06-29

### Added

- **Contents**: Editor for custom workflows.

## [v2.2.0] - 2019-06-29

### Added

- **Login**: Redirect to authentication provider automatically if only one provider is active.

### Fixed 

- **GraphQL**: Fix a bug in styles that prevented to autocomplete to show properly

## [v3.0.0-beta1] - 2019-06-24

This version contains many major breaking changes. Please read: https://docs.squidex.io/next/02-api-compatibility

## [v2.1.0] - 2019-06-22

### Added

- **Assets**: Parameter to prevent download in Browser.
- **Assets**: FTP asset store.
- **GraphQL**: Logging for field resolvers
- **GraphQL**: Performance optimizations for asset fields and references with DataLoader.
- **MongoDB**: Performance optimizations.
- **MongoDB**: Support for AWS DocumentDB.
- **Schemas**: Separator field.
- **Schemas**: Setting to prevent duplicate references.
- **UI**: Improved styling of DateTime editor.
- **UI**: Custom Editors: Provide all values.
- **UI**: Custom Editors: Provide context with user information and auth token.
- **UI**: Filter by status.
- **UI**: Dropdown field for references.
- **Users**: Email notifications when contributors is added.

### Fixed 

- **Contents**: Fix for scheduled publishing.
- **GraphQL**: Fix query parameters for assets.
- **GraphQL**: Fix for duplicate field names in GraphQL.
- **GraphQL**: Fix for invalid field names.
- **Plans**: Fix when plans reset and extra events.
- **UI**: Unify slugify in Frontend and Backend.

## [v2.0.5] - 2019-04-21

### Added

- **UI**: Sort content by clicking on the table header.

### Fixed 

- **UI**: Fix publish button in content context menu.

## [v2.0.4] - 2019-04-20

### Added

- **UI**: Link to go from a referenced content to the corresponding edit screen.
- **Contents**: Also query by items in array fields.

You can use the following syntax for array items:

    $filter=data/iv/hobbies/name eq 'Programming'

## [v2.0.3] - 2019-04-19

### Fixed 

- **UI**: Serveral essential bugfixes for radio buttons in Angular forms.

## [v2.0.2] - 2019-04-16

### Fixed 

- **Fulltext**: Workaround for a clustering bug that prevented the text indexer to run properly in cluster mode.
- **Fulltext**: Big performance improvement for text indexer.
- **Identity-Server**: Use special callback path for internal odic to not interfere with external oidc and Orleans Dashboard.

## [v2.0.1] - 2019-04-06

### Fixed 

- **Assets**: Fix the naming of assets that has changed since last version.
- **Assets**: Fix when overriding assets that do now exists.
- **Contents**: Fixed a bug that made the text indexer crash when an content was published that had no text.

### Added

- **Assets**: Introduces slugs for assets and the option to query assets by slugs.
- **Assets**: Dialogs to edit slugs.
- **UI**: Ability to host Squidex in a virtual directory.

### Changed

- This release sets the clustering mode to 'Development' which means it is turned off. This makes operations simpler for most users.

## [v2.0.0] - 2018-04-01

### Added

- **UI**: Automatic generation of UI for rule actions.
- **Contents**: Improved full text engine with `Lucene.NET`.
- **Server**: Plugin system.
- **Server**: Performance improvement for event handling.

The major feature of this release is the improved full text search. Content will be added to separate indices, which gives the following advantages:

- Each language is added to one field with individual stop words.
- Fuzzy search, e.g. `awsome~` to search for `awesome`.
- Search in one language only, e.g. `en:Home`

The full text index is populated in the background and it can therefore take a few seconds until you see the change. As ad admin you can restart the process in the admin section.

## [v1.16.2] - 2019-03-16

### Fixed 

- **UI**: Corrections for auto completions.
- **UI**: Correctly close onboarding tooltips.

## [v1.16.1] - 2019-03-08

### Fixed 

- **UI**: Multiple placeholders for interpolation.
- **UI**: Fix for button activation when adding rules.

## [v1.16.0] - 2019-02-23

### Added

- **CLI**: New commands for schema synchronization.
- **UI**: Imroved validation messages.
- **UI**: Integrate CLI documentation to client UI.
- **UI**: Side by side view for content differences.
- **UI**: Drag and drop assets to markdown editor.
- **UI**: Drag and drop assets to rich text editor.
- **UI**: Copy assets from clipboard to asset views.
- **UI**: Copy assets from clipboard to markdown editor.
- **UI**: Copy assets from clipboard to rich text editor.
- **UI**: Performance improvements and refactoring of components.
- **Schemas**: New endpoint to synchronize schemas.
- **Server**: Log all requests for cloud version and provide endpoint to download logs.
- **Server**: Improved logging for http requests.
- **Rules**: Generate event and trigger when the app consumed almost all resources.

### Fixed 

(Mostly due to UI refactoring :( )

- **UI**: Fixed custom editors.
- **UI**: Fixed disable state of restore button.
- **UI**: Fixes for addition button states.

## [v1.15.0] - 2019-01-05

### Added

- **Rules**: Javascript conditions for rule triggers.
- **Rules**: Javascript formatting for rule actions.

## [v1.14.0] - 2018-12-23

### Added

- **CLI**: Basic setup
- **CLI**: Export all Content
- **UI**: Edit asset tags and names in asset field.
- **UI**: Preview for SVG assets.
- **UI**: No stacked bars for API performance chart and a checkbox to toggle between stacked and non-stacked bars.
- **Users**: Invite users to an app even if they do not have an account yet.
- **Users**: Github authentication.
- **Client Library**: Integrated autogenerated management library.
- **Contents**: Preview urls for schemas.
- **Contents**: Button  to show all input fields for localized fields.
- **Scripting**: Access to user roles.

### Fixed 

- **API**: Several bugfixes for the JSON API and Swagger
- **UI**: Fixed dependencies and sortable lists.
- **UI**: Fixed disabled state for custom field editors.
- **Permissions**: Fixed duplicate permissions.

### Changed 

- *Improved build performance for the Frontend.
- *Migration to Angular7

## [v1.13.0] - 2018-12-08

### Added

- **Contents**: Uniqueness validator.
- **Swagger**: Show needed permission in swagger definition.
- **UI**: Array fields: Clone items.
- **UI**: Array fields: Collapsed all items to make sorting measier.
- **UI**: Array fields: Buttons for sorting.
- **UI**: Drag indicators for drag and drop lists.
- **General**: New fine grained permission system.
- **General**: Custom roles.
- **Rules**: Cancel queued events.
- **Rules**: Publication id for medium action.

### Fixed 

- **Backup**: Always make  the user, who restored an backup an owner.
- **Contents**: Implemented $search for Array fields.
- **UI**: Fixed for Edge browser.

### Changed 

- Migration to `.NET Core 2.2`
- Migration to `ASP.NET Core 2.2`
- Migration to `ASP.NET Core` Health System
- Performance improvements in logging system.
- Performance improvements for json serialization.
- Unificiation of storage implementations.
- Custom JSON structures to replace `JSON.NET` with faster serializer at later point of time.

## [v1.12.0] - 2018-11-06

### Added

- **Contents**: Comments for content items.
- **Contents**: Specify limits for max number of assets via options.
- **Assets**: Specify limits for max number of assets via options
- **UI**: List view for assets.
- **UI**: Reorder contents in references fields.

### Fixed 

- **GraphQL**: Fix languages with region support.

Various small bugfixes for UI and API.

## [v1.11.0] - 2018-09-24

### Added

- **API**: Correct handling of `If-None-Match` header to support caching.
- **Rules**: Major refactoring of action handlers to add new actions with less code.
- **Rules**: Twitter action to post status update.
- **Rules**: Prerender.io action to invalidate cache entries for SPA sites.
- **Contents**: Support IN-queries, like `fileName in ['Logo.jpg', 'Logo.png']`
- **UI**: Cloning content items.
- **UI**: Tag input in header to filter by tags when assigning assets to content.
- **Schemas**: Color picker as additional editor for string fields.
- **Statistics**: Report api usage and performance per client.

### Fixed 

- **Clustering / Orleans**: Fixed correct serialization of exceptions, e.g. as validation errors.
- **Backups**: Always assign the user who started the restore operation as Owner to the app.
- **UI**: Reset name when a asset or content query is saved.
- **UI**: Disable spellchecking for tag editor.

## [v1.10.0] - 2018-08-29

### Featues

- **Contents**: Introduce `X-Unpublished` header to also get unpublished content.
- **UI**: General feature to store UI settings.
- **UI**: Save content queries.
- **UI**: Save assets queries.
- **GraphQL**: Endpoint to run multiple queries in parallel with a single request.

## [v1.9.0] - 2018-08-19

### Added

- **Scripting**: Override for the slugify method to use single line characters when replacing diacritics.
- **Docker**: Smaller image size.

## [v1.8.0] - 2018-06-30

### Added

- **Schemas**: Singleton schemas (can only have single content)

### Fixed 

- **UI**: Nested fields got wrong ids and names and could not be saved.
- **Contents**: Ensure that the content api returns content in correct order when querying by ids.

## [v1.7.0] - 2018-06-25

- Migration to .NET Core 2.1

## [v1.6.2] - 2018-06-23

### Added

- **UI**: Better sortable with improved UX.
- **Migration**: Increased performance.
- **Migration**: Disable event handlers during migration.

### Fixed 

- **Schemas**: Invariant name handling for field names.

## [v1.6.1] - 2018-06-22

### Fixed 

- **MongoDB**: Fixed date time handling.

## [v1.6.0] - 2018-06-07

### Added

- **Schemas**: Nested Schemas.
- **UI**: Migration to RxJS6.
- **UI**: Migration to Angular6.

## [v1.5.0] - 2018-05-20

### Fixed 

- **UI**: Fixed the pattern selector in field editor.

### Added

- **Contents**: Allow to save content updates as draft.
- **Schemas**: Create folders to group schemas.
- **UI**: Increased the search input.
- **UI**: Plugin system for content editors.

## [v1.4.1] - 2018-05-02

### Fixed 

- **Orleans**: Remove orleans dashboard from 8080.

## [v1.4.0] - 2018-05-02

### Added

- **UI**: Big refactorings and UI improvements.
- **Actions**: New log formatter with placeholder for user infos.
- **Actions**: Azure Queue action.
- **Actions**: Algolia action.
- **Actions**: Fastly action.
- **Backup**: Backup all your data to an archive.

## [v1.3.0] - 2018-02-17

### Added

- **Actions**: ElasticSearch action.

### Changed 

- **DomainObjects**: Refactored domain objects to be ready for Orleans.

## [v1.2.0] - 2018-02-10

### Added

- **EventStore**: Event metadata are stored as json objects in MongoDB now and you cacn query by metadata.
- **Contents**: Updated to state can be scheduled, e.g. to publish them.

> This releases will run a migration, which might take a while and also effects the events. We recommend to make a backup first.

## [v1.1.7] - 2018-02-06

### Fixed 

- **UI**: Checkbox style fixed.

## [v1.1.6] - 2018-02-06

### Added

- **Rules**: Allow content triggers to catch all content events.
- **Rules**: Ensure that the events for an aggregate are handled sequentially.
- **UI**: History stream in the dashboard.
- **UI**: Better UI for apps overview.
- **Apps**: Added a ready to use blog sample.

### Fixed 

- **UI**: History UI was throwing an exception when a user was referenced in the message.
- **UI**: A lot of style fixes. 

## [v1.1.5] - 2018-02-03

### Added

- **Contents**: Slugify function for custom scripts.

### Fixed 

- **Migration**: Assets and schemas were not removed before recreation.
- **Contents**: OData queries only worked for data fields.
- **Assets**: OData queries did not work at all and included too many fields (e.g. AppId, Id).

## [v1.1.4] - 2018-02-03

### Added

- **Login**: Consent screen to inform the user about privacy policies.

## [v1.1.3] - 2018-02-03

### Added

- **Rules**: Trigger when asset has changed
- **Rules**: Action to purge cache items in fastly
- **Rules**: Action to push events to Azure storage queues.

### Fixed 

- **Rules**: Layout fixes.

### Changed 

- Freeze action, triggers and field properties using Fody.
- Fetch derived types automatically for Swagger generation.

## [v1.1.2] - 2018-01-31

### Added

- **Assets**: OData support, except full text search (`$search`)
- **Rules**: Slack action
- **Rules**: Algolia action

### Bugixes

- **Rules**: Color corrections for actions. 

### Changed

- Asset structure has changed: Migration will update the ocllection automatically.
- Asset endpoint:
    - `take` query parameter renamed to `$top`  for OData compatibility.
    - `skip` query parameter renamed to `$skip` for OData compatibility.
    - `query` query parameter replaced with OData. Use `$query=contains(fileName, 'MyQuery')` instead.
