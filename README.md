Swashbuckle
=========
Seamlessly adds a Swagger to WebApi projects! Uses a combination of ApiExplorer and Swagger/Swagger-UI to provide a rich discovery and documentation experience for consumers.

Getting Started
--------------------

To start exposing auto-generated Swagger docs and Swagger UI, simply install the Nuget package from your WebApi project:

<pre>
	<code>
	Install-Package Swashbuckle.WebApi
	</code>
</pre>

This contains an embedded "swagger" Area that is registered at application startup. The Area exposes routes for the raw Swagger spec and a corresponding Swagger UI:

swagger/api-docs
swagger/ui

NOTE: The Swagger spec groups endpoints by resource (Resource Listing). When generating the Swagger spec, Swashbuckle creates this grouping by controller name. This affects the way API's are grouped in the UI. For the majority of cases, where the controller-per-resource convention is used, this amounts to the same thing. For other cases, it's just worth noting that the grouping may not correspond exactly to the resource. 

Extensibility
--------------------

Swashbuckle automtically generates a Swagger spec and UI based off the WebApi ApiExplorer. The out-of-the-box generator caters for the majority of WebApi implementations but also includes some extensibility points for application-specific needs.

## OperationSpecFilters

TODO: Describe

## Javascripts for modifying the UI

TODO: Describe