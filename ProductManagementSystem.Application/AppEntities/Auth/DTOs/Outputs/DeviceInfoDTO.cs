using System.Text.Json.Serialization;

namespace ProductManagementSystem.Application.AppEntities.Auth.DTOs.Outputs;

public class DeviceInfoDTO
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("device_name")]
    public string DeviceName { get; set; } = string.Empty;

    [JsonPropertyName("login_date")]
    public DateTime LoginDate { get; set; }

    [JsonPropertyName("last_activity")]
    public DateTime LastActivity { get; set; }
}