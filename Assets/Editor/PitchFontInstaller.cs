#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;

public static class PitchFontInstaller
{
    private const string RunOnceKey = "NPC.ApplyPitchTypographyOnce";
    private const string SourceFontPath = "Assets/UI/Fonts/Orbitron[wght].ttf";
    private const string FontAssetPath = "Assets/UI/Fonts/Orbitron SDF.asset";
    private const string FallbackFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";

    [InitializeOnLoadMethod]
    private static void ScheduleOneTimeApply()
    {
        if (SessionState.GetBool(RunOnceKey, false))
        {
            return;
        }

        SessionState.SetBool(RunOnceKey, true);
        EditorApplication.delayCall += ApplyPitchTypography;
    }

    [MenuItem("Tools/N.P.C./Apply Pitch Typography")]
    public static void ApplyPitchTypography()
    {
        AssetDatabase.Refresh();

        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
        if (sourceFont == null)
        {
            throw new InvalidOperationException("The Orbitron source font could not be imported.");
        }

        FontEngine.InitializeFontEngine();
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024,
            AtlasPopulationMode.Dynamic, true);
        if (fontAsset == null)
        {
            throw new InvalidOperationException("TextMesh Pro could not create the Orbitron font asset.");
        }

        fontAsset.name = "Orbitron SDF";
        Texture2D atlas = fontAsset.atlasTextures[0];
        Material fontMaterial = fontAsset.material;
        atlas.name = "Orbitron SDF Atlas";
        fontMaterial.name = "Orbitron SDF Material";

        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
        AssetDatabase.AddObjectToAsset(atlas, fontAsset);
        AssetDatabase.AddObjectToAsset(fontMaterial, fontAsset);

        TMP_FontAsset fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FallbackFontPath);
        if (fallback == null)
        {
            throw new InvalidOperationException("The existing Liberation Sans fallback font is missing.");
        }

        fontAsset.fallbackFontAssetTable = new List<TMP_FontAsset> { fallback };
        string asciiCharacters = new string(Enumerable.Range(32, 95).Select(value => (char)value).ToArray());
        if (!fontAsset.TryAddCharacters(asciiCharacters, out string missingCharacters))
        {
            Debug.LogWarning("Orbitron is missing glyphs that will use the fallback font: " + missingCharacters);
        }

        fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;
        EditorUtility.SetDirty(fontAsset);
        EditorUtility.SetDirty(atlas);
        EditorUtility.SetDirty(fontMaterial);
        AssetDatabase.SaveAssets();

        Scene scene = EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
        var targetNames = new HashSet<string>
        {
            "GameTitle",
            "SplashSubtitle",
            "Tagline",
            "PressAnyKeyPrompt",
            "VersionNotice"
        };
        int updated = 0;
        foreach (TextMeshProUGUI text in scene.GetRootGameObjects()
                     .SelectMany(root => root.GetComponentsInChildren<TextMeshProUGUI>(true)))
        {
            if (!targetNames.Contains(text.gameObject.name))
            {
                continue;
            }

            text.font = fontAsset;
            text.fontSharedMaterial = fontMaterial;
            text.ForceMeshUpdate();
            EditorUtility.SetDirty(text);
            updated++;
        }

        if (updated != targetNames.Count)
        {
            throw new InvalidOperationException($"Expected to update {targetNames.Count} splash texts, updated {updated}.");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log($"Applied Orbitron typography to {updated} Main Menu splash texts.");
    }
}
#endif
