using System;
using NUnit.Framework;

public sealed class GameDataProjectValidatorTests
{
    [Test]
    public void ProjectData_HasNoBlockingValidationErrors()
    {
        GameDataValidationReport report = GameDataProjectValidator.Validate();
        if (report.HasErrors)
            throw new InvalidOperationException(report.Format());
    }
}
