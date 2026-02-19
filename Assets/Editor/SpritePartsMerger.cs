// Assets/Editor/SpritePartsMerger.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class SpritePartsMerger
{
    [MenuItem("Tools/Sprite Parts Merger/Merge Selected Textures (Trim)")]
    public static void MergeSelectedTexturesTrim()
    {
        var selected = Selection.objects
            .Select(AssetDatabase.GetAssetPath)
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .ToList();

        if (selected.Count == 0)
        {
            Debug.LogWarning("No assets selected.");
            return;
        }

        // 텍스처만 처리
        var texturePaths = selected
            .Where(p => AssetImporter.GetAtPath(p) is TextureImporter)
            .ToList();

        if (texturePaths.Count == 0)
        {
            Debug.LogWarning("No textures selected.");
            return;
        }

        int processed = 0;
        foreach (var path in texturePaths)
        {
            try
            {
                if (TryMergeTexture(path, out string outputPath))
                {
                    processed++;
                    Debug.Log($"Merged: {Path.GetFileName(path)} -> {outputPath}");
                }
                else
                {
                    Debug.LogWarning($"Skipped: {Path.GetFileName(path)} (no sprites found?)");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to merge {path}\n{e}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"Done. Processed: {processed}/{texturePaths.Count}");
    }

    private static bool TryMergeTexture(string texturePath, out string outputPath)
    {
        outputPath = null;

        // 원본 텍스처 로드
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (tex == null) return false;

        // Importer 설정: Read/Write 활성화(픽셀 접근 필요)
        var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
        bool prevReadable = importer.isReadable;
        TextureImporterCompression prevCompression = importer.textureCompression;
        FilterMode prevFilter = tex.filterMode;

        // 스프라이트 파츠 로드 (Multiple로 슬라이스된 sub-sprites)
        var sprites = LoadSubSprites(texturePath);
        if (sprites.Count == 0) return false;

        // 파츠 정렬: 이름 끝의 _숫자 기반( idle_0_0, idle_0_1, idle_0_2 … )
        sprites = sprites
            .OrderBy(s => GetSuffixNumber(s.name))
            .ThenBy(s => s.name)
            .ToList();

        try
        {
            // 가급적 압축 해제 + readable
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            // 원본 픽셀 가져오기
            Color32[] srcPixels = tex.GetPixels32();
            int srcW = tex.width;
            int srcH = tex.height;

            // 합성용 캔버스(원본과 동일 크기에서 먼저 합성 후 Trim)
            var composite = new Texture2D(srcW, srcH, TextureFormat.RGBA32, false);
            var compPixels = new Color32[srcPixels.Length];
            // 초기 투명
            for (int i = 0; i < compPixels.Length; i++)
                compPixels[i] = new Color32(0, 0, 0, 0);

            // 각 파츠 스프라이트를 원본 텍스처 좌표 기준으로 합성
            foreach (var sp in sprites)
            {
                Rect r = sp.rect; // texture space
                int xMin = Mathf.RoundToInt(r.x);
                int yMin = Mathf.RoundToInt(r.y);
                int w = Mathf.RoundToInt(r.width);
                int h = Mathf.RoundToInt(r.height);

                // sprite.rect 영역을 그대로 덮어쓰기(알파 블렌딩)
                // 참고: Unity Texture2D 픽셀 좌표는 좌하단(0,0) 기준
                for (int y = 0; y < h; y++)
                {
                    int srcY = yMin + y;
                    for (int x = 0; x < w; x++)
                    {
                        int srcX = xMin + x;

                        int srcIdx = srcY * srcW + srcX;
                        Color32 s = srcPixels[srcIdx];
                        if (s.a == 0) continue;

                        int dstIdx = srcIdx;
                        Color32 d = compPixels[dstIdx];
                        compPixels[dstIdx] = AlphaOver(d, s);
                    }
                }
            }

            composite.SetPixels32(compPixels);
            composite.Apply();

            // Trim(투명 영역 제거)
            if (!TryFindOpaqueBounds(compPixels, srcW, srcH, out int minX, out int minY, out int maxX, out int maxY))
            {
                // 전부 투명
                return false;
            }

            int outW = (maxX - minX + 1);
            int outH = (maxY - minY + 1);

            var trimmed = new Texture2D(outW, outH, TextureFormat.RGBA32, false);
            var trimmedPixels = new Color32[outW * outH];

            for (int y = 0; y < outH; y++)
            {
                int srcY = minY + y;
                for (int x = 0; x < outW; x++)
                {
                    int srcX = minX + x;
                    trimmedPixels[y * outW + x] = compPixels[srcY * srcW + srcX];
                }
            }

            trimmed.SetPixels32(trimmedPixels);
            trimmed.Apply();

            // 저장 경로: 원본 폴더/Merged/idle_0_merged.png
            string dir = Path.GetDirectoryName(texturePath).Replace("\\", "/");
            string mergedDir = $"{dir}/Merged";
            if (!AssetDatabase.IsValidFolder(mergedDir))
            {
                AssetDatabase.CreateFolder(dir, "Merged");
            }

            string fileNameNoExt = Path.GetFileNameWithoutExtension(texturePath);
            outputPath = $"{mergedDir}/{fileNameNoExt}.png";

            byte[] png = trimmed.EncodeToPNG();
            File.WriteAllBytes(outputPath, png);

            // Output을 Unity가 Import할 수 있도록 Asset 경로로 맞춤
            // File.WriteAllBytes는 프로젝트 상대 경로를 받으므로 outputPath가 Assets/... 형태여야 함
            // 여기서는 texturePath 기반으로 만들었으니 Assets로 시작합니다.
            return true;
        }
        finally
        {
            // importer 설정 복구
            importer.isReadable = prevReadable;
            importer.textureCompression = prevCompression;
            importer.SaveAndReimport();

            // filter mode는 프로젝트 정책에 맞게 나중에 merged 텍스처에서 설정 권장
            tex.filterMode = prevFilter;
        }
    }

    private static List<Sprite> LoadSubSprites(string texturePath)
    {
        // 같은 텍스처에 포함된 서브 스프라이트들을 모두 로드
        // 대표적으로 Sprite Mode=Multiple 이고 Slice 되어있어야 나옵니다.
        return AssetDatabase.LoadAllAssetRepresentationsAtPath(texturePath)
            .OfType<Sprite>()
            .ToList();
    }

    private static int GetSuffixNumber(string name)
    {
        // idle_0_2 -> 2, idle_0_10 -> 10, 못 찾으면 큰 값
        var m = Regex.Match(name, @"_(\d+)$");
        if (!m.Success) return int.MaxValue;
        if (int.TryParse(m.Groups[1].Value, out int n)) return n;
        return int.MaxValue;
    }

    private static Color32 AlphaOver(Color32 dst, Color32 src)
    {
        // src over dst (premultiplied 아님)
        float sa = src.a / 255f;
        float da = dst.a / 255f;

        float outA = sa + da * (1f - sa);
        if (outA <= 0f) return new Color32(0, 0, 0, 0);

        byte r = (byte)Mathf.RoundToInt((src.r * sa + dst.r * da * (1f - sa)) / outA);
        byte g = (byte)Mathf.RoundToInt((src.g * sa + dst.g * da * (1f - sa)) / outA);
        byte b = (byte)Mathf.RoundToInt((src.b * sa + dst.b * da * (1f - sa)) / outA);
        byte a = (byte)Mathf.RoundToInt(outA * 255f);

        return new Color32(r, g, b, a);
    }

    private static bool TryFindOpaqueBounds(Color32[] pixels, int w, int h, out int minX, out int minY, out int maxX, out int maxY)
    {
        minX = w; minY = h;
        maxX = -1; maxY = -1;

        for (int y = 0; y < h; y++)
        {
            int row = y * w;
            for (int x = 0; x < w; x++)
            {
                if (pixels[row + x].a == 0) continue;

                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }
        }

        return maxX >= 0 && maxY >= 0;
    }
}
