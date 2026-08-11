namespace Identity.Api.Configuration;

/// <summary>
/// OpenTelemetry configuration settings
/// Cấu hình OpenTelemetry settings
/// </summary>
public class OtelSettings
{
    /// <summary>
    /// The service name to identify traces and metrics
    /// Tên service để định danh traces và metrics
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// The service version
    /// Phiên bản service
    /// </summary>
    public string ServiceVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Resource attributes in the format "key1=value1,key2=value2"
    /// Các thuộc tính resource theo định dạng "key1=value1,key2=value2"
    /// </summary>
    public string ResourceAttributes { get; set; } = string.Empty;

    /// <summary>
    /// OTLP exporter endpoint for traces
    /// Endpoint OTLP exporter cho traces
    /// </summary>
    public string ExporterOtlpEndpoint { get; set; } = "http://tempo:4317";

    /// <summary>
    /// Traces sampler type (e.g., "always_on", "always_off", "traceidratio", "parentbased_always_on")
    /// Loại traces sampler (ví dụ: "always_on", "always_off", "traceidratio", "parentbased_always_on")
    /// </summary>
    public string TracesSampler { get; set; } = "traceidratio";

    /// <summary>
    /// Traces sampler argument (for traceidratio, this is the sampling ratio 0.0-1.0)
    /// Tham số traces sampler (cho traceidratio, đây là tỷ lệ sampling từ 0.0-1.0)
    /// </summary>
    public string TracesSamplerArg { get; set; } = "1.0";

    /// <summary>
    /// Parse sampling ratio as double
    /// Parse tỷ lệ sampling thành double
    /// </summary>
    public double GetSamplingRatio()
    {
        if (double.TryParse(TracesSamplerArg, out var ratio))
        {
            return Math.Max(0.0, Math.Min(1.0, ratio)); // Clamp between 0.0 and 1.0
        }
        return 1.0; // Default to 100% sampling if parsing fails
    }

    /// <summary>
    /// Parse resource attributes into a dictionary
    /// Parse resource attributes thành dictionary
    /// </summary>
    public Dictionary<string, object> GetResourceAttributesDictionary()
    {
        var attributes = new Dictionary<string, object>();
        
        if (string.IsNullOrWhiteSpace(ResourceAttributes))
            return attributes;

        var pairs = ResourceAttributes.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
            if (keyValue.Length == 2)
            {
                attributes[keyValue[0].Trim()] = keyValue[1].Trim();
            }
        }

        return attributes;
    }
}