The squidex API the OData url convention to query data. 

We support the following query options.

* **$top**: The $top query option requests the number of items in the queried collection to be included in the result. The default value is 20 and the maximum allowed value is 200.
* **$skip**: The $skip query option requests the number of items in the queried collection that are to be skipped and not included in the result. Use it together with $top to read the all your data page by page. 
* **$search**: The $search query option allows clients to request entities matching a free-text search expression. We add the data of all fields for all languages to a single field in the database and use this combined field to implement the full text search.
* **$filter**: The $filter query option allows clients to filter a collection of resources that are addressed by a request URL.
* **$orderby**: The $orderby query option allows clients to request resources in a particular order.

Read more about it at: https://docs.squidex.io/04-guides/02-api.html
