using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public enum GameDataValidationSeverity
{
    Warning,
    Error,
}

public readonly struct GameDataValidationIssue
{
    public GameDataValidationIssue(GameDataValidationSeverity severity, string assetPath, string message)
    {
        Severity = severity;
        AssetPath = assetPath;
        Message = message;
    }

    public GameDataValidationSeverity Severity { get; }
    public string AssetPath { get; }
    public string Message { get; }
}

public sealed class GameDataValidationReport
{
    private readonly List<GameDataValidationIssue> issues = new();

    public IReadOnlyList<GameDataValidationIssue> Issues => issues;
    public int ErrorCount => issues.Count(issue => issue.Severity == GameDataValidationSeverity.Error);
    public int WarningCount => issues.Count(issue => issue.Severity == GameDataValidationSeverity.Warning);
    public bool HasErrors => ErrorCount > 0;

    public void AddError(UnityEngine.Object asset, string message) =>
        Add(GameDataValidationSeverity.Error, asset, message);

    public void AddWarning(UnityEngine.Object asset, string message) =>
        Add(GameDataValidationSeverity.Warning, asset, message);

    public string Format()
    {
        StringBuilder builder = new();
        builder.Append($"Project validation: {ErrorCount} error(s), {WarningCount} warning(s)");
        foreach (GameDataValidationIssue issue in issues)
        {
            builder.AppendLine();
            builder.Append($"[{issue.Severity}] {issue.AssetPath}: {issue.Message}");
        }

        return builder.ToString();
    }

    private void Add(GameDataValidationSeverity severity, UnityEngine.Object asset, string message)
    {
        string path = asset != null ? AssetDatabase.GetAssetPath(asset) : string.Empty;
        issues.Add(new GameDataValidationIssue(
            severity,
            string.IsNullOrWhiteSpace(path) ? "Project" : path,
            message));
    }
}

public static class GameDataProjectValidator
{
    public static GameDataValidationReport Validate()
    {
        GameDataValidationReport report = new();
        UnitDataSO[] unitAssets = LoadAllAssets<UnitDataSO>();
        ItemDataSO[] itemAssets = LoadAllAssets<ItemDataSO>();
        StageDataSO[] stageAssets = LoadAllAssets<StageDataSO>();

        IUnitCatalog units = TryCreateCatalog(() => new UnitCatalog(unitAssets), "Unit catalog", report);
        IItemCatalog items = TryCreateCatalog(() => new ItemCatalog(itemAssets), "Item catalog", report);
        IStageCatalog stages = TryCreateCatalog(() => new StageCatalog(stageAssets), "Stage catalog", report);

        ValidateRequiredConfigs(units, report);
        ValidateUnits(unitAssets, items, report);
        ValidateItems(itemAssets, report);
        ValidateGachaBanners(units, items, report);
        ValidateStages(stageAssets, stages, report);
        GameSceneProjectValidator.Validate(report);
        return report;
    }

    private static void ValidateRequiredConfigs(IUnitCatalog units, GameDataValidationReport report)
    {
        NewUserConfigSO newUser = LoadRequired<NewUserConfigSO>("GameData/Configs/NewUserConfig", report);
        UserLevelProgressionSO levelProgression = LoadRequired<UserLevelProgressionSO>("GameData/Configs/UserLevelProgression", report);
        GachaEconomyConfigSO economy = LoadRequired<GachaEconomyConfigSO>("GameData/Configs/GachaEconomyConfig", report);
        GameIconSetSO icons = LoadRequired<GameIconSetSO>("GameData/Catalogs/GameIconSet", report);

        if (newUser != null && units != null && !newUser.TryValidate(units, out string newUserError))
            report.AddError(newUser, newUserError);
        if (levelProgression != null && !levelProgression.TryValidate(out string levelError))
            report.AddError(levelProgression, levelError);
        if (economy != null && !economy.TryValidate(out string economyError))
            report.AddError(economy, economyError);
        if (icons != null && !icons.TryValidate(out string iconError))
            report.AddError(icons, iconError);
    }

