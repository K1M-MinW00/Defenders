using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.AI;

public static class MagmaMawAssetBuilder
{
    private const string MonsterArt = "Assets/Art/Generated/Monsters/MagmaMaw/";
    private const string ProjectileArt = "Assets/Art/Generated/Projectiles/MagmaMaw/";
    private const string EffectArt = "Assets/Art/Generated/Effects/MagmaMaw/";
    private const string AnimationRoot = "Assets/Animations/Monsters/MagmaMaw";
    private const string ProjectileAnimationRoot = "Assets/Animations/Projectiles/MagmaMaw";
    private const string EffectAnimationRoot = "Assets/Animations/Effects/MagmaMaw";
    private const string BaseControllerPath = "Assets/Animations/Monsters/Base/Monster_Base.controller";
    private const string BasePrefabPath = "Assets/Prefabs/Monsters/Monster_Base.prefab";
    private const string PrefabPath = "Assets/Prefabs/Monsters/MagmaMaw.prefab";
    private const string DataPath = "Assets/GameData/Monsters/Bosses/Magma_Maw.asset";
    private const string DefinitionPath = "Assets/GameData/Monsters/Bosses/Definitions/Boss_Magma_Maw_Definition.asset";
    private const string SmallProjectilePath = "Assets/Prefabs/Projectile/MagmaMaw_SmallFireball.prefab";
    private const string LargeProjectilePath = "Assets/Prefabs/Projectile/MagmaMaw_LargeMagmaBall.prefab";
    private const string ImpactPrefabPath = "Assets/Prefabs/Skill_Effect/MagmaMaw_LargeMagmaBall_Impact.prefab";

    private static readonly SheetSpec Idle = new(MonsterArt + "MagmaMaw-Idle.png", "Idle", 6, 256, 0.13f);
    private static readonly SheetSpec Attack = new(MonsterArt + "MagmaMaw-Attack.png", "Attack", 9, 256, 0.1f);
    private static readonly SheetSpec Death = new(MonsterArt + "MagmaMaw-Death.png", "Death", 4, 256, 0.16f);
    private static readonly SheetSpec SmallFireball = new(ProjectileArt + "MagmaMaw-SmallFireball-Flight.png", "SmallFireball", 4, 48, 0.1f);
    private static readonly SheetSpec LargeMagmaBall = new(ProjectileArt + "MagmaMaw-LargeMagmaBall-Flight.png", "LargeMagmaBall", 6, 80, 0.1f);
    private static readonly SheetSpec Impact = new(EffectArt + "MagmaMaw-LargeMagmaBall-Impact.png", "Impact", 7, 128, 0.1f);

