using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class CharacterAssetOrganizationMigration
{
    private const string ThirdPartyCharacters = "Assets/Art/ThirdParty/RPG/Characters(100x100)";
    private const string UnitAnimations = "Assets/Animations/Units";
    private const string MonsterAnimations = "Assets/Animations/Monsters";

    private static readonly (string sourceName, string destinationName)[] UnitCharacters =
    {
        ("Archer", "Archer"),
        ("Armored Axeman", "Axeman"),
        ("Knight", "Knight"),
        ("Knight Templar", "KnightTemplar"),
        ("Lancer", "Lancer"),
        ("Priest", "Priest"),
        ("Soldier", "Soldier"),
        ("Swordsman", "Swordsman"),
        ("Werebear", "Werebear"),
        ("Wizard", "Wizard"),
    };

    private static readonly (string sourceName, string destinationName)[] MonsterCharacters =
    {
        ("Armored Orc", "ArmoredOrc"),
        ("Armored Skeleton", "ArmoredSkeleton"),
        ("Elite Orc", "EliteOrc"),
        ("Orc", "Orc"),
        ("Orc rider", "OrcRider"),
        ("Skeleton", "Skeleton"),
        ("Skeleton Archer", "SkeletonArcher"),
        ("Slime", "Slime"),
        ("Werewolf", "Werewolf"),
    };

    private static readonly Dictionary<string, string> SkillFolders = new(StringComparer.Ordinal)
    {
        ["Archer"] = "Archer",
        ["Axeman"] = "Axeman",
        ["Knight"] = "Knight",
        ["KnightTemplar"] = "KnightTemplar",
        ["Lancer"] = "Lancer",
        ["PlagueAlchemist"] = "PlagueAlchemist",
        ["Priest"] = "Priest",
        ["SoldierM"] = "SoldierMelee",
        ["SoldierR"] = "SoldierRange",
        ["Swordsman"] = "Swordsman",
        ["Werebear"] = "Werebear",
        ["Wizard"] = "Wizard",
    };

    [MenuItem("Tools/Defenders/Migrations/Organize Character, VFX, Art, and GameData")]
    public static void Run()
    {
        try
        {
            AssetDatabase.SaveAssets();
            OrganizeAnimations();
            OrganizeArt();
            OrganizeGameData();
            StandardizeCharacterSpriteImporters();
            GachaPoolSynchronizer.SynchronizeAll();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("Character asset organization migration completed successfully.");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            throw;
        }
    }

    private static void OrganizeAnimations()
    {
        EnsureFolder(UnitAnimations);
        EnsureFolder(MonsterAnimations);

        MoveFolderContents("Assets/Animations/UnitBase", $"{UnitAnimations}/Base", AnimationAssetExtensions);
        MoveFolderContents("Assets/Animations/Unit/PlagueAlchemist", $"{UnitAnimations}/PlagueAlchemist", AnimationAssetExtensions);

        MoveDirectFiles("Assets/Animations/Monster", $"{MonsterAnimations}/Base", AnimationAssetExtensions);
        MoveFolderContents("Assets/Animations/Monster/WraithShaman", $"{MonsterAnimations}/WraithShaman", AnimationAssetExtensions);

        foreach ((string sourceName, string destinationName) in UnitCharacters)
        {
            string source = $"{ThirdPartyCharacters}/{sourceName}/{sourceName}(Split Effects)";
            MoveFolderContents(source, $"{UnitAnimations}/{destinationName}", AnimationAssetExtensions);
        }

        foreach ((string sourceName, string destinationName) in MonsterCharacters)
        {
            string source = $"{ThirdPartyCharacters}/{sourceName}/{sourceName}(Split Effects)";
            MoveFolderContents(source, $"{MonsterAnimations}/{destinationName}", AnimationAssetExtensions);
        }

        MoveAssetIfPresent("Assets/Animations/Projectile", "Assets/Animations/Projectiles");
        MoveAssetIfPresent("Assets/Animations/VFX", "Assets/Animations/Effects/VFX");

        DeleteFolderIfEmpty("Assets/Animations/Unit/PlagueAlchemist");
        DeleteFolderIfEmpty("Assets/Animations/Unit");
        DeleteFolderIfEmpty("Assets/Animations/UnitBase");
        DeleteFolderIfEmpty("Assets/Animations/Monster/WraithShaman");
        DeleteFolderIfEmpty("Assets/Animations/Monster");

        OrganizeEffectAnimations();
    }

    private static void OrganizeEffectAnimations()
    {
        MoveFolderContents("Assets/Art/ThirdParty/RPG/Magic(Projectile)", "Assets/Animations/Effects/Priest", AnimationAssetExtensions, "Priest_");
        MoveFolderContents("Assets/Art/ThirdParty/RPG/Magic(Projectile)", "Assets/Animations/Effects/Wizard", AnimationAssetExtensions, "Wizard_");
        MoveAssetIfPresent("Assets/Art/ThirdParty/RPG/Magic(Projectile)/WIzard_Skill.controller", "Assets/Animations/Effects/Wizard/Wizard_Skill.controller");

        MoveAssetIfPresent("Assets/Animations/Units/Axeman/Axe_Aura.anim", "Assets/Animations/Effects/Axeman/Axeman_VFX.anim");
        MoveAssetIfPresent("Assets/Animations/Units/Axeman/Axe_Aura.controller", "Assets/Animations/Effects/Axeman/Axeman_VFX.controller");
        MoveAssetIfPresent("Assets/Animations/Units/Lancer/Lancer_Aura.anim", "Assets/Animations/Effects/Lancer/Lancer_VFX.anim");
        MoveAssetIfPresent("Assets/Animations/Units/Lancer/Spear_VFX.controller", "Assets/Animations/Effects/Lancer/Lancer_VFX.controller");
        MoveAssetIfPresent("Assets/Animations/Units/Werebear/Claw_Aura.anim", "Assets/Animations/Effects/Werebear/Werebear_VFX.anim");
        MoveAssetIfPresent("Assets/Animations/Units/Werebear/Werebear_VFX.controller", "Assets/Animations/Effects/Werebear/Werebear_VFX.controller");
        MoveAssetIfPresent("Assets/Animations/Monsters/Werewolf/Werewolf_Aura.anim", "Assets/Animations/Effects/Werewolf/Werewolf_VFX.anim");
        MoveAssetIfPresent("Assets/Animations/Monsters/Werewolf/Werewolf_Aura.controller", "Assets/Animations/Effects/Werewolf/Werewolf_VFX.controller");

        // These two were unreferenced duplicate Lancer assets. Keep them grouped and clearly marked
        // instead of leaving a generic Aura folder/name at the Effects root.
        MoveAssetIfPresent("Assets/Animations/Effects/VFX/Lancer_Aura.anim", "Assets/Animations/Effects/Lancer/Lancer_VFX_Legacy.anim");
        MoveAssetIfPresent("Assets/Animations/Effects/VFX/Aura.controller", "Assets/Animations/Effects/Lancer/Lancer_VFX_Legacy.controller");

        // Aura_Base is a levelled projectile animation, not a character attack effect.
        MoveAssetIfPresent("Assets/Animations/Projectiles/Aura_Base.controller", "Assets/Animations/Projectiles/LevelledProjectile_VFX.controller");
        DeleteFolderIfEmpty("Assets/Animations/Effects/VFX");
    }

    private static void OrganizeArt()
    {
        const string magic = "Assets/Art/ThirdParty/RPG/Magic(Projectile)";
        MoveAssetIfPresent($"{magic}/Priest-Attack_effect.png", "Assets/Art/ThirdParty/RPG/Effects/Priest/Priest-Attack_effect.png");
        MoveAssetIfPresent($"{magic}/Priest-Heal_Effect.png", "Assets/Art/ThirdParty/RPG/Effects/Priest/Priest-Heal_Effect.png");
        MoveAssetIfPresent($"{magic}/Wizard-Attack01_Effect.png", "Assets/Art/ThirdParty/RPG/Effects/Wizard/Wizard-Attack01_Effect.png");
        MoveAssetIfPresent($"{magic}/Wizard-Attack02_Effect.png", "Assets/Art/ThirdParty/RPG/Effects/Wizard/Wizard-Attack02_Effect.png");
        DeleteFolderIfEmpty(magic);

        // Generated is first-party runtime art. Concepts remain separate because they are
        // references/source material rather than assets used by gameplay.
        MoveAssetIfPresent("Assets/Art/Generated", "Assets/Art/FirstParty/Runtime");
        MoveAssetIfPresent("Assets/Art/Concepts", "Assets/Art/FirstParty/Concepts");

        MoveFolderContents("Assets/Art/UI/Generated", "Assets/Art/UI/Common", ImageAssetExtensions);
        MoveAssetIfPresent("Assets/Art/UI/Generated/Equipment", "Assets/Art/UI/Equipment");
        MoveAssetIfPresent("Assets/Art/UI/Generated/Lobby", "Assets/Art/UI/Lobby");
        MoveAssetIfPresent("Assets/Art/UI/Generated/Relics", "Assets/Art/UI/Relics");
        MoveAssetIfPresent("Assets/Art/UI/Generated/StatusEffects", "Assets/Art/UI/StatusEffects");
        MoveAssetIfPresent("Assets/Art/UI/Shop_Banner.png", "Assets/Art/UI/Shop/Shop_Banner.png");
        OrganizeLegacyUiImages();
        DeleteFolderIfEmpty("Assets/Art/UI/Generated");
    }

    private static void OrganizeLegacyUiImages()
    {
        const string source = "Assets/Art/UI/Images";
        MoveAssetIfPresent($"{source}/Icons.png", "Assets/Art/UI/LegacyAtlases/GameplayIcons.png");
        MoveAssetIfPresent($"{source}/Icons.asset", "Assets/Art/UI/LegacyAtlases/GameplayIcons_TMP.asset");
        MoveAssetIfPresent($"{source}/Icons_2.png", "Assets/Art/UI/LegacyAtlases/LobbyIcons.png");
        MoveAssetIfPresent($"{source}/Icons_2.asset", "Assets/Art/UI/LegacyAtlases/LobbyIcons_TMP.asset");
        MoveAssetIfPresent($"{source}/Icons_3.png", "Assets/Art/UI/LegacyAtlases/ConsumableIcons.png");
        MoveAssetIfPresent($"{source}/Icons_3.asset", "Assets/Art/UI/LegacyAtlases/ConsumableIcons_TMP.asset");
        MoveAssetIfPresent($"{source}/icons_4.png", "Assets/Art/UI/Atlases/UnitProgressionIcons.png");
        MoveAssetIfPresent($"{source}/materials_prom_icons.png", "Assets/Art/UI/Atlases/PromotionMaterialIcons.png");
        MoveAssetIfPresent($"{source}/prom_icons.png", "Assets/Art/UI/Atlases/PromotionRankIcons.png");
        MoveAssetIfPresent($"{source}/Skill_Icons_1.png", "Assets/Art/UI/Atlases/SkillIcons_Melee.png");
        MoveAssetIfPresent($"{source}/Skill_Icons_2.png", "Assets/Art/UI/Atlases/SkillIcons_RangedMagic.png");
        MoveAssetIfPresent($"{source}/UnitTab_Icons.png", "Assets/Art/UI/Atlases/UnitTabIcons.png");
        MoveAssetIfPresent($"{source}/Portrait.png", "Assets/Art/UI/Atlases/UnitPortraits.png");
        MoveAssetIfPresent($"{source}/Lobby_BG.png", "Assets/Art/UI/Backgrounds/LobbyBackground.png");
        MoveAssetIfPresent($"{source}/Start_BG.png", "Assets/Art/UI/Backgrounds/StartBackground.png");
        MoveAssetIfPresent($"{source}/Title.png", "Assets/Art/UI/Branding/GameTitle.png");
        MoveAssetIfPresent($"{source}/Flag.png", "Assets/Art/FirstParty/Runtime/Effects/Lancer/BattleStandard.png");
        DeleteFolderIfEmpty(source);
    }

    private static void OrganizeGameData()
    {
        EnsureFolder("Assets/GameData/Units");

        string skillRoot = "Assets/GameData/Skills";
        if (AssetDatabase.IsValidFolder(skillRoot))
        {
            foreach (string guid in AssetDatabase.FindAssets("t:SkillDataSO", new[] { skillRoot }))
            {
                string source = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(source);
                int separator = fileName.IndexOf('_');
                if (separator <= 0)
                {
                    throw new InvalidOperationException($"Cannot determine skill owner from '{source}'.");
                }

                string owner = fileName.Substring(0, separator);
                if (!SkillFolders.TryGetValue(owner, out string destinationFolder))
                {
                    throw new InvalidOperationException($"No skill folder mapping exists for '{source}'.");
                }

                string targetFolder = $"Assets/GameData/Units/{destinationFolder}/Skills";
                EnsureFolder(targetFolder);
                MoveAssetIfPresent(source, $"{targetFolder}/{Path.GetFileName(source)}");
            }
        }

        EnsureFolder("Assets/GameData/Monsters/Normal");
        foreach (string source in GetDirectAssetFiles("Assets/GameData/Monsters", ".asset"))
        {
            MoveAssetIfPresent(source, $"Assets/GameData/Monsters/Normal/{Path.GetFileName(source)}");
        }

        EnsureFolder("Assets/GameData/Monsters/Bosses/Definitions");
        MoveFolderContents("Assets/GameData/Bosses", "Assets/GameData/Monsters/Bosses/Definitions", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".asset" });

        EnsureFolder("Assets/GameData/Economy/Configs");
        MoveAssetIfPresent("Assets/GameData/Economy/BaseConfig.asset", "Assets/GameData/Economy/Configs/BaseConfig.asset");
        MoveAssetIfPresent("Assets/GameData/Shops", "Assets/GameData/Economy/Shops");
        MoveAssetIfPresent("Assets/Scripts/Data/Gacha/Normal.asset", "Assets/GameData/Economy/Gacha/Banners/Normal.asset");
        MoveAssetIfPresent("Assets/Scripts/Data/Gacha/Special.asset", "Assets/GameData/Economy/Gacha/Banners/Special.asset");

        DeleteFolderIfEmpty("Assets/GameData/Skills");
        DeleteFolderIfEmpty("Assets/GameData/Bosses");
    }

    private static void StandardizeCharacterSpriteImporters()
    {
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { ThirdPartyCharacters });
        int updated = 0;

        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.Contains("(Split Effects)/", StringComparison.Ordinal) ||
                !path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
            {
                throw new InvalidOperationException($"TextureImporter unavailable for '{path}'. Operation aborted.");
            }

            bool changed = false;
            changed |= SetIfDifferent(importer.spritePixelsPerUnit, 16f, value => importer.spritePixelsPerUnit = value);
            changed |= SetIfDifferent(importer.filterMode, FilterMode.Point, value => importer.filterMode = value);
            changed |= SetIfDifferent(importer.mipmapEnabled, false, value => importer.mipmapEnabled = value);
            changed |= SetIfDifferent(importer.textureCompression, TextureImporterCompression.Uncompressed, value => importer.textureCompression = value);
            changed |= SetIfDifferent(importer.wrapMode, TextureWrapMode.Clamp, value => importer.wrapMode = value);
            changed |= SetIfDifferent(importer.alphaIsTransparency, true, value => importer.alphaIsTransparency = value);
            changed |= SetIfDifferent(importer.isReadable, false, value => importer.isReadable = value);

            var textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);
            if (textureSettings.spriteMeshType != SpriteMeshType.FullRect)
            {
                textureSettings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(textureSettings);
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
                updated++;
            }
        }

        Debug.Log($"Standardized {updated} Split Effects texture importers.");
    }

    private static readonly HashSet<string> AnimationAssetExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".anim",
        ".controller",
        ".overrideController",
    };

    private static readonly HashSet<string> ImageAssetExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".psd", ".tga",
    };

    private static void MoveFolderContents(string sourceFolder, string destinationFolder, HashSet<string> extensions)
    {
        if (!AssetDatabase.IsValidFolder(sourceFolder))
        {
            return;
        }

        EnsureFolder(destinationFolder);
        foreach (string source in GetDirectAssetFiles(sourceFolder, extensions))
        {
            MoveAssetIfPresent(source, $"{destinationFolder}/{Path.GetFileName(source)}");
        }
    }

    private static void MoveFolderContents(string sourceFolder, string destinationFolder, HashSet<string> extensions, string fileNamePrefix)
    {
        if (!AssetDatabase.IsValidFolder(sourceFolder))
        {
            return;
        }

        EnsureFolder(destinationFolder);
        foreach (string source in GetDirectAssetFiles(sourceFolder, extensions))
        {
            if (Path.GetFileName(source).StartsWith(fileNamePrefix, StringComparison.OrdinalIgnoreCase))
            {
                MoveAssetIfPresent(source, $"{destinationFolder}/{Path.GetFileName(source)}");
            }
        }
    }

    private static void MoveDirectFiles(string sourceFolder, string destinationFolder, HashSet<string> extensions)
    {
        MoveFolderContents(sourceFolder, destinationFolder, extensions);
    }

    private static IEnumerable<string> GetDirectAssetFiles(string folder, string extension)
    {
        return GetDirectAssetFiles(folder, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { extension });
    }

    private static IEnumerable<string> GetDirectAssetFiles(string folder, HashSet<string> extensions)
    {
        if (!Directory.Exists(folder))
        {
            yield break;
        }

        foreach (string file in Directory.GetFiles(folder, "*", SearchOption.TopDirectoryOnly))
        {
            string normalized = file.Replace('\\', '/');
            if (!normalized.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) &&
                extensions.Contains(Path.GetExtension(normalized)))
            {
                yield return normalized;
            }
        }
    }

    private static void MoveAssetIfPresent(string source, string destination)
    {
        if (AssetDatabase.LoadMainAssetAtPath(source) == null && !AssetDatabase.IsValidFolder(source))
        {
            return;
        }

        if (AssetDatabase.LoadMainAssetAtPath(destination) != null || AssetDatabase.IsValidFolder(destination))
        {
            throw new InvalidOperationException($"Migration destination already exists: '{destination}'.");
        }

        EnsureFolder(Path.GetDirectoryName(destination)?.Replace('\\', '/'));
        string error = AssetDatabase.MoveAsset(source, destination);
        if (!string.IsNullOrEmpty(error))
        {
            throw new InvalidOperationException($"Failed to move '{source}' to '{destination}': {error}");
        }
    }

    private static void EnsureFolder(string folder)
    {
        if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
        string name = Path.GetFileName(folder);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static void DeleteFolderIfEmpty(string folder)
    {
        if (!AssetDatabase.IsValidFolder(folder) || Directory.GetFiles(folder, "*", SearchOption.AllDirectories).Length != 0)
        {
            return;
        }

        AssetDatabase.DeleteAsset(folder);
    }

    private static bool SetIfDifferent<T>(T current, T desired, Action<T> setter)
    {
        if (EqualityComparer<T>.Default.Equals(current, desired))
        {
            return false;
        }

        setter(desired);
        return true;
    }
}