    private static void ValidateUnits(IEnumerable<UnitDataSO> units, IItemCatalog items, GameDataValidationReport report)
    {
        foreach (UnitDataSO unit in units)
        {
            if (string.IsNullOrWhiteSpace(unit.displayName))
                report.AddWarning(unit, "Display name is empty.");
            if (unit.icon == null)
                report.AddWarning(unit, "Icon is missing.");
            if (unit.unitPrefab == null)
                report.AddWarning(unit, "Unit prefab is missing.");
            if (unit.maxLevel <= 0)
                report.AddError(unit, "Max level must be positive.");

            ValidateUnitSkillAnimation(unit, report);

            if (unit.promotionCost == null)
                continue;

            for (int i = 0; i < unit.promotionCost.Length; i++)
            {
                PromotionCost cost = unit.promotionCost[i];
                if (cost == null)
                {
                    report.AddError(unit, $"Promotion cost at index {i} is null.");
                    continue;
                }

                if (cost.Count <= 0)
                    report.AddError(unit, $"Promotion cost at index {i} must be positive.");
                if (string.IsNullOrWhiteSpace(cost.MaterialId))
                {
                    report.AddError(unit, $"Promotion material ID at index {i} is empty.");
                    continue;
                }

                ItemDataSO material = items?.Get(cost.MaterialId);
                if (material == null)
                    report.AddError(unit, $"Promotion material does not exist: {cost.MaterialId}");
                else if (material.Category != ItemCategory.Material)
                    report.AddError(unit, $"Promotion item is not a material: {cost.MaterialId}");
            }
        }
    }

    public static void ValidateUnitSkillAnimation(UnitDataSO unit, GameDataValidationReport report)
    {
        if (unit == null || report == null || unit.activeSkill == null || unit.unitPrefab == null)
            return;

        ActiveSkillBase skill = unit.unitPrefab.GetComponent<ActiveSkillBase>();
        if (skill == null)
        {
            report.AddError(unit, "Active skill data is assigned, but the unit prefab has no ActiveSkillBase component.");
            return;
        }

        var skillClips = new HashSet<AnimationClip>();
        Animator[] animators = unit.unitPrefab.GetComponentsInChildren<Animator>(true);

        foreach (Animator animator in animators)
        {
            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            if (controller == null)
                continue;

            AnimationClip[] clips = controller.animationClips;
            bool ownsSkillClip = clips.Any(IsSkillClip);
            if (!ownsSkillClip)
                continue;

            if (animator.GetComponent<UnitAnimationEvent>() == null)
                report.AddError(unit, $"Animator '{animator.name}' has a skill clip but no UnitAnimationEvent relay.");

            foreach (AnimationClip clip in clips)
            {
                if (IsSkillClip(clip))
                    skillClips.Add(clip);
            }
        }

        if (skillClips.Count == 0)
        {
            report.AddError(unit, "No skill animation clip is assigned to the unit prefab.");
            return;
        }

        foreach (AnimationClip clip in skillClips)
            ValidateSkillClip(unit, clip, report);
    }

    private static bool IsSkillClip(AnimationClip clip)
    {
        return clip != null && clip.name.Contains("Skill", StringComparison.OrdinalIgnoreCase);
    }

    private static void ValidateSkillClip(UnitDataSO unit, AnimationClip clip, GameDataValidationReport report)
    {
        AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);
        AnimationEvent applyEvent = events.FirstOrDefault(animationEvent =>
            animationEvent.functionName == nameof(UnitAnimationEvent.OnSkillApplyEvent));
        AnimationEvent finishEvent = events.FirstOrDefault(animationEvent =>
            animationEvent.functionName == nameof(UnitAnimationEvent.OnSkillFinishedEvent));

