using System.Net;
using System.Net.Http.Headers;
using CoreFinance.Contracts.Messages;
using Tests.Shared.Helpers;

namespace CoreFinance.Api.Tests.Integration;

/// <summary>
///     Characterization: real XLSX -> ExcelApi -> RabbitMQ -> CoreFinance consumer -> result message (EN)<br/>
///     Characterization: XLSX thật -> ExcelApi -> RabbitMQ -> consumer CoreFinance -> message kết quả (VI)
/// </summary>
/// <remarks>The consumer validates and counts only; this does not claim imported transactions are stored.</remarks>
[Trait("Category", "Characterization")]
public class ExcelMessageParityTests
{
    [Fact]
    public async Task Xlsx_Upload_Should_Reach_Consumer_And_Publish_Validation_Result()
    {
        await using var published = await RabbitMqResultObserver<UploadTransactionDataMessage>.StartAsync();
        await using var results = await RabbitMqResultObserver<TransactionProcessedMessage>.StartAsync();
        var fileName = $"characterization-{Guid.CreateVersion7():N}.xlsx";
        var xlsx = XlsxFixtureBuilder.Build([
            ["Date", "Description", "Amount", "Reference"],
            ["2026-10-01", "Coffee shop", "-45000", "REF001"],
            ["2026-10-02", "Salary", "15000000", "REF002"],
            ["not-a-date", "Bad row", "abc", "REF003"],
            ["Total Debit Transaction", "", "", ""]
        ]);
        using var excel = ExternalBackend.Client("excel");
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(xlsx);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        form.Add(file, "File", fileName);
        form.Add(new StringContent("0"), "HeaderRowIndex");

        var response = await excel.PostAsync("/api/Excel/extract", form);

        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        var upload = await published.WaitForAsync(m => m.FileName == fileName, TimeSpan.FromSeconds(30));
        upload.TransactionData.Should().HaveCount(3, "rows stop at the end marker");
        upload.TransactionData[0].Description.Should().Be("Coffee shop");
        upload.TransactionData[0].Amount.Should().Be(-45000m);
        upload.TransactionData[0].Reference.Should().Be("REF001");

        var result = await results.WaitForAsync(m => m.CorrelationId == upload.CorrelationId, TimeSpan.FromSeconds(30));
        result.ProcessedCount.Should().Be(2);
        result.FailedCount.Should().Be(1);
        result.Success.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Contain("Bad row").And.Contain("amount cannot be zero");
    }
}
