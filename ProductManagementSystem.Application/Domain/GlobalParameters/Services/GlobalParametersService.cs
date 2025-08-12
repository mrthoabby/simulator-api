using AutoMapper;
using ProductManagementSystem.Application.Common.Errors;
using ProductManagementSystem.Application.Domain.ConceptCodes.Services;
using ProductManagementSystem.Application.Domain.GlobalParameters.DTOs.Inputs;
using ProductManagementSystem.Application.Domain.GlobalParameters.DTOs.Outputs;
using ProductManagementSystem.Application.Domain.GlobalParameters.Models;
using ProductManagementSystem.Application.Domain.GlobalParameters.Repository;
using ProductManagementSystem.Application.Domain.Shared.Enum;
using ProductManagementSystem.Application.Domain.Shared.Type;

namespace ProductManagementSystem.Application.Domain.GlobalParameters.Services;

public class GlobalParametersService : IGlobalParametersService
{
    private readonly IGlobalParametersRepository _globalParametersRepository;
    private readonly IConceptCodeService _deductionCodeService;

    private readonly IMapper _mapper;
    public GlobalParametersService(IGlobalParametersRepository globalParametersRepository, IConceptCodeService deductionCodeService, IMapper mapper)
    {
        _globalParametersRepository = globalParametersRepository;
        _deductionCodeService = deductionCodeService;
        _mapper = mapper;

    }

    public async Task<GlobalParameterDTO> CreateAsync(AddGlobalParameterDTO request)
    {
        var deductionCode = await _deductionCodeService.GetByCodeAsync(request.ConceptCode);
        if (deductionCode == null)
        {
            throw new NotFoundException("Deduction code not found");
        }

        if (request.Type == EnumConceptType.FixedValue && request.Price == null)
        {
            throw new ConflictException("Price is required when type is FixedValue");
        }
        if (request.Type == EnumConceptType.Percentage && request.Percentage == null)
        {
            throw new ConflictException("Percentage is required when type is Percentage");
        }

        var price = request.Type == EnumConceptType.FixedValue && request.Price != null ? _mapper.Map<Money>(request.Price) : null;
        var percentage = request.Type == EnumConceptType.Percentage && request.Percentage != null ? request.Percentage.Value : 0;

        var globalParameter = GlobalParameter.Create(
            request.ConceptCode,
            request.Name,
            request.Application,
            request.Type == EnumConceptType.FixedValue && request.Price != null ? request.Price.Value : percentage,
            request.Description);


        var result = await _globalParametersRepository.CreateAsync(globalParameter);
        return _mapper.Map<GlobalParameterDTO>(result);
    }

    public async Task<List<GlobalParameterDTO>> GetAllAsync()
    {
        var globalParameters = await _globalParametersRepository.GetAllAsync();
        return _mapper.Map<List<GlobalParameterDTO>>(globalParameters);

    }

    public async Task<GlobalParameterDTO> GetAsync(string conceptCode)
    {
        var globalParameter = await _globalParametersRepository.GetAsync(conceptCode);
        return _mapper.Map<GlobalParameterDTO>(globalParameter);
    }

    public async Task<GlobalParameterDTO> UpdateAsync(string conceptCode, UpdateGlobalParameterDTO updateDTO)
    {
        var globalParameter = await _globalParametersRepository.GetAsync(conceptCode);
        if (globalParameter == null)
        {
            throw new NotFoundException("Global parameter not found");
        }
        if (updateDTO.Name != null)
        {
            globalParameter.SetName(updateDTO.Name);
        }
        if (updateDTO.Description != null)
        {
            globalParameter.SetDescription(updateDTO.Description);
        }
        if (updateDTO.Application != null)
        {
            globalParameter.SetApplication(updateDTO.Application.Value);
        }
        if (updateDTO.Type != null)
        {

            if (updateDTO.Type.Value == EnumConceptType.FixedValue)
            {
                if (updateDTO.Price == null)
                {
                    throw new ConflictException("Price is required when type is FixedValue");
                }
                globalParameter.SetType(EnumConceptType.FixedValue);
                globalParameter.SetPrice(_mapper.Map<Money>(updateDTO.Price));
            }
            else if (updateDTO.Type.Value == EnumConceptType.Percentage)
            {
                if (updateDTO.Percentage == null)
                {
                    throw new ConflictException("Percentage is required when type is Percentage");
                }
                globalParameter.SetType(EnumConceptType.Percentage);
                globalParameter.SetPercentage(updateDTO.Percentage.Value);
            }
        }
        else
        {
            if (updateDTO.Price != null)
            {
                globalParameter.SetPrice(_mapper.Map<Money>(updateDTO.Price));
            }
            if (updateDTO.Percentage != null)
            {
                globalParameter.SetPercentage(updateDTO.Percentage.Value);
            }
        }
        var result = await _globalParametersRepository.UpdateAsync(globalParameter);
        return _mapper.Map<GlobalParameterDTO>(result);
    }
}