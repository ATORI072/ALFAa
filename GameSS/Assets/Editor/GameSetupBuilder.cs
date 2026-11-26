using UnityEngine;
using UnityEditor;

/// <summary>
/// 一键装配：生成玩家占位胶囊（青色）、敌人预制体（红色胶囊）、配置刷怪点。
/// 菜单：GameSS → 一键装配玩家与敌人
/// </summary>
public static class GameSetupBuilder
{
    [MenuItem("GameSS/一键装配玩家与敌人")]
    public static void Setup()
    {
        // ———— 玩家（占位胶囊，之后可换成自己的模型）————
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.tag = "Player";
            player.GetComponent<Renderer>().sharedMaterial = MakeMat("PlayerCyan", new Color(0.3f, 0.9f, 0.9f));
        }
        // 放到出生点
        var spawn = GameObject.Find("PlayerSpawn");
        if (spawn != null) player.transform.position = spawn.transform.position + Vector3.up * 1f;

        // 挂组件（已挂过就跳过）
        if (player.GetComponent<CharacterController>() == null) player.AddComponent<CharacterController>();
        if (player.GetComponent<PlayerController>() == null) player.AddComponent<PlayerController>();
        var shooter = player.GetComponent<PlayerShooter>();
        if (shooter == null) shooter = player.AddComponent<PlayerShooter>();
        var hp = player.GetComponent<Health>();
        if (hp == null) hp = player.AddComponent<Health>();
        hp.isPlayer = true;
        if (player.GetComponent<HUD>() == null) player.AddComponent<HUD>();

        // 枪口占位（曳光起点）
        var muzzle = player.transform.Find("Muzzle");
        if (muzzle == null)
        {
            var m = new GameObject("Muzzle");
            m.transform.SetParent(player.transform);
            m.transform.localPosition = new Vector3(0.3f, 1.2f, 0.6f);
            muzzle = m.transform;
        }
        shooter.muzzle = muzzle;

        // ———— 敌人预制体（红色胶囊占位）————
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        const string prefabPath = "Assets/Prefabs/Enemy.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            var e = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            e.name = "Enemy";
            e.GetComponent<Renderer>().sharedMaterial = MakeMat("EnemyRed", new Color(0.9f, 0.25f, 0.2f));
            e.AddComponent<CharacterController>();
            e.AddComponent<EnemyAI>();
            var ehp = e.AddComponent<Health>();
            ehp.isPlayer = false;
            ehp.maxHP = 50f;
            PrefabUtility.SaveAsPrefabAsset(e, prefabPath);
            Object.DestroyImmediate(e);
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        // ———— 刷怪点配置（场景里所有名为 EnemySpawn 的物体）————
        int spawnerCount = 0;
        foreach (var t in Object.FindObjectsOfType<Transform>())
        {
            if (t.name == "EnemySpawn")
            {
                var sp = t.GetComponent<EnemySpawner>();
                if (sp == null) sp = t.gameObject.AddComponent<EnemySpawner>();
                sp.enemyPrefab = prefab;
                spawnerCount++;
            }
        }

        // 确保主相机有 MainCamera 标签
        var camObj = GameObject.Find("Main Camera");
        if (camObj != null && Camera.main == null) camObj.tag = "MainCamera";

        EditorUtility.DisplayDialog("完成",
            "装配完毕：\n· 玩家（青色胶囊）已就位\n· 敌人预制体：Assets/Prefabs/Enemy.prefab\n· 刷怪点配置 " + spawnerCount + " 个\n\n直接点运行即可测试！",
            "好");
    }

    static Material MakeMat(string name, Color c)
    {
        // 按当前管线选 shader，避免粉色
        string shaderName = "Standard";
        var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
        if (rp != null)
            shaderName = rp.GetType().Name.Contains("Universal") ? "Universal Render Pipeline/Lit" : "HDRP/Lit";

        string path = "Assets/Materials/Arena/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find(shaderName));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = c;
        EditorUtility.SetDirty(mat);
        return mat;
    }
}
