using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// 一键搭建第三人称射击场景：深色瓷砖回廊 + 大厅 + 储物柜 + 长条顶灯。
/// 菜单：GameSS → 搭建射击场景
/// </summary>
public static class ShooterSceneBuilder
{
    const string ScenePath = "Assets/Scenes/ShooterArena.unity";
    const string MatDir = "Assets/Materials/Arena";

    static readonly Color TileCol   = new Color(0.23f, 0.24f, 0.26f); // 瓷砖底色（深灰）
    static readonly Color TileLine  = new Color(0.13f, 0.14f, 0.15f); // 瓷砖缝
    static readonly Color MetalCol  = new Color(0.18f, 0.19f, 0.21f); // 柜体金属
    static readonly Color LampCol   = new Color(0.95f, 0.97f, 1.00f); // 灯管冷白

    [MenuItem("GameSS/搭建射击场景")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var old = GameObject.Find("ARENA");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("ARENA");

        EnsureFolder();

        // —— 程序化贴图 ——
        var tileTex = MakeTileTexture("Corridor_Tile", TileCol, TileLine);
        var lockerTex = MakeLockerTexture("Corridor_Locker");

        // —— 大厅 30m × 12m × 高4m ——
        Tile(root, "Floor",   P(0, -0.15f, 0),  P(30, 0.3f, 12), tileTex);
        Tile(root, "Ceil",    P(0, 4.15f, 0),   P(30, 0.3f, 12), tileTex);
        Tile(root, "Wall_N",  P(0, 2, -6),      P(30, 4, 0.3f),  tileTex);
        Tile(root, "Wall_S",  P(0, 2, 6),       P(30, 4, 0.3f),  tileTex);
        Tile(root, "Wall_W",  P(-15, 2, 0),     P(0.3f, 4, 12),  tileTex);
        // 东墙开 4m 门洞（分两段 + 门楣）
        Tile(root, "Wall_E1", P(15, 2, -4),     P(0.3f, 4, 4),   tileTex);
        Tile(root, "Wall_E2", P(15, 2, 4),      P(0.3f, 4, 4),   tileTex);
        Tile(root, "Wall_E3", P(15, 3.5f, 0),   P(0.3f, 1, 4),   tileTex);

        // —— 走廊（东墙门洞延伸 12m，宽4m）——
        Tile(root, "CorFloor", P(21, -0.15f, 0), P(12, 0.3f, 4), tileTex);
        Tile(root, "CorCeil",  P(21, 4.15f, 0),  P(12, 0.3f, 4), tileTex);
        Tile(root, "CorWallN", P(21, 2, -2),     P(12, 4, 0.3f), tileTex);
        Tile(root, "CorWallS", P(21, 2, 2),      P(12, 4, 0.3f), tileTex);
        Tile(root, "CorEnd",   P(27, 2, 0),      P(0.3f, 4, 4.6f), tileTex);

        // —— 储物柜两排（北墙）——
        Locker(root, "Lockers1", P(-6, 1, -5.55f), P(4.8f, 2, 0.5f), lockerTex);
        Locker(root, "Lockers2", P(-0.8f, 1, -5.55f), P(4.8f, 2, 0.5f), lockerTex);

        // —— 掩体 ——
        Tile(root, "Cover1", P(-4, 0.55f, 0),  P(4, 1.1f, 0.4f), tileTex, new Vector3(0, 15, 0));
        Tile(root, "Cover2", P(5, 0.55f, 2),   P(4, 1.1f, 0.4f), tileTex, new Vector3(0, -12, 0));
        Tile(root, "Bench",  P(8, 0.25f, -5),  P(6, 0.5f, 1),    tileTex);
        Metal(root, "Crate1", P(-9, 0.6f, 2),  P(1.2f, 1.2f, 1.2f));
        Metal(root, "Crate2", P(9, 0.6f, -3),  P(1.2f, 1.2f, 1.2f));
        Metal(root, "Crate3", P(11, 1.2f, 4),  P(2, 2.4f, 1));

        // —— 长条顶灯：大厅5个 + 走廊2个 ——
        for (int i = 0; i < 5; i++)
            Lamp(root, "Lamp" + i, P(-12 + i * 6, 3.85f, 0));
        Lamp(root, "LampC1", P(18, 3.85f, 0));
        Lamp(root, "LampC2", P(24, 3.85f, 0));
        // 走廊尽头暖色小灯（参考图右上角那盏）
        var warm = EmissiveMat("Corridor_WarmLamp", new Color(1f, 0.55f, 0.2f), 2.5f);
        Box(root, "EndLamp", P(26.8f, 3.5f, 0), P(0.15f, 0.1f, 1.2f), warm);
        var wl = new GameObject("EndLight").AddComponent<Light>();
        wl.type = LightType.Point; wl.color = new Color(1f, 0.55f, 0.25f);
        wl.intensity = 1.4f; wl.range = 8f; wl.shadows = LightShadows.None;
        wl.transform.position = new Vector3(26.2f, 3.3f, 0);
        wl.transform.SetParent(root.transform);

        // —— 出生点标记 ——
        var spawnMat = EmissiveMat("Corridor_Spawn", new Color(1f, 0.48f, 0.1f), 1.5f);
        Box(root, "PlayerSpawn", P(-12, 0.1f, 3), P(0.8f, 0.2f, 0.8f), spawnMat);
        Box(root, "EnemySpawn",  P(10, 0.1f, -3), P(0.8f, 0.2f, 0.8f), spawnMat);

        // —— 暗调环境 + 深色雾 ——
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = new Color(0.25f, 0.28f, 0.32f);
        RenderSettings.ambientEquatorColor = new Color(0.17f, 0.18f, 0.20f);
        RenderSettings.ambientGroundColor  = new Color(0.07f, 0.07f, 0.08f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.09f, 0.10f, 0.12f);
        RenderSettings.fogDensity = 0.035f;
        RenderSettings.skybox = null;

        // —— 相机：第三人称视角，从大厅西侧看向走廊 ——
        var camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        camObj.AddComponent<Camera>();
        camObj.transform.position = new Vector3(-12, 1.9f, 4.2f);
        camObj.transform.LookAt(new Vector3(8, 1.1f, -1.5f));

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("完成", "回廊射击场景已生成：\n" + ScenePath, "好");
    }

