namespace ProductManagementSystem.Application.Products.Models;

public enum EnumDeductionType
{
    PERCENTAGE,
    FIXED_AMOUNT
}

public enum EnumDeductionApplication
{
    ADD_UNIT_PRODUCT_PRICE,
    MULTIPLY_BY_DISPLACEMENT_BOX
}

public record Deduction(string Name, string Description, string ConceptCode, DeductionValue Value, EnumDeductionApplication Application);

public record DeductionValue(decimal Value, EnumDeductionType Type);