    [MenuItem("Tools/Defenders/Build Magma Maw Assets")]
    public static void Build()
    {
        Directory.CreateDirectory(AnimationRoot);
        Directory.CreateDirectory(ProjectileAnimationRoot);
        Directory.CreateDirectory(EffectAnimationRoot);
        Directory.CreateDirectory("Assets/GameData/Monsters/Bosses/Definitions");
        Directory.CreateDirectory("Assets/Prefabs/Projectile");
        Directory.CreateDirectory("Assets/Prefabs/Skill_Effect");

        ConfigureAndSlice(Idle, 128f, 113f);
        ConfigureAndSlice(Attack, 128f, 113f);
        ConfigureAndSlice(Death, 128f, 113f);
        ConfigureAndSlice(SmallFireball, 24f, 24f);
        ConfigureAndSlice(LargeMagmaBall, 40f, 40f);
        ConfigureAndSlice(Impact, 64f, 24f);

        Sprite[] idleSprites = LoadSprites(Idle);
        Sprite[] attackSprites = LoadSprites(Attack);
        Sprite[] deathSprites = LoadSprites(Death);
        Sprite[] smallSprites = LoadSprites(SmallFireball);
        Sprite[] largeSprites = LoadSprites(LargeMagmaBall);
        Sprite[] impactSprites = LoadSprites(Impact);

        ValidateFrameCount(Idle, idleSprites);
        ValidateFrameCount(Attack, attackSprites);
        ValidateFrameCount(Death, deathSprites);
        ValidateFrameCount(SmallFireball, smallSprites);
        ValidateFrameCount(LargeMagmaBall, largeSprites);
        ValidateFrameCount(Impact, impactSprites);
        ValidateBossScale(idleSprites);

        AnimationClip idleClip = CreateClip(idleSprites, "MagmaMaw_Idle", AnimationRoot + "/MagmaMaw_Idle.anim", Idle.Interval, true);
        AnimationClip attackClip = CreateClip(
            attackSprites,
            "MagmaMaw_Attack",
            AnimationRoot + "/MagmaMaw_Attack.anim",
            Attack.Interval,
            false,
            new[]
            {
                new AnimationEvent { functionName = nameof(MonsterAnimationEvent.OnAttackHit), time = 0.5f },
                new AnimationEvent { functionName = nameof(MonsterAnimationEvent.OnAttackFinished), time = 0.8f }
            });
        AnimationClip deathClip = CreateClip(deathSprites, "MagmaMaw_Death", AnimationRoot + "/MagmaMaw_Death.anim", Death.Interval, false);
        AnimatorOverrideController monsterController = CreateMonsterController(idleClip, attackClip, deathClip);

        RuntimeAnimatorController smallController = CreateSingleStateController(
            CreateClip(smallSprites, "MagmaMaw_SmallFireball_Flight", ProjectileAnimationRoot + "/MagmaMaw_SmallFireball_Flight.anim", SmallFireball.Interval, true),
            ProjectileAnimationRoot + "/MagmaMaw_SmallFireball.controller");
        RuntimeAnimatorController largeController = CreateSingleStateController(
            CreateClip(largeSprites, "MagmaMaw_LargeMagmaBall_Flight", ProjectileAnimationRoot + "/MagmaMaw_LargeMagmaBall_Flight.anim", LargeMagmaBall.Interval, true),
            ProjectileAnimationRoot + "/MagmaMaw_LargeMagmaBall.controller");
        RuntimeAnimatorController impactController = CreateSingleStateController(
            CreateClip(impactSprites, "MagmaMaw_LargeMagmaBall_Impact", EffectAnimationRoot + "/MagmaMaw_LargeMagmaBall_Impact.anim", Impact.Interval, false),
            EffectAnimationRoot + "/MagmaMaw_LargeMagmaBall_Impact.controller");

        MagmaMawImpactEffect impactPrefab = CreateImpactPrefab(impactSprites[0], impactController);
        ArrowProjectile smallPrefab = CreateSmallProjectile(smallSprites[0], smallController);
        MagmaMawMagmaBallProjectile largePrefab = CreateLargeProjectile(largeSprites[0], largeController, impactPrefab);
        BossDefinitionSO definition = CreateBossDefinition();
        GameObject monsterPrefab = CreateMonsterPrefab(monsterController, idleSprites[0], smallPrefab, largePrefab);
        CreateMonsterData(monsterPrefab, idleSprites[0], definition);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Debug.Log("Magma Maw assets built: sprite slices, animations, projectiles, impact, boss prefab, definition, and MonsterDataSO.");
    }

    private static void ConfigureAndSlice(SheetSpec spec, float anchorX, float anchorY)
    {
        AssetDatabase.ImportAsset(spec.Path, ImportAssetOptions.ForceSynchronousImport);
        var importer = AssetImporter.GetAtPath(spec.Path) as TextureImporter;
        if (importer == null)
            throw new InvalidOperationException($"Texture importer missing: {spec.Path}");

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
        if (!texture.LoadImage(File.ReadAllBytes(spec.Path), false))
            throw new InvalidOperationException($"Could not decode {spec.Path}");
        if (texture.width != spec.Frames * spec.CellSize || texture.height != spec.CellSize)
            throw new InvalidOperationException($"Unexpected dimensions for {spec.Path}: {texture.width}x{texture.height}");

        var rects = new SpriteRect[spec.Frames];
        for (int index = 0; index < spec.Frames; index++)
        {
            Rect rect = FindOpaqueBounds(texture, index * spec.CellSize, spec.CellSize);
            float logicalAnchorX = index * spec.CellSize + anchorX;
            rects[index] = new SpriteRect
            {
                name = $"MagmaMaw_{spec.Name}_{index}",
                rect = rect,
                alignment = SpriteAlignment.Custom,
                pivot = new Vector2(
                    Mathf.Clamp01((logicalAnchorX - rect.xMin) / rect.width),
                    Mathf.Clamp01((anchorY - rect.yMin) / rect.height)),
                spriteID = GUID.Generate()
            };
        }

        UnityEngine.Object.DestroyImmediate(texture);
        var factories = new SpriteDataProviderFactories();
        factories.Init();
        ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        provider.SetSpriteRects(rects);
        provider.Apply();
        importer.SaveAndReimport();
    }

