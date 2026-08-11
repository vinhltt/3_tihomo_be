using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CoreFinance.Api.Infrastructures.Swagger.SchemaFilters;

/// <summary>
///     Applies a schema filter to enumerate enum values with their integer representation and name for Swagger
///     documentation.
///     This improves the clarity of API documentation for enum types. (EN)<br />
///     Áp dụng bộ lọc schema để liệt kê các giá trị enum cùng với biểu diễn số nguyên và tên của chúng cho tài liệu
///     Swagger.
///     Điều này cải thiện sự rõ ràng của tài liệu API đối với các kiểu enum. (VI)
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    /// <summary>
    ///     Applies the filter to the specified schema. (EN)<br />
    ///     Áp dụng bộ lọc vào schema được chỉ định. (VI)
    /// </summary>
    /// <param name="schema">
    ///     The schema to apply the filter to. (EN)<br />
    ///     Schema để áp dụng bộ lọc. (VI)
    /// </param>
    /// <param name="context">
    ///     The context for the schema filter. (EN)<br />
    ///     Ngữ cảnh cho bộ lọc schema. (VI)
    /// </param>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum) return;

        schema.Enum.Clear();

        Enum.GetNames(context.Type)
            .ToList()
            .ForEach(name =>
                schema.Enum.Add(
                    new OpenApiString(
                        $"{Convert.ToInt64(Enum.Parse(context.Type, name))} = {name}")));
    }
}