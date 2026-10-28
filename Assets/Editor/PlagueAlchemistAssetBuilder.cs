using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class PlagueAlchemistAssetBuilder
{
    private const string SpriteDirectory = "Assets/Art/FirstParty/Runtime/Units/PlagueAlchemist";
    private const string IdleSpritePath = SpriteDirectory + "/PlagueAlchemist-Idle.png";
    private const string MoveSpritePath = SpriteDirectory + "/PlagueAlchemist-Move.png";
    private const string AttackSpritePath = SpriteDirectory + "/PlagueAlchemist-Attack.png";
    private const string DieSpritePath = SpriteDirectory + "/PlagueAlchemist-Die.png";
    private const string SkillSpritePath = SpriteDirectory + "/PlagueAlchemist-Skill.png";

    private const string PoisonVialSpritePath = "Assets/Art/FirstParty/Runtime/Projectiles/PlagueAlchemist/PlagueAlchemist-PoisonVial-Flight.png";
    private const string ToxicFlaskSpritePath = "Assets/Art/FirstParty/Runtime/Projectiles/PlagueAlchemist/PlagueAlchemist-ToxicFlask-Flight.png";
    private const string ToxicPoolSpritePath = "Assets/Art/FirstParty/Runtime/Effects/PlagueAlchemist/PlagueAlchemist-ToxicPool-Loop.png";
    private const string PortraitPath = "Assets/Art/FirstParty/Concepts/PlagueAlchemist/portrait_plague_alchemist.png";

    private const string OutputDirectory = "Assets/Animations/Units/PlagueAlchemist";
    private const string IdleClipPath = OutputDirectory + "/PlagueAlchemist_Idle.anim";
    private const string MoveClipPath = OutputDirectory + "/PlagueAlchemist_Move.anim";
    private const string AttackClipPath = OutputDirectory + "/PlagueAlchemist_Attack.anim";
    private const string DieClipPath = OutputDirectory + "/PlagueAlchemist_Die.anim";
    private const string SkillClipPath = OutputDirectory + "/PlagueAlchemist_Skill.anim";
    private const string OverrideControllerPath = OutputDirectory + "/Plague_Alchemist.overrideController";

    private const string ProjectileAnimationDirectory = "Assets/Animations/Projectiles/PlagueAlchemist";
    private const string PoisonVialClipPath = ProjectileAnimationDirectory + "/PlagueAlchemist_PoisonVial_Flight.anim";
    private const string PoisonVialControllerPath = ProjectileAnimationDirectory + "/PlagueAlchemist_PoisonVial.controller";
    private const string ToxicFlaskClipPath = ProjectileAnimationDirectory + "/PlagueAlchemist_ToxicFlask_Flight.anim";
    private const string ToxicFlaskControllerPath = ProjectileAnimationDirectory + "/PlagueAlchemist_ToxicFlask.controller";
    private const string EffectAnimationDirectory = "Assets/Animations/Effects/PlagueAlchemist";
    private const string ToxicPoolClipPath = EffectAnimationDirectory + "/PlagueAlchemist_ToxicPool_Loop.anim";
    private const string ToxicPoolControllerPath = EffectAnimationDirectory + "/PlagueAlchemist_ToxicPool.controller";

    private const string BaseControllerPath = "Assets/Animations/Units/Base/Unit_Base.controller";
    private const string BasePrefabPath = "Assets/Prefabs/Units/Unit_Base.prefab";
    private const string PrefabPath = "Assets/Prefabs/Units/Plague_Alchemist.prefab";
    private const string PoisonVialPrefabPath = "Assets/Prefabs/Projectile/PlagueAlchemist_PoisonVial.prefab";
    private const string ToxicFlaskPrefabPath = "Assets/Prefabs/Projectile/PlagueAlchemist_ToxicFlask.prefab";
    private const string ToxicPoolPrefabPath = "Assets/Prefabs/Skill_Effect/PlagueAlchemist_ToxicPool.prefab";
    private const string SkillDirectory = "Assets/GameData/Units/PlagueAlchemist/Skills";
    private const string PassiveSkillPath = SkillDirectory + "/PlagueAlchemist_Passive.asset";
    private const string ActiveSkillPath = SkillDirectory + "/PlagueAlchemist_Active.asset";
    private const string UnitDataPath = "Assets/Resources/GameData/Units/Plague_Alchemist.asset";

    public static void Build()
    {
        Directory.CreateDirectory(OutputDirectory);
        Directory.CreateDirectory(ProjectileAnimationDirectory);
        Directory.CreateDirectory(EffectAnimationDirectory);
        Directory.CreateDirectory(SkillDirectory);
        Directory.CreateDirectory("Assets/Resources/GameData/Units");

        ConfigureAndSliceSheet(IdleSpritePath, "Idle", 6);
        ConfigureAndSliceSheet(MoveSpritePath, "Move", 8);
        ConfigureAndSliceSheet(AttackSpritePath, "Attack", 7);
        ConfigureAndSliceSheet(DieSpritePath, "Die", 4);
        ConfigureAndSliceSheet(SkillSpritePath, "Skill", 9);
        ConfigureAndSliceSheet(PoisonVialSpritePath, "PoisonVialFlight", 8);
        ConfigureAndSliceSheet(ToxicFlaskSpritePath, "ToxicFlaskFlight", 8);
        ConfigureAndSliceSheet(ToxicPoolSpritePath, "ToxicPoolLoop", 8);

        var idleSprites = LoadSprites(IdleSpritePath, 6);
        var moveSprites = LoadSprites(MoveSpritePath, 8);
        var attackSprites = LoadSprites(AttackSpritePath, 7);
        var dieSprites = LoadSprites(DieSpritePath, 4);
        var skillSprites = LoadSprites(SkillSpritePath, 9);
        var poisonVialSprites = LoadSprites(PoisonVialSpritePath, 8);
        var toxicFlaskSprites = LoadSprites(ToxicFlaskSpritePath, 8);
        var toxicPoolSprites = LoadSprites(ToxicPoolSpritePath, 8);

        ValidateScale(idleSprites, moveSprites, attackSprites, skillSprites);

        var idleClip = CreateClip(idleSprites, "PlagueAlchemist_Idle", IdleClipPath, 2f / 15f, true);
        var moveClip = CreateClip(moveSprites, "PlagueAlchemist_Move", MoveClipPath, 0.1f, true);
        var attackClip = CreateClip(
            attackSprites,
            "PlagueAlchemist_Attack",
            AttackClipPath,
            0.1f,
            false,
            new[]
            {
                new AnimationEvent { functionName = nameof(UnitAnimationEvent.OnAttackHit), time = 0.3f },
                new AnimationEvent { functionName = nameof(UnitAnimationEvent.OnAttackFinished), time = 0.58f }
            });
        var dieClip = CreateClip(dieSprites, "PlagueAlchemist_Die", DieClipPath, 0.15f, false);
        var skillClip = CreateClip(
            skillSprites,
            "PlagueAlchemist_Skill",
            SkillClipPath,
            0.1f,
            false,
            new[]
            {
                new AnimationEvent { functionName = nameof(UnitAnimationEvent.OnSkillApplyEvent), time = 0.5f },
                new AnimationEvent { functionName = nameof(UnitAnimationEvent.OnSkillFinishedEvent), time = 0.8f }
            });

        var poisonVialClip = CreateClip(poisonVialSprites, "PlagueAlchemist_PoisonVial_Flight", PoisonVialClipPath, 0.1f, true);
        var toxicFlaskClip = CreateClip(toxicFlaskSprites, "PlagueAlchemist_ToxicFlask_Flight", ToxicFlaskClipPath, 0.1f, true);
        var toxicPoolClip = CreateClip(toxicPoolSprites, "PlagueAlchemist_ToxicPool_Loop", ToxicPoolClipPath, 0.125f, true);

        var poisonVialController = CreateLoopController(PoisonVialControllerPath, "Flight", poisonVialClip);
        var toxicFlaskController = CreateLoopController(ToxicFlaskControllerPath, "Flight", toxicFlaskClip);
        var toxicPoolController = CreateLoopController(ToxicPoolControllerPath, "Loop", toxicPoolClip);
        var poisonVialPrefab = CreatePoisonVialPrefab(poisonVialSprites[0], poisonVialController);
        var toxicFlaskPrefab = CreateToxicFlaskPrefab(toxicFlaskSprites[0], toxicFlaskController);
        var toxicPoolPrefab = CreateToxicPoolPrefab(toxicPoolSprites[0], toxicPoolController);

        var overrideController = CreateOverrideController(idleClip, moveClip, attackClip, dieClip, skillClip);
        var unitPrefab = CreatePrefab(
            overrideController,
            idleSprites[0],
            poisonVialPrefab,
            toxicFlaskPrefab,
            toxicPoolPrefab);
        Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(PortraitPath) ?? idleSprites[0];
        SkillDataSO passiveSkill = CreatePassiveSkill(icon);
        SkillDataSO activeSkill = CreateActiveSkill(icon);
        CreateUnitData(unitPrefab, icon, passiveSkill, activeSkill);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Debug.Log("Plague Alchemist assets built: arcing attacks, poison, toxic pool skill, animation overrides, prefabs, skills, and UnitDataSO.");
    }

    private static Sprite[] LoadSprites(string path, int expectedCount)
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(sprite => ParseFrameIndex(sprite.name))
            .ToArray();
        if (sprites.Length != expectedCount)
            throw new InvalidOperationException($"Expected {expectedCount} sprites at {path}, found {sprites.Length}.");
        return sprites;
    }

    private static void ConfigureAndSliceSheet(string path, string animationName, int frameCount)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            throw new InvalidOperationException($"TextureImporter was not found for {path}.");

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 16f;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(File.ReadAllBytes(path), false))
            throw new InvalidOperationException($"Could not read {path}.");
        if (texture.width != frameCount * 100 || texture.height != 100)
            throw new InvalidOperationException($"Expected {frameCount * 100}x100 at {path}, found {texture.width}x{texture.height}.");

        var rects = new SpriteRect[frameCount];
        for (var index = 0; index < frameCount; index++)
        {
            var rect = FindOpaqueBounds(texture, index * 100, 0, 100, 100);
            rects[index] = new SpriteRect
            {
                name = $"PlagueAlchemist_{animationName}_{index}",
                rect = rect,
                alignment = SpriteAlignment.Custom,
                // Preserve the common center of each logical 100x100 cell after tight slicing.
                pivot = new Vector2(
                    ((index * 100f) + 50f - rect.xMin) / rect.width,
                    (50f - rect.yMin) / rect.height),
                border = Vector4.zero,
                spriteID = GUID.Generate()
            };
        }
        UnityEngine.Object.DestroyImmediate(texture);

        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        if (provider == null)
            throw new InvalidOperationException($"Sprite Editor data provider is unavailable for {path}.");
        provider.InitSpriteEditorDataProvider();
        var editCapability = provider.GetDataProvider<ISpriteFrameEditCapability>();
        if (editCapability == null)
            throw new InvalidOperationException($"Sprite frame edit capability is unavailable for {path}.");
        var capability = editCapability.GetEditCapability();
        if (!capability.HasCapability(EEditCapability.CreateAndDeleteSprite) ||
            !capability.HasCapability(EEditCapability.EditSpriteName) ||
            !capability.HasCapability(EEditCapability.EditSpriteRect) ||
            !capability.HasCapability(EEditCapability.EditPivot))
        {
            throw new InvalidOperationException($"Importer lacks required Sprite Editor capabilities for {path}.");
        }
        provider.SetSpriteRects(rects);
        provider.Apply();
        importer.SaveAndReimport();
    }

    private static Rect FindOpaqueBounds(Texture2D texture, int cellX, int cellY, int width, int height)
    {
        var minX = cellX + width;
        var minY = cellY + height;
        var maxX = -1;
        var maxY = -1;
        for (var y = cellY; y < cellY + height; y++)
        {
            for (var x = cellX; x < cellX + width; x++)
            {
                if (texture.GetPixel(x, y).a < 0.5f)
                    continue;
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
        }
        if (maxX < minX || maxY < minY)
            throw new InvalidOperationException($"No opaque pixels found in cell x={cellX}.");
        return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    private static AnimationClip CreateClip(
        IReadOnlyList<Sprite> sprites,
        string name,
        string path,
        float interval,
        bool loop,
        AnimationEvent[] events = null)
    {
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(path) != null)
            AssetDatabase.DeleteAsset(path);

        var clip = new AnimationClip { name = name, frameRate = 60f };
        var keys = new ObjectReferenceKeyframe[sprites.Count];
        for (var index = 0; index < sprites.Count; index++)
        {
            keys[index] = new ObjectReferenceKeyframe
            {
                time = index * interval,
                value = sprites[index]
            };
        }
        AnimationUtility.SetObjectReferenceCurve(
            clip,
            new EditorCurveBinding
            {
                path = string.Empty,
                type = typeof(SpriteRenderer),
                propertyName = "m_Sprite"
            },
            keys);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        if (events != null)
            AnimationUtility.SetAnimationEvents(clip, events);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static AnimatorOverrideController CreateOverrideController(
        AnimationClip idle,
        AnimationClip move,
        AnimationClip attack,
        AnimationClip die,
        AnimationClip skill)
    {
        if (AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(OverrideControllerPath) != null)
            AssetDatabase.DeleteAsset(OverrideControllerPath);
        var baseController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(BaseControllerPath);
        if (baseController == null)
            throw new InvalidOperationException($"Missing base controller: {BaseControllerPath}");

        var controller = new AnimatorOverrideController(baseController) { name = "Plague_Alchemist" };
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        controller.GetOverrides(overrides);
        for (var index = 0; index < overrides.Count; index++)
        {
            var key = overrides[index].Key;
            if (key == null) continue;
            var replacement = key.name switch
            {
                "Idle" => idle,
                "Move" => move,
                "Attack" => attack,
                "Die" => die,
                "Skill" => skill,
                _ => overrides[index].Value
            };
            overrides[index] = new KeyValuePair<AnimationClip, AnimationClip>(key, replacement);
        }
        controller.ApplyOverrides(overrides);
        AssetDatabase.CreateAsset(controller, OverrideControllerPath);
        return controller;
    }

    private static GameObject CreatePrefab(
        RuntimeAnimatorController controller,
        Sprite initialSprite,
        PoisonVialProjectile poisonVialPrefab,
        ToxicFlaskProjectile toxicFlaskPrefab,
        ToxicPoolArea toxicPoolPrefab)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            AssetDatabase.DeleteAsset(PrefabPath);
        var basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BasePrefabPath);
        if (basePrefab == null)
            throw new InvalidOperationException($"Missing base prefab: {BasePrefabPath}");

        var instance = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
        if (instance == null)
            throw new InvalidOperationException("Could not instantiate Unit_Base.");
        try
        {
            instance.name = "Plague_Alchemist";
            var body = instance.transform.Find("Body");
            if (body == null)
                throw new InvalidOperationException("Unit_Base/Body was not found.");
            var renderer = body.GetComponent<SpriteRenderer>();
            var animator = body.GetComponent<Animator>();
            var relay = body.GetComponent<UnitAnimationEvent>();
            if (renderer == null || animator == null || relay == null)
                throw new InvalidOperationException("Unit_Base/Body must keep SpriteRenderer, Animator, and UnitAnimationEvent.");
            renderer.sprite = initialSprite;
            animator.runtimeAnimatorController = controller;

            var firePoint = new GameObject("ThrowPoint").transform;
            firePoint.SetParent(body, false);
            firePoint.localPosition = new Vector3(0.48f, 0.28f, 0f);

            var rangedAttack = instance.AddComponent<PlagueAlchemistProjectileAttack>();
            var attackSettings = new SerializedObject(rangedAttack);
            var targetLayer = LayerMask.GetMask("Monster");
            if (targetLayer == 0)
                targetLayer = LayerMask.GetMask("Enemy");
            attackSettings.FindProperty("targetLayer").intValue = targetLayer;
            attackSettings.FindProperty("firePoint").objectReferenceValue = firePoint;
            attackSettings.FindProperty("projectilePrefab").objectReferenceValue = poisonVialPrefab;
            attackSettings.FindProperty("flightDuration").floatValue = 0.45f;
            attackSettings.FindProperty("arcHeight").floatValue = 0.8f;
            attackSettings.FindProperty("hitRadius").floatValue = 0.65f;
            attackSettings.ApplyModifiedPropertiesWithoutUndo();

            var passive = instance.AddComponent<PlagueAlchemist_ToxicAccumulation>();
            var passiveSettings = new SerializedObject(passive);
            passiveSettings.FindProperty("maximumStacks").intValue = 3;
            passiveSettings.FindProperty("upgradedMaximumStacks").intValue = 4;
            passiveSettings.FindProperty("firstStackDamageRatio").floatValue = 0.1f;
            passiveSettings.FindProperty("additionalStackDamageRatio").floatValue = 0.05f;
            passiveSettings.FindProperty("tickInterval").floatValue = 1f;
            passiveSettings.FindProperty("duration").floatValue = 4f;
            passiveSettings.ApplyModifiedPropertiesWithoutUndo();

            var active = instance.AddComponent<PlagueAlchemist_ToxicDistillation_Skill>();
            var activeSettings = new SerializedObject(active);
            activeSettings.FindProperty("throwPoint").objectReferenceValue = firePoint;
            activeSettings.FindProperty("flaskPrefab").objectReferenceValue = toxicFlaskPrefab;
            activeSettings.FindProperty("poolPrefab").objectReferenceValue = toxicPoolPrefab;
            activeSettings.FindProperty("enemyLayer").intValue = targetLayer;
            activeSettings.FindProperty("damageMultiplier").floatValue = 1.25f;
            activeSettings.FindProperty("upgradedDamageMultiplier").floatValue = 1.65f;
            activeSettings.FindProperty("impactRadius").floatValue = 1.35f;
            activeSettings.FindProperty("flightDuration").floatValue = 0.65f;
            activeSettings.FindProperty("arcHeight").floatValue = 1.5f;
            activeSettings.FindProperty("poolDuration").floatValue = 5f;
            activeSettings.FindProperty("poolRadius").floatValue = 1.6f;
            activeSettings.FindProperty("poolDamageRatioPerTick").floatValue = 0.12f;
            activeSettings.FindProperty("poolTickInterval").floatValue = 0.5f;
            activeSettings.FindProperty("poolSlowRatio").floatValue = 0.25f;
            activeSettings.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath, out var success);
            if (prefab == null || !success)
                throw new InvalidOperationException($"Could not save prefab variant: {PrefabPath}");
            return prefab;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(instance);
        }
    }

    private static RuntimeAnimatorController CreateLoopController(
        string path,
        string stateName,
        AnimationClip clip)
    {
        if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path) != null)
            AssetDatabase.DeleteAsset(path);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        var state = controller.layers[0].stateMachine.AddState(stateName);
        state.motion = clip;
        controller.layers[0].stateMachine.defaultState = state;
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static PoisonVialProjectile CreatePoisonVialPrefab(
        Sprite initialSprite,
        RuntimeAnimatorController controller)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PoisonVialPrefabPath) != null)
            AssetDatabase.DeleteAsset(PoisonVialPrefabPath);
        var gameObject = CreateAnimatedVisual("PlagueAlchemist_PoisonVial", initialSprite, controller, 6);
        try
        {
            gameObject.AddComponent<Poolable>();
            var projectile = gameObject.AddComponent<PoisonVialProjectile>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, PoisonVialPrefabPath);
            if (prefab == null)
                throw new InvalidOperationException($"Could not save prefab: {PoisonVialPrefabPath}");
            return prefab.GetComponent<PoisonVialProjectile>();
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }

    private static ToxicFlaskProjectile CreateToxicFlaskPrefab(Sprite initialSprite, RuntimeAnimatorController controller)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(ToxicFlaskPrefabPath) != null)
            AssetDatabase.DeleteAsset(ToxicFlaskPrefabPath);
        var gameObject = CreateAnimatedVisual("PlagueAlchemist_ToxicFlask", initialSprite, controller, 6);
        try
        {
            gameObject.AddComponent<Poolable>();
            gameObject.AddComponent<ToxicFlaskProjectile>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, ToxicFlaskPrefabPath);
            if (prefab == null)
                throw new InvalidOperationException($"Could not save prefab: {ToxicFlaskPrefabPath}");
            return prefab.GetComponent<ToxicFlaskProjectile>();
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }

    private static ToxicPoolArea CreateToxicPoolPrefab(Sprite initialSprite, RuntimeAnimatorController controller)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(ToxicPoolPrefabPath) != null)
            AssetDatabase.DeleteAsset(ToxicPoolPrefabPath);
        var gameObject = CreateAnimatedVisual("PlagueAlchemist_ToxicPool", initialSprite, controller, 1);
        try
        {
            var renderer = gameObject.GetComponent<SpriteRenderer>();
            renderer.color = new Color(1f, 1f, 1f, 0.45f);
            gameObject.AddComponent<Poolable>();
            var poolArea = gameObject.AddComponent<ToxicPoolArea>();
            var poolSettings = new SerializedObject(poolArea);
            poolSettings.FindProperty("maximumOpacity").floatValue = 0.45f;
            poolSettings.FindProperty("fadeInDuration").floatValue = 0.15f;
            poolSettings.FindProperty("fadeOutDuration").floatValue = 0.4f;
            poolSettings.ApplyModifiedPropertiesWithoutUndo();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, ToxicPoolPrefabPath);
            if (prefab == null)
                throw new InvalidOperationException($"Could not save prefab: {ToxicPoolPrefabPath}");
            return prefab.GetComponent<ToxicPoolArea>();
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }

    private static GameObject CreateAnimatedVisual(
        string name,
        Sprite initialSprite,
        RuntimeAnimatorController controller,
        int sortingOrder)
    {
        var gameObject = new GameObject(name);
        var renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = initialSprite;
        renderer.sortingOrder = sortingOrder;
        var animator = gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        return gameObject;
    }

    private static SkillDataSO CreatePassiveSkill(Sprite icon)
    {
        SkillDataSO skill = LoadOrCreateAsset<SkillDataSO>(PassiveSkillPath);
        skill.name = "PlagueAlchemist_Passive";
        skill.skillName = "독성 축적";
        skill.icon = icon;
        skill.skillType = SkillType.Passive;
        skill.onHitStatusEffect = new SkillOnHitStatusEffectSettings();
        skill.upgrades = new List<SkillUpgradeData>
        {
            new()
            {
                promotionLevel = 0,
                description = "기본 공격이 중독을 부여하며 같은 적을 연속 공격하면 최대 3단계까지 피해가 증가한다."
            },
            new()
            {
                promotionLevel = 2,
                description = "독성 축적의 최대 단계가 4단계로 증가한다."
            },
            new()
            {
                promotionLevel = 4,
                description = "전투 시작 시 에너지를 50% 획득한다."
            }
        };
        EditorUtility.SetDirty(skill);
        return skill;
    }

    private static SkillDataSO CreateActiveSkill(Sprite icon)
    {
        SkillDataSO skill = LoadOrCreateAsset<SkillDataSO>(ActiveSkillPath);
        skill.name = "PlagueAlchemist_Active";
        skill.skillName = "역병 증류";
        skill.icon = icon;
        skill.skillType = SkillType.Active;
        skill.onHitStatusEffect = new SkillOnHitStatusEffectSettings
        {
            enabled = true,
            type = StatusEffectType.Poison,
            chance = 1f,
            duration = 4f,
            magnitude = 0.12f,
            magnitudeMode = SkillStatusMagnitudeMode.SourceAttackRatio,
            tickInterval = 1f
        };
        skill.upgrades = new List<SkillUpgradeData>
        {
            new()
            {
                promotionLevel = 0,
                description = "대형 플라스크를 던져 범위 피해를 입히고 5초 동안 피해와 감속을 주는 독성 지대를 만든다."
            },
            new()
            {
                promotionLevel = 2,
                description = "플라스크 착탄 피해가 공격력의 125%에서 165%로 증가한다."
            },
            new()
            {
                promotionLevel = 4,
                description = "전투 시작 시 에너지를 50% 획득한다."
            }
        };
        EditorUtility.SetDirty(skill);
        return skill;
    }

    private static void CreateUnitData(
        GameObject unitPrefab,
        Sprite icon,
        SkillDataSO passiveSkill,
        SkillDataSO activeSkill)
    {
        UnitDataSO data = LoadOrCreateAsset<UnitDataSO>(UnitDataPath);
        data.name = "Plague_Alchemist";
        data.unitId = "unit_plague_alchemist";
        data.displayName = "역병 연금술사";
        data.icon = icon;
        data.rarity = Rarity.Legend;
        data.unitPrefab = unitPrefab;
        data.passiveSkill = passiveSkill;
        data.activeSkill = activeSkill;
        data.baseStats = new UnitStats(
            attack: 42f,
            maxHp: 320f,
            attackPerSec: 0.85f,
            detectRange: 5.5f,
            critChance: 0.08f,
            critDamage: 1.5f,
            energyRecovery: 11f);
        data.maxLevel = 50;
        data.attackGrowthValue = 2;
        data.hpGrowthValue = 10;
        data.starAttackMultipliers = new[] { 1f, 1.35f, 1.8f, 2.4f };
        data.starHpMultipliers = new[] { 1f, 1.25f, 1.6f, 2.05f };
        data.starDetectRangeMultipliers = new[] { 1f, 1.03f, 1.06f, 1.1f };
        data.starAttackPerSecondMultipliers = new[] { 1f, 1.05f, 1.1f, 1.15f };
        data.promotionCost = new[]
        {
            new PromotionCost { MaterialId = "material_prom_1", Count = 5 },
            new PromotionCost { MaterialId = "material_prom_2", Count = 10 },
            new PromotionCost { MaterialId = "material_prom_3", Count = 15 },
            new PromotionCost { MaterialId = "material_prom_4", Count = 20 }
        };
        data.limitBreaks = new List<LimitBreakData>
        {
            new() { statType = StatType.Attack, value = 10f, description = "공격력 10% 증가" },
            new() { statType = StatType.EnergyRecovery, value = 10f, description = "에너지 회복 10% 증가" },
            new() { statType = StatType.CritChance, value = 15f, description = "치명타 확률 15% 증가" },
            new() { statType = StatType.AttackPerSec, value = 10f, description = "공격 속도 10% 증가" },
            new() { statType = StatType.CritDamage, value = 15f, description = "치명타 피해 15% 증가" }
        };
        EditorUtility.SetDirty(data);
    }

    private static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null)
            return asset;

        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void ValidateScale(params IReadOnlyList<Sprite>[] groups)
    {
        var idleHeight = groups[0][0].rect.height;
        foreach (var group in groups)
        {
            foreach (var sprite in group)
            {
                if (sprite.rect.height < idleHeight - 3f || sprite.rect.height > idleHeight + 3f)
                    throw new InvalidOperationException($"Unexpected body-scale drift in {sprite.name}: {sprite.rect.height}px vs Idle {idleHeight}px.");
            }
        }
    }

    private static int ParseFrameIndex(string name)
    {
        var separator = name.LastIndexOf('_');
        return separator >= 0 && int.TryParse(name[(separator + 1)..], out var index) ? index : int.MaxValue;
    }
}
