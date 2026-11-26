using UnityEngine;
using UnityEditor;

/// <summary>
/// 把玩家占位胶囊替换成导入的角色模型（芙铃）。
/// 菜单：GameSS → 用模型替换玩家胶囊
/// </summary>
public static class PlayerModelSwapper
{
    const string ModelPath = "Assets/Player/Model/PlayerModel/芙铃/黑白.fbx";

    [MenuItem("GameSS/用模型替换玩家胶囊")]
    public static void Swap()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            EditorUtility.DisplayDialog("提示", "场景里还没有玩家。\n请先点菜单 GameSS → 一键装配玩家与敌人。", "好");
            return;
        }

        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        if (model == null)
        {
            EditorUtility.DisplayDialog("错误", "找不到模型：" + ModelPath, "好");
            return;
        }

        // 1. 去掉胶囊外观（保留 CharacterController 碰撞）
        var mr = player.GetComponent<MeshRenderer>(); if (mr != null) Object.DestroyImmediate(mr);
        var mf = player.GetComponent<MeshFilter>();   if (mf != null) Object.DestroyImmediate(mf);
        var cap = player.GetComponent<CapsuleCollider>(); if (cap != null) Object.DestroyImmediate(cap);

        // 2. 清掉旧模型实例（防止重复点菜单叠两个）
        var old = player.transform.Find("PlayerModel");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        // 3. 实例化模型作为玩家子物体
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(model);
        inst.name = "PlayerModel";
        inst.transform.SetParent(player.transform);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;

        // 4. 自动缩放到人高 1.65m，脚底对齐地面
        var bounds = CalcBounds(inst);
        if (bounds.size.y > 0.01f)
        {
            float scale = 1.65f / bounds.size.y;
            inst.transform.localScale = Vector3.one * scale;
            var b2 = CalcBounds(inst);
            inst.transform.position += new Vector3(0, player.transform.position.y - b2.min.y, 0);
        }

        // 5. 碰撞体贴合模型身高
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) { cc.height = 1.7f; cc.center = new Vector3(0, 0.85f, 0); }

        EditorUtility.DisplayDialog("完成", "玩家模型已替换为芙铃。\n模型暂无动画（站立姿势），动画之后配 Animator。", "好");
    }

    static Bounds CalcBounds(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.one);
        var b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }
}
