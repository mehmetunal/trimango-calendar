using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TrimangoCalendar.API.Swagger;

public class SwaggerDefaultResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Responses ??= new OpenApiResponses();

        AddIfMissing(operation, "200", "Başarılı işlem sonucu", typeof(SwaggerSuccessResponse));
        AddIfMissing(operation, "400", "Geçersiz istek veya doğrulama hatası", typeof(SwaggerErrorResponse));
        AddIfMissing(operation, "401", "Yetkisiz erişim", typeof(SwaggerErrorResponse));
        AddIfMissing(operation, "403", "Bu işlem için yetkiniz yok", typeof(SwaggerErrorResponse));
        AddIfMissing(operation, "404", "Kayıt bulunamadı", typeof(SwaggerErrorResponse));
        AddIfMissing(operation, "500", "Sunucu tarafında beklenmeyen hata", typeof(SwaggerErrorResponse));
        void AddIfMissing(OpenApiOperation op, string statusCode, string description, Type schemaType)
        {
            var schema = context.SchemaGenerator.GenerateSchema(schemaType, context.SchemaRepository);
            if (!op.Responses.TryGetValue(statusCode, out var response))
            {
                response = new OpenApiResponse();
                op.Responses[statusCode] = response;
            }

            response.Description = string.IsNullOrWhiteSpace(response.Description)
                ? description
                : response.Description;

            response.Content ??= new Dictionary<string, OpenApiMediaType>();
            if (!response.Content.TryGetValue("application/json", out var mediaType))
            {
                mediaType = new OpenApiMediaType();
                response.Content["application/json"] = mediaType;
            }

            // Method seviyesinde açıkça belirtilmiş şema varsa üzerine yazmayalım.
            if (mediaType.Schema == null)
            {
                mediaType.Schema = schema;
            }

        }
    }
}

public class SwaggerSuccessResponse
{
    public bool Success { get; set; }
    public object? Data { get; set; }
    public string? Message { get; set; }
}

public class SwaggerErrorResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}
