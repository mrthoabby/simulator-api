using AutoMapper;
using ProductManagementSystem.Application.Common.Domain.Type;
using ProductManagementSystem.Application.Common.Errors;
using ProductManagementSystem.Application.Domain.DeductionCodes.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.DeductionCodes.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.DeductionCodes.Repository;
using ProductManagementSystem.Application.Domain.DeductionCodes.Models;

namespace ProductManagementSystem.Application.Domain.DeductionCodes.Services;

public class DeductionCodeService : IDeductionCodeService
{
    private readonly IDeductionCodeRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<DeductionCodeService> _logger;

    public DeductionCodeService(
        IDeductionCodeRepository repository,
        IMapper mapper,
        ILogger<DeductionCodeService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<DeductionCodeDTO?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Getting deduction code by ID: {Id}", id);

        var deductionCode = await _repository.GetByIdAsync(id);
        if (deductionCode == null)
        {
            _logger.LogWarning("Deduction code not found with ID: {Id}", id);
            return null;
        }

        return _mapper.Map<DeductionCodeDTO>(deductionCode);
    }

    public async Task<DeductionCodeDTO?> GetByCodeAsync(string code)
    {
        _logger.LogInformation("Getting deduction code by code: {Code}", code);

        var deductionCode = await _repository.GetByCodeAsync(code);
        if (deductionCode == null)
        {
            _logger.LogWarning("Deduction code not found with code: {Code}", code);
            return null;
        }

        return _mapper.Map<DeductionCodeDTO>(deductionCode);
    }

    public async Task<List<DeductionCodeDTO>> GetAllAsync()
    {
        _logger.LogInformation("Getting all deduction codes");

        var deductionCodes = await _repository.GetAllAsync();
        return _mapper.Map<List<DeductionCodeDTO>>(deductionCodes);
    }

    public async Task<PaginatedResult<DeductionCodeDTO>> GetFilteredAsync(PaginationConfigs paginationConfigs, FilterDeductionCodeDTO filter)
    {
        _logger.LogInformation("Getting filtered deduction codes with page: {Page}, pageSize: {PageSize}",
            paginationConfigs.Page, paginationConfigs.PageSize);

        var result = await _repository.GetFilteredAsync(paginationConfigs, filter);

        return new PaginatedResult<DeductionCodeDTO>
        {
            Items = _mapper.Map<List<DeductionCodeDTO>>(result.Items),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };
    }

    public async Task<DeductionCodeDTO> CreateAsync(CreateDeductionCodeDTO request)
    {
        _logger.LogInformation("Creating deduction code: {Code}", request.Code);

        // Check if code already exists
        var exists = await _repository.ExistsByCodeAsync(request.Code);
        if (exists)
        {
            throw new ConflictException(DeductionCodeServiceValues.Errors.DeductionCodeAlreadyExists);
        }

        var deductionCode = DeductionCode.Create(request.Code);
        var created = await _repository.CreateAsync(deductionCode);

        _logger.LogInformation("Deduction code created successfully: {Code}", request.Code);
        return _mapper.Map<DeductionCodeDTO>(created);
    }

    public async Task<DeductionCodeDTO> UpdateAsync(string code, UpdateDeductionCodeDTO request)
    {
        _logger.LogInformation("Updating deduction code: {Code}", code);

        var existing = await _repository.GetByCodeAsync(code);
        if (existing == null)
        {
            throw new NotFoundException(DeductionCodeServiceValues.Errors.DeductionCodeNotFound);
        }

        // Update the code
        var updatedDeductionCode = DeductionCode.Create(request.Code);
        var result = await _repository.UpdateAsync(updatedDeductionCode);

        _logger.LogInformation("Deduction code updated successfully: {Code}", code);
        return _mapper.Map<DeductionCodeDTO>(result);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting deduction code with ID: {Id}", id);

        var result = await _repository.DeleteAsync(id);
        if (result)
        {
            _logger.LogInformation("Deduction code deleted successfully with ID: {Id}", id);
        }
        else
        {
            _logger.LogWarning("Deduction code not found for deletion with ID: {Id}", id);
        }

        return result;
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        return await _repository.ExistsByCodeAsync(code);
    }

    public async Task<List<DeductionCodeDTO>> SearchByPatternAsync(string pattern)
    {
        _logger.LogInformation("Searching deduction codes by pattern: {Pattern}", pattern);

        var deductionCodes = await _repository.SearchByPatternAsync(pattern);
        return _mapper.Map<List<DeductionCodeDTO>>(deductionCodes);
    }



    public async Task<string> GenerateNextCodeAsync(string prefix)
    {
        _logger.LogInformation("Generating next code with prefix: {Prefix}", prefix);

        var existingCodes = await _repository.GetByPrefixAsync(prefix.ToUpper());

        var maxNumber = 0;
        var prefixPattern = $"{prefix.ToUpper()}_";

        foreach (var code in existingCodes)
        {
            if (code.Code.StartsWith(prefixPattern))
            {
                var numberPart = code.Code.Substring(prefixPattern.Length);
                if (int.TryParse(numberPart, out var number) && number > maxNumber)
                {
                    maxNumber = number;
                }
            }
        }

        var nextCode = $"{prefixPattern}{(maxNumber + 1):D3}"; // Format with 3 digits

        _logger.LogInformation("Generated next code: {NextCode}", nextCode);
        return nextCode;
    }
}

public static class DeductionCodeServiceValues
{
    public static class Errors
    {
        public const string DeductionCodeAlreadyExists = "Deduction code already exists";
        public const string DeductionCodeNotFound = "Deduction code not found";
    }

    public static class Messages
    {
        public const string DeductionCodeCreated = "Deduction code created successfully";
        public const string DeductionCodeUpdated = "Deduction code updated successfully";
        public const string DeductionCodeDeleted = "Deduction code deleted successfully";
    }
}