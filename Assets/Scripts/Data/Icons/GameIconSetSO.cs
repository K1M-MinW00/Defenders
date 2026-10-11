using UnityEngine;

[CreateAssetMenu(menuName = "Database/Game Icon Set")]
public class GameIconSetSO : ScriptableObject
{
    [Header("Resource")]
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite gemIcon;
    [SerializeField] private Sprite fuelIcon;
    [Tooltip("Optional. Uses the gold icon as a placeholder when empty.")]
    [SerializeField] private Sprite experienceIcon;
    [Tooltip("연구소 전용 재화 아이콘. 비어 있으면 경험치 아이콘을 사용합니다.")]
    [SerializeField] private Sprite researchMaterialIcon;

    [Header("Resource Presentation")]
    [SerializeField] private string goldDisplayName = "골드";
    [SerializeField] private Rarity goldRarity = Rarity.Normal;
    [SerializeField] private string gemDisplayName = "젬";
    [SerializeField] private Rarity gemRarity = Rarity.Normal;
    [SerializeField] private string fuelDisplayName = "연료";
    [SerializeField] private Rarity fuelRarity = Rarity.Normal;
    [SerializeField] private string experienceDisplayName = "경험치";
    [SerializeField] private Rarity experienceRarity = Rarity.Normal;
    [SerializeField] private string researchMaterialDisplayName = "연구 재료";
    [SerializeField] private Rarity researchMaterialRarity = Rarity.Normal;

    public Sprite Gold => goldIcon;
    public Sprite Gem => gemIcon;
    public Sprite Fuel => fuelIcon;
    public Sprite Experience => experienceIcon != null ? experienceIcon : goldIcon;
    public Sprite ResearchMaterial => researchMaterialIcon != null ? researchMaterialIcon : Experience;
    public string GetResourceDisplayName(RewardType type)
    {
        return type switch
        {
            RewardType.Gold => goldDisplayName,
            RewardType.Gem => gemDisplayName,
            RewardType.Fuel => fuelDisplayName,
            RewardType.Experience => experienceDisplayName,
            RewardType.ResearchMaterial => researchMaterialDisplayName,
            _ => type.ToString(),
        };
    }

    public Rarity GetResourceRarity(RewardType type)
    {
        return type switch
        {
            RewardType.Gold => goldRarity,
            RewardType.Gem => gemRarity,
            RewardType.Fuel => fuelRarity,
            RewardType.Experience => experienceRarity,
            RewardType.ResearchMaterial => researchMaterialRarity,
            _ => Rarity.Normal,
        };
    }

    public bool TryValidate(out string error)
    {
        if (goldIcon == null || gemIcon == null || fuelIcon == null)
        {
            error = "One or more resource icons are missing.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(goldDisplayName) ||
            string.IsNullOrWhiteSpace(gemDisplayName) ||
            string.IsNullOrWhiteSpace(fuelDisplayName) ||
            string.IsNullOrWhiteSpace(experienceDisplayName) ||
            string.IsNullOrWhiteSpace(researchMaterialDisplayName))
        {
            error = "One or more resource display names are missing.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
