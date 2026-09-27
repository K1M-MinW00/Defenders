#if UNITY_EDITOR
using NUnit.Framework;

public sealed class ShopDataValidatorTests
{
    [Test]
    public void CurrentShopData_HasNoValidationErrors()
    {
        ShopValidationReport report = ShopDataValidator.ValidateProject(false);

        Assert.That(
            report.Errors,
            Is.Empty,
            string.Join("\n", report.Errors));
    }
}
#endif
