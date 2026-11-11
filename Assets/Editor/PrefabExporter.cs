#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Converts scene GameObjects into standard Unity Prefab assets in Assets/Prefabs/.
/// Connects scene objects as true Prefab Instances (blue hierarchy icons)
/// to follow standard professional & academic Unity workflow.
/// </summary>
public static class PrefabExporter
{
    [MenuItem("N.P.C./Save Scene Elements as Standard Prefabs")]
    public static void ExportSceneElementsToPrefabs()
    {
        EnsureFolders();

        int exportedCount = 0;

        // 1. Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player != null)
        {
            SaveAndConnectPrefab(player, "Assets/Prefabs/Player/Player.prefab");
            exportedCount++;
        }

        // 2. HUD Canvas
        GameObject hud = GameObject.Find("HUD_Canvas");
        if (hud != null)
        {
            SaveAndConnectPrefab(hud, "Assets/Prefabs/UI/HUD_Canvas.prefab");
            exportedCount++;
        }

        // 3. Garbage Collector
        GameObject gc = GameObject.Find("GarbageCollector_Daemon");
        if (gc != null)
        {
            SaveAndConnectPrefab(gc, "Assets/Prefabs/AI/GarbageCollector_Daemon.prefab");
            exportedCount++;
        }

        // 4. Barkeep NPC
        GameObject barkeep = GameObject.Find("Barkeep_01.npc");
        if (barkeep != null)
        {
            SaveAndConnectPrefab(barkeep, "Assets/Prefabs/AI/Barkeep_NPC.prefab");
            exportedCount++;
        }

        // 5. Tavern
        GameObject tavern = GameObject.Find("NullPointer_Tavern");
        if (tavern != null)
        {
            SaveAndConnectPrefab(tavern, "Assets/Prefabs/Environment/NullPointer_Tavern.prefab");
            exportedCount++;
        }

        // 6. Village Well
        GameObject well = GameObject.Find("Village_Well");
        if (well != null)
        {
            SaveAndConnectPrefab(well, "Assets/Prefabs/Environment/Village_Well.prefab");
            exportedCount++;
        }

        // 7. Street Lantern
        GameObject lantern = GameObject.Find("StreetLantern_Post");
        if (lantern != null)
        {
            SaveAndConnectPrefab(lantern, "Assets/Prefabs/Environment/Street_Lantern.prefab");
            exportedCount++;
        }

        // 8. Fence Barricade
        GameObject fence = GameObject.Find("Fence_Blockade_Requires_Crates");
        if (fence != null)
        {
            SaveAndConnectPrefab(fence, "Assets/Prefabs/Environment/Fence_Barricade.prefab");
            exportedCount++;
        }

        // 9. Glitch Seam Wall
        GameObject gwall = GameObject.Find("GlitchWall_Seam_Exit");
        if (gwall != null)
        {
            SaveAndConnectPrefab(gwall, "Assets/Prefabs/Environment/GlitchWall_Seam.prefab");
            exportedCount++;
        }

        // 10. Crash Log Collectible
        GameObject clog = GameObject.Find("CrashLog_01_Collectible");
        if (clog != null)
        {
            SaveAndConnectPrefab(clog, "Assets/Prefabs/Mechanics/CrashLog_Collectible.prefab");
            exportedCount++;
        }

        // 11. Void Killplane
        GameObject killplane = GameObject.Find("VoidKillplane");
        if (killplane != null)
        {
            SaveAndConnectPrefab(killplane, "Assets/Prefabs/Systems/VoidKillplane.prefab");
            exportedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=green><b>[N.P.C.] Success! {exportedCount} standard Prefabs created in Assets/Prefabs/ and connected to scene instances.</b></color>");
    }

    private static void SaveAndConnectPrefab(GameObject target, string path)
    {
        try
        {
            PrefabUtility.SaveAsPrefabAssetAndConnect(target, path, InteractionMode.UserAction);
            Debug.Log($"[N.P.C.] Prefab saved: {path}");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[N.P.C.] Could not save prefab {path}: {ex.Message}");
        }
    }

    [MenuItem("N.P.C./Organize Hierarchy (Academic Clean Layout)")]
    public static void OrganizeSceneHierarchy()
    {
        // 1. Management
        GameObject mgmt = GetOrCreateContainer("[ MANAGEMENT ]");
        MoveToContainer("GameManager", mgmt);

        // 2. Lighting & Atmosphere
        GameObject lighting = GetOrCreateContainer("[ LIGHTING & ATMOSPHERE ]");
        MoveToContainer("Directional Light", lighting);
        MoveToContainer("GlobalVolume_VillageFantasy", lighting);
        MoveToContainer("LocalVolume_BackstageGlitch", lighting);

        // 3. Environment
        GameObject env = GetOrCreateContainer("[ ENVIRONMENT ]");
        MoveToContainer("--- LEVEL 01: THE BROKEN VILLAGE ---", env);

        // 4. Entities
        GameObject entities = GetOrCreateContainer("[ ENTITIES ]");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player != null && entities != null)
        {
            Undo.SetTransformParent(player.transform, entities.transform, "Organize Player");
        }

        MoveToContainer("PlayerCameraRig", entities);

        // 5. Gameplay & Systems
        GameObject gameplay = GetOrCreateContainer("[ GAMEPLAY & SYSTEMS ]");
        MoveToContainer("VoidKillplane", gameplay);
        MoveToContainer("Waypoints_GarbageCollector_Patrol", gameplay);

        // 6. UI
        GameObject ui = GetOrCreateContainer("[ USER INTERFACE ]");
        MoveToContainer("HUD_Canvas", ui);
        MoveToContainer("EventSystem", ui);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );

        Debug.Log("<color=green><b>[N.P.C.] Scene Hierarchy perfectly organized into clean academic containers!</b></color>");
    }

    private static GameObject GetOrCreateContainer(string name)
    {
        GameObject c = GameObject.Find(name);
        if (c == null)
        {
            c = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(c, "Create " + name);
        }
        return c;
    }

    private static void MoveToContainer(string objectName, GameObject container)
    {
        GameObject go = GameObject.Find(objectName);
        if (go != null && container != null && go != container)
        {
            Undo.SetTransformParent(go.transform, container.transform, "Organize " + objectName);
        }
    }

    private static void EnsureFolders()
    {
        string[] folders = new string[]
        {
            "Assets/Prefabs/Player",
            "Assets/Prefabs/AI",
            "Assets/Prefabs/UI",
            "Assets/Prefabs/Environment",
            "Assets/Prefabs/Mechanics",
            "Assets/Prefabs/Systems"
        };

        foreach (string folder in folders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parent = Path.GetDirectoryName(folder).Replace("\\", "/");
                string name = Path.GetFileName(folder);
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }
}
#endif
