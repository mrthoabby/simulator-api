using ProductManagementSystem.Application.AppEntities.GlobalParameters.DTOs.Inputs;
using ProductManagementSystem.Application.AppEntities.GlobalParameters.DTOs.Outputs;

namespace ProductManagementSystem.Application.AppEntities.GlobalParameters.Services;

public interface IGlobalParametersService
{
    Task<GlobalParameterDTO> CreateAsync(AddGlobalParameterDTO addDTO);
    Task<List<GlobalParameterDTO>> GetAllAsync();
    Task<GlobalParameterDTO> GetAsync(string conceptCode);
    Task<GlobalParameterDTO> UpdateAsync(string conceptCode, UpdateGlobalParameterDTO updateDTO);

}