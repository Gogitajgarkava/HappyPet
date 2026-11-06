using System.Text.Json;
using HappyPet.Application.Exceptions;
using HappyPet.WebAPI.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace HappyPet.Tests.ErrorHandling;

public class ApiExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WithBusinessRuleViolation_Returns400AndCode()
    {
        var context = CreateHttpContext();

        var handled = await CreateHandler().TryHandleAsync(
            context,
            new InvalidOperationException("ცხოველი უკვე რეგისტრირებულია."),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        var body = await ReadBodyAsync(context);
        Assert.Equal("BUSINESS_RULE_VIOLATION", body.GetProperty("code").GetString());
        Assert.Equal("ცხოველი უკვე რეგისტრირებულია.", body.GetProperty("message").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_WithNotFound_Returns404()
    {
        var context = CreateHttpContext();

        await CreateHandler().TryHandleAsync(
            context,
            new KeyNotFoundException("აღნიშნული ჩანაწერი ვერ მოიძებნა."),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        var body = await ReadBodyAsync(context);
        Assert.Equal("NOT_FOUND", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_WithAirsUnavailable_Returns503()
    {
        var context = CreateHttpContext();

        await CreateHandler().TryHandleAsync(
            context,
            new AirsUnavailableException("AIRS მიუწვდომელია."),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        var body = await ReadBodyAsync(context);
        Assert.Equal("AIRS_UNAVAILABLE", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_WithUnexpectedException_Returns500WithoutInternalDetails()
    {
        var context = CreateHttpContext();

        await CreateHandler().TryHandleAsync(
            context,
            new Exception("sensitive database connection string"),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        var body = await ReadBodyAsync(context);
        Assert.Equal("INTERNAL_ERROR", body.GetProperty("code").GetString());
        Assert.DoesNotContain("sensitive database connection string", body.GetRawText());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task TryHandleAsync_WithEmailNotVerified_Returns403()
    {
        var context = CreateHttpContext();

        await CreateHandler().TryHandleAsync(
            context,
            new EmailNotVerifiedException(),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        var body = await ReadBodyAsync(context);
        Assert.Equal("EMAIL_NOT_VERIFIED", body.GetProperty("code").GetString());
    }

    private static ApiExceptionHandler CreateHandler()
    {
        return new ApiExceptionHandler(NullLogger<ApiExceptionHandler>.Instance);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<JsonElement> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        return JsonDocument.Parse(json).RootElement.Clone();
    }
}
