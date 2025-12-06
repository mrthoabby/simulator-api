using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using ProductManagementSystem.Application.AppEntities.GlobalParameters.Repository;
using ProductManagementSystem.Application.AppEntities.GlobalParameters.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.GlobalParameters.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.GlobalParameters.Models;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.Services;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.Shared.Enum;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;
using ProductManagementSystem.Application.AppEntities.Shared.Type;
using ProductManagementSystem.Application.Common.Errors;

namespace ProductManagementSystem.Application.AppEntities.GlobalParameters.Services;

public class GlobalParametersServiceTests
{
    private readonly Mock<IGlobalParametersRepository> _mockRepository;
    private readonly Mock<IConceptCodeService> _mockConceptCodeService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GlobalParametersService _service;

    public GlobalParametersServiceTests()
    {
        _mockRepository = new Mock<IGlobalParametersRepository>();
        _mockConceptCodeService = new Mock<IConceptCodeService>();
        _mockMapper = new Mock<IMapper>();

        _service = new GlobalParametersService(
            _mockRepository.Object,
            _mockConceptCodeService.Object,
            _mockMapper.Object
        );
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidFixedValueData_CallsRepository()
    {
        var request = new AddGlobalParameterDTO
        {
            ConceptCode = "TEST-CODE",
            Name = "Test Parameter",
            Description = "Test Description",
            Application = EnumConceptApplication.AddToProduct,
            Type = EnumConceptType.FixedValue,
            Price = new MoneyDTO { Value = 100m, Currency = EnumCurrency.USD }
        };

        var conceptCodeDto = new ConceptCodeDTO { Code = "TEST-CODE" };
        var globalParameter = new GlobalParameter("TEST-CODE", "Test Parameter", EnumConceptApplication.AddToProduct, Money.Create(100m, EnumCurrency.USD), "Test Description");
        var resultDto = new GlobalParameterDTO { ConceptCode = "TEST-CODE", Name = "Test Parameter" };

        _mockConceptCodeService.Setup(s => s.GetByCodeAsync(request.ConceptCode))
            .ReturnsAsync(conceptCodeDto);
        _mockMapper.Setup(m => m.Map<Money>(request.Price))
            .Returns(Money.Create(100m, EnumCurrency.USD));
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Concept>()))
            .ReturnsAsync(globalParameter);
        _mockMapper.Setup(m => m.Map<GlobalParameterDTO>(It.IsAny<Concept>()))
            .Returns(resultDto);

