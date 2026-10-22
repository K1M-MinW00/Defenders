using System;

[Serializable]
public sealed class BossAudioSettings
{
    public AudioClipSettings areaAttackTelegraph = new();
    public AudioClipSettings areaAttackImpact = new();
    public AudioClipSettings shieldActivated = new();
    public AudioClipSettings enrage = new();
    public AudioClipSettings summon = new();
}