        if (applyEvent == null)
            report.AddError(unit, $"Skill clip '{clip.name}' is missing OnSkillApplyEvent.");
        if (finishEvent == null)
            report.AddError(unit, $"Skill clip '{clip.name}' is missing OnSkillFinishedEvent.");
        if (applyEvent != null && finishEvent != null && applyEvent.time >= finishEvent.time)
            report.AddError(unit, $"Skill clip '{clip.name}' must apply before it finishes.");
    }

    private static void ValidateItems(IEnumerable<ItemDataSO> items, GameDataValidationReport report)
    {
        foreach (ItemDataSO item in items)
        {
            if (string.IsNullOrWhiteSpace(item.ItemName))
                report.AddWarning(item, "Item name is empty.");
            if (item.Icon == null)
                report.AddWarning(item, "Item icon is missing.");
        }
    }

    private static void ValidateStages(
        IEnumerable<StageDataSO> stages,
        IStageCatalog catalog,
        GameDataValidationReport report)
    {
        foreach (StageDataSO stage in stages)
        {
            if (!stage.TryValidate(out string error))
            {
                report.AddError(stage, error);
                continue;
            }

            if (catalog != null && catalog.Get(stage.sector, stage.stage) != stage)
                report.AddError(stage, $"Stage is outside the canonical catalog: {stage.StageKey}");

            StageMapContext mapContext = stage.mapPrefab.GetComponent<StageMapContext>();
            if (mapContext == null)
                report.AddError(stage, $"Map prefab has no {nameof(StageMapContext)} on its root.");
            else if (!mapContext.TryValidate(out string mapError))
                report.AddError(stage, $"Map prefab is invalid: {mapError}");
        }
    }

    private static void ValidateGachaBanners(IUnitCatalog units, IItemCatalog items, GameDataValidationReport report)
    {
        foreach (GachaDataSO banner in LoadAllAssets<GachaDataSO>())
        {
            if (!banner.TryValidate(out string bannerError))
            {
                report.AddError(banner, bannerError);
                continue;
            }

            ItemDataSO ticket = items?.Get(banner.TicketItemId);
            if (ticket == null)
                report.AddError(banner, $"Recruit ticket does not exist: {banner.TicketItemId}");
            else if (ticket.Category != ItemCategory.Consumable)
                report.AddError(banner, $"Recruit ticket is not consumable: {banner.TicketItemId}");

            ValidatePoolReferences(banner, banner.NormalPool, units, report);
            ValidatePoolReferences(banner, banner.RarePool, units, report);
            ValidatePoolReferences(banner, banner.LegendPool, units, report);
        }
    }

    private static void ValidatePoolReferences(
        GachaDataSO banner,
        IEnumerable<UnitDataSO> pool,
        IUnitCatalog units,
        GameDataValidationReport report)
    {
        if (pool == null || units == null)
            return;

        foreach (UnitDataSO unit in pool)
        {
            if (unit != null && units.Get(unit.unitId) != unit)
                report.AddError(banner, $"Recruit pool references a unit outside the canonical catalog: {unit.unitId}");
        }
    }

    private static T LoadRequired<T>(string resourcesPath, GameDataValidationReport report) where T : UnityEngine.Object
    {
        T asset = Resources.Load<T>(resourcesPath);
        if (asset == null)
            report.AddError(null, $"Required asset is missing: Resources/{resourcesPath}");
        return asset;
    }

    private static TCatalog TryCreateCatalog<TCatalog>(
        Func<TCatalog> create,
        string name,
        GameDataValidationReport report) where TCatalog : class
    {
        try
        {
            return create();
        }
        catch (Exception exception)
        {
            report.AddError(null, $"{name} is invalid: {exception.Message}");
            return null;
        }
    }

    private static T[] LoadAllAssets<T>() where T : UnityEngine.Object
    {
        return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<T>)
            .Where(asset => asset != null)
            .ToArray();
    }
}

public static class GameDataValidationMenu
{
    [MenuItem("Tools/Validation/Validate Project")]
    public static void ValidateFromMenu()
    {
        GameDataValidationReport report = GameDataProjectValidator.Validate();
        if (report.HasErrors)
            Debug.LogError(report.Format());
        else if (report.WarningCount > 0)
            Debug.LogWarning(report.Format());
        else
            Debug.Log(report.Format());
    }
}

public sealed class GameDataBuildPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport buildReport)
    {
        GameDataValidationReport report = GameDataProjectValidator.Validate();
        if (report.HasErrors)
            throw new BuildFailedException(report.Format());

        if (report.WarningCount > 0)
            Debug.LogWarning(report.Format());
    }
}