        var result = await _service.CreateAsync(request);

        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Concept>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithValidPercentageData_CallsRepository()
    {
        var request = new AddGlobalParameterDTO
        {
            ConceptCode = "TEST-CODE",
            Name = "Test Parameter",
            Application = EnumConceptApplication.AddToProduct,
            Type = EnumConceptType.Percentage,
            Percentage = 15.5m
        };

        var conceptCodeDto = new ConceptCodeDTO { Code = "TEST-CODE" };
        var globalParameter = new GlobalParameter("TEST-CODE", "Test Parameter", EnumConceptApplication.AddToProduct, 15.5m, null);
        var resultDto = new GlobalParameterDTO { ConceptCode = "TEST-CODE", Name = "Test Parameter" };

        _mockConceptCodeService.Setup(s => s.GetByCodeAsync(request.ConceptCode))
            .ReturnsAsync(conceptCodeDto);
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Concept>()))
            .ReturnsAsync(globalParameter);
        _mockMapper.Setup(m => m.Map<GlobalParameterDTO>(It.IsAny<Concept>()))
            .Returns(resultDto);

        var result = await _service.CreateAsync(request);

        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Concept>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistingConceptCode_ThrowsNotFoundException()
    {
        var request = new AddGlobalParameterDTO
        {
            ConceptCode = "NON-EXISTING",
            Name = "Test Parameter",
            Application = EnumConceptApplication.AddToProduct,
            Type = EnumConceptType.Percentage,
            Percentage = 10m
        };

        _mockConceptCodeService.Setup(s => s.GetByCodeAsync(request.ConceptCode))
            .ReturnsAsync((ConceptCodeDTO?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WithFixedValueWithoutPrice_ThrowsConflictException()
    {
        var request = new AddGlobalParameterDTO
        {
            ConceptCode = "TEST-CODE",
            Name = "Test Parameter",
            Application = EnumConceptApplication.AddToProduct,
            Type = EnumConceptType.FixedValue,
            Price = null
        };

        var conceptCodeDto = new ConceptCodeDTO { Code = "TEST-CODE" };
        _mockConceptCodeService.Setup(s => s.GetByCodeAsync(request.ConceptCode))
            .ReturnsAsync(conceptCodeDto);

        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WithPercentageWithoutPercentageValue_ThrowsConflictException()
    {
        var request = new AddGlobalParameterDTO
        {
            ConceptCode = "TEST-CODE",
            Name = "Test Parameter",
            Application = EnumConceptApplication.AddToProduct,
            Type = EnumConceptType.Percentage,
            Percentage = null
        };

        var conceptCodeDto = new ConceptCodeDTO { Code = "TEST-CODE" };
        _mockConceptCodeService.Setup(s => s.GetByCodeAsync(request.ConceptCode))
            .ReturnsAsync(conceptCodeDto);

        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(request));
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_CallsRepository()
    {
        var concepts = new List<Concept>();
        var globalParameterDtos = new List<GlobalParameterDTO>();

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(concepts);
        _mockMapper.Setup(m => m.Map<List<GlobalParameterDTO>>(concepts))
            .Returns(globalParameterDtos);

        var result = await _service.GetAllAsync();

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    #endregion

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_WithExistingCode_CallsRepository()
    {
        var conceptCode = "TEST-CODE";
        var globalParameter = new GlobalParameter(conceptCode, "Test", EnumConceptApplication.AddToProduct, 10m, null);
        var globalParameterDto = new GlobalParameterDTO { ConceptCode = conceptCode, Name = "Test" };

        _mockRepository.Setup(r => r.GetAsync(conceptCode))
            .ReturnsAsync(globalParameter);
        _mockMapper.Setup(m => m.Map<GlobalParameterDTO>(globalParameter))
            .Returns(globalParameterDto);

        var result = await _service.GetAsync(conceptCode);

        _mockRepository.Verify(r => r.GetAsync(conceptCode), Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidData_CallsRepository()
    {
        var conceptCode = "TEST-CODE";
        var updateDto = new UpdateGlobalParameterDTO
        {
            Name = "Updated Name",
            Description = "Updated Description"
        };
        var globalParameter = new GlobalParameter(conceptCode, "Test", EnumConceptApplication.AddToProduct, 10m, null);
        var resultDto = new GlobalParameterDTO { ConceptCode = conceptCode, Name = "Updated Name" };

        _mockRepository.Setup(r => r.GetAsync(conceptCode))
            .ReturnsAsync(globalParameter);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Concept>()))
            .ReturnsAsync(globalParameter);
        _mockMapper.Setup(m => m.Map<GlobalParameterDTO>(It.IsAny<Concept>()))
            .Returns(resultDto);

        var result = await _service.UpdateAsync(conceptCode, updateDto);

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Concept>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingCode_ThrowsNotFoundException()
    {
        var conceptCode = "NON-EXISTING";
        var updateDto = new UpdateGlobalParameterDTO { Name = "Updated" };

        _mockRepository.Setup(r => r.GetAsync(conceptCode))
            .ReturnsAsync((Concept?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(conceptCode, updateDto));
    }

    [Fact]
    public async Task UpdateAsync_WithFixedValueTypeAndNoPrice_ThrowsConflictException()
    {
        var conceptCode = "TEST-CODE";
        var updateDto = new UpdateGlobalParameterDTO
        {
            Type = EnumConceptType.FixedValue,
            Price = null
        };
        var globalParameter = new GlobalParameter(conceptCode, "Test", EnumConceptApplication.AddToProduct, 10m, null);

        _mockRepository.Setup(r => r.GetAsync(conceptCode))
            .ReturnsAsync(globalParameter);

        await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateAsync(conceptCode, updateDto));
    }

    [Fact]
    public async Task UpdateAsync_WithPercentageTypeAndNoPercentage_ThrowsConflictException()
    {
        var conceptCode = "TEST-CODE";
        var updateDto = new UpdateGlobalParameterDTO
        {
            Type = EnumConceptType.Percentage,
            Percentage = null
        };
        var globalParameter = new GlobalParameter(conceptCode, "Test", EnumConceptApplication.AddToProduct, 10m, null);

        _mockRepository.Setup(r => r.GetAsync(conceptCode))
            .ReturnsAsync(globalParameter);

        await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateAsync(conceptCode, updateDto));
    }

    #endregion
}
