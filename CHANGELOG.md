# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [7.3.0] - 2022-18-11

This version is all about the Update to .NET 7. If you host Squidex with Containers, e.g. with Docker Compose, Kubernetes or Managed Containers nothing changes. But if you host Squidex yourself in IIS or Linux, you have to ensure that you use the latest runtime.

### Changed

* **Runtime**: Migration to .NET 7
* **API**: New system how to manage type names, e.g. for field types, rule actions and so on, with the goal to improve serialization performance.

## [7.2.0] - 2022-11-11

### Fixed 

* **Assets**: Configured timeout for queries was ignored.
* **Assets**: Several fixes for tags which was causing duplicate tag names when a tag was renamed.
* **Backups**: Fix timeout handling for backups to mark timed out backups as failed.
* **Contents**: Configured timeout for queries was ignored.
* **Contents**: Disable component fields when field is disabled.
* **Contents**: Disable drag and drop for array editors, when the array field is disabled.
* **Contents**: Fix generated OpenApi specs.
* **Contents**: Fix geo queries (latitude and longitude) was swapped.
* **Rules**: Fixes cache duration for rule handling.
* **Templates**: Updated the template system to create a new temporary folder for each operation to query the repository.
* **UI**: More help pages.
* **UI**: More history pages.
* **UI**: Several fixes to handle schema fields.
* **UI**: Use a fallback image, if the app image cannot be loaded.

### Changed

* **API**: Better status handling for exceptions.
* **API**: Move to file scoped namespaces.
* **Backups**: More logs for the backup.
* **Contents**: Mark content-version endpoint as obsolete.
* **Helm**: Document helm parameters.
* **Rules**: Dedicated Rule action for OpenSearch.
* **Rules**: More logs for rule enqueuer.
* **UI**: Updated Angular to 15.0
* **UI**: Updated Bootstrap.

### Added

* **Billing**: Introducing teams to manage subscriptions across teams.
* **Billing**: Several changes to introduce a referral program.
* **Contents**: Button to show or hide the input for custom ID.
* **Contents**: Column to show the translation status.
* **Contents**: Define a custom GraphQL schema for JSON fields.
* **Contents**: New endpoint to fetch a specific content by version.
* **Contents**: OpenAPI definitions for the bulkd endpoint.
* **OpenAPI**: Add more controllers to OpenAPI spec.
* **Users**: Added tests for user management.

## [7.1.0] - 2022-09-01

### Fixed

* **Assets**: Fixes to UI texts.
* **Contents**: Do not make writes in parallel when running inside a transaction.
* **Contents**: Fixes the resize functionality of table columns.
* **Rules**: Fixes serialization of rule events inside rule events.
* **Rules**: Fixes the schema triggers for rules.
* **UI**: Do now show the first value in dropdowns when the current value is not valid.
* **UI**: Fixes the tag editor, which was measuring uninitialized content elements.

### Changed

* **Assets**: Also provide the edit token in the frontend GraphQL interface.
* **Contents**: Also provide the edit token in the frontend GraphQL interface.
* **Subscriptions**: Show the username of the current provider in the subscription settings.

### Added

* **Assets**: Subscribe to asset changes in GraphQL.
* **Contents**: Provide translation status in the UI
* **Contents**: Query contents by translation status.
* **Contents**: Subscribe to content changes in GraphQL.
* **UI**: Support custom role properties for UI over claims that start with `urn:squidex:custom:{APPNAME}`.

## [7.0.3] - 2022-08-15

### Fixed

* **UI**: Fixes the tag editor.

## [6.14.0] - 2022-08-15

### Fixed

* **UI**: Fixes the tag editor.

## [7.0.2] - 2022-08-09

### Fixed

* **APM**: Fixed Stackdriver Monitoring and the dependencies with Google libraries.

## [7.0.1] - 2022-08-04

### Fixed

* **JSON**: Correct usage of a JSON serializer for field IDs with fallback support.
* **Restore**: Provide log output during restore operation.

## [7.0.0] - 2022-08-02

