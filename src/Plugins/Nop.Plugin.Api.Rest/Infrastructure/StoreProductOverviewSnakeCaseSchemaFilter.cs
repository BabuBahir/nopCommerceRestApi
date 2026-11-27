using System;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Nop.Plugin.Api.Rest.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nop.Plugin.Api.Rest.Infrastructure;

public sealed class StoreProductOverviewSnakeCaseSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.Namespace != typeof(StoreProductOverviewDto).Namespace)
            return;

        foreach (var property in context.Type.GetProperties())
        {
            var jsonName = property.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName;
            if (string.IsNullOrEmpty(jsonName))
                continue;

            var schemaName = schema.Properties.Keys.FirstOrDefault(name =>
                string.Equals(name, property.Name, StringComparison.OrdinalIgnoreCase));
            if (schemaName == null || string.Equals(schemaName, jsonName, StringComparison.Ordinal))
                continue;

            var propertySchema = schema.Properties[schemaName];
            schema.Properties.Remove(schemaName);
            schema.Properties[jsonName] = propertySchema;

            if (schema.Required?.Remove(schemaName) == true)
                schema.Required.Add(jsonName);
        }
    }
}
