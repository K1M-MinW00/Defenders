using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Game/Units/Unit Data")]
public class UnitDataSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private UnitCode unitCode;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    [Header("Prefab")]
    [SerializeField] private GameObject unitPrefab;

    [Header("Base Stats (Star 1)")]
    [SerializeField] private UnitStats baseStats;

    //[Header("Skills")]
    //[SerializeField] private SkillDataSO activeSkill;
    //[SerializeField] private SkillDataSO passiveSkill;

    public UnitCode UnitCode => unitCode;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject UnitPrefab => unitPrefab;
    public UnitStats BaseStats => baseStats;

    //public SkillDataSO ActiveSkill => activeSkill;
    //public SkillDataSO PassiveSkill => PassiveSkill;
}