    private static Rect FindOpaqueBounds(Texture2D texture, int cellX, int cellSize)
    {
        int minX = cellX + cellSize;
        int minY = cellSize;
        int maxX = -1;
        int maxY = -1;
        for (int y = 0; y < cellSize; y++)
        for (int x = cellX; x < cellX + cellSize; x++)
        {
            if (texture.GetPixel(x, y).a < 0.5f)
                continue;
            minX = Math.Min(minX, x);
            minY = Math.Min(minY, y);
            maxX = Math.Max(maxX, x);
            maxY = Math.Max(maxY, y);
        }

        if (maxX < minX)
            throw new InvalidOperationException($"Empty sprite cell at x={cellX} in {texture.name}");
        return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    private static Sprite[] LoadSprites(SheetSpec spec) => AssetDatabase.LoadAllAssetsAtPath(spec.Path)
        .OfType<Sprite>()
        .OrderBy(sprite => ParseIndex(sprite.name))
        .ToArray();

    private static void ValidateFrameCount(SheetSpec spec, Sprite[] sprites)
    {
        if (sprites.Length != spec.Frames)
            throw new InvalidOperationException($"Expected {spec.Frames} sprites in {spec.Path}, found {sprites.Length}.");
    }

    private static void ValidateBossScale(Sprite[] idle)
    {
        float idleHeight = idle.Select(sprite => sprite.rect.height).OrderBy(value => value).ElementAt(idle.Length / 2);
        if (idleHeight < 80f || idleHeight > 160f)
            throw new InvalidOperationException($"Magma Maw Idle opaque height {idleHeight}px is outside the giant-boss range.");
    }

    private static AnimationClip CreateClip(Sprite[] sprites, string name, string path, float interval, bool loop, AnimationEvent[] events = null)
    {
        DeleteExisting(path);
        var clip = new AnimationClip { name = name, frameRate = 60f };
        var keys = sprites.Select((sprite, index) => new ObjectReferenceKeyframe
        {
            time = index * interval,
            value = sprite
        }).ToArray();
        AnimationUtility.SetObjectReferenceCurve(clip, new EditorCurveBinding
        {
            path = string.Empty,
            type = typeof(SpriteRenderer),
            propertyName = "m_Sprite"
        }, keys);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        if (events != null)
            AnimationUtility.SetAnimationEvents(clip, events);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static AnimatorOverrideController CreateMonsterController(AnimationClip idle, AnimationClip attack, AnimationClip death)
    {
        string path = AnimationRoot + "/MagmaMaw.overrideController";
        DeleteExisting(path);
        RuntimeAnimatorController baseController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(BaseControllerPath);
        var controller = new AnimatorOverrideController(baseController) { name = "MagmaMaw" };
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        controller.GetOverrides(overrides);
        for (int i = 0; i < overrides.Count; i++)
        {
            string key = overrides[i].Key?.name;
            if (key == "Monster_Idle" || key == "Monster_Move")
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, idle);
            else if (key == "Monster_Attack")
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, attack);
            else if (key == "Monster_Die")
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, death);
        }
        controller.ApplyOverrides(overrides);
        AssetDatabase.CreateAsset(controller, path);
        return controller;
    }

    private static RuntimeAnimatorController CreateSingleStateController(AnimationClip clip, string path)
    {
        DeleteExisting(path);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        AnimatorState state = controller.layers[0].stateMachine.AddState("Play");
        state.motion = clip;
        controller.layers[0].stateMachine.defaultState = state;
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static MagmaMawImpactEffect CreateImpactPrefab(Sprite sprite, RuntimeAnimatorController controller)
    {
        DeleteExisting(ImpactPrefabPath);
        var go = new GameObject("MagmaMaw_LargeMagmaBall_Impact");
        try
        {
            AddAnimatedRenderer(go, sprite, controller, 8);
            go.AddComponent<Poolable>();
            MagmaMawImpactEffect effect = go.AddComponent<MagmaMawImpactEffect>();
            SetFloat(effect, "duration", 0.72f);
            return PrefabUtility.SaveAsPrefabAsset(go, ImpactPrefabPath).GetComponent<MagmaMawImpactEffect>();
        }
        finally { UnityEngine.Object.DestroyImmediate(go); }
    }

    private static ArrowProjectile CreateSmallProjectile(Sprite sprite, RuntimeAnimatorController controller)
    {
        DeleteExisting(SmallProjectilePath);
        var go = new GameObject("MagmaMaw_SmallFireball");
        try
        {
            AddAnimatedRenderer(go, sprite, controller, 7);
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.55f;
            go.AddComponent<Poolable>();
            ArrowProjectile projectile = go.AddComponent<ArrowProjectile>();
            SetFloat(projectile, "lifeTime", 5f);
            return PrefabUtility.SaveAsPrefabAsset(go, SmallProjectilePath).GetComponent<ArrowProjectile>();
        }
        finally { UnityEngine.Object.DestroyImmediate(go); }
    }

    private static MagmaMawMagmaBallProjectile CreateLargeProjectile(Sprite sprite, RuntimeAnimatorController controller, MagmaMawImpactEffect impact)
    {
        DeleteExisting(LargeProjectilePath);
        var go = new GameObject("MagmaMaw_LargeMagmaBall");
        try
        {
            AddAnimatedRenderer(go, sprite, controller, 7);
            go.AddComponent<Poolable>();
            MagmaMawMagmaBallProjectile projectile = go.AddComponent<MagmaMawMagmaBallProjectile>();
            var serialized = new SerializedObject(projectile);
            serialized.FindProperty("lifetime").floatValue = 6f;
            serialized.FindProperty("impactEffectPrefab").objectReferenceValue = impact;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return PrefabUtility.SaveAsPrefabAsset(go, LargeProjectilePath).GetComponent<MagmaMawMagmaBallProjectile>();
        }
        finally { UnityEngine.Object.DestroyImmediate(go); }
    }

    private static BossDefinitionSO CreateBossDefinition()
    {
        BossDefinitionSO definition = AssetDatabase.LoadAssetAtPath<BossDefinitionSO>(DefinitionPath);
        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<BossDefinitionSO>();
            AssetDatabase.CreateAsset(definition, DefinitionPath);
        }
        definition.bossId = "Magma_Maw";
        definition.shieldTriggerHpRatio = 0.55f;
        definition.shieldMaxHpRatio = 0.15f;
        definition.enragedDamageMultiplier = 1.25f;
        definition.enragedAttackSpeedMultiplier = 1.2f;
        definition.areaAttackStatusEffects = new BossAreaAttackStatusSettings();
        definition.summon = new BossSummonSettings { enabled = false };
        EditorUtility.SetDirty(definition);
        return definition;
    }

    private static GameObject CreateMonsterPrefab(RuntimeAnimatorController controller, Sprite idle, ArrowProjectile small, MagmaMawMagmaBallProjectile large)
    {
        DeleteExisting(PrefabPath);
        GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BasePrefabPath);
        GameObject instance = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
        if (instance == null)
            throw new InvalidOperationException("Could not instantiate Monster_Base.");
        try
        {
            instance.name = "MagmaMaw";
            Transform body = instance.transform.Find("Body");
            SpriteRenderer renderer = body.GetComponent<SpriteRenderer>();
            renderer.sprite = idle;
            renderer.flipX = true;
            body.GetComponent<Animator>().runtimeAnimatorController = controller;
            if (body.GetComponent<MonsterAnimationEvent>() == null)
                body.gameObject.AddComponent<MonsterAnimationEvent>();

            Transform firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(body, false);
            firePoint.localPosition = new Vector3(4.8f, 2.7f, 0f);

            MagmaMawAttack attack = instance.AddComponent<MagmaMawAttack>();
            var attackData = new SerializedObject(attack);
            attackData.FindProperty("useAnimationEvents").boolValue = true;
            attackData.FindProperty("firePoint").objectReferenceValue = firePoint;
            attackData.FindProperty("smallFireballPrefab").objectReferenceValue = small;
            attackData.FindProperty("largeMagmaBallPrefab").objectReferenceValue = large;
            int characterLayer = LayerMask.NameToLayer("Character");
            attackData.FindProperty("targetLayer").intValue = characterLayer >= 0 ? 1 << characterLayer : 0;
            attackData.ApplyModifiedPropertiesWithoutUndo();

            var collider = instance.GetComponent<CircleCollider2D>();
            collider.radius = 4.6f;
            collider.offset = new Vector2(0f, 2.4f);
            NavMeshAgent agent = instance.GetComponent<NavMeshAgent>();
            agent.radius = 2.5f;
            agent.height = 5f;
            agent.speed = 0f;

            BossAreaAttackAbility areaAbility = instance.AddComponent<BossAreaAttackAbility>();
            areaAbility.enabled = false;

            return PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(instance); }
    }

    private static void CreateMonsterData(GameObject prefab, Sprite portrait, BossDefinitionSO definition)
    {
        MonsterDataSO data = AssetDatabase.LoadAssetAtPath<MonsterDataSO>(DataPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<MonsterDataSO>();
            AssetDatabase.CreateAsset(data, DataPath);
        }
        var serialized = new SerializedObject(data);
        serialized.FindProperty("monsterId").stringValue = "Magma_Maw";
        serialized.FindProperty("displayName").stringValue = "용암 포식귀";
        serialized.FindProperty("portrait").objectReferenceValue = portrait;
        serialized.FindProperty("description").stringValue = "용암 구덩이에 몸을 고정한 채 가장 가까운 적에게 화염탄과 거대한 용암탄을 뱉는 초거대 보스";
        serialized.FindProperty("prefab").objectReferenceValue = prefab;
        serialized.FindProperty("classification").enumValueIndex = (int)MonsterClassification.Boss;
        serialized.FindProperty("visualScaleOverride").floatValue = 1f;
        serialized.FindProperty("bossDefinition").objectReferenceValue = definition;
        SerializedProperty stats = serialized.FindProperty("baseStats");
        stats.FindPropertyRelative("maxHp").floatValue = 2200f;
        stats.FindPropertyRelative("moveSpeed").floatValue = 0f;
        stats.FindPropertyRelative("atkDamage").floatValue = 38f;
        stats.FindPropertyRelative("atkRange").floatValue = 30f;
        stats.FindPropertyRelative("atkPerSec").floatValue = 0.55f;
        serialized.FindProperty("threatCost").floatValue = 20f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(data);
    }

    private static void AddAnimatedRenderer(GameObject go, Sprite sprite, RuntimeAnimatorController controller, int sortingOrder)
    {
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        go.AddComponent<Animator>().runtimeAnimatorController = controller;
    }

    private static void SetFloat(UnityEngine.Object target, string property, float value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(property).floatValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void DeleteExisting(string path)
    {
        if (AssetDatabase.LoadMainAssetAtPath(path) != null)
            AssetDatabase.DeleteAsset(path);
    }

    private static int ParseIndex(string name)
    {
        int separator = name.LastIndexOf('_');
        return separator >= 0 && int.TryParse(name[(separator + 1)..], out int value) ? value : int.MaxValue;
    }

    private readonly struct SheetSpec
    {
        public readonly string Path;
        public readonly string Name;
        public readonly int Frames;
        public readonly int CellSize;
        public readonly float Interval;

        public SheetSpec(string path, string name, int frames, int cellSize, float interval)
        {
            Path = path;
            Name = name;
            Frames = frames;
            CellSize = cellSize;
            Interval = interval;
        }
    }
}
