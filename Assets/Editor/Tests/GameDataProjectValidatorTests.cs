using System;

public sealed class GameDataProjectValidatorTests
{
    public void ProjectData_HasNoBlockingValidationErrors()
    {
        GameDataValidationReport report = GameDataProjectValidator.Validate();
        if (report.HasErrors)
            throw new InvalidOperationException(report.Format());
    }
}
