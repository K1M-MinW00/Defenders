using NUnit.Framework;

public sealed class StaggeredUpdateScheduleTests
{
    [Test]
    public void InitialDelay_IsDeterministicAndWithinInterval()
    {
        const int stableId = 12345;
        const float interval = 0.5f;

        float first = StaggeredUpdateSchedule.GetInitialDelay(stableId, interval);
        float second = StaggeredUpdateSchedule.GetInitialDelay(stableId, interval);

        Assert.That(first, Is.EqualTo(second));
        Assert.That(first, Is.GreaterThanOrEqualTo(0f));
        Assert.That(first, Is.LessThan(interval));
    }

    [Test]
    public void DifferentIds_AreDistributedWithinInterval()
    {
        const float interval = 0.5f;

        float first = StaggeredUpdateSchedule.GetInitialDelay(1, interval);
        float second = StaggeredUpdateSchedule.GetInitialDelay(2, interval);

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    public void NonPositiveInterval_ReturnsZero(float interval)
    {
        Assert.That(StaggeredUpdateSchedule.GetInitialDelay(1, interval), Is.Zero);
    }
}
