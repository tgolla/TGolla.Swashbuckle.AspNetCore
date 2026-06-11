# TGolla.Swashbuckle.AspNetCore

This repository contains the following Swashbuckle addons which are part of the NuGet package [TGolla.Swashbuckle.AspNetCore](https://www.nuget.org/packages/TGolla.Swashbuckle.AspNetCore). 

- [Custom Ordering of Controllers](https://github.com/tgolla/TGolla.Swashbuckle.AspNetCore/blob/main/TGolla.Swashbuckle.AspNetCore/2021-9-16-swashbucklecustom-ordering-of-controllers.md)
- [Adding Authentication/Authorization Information to Swagger API Documentation with Swashbuckle](https://github.com/tgolla/TGolla.Swashbuckle.AspNetCore/blob/main/TGolla.Swashbuckle.AspNetCore/2023-4-20-adding-authentication-authorization-information-to-swagger-api-documentation-with-swashbuckle.md)

## .NET 10 Upgrade Notes

Starting with .NET 9, ASP.NET Core no longer includes Swagger by default in web API templates. And with .NET 10, Microsoft has doubled down on native OpenAPI support - now generating OpenAPI 3.1 documents out of the box with improved transformer APIs and better tooling. If you’re wondering what changed, whether Swagger is truly dead (spoiler: it’s not), and what the best alternatives are, keep reading to find out! (Ref: [ASP.NET Core Dropped Swagger - Here’s What Replaced It in .NET 10 - codewithmukesh](https://codewithmukesh.com/blog/dotnet-swagger-alternatives-openapi/))

With the release .NET 10 Microsoft made breaking changes to `Microsoft.OpenApi` resulting in changes in Swashbuckle document generation. The following is a check list of changes you will need to contline using Swashbuckle along with `TGolla.Swashbuckle.AspNetCore` and `TGolla.AspNetCore.Mvc.Filter`.

1.  Upgrade your Swashbuckle packges to v10.*.
2. Upgrade packages TGolla.Swashbuckle.AspNetCore` and `TGolla.AspNetCore.Mvc.Filter` to v10.*.
3. Remove `builder.Services.AddEndpointsApiExplorer();` form `Program.cs`. in .NET 10 Swashbuckle v10 no longer uses or cooperates with the built‑in Endpoints API Explorer. Leaving it in causes duplicate or conflicting OpenAPI metadata, and Swashbuckle v10 replaces that entire pipeline with its own.
4. In `Program.cs` change `app.UseSwagger();` to `app.MapSwagger();`.  .NET 8+ introduced a new endpoint‑based middleware model, and Swashbuckle v10 adopted it. `UseSwagger()` is part of the old middleware pipeline. `MapSwagger()` is the new endpoint‑based pipeline that aligns with .NET 10’s hosting model.5
5. if to have invoking the ```AddSecurityDefinition``` method you also need to indicate which operations that scheme is applicable to. As a result you will need to update any `AddSecurityRequirement()` and `OperationFilter<AddSecurityRequirement>()` implemented by `AddSwaggerGen()`. Reference [Adding Authentication/Authorization Information to Swagger API Documentation with Swashbuckle](https://github.com/tgolla/TGolla.Swashbuckle.AspNetCore/blob/main/TGolla.Swashbuckle.AspNetCore/2023-4-20-adding-authentication-authorization-information-to-swagger-api-documentation-with-swashbuckle.md) for complete details.
