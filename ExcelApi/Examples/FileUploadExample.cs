using ExcelApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace ExcelApi.Examples;

/// <summary>
///     Example provider for file upload requests in Swagger
/// </summary>
public class FileUploadExample : IExamplesProvider<FileUploadModel>
{
    /// <summary>
    ///     Gets the example model for Swagger documentation
    /// </summary>
    /// <returns>Example FileUploadModel</returns>
    public FileUploadModel GetExamples()
    {
        return new FileUploadModel
        {
            File = null,
            Password = "optional_password"
        };
    }
}