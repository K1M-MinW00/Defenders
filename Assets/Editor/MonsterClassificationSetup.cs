using System.IO;
using UnityEditor;
using UnityEngine;

public static class MonsterClassificationSetup
{
    private const string MonsterDataDirectory = "Assets/GameData/Monsters/Normal";
    private const string BossDataDirectory = "Assets/GameData/Monsters/Bosses";
    private const string BossDefinitionDirectory = BossDataDirectory + "/Definitions";

    private static readonly string[] EliteMonsterNames =
    {
        "Elite_Orc",
        "Armored_Orc",
        "Armored_Skeleton"
    };

    private static readonly string[] BossCandidateNames =
    {
        "Elite_Orc",
        "Armored_Orc",
        "Armored_Skeleton",
        "WereWolf"
    };

    [MenuItem("Tools/Game/Monsters/Apply Classification And Boss Candidates")]
    public static void Apply()
    {
        EnsureFolder(BossDataDirectory);
        EnsureFolder(BossDefinitionDirectory);

        foreach (string monsterName in EliteMonsterNames)
        {
            MonsterDataSO data = LoadMonsterData(monsterName);
            SetClassification(data, MonsterClassification.Elite, null);
        }

        foreach (string monsterName in BossCandidateNames)
            CreateOrUpdateBossCandidate(monsterName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Monster classifications and four boss candidates were created.");
    }

    private static void CreateOrUpdateBossCandidate(string sourceName)
    {
        MonsterDataSO source = LoadMonsterData(sourceName);
        string bossName = $"Boss_{sourceName}";
        string bossDataPath = $"{BossDataDirectory}/{bossName}.asset";
        string definitionPath = $"{BossDefinitionDirectory}/{bossName}_Definition.asset";

        BossDefinitionSO definition = AssetDatabase.LoadAssetAtPath<BossDefinitionSO>(definitionPath);
        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<BossDefinitionSO>();
            definition.name = $"{bossName}_Definition";
            AssetDatabase.CreateAsset(definition, definitionPath);
        }

        definition.bossId = bossName;
        ConfigureStatusEffects(definition, bossName);
        ConfigureSummon(definition, bossName);
        ConfigureResistance(definition, bossName);
        EditorUtility.SetDirty(definition);

        MonsterDataSO bossData = AssetDatabase.LoadAssetAtPath<MonsterDataSO>(bossDataPath);
        if (bossData == null)
        {
            bossData = Object.Instantiate(source);
            bossData.name = bossName;
            AssetDatabase.CreateAsset(bossData, bossDataPath);
        }
        else
        {
            EditorUtility.CopySerialized(source, bossData);
            bossData.name = bossName;
        }

        bossData.monsterId = bossName;
        bossData.displayName = $"Boss {source.displayName}";
        bossData.description = $"{source.description}\n보스 후보 외형을 재사용한 확대 버전";
        SetClassification(bossData, MonsterClassification.Boss, definition);
    }

    private static void ConfigureStatusEffects(BossDefinitionSO definition, string bossName)
    {
        BossAreaAttackStatusSettings settings = new();
        switch (bossName)
        {
            case "Boss_Armored_Orc":
                settings.enableStun = true;
                settings.enableDefenseDown = true;
                break;

            case "Boss_WereWolf":
                settings.enableSlow = true;
                settings.enableBurn = true;
                break;

            case "Boss_Elite_Orc":
                settings.enablePoison = true;
                break;

            case "Boss_Armored_Skeleton":
                settings.enablePoison = true;
                settings.enableDefenseDown = true;
                break;
        }

        definition.areaAttackStatusEffects = settings;
    }

    private static void ConfigureSummon(BossDefinitionSO definition, string bossName)
    {
        BossSummonSettings settings = new();
        if (bossName == "Boss_Armored_Orc")
        {
            settings.enabled = true;
            settings.candidates.Add(LoadMonsterData("Orc"));
        }

        definition.summon = settings;
    }

    private static void ConfigureResistance(BossDefinitionSO definition, string bossName)
    {
        BossStatusResistanceSettings settings = new();
        switch (bossName)
        {
            case "Boss_Armored_Orc":
                settings.slow = 0.25f;
                settings.stun = 0.75f;
                settings.poison = 0.25f;
                settings.defenseDown = 0.5f;
                break;
            case "Boss_WereWolf":
                settings.slow = 0.75f;
                settings.stun = 0.5f;
                settings.poison = 0.25f;
                settings.burn = 0.25f;
                settings.defenseDown = 0.25f;
                break;
            case "Boss_Elite_Orc":
                settings.slow = 0.25f;
                settings.stun = 0.5f;
                settings.poison = 0.75f;
                settings.burn = 0.25f;
                settings.defenseDown = 0.25f;
                break;
            case "Boss_Armored_Skeleton":
                settings.slow = 0.5f;
                settings.stun = 0.75f;
                settings.poison = 1f;
                settings.defenseDown = 0.5f;
                break;
        }
        definition.statusResistance = settings;
    }

    private static MonsterDataSO LoadMonsterData(string monsterName)
    {
        string path = $"{MonsterDataDirectory}/{monsterName}.asset";
        MonsterDataSO data = AssetDatabase.LoadAssetAtPath<MonsterDataSO>(path);
        if (data == null)
            throw new FileNotFoundException($"Monster data was not found: {path}");

        return data;
    }

    private static void SetClassification(
        MonsterDataSO data,
        MonsterClassification classification,
        BossDefinitionSO definition)
    {
        SerializedObject serializedData = new(data);
        serializedData.FindProperty("classification").enumValueIndex = (int)classification;
        serializedData.FindProperty("visualScaleOverride").floatValue = 0f;
        serializedData.FindProperty("bossDefinition").objectReferenceValue = definition;
        serializedData.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(data);
    }

    private static void EnsureFolder(string path)
    {
        string[] segments = path.Split('/');
        string current = segments[0];
        for (int index = 1; index < segments.Length; index++)
        {
            string next = $"{current}/{segments[index]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, segments[index]);
            current = next;
        }
    }
}
