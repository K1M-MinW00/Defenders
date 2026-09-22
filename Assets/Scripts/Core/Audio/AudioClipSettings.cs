using System;
using UnityEngine;

[Serializable]
public sealed class AudioClipSettings
{
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    public AudioClip Clip => clip;
    public float Volume => volume;
    public bool IsValid => clip != null && volume > 0f;
}
