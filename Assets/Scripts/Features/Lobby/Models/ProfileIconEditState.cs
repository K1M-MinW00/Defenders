using System.Collections.Generic;
using UnityEngine;

public sealed class ProfileIconEditState
{
    public IReadOnlyList<ProfileIconOption> Options { get; set; }
    public Sprite PreviewIcon { get; set; }
    public bool CanSave { get; set; }
}
