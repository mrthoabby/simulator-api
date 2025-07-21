using ProductManagementSystem.Application.Common.Domain.Enum;

namespace ProductManagementSystem.Application.Common.Domain.Type;

public record Price(decimal Value, EnumCurrency Currency);