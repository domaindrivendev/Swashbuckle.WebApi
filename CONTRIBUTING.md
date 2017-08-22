Contributing to Swashbuckle
=========

Contributions to Swashbuckle are welcomed in the form of construcutive, reproducible bug reports, feature requests that align with the project's goals, or better still a PR that's accompanied with passing tests.

If you have general questions or feedback around the use of Swashbuckle, __PLEASE POST THEM ON STACKOVERFLOW INSTEAD OF GITHUB.__

## Bug Reports ##

If you're reporting a bug, please include a clear description of the issue, the version of Swashbuckle you're using, and a simple set of repro steps.

Please remember that Swashbuckle is a free and open-source project provided to the community with zero financial gain to the author(s). Any issues deemed to have a negative or arrogant tone will be closed without response.

## Feature Requests ##

Fundamentally, Swashbuckle is a library that attempts to generate an accurate description of your API, using [Swagger 2.0](https://swagger.io/docs/specification/2-0/basic-structure/), according to the routes, controllers and models that you've implemented. So, the resulting API documentation is driven by "actual" behavior as opposed to "intended" behavior. This is an important distinction to consider when submitting feature requests. For example, a feature that leverages built-in attributes (e.g. AuthorizeAttribute, RequiredAttribute etc.) would be more aligned to the project goals than one that introduces custom, documentation-specific attributes that have no impact on actual API behavior.

It's also worth noting that Swashbuckle ships with an embedded version of the [swagger-ui](swagger-ui), providing a powerful documentation solution when combined with the auto-generated Swagger, but is not responsible for the development of that library and therefore any UI-specific features.

Feel free to submit feature requests but please keep these constraints in mind when doing so.

## Pull Requests ##

If you've identified a feature/bug fix that aligns to the project goals, or even just an addition to the docs, please submit a Pull Request (PR). If applicable, include tests and ensure all tests are passing locally before you commit.

Once you clone the repo, development should be straightforward bar one gotcha: the swagger-ui assets are pulled down as a git submodule and incorporated into the library as "embedded resources" during build-time. So, before building, you'll need to inititiate the submodule:

```
git submodule update --init
```