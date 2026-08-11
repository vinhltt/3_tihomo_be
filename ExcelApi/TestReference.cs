namespace ExcelApi.Test;

public class TestClass
{
    public void TestMethod()
    {
        // Test if we can access the local correlation service
        var service = new LocalCorrelationContextService();
        var correlationId = service.GenerateCorrelationId();
    }
}