#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class ShopValidationReport
{
    public IReadOnlyList<string> Errors => errors;
    public IReadOnlyList<string> Warnings => warnings;
    public bool IsValid => errors.Count == 0;

    private readonly List<string> errors = new();
    private readonly List<string> warnings = new();

    public void Error(string message) => errors.Add(message);
    public void Warning(string message) => warnings.Add(message);
}

public static class ShopDataValidator
{
    private const string GroupPath = "Assets/GameData/Shops/ShopItems.asset";
    private const string ProductFolder = "Assets/GameData/Shops/Products";
    private const string PrefabFolder = "Assets/Prefabs/UI/Shop";

    [MenuItem("Tools/Defenders/Validate Shop Data")]
    public static void ValidateFromMenu()
    {
        ShopValidationReport report = ValidateProject(true);
        string result = report.IsValid ? "검사 통과" : "검사 실패";
        EditorUtility.DisplayDialog(
            "Shop Validation",
            $"{result}\n오류 {report.Errors.Count}개 / 경고 {report.Warnings.Count}개\n\n자세한 내용은 Console을 확인하세요.",
            "확인");
    }

    public static void ValidateForBatchMode()
    {
        ShopValidationReport report = ValidateProject(true);
        if (!report.IsValid)
            throw new InvalidOperationException(
                $"Shop validation failed with {report.Errors.Count} error(s).");
    }

    public static ShopValidationReport ValidateProject(bool logResult)
    {
        ShopValidationReport report = new();
        List<ShopProductData> products = LoadProducts();

        ValidateProducts(products, report);
        ValidateGroup(AssetDatabase.LoadAssetAtPath<ShopProductGroup>(GroupPath), report);
        ValidatePrefabs(report);

        if (logResult)
            Log(report);

        return report;
    }

