#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class VisualPolishInstaller
{
    private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string LevelScenePath = "Assets/Scenes/Level01_BrokenVillage.unity";
    private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player.prefab";
    private const string BackdropPath = "Assets/UI/MainMenuBackdrop.png";
    [MenuItem("Tools/N.P.C./Apply Visual Polish")]
    public static void ApplyVisualPolish()
    {
        EnsureFolder("Assets", "Models");
        VillageMaterials materials = CreateMaterials();
        ApplyBackdropImportSettings();
        PolishPlayerPrefab(materials);
        PolishLevelOne(materials);
        PolishMainMenu();
        AssetDatabase.Refresh();
        Debug.Log("[N.P.C.] Main Menu, Level 1, and Merchant_04 visual polish applied.");
    }

    private static void EnsureFolder(string parent, string folder)
    {
        string path = parent + "/" + folder;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    private static Material CreateMaterial(string name, Color color, float smoothness = 0.15f, float metallic = 0f)
    {
        string path = "Assets/Materials/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                throw new InvalidOperationException("No supported lit shader is available for the visual polish materials.");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", metallic);
        }

        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssetIfDirty(material);
        return material;
    }

    private static VillageMaterials CreateMaterials()
    {
        return new VillageMaterials
        {
            PlasterA = CreateMaterial("MAT_Village_Plaster_A", new Color32(177, 150, 112, 255)),
            PlasterB = CreateMaterial("MAT_Village_Plaster_B", new Color32(151, 129, 100, 255)),
            PlasterC = CreateMaterial("MAT_Village_Plaster_C", new Color32(184, 159, 126, 255)),
            Timber = CreateMaterial("MAT_Village_Timber", new Color32(86, 56, 38, 255)),
            TimberLight = CreateMaterial("MAT_Village_Timber_Light", new Color32(126, 84, 49, 255)),
            Roof = CreateMaterial("MAT_Village_Roof", new Color32(94, 54, 46, 255)),
            RoofLight = CreateMaterial("MAT_Village_Roof_Light", new Color32(113, 67, 51, 255)),
            Stone = CreateMaterial("MAT_Village_Stone", new Color32(105, 103, 91, 255)),
            Cobble = CreateMaterial("MAT_Village_Cobble", new Color32(116, 111, 97, 255)),
            Iron = CreateMaterial("MAT_Village_Iron", new Color32(57, 63, 64, 255), 0.3f, 0.55f),
            Gold = CreateMaterial("MAT_Merchant_Brass", new Color32(174, 126, 55, 255), 0.35f, 0.6f),
            DarkWood = CreateMaterial("MAT_Merchant_DarkLeather", new Color32(68, 45, 33, 255)),
            Leather = CreateMaterial("MAT_Merchant_Leather", new Color32(112, 77, 49, 255)),
            Cloak = CreateMaterial("MAT_Merchant_Cloak", new Color32(135, 62, 46, 255)),
            Tunic = CreateMaterial("MAT_Merchant_Tunic", new Color32(68, 80, 68, 255)),
            Sleeve = CreateMaterial("MAT_Merchant_Sleeves", new Color32(81, 91, 75, 255)),
            Skin = CreateMaterial("MAT_Merchant_Skin", new Color32(177, 133, 95, 255)),
            Boot = CreateMaterial("MAT_Merchant_Boots", new Color32(63, 48, 38, 255)),
            Eye = CreateMaterial("MAT_Merchant_Eyes", new Color32(34, 30, 27, 255)),
            Window = CreateMaterial("MAT_Village_Window", new Color32(48, 61, 64, 255), 0.25f),
            ClothRed = CreateMaterial("MAT_Village_Canopy_Red", new Color32(139, 66, 49, 255)),
            ClothCream = CreateMaterial("MAT_Village_Canopy_Cream", new Color32(190, 160, 111, 255)),
            FruitRed = CreateMaterial("MAT_Market_Apple_Red", new Color32(167, 66, 43, 255)),
            FruitGreen = CreateMaterial("MAT_Market_Apple_Green", new Color32(108, 132, 62, 255)),
            FruitGold = CreateMaterial("MAT_Market_Pear_Gold", new Color32(197, 151, 62, 255)),
            Moss = CreateMaterial("MAT_Village_Moss", new Color32(70, 91, 55, 255))
        };
    }

    private static void ApplyBackdropImportSettings()
    {
        TextureImporter importer = AssetImporter.GetAtPath(BackdropPath) as TextureImporter;
        if (importer == null)
        {
            throw new InvalidOperationException("The generated menu backdrop texture was not imported: " + BackdropPath);
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 2048;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.SaveAndReimport();
    }

    private static void PolishPlayerPrefab(VillageMaterials materials)
    {
        GameObject player = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        try
        {
            foreach (Renderer renderer in player.GetComponentsInChildren<Renderer>(true))
            {
                string name = renderer.gameObject.name;
                if (name == "Tunic_Body" || name == "Torso")
                {
                    renderer.sharedMaterial = materials.Tunic;
                }
                else if (name.StartsWith("Sleeve", StringComparison.Ordinal) ||
                         name.StartsWith("Arm_Mesh", StringComparison.Ordinal))
                {
                    renderer.sharedMaterial = materials.Sleeve;
                }
                else if (name.StartsWith("Thigh", StringComparison.Ordinal) ||
                         name.StartsWith("Leg_", StringComparison.Ordinal))
                {
                    renderer.sharedMaterial = materials.Tunic;
                }
                else if (name.StartsWith("Boot", StringComparison.Ordinal) ||
                         name == "Belt")
                {
                    renderer.sharedMaterial = materials.Boot;
                }
                else if (name == "Buckle")
                {
                    renderer.sharedMaterial = materials.Gold;
                }
                else if (name == "Head_Mesh" || name == "Hand_L" || name == "Hand_R")
                {
                    renderer.sharedMaterial = materials.Skin;
                }
                else if (name == "Eye_L" || name == "Eye_R")
                {
                    renderer.sharedMaterial = materials.Eye;
                }
                else if (name == "Merchant_Cap")
                {
                    renderer.sharedMaterial = materials.DarkWood;
                }
                else if (name == "Backpack" || name == "Satchel")
                {
                    renderer.sharedMaterial = materials.Leather;
                }
            }

            Transform hips = FindDeep(player.transform, "Hips");
            Transform torso = FindDeep(player.transform, "Torso");
            if (hips == null || torso == null)
            {
                throw new InvalidOperationException("Player prefab is missing the expected Hips or Torso transform.");
            }

            Mesh capeMesh = LoadOrCreateCapeMesh();
            GameObject cape = GetOrCreateChild(hips, "ArtPass_Cape");
            cape.transform.localPosition = Vector3.zero;
            cape.transform.localRotation = Quaternion.identity;
            cape.transform.localScale = Vector3.one;
            MeshFilter filter = cape.GetComponent<MeshFilter>();
            if (filter == null)
            {
                filter = cape.AddComponent<MeshFilter>();
            }

            filter.sharedMesh = capeMesh;
            MeshRenderer capeRenderer = cape.GetComponent<MeshRenderer>();
            if (capeRenderer == null)
            {
                capeRenderer = cape.AddComponent<MeshRenderer>();
            }

            capeRenderer.sharedMaterial = materials.Cloak;

            AddPart(torso, "ArtPass_CloakClasp", PrimitiveType.Sphere,
                new Vector3(0f, 0.18f, 0.2f), new Vector3(0.1f, 0.1f, 0.045f), materials.Gold,
                Quaternion.identity);

            PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(player);
        }
    }

    private static Mesh LoadOrCreateCapeMesh()
    {
        const string path = "Assets/Models/MerchantCape.asset";
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh != null)
        {
            return mesh;
        }

        Vector3[] front =
        {
            new Vector3(-0.27f, 0.66f, -0.22f),
            new Vector3(0.27f, 0.66f, -0.22f),
            new Vector3(-0.34f, 0.12f, -0.29f),
            new Vector3(0.34f, 0.12f, -0.29f),
            new Vector3(-0.42f, -0.55f, -0.33f),
            new Vector3(0f, -0.65f, -0.38f),
            new Vector3(0.42f, -0.55f, -0.33f)
        };
        Vector3[] vertices = new Vector3[front.Length * 2];
        Array.Copy(front, vertices, front.Length);
        for (int i = 0; i < front.Length; i++)
        {
            vertices[i + front.Length] = front[i] + Vector3.forward * 0.025f;
        }

        int[] frontTriangles =
        {
            0, 2, 5, 0, 5, 4,
            0, 1, 3, 0, 3, 2,
            1, 6, 5, 1, 5, 3,
            2, 3, 5, 2, 5, 4,
            3, 6, 5
        };
        int[] triangles = new int[frontTriangles.Length * 2];
        for (int i = 0; i < frontTriangles.Length; i += 3)
        {
            triangles[i] = frontTriangles[i];
            triangles[i + 1] = frontTriangles[i + 1];
            triangles[i + 2] = frontTriangles[i + 2];
            triangles[frontTriangles.Length + i] = frontTriangles[i] + front.Length;
            triangles[frontTriangles.Length + i + 1] = frontTriangles[i + 2] + front.Length;
            triangles[frontTriangles.Length + i + 2] = frontTriangles[i + 1] + front.Length;
        }

        mesh = new Mesh { name = "MerchantCape_LowPoly" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    private static void PolishLevelOne(VillageMaterials materials)
    {
        bool opened;
        Scene scene = GetLoadedScene(LevelScenePath, out opened);
        try
        {
            Transform environment = FindDeepInScene(scene, "[ ENVIRONMENT ]");
            if (environment == null)
            {
                throw new InvalidOperationException("The Level 1 environment root could not be found.");
            }

            string[] houses = { "House_West_01", "House_West_02", "House_East_01", "House_East_02" };
            float[] facadeZ = { -2.17f, -2.67f, -2.17f, -2.42f };
            Material[] plaster = { materials.PlasterA, materials.PlasterB, materials.PlasterC, materials.PlasterA };
            Material[] roofs = { materials.Roof, materials.RoofLight, materials.RoofLight, materials.Roof };

            for (int i = 0; i < houses.Length; i++)
            {
                Transform house = FindDeep(environment, houses[i]);
                if (house == null)
                {
                    throw new InvalidOperationException("Expected Level 1 house not found: " + houses[i]);
                }

                ApplyHouseMaterials(house, plaster[i], roofs[i], materials);
                AddHouseFacade(house, facadeZ[i], i, materials);
            }

            PolishMarketStall(FindDeep(environment, "MarketStall_Produce"), true, materials);
            PolishMarketStall(FindDeep(environment, "MarketStall_General"), false, materials);
            PolishStreet(environment, materials);
            PolishVillageStructures(environment, materials);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException("Could not save Level 1 after the visual polish pass.");
            }
        }
        finally
        {
            if (opened)
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }

    private static void ApplyHouseMaterials(Transform house, Material plaster, Material roof, VillageMaterials materials)
    {
        foreach (Renderer renderer in house.GetComponentsInChildren<Renderer>(true))
        {
            string name = renderer.gameObject.name;
            if (name == "Body")
            {
                renderer.sharedMaterial = plaster;
            }
            else if (name.StartsWith("Beam", StringComparison.Ordinal))
            {
                renderer.sharedMaterial = materials.Timber;
            }
            else if (name == "Roof")
            {
                renderer.sharedMaterial = roof;
            }
        }
    }

    private static void AddHouseFacade(Transform house, float frontZ, int variant, VillageMaterials materials)
    {
        Transform details = GetOrCreateChild(house, "ArtPass_FacadeDetails").transform;
        float windowY = variant == 1 ? 3.55f : variant == 3 ? 3.65f : 3.12f;
        Material door = variant % 2 == 0 ? materials.DarkWood : materials.Timber;

        AddPart(details, "Door_Leaf", PrimitiveType.Cube, new Vector3(0f, 0.97f, frontZ),
            new Vector3(0.98f, 1.86f, 0.09f), door, Quaternion.identity);
        AddPart(details, "Door_Frame_Left", PrimitiveType.Cube, new Vector3(-0.56f, 1.02f, frontZ - 0.045f),
            new Vector3(0.13f, 2.08f, 0.14f), materials.TimberLight, Quaternion.identity);
        AddPart(details, "Door_Frame_Right", PrimitiveType.Cube, new Vector3(0.56f, 1.02f, frontZ - 0.045f),
            new Vector3(0.13f, 2.08f, 0.14f), materials.TimberLight, Quaternion.identity);
        AddPart(details, "Door_Lintel", PrimitiveType.Cube, new Vector3(0f, 2.07f, frontZ - 0.045f),
            new Vector3(1.25f, 0.16f, 0.14f), materials.TimberLight, Quaternion.identity);
        AddPart(details, "Door_Panel_Lower", PrimitiveType.Cube, new Vector3(0f, 0.55f, frontZ - 0.055f),
            new Vector3(0.78f, 0.045f, 0.035f), materials.DarkWood, Quaternion.identity);
        AddPart(details, "Door_Panel_Upper", PrimitiveType.Cube, new Vector3(0f, 1.35f, frontZ - 0.055f),
            new Vector3(0.78f, 0.045f, 0.035f), materials.DarkWood, Quaternion.identity);
        AddPart(details, "Door_Handle", PrimitiveType.Sphere, new Vector3(0.34f, 1f, frontZ - 0.12f),
            new Vector3(0.09f, 0.09f, 0.06f), materials.Gold, Quaternion.identity);

        AddWindow(details, "Window_Left", new Vector3(-1.35f, windowY, frontZ - 0.03f), materials);
        AddWindow(details, "Window_Right", new Vector3(1.35f, windowY, frontZ - 0.03f), materials);
        AddPart(details, "Facade_Eave_Trim", PrimitiveType.Cube,
            new Vector3(0f, windowY + 0.88f, frontZ + 0.015f), new Vector3(4.15f, 0.13f, 0.14f),
            materials.Timber, Quaternion.identity);
    }

    private static void AddWindow(Transform parent, string prefix, Vector3 center, VillageMaterials materials)
    {
        AddPart(parent, prefix + "_Glass", PrimitiveType.Cube, center, new Vector3(0.68f, 0.78f, 0.06f),
            materials.Window, Quaternion.identity);
        AddPart(parent, prefix + "_Frame_Left", PrimitiveType.Cube, center + new Vector3(-0.39f, 0f, -0.04f),
            new Vector3(0.1f, 0.94f, 0.12f), materials.TimberLight, Quaternion.identity);
        AddPart(parent, prefix + "_Frame_Right", PrimitiveType.Cube, center + new Vector3(0.39f, 0f, -0.04f),
            new Vector3(0.1f, 0.94f, 0.12f), materials.TimberLight, Quaternion.identity);
        AddPart(parent, prefix + "_Frame_Top", PrimitiveType.Cube, center + new Vector3(0f, 0.44f, -0.04f),
            new Vector3(0.88f, 0.1f, 0.12f), materials.TimberLight, Quaternion.identity);
        AddPart(parent, prefix + "_Frame_Bottom", PrimitiveType.Cube, center + new Vector3(0f, -0.44f, -0.04f),
            new Vector3(0.88f, 0.1f, 0.12f), materials.TimberLight, Quaternion.identity);
        AddPart(parent, prefix + "_Mullion_Vertical", PrimitiveType.Cube, center + new Vector3(0f, 0f, -0.06f),
            new Vector3(0.045f, 0.78f, 0.04f), materials.Timber, Quaternion.identity);
        AddPart(parent, prefix + "_Mullion_Horizontal", PrimitiveType.Cube, center + new Vector3(0f, 0f, -0.06f),
            new Vector3(0.68f, 0.045f, 0.04f), materials.Timber, Quaternion.identity);
    }

    private static void PolishMarketStall(Transform stall, bool produce, VillageMaterials materials)
    {
        if (stall == null)
        {
            throw new InvalidOperationException("An expected Level 1 market stall could not be found.");
        }

        foreach (Renderer renderer in stall.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.gameObject.name == "Bench")
            {
                renderer.sharedMaterial = materials.Timber;
            }
            else if (renderer.gameObject.name == "Canopy")
            {
                renderer.sharedMaterial = produce ? materials.ClothRed : materials.ClothCream;
            }
        }

        Transform details = GetOrCreateChild(stall, "ArtPass_MarketDetails").transform;
        for (int i = 0; i < 4; i++)
        {
            float x = -0.84f + i * 0.56f;
            Material stripe = i % 2 == 0 ? (produce ? materials.ClothCream : materials.ClothRed)
                                         : (produce ? materials.ClothRed : materials.ClothCream);
            AddPart(details, "Canopy_Stripe_" + i, PrimitiveType.Cube, new Vector3(x, 2.39f, -0.73f),
                new Vector3(0.42f, 0.19f, 0.07f), stripe, Quaternion.identity);
        }

        if (produce)
        {
            Material[] fruit = { materials.FruitRed, materials.FruitGreen, materials.FruitGold };
            for (int i = 0; i < 9; i++)
            {
                float x = -0.68f + (i % 3) * 0.48f;
                float z = -0.2f + (i / 3) * 0.31f;
                AddPart(details, "Market_Produce_" + i, PrimitiveType.Sphere,
                    new Vector3(x, 1.3f + (i % 2) * 0.045f, z), new Vector3(0.22f, 0.2f, 0.22f),
                    fruit[i % fruit.Length], Quaternion.identity);
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                AddPart(details, "Market_Jar_" + i, PrimitiveType.Cylinder,
                    new Vector3(-0.65f + i * 0.62f, 1.28f, 0.22f), new Vector3(0.24f, 0.25f, 0.24f),
                    i == 1 ? materials.Leather : materials.TimberLight, Quaternion.identity);
            }
        }
    }

    private static void PolishStreet(Transform environment, VillageMaterials materials)
    {
        Transform details = GetOrCreateChild(environment, "ArtPass_StreetDetails").transform;
        for (int zIndex = 0; zIndex < 6; zIndex++)
        {
            float z = 4f + zIndex * 3.1f;
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 3.45f;
                float angle = (zIndex % 2 == 0 ? -12f : 8f) * side;
                AddPart(details, "Edge_Cobble_" + (side < 0 ? "L_" : "R_") + zIndex,
                    PrimitiveType.Cube, new Vector3(x, 0.035f, z), new Vector3(0.72f, 0.07f, 0.44f),
                    zIndex % 3 == 0 ? materials.Stone : materials.Cobble, Quaternion.Euler(0f, angle, 0f));
            }
        }

        AddBarrel(details, "Square_Barrel_West", new Vector3(-8.1f, 0f, 10.8f), materials);
        AddBarrel(details, "Square_Barrel_East", new Vector3(8.1f, 0f, 10.8f), materials);
        AddBarrel(details, "Square_Barrel_North", new Vector3(8.15f, 0f, 19.5f), materials);
        AddPlant(details, "Wall_Plant_West", new Vector3(-8.75f, 0f, 6.5f), materials);
        AddPlant(details, "Wall_Plant_East", new Vector3(8.75f, 0f, 6.5f), materials);
        AddPlant(details, "Wall_Plant_West_North", new Vector3(-8.75f, 0f, 20f), materials);
        AddPlant(details, "Wall_Plant_East_North", new Vector3(8.75f, 0f, 20f), materials);
    }

    private static void AddBarrel(Transform parent, string name, Vector3 position, VillageMaterials materials)
    {
        Transform barrel = GetOrCreateChild(parent, name).transform;
        AddPart(barrel, "Body", PrimitiveType.Cylinder, new Vector3(0f, 0.56f, 0f),
            new Vector3(0.56f, 0.58f, 0.56f), materials.Timber, Quaternion.identity);
        AddPart(barrel, "Iron_Hoop_Lower", PrimitiveType.Cylinder, new Vector3(0f, 0.24f, 0f),
            new Vector3(0.59f, 0.055f, 0.59f), materials.Iron, Quaternion.identity);
        AddPart(barrel, "Iron_Hoop_Upper", PrimitiveType.Cylinder, new Vector3(0f, 0.87f, 0f),
            new Vector3(0.59f, 0.055f, 0.59f), materials.Iron, Quaternion.identity);
        barrel.localPosition = position;
    }

    private static void AddPlant(Transform parent, string name, Vector3 position, VillageMaterials materials)
    {
        Transform plant = GetOrCreateChild(parent, name).transform;
        plant.localPosition = position;
        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * 0.11f;
            AddPart(plant, "Leaf_" + i, PrimitiveType.Capsule,
                new Vector3(x, 0.17f + (i % 2) * 0.05f, (i - 1) * 0.06f),
                new Vector3(0.08f, 0.22f, 0.08f), materials.Moss,
                Quaternion.Euler(0f, i * 55f, (i - 1) * 14f));
        }
        AddPart(plant, "Stone", PrimitiveType.Sphere, new Vector3(0f, 0.04f, 0f),
            new Vector3(0.28f, 0.1f, 0.2f), materials.Stone, Quaternion.identity);
    }

    private static void PolishVillageStructures(Transform environment, VillageMaterials materials)
    {
        string[] stoneNames =
        {
            "Perimeter_Wall_West", "Perimeter_Wall_East", "Perimeter_Wall_South",
            "NorthWall_Left", "NorthWall_Right", "Alley_Wall_West", "Alley_Wall_East",
            "TownGate_Tower_Left", "TownGate_Tower_Right", "TownGate_Arch_Top"
        };
        foreach (string name in stoneNames)
        {
            Transform item = FindDeep(environment, name);
            if (item != null)
            {
                Renderer renderer = item.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = materials.Stone;
                }
            }
        }

        Transform portcullis = FindDeep(environment, "TownGate_Portcullis");
        if (portcullis != null && portcullis.GetComponent<Renderer>() != null)
        {
            portcullis.GetComponent<Renderer>().sharedMaterial = materials.Iron;
        }

        string[] floors = { "VillageSquare_Ground", "Alley_Floor" };
        foreach (string name in floors)
        {
            Transform floor = FindDeep(environment, name);
            if (floor != null && floor.GetComponent<Renderer>() != null)
            {
                floor.GetComponent<Renderer>().sharedMaterial = materials.Cobble;
            }
        }

        foreach (Transform lantern in FindAllByName(environment, "StreetLantern_Post"))
        {
            foreach (Renderer renderer in lantern.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.gameObject.name == "Pole")
                {
                    renderer.sharedMaterial = materials.Timber;
                }
                else if (renderer.gameObject.name == "Bracket" || renderer.gameObject.name == "Lantern_Housing")
                {
                    renderer.sharedMaterial = materials.Iron;
                }
            }
        }
    }

    private static void PolishMainMenu()
    {
        bool opened;
        Scene scene = GetLoadedScene(MenuScenePath, out opened);
        try
        {
            if (scene.isDirty)
            {
                throw new InvalidOperationException("MainMenu has unsaved editor changes. Save it before applying the visual polish.");
            }

            GameObject canvas = FindInScene(scene, "MainMenu_Canvas");
            if (canvas == null)
            {
                throw new InvalidOperationException("The MainMenu canvas could not be found.");
            }

            Sprite backdrop = AssetDatabase.LoadAssetAtPath<Sprite>(BackdropPath);
            if (backdrop == null)
            {
                throw new InvalidOperationException("The menu backdrop did not import as a Sprite.");
            }

            Transform backdropTransform = canvas.transform.Find("ArtPass_MenuBackdrop");
            GameObject background = backdropTransform != null
                ? backdropTransform.gameObject
                : new GameObject("ArtPass_MenuBackdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            if (backdropTransform == null)
            {
                background.transform.SetParent(canvas.transform, false);
            }

            RectTransform rect = background.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            Image image = background.GetComponent<Image>();
            image.sprite = backdrop;
            image.color = Color.white;
            image.preserveAspect = false;
            image.raycastTarget = false;
            background.transform.SetAsFirstSibling();

            SetPanelOpacity(scene, "SplashPanel", 0.68f);
            SetPanelOpacity(scene, "MenuPanel", 0.68f);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException("Could not save MainMenu after adding its visual backdrop.");
            }
        }
        finally
        {
            if (opened)
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }

    private static void SetPanelOpacity(Scene scene, string name, float alpha)
    {
        GameObject panel = FindInScene(scene, name);
        if (panel == null)
        {
            throw new InvalidOperationException("Expected menu panel not found: " + name);
        }

        Image image = panel.GetComponent<Image>();
        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    private static Scene GetLoadedScene(string path, out bool opened)
    {
        Scene scene = SceneManager.GetSceneByPath(path);
        opened = !scene.IsValid() || !scene.isLoaded;
        if (opened)
        {
            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        }

        return scene;
    }

    private static void AddPart(Transform parent, string name, PrimitiveType type, Vector3 position,
        Vector3 scale, Material material, Quaternion rotation)
    {
        Transform existing = parent.Find(name);
        GameObject part = existing != null ? existing.gameObject : GameObject.CreatePrimitive(type);
        part.name = name;
        if (existing == null)
        {
            part.transform.SetParent(parent, false);
        }

        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            UnityEngine.Object.DestroyImmediate(collider);
        }

        part.transform.localPosition = position;
        part.transform.localRotation = rotation;
        part.transform.localScale = scale;
        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }
    }

    private static GameObject GetOrCreateChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            return child.gameObject;
        }

        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static Transform FindDeepInScene(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform found = FindDeep(root.transform, name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static GameObject FindInScene(Scene scene, string name)
    {
        Transform found = FindDeepInScene(scene, name);
        return found != null ? found.gameObject : null;
    }

    private static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDeep(root.GetChild(i), name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static System.Collections.Generic.List<Transform> FindAllByName(Transform root, string name)
    {
        var matches = new System.Collections.Generic.List<Transform>();
        CollectByName(root, name, matches);
        return matches;
    }

    private static void CollectByName(Transform root, string name, System.Collections.Generic.List<Transform> matches)
    {
        if (root.name == name)
        {
            matches.Add(root);
        }

        for (int i = 0; i < root.childCount; i++)
        {
            CollectByName(root.GetChild(i), name, matches);
        }
    }

    private sealed class VillageMaterials
    {
        public Material PlasterA;
        public Material PlasterB;
        public Material PlasterC;
        public Material Timber;
        public Material TimberLight;
        public Material Roof;
        public Material RoofLight;
        public Material Stone;
        public Material Cobble;
        public Material Iron;
        public Material Gold;
        public Material DarkWood;
        public Material Leather;
        public Material Cloak;
        public Material Tunic;
        public Material Sleeve;
        public Material Skin;
        public Material Boot;
        public Material Eye;
        public Material Window;
        public Material ClothRed;
        public Material ClothCream;
        public Material FruitRed;
        public Material FruitGreen;
        public Material FruitGold;
        public Material Moss;
    }
}
#endif
