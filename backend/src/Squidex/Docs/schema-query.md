How to make queries?

Read more about it at: https://docs.squidex.io/04-guides/02-api.html

The query endpoints support three options:

### Query with OData

Squidex supports a subset of the OData (https://www.odata.org/) syntax with with the following query options:

* **$top**: The $top query option requests the number of items in the queried collection to be included in the result. The default value is 20 and the maximum allowed value is 200. You can change the maximum in the app settings, when you host Squidex yourself.
* **$skip**: The $skip query option requests the number of items in the queried collection that are to be skipped and not included in the result. Use it together with $top to read the all your data page by page. 
* **$search**: The $search query option allows clients to request entities matching a free-text search expression. We add the data of all fields for all languages to our full text engine.
* **$filter**: The $filter query option allows clients to filter a collection of resources that are addressed by a request URL.
* **$orderby**: The $orderby query option allows clients to request resources in a particular order.

### Query with JSON query

Squidex also supports a query syntax based on JSON. You have to pass in the query object as query parameter:

* **q**: A json text that represents the same query options as with OData, but is more performant to parse.

### Query by IDs

Query your items by passing in one or many IDs with the following query parameter:

* **ids**: A comma-separated list of ids. If you define this option all other settings are ignored.
