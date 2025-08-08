using AutoMapper;
using ProductManagementSystem.Application.Common.Errors;
using ProductManagementSystem.Application.Domain.ConceptCodes.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.ConceptCodes.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.ConceptCodes.Repository;
using ProductManagementSystem.Application.Domain.ConceptCodes.Models;
using ProductManagementSystem.Application.Domain.Shared.DTOs;
using ProductManagementSystem.Application.Common.Domain.Type;

namespace ProductManagementSystem.Application.Domain.ConceptCodes.Services;

public class ConceptCodeService : IConceptCodeService
{
    private readonly IConceptCodeRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ConceptCodeService> _logger;

    public ConceptCodeService(
        IConceptCodeRepository repository,
        IMapper mapper,
        ILogger<ConceptCodeService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }


    public async Task<ConceptCodeDTO?> GetByCodeAsync(string code)
    {
        _logger.LogInformation("Getting code by code: {Code}", code);

        var conceptCode = await _repository.GetByCodeAsync(code);
        if (conceptCode == null)
        {
            throw new NotFoundException("Concept code not found");
        }

        return _mapper.Map<ConceptCodeDTO>(conceptCode);
    }



    public async Task<List<ConceptCodeDTO>> GetAllAsync()
    {
        var deductionCodes = await _repository.GetAllAsync();
        return _mapper.Map<List<ConceptCodeDTO>>(deductionCodes);
    }

    public async Task<PaginatedResult<ConceptCodeDTO>> GetAllAsync(PaginationConfigDTO paginationConfigs, FilterConceptCodeDTO filter)
    {
        var paginationConfig = _mapper.Map<PaginationConfigs>(paginationConfigs);

        var result = await _repository.GetAllAsync(paginationConfig, filter);

        return _mapper.Map<PaginatedResult<ConceptCodeDTO>>(result);
    }

    public async Task<ConceptCodeDTO> CreateAsync(CreateConceptCodeDTO request)
    {
        _logger.LogInformation("Creating concept code: {Code}", request.Code);

        var exists = await _repository.ExistsByCodeAsync(request.Code);
        if (exists)
        {
            throw new ConflictException("Concept code already exists");
        }

        var deductionCode = ConceptCode.Create(request.Code, request.IsFromSystem ?? false);
        var created = await _repository.CreateAsync(deductionCode);

        _logger.LogInformation("Deduction code created successfully: {Code}", request.Code);
        return _mapper.Map<ConceptCodeDTO>(created);
    }

    public async Task<ConceptCodeDTO> UpdateAsync(string code, UpdateConceptCodeDTO request)
    {
        _logger.LogInformation("Updating deduction code: {Code}", code);

        var existing = await _repository.GetByCodeAsync(code);
        if (existing == null)
        {
            throw new NotFoundException("Concept code not found");
        }

        var updatedDeductionCode = ConceptCode.Create(request.Code ?? existing.Code, existing.IsFromSystem);
        var result = await _repository.UpdateAsync(updatedDeductionCode);

        _logger.LogInformation("Concept code updated successfully: {Code}", code);
        return _mapper.Map<ConceptCodeDTO>(result);
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        return await _repository.ExistsByCodeAsync(code);
    }
}
