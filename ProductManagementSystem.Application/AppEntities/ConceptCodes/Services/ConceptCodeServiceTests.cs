using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.Repository;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.DTOs.Outputs;
using ProductManagementSystem.Application.AppEntities.ConceptCodes.Models;
using ProductManagementSystem.Application.AppEntities.Shared.DTOs;
using ProductManagementSystem.Application.Common.AppEntities.Type;
using ProductManagementSystem.Application.Common.Errors;

namespace ProductManagementSystem.Application.AppEntities.ConceptCodes.Services;

public class ConceptCodeServiceTests
{
    private readonly Mock<IConceptCodeRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ConceptCodeService>> _mockLogger;
    private readonly ConceptCodeService _service;

    public ConceptCodeServiceTests()
    {
        _mockRepository = new Mock<IConceptCodeRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ConceptCodeService>>();

        _service = new ConceptCodeService(
            _mockRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    #region GetByCodeAsync Tests

    [Fact]
    public async Task GetByCodeAsync_WithExistingCode_ReturnsConceptCodeDTO()
    {
        var code = "TEST-CODE";
        var conceptCode = ConceptCode.Create(code, true);
        var conceptCodeDto = new ConceptCodeDTO { Code = code };

        _mockRepository.Setup(r => r.GetByCodeAsync(code))
            .ReturnsAsync(conceptCode);
        _mockMapper.Setup(m => m.Map<ConceptCodeDTO>(conceptCode))
            .Returns(conceptCodeDto);

        var result = await _service.GetByCodeAsync(code);

        result.Should().NotBeNull();
        result!.Code.Should().Be(code);
        _mockRepository.Verify(r => r.GetByCodeAsync(code), Times.Once);
    }

    [Fact]
    public async Task GetByCodeAsync_WithNonExistingCode_ThrowsNotFoundException()
    {
        var code = "NON-EXISTING";

        _mockRepository.Setup(r => r.GetByCodeAsync(code))
            .ReturnsAsync((ConceptCode?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByCodeAsync(code));
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllConceptCodes()
    {
        var conceptCodes = new List<ConceptCode>
        {
            ConceptCode.Create("CODE-1", true),
            ConceptCode.Create("CODE-2", false)
        };
        var conceptCodeDtos = new List<ConceptCodeDTO>
        {
            new ConceptCodeDTO { Code = "CODE-1" },
            new ConceptCodeDTO { Code = "CODE-2" }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(conceptCodes);
        _mockMapper.Setup(m => m.Map<List<ConceptCodeDTO>>(conceptCodes))
            .Returns(conceptCodeDtos);

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(2);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ReturnsPaginatedResults()
    {
        var paginationConfig = new PaginationConfigDTO { Page = 1, PageSize = 10 };
        var filter = new FilterConceptCodeDTO();
        var conceptCodes = new List<ConceptCode> { ConceptCode.Create("CODE-1", true) };

        var paginatedResult = new PaginatedResult<ConceptCode>
        {
            Items = conceptCodes,
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        var paginatedDto = new PaginatedResult<ConceptCodeDTO>
        {
            Items = new List<ConceptCodeDTO> { new ConceptCodeDTO { Code = "CODE-1" } },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        _mockMapper.Setup(m => m.Map<PaginationConfigs>(paginationConfig))
            .Returns(PaginationConfigs.Create(1, 10));
        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<PaginationConfigs>(), filter))
            .ReturnsAsync(paginatedResult);
        _mockMapper.Setup(m => m.Map<PaginatedResult<ConceptCodeDTO>>(paginatedResult))
            .Returns(paginatedDto);

        var result = await _service.GetAllAsync(paginationConfig, filter);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsConceptCodeDTO()
    {
        var createDto = new CreateConceptCodeDTO { Code = "NEW-CODE", IsFromSystem = false };
        var conceptCode = ConceptCode.Create("NEW-CODE", false);
        var conceptCodeDto = new ConceptCodeDTO { Code = "NEW-CODE" };

        _mockRepository.Setup(r => r.ExistsByCodeAsync(createDto.Code))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<ConceptCode>()))
            .ReturnsAsync(conceptCode);
        _mockMapper.Setup(m => m.Map<ConceptCodeDTO>(conceptCode))
            .Returns(conceptCodeDto);

        var result = await _service.CreateAsync(createDto);

        result.Should().NotBeNull();
        result.Code.Should().Be("NEW-CODE");
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<ConceptCode>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithExistingCode_ThrowsConflictException()
    {
        var createDto = new CreateConceptCodeDTO { Code = "EXISTING-CODE" };

        _mockRepository.Setup(r => r.ExistsByCodeAsync(createDto.Code))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(createDto));
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<ConceptCode>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithNullIsFromSystem_DefaultsToFalse()
    {
        var createDto = new CreateConceptCodeDTO { Code = "NEW-CODE", IsFromSystem = null };
        var conceptCode = ConceptCode.Create("NEW-CODE", false);
        var conceptCodeDto = new ConceptCodeDTO { Code = "NEW-CODE" };

        _mockRepository.Setup(r => r.ExistsByCodeAsync(createDto.Code))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<ConceptCode>()))
            .ReturnsAsync(conceptCode);
        _mockMapper.Setup(m => m.Map<ConceptCodeDTO>(conceptCode))
            .Returns(conceptCodeDto);

        var result = await _service.CreateAsync(createDto);

        result.Should().NotBeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithExistingCode_ReturnsUpdatedConceptCodeDTO()
    {
        var code = "EXISTING-CODE";
        var updateDto = new UpdateConceptCodeDTO { Code = "UPDATED-CODE" };
        var existingCode = ConceptCode.Create(code, true);
        var updatedCode = ConceptCode.Create("UPDATED-CODE", true);
        var conceptCodeDto = new ConceptCodeDTO { Code = "UPDATED-CODE" };

        _mockRepository.Setup(r => r.GetByCodeAsync(code))
            .ReturnsAsync(existingCode);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ConceptCode>()))
            .ReturnsAsync(updatedCode);
        _mockMapper.Setup(m => m.Map<ConceptCodeDTO>(updatedCode))
            .Returns(conceptCodeDto);

        var result = await _service.UpdateAsync(code, updateDto);

        result.Should().NotBeNull();
        result.Code.Should().Be("UPDATED-CODE");
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<ConceptCode>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingCode_ThrowsNotFoundException()
    {
        var code = "NON-EXISTING";
        var updateDto = new UpdateConceptCodeDTO { Code = "UPDATED-CODE" };

        _mockRepository.Setup(r => r.GetByCodeAsync(code))
            .ReturnsAsync((ConceptCode?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(code, updateDto));
    }

    [Fact]
    public async Task UpdateAsync_WithNullCode_KeepsExistingCode()
    {
        var code = "EXISTING-CODE";
        var updateDto = new UpdateConceptCodeDTO { Code = null };
        var existingCode = ConceptCode.Create(code, true);
        var updatedCode = ConceptCode.Create(code, true);
        var conceptCodeDto = new ConceptCodeDTO { Code = code };

        _mockRepository.Setup(r => r.GetByCodeAsync(code))
            .ReturnsAsync(existingCode);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ConceptCode>()))
            .ReturnsAsync(updatedCode);
        _mockMapper.Setup(m => m.Map<ConceptCodeDTO>(updatedCode))
            .Returns(conceptCodeDto);

        var result = await _service.UpdateAsync(code, updateDto);

        result.Code.Should().Be(code);
    }

    #endregion

    #region ExistsByCodeAsync Tests

    [Fact]
    public async Task ExistsByCodeAsync_WithExistingCode_ReturnsTrue()
    {
        var code = "EXISTING-CODE";

        _mockRepository.Setup(r => r.ExistsByCodeAsync(code))
            .ReturnsAsync(true);

        var result = await _service.ExistsByCodeAsync(code);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByCodeAsync_WithNonExistingCode_ReturnsFalse()
    {
        var code = "NON-EXISTING";

        _mockRepository.Setup(r => r.ExistsByCodeAsync(code))
            .ReturnsAsync(false);

        var result = await _service.ExistsByCodeAsync(code);

        result.Should().BeFalse();
    }

    #endregion
}
