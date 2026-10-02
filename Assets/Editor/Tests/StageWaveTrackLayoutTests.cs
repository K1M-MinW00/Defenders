using NUnit.Framework;

public sealed class StageWaveTrackLayoutTests
{
    [TestCase(0, new[] { 0, 1, 2, 9 })]
    [TestCase(3, new[] { 3, 4, 5, 9 })]
    [TestCase(6, new[] { 6, 7, 8, 9 })]
    [TestCase(9, new[] { 6, 7, 8, 9 })]
    public void TenWaves_UsesThreeWavePagesAndFinalBoss(int currentIndex, int[] expected)
    {
        CollectionAssert.AreEqual(expected, StageWaveTrackUI.BuildVisibleWaveIndices(10, currentIndex));
    }

    [TestCase(0, new[] { 0, 1, 2, 5 })]
    [TestCase(3, new[] { 2, 3, 4, 5 })]
    [TestCase(5, new[] { 2, 3, 4, 5 })]
    public void SixWaves_KeepsBossVisibleAndEndsWithFourConsecutiveWaves(int currentIndex, int[] expected)
    {
        CollectionAssert.AreEqual(expected, StageWaveTrackUI.BuildVisibleWaveIndices(6, currentIndex));
    }

    [Test]
    public void FourOrFewerWaves_ShowsEveryWave()
    {
        CollectionAssert.AreEqual(
            new[] { 0, 1, 2, 3 },
            StageWaveTrackUI.BuildVisibleWaveIndices(4, 2));
    }
}
