using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class WraithShamanAssetBuilder
{
    private const string SpritePath = "Assets/Art/FirstParty/Runtime/Monsters/WraithShaman/WraithShaman-Idle.png";
    private const string WalkSpritePath = "Assets/Art/FirstParty/Runtime/Monsters/WraithShaman/WraithShaman-Walk.png";
    private const string AttackSpritePath = "Assets/Art/FirstParty/Runtime/Monsters/WraithShaman/WraithShaman-Attack.png";
    private const string DieSpritePath = "Assets/Art/FirstParty/Runtime/Monsters/WraithShaman/WraithShaman-Die.png";
    private const string OutputDirectory = "Assets/Animations/Monsters/WraithShaman";
    private const string IdleClipPath = OutputDirectory + "/WraithShaman_Idle.anim";
    private const string WalkClipPath = OutputDirectory + "/WraithShaman_Walk.anim";
    private const string AttackClipPath = OutputDirectory + "/WraithShaman_Attack.anim";
    private const string DieClipPath = OutputDirectory + "/WraithShaman_Die.anim";
    private const string OverrideControllerPath = OutputDirectory + "/WraithShaman.overrideController";
    private const string BaseControllerPath = "Assets/Animations/Monsters/Base/Monster_Base.controller";
    private const string BasePrefabPath = "Assets/Prefabs/Monsters/Monster_Base.prefab";
    private const string PrefabPath = "Assets/Prefabs/Monsters/WraithShaman.prefab";
    private const string ProjectileSpritePath = "Assets/Art/FirstParty/Runtime/Projectiles/WraithShaman/WraithShaman-SoulBolt-Flight.png";
    private const string ProjectileAnimationDirectory = "Assets/Animations/Projectiles/WraithShaman";
    private const string ProjectileClipPath = ProjectileAnimationDirectory + "/WraithShaman_SoulBolt_Flight.anim";
    private const string ProjectileControllerPath = ProjectileAnimationDirectory + "/WraithShaman_SoulBolt.controller";
    private const string ProjectilePath = "Assets/Prefabs/Projectile/WraithShaman_SoulBolt.prefab";
    private const string MonsterDataPath = "Assets/GameData/Monsters/Normal/Wraith_Shaman.asset";

    public static void Build()
    {
        Directory.CreateDirectory(OutputDirectory);
        Directory.CreateDirectory(ProjectileAnimationDirectory);
        ConfigureAndSliceSheet(SpritePath, "Idle", 6);
        ConfigureAndSliceSheet(WalkSpritePath, "Walk", 8);
        ConfigureAndSliceSheet(AttackSpritePath, "Attack", 8);
        ConfigureAndSliceSheet(DieSpritePath, "Die", 4);
        ConfigureAndSliceSheet(ProjectileSpritePath, "SoulBoltFlight", 6);

        var idleSprites = AssetDatabase.LoadAllAssetsAtPath(SpritePath)
            .OfType<Sprite>()
            .OrderBy(sprite => ParseFrameIndex(sprite.name))
            .ToArray();
        var walkSprites = AssetDatabase.LoadAllAssetsAtPath(WalkSpritePath)
            .OfType<Sprite>()
            .OrderBy(sprite => ParseFrameIndex(sprite.name))
            .ToArray();
        var attackSprites = AssetDatabase.LoadAllAssetsAtPath(AttackSpritePath)
            .OfType<Sprite>()
            .OrderBy(sprite => ParseFrameIndex(sprite.name))
            .ToArray();
        var dieSprites = AssetDatabase.LoadAllAssetsAtPath(DieSpritePath)
            .OfType<Sprite>()
            .OrderBy(sprite => ParseFrameIndex(sprite.name))
            .ToArray();
        var projectileSprites = AssetDatabase.LoadAllAssetsAtPath(ProjectileSpritePath)
            .OfType<Sprite>()
            .OrderBy(sprite => ParseFrameIndex(sprite.name))
            .ToArray();

        if (idleSprites.Length != 6)
            throw new InvalidOperationException($"Expected 6 Idle sprites at {SpritePath}, found {idleSprites.Length}.");
        if (walkSprites.Length != 8)
            throw new InvalidOperationException($"Expected 8 Walk sprites at {WalkSpritePath}, found {walkSprites.Length}.");
        if (attackSprites.Length != 8)
            throw new InvalidOperationException($"Expected 8 Attack sprites at {AttackSpritePath}, found {attackSprites.Length}.");
        if (dieSprites.Length != 4)
            throw new InvalidOperationException($"Expected 4 Die sprites at {DieSpritePath}, found {dieSprites.Length}.");
        if (projectileSprites.Length != 6)
            throw new InvalidOperationException($"Expected 6 soul bolt sprites at {ProjectileSpritePath}, found {projectileSprites.Length}.");

        ValidateActionScale(idleSprites, attackSprites);

        var idleClip = CreateClip(idleSprites, "WraithShaman_Idle", IdleClipPath, 2f / 15f, true);
        var walkClip = CreateClip(walkSprites, "WraithShaman_Walk", WalkClipPath, 0.1f, true);
        var attackClip = CreateClip(
            attackSprites,
            "WraithShaman_Attack",
            AttackClipPath,
            0.1f,
            false,
            new[]
            {
                new AnimationEvent { functionName = "OnAttackHit", time = 0.4f },
                new AnimationEvent { functionName = "OnAttackFinished", time = 0.7f }
            });
        var dieClip = CreateClip(dieSprites, "WraithShaman_Die", DieClipPath, 0.15f, false);
        var overrideController = CreateOverrideController(idleClip, walkClip, attackClip, dieClip);
        var projectileClip = CreateClip(
            projectileSprites,
            "WraithShaman_SoulBolt_Flight",
            ProjectileClipPath,
            0.125f,
            true);
        var projectileController = CreateProjectileController(projectileClip);
        var projectile = CreateSoulBoltProjectile(projectileSprites[0], projectileController);
        var monsterPrefab = CreateMonsterPrefab(overrideController, idleSprites[0], projectile);
        CreateMonsterData(monsterPrefab, idleSprites[0]);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Debug.Log("Wraith Shaman assets built: animations, soul bolt, ranged Monster_Base prefab variant, and MonsterDataSO.");
    }

    private static void ConfigureAndSliceSheet(string spritePath, string animationName, int frameCount)
    {
        AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceSynchronousImport);
        var importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
        if (importer == null)
            throw new InvalidOperationException($"TextureImporter was not found for {spritePath}.");

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 16f;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();

        var png = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!png.LoadImage(File.ReadAllBytes(spritePath), false))
            throw new InvalidOperationException($"Could not read pixels from {spritePath}.");
        if (png.width != frameCount * 100 || png.height != 100)
            throw new InvalidOperationException($"Expected a {frameCount * 100}x100 {animationName} sheet, found {png.width}x{png.height}.");

        var frames = new SpriteRect[frameCount];
        for (var index = 0; index < frames.Length; index++)
        {
            var rect = FindOpaqueBounds(png, index * 100, 0, 100, 100);
            frames[index] = new SpriteRect
            {
                name = $"WraithShaman_{animationName}_{index}",
                rect = rect,
                alignment = SpriteAlignment.Custom,
                // Keep the logical 100x100 cell center as the common animation anchor.
                // A per-rect center pivot would cancel the intended 1px floating motion.
                pivot = new Vector2(
                    ((index * 100f) + 50f - rect.xMin) / rect.width,
                    (50f - rect.yMin) / rect.height),
                border = Vector4.zero,
                spriteID = GUID.Generate()
            };
        }

        UnityEngine.Object.DestroyImmediate(png);

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        if (dataProvider == null)
            throw new InvalidOperationException("Sprite Editor data provider is unavailable. Slicing aborted.");
        dataProvider.InitSpriteEditorDataProvider();

        var editCapability = dataProvider.GetDataProvider<ISpriteFrameEditCapability>();
        if (editCapability == null)
            throw new InvalidOperationException("Sprite frame edit capability is unavailable. Slicing aborted.");
        var capability = editCapability.GetEditCapability();
        if (!capability.HasCapability(EEditCapability.CreateAndDeleteSprite) ||
            !capability.HasCapability(EEditCapability.EditSpriteName) ||
            !capability.HasCapability(EEditCapability.EditSpriteRect) ||
            !capability.HasCapability(EEditCapability.EditPivot))
        {
            throw new InvalidOperationException("Importer lacks required Sprite Editor capabilities. Slicing aborted.");
        }

        dataProvider.SetSpriteRects(frames);
        dataProvider.Apply();
        importer.SaveAndReimport();
    }

    private static Rect FindOpaqueBounds(Texture2D texture, int cellX, int cellY, int cellWidth, int cellHeight)
    {
        var minX = cellX + cellWidth;
        var minY = cellY + cellHeight;
        var maxX = -1;
        var maxY = -1;

        for (var y = cellY; y < cellY + cellHeight; y++)
        {
            for (var x = cellX; x < cellX + cellWidth; x++)
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
            throw new InvalidOperationException($"No opaque pixels found in cell beginning at x={cellX}.");

        return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    private static AnimationClip CreateClip(
        IReadOnlyList<Sprite> sprites,
        string clipName,
        string clipPath,
        float frameInterval,
        bool loop,
        AnimationEvent[] events = null)
    {
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null)
            AssetDatabase.DeleteAsset(clipPath);

        var clip = new AnimationClip
        {
            name = clipName,
            frameRate = 60f
        };

        var keyframes = new ObjectReferenceKeyframe[sprites.Count];
        for (var index = 0; index < sprites.Count; index++)
        {
            keyframes[index] = new ObjectReferenceKeyframe
            {
                time = index * frameInterval,
                value = sprites[index]
            };
        }

        var binding = new EditorCurveBinding
        {
            path = string.Empty,
            type = typeof(SpriteRenderer),
            propertyName = "m_Sprite"
        };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        if (events != null)
            AnimationUtility.SetAnimationEvents(clip, events);
        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }

    private static void ValidateActionScale(IReadOnlyList<Sprite> idleSprites, IReadOnlyList<Sprite> attackSprites)
    {
        var idleHeight = idleSprites.Select(sprite => sprite.rect.height).OrderBy(height => height).ElementAt(idleSprites.Count / 2);
        var minimumAllowedHeight = idleHeight - 1f;
        for (var index = 0; index < attackSprites.Count; index++)
        {
            if (attackSprites[index].rect.height < minimumAllowedHeight)
            {
                throw new InvalidOperationException(
                    $"Attack frame {index} is only {attackSprites[index].rect.height}px tall; " +
                    $"Idle reference height is {idleHeight}px. Do not shrink the character to fit a raised weapon or effect.");
            }
        }

        foreach (var index in new[] { 0, attackSprites.Count - 1 })
        {
            if (Math.Abs(attackSprites[index].rect.height - idleHeight) > 1f)
            {
                throw new InvalidOperationException(
                    $"Attack neutral frame {index} is {attackSprites[index].rect.height}px tall; " +
                    $"expected approximately {idleHeight}px to match Idle/Walk.");
            }
        }
    }

    private static AnimatorOverrideController CreateOverrideController(
        AnimationClip idleClip,
        AnimationClip walkClip,
        AnimationClip attackClip,
        AnimationClip dieClip)
    {
        if (AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(OverrideControllerPath) != null)
            AssetDatabase.DeleteAsset(OverrideControllerPath);

        var baseController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(BaseControllerPath);
        if (baseController == null)
            throw new InvalidOperationException($"Base animator controller was not found at {BaseControllerPath}.");

        var controller = new AnimatorOverrideController(baseController) { name = "WraithShaman" };
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        controller.GetOverrides(overrides);
        for (var index = 0; index < overrides.Count; index++)
        {
            if (overrides[index].Key != null && overrides[index].Key.name == "Monster_Idle")
                overrides[index] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[index].Key, idleClip);
            else if (overrides[index].Key != null && overrides[index].Key.name == "Monster_Move")
                overrides[index] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[index].Key, walkClip);
            else if (overrides[index].Key != null && overrides[index].Key.name == "Monster_Attack")
                overrides[index] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[index].Key, attackClip);
            else if (overrides[index].Key != null && overrides[index].Key.name == "Monster_Die")
                overrides[index] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[index].Key, dieClip);
        }

        controller.ApplyOverrides(overrides);
        AssetDatabase.CreateAsset(controller, OverrideControllerPath);
        return controller;
    }

    private static RuntimeAnimatorController CreateProjectileController(AnimationClip flightClip)
    {
        if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ProjectileControllerPath) != null)
            AssetDatabase.DeleteAsset(ProjectileControllerPath);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(ProjectileControllerPath);
        var stateMachine = controller.layers[0].stateMachine;
        var state = stateMachine.AddState("Flight");
        state.motion = flightClip;
        stateMachine.defaultState = state;
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static ArrowProjectile CreateSoulBoltProjectile(
        Sprite initialSprite,
        RuntimeAnimatorController controller)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePath) != null)
            AssetDatabase.DeleteAsset(ProjectilePath);

        var projectileObject = new GameObject("WraithShaman_SoulBolt");
        try
        {
            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = initialSprite;
            renderer.sortingOrder = 5;
            renderer.color = Color.white;

            var animator = projectileObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            var rigidbody = projectileObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            rigidbody.gravityScale = 0f;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = projectileObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.28f;

            projectileObject.AddComponent<Poolable>();
            var projectile = projectileObject.AddComponent<ArrowProjectile>();
            var projectileSettings = new SerializedObject(projectile);
            projectileSettings.FindProperty("lifeTime").floatValue = 3f;
            projectileSettings.ApplyModifiedPropertiesWithoutUndo();

            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(projectileObject, ProjectilePath);
            if (savedPrefab == null)
                throw new InvalidOperationException($"Could not save soul bolt prefab to {ProjectilePath}.");

            return savedPrefab.GetComponent<ArrowProjectile>();
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(projectileObject);
        }
    }

    private static GameObject CreateMonsterPrefab(
        RuntimeAnimatorController controller,
        Sprite initialSprite,
        ArrowProjectile projectilePrefab)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            AssetDatabase.DeleteAsset(PrefabPath);

        var basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BasePrefabPath);
        if (basePrefab == null)
            throw new InvalidOperationException($"Monster_Base prefab was not found at {BasePrefabPath}.");

        var instance = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
        if (instance == null)
            throw new InvalidOperationException("Could not instantiate Monster_Base as a prefab instance.");

        try
        {
            instance.name = "WraithShaman";
            var body = instance.transform.Find("Body");
            if (body == null)
                throw new InvalidOperationException("Monster_Base/Body was not found.");

            var renderer = body.GetComponent<SpriteRenderer>();
            var animator = body.GetComponent<Animator>();
            if (renderer == null || animator == null)
                throw new InvalidOperationException("Monster_Base/Body must keep its SpriteRenderer and Animator components.");

            renderer.sprite = initialSprite;
            animator.runtimeAnimatorController = controller;
            if (body.GetComponent<MonsterAnimationEvent>() == null)
                body.gameObject.AddComponent<MonsterAnimationEvent>();

            var firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(body, false);
            firePoint.localPosition = new Vector3(0.72f, 0.62f, 0f);

            var rangedAttack = instance.GetComponent<RangedMonsterAttack>() ??
                               instance.AddComponent<RangedMonsterAttack>();
            var attackSettings = new SerializedObject(rangedAttack);
            attackSettings.FindProperty("useAnimationEvents").boolValue = true;
            attackSettings.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
            attackSettings.FindProperty("firePoint").objectReferenceValue = firePoint;
            attackSettings.FindProperty("speed").floatValue = 5f;
            attackSettings.FindProperty("targetLayer").intValue = 1 << LayerMask.NameToLayer("Character");
            attackSettings.ApplyModifiedPropertiesWithoutUndo();

            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath, out var success);
            if (!success)
                throw new InvalidOperationException($"Could not save prefab variant to {PrefabPath}.");
            return savedPrefab;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(instance);
        }
    }

    private static void CreateMonsterData(GameObject prefab, Sprite portrait)
    {
        var data = AssetDatabase.LoadAssetAtPath<MonsterDataSO>(MonsterDataPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<MonsterDataSO>();
            AssetDatabase.CreateAsset(data, MonsterDataPath);
        }

        var serializedData = new SerializedObject(data);
        serializedData.FindProperty("monsterId").stringValue = "Wraith_Shaman";
        serializedData.FindProperty("displayName").stringValue = "망령 주술사";
        serializedData.FindProperty("portrait").objectReferenceValue = portrait;
        serializedData.FindProperty("description").stringValue =
            "전열 뒤에서 영혼불을 발사하며 천천히 압박하는 튼튼한 중거리 언데드 주술사";
        serializedData.FindProperty("prefab").objectReferenceValue = prefab;
        serializedData.FindProperty("classification").enumValueIndex = (int)MonsterClassification.Normal;
        serializedData.FindProperty("visualScaleOverride").floatValue = 0f;
        serializedData.FindProperty("bossDefinition").objectReferenceValue = null;

        var stats = serializedData.FindProperty("baseStats");
        stats.FindPropertyRelative("maxHp").floatValue = 90f;
        stats.FindPropertyRelative("moveSpeed").floatValue = 2.5f;
        stats.FindPropertyRelative("atkDamage").floatValue = 12f;
        stats.FindPropertyRelative("atkRange").floatValue = 5.5f;
        stats.FindPropertyRelative("atkPerSec").floatValue = 0.65f;
        serializedData.FindProperty("threatCost").floatValue = 3f;
        serializedData.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(data);
    }

    private static int ParseFrameIndex(string spriteName)
    {
        var separator = spriteName.LastIndexOf('_');
        return separator >= 0 && int.TryParse(spriteName[(separator + 1)..], out var index)
            ? index
            : int.MaxValue;
    }
}
