using ProductManagementSystem.Application.AppEntities.Auth.DTOs.Outputs;
using ProductManagementSystem.Application.Common.Errors;

namespace ProductManagementSystem.Application.Common.AppEntities.Errors;

public class DeviceLimitExceededException : ConflictException
{
    public int MaxDevices { get; set; }
    public List<DeviceInfoDTO> ActiveDevices { get; set; }

    public DeviceLimitExceededException(int maxDevices, List<DeviceInfoDTO> devices)
        : base("Device limit exceeded")
    {
        MaxDevices = maxDevices;
        ActiveDevices = devices;
    }
}