    // ————— 基础工具 —————

    static Vector3 P(float x, float y, float z) => new Vector3(x, y, z);

    static void Box(GameObject parent, string name, Vector3 pos, Vector3 size, Material mat, Vector3? euler = null)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        go.transform.localScale = size;
        if (euler.HasValue) go.transform.rotation = Quaternion.Euler(euler.Value);
        go.GetComponent<Renderer>().sharedMaterial = mat;
    }

    // 瓷砖物体：按尺寸自动铺砖（约 4 块/米）
    static void Tile(GameObject parent, string name, Vector3 pos, Vector3 size, Texture2D tex, Vector3? euler = null)
    {
        var mat = NewMat("Tile_" + name, TileCol, 0.25f, 0.0f);
        mat.mainTexture = tex;
        mat.mainTextureScale = new Vector2(Mathf.Max(size.x, size.z), size.y);
        Box(parent, name, pos, size, mat, euler);
    }

    static void Locker(GameObject parent, string name, Vector3 pos, Vector3 size, Texture2D tex)
    {
        var mat = NewMat("Locker_" + name, Color.white, 0.55f, 0.7f);
        mat.mainTexture = tex;
        mat.mainTextureScale = new Vector2(size.x / 1.2f, 1); // 每 1.2m 一扇门
        Box(parent, name, pos, size, mat);
    }

    static void Metal(GameObject parent, string name, Vector3 pos, Vector3 size)
    {
        Box(parent, name, pos, size, NewMat("Metal_" + name, MetalCol, 0.5f, 0.6f));
    }

    static void Lamp(GameObject parent, string name, Vector3 pos)
    {
        var mat = EmissiveMat("LampTube", LampCol, 2.2f);
        Box(parent, name, pos, P(2, 0.08f, 0.25f), mat);
        var l = new GameObject(name + "_Light").AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0.9f, 0.93f, 1f);
        l.intensity = 1.1f; l.range = 9f; l.shadows = LightShadows.None;
        l.transform.position = pos + new Vector3(0, -0.5f, 0);
        l.transform.SetParent(parent.transform);
    }

    // ————— 材质与贴图 —————

    static string ShaderName()
    {
        var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
        if (rp == null) return "Standard";
        return rp.GetType().Name.Contains("Universal") ? "Universal Render Pipeline/Lit" : "HDRP/Lit";
    }

    static Material NewMat(string name, Color color, float smooth, float metallic)
    {
        string path = MatDir + "/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find(ShaderName()));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        mat.SetFloat("_Glossiness", smooth);
        mat.SetFloat("_Smoothness", smooth);
        mat.SetFloat("_Metallic", metallic);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Material EmissiveMat(string name, Color color, float boost)
    {
        var mat = NewMat(name, color, 0.4f, 0f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * boost);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        return mat;
    }

    static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder(MatDir))
            AssetDatabase.CreateFolder("Assets/Materials", "Arena");
    }

    // 瓷砖贴图：256px，4×4 格
    static Texture2D MakeTileTexture(string name, Color baseC, Color lineC)
    {
        string path = MatDir + "/" + name + ".png";
        int res = 256, step = 64, lw = 3;
        var px = new Color[res * res];
        for (int y = 0; y < res; y++)
        for (int x = 0; x < res; x++)
        {
            bool line = (x % step) < lw || (y % step) < lw;
            float v = 1f + (((x * 7 + y * 13) % 11) - 5) * 0.006f; // 微弱明暗变化
            px[y * res + x] = line ? lineC : new Color(baseC.r * v, baseC.g * v, baseC.b * v);
        }
        return SaveTex(path, px, res);
    }

    // 储物柜贴图：512px，横向4扇门，带门缝、通风孔、把手
    static Texture2D MakeLockerTexture(string name)
    {
        string path = MatDir + "/" + name + ".png";
        int res = 512, door = 128;
        var body = new Color(0.20f, 0.21f, 0.23f);
        var edge = new Color(0.11f, 0.12f, 0.13f);
        var handle = new Color(0.45f, 0.47f, 0.50f);
        var px = new Color[res * res];
        for (int y = 0; y < res; y++)
        for (int x = 0; x < res; x++)
        {
            Color c = body;
            int lx = x % door;
            if (lx < 3 || lx > door - 4 || y < 3 || y > res - 4) c = edge;          // 门缝
            if (lx > 20 && lx < door - 20 && y > 40 && y < 100 && (y % 14) < 4) c = edge; // 通风孔
            if (lx > door - 30 && lx < door - 16 && y > 230 && y < 290) c = handle; // 把手
            px[y * res + x] = c;
        }
        return SaveTex(path, px, res);
    }

    static Texture2D SaveTex(string path, Color[] px, int res)
    {
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.SetPixels(px); tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.wrapMode = TextureWrapMode.Repeat;
        imp.filterMode = FilterMode.Bilinear;
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
}
