Traditionally the Squidex API uses OData for Filtering and Searching.

We support the following query options.

* **$top**: The $top query option requests the number of items in the queried collection to be included in the result. The default value is 20 and the maximum allowed value is 200. You can change the maximum in the app settings, when you host Squidex yourself.
* **$skip**: The $skip query option requests the number of items in the queried collection that are to be skipped and not included in the result. Use it together with $top to read the all your data page by page. 
* **$search**: The $search query option allows clients to request entities matching a free-text search expression. We add the data of all fields for all languages to our full text engine.
* **$filter**: The $filter query option allows clients to filter a collection of resources that are addressed by a request URL.
* **$orderby**: The $orderby query option allows clients to request resources in a particular order.

As an alternative you can also use the following query option:

* **q**: A json text that represents the query options above and has the same capabilities but is more performant.

Furthermore you can also query items by ids with the following query option:

* **ids**: A comma-separated list of ids. If you define this option all other settings are ignored.

Read more about it at: https://docs.squidex.io/04-guides/02-api.html
