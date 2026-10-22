using NUnit.Framework;
using UnityEngine;

public sealed class BossAreaTelegraphViewTests
{
    [Test]
    public void Initialize_CreatesAreaProgressAndOutlineAtRequestedRadius()
    {
        BossAreaTelegraphView view = BossAreaTelegraphView.Create(Vector3.zero, 2.5f, 2f);

        Assert.That(view.GetComponent<MeshRenderer>(), Is.Not.Null);
        Assert.That(view.transform.Find("Progress")?.GetComponent<MeshRenderer>(), Is.Not.Null);
        LineRenderer outline = view.transform.Find("Outline")?.GetComponent<LineRenderer>();
        Assert.That(outline, Is.Not.Null);
        Assert.That(outline.positionCount, Is.EqualTo(64));
        Assert.That(outline.GetPosition(0).magnitude, Is.EqualTo(2.5f).Within(0.001f));

        view.SetProgress(0.75f);
        Assert.That(view.Progress, Is.EqualTo(0.75f));

        Object.DestroyImmediate(view.gameObject);
    }
}
