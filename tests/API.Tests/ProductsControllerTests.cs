using System.Net;
using System.Net.Http.Json;
using Application.DTOs;
using Xunit;

namespace API.Tests;

public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_CreatesProduct_AndGet_ReturnsIt()
    {
        var createDto = new CreateProductDto { ProductName = "Integration Widget", CreatedBy = "test-suite" };

        var postResponse = await _client.PostAsJsonAsync("/api/v1/products", createDto);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var created = await postResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(created);

        var getResponse = await _client.GetAsync($"/api/v1/products/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.Equal("Integration Widget", fetched!.ProductName);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_ForUnknownId()
    {
        var response = await _client.GetAsync("/api/v1/products/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_ReturnsBadRequest_WhenProductNameMissing()
    {
        var invalidDto = new CreateProductDto { ProductName = "", CreatedBy = "test-suite" };

        var response = await _client.PostAsJsonAsync("/api/v1/products", invalidDto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