This version contains major changes, that are also described in the [Blog](https://squidex.io/post/squidex-7.0-release-candidate-3-released).

1. **Orleans removed**: Orleans has been removed to make deployment easier. Nodes are independent now and you can host it basically everywhere. One instance needs to be assigned as a worker with the `CLUSTERING__WORKER=true` to environment variable. The default is true, so ensure that the other nodes have the value to set to false. The communication between instances and worker is established with a simple MongoDB-based queue, therefore there is no additional dependency.

2. **Faster JSON serialization**: Newtonsoft.JSON has been replaced with System.Text.Json, leading to a 100% performance improvement when writing and reading JSON. Some part of the MongoDB serialization (especially writing of app, schema or rule objects) was also implemented with Newtonsoft.JSON. This part became actually slower, but you can tell Squidex to use a faster serialization with the environment variable `STORE__MONGODB__VALUEREPRESENTATION=String`. The downside is that the objects are written to MongoDB as strings and not as normal values, therefore you cannot query for app, schema or rule properties anymore.

3. **Dedicated collections per content**: You can have one collection per schema now, which gives you the option to create indexes manually to improve query performance. Read the blog post for migration instructions.

### Changed

* **Assets**: Moved the update of tag counts to a event consumers to improve consistency.

## [6.13.0] - 2022-08-02

### Fixed

* **UI**: Fixes the rendering of reference lists.

## [6.12.0] - 2022-08-01

### Changed

* No changes, just meant to run CI again.

## [6.11.0] - 2022-07-29

### Fixed

* **Assets**: Fix recursive asset deletion. Query was selecting the wrong assets.

## [7.0.0-rc3] - 2022-07-29

### Fixed

* **Assets**: Fix recursive asset deletion. Query was selecting the wrong assets.
* **Assets**: Compatibility with 6.X collections fixed.
* **Contents**: Compatibility with 6.X collections fixed.

### Changed

* **Assets**: Moved the update of tag counts to a event consumers to improve consistency.

### Added

* **API**: New tests to cover more cases.

## [7.0.0-rc2] - 2022-07-25

### Fixed

* **Contents**: Disable the delete button on content list, if user does not have the necessary permissions.
* **Contents**: Fixed a bug which was hiding singleton schemas in the frontend.
* **Contents**: Fixed a critical bug with the validation scheduler.
* **Contents**: Use the correct asset folder, when asset is uploaded with button in rich text editor.
* **Indexing**: Several bugfixes with index grains.
* **Restore**: Fix a bug to restore apps without any published content items.
* **Translations**: Map the translation result code properly.
* **UI**: Several translation fixes.

### Changed

* **API**: Migration to System.Text.JSON for faster JSON performance.

### Added

* **Contents**: New flag to store each schema in a dedicated collection, so that indexes can be created.

## [6.11.0] - 2022-07-29

### Fixed

* **Assets**: Fix recursive asset deletion. Query was selecting the wrong assets.

## [6.10.0] - 2022-07-19

### Fixed

* **Contents**: Fixed a bug which was hiding singleton schemas in the frontend.

## [6.9.0] - 2022-07-14

### Fixed

* **Contents**: Disable the delete button on content list, if user does not have the necessary permissions.
* **Contents**: Fixed a critical bug with the validation scheduler.
* **Contents**: Use the correct asset folder, when asset is uploaded with button in rich text editor.
* **Indexing**: Several bugfixes with index grains.
* **Restore**: Fix a bug to restore apps without any published content items.
* **Translations**: Map the translation result code properly.
* **UI**: Several translation fixes.

### Added

* **UI**: Tooltips for asset folders.

## [7.0.0-rc1] - 2022-07-11

This version removes Orleans to make Squidex instances stateless and easier to deploy in multiple environments.

If you want to deploy this version, one, and only one, node must be declared as worker with the environment variable `CLUSTERING__WORKER=true`.

At the moment 2 extra components have been introduced:

1. A distributed cache to store specific versions of a content item or asset item over a short period of time to make the rule service faster.
2. A queue implementation to distribute workloads to the worker node.

To make the migration as easy as possible, the default implementation uses MongoDb, but other implementations might follow.

## [6.8.0] - 2022-06-29

### Fixed

* **Assets**: Allow to add assets without mime type via drag and drop.
* **Contents**: Do not scroll the markdown editor to the bottom of the text when a content is saved.
* **Contents**: Fixed the layout of half width editors in components and assets.
* **Dashboard**: Fixed bottom padding in app dashboard page.
* **Hosting**: Fixed config and path resolution when Squidex is hosted inside a subfolder.
* **UI**: Disable or hide several buttons and elements when the user does not have permissions for the corresponding feature.
* **UI**: Do not load notifications when notifo is enabled.
* **UI**: Fix layout out of history events.
* **UI**: Fixed the computed height of the schema lists.
* **UI**: Several translation fixes.
* **Usage Tracking**: Improved a index out of bounds exception when collecting usage data in the background.

### Changed

* **Contents**: Improved the performance of content validation for very large content models.
* **Contents**: New model for JSON values to reduce memory usage.
* **Contents**: Reduced the height of separator fields in the content UI.
* **Contents**: Several performance improvements to reduce allocations and memory usage.
* **GraphQL**: Fixed a minor memory leak.
* **Identity**: Fix a minor memory leak in a third partyy component.
* **Monitoring**: Improve health check and collect actual memory usage, not managed memory usage.
* **Monitoring**: Special setting to collect memory dumps automatically.
* **Scripting**: Validate content via sscripting.

### Added

* **Assets**: Added `auto` parameter to provide the best image format based on browser settings.
* **Assets**: Store the total number of assets per folder in MongoDB when the size exceeds 10.000 items. Improves performance for large results with a small extra cost for smaller result sets.
* **Contents**: Add the schema name to components when queried with a normal client (not the Management UI).
* **Contents**: Added a reference selector to markdown editor.
* **Contents**: Added a reference selector to rich text editor.
* **Contents**: Added a tooltip for string fields in reference lists.
* **Contents**: Define an option per workflow when a content should be published.
* **Contents**: Flag to make an patch not an update for upsert content jobs.
* **Contents**: Store the total number of contents per schema in MongoDB when the size exceeds 10.000 items. Improves performance for large results with a small extra cost for smaller result sets.
* **GraphQL**: New resolvers to extract embedded contents from string based fields.
* **Rules**: Readonly mode for rule events.

### Security

* **Identity**: Added an option to remove `X-Frame-Options` header when Squidex is hosted inside an iframe.
* **Rules**: Fine grained permissions for rule events.

## [6.7.0] - 2022-04-23

### Fixed

* **Contents**: Allow flatting of localized fields with custom languages codes.
* **Contents**: Fixed permission checks for some endpoints.
* **Routing**: Fixed base URL.
* **Scripting**: Fixed auto-completion of scripting.
* **UI**: Fixed default values for components.
* **UI**: Fixed input placeholders in route actions forms.
* **UI**: Fixed markdown fullscreen mode.
* **UI**: Fixed some z-index settings in the embed SDK.
* **UI**: Fixed table column widths for reference selector.

## Changed

* **Elastic**: Downgrade elastic search client.
* **Identity**: Map and merge all roles for OIDC authentication.
* **UI**: Improved the layout of the datetime editor.
* **UI**: Remember selected language in contents and content view.
* **UI**: Remove description of merged schema fields for contents filter.

## Added

* **Testing**: New tests for template handling.
* **Testing**: Storybook basic setup.
* **UI**: Tooltips for field fullscreen mode.

## Security

* **Routing**: Fixed a bug where the check for apps in a route was causing a 500 in some cases and not a 403, when the route parameter was invalid.
* **Routing**: Fixed a bug where the check for schemas in a route was causing a 500 in some cases and not a 403, when the route parameter was invalid.
* **Identity**: Allow request of access token for all domains.

## [6.6.0] - 2022-03-25

There was actually a 6.5.0, but it was not documented and due to a problem with the Github CI never published as a Release to Github.

This version contains a lot of small improvements, but also one breaking change. Therefore it was not easy to decide whether the major version should be increased or not. As part of a refactoring the redirect URIs for external authentication providers have been changed. You have to change the following paths when you use Google, Github, Microsoft or OIDC authentication.

* `/identity-server/signin-github` -> `/signin-github`
* `/identity-server/signin-google` -> `/signin-google`
* `/identity-server/signin-microsoft` -> `/signin-microsoft`
* `/identity-server/signin-oidc` -> `/signin-oidc`

### Fixed

* **Assets**: Fixed a bug when image metadata was resolved.
* **Assets**: Several fixes to the calculation of etags.
* **Contents**: Several fixes to the calculation of etags.
* **OpenAPI**: Several fixes to OpenAPI.
* **Rules**: Fixed the authentication for the Medium action.
* **Translations**: Fixed the selection of the chinese localization.
* **UI**: Fixed the default values for component fields.
* **UI**: Fixes for the field rules forms.

### Changed

* **Filters**: Better model for filters to simplify the UI and autocompletion in scripts.
* **Identity**: Migration to a new authentication structure.
* **Logs**: Easier logs with reabable messages.
* **UI**: Better unsplash field editor.
* **UI**: Migration to Angular CLI.
* **UI**: New library for virtualization to fix several bugs with the array editor and large lists.
* **UI**: New profile page.
* **UI**: New style for content table to support resizable columns.

### Added

* **Assets**: New function to calculate the blur hash of an image.
* **Assets**: Resumable file uploads with tus.
* **GraphQL**: Memory caching directive (only self hosting).
* **GraphQL**: New resolver to query references.
* **GraphQL**: Support to define the allowed values for string fields as GraphQL enum.
* **Templates**: New template system based on Github repositories.
* **UI**: Fullscreen button for all fields.
* **UI**: New embed SDK to allow inline editing to your website.
* **UI**: New gallery page to explain how to install templates.
* **UI**: Option to word wrap table columns.
* **UI**: Resizable table columns.

## [6.4.0] - 2021-12-20

### Fixed

* **Database**: Fix to support duplicate entries in the database (database is corrupt).
* **EventConsumer**: Fixed a swallowed exception in the event consumer.
* **UI**: Fixed a bug in the UI settings page.
* **UI**: Several styling fixes.
* **Users**: User endpoint should not throw exception when search string is null.

### Changed

* **Queries**: Also accept single values for IN operator and convert them to a arrays.
* **Scriping**: Allow to set nested content values in scripts.
* **Telemetry**: Remove telemetry for Orleans serialization and deserialization because it was detected as root span.
* **UI**: Automatically add scrollbars for large dropdowns.
* **UI**: Automatically collapse deeply nested UI components in the content page to improve performance.
* **UI**: Big refactoring of Angular forms to simplify code.
* **UI**: Do not load total count when moving to next page for lists like contents or assets.
* **UI**: Load referenced assets and contents in batches in the content page.

### Added

* **Assets**: Integrated ImageMagick for TGA support.
* **Assets**: Integrated ImageMagick for Webpack support.
* **Assets**: Support for a resizer server to outsource image resizing to a microservice.
* **Languages**: Expose native language names over API.
* **Scripting**: New function to generate guid values in scripts.
* **Tests**: New API tests to improve overall quality.

## [6.3.0] - 2021-11-22

### Fixed

- **Contents**: Urgent fix to solve NullReferenceException when resolving components in API.

## [6.2.0] - 2021-11-21

### Fixed

- **Events**: Ignore events after a deletion to fix a bug with migration.

### Added

- **Search**: Various improvements.

## [6.1.0] - 2021-11-18

### Fixed

- **Contents**: Fixed caching headers if they contain non-ASCII characters.
- **Contents**: Fixed OpenAPI generation for self referencing components.
- **Contents**: Improve the number of hold contents in memory to reduce memory footprint.
- **Migration**: Accept a few errors when a migration fails for some items.
- **UI**: Fix modals by showing scrollbar's if they would not have enough space.
- **UI**: Fixed spelling errors.

### Changed

- **Assets**: Remove max length restriction in the UI for assets metadata.

### Added

- **Assets**: Rename asset tags.
- **Clustering**: Support to run Squidex in Azure App Services.
- **Contents**: Fixed OData queries over POST requests.
- **Contents**: Sample how to implement custom editor with EditorJS.
- **Contents**: Sample how to implement custom editor with Monaco.
- **Contents**: Save content from inspection tab.
- **Event Sourcing**: EXPERIMENTAL Support for EventStore.
- **Search**: EXPERIMENTAL Azure Cognitive Search support.
- **Search**: EXPERIMENTAL Mongo Atlas Search support.
- **UI**: Fix tooltip performance issues (can really slow down the Browser.)
- **UI**: Nested schema categories.
- **UI**: New modal dialog to copy from one language to other languages.
- **UI**: Show the users that are currently visiting a content page.

## [6.0.1] - 2021-10-15

### Fixed

- **MongoDB**: Critical bugfix for the migration to 6.0.0

## [6.0.0] - 2021-10-14

### Fixed

- **MongoDB**: Fixed parsing of MongoDB versions for release candidates or beta versions.
- **UI**: Correct styling of content compare view.
- **UI**: Fix for reference dropdown.
- **UI**: Improve auto-save feature.

### Changed

- **API**: New content structure to allow full app deletion.
- **GraphQL**: Do not apply page size when retrieving references to get rid of limitation.
- **Schemas**: Simplify schema builder.

### Added

- **Assets**: Asset scripts.
- **Contents**: Uniqueness validation for components and array fields.
- **GraphQL**: Do not return error when content to query is from another schema.
- **UI**: Hint to run webpack for development mode.
- **UI**: Persist collapsed status for array fields in UI.

## [5.9.0] - 2021-09-14

### Fixed

- **Contents**: Fixed field rules to also work with components.
- **Hosting**: Several fixes to improve URL resolution when Squidex is hosted in a subfolder.
- **Notifo**: Improved error handling for Notifo integration.

### Changed

- **Assets**: Improved and optimized queries for asset folders.
- **Assets**: Improved asset folder dropdown.
- **Contents**: Added markdown support to all field hints.
- **Contents**: Improved workflow handling for `Save and Publish`.
- **Contents**: Resolve component types with `schemaName` field in the component object.
- **Event Processing**: Migration to channels.
- **Monitoring**: Migration to Open Telemetry traces.
- **Rules**: Improved rule simulator.
- **UI**: New and better design.

### Added

- **Assets**: New metadata provider for azure image recognition.
- **Clustering**: Added support for Kubernetes.
- **Contents**: Added a button to the UI to cancel content scheduling.
- **Contents**: Added a text field to the content editor to define a custom ID when creating content items.
- **Contents**: New calendar view for scheduled content items.
- **Contents**: New tab to inspect the content item and view the structure as pure JSON.
- **Contents**: New validation properties for videos.
- **GraphQL**: Exposed the `newStatus` field in GraphQL.
- **GraphQL**: Exposed the `newStatusColor` field in GraphQL.
- **Logging**: Added more information to the request log.
- **Rules**: Enable or disable rules over the context menu.
- **Rules**: New action to trigger SignalR.
- **Scripting**: New method to make HTTP methods using POST, PUT, PATCH and DELETE verbs.

## [5.8.2] - 2021-07-18

### Fixed

- **GraphQL**: Urgent bugfix for references and components. Please upgrade immediately from 5.8.1.

## [5.8.1] - 2021-07-17

### Fixed

- **API**: Several bugfixes for components, especially around caching of components.
- **GraphQL**: Several bugfixes for components.
- **UI**: Several bugfixes for components.
- **UI**: Several fixes to font-size.
- **UI**: Virtual scrolling for nested fields with a lot of items (> 50) to improve performance.

### Added

- **Assets**: Check SVG files for embedded scripts and block uploads.
- **Assets**: Option to create one folder per app for uploaded assets.
- **Rules**: Add `newStatus` to content changed event.
- **UI**: Define your custom date or date-time format for Date fields.

### Changed

- **API**: Support hosting without https (not recommended).
- **UI**: Load leaflet from local files instead of CDN.
- **UI**: Load leaflet geocoder from local files instead of CDN.
- **UI**: Load Orleans Dashboard assets from local files instead of CDN.
- **UI**: Load outdated-browser from local files instead of CDN.

## [5.8.0] - 2021-06-28

### Fixed

- **API**: Fixes for anonymous write access.
- **API**: Fixes for client access when creating apps.
- **Schemas**: Correct sync of field rules, especially for CLI.
- **UI**: Allow scrolling when content is disabled.
- **UI**: Encode IDs to allow custom content IDs with slash.
- **UI**: Fix confirm click.
- **UI**: Fixes references/referencing view for localized content.
- **UI**: Fixes typos for italian translation.
- **UserManagement**: Fix pagination

### Added

- **API**: Better API tests to improve stability.
- **API**: Better timeout and cancellation handling.
- **API**: Default timeouts for most important MongoDB calls.
- **Assets**: Additional configuration flag to allow one folder per asset.
- **Contents**: Array builder when building custom code extension.
- **Contents**: Better indexes for improved performance.
- **Contents**: Read published contents from secondary MongodB instances for better load distribution.
- **Rules**: New liquid and javascript extensions to read the asset as text in rules.
- **Rules**: Simpler syntax to resolve assets and contents in liquid templates.
- **UI**: Chinese translation.
- **UI**: Show SVG as images when in content overview.

## [5.7.1] - 2021-05-21

### Fixed

- **Events**: Read the correct amount of events with event store.

## [5.7.0] - 2021-05-07

### Added
 
- **Assets**: Delete assets permanently.
- **GraphQL**: Provide detailed user information for contents and assets.
- **GraphQL**: Upgrade to GraphQL.NET 4 for better performance.
- **Infrastructure**: New option to disable the request log.
- **Infrastructure**: Update of NSWAG to reduce memory footprint.
- **Performance**: Performance improvements for restore operations, recovery and repair.
- **Performance**: Performance optimization for batch operations.
- **Rules**: Better rule editor for scripts and templates including intellisense code formatting.
- **Rules**: Http DELETE for webhooks.
- **Rules**: New rule simulator to test how a rule would have behaved for the latest 100 events within the last 7 days.
- **Scripting**: More variables for scripts.
- **Scripting**: New functions to hash strings with SHA256 and MD5.
- **UI**: Easy cloning of references.
- **UI**: New fullscreen view for rules.
- **UI**: Show asset selector in UI plugins.
- **UI**: Show confirm and alert dialogs in UI plugins.
- **UI**: Show saved queries in search dialog.

### Fixed 

- **Contents**: Correct validation for PUT requests.
- **GraphQL**: Fix GraphQL for empty nested fields.
- **OpenAPI**: Fix several endpoint descriptions.
- **UI**: Fix several error handling bugs, especially around content updates.
- **UI**: Url decode role names in API endpoint.

## [5.6.0] - 2021-02-27

### Added

- **API**: Better support for OpenAPI code generators.
- **API**: Endpoint to provide JSON schemas for all rule events.
- **API**: Fix total calculation in same hot paths.
- **API**: Performance improvements to reduce memory usage.
- **API**: Performance improvements to the GraphQL endpoint.
- **Contents**: New flat data API description.
- **Contents**: New scripting methods: `getReferences` and `getAssets`
- **Contents**: Punjabi as new language.
- **Infrastructure**: Simplified serializer to migrate to System.Text.Json when possible.
- **Permissions**: New permissions to only give access to contents created by the user.
- **Rules**: Custom payloads for Azure Queue rules.
- **UI**: Delay tooltip a little bit.
- **UI**: Drag and drop of multiple files and folders.
- **UI**: Editor extensions.
- **UI**: Simply autocompletion for scripting.

### Fixed 

- **Assets**: Fix surrogate keys for asset endpoint.
- **Contents**: Null fixes for OData.
- **UI**: Fix custom editor in nested forms.
- **UI**: Fix reset button for assets.
- **UI**: Max height for language dropdown.
- **UI**: Shortcut for new asset folder fixed.
- **UI**: Show all properties when exporting schemas.
- **Usages**: Fix usage notifications.

## [5.5.0] - 2021-01-24

### Added

- **Admin**: Better user management in the backend.
- **Admin**: Possible to delete users now.
- **Backend**: Better tests for replicated caches.
- **Backend**: Reduce memory allocations in the backend.
- **Contents**: Geolocation fields also accept GeoJson objects now.
- **Contents**: Reference specific content version in GraphQL endpoint.
- **Contents**: Support for Geo queries with the full text index. You need to rerun the full text indexer for old geolocation fields.
- **Notifo**: Update to newer notifo REST service.
- **UI**: Better sorting of schemas in custom Roles.
- **UI**: Better support for SVG images in the rich text and markdown editor.
- **UI**: Change the default of date editors to local mode instead of UTC.
- **UI**: Fixed default value handling.
- **UI**: Integrated document viewer for documents like Word and PowerPoint.
- **UI**: Integrated video player for assets.

### Fixed 

- **Assets**: Fixed a bug where the wrong version was used when referencing assets.
- **Assets**: Fixed a critical bug in the backend to not deliver protected assets when an old version without that protection was queried.
- **Events**: Fix event consumers for older mongo installations (< 4.0 ).
- **Identity**: Fix the config management for external ODIC providers.
- **UI**: Fix quick nav after migration to newer Angular version.
- **UI**: Fixed a bug that was causing the folders not to show up properly.
- **UI**: Fixed a layer in the OpenStreetMap editor to use https instead of http.
- **UI**: Fixed array sorting in the UI.

## [5.4.0] - 2020-12-28

### Added

- **API**: Migrate to new Notifo API.
- **API**: Update to Net5.
- **Contents**: Check referrers before a content item is unpublished.
- **Contents**: Ensure that sorting is always consistent.
- **Contents**: More default values for other field types like references and assets.
- **Contents**: More support for bulk actions.
- **Contents**: Optimize DB calls.
- **Contents**: Optionally validate during publish operation.
- **Contents**: Update multiple content items in a bulk operation.
- **Rules**: Run rules from snapshots.
- **Translation**: Also support Google Translate.
- **UI**: Allow to translate all string fields.
- **UI**: Localize date-picker.
- **UI**: News views to show all references contents and referencing content items for a single content.
- **UI**: Quick links to asset folder in asset overview.
- **UI**: Rich dropdown for users when creating content filters.
- **UI**: Show asset path in asset dialog.
- **UI**: Update to Angular 11.

### Fixed 

- **Assets**: Fixed a bug in usage tracking of asset downloads that was causing it to never decrease.
- **Assets**: Fixes parent id (folder id) after migration from 4.X.
- **Contents**: GraphQL fallback handling when schema is called `content`.
- **Hosting**: Fixes a few problems around hosting and header forwarding (e.g. `X-Proto-For`).
- **Rules**: Fix rerun logic.
- **UI**: Several drag and drop bugfixes for angular CDK.

## [5.3.0] - 2020-10-30

### Changed

This changes reverts the changes from `5.1.0` and uses a normal handling of forwarded headers again.

Futhermore it fixes some issues with the migration from 4.X versions. You have to run the migrations again as explained here: https://docs.squidex.io/01-getting-started/installation/troubleshooting-and-support#my-migration-is-broken

### Added

- **Assets**: Introduce a new option to fix broken asset files.
- **Assets**: Upload assets by dropping a folder to the drop area.
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

- **Backup**: Adjust asset urls in strings to new app.
- **Backup**: Check version compatibility.
- **Search**: Batching operations for Elastic Search
- **Search**: Search in field with Elastic Search
- **Search**: Search lazy with Elastic Search
- **Server**: Remove support for https redirects and X-FORWARDED headers.
- **UI**: Fullscreen mode for custom field editors.
- **UI**: Half width fields for field editors.
- **UI**: Language toggle to switch UI language.
- **UI**: Leave an app.
- **UI**: Make the schema selector dropdown more visible.
- **UI**: Markdown support for field labels.

### Fixed 

- **UI**: Fixed the route synchronizer.
- **UI**: Fix a few spelling errors.
- **UI**: Fixes checkboxes for custom roles.
- **Assets**: Fix asset urls in GraphQL.

## [5.0.0] - 2020-10-08

This feature adds custom IDs to the system. So far every ID like a content ID is globally unique. This means that you cannot create custom IDs and when a backup is restored you need to assign new ids, because the old IDs might be already in use.

With this version on, every ID is prefixed with the app id, meaning they are only unique within an app. This allows new endpoints for upserting contents and defining custom ids for assets.

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
- **Assets**: New query string properties to change the format of assets, e.g. from PNG to JPG.
- **Assets**: Reduce the number of threads that resize images to improve performance.
- **Content**: New full text index based on MongoDB.
- **Contents**: Better exception handling in validation.
- **Events**: Count the number of processed events to get an understanding of performance.
- **Events**: New event consumers with support for batching. Can process up to 10.000 events / second.
- **General**: Better default configuration.
- **GraphQL**: GraphQL Mutations.
- **Roles**: Role properties to customize the UI.
- **Rules**: Enable algolia and ElasticSearch rule actions for all events.
- **Rules**: New functions for rules.
- **UI**: Disable add button for array field when max items is reached.
- **UI**: Dutch translation.
- **UI**: Italian translation.
- **UI**: Sidebar plugin for contents and content items.

### Bugfixes

- **Backup**: Fix memory usage when downloading backups.
- **Clients**: Fix anonymous access clients.
- **Contents**: Fixes to empty filter.
- **Contents**: Fixes to the angular forms to revert a performance improvements.
- **UI**: Fix confirm button.

## [5.0.0 BETA 2] - 2020-09-02

Includes new features in 4.6.0

## [4.6.0] - 2020-09-02

### Added

- **API**: Allow anonymous access per client.
- **API**: Client contingent or API calls to protected your API.
- **API**: Special headers to simulate errors in the UI.
- **Assets**: Better fallback handling when resizing assets failed.
- **Assets**: Rotate images with orientation metadata.
- **Contents / Schemas**: Field rules to disable or hide fields based on conditions.
- **Contents**: Better word count implementation which also works for CJK languages. (Chinese, Japanese, Korean).
- **Contents**: Filter content by `newStatus` field.
- **Contents**: Text based validation for string fields. You can define the content type now and the validator will extract the plain text from html and markdown to make character or word count validation.
- **Performance**: Replicated cache for some high load scenarios.
- **UI / General**: Support for localized UI and backend with support for `en` and `nl` for now. Italian is coming as well.
- **UI**: Button per field to unset a field value.
- **UI**: Confirm dialog when removing assets or references.
- **UI**: Custom editors for all field types.
- **UI**: Open referenced content in new tab.
- **UI**: Show current traffic usage in Dashboard.
- **UI**: Toggle between locale and UTC mode for datetime editors.
- **Workflows**: Visualization of the workflow with a readonly diagram.


### Fixed 

- **Algolia**: Fixed Algolia rule action.
- **Contents**: Fixes for authentication and GraphQl GET Endpoint.
- **Contents**: Improve javascript error handling for schemas scripts. Some exceptions have been swallowed before.
- **Contents**: Improved content scheduler to handle contents better, when the app or schema has already been deleted.
- **Contributors**: Ignore casing of email addresses, which was causing Squidex not to invite people with uppercase characters in email addresses.
- **EventSourcing**: Fixed a bug for the event consumer, which was skipping some events in high load scenarios. Need a replica set and Mongo 4.2. to work properly.

## [5.0.0 BETA 1] - 2020-07-06

This version introduces a new way to deal with ids. So far each content element has an id that is unique across all apps. This causes problems, because you cannot define your own ids and the ids have to change when you clone an app via backup and restore.

With this version ids for content items and assets are only unique within an app.

To make this possible, this version rebuilds all content items, assets and asset folders which can take a few minutes.

## [4.5.0] - 2020-07-06

### Added

- **Backups**: Increase download timeout for backups to 60 minutes.
- **Notifo**: Notifo integration finalized.
- **Rules**: Also create rule events when the creation failed to simplify debugging.
- **Rules**: Change expiration for rule events to be relative from now instead of relative to the original event to make replaying easier.
- **UI**: CTRL+Click for content items to open them in new tabs. Mimic default browser behavior for links.
- **UI**: Customizable dashboard.
- **UI**: Define the preview modes for assets.
- **UI**: Include external dependencies into the build to run Squidex in protected company networks.
- **UI**: Make sections defined by schema separators collapsible.

### Fixed 

- **Assets**: Fixed a bug where the wrong permission was checked for protected assets.
- **Assets**: Fixed the wrong calculation of focus points when resizing assets.
- **Assets**: Upgraded the image library to a newer version to fix a bug with resizing.
- **Contents**: Minor fixes for flat content.
- **Rules**: Fixed avro serialize for union schemas.
- **Rules**: Fixed several bugs in the rule runner.
- **Rules**: Proper cancellation for kafka.
- **Schemas**: Fixed several minor bugs in the schema synchronizer.

## [4.4.0] - 2020-06-15

### Added

- **Rules**: Liquid support.

### Fixed 

- **EventStore**: Fix a bug where very old events were not consumed properly.
- **Grains**: Fix restart of grains

## [4.4.0 RC] - 2020-05-30

### General

- Many improvements to tests and integration of API tests into CI pipeline.

### Added 

- **Amazon S3**: Allow to upload assets where the stream has no length.
- **Authentication**: Improve performance when checking permissions by simple caching.
- **Authentication**: Use local API authentication to bypass the extra call to identity server and to make deployment easier.
- **Clustering**: Auto restart background processes.
- **Contents**: Improvements to the bulk endpoint to also allow deletion and updates.
- **Contents**: Improvements to the enrichment flow of contents when they are queried from the database.
- **Contents**: Use aggregation framework to order large data sets.
- **Rules**: Define fallback values in formatting, e.g. `${CONTENT_DATA.name.iv ? Fallback}`
- **Rules**: Define transformations, e.g. `${CONTENT_DATA.name.iv | upper}`: Upper, Lower, Slugify, EncodeJson, Timestamp_Ms, Timestamp_Seconds
- **Rules**: Defined payload, headers and key in kafka rule action.
- **Rules**: Resolve reference in formatting, e.g. `${CONTENT_DATA.city.iv.data.name.iv}`
- **Rules**: Support for avro serialization in kafka rule action.
- **Scripts**: Fallback for `oldStatus`.
- **UI**: Additional editor to use checkboxes for references.

### Fixed 

- **Assets**: Fixed a bug where deleting folders using the UI was not working properly.
- **Authentication**: Fixed a bug where invited collaborators were not added to an app correctly.
- **Contents**: Fixed a bug where references were cleared in some conditions.
- **Contents**: Use aggregation framework to order large data sets.
- **Schemas**: Fix for schema synchronizer were some changes were not discovered correctly.
- **UI**: Do not show first value in dropdowns when no value is defined.
- **UI**: Fixed the layout of asset preview in content list.
- **UI**: Fixes for notifications and show newest notifications first.

## [4.3.0] - 2020-04-27

### Added

- **API**: Dedicated health check for event consumers and background processes.
- **Rules**: Integrated a background worker to start rules from beginning.
- **Scripting**: Incrementing counters.
- **Users**: Custom user properties.

### Fixed 

- **API**: Better error handling for unsupported OData features.
- **API**: Fix in OpenAPI schema to get rid of FieldNames collection that causes problems in code generators.
- **API**: Fixed index usage for event store.
- **API**: Short header for surrogate keys and custom request header to turn off keys.
- **Rules**: Fixed a bug in email rule which was using email body as sender and recipient address.
- **Rules**: Use default timeout in webhook.
- **Search**: Fixed a small minor in full text index.
- **UI**: Fix in autocompletion component which was causing issues in role form.
- **UI**: Fixed a bug that was showing all assets and not in their folders.
- **UI**: Fixed a layout bug in tag editor.
- **UI**: Fixed a layout bug in the role form.
- **UI**: Time formatting fixed.

## [4.2.0 Beta 2] - 2020-02-24

This release just contains a lot of bugfixes.

## [4.2.0 Beta 1] - 2020-02-20

The release makes a lot of changed to the content structure, therefore it will run a migration to recreate the contents collections. This can take a while.

### Added

- **Contents**: Alignment of workflows.
- **Contents**: Full text search also includes references items.
- **Contents**: Improvements to full text index for later support of elastic search.
- **UI**: Global search

## [4.1.3] - 2020-02-20

### Fixed 

- **UI**: Several fixes due to wrong references of SCSS mixins.

### Added

- **Assets**: Option to turn on the recursive deletion process.

## [4.1.2] - 2020-02-19

### Fixed 

- **UI**: Fix for sorting content when clicking on table headers.
- **UI**: Fix to show all controls for localized assets.
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

- **Assets**: Integrated editor / cropper for basic image editing.
- **Assets**: Support for focus points in UI and API.
- **Contents**: Better and more consistent content cleanup and enrichment.

### Fixed 

- **API**: Faster equals checks.
- **API**: Fixed a critical bug that caused an infinite loop and Out of Memory.
- **API**: Many small bugfixes.

## [4.1.0 Beta 1] - 2020-01-17

### Added

- **Assets**: Amazon S3 support for assets.
- **Assets**: Asset metadata with built in editor in the management UI.
- **Assets**: Better detection of asset type, including videos and extracting of more metadata.
- **Assets**: Dedicated permission to upload new version of asset.
- **Assets**: Folders to organized your assets.
- **Assets**: Protect assets.
- **Comments**: Markdown support.
- **Comments**: Mention other contributors by email address.
- **Comments**: Notification when you get mentioned.
- **Comments**: Rule action to create comments for content items.
- **Comments**: Trigger to handle notifications, for example to forward them to slack.
- **Geolocation**: General UX improvements for Editor.
- **Geolocation**: Search by location in OpenStreetMap-Editor.
- **GraphQL**: Flat data to provide content with default language rules.
- **Logging**: Increased log levels and performance improvements.
- **Logging**: Store request logs in MongoDB for fast download (also in hosted version).
- **Performance**: Faster full text index.
- **Performance**: Performance improvements and reduced memory allocations.
- **References**: Added button to open contents view in a new browser tab.
- **References**: Tag editor for references.
- **Strings**: StockImage editor with photos provided by Unsplash.
- **UI**: Clearer link to API documentation.
- **UI**: Less forgiving markdown preview.
- **UI**: Page size for contents view.
- **UI**: Video support for rich text editor.

### Fixed 

- **Authentication**: Better logout.
- **Backups**: Fixed several minor bugs in backups and increased the test coverage.
- **Content**: Use proper MongoDB indices for default sorting.
- **Infrastructure**: Fixed a bug when resetting plans (Cloud only).
- **Infrastructure**: Fixed header handling when Squidex is hosted behind a proxy.
- **Translation**: Fix for deepl translation.
- **UI**: Fix for auto-saving content.
- **UI**: Fixed image positioning in Safari in content list.

## [4.0.3] - 2019-11-18

### Added

- **Login**: Support for Microsoft TenantId. Thanks to [mhilgersom](https://github.com/mhilgersom)

## [4.0.2] - 2019-11-18

### Fixed 

- **API**: Also add client to contributor index.
- **API**: Fix Asset upload size limit.
- **API**: Fix parsing of OData queries with required fields.
- **API**: Fixed required attribute for generated OpenAPI schema.
- **UI**: Add scripts to schema export so that it does not get overwritten on sync.
- **UI**: Field readonly fields in content lists.

## [4.0.1] - 2019-11-14

### Fixed 

- **Schema**: Fixed synchronization for list fields and reference fields.
- **UI**: Cancel button for new apps should not be disabled after submit.

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

- **API**: Configuration option to recreate the super-admin when you loose the password.
- **API**: Migration to .NET Core 3.0
- **Clustering**: Clustering improvements.
- **GraphQL**: Flat content structure.
- **Rules**: Action to write comments.
- **UI**: Confirm dialog before removing contributor.
- **UI**: Improved dialog to connect a client.
- **UI**: Improved Rule Wizard dialog.
- **UI**: Improved schema UI.
- **UI**: Improvements to the Geolocation editor.
- **UI**: Integrated cluster monitoring UI.
- **UI**: Many small UI / UX improvements.
- **UI**: New approach to manage list fields and schema fields.
- **Workflows**: Define multiple roles for workflows.
- **Workflows**: Restrict when a content item can be updated by setting an expression or roles.

### Fixed 

- **UI**: Fixed the buttons to change the status of multiple contents.
- **Rules**: Fixed saving of rule names.

## [4.0.0 Beta 1] - 2019-10-27

Migration to .NET Core 3.0. This also includes some code changes such as cleanup of configuration and proper nullable support.

This version does not use alpine image with self contained image any more. Therefore the final image is larger than before but the Squidex layer itself is smaller, which means a reduced disk size and download size when you update Squidex or when you have multiple versions installed or other .NET Core applications on your servers.

## [3.5.0] - 2019-10-26

**NOTE**: This is the latest release with .NET Core 2.X. Next release will be 3.0 and above. Does not really matter when you use Docker.

### Added

- **Common**: New diacritic character for slug 
- **Content**: Multiple schemas allowed for references.
- **Grain**: Fixed grain indices.
- **Migration**: Better cancellation support for migration.
- **Rules**: Added exception details for Algolia.
- **Rules**: Basic statistic summary per rule.
- **Rules**: Custom payload for Webhook and Algolia action.
- **Rules**: Filter rule events by rule.
- **Rules**: Optional names for rules when you have multiple rules with the same actions and triggers.
- **UI**: Better error indicating when saving content.
- **UI**: Collapse or expand all array items.
- **UI**: Custom "Forbidden" page when users access a page he is not allowed to instead of automatic logout.
- **UI**: Design improvements.
- **UI**: Get rid of immutable arrays.
- **UI**: Hide date buttons based on settings.
- **UI**: Improvement to contributor page.
- **UI**: Inline stars editor.
- **UI**: Markdown support for Alerts and Dialogs.
- **UI**: Migration to angular CDK drag and drop to replace two drag and drop libraries.
- **UI**: Updated several packages.
- **UI**: Warning when changing content status and you have pending changes.

### Fixed 

- **App**: Do not store default roles in the database so that we can change them later when new features are added.
- **App**: Fix plan settings.
- **Logging**: Critical performance improvement.
- **Logging**: Use explicit thread for logging to console.
- **Rules**: Fixed discourse action.
- **UI**: Fix references dropdown in query UI for localized values.
- **UI**: Fixed default date handling.
- **UI**: Fixed preview button when multiple preview targets where configured.
- **UI**: Fixed sorting of fields in schema synchronization endpoint.
- **UI**: Fixed the unique checkbox in schema editor.
- **UI**: Fixed TinymCE editor in arrays (Not recommended to use that!)

## [3.3.0] - 2019-09-08

### Added

- **API**: Client per user.
- **API**: Limits for number of living content grains.
- **Assets**: Use asset url with slug when adding assets to rich text or markdown.
- **Rules**: Configurable default timeout for rule execution.
- **UI**: Autosaving for content in local store.
- **UI**: Bulk import for contributors.
- **UI**: Improve file size for generated javascript bundles.
- **UI**: Labels, descriptions and icons for contents.
- **UI**: Pagination and search for contributors.

### Fixed 

- **API**: Fixes hateaos links for nested schema fields.
- **UI**: Fix for dynamic chunk loading.
- **UI**: Improvement and fixes for checking unsaved changes.
- **UI**: Styling fixes for date editor.

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

- **API**: Improved GraphQL error handling.
- **API**: Json queries for new query editor.
- **API**: Moved from Swagger2 to OpenAPI for generated documentation.
- **API**: Setting to show PII (Personally Identifiable Information) in logs.
- **Assets**: Pass in time to cache to asset API.
- **Assets**: Shorter asset fields and asset migration.
- **Contents**: Improved reference dropdown selector.
- **MongoDB**: Removed support for CosmosDB and DocumentDB due to high costs.
- **Rules**: Kafka rule action, thanks to https://github.com/sauravvijay
- **UI**: Horizontal scrolling in UI.
- **UI**: Query editor for json queries.

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

- **API**: HATEAOS
- **API**: Info endpoint
- **Configuration**: A lot of configuration settings to tweak some aspects of the UI.
- **Contents**: Workflow system

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

- **Assets**: FTP asset store.
- **Assets**: Parameter to prevent download in Browser.
- **GraphQL**: Logging for field resolvers
- **GraphQL**: Performance optimizations for asset fields and references with DataLoader.
- **MongoDB**: Performance optimizations.
- **MongoDB**: Support for AWS DocumentDB.
- **Schemas**: Separator field.
- **Schemas**: Setting to prevent duplicate references.
- **UI**: Custom Editors: Provide all values.
- **UI**: Custom Editors: Provide context with user information and auth token.
- **UI**: Dropdown field for references.
- **UI**: Filter by status.
- **UI**: Improved styling of DateTime editor.
- **Users**: Email notifications when contributors is added.

### Fixed 

- **Contents**: Fix for scheduled publishing.
- **GraphQL**: Fix for duplicate field names in GraphQL.
- **GraphQL**: Fix for invalid field names.
- **GraphQL**: Fix query parameters for assets.
- **Plans**: Fix when plans reset and extra events.
- **UI**: Unify slugify in Frontend and Backend.

## [v2.0.5] - 2019-04-21

### Added

- **UI**: Sort content by clicking on the table header.

### Fixed 

- **UI**: Fix publish button in content context menu.

## [v2.0.4] - 2019-04-20

### Added

- **Contents**: Also query by items in array fields.
- **UI**: Link to go from a referenced content to the corresponding edit screen.

You can use the following syntax for array items:

    $filter=data/iv/hobbies/name eq 'Programming'

## [v2.0.3] - 2019-04-19

### Fixed 

- **UI**: Several essential bugfixes for radio buttons in Angular forms.

## [v2.0.2] - 2019-04-16

### Fixed 

- **Identity-Server**: Use special callback path for internal odic to not interfere with external OIDC and Orleans Dashboard.
- **Search**: Big performance improvement for text indexer.
- **Search**: Workaround for a clustering bug that prevented the text indexer to run properly in cluster mode.

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

- **Contents**: Improved full text engine with `Lucene.NET`.
- **Server**: Performance improvement for event handling.
- **Server**: Plugin system.
- **UI**: Automatic generation of UI for rule actions.

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
- **Rules**: Generate event and trigger when the app consumed almost all resources.
- **Schemas**: New endpoint to synchronize schemas.
- **Server**: Improved logging for http requests.
- **Server**: Log all requests for cloud version and provide endpoint to download logs.
- **UI**: Copy assets from clipboard to asset views.
- **UI**: Copy assets from clipboard to markdown editor.
- **UI**: Copy assets from clipboard to rich text editor.
- **UI**: Drag and drop assets to markdown editor.
- **UI**: Drag and drop assets to rich text editor.
- **UI**: Improved validation messages.
- **UI**: Integrate CLI documentation to client UI.
- **UI**: Performance improvements and refactoring of components.
- **UI**: Side by side view for content differences.

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
- **Client Library**: Integrated auto-generated management library.
- **Contents**: Button  to show all input fields for localized fields.
- **Contents**: Preview urls for schemas.
- **Scripting**: Access to user roles.
- **UI**: Edit asset tags and names in asset field.
- **UI**: No stacked bars for API performance chart and a checkbox to toggle between stacked and non-stacked bars.
- **UI**: Preview for SVG assets.
- **Users**: Github authentication.
- **Users**: Invite users to an app even if they do not have an account yet.

### Fixed 

- **API**: Several bugfixes for the JSON API and Swagger
- **Permissions**: Fixed duplicate permissions.
- **UI**: Fixed dependencies and sortable lists.
- **UI**: Fixed disabled state for custom field editors.

### Changed 

- *Improved build performance for the Frontend.
- *Migration to Angular7

## [v1.13.0] - 2018-12-08

### Added

- **Contents**: Uniqueness validator.
- **General**: Custom roles.
- **General**: New fine grained permission system.
- **Rules**: Cancel queued events.
- **Rules**: Publication id for medium action.
- **Swagger**: Show needed permission in swagger definition.
- **UI**: Array fields: Buttons for sorting.
- **UI**: Array fields: Clone items.
- **UI**: Array fields: Collapsed all items to make sorting easier.
- **UI**: Drag indicators for drag and drop lists.

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
- Unification of storage implementations.
- Custom JSON structures to replace `JSON.NET` with faster serializer at later point of time.

## [v1.12.0] - 2018-11-06

### Added

- **Assets**: Specify limits for max number of assets via options
- **Contents**: Comments for content items.
- **Contents**: Specify limits for max number of assets via options.
- **UI**: List view for assets.
- **UI**: Reorder contents in references fields.

### Fixed 

- **GraphQL**: Fix languages with region support.

Various small bugfixes for UI and API.

## [v1.11.0] - 2018-09-24

### Added

- **API**: Correct handling of `If-None-Match` header to support caching.
- **Contents**: Support IN-queries, like `fileName in ['Logo.jpg', 'Logo.png']`
- **Rules**: Major refactoring of action handlers to add new actions with less code.
- **Rules**: Prerender.io action to invalidate cache entries for SPA sites.
- **Rules**: Twitter action to post status update.
- **Schemas**: Color picker as additional editor for string fields.
- **Statistics**: Report api usage and performance per client.
- **UI**: Cloning content items.
- **UI**: Tag input in header to filter by tags when assigning assets to content.

### Fixed 

- **Backups**: Always assign the user who started the restore operation as Owner to the app.
- **Clustering / Orleans**: Fixed correct serialization of exceptions, e.g. as validation errors.
- **UI**: Disable spellchecking for tag editor.
- **UI**: Reset name when a asset or content query is saved.

## [v1.10.0] - 2018-08-29

### Features

- **Contents**: Introduce `X-Unpublished` header to also get unpublished content.
- **GraphQL**: Endpoint to run multiple queries in parallel with a single request.
- **UI**: General feature to store UI settings.
- **UI**: Save assets queries.
- **UI**: Save content queries.

## [v1.9.0] - 2018-08-19

### Added

- **Docker**: Smaller image size.
- **Scripting**: Override for the slugify method to use single line characters when replacing diacritics.

## [v1.8.0] - 2018-06-30

### Added

- **Schemas**: Singleton schemas (can only have single content)

### Fixed 

- **Contents**: Ensure that the content api returns content in correct order when querying by ids.
- **UI**: Nested fields got wrong ids and names and could not be saved.

## [v1.7.0] - 2018-06-25

- Migration to .NET Core 2.1

## [v1.6.2] - 2018-06-23

### Added

- **Migration**: Disable event handlers during migration.
- **Migration**: Increased performance.
- **UI**: Better sortable with improved UX.

### Fixed 

- **Schemas**: Invariant name handling for field names.

## [v1.6.1] - 2018-06-22

### Fixed 

- **MongoDB**: Fixed date time handling.

## [v1.6.0] - 2018-06-07

### Added

- **Schemas**: Nested Schemas.
- **UI**: Migration to Angular6.
- **UI**: Migration to RxJS6.

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

- **Actions**: Algolia action.
- **Actions**: Azure Queue action.
- **Actions**: Fastly action.
- **Actions**: New log formatter with placeholder for user infos.
- **Backup**: Backup all your data to an archive.
- **UI**: Big refactoring and UI improvements.

## [v1.3.0] - 2018-02-17

### Added

- **Actions**: ElasticSearch action.

### Changed 

- **DomainObjects**: Refactored domain objects to be ready for Orleans.

## [v1.2.0] - 2018-02-10

### Added

- **Contents**: Updated to state can be scheduled, e.g. to publish them.
- **EventStore**: Event metadata are stored as json objects in MongoDB now and you can query by metadata.

> This releases will run a migration, which might take a while and also effects the events. We recommend to make a backup first.

## [v1.1.7] - 2018-02-06

### Fixed 

- **UI**: Checkbox style fixed.

## [v1.1.6] - 2018-02-06

### Added

- **Apps**: Added a ready to use blog sample.
- **Rules**: Allow content triggers to catch all content events.
- **Rules**: Ensure that the events for an aggregate are handled sequentially.
- **UI**: Better UI for apps overview.
- **UI**: History stream in the dashboard.

### Fixed 

- **UI**: A lot of style fixes. 
- **UI**: History UI was throwing an exception when a user was referenced in the message.

## [v1.1.5] - 2018-02-03

### Added

- **Contents**: Slugify function for custom scripts.

### Fixed 

- **Assets**: OData queries did not work at all and included too many fields (e.g. AppId, Id).
- **Contents**: OData queries only worked for data fields.
- **Migration**: Assets and schemas were not removed before recreation.

## [v1.1.4] - 2018-02-03

### Added

- **Login**: Consent screen to inform the user about privacy policies.

## [v1.1.3] - 2018-02-03

### Added

- **Rules**: Action to purge cache items in fastly
- **Rules**: Action to push events to Azure storage queues.
- **Rules**: Trigger when asset has changed

### Fixed 

- **Rules**: Layout fixes.

### Changed 

- Fetch derived types automatically for Swagger generation.
- Freeze action, triggers and field properties using Fody.

## [v1.1.2] - 2018-01-31

### Added

- **Assets**: OData support, except full text search (`$search`)
- **Rules**: Algolia action
- **Rules**: Slack action

### Bugfixes

- **Rules**: Color corrections for actions. 

### Changed

- Asset structure has changed: Migration will update the collection automatically.
- Asset endpoint:
    - `take` query parameter renamed to `$top`  for OData compatibility.
    - `skip` query parameter renamed to `$skip` for OData compatibility.
    - `query` query parameter replaced with OData. Use `$query=contains(fileName, 'MyQuery')` instead.
