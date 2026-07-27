using Application.DTOs;
using Application.Interfaces;
using Application.Mapping;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Moq;
using Xunit;

namespace Application.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock = new();
    private readonly IMapper _mapper;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _sut = new ProductService(_repositoryMock.Object, _mapper);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsProduct_WhenProductExists()
    {
        var product = new Product { Id = 1, ProductName = "Widget", CreatedBy = "tester", CreatedOn = DateTime.UtcNow };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await _sut.GetByIdAsync(1);

        Assert.Equal("Widget", result.ProductName);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundException_WhenProductDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(99));
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedProduct_WithGeneratedId()
    {
        var dto = new CreateProductDto { ProductName = "New Product", CreatedBy = "tester" };
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) => { p.Id = 42; return p; });

        var result = await _sut.CreateAsync(dto);

        Assert.Equal(42, result.Id);
        Assert.Equal("New Product", result.ProductName);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenProductDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(5));
    }

    [Fact]
    public async Task GetAllAsync_DefaultsInvalidPageSize_To20()
    {
        _repositoryMock
            .Setup(r => r.GetAllAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Product>(), 0));

        var result = await _sut.GetAllAsync(pageNumber: 0, pageSize: 500);

        Assert.Equal(1, result.PageNumber);
        Assert.Equal(20, result.PageSize);
    }
}
