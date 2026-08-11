using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Tests.Shared.Helpers;

/// <summary>
///     Shared helper for API Key authentication across different test projects (EN)<br/>
///     Shared helper cho API Key authentication qua các test projects khác nhau (VI)
/// </summary>
public static class ApiKeyAuthenticationHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    #region HTTP Client Configuration

    /// <summary>
    ///     Configure HTTP client with API Key authentication (EN)<br/>
    ///     Cấu hình HTTP client với API Key authentication (VI)
    /// </summary>
    public static void SetApiKeyAuthentication(this HttpClient client, string apiKey)
    {
        client.DefaultRequestHeaders.Remove("X-API-Key");
        client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    }

    /// <summary>
    ///     Configure HTTP client with JWT authentication (EN)<br/>
    ///     Cấu hình HTTP client với JWT authentication (VI)
    /// </summary>
    public static void SetJwtAuthentication(this HttpClient client, string jwtToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
    }

    /// <summary>
    ///     Clear all authentication headers (EN)<br/>
    ///     Xóa tất cả authentication headers (VI)
    /// </summary>
    public static void ClearAuthentication(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
        client.DefaultRequestHeaders.Remove("X-API-Key");
    }

    /// <summary>
    ///     Set both JWT and API Key authentication (EN)<br/>
    ///     Set cả JWT và API Key authentication (VI)
    /// </summary>
    public static void SetDualAuthentication(this HttpClient client, string jwtToken, string apiKey)
    {
        client.SetJwtAuthentication(jwtToken);
        client.SetApiKeyAuthentication(apiKey);
    }

    #endregion

    #region HTTP Content Helpers

    /// <summary>
    ///     Create JSON content from object (EN)<br/>
    ///     Tạo JSON content từ object (VI)
    /// </summary>
    public static StringContent CreateJsonContent<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj, JsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    /// <summary>
    ///     Create form data content for CRUD operations (EN)<br/>
    ///     Tạo form data content cho CRUD operations (VI)
    /// </summary>
    public static MultipartFormDataContent CreateFormContent(Dictionary<string, string> fields)
    {
        var formData = new MultipartFormDataContent();
        
        foreach (var field in fields)
        {
            if (!string.IsNullOrEmpty(field.Value))
            {
                formData.Add(new StringContent(field.Value), field.Key);
            }
        }
        
        return formData;
    }

    #endregion

    #region Response Helpers

    /// <summary>
    ///     Deserialize HTTP response content (EN)<br/>
    ///     Deserialize nội dung HTTP response (VI)
    /// </summary>
    public static async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        
        if (string.IsNullOrEmpty(content))
            return default(T);
            
        try
        {
            return JsonSerializer.Deserialize<T>(content, JsonOptions);
        }
        catch (JsonException)
        {
            // If deserialization fails, return default
            return default(T);
        }
    }

    /// <summary>
    ///     Get response content as string (EN)<br/>
    ///     Lấy response content dưới dạng string (VI)
    /// </summary>
    public static async Task<string> GetResponseContentAsync(HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    ///     Check if response contains error message (EN)<br/>
    ///     Kiểm tra response có chứa error message không (VI)
    /// </summary>
    public static async Task<bool> ContainsErrorAsync(HttpResponseMessage response, string errorMessage)
    {
        var content = await GetResponseContentAsync(response);
        return content.Contains(errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Test Data Generation

    /// <summary>
    ///     Generate test API key with valid format (EN)<br/>
    ///     Tạo test API key với format hợp lệ (VI)
    /// </summary>
    public static string GenerateTestApiKey(string suffix = "")
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var random = new Random().Next(1000, 9999);
        return $"tihomo_test_{timestamp}_{random}{suffix}";
    }

    /// <summary>
    ///     Generate invalid API key for negative testing (EN)<br/>
    ///     Tạo invalid API key cho negative testing (VI)
    /// </summary>
    public static string GenerateInvalidApiKey()
    {
        return "invalid_api_key_" + Guid.CreateVersion7().ToString("N")[..16];
    }

    /// <summary>
    ///     Generate test email for user creation (EN)<br/>
    ///     Tạo test email cho việc tạo user (VI)
    /// </summary>
    public static string GenerateTestEmail(string prefix = "testuser")
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return $"{prefix}_{timestamp}@tihomo.local";
    }

    /// <summary>
    ///     Generate test username (EN)<br/>
    ///     Tạo test username (VI)
    /// </summary>
    public static string GenerateTestUsername(string prefix = "testuser")
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return $"{prefix}_{timestamp}";
    }

    #endregion

    #region Validation Helpers

    /// <summary>
    ///     Validate API key format (EN)<br/>
    ///     Validate format của API key (VI)
    /// </summary>
    public static bool IsValidApiKeyFormat(string? apiKey)
    {
        return !string.IsNullOrEmpty(apiKey) && 
               apiKey.StartsWith("tihomo_") && 
               apiKey.Length >= 40;
    }

    /// <summary>
    ///     Validate JWT token format (EN)<br/>
    ///     Validate format của JWT token (VI)
    /// </summary>
    public static bool IsValidJwtFormat(string? jwtToken)
    {
        return !string.IsNullOrEmpty(jwtToken) && 
               jwtToken.Split('.').Length == 3;
    }

    /// <summary>
    ///     Validate email format (EN)<br/>
    ///     Validate format của email (VI)
    /// </summary>
    public static bool IsValidEmailFormat(string? email)
    {
        return !string.IsNullOrEmpty(email) && 
               email.Contains("@") && 
               email.Contains(".");
    }

    /// <summary>
    ///     Validate GUID format (EN)<br/>
    ///     Validate format của GUID (VI)
    /// </summary>
    public static bool IsValidGuidFormat(string? guidString)
    {
        return Guid.TryParse(guidString, out _);
    }

    #endregion

    #region Assertion Helpers

    /// <summary>
    ///     Assert that response is successful (EN)<br/>
    ///     Assert rằng response là successful (VI)
    /// </summary>
    public static void AssertSuccessResponse(HttpResponseMessage response, ILogger? logger = null)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = $"Expected successful response but got {response.StatusCode}";
            logger?.LogError(errorMessage);
            throw new Exception(errorMessage);
        }
    }

    /// <summary>
    ///     Assert that response has specific status code (EN)<br/>
    ///     Assert rằng response có status code cụ thể (VI)
    /// </summary>
    public static void AssertStatusCode(HttpResponseMessage response, System.Net.HttpStatusCode expectedStatusCode, ILogger? logger = null)
    {
        if (response.StatusCode != expectedStatusCode)
        {
            var errorMessage = $"Expected status code {expectedStatusCode} but got {response.StatusCode}";
            logger?.LogError(errorMessage);
            throw new Exception(errorMessage);
        }
    }

    /// <summary>
    ///     Assert that response contains specific content (EN)<br/>
    ///     Assert rằng response chứa content cụ thể (VI)
    /// </summary>
    public static async Task AssertResponseContainsAsync(HttpResponseMessage response, string expectedContent, ILogger? logger = null)
    {
        var content = await GetResponseContentAsync(response);
        
        if (!content.Contains(expectedContent))
        {
            var errorMessage = $"Response does not contain expected content: {expectedContent}";
            logger?.LogError(errorMessage);
            throw new Exception(errorMessage);
        }
    }

    #endregion

    #region Retry and Wait Helpers

    /// <summary>
    ///     Retry HTTP request with exponential backoff (EN)<br/>
    ///     Retry HTTP request với exponential backoff (VI)
    /// </summary>
    public static async Task<HttpResponseMessage> RetryRequestAsync(
        Func<Task<HttpResponseMessage>> requestFunc,
        int maxRetries = 3,
        TimeSpan? initialDelay = null,
        ILogger? logger = null)
    {
        var delay = initialDelay ?? TimeSpan.FromMilliseconds(100);
        
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await requestFunc();
                
                if (response.IsSuccessStatusCode || attempt == maxRetries)
                {
                    return response;
                }
                
                logger?.LogWarning("Request failed with status {StatusCode}, retrying in {Delay}ms (attempt {Attempt}/{MaxRetries})", 
                    response.StatusCode, delay.TotalMilliseconds, attempt + 1, maxRetries + 1);
                
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); // Exponential backoff
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                logger?.LogWarning(ex, "Request failed with exception, retrying in {Delay}ms (attempt {Attempt}/{MaxRetries})", 
                    delay.TotalMilliseconds, attempt + 1, maxRetries + 1);
                
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }
        
        throw new Exception($"Request failed after {maxRetries + 1} attempts");
    }

    /// <summary>
    ///     Wait for condition to be true with timeout (EN)<br/>
    ///     Đợi condition đúng với timeout (VI)
    /// </summary>
    public static async Task<bool> WaitForConditionAsync(
        Func<Task<bool>> conditionFunc,
        TimeSpan timeout,
        TimeSpan? checkInterval = null,
        ILogger? logger = null)
    {
        var interval = checkInterval ?? TimeSpan.FromMilliseconds(100);
        var endTime = DateTime.UtcNow.Add(timeout);
        
        while (DateTime.UtcNow < endTime)
        {
            try
            {
                if (await conditionFunc())
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger?.LogDebug(ex, "Condition check failed, continuing to wait");
            }
            
            await Task.Delay(interval);
        }
        
        logger?.LogWarning("Condition was not met within timeout of {Timeout}", timeout);
        return false;
    }

    #endregion
}