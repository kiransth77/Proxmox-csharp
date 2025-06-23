using Xunit;
using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using ProxmoxApi.Core;
using ProxmoxApi.Models;
using ProxmoxApi.Exceptions;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ProxmoxApi.Tests.Core;

public class ProxmoxHttpClientTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<ILogger<ProxmoxHttpClient>> _mockLogger;
    private readonly HttpClient _httpClient;
    private readonly ProxmoxConnectionInfo _connectionInfo;
    private readonly ProxmoxHttpClient _proxmoxHttpClient;

    public ProxmoxHttpClientTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockLogger = new Mock<ILogger<ProxmoxHttpClient>>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        
        _connectionInfo = new ProxmoxConnectionInfo
        {
            Host = "test.proxmox.com",
            Port = 8006,
            Username = "testuser",
            Password = "testpass",
            Realm = "pve"
        };

        _proxmoxHttpClient = new ProxmoxHttpClient(_httpClient, _connectionInfo, _mockLogger.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_SetsAuthTicket_WhenSuccessful()
    {
        // Arrange
        var authResponse = new
        {
            data = new
            {
                ticket = "test-ticket",
                CSRFPreventionToken = "test-csrf-token"
            }
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(authResponse), Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        await _proxmoxHttpClient.AuthenticateAsync();

        // Assert
        _mockHttpMessageHandler
            .Protected()
            .Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri!.ToString().Contains("/access/ticket")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task AuthenticateAsync_ThrowsException_WhenAuthenticationFails()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("Unauthorized", Encoding.UTF8, "text/plain")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);        // Act & Assert
        await Assert.ThrowsAsync<ProxmoxAuthenticationException>(() => 
            _proxmoxHttpClient.AuthenticateAsync());
    }

    [Fact]
    public async Task GetAsync_ReturnsDeserializedResponse_WhenSuccessful()
    {
        // Arrange
        var testData = new { message = "test data" };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(testData), Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _proxmoxHttpClient.GetAsync<object>("test/endpoint");

        // Assert
        Assert.NotNull(result);
        _mockHttpMessageHandler
            .Protected()
            .Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString().Contains("test/endpoint")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task PostAsync_SendsDataAndReturnsResponse_WhenSuccessful()
    {
        // Arrange
        var postData = new { key = "value" };
        var responseData = new { success = true };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseData), Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _proxmoxHttpClient.PostAsync<object>("test/endpoint", postData);

        // Assert
        Assert.NotNull(result);
        _mockHttpMessageHandler
            .Protected()
            .Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri!.ToString().Contains("test/endpoint")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task PutAsync_SendsDataAndReturnsResponse_WhenSuccessful()
    {
        // Arrange
        var putData = new { key = "updated_value" };
        var responseData = new { success = true };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseData), Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _proxmoxHttpClient.PutAsync<object>("test/endpoint", putData);

        // Assert
        Assert.NotNull(result);
        _mockHttpMessageHandler
            .Protected()
            .Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Put && 
                    req.RequestUri!.ToString().Contains("test/endpoint")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ReturnsResponse_WhenSuccessful()
    {
        // Arrange
        var responseData = new { success = true };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(responseData), Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _proxmoxHttpClient.DeleteAsync<object>("test/endpoint");

        // Assert
        Assert.NotNull(result);
        _mockHttpMessageHandler
            .Protected()
            .Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Delete && 
                    req.RequestUri!.ToString().Contains("test/endpoint")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task HttpMethods_ThrowProxmoxApiException_WhenHttpErrorOccurs(HttpStatusCode statusCode)
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent("Error occurred", Encoding.UTF8, "text/plain")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        await Assert.ThrowsAsync<ProxmoxApiException>(() => 
            _proxmoxHttpClient.GetAsync<object>("test/endpoint"));
            
        await Assert.ThrowsAsync<ProxmoxApiException>(() => 
            _proxmoxHttpClient.PostAsync<object>("test/endpoint", new { }));
            
        await Assert.ThrowsAsync<ProxmoxApiException>(() => 
            _proxmoxHttpClient.PutAsync<object>("test/endpoint", new { }));
            
        await Assert.ThrowsAsync<ProxmoxApiException>(() => 
            _proxmoxHttpClient.DeleteAsync<object>("test/endpoint"));
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