    private static List<ShopProductData> LoadProducts()
    {
        return AssetDatabase.FindAssets("t:ShopProductData", new[] { ProductFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<ShopProductData>)
            .Where(product => product != null)
            .ToList();
    }

    private static void ValidateProducts(
        IReadOnlyCollection<ShopProductData> products,
        ShopValidationReport report)
    {
        foreach (IGrouping<string, ShopProductData> duplicate in products
                     .Where(product => !string.IsNullOrWhiteSpace(product.ProductId))
                     .GroupBy(product => product.ProductId, StringComparer.Ordinal)
                     .Where(group => group.Count() > 1))
        {
            report.Error($"상품 ID가 중복되었습니다: {duplicate.Key}");
        }

        foreach (ShopProductData product in products)
        {
            string label = ProductLabel(product);

            if (string.IsNullOrWhiteSpace(product.ProductId))
                report.Error($"{label}: ProductId가 비어 있습니다.");
            if (string.IsNullOrWhiteSpace(product.DisplayName))
                report.Error($"{label}: DisplayName이 비어 있습니다.");
            if (product.PurchaseLimit < 0)
                report.Error($"{label}: PurchaseLimit은 0 이상이어야 합니다.");

            ValidateProductType(product, label, report);
            ValidatePurchase(product, label, report);
            ValidateRewards(product, label, report);
            ValidateSalePeriod(product, label, report);
        }
    }

    private static void ValidateProductType(
        ShopProductData product,
        string label,
        ShopValidationReport report)
    {
        bool valid = product switch
        {
            LimitedShopProductData => product.Tab == ShopTabType.Limited,
            PackageShopProductData => product.Tab == ShopTabType.Package,
            RechargeShopProductData => product.Tab == ShopTabType.Recharge,
            ExchangeShopProductData => product.Tab == ShopTabType.Exchange,
            _ => false,
        };

        if (!valid)
            report.Error($"{label}: SO 타입과 Tab 값이 일치하지 않습니다.");
    }

    private static void ValidatePurchase(
        ShopProductData product,
        string label,
        ShopValidationReport report)
    {
        switch (product.PurchaseType)
        {
            case ShopPurchaseType.Gem:
            case ShopPurchaseType.Gold:
                if (product.CostAmount <= 0)
                    report.Error($"{label}: 재화 구매 가격은 0보다 커야 합니다.");
                break;

            case ShopPurchaseType.InAppPurchase:
                if (product.Price <= 0)
                    report.Error($"{label}: 표시 가격은 0보다 커야 합니다.");
                if (string.IsNullOrWhiteSpace(product.IAPProductId))
                    report.Warning($"{label}: IAPProductId가 아직 지정되지 않았습니다.");
                break;
        }
    }

    private static void ValidateRewards(
        ShopProductData product,
        string label,
        ShopValidationReport report)
    {
        if (product.Rewards == null || product.Rewards.Count == 0)
        {
            report.Error($"{label}: 보상이 하나 이상 필요합니다.");
            return;
        }

        for (int i = 0; i < product.Rewards.Count; i++)
        {
            RewardData reward = product.Rewards[i];
            if (reward == null)
            {
                report.Error($"{label}: Rewards[{i}]가 null입니다.");
                continue;
            }

            if (reward.Amount <= 0)
                report.Error($"{label}: Rewards[{i}] 수량은 0보다 커야 합니다.");

            if (RequiresId(reward.Type) && string.IsNullOrWhiteSpace(reward.Id))
                report.Error($"{label}: {reward.Type} 보상에는 Id가 필요합니다.");
        }
    }

    private static void ValidateSalePeriod(
        ShopProductData product,
        string label,
        ShopValidationReport report)
    {
        bool hasStart = TryParseDate(product.SaleStartUtc, out DateTimeOffset start, label, "SaleStartUtc", report);
        bool hasEnd = TryParseDate(product.SaleEndUtc, out DateTimeOffset end, label, "SaleEndUtc", report);

        if (hasStart && hasEnd && start >= end)
            report.Error($"{label}: 판매 종료 시각은 시작 시각보다 늦어야 합니다.");
    }

    private static bool TryParseDate(
        string value,
        out DateTimeOffset result,
        string label,
        string field,
        ShopValidationReport report)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out result))
            return true;

        report.Error($"{label}: {field}가 올바른 ISO-8601 형식이 아닙니다.");
        return false;
    }

    private static void ValidateGroup(ShopProductGroup group, ShopValidationReport report)
    {
        if (group == null)
        {
            report.Error($"상품 그룹을 찾을 수 없습니다: {GroupPath}");
            return;
        }

        HashSet<ShopProductData> registered = new();
        ValidateGroupList(group.LimitedProducts, typeof(LimitedShopProductData), ShopTabType.Limited, ShopResetPeriod.None, "한정", registered, report);
        ValidateGroupList(group.DailyPackages, typeof(PackageShopProductData), ShopTabType.Package, ShopResetPeriod.Daily, "일간 패키지", registered, report);
        ValidateGroupList(group.WeeklyPackages, typeof(PackageShopProductData), ShopTabType.Package, ShopResetPeriod.Weekly, "주간 패키지", registered, report);
        ValidateGroupList(group.MonthlyPackages, typeof(PackageShopProductData), ShopTabType.Package, ShopResetPeriod.Monthly, "월간 패키지", registered, report);
        ValidateGroupList(group.RechargeProducts, typeof(RechargeShopProductData), ShopTabType.Recharge, ShopResetPeriod.None, "충전", registered, report);
        ValidateGroupList(group.ExchangeProducts, typeof(ExchangeShopProductData), ShopTabType.Exchange, ShopResetPeriod.None, "교환소", registered, report);
    }

    private static void ValidateGroupList(
        IEnumerable<ShopProductData> products,
        Type expectedType,
        ShopTabType expectedTab,
        ShopResetPeriod expectedReset,
        string section,
        ISet<ShopProductData> registered,
        ShopValidationReport report)
    {
        if (products == null)
        {
            report.Error($"{section}: 상품 목록이 null입니다.");
            return;
        }

        foreach (ShopProductData product in products)
        {
            if (product == null)
            {
                report.Error($"{section}: null 상품이 등록되어 있습니다.");
                continue;
            }

            string label = ProductLabel(product);
            if (!registered.Add(product))
                report.Error($"{label}: 둘 이상의 상품 그룹에 중복 등록되었습니다.");
            if (product.GetType() != expectedType)
                report.Error($"{label}: {section}에 잘못된 SO 타입이 등록되었습니다.");
            if (product.Tab != expectedTab)
                report.Error($"{label}: {section}의 Tab 값이 올바르지 않습니다.");
            if (product.ResetPeriod != expectedReset)
                report.Error($"{label}: {section}의 ResetPeriod는 {expectedReset}이어야 합니다.");
        }
    }

    private static void ValidatePrefabs(ShopValidationReport report)
    {
        ValidatePrefab<LimitedProductView>("LimitedShopCard", "portraitImage", "descriptionText", "rewardContainer", "rewardSlotPrefab", report);
        ValidatePrefab<PackageProductView>("PackageShopCard", "packageImage", "rewardContainer", "rewardSlotPrefab", report);
        ValidatePrefab<RechargeProductView>("RechargeShopCard", "gemImage", "bonusPanel", "bonusText", report);
        ValidatePrefab<ExchangeProductView>("ExchangeShopCard", "productImage", "rewardContainer", "rewardSlotPrefab", report);
    }

    private static void ValidatePrefab<T>(
        string prefabName,
        string field1,
        string field2,
        string field3,
        string field4,
        ShopValidationReport report) where T : ShopProductView
    {
        ValidatePrefab<T>(prefabName, new[] { field1, field2, field3, field4 }, report);
    }

    private static void ValidatePrefab<T>(
        string prefabName,
        string field1,
        string field2,
        string field3,
        ShopValidationReport report) where T : ShopProductView
    {
        ValidatePrefab<T>(prefabName, new[] { field1, field2, field3 }, report);
    }

    private static void ValidatePrefab<T>(
        string prefabName,
        IReadOnlyList<string> fields,
        ShopValidationReport report) where T : ShopProductView
    {
        string path = $"{PrefabFolder}/{prefabName}.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            report.Error($"프리팹을 찾을 수 없습니다: {path}");
            return;
        }

        T view = prefab.GetComponent<T>();
        if (view == null)
        {
            report.Error($"{prefabName}: {typeof(T).Name} 컴포넌트가 없습니다.");
            return;
        }

        SerializedObject serialized = new(view);
        ValidateReference(serialized, "titleText", prefabName, report);
        ValidateReference(serialized, "costText", prefabName, report);
        ValidateReference(serialized, "remainText", prefabName, report);
        ValidateReference(serialized, "timerText", prefabName, report);
        ValidateReference(serialized, "statusText", prefabName, report);
        ValidateReference(serialized, "purchaseButton", prefabName, report);

        foreach (string field in fields)
            ValidateReference(serialized, field, prefabName, report);
    }

    private static void ValidateReference(
        SerializedObject serialized,
        string field,
        string prefabName,
        ShopValidationReport report)
    {
        SerializedProperty property = serialized.FindProperty(field);
        if (property == null)
        {
            report.Error($"{prefabName}: 직렬화 필드 '{field}'를 찾을 수 없습니다.");
            return;
        }

        if (property.objectReferenceValue == null)
            report.Error($"{prefabName}: '{field}' 참조가 할당되지 않았습니다.");
    }

    private static bool RequiresId(RewardType type)
    {
        return type == RewardType.Item ||
               type == RewardType.Equipment ||
               type == RewardType.Unit;
    }

    private static string ProductLabel(ShopProductData product)
    {
        string path = AssetDatabase.GetAssetPath(product);
        string id = string.IsNullOrWhiteSpace(product.ProductId) ? product.name : product.ProductId;
        return $"{id} ({path})";
    }

    private static void Log(ShopValidationReport report)
    {
        foreach (string error in report.Errors)
            Debug.LogError($"[ShopValidator] {error}");
        foreach (string warning in report.Warnings)
            Debug.LogWarning($"[ShopValidator] {warning}");

        if (report.IsValid)
            Debug.Log($"[ShopValidator] 검사 통과: 오류 0개, 경고 {report.Warnings.Count}개");
        else
            Debug.LogError($"[ShopValidator] 검사 실패: 오류 {report.Errors.Count}개, 경고 {report.Warnings.Count}개");
    }
}
#endif

