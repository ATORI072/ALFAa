using UnityEngine;

/// <summary>
/// 战斗HUD（占位）：中心准星、左下血条、右下弹药、左上击杀数、阵亡提示。
/// 用 OnGUI 实现，无需搭 Canvas；之后可替换为正式 UI。
/// 挂在玩家身上。
/// </summary>
public class HUD : MonoBehaviour
{
    Health hp;
    PlayerShooter gun;

    void Start()
    {
        hp = GetComponent<Health>();
        gun = GetComponent<PlayerShooter>();
    }

    void OnGUI()
    {
        float cx = Screen.width / 2f, cy = Screen.height / 2f;

        // —— 准星（中心十字）——
        float s = 8f;
        GUI.DrawTexture(new Rect(cx - s, cy - 1, s * 2, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 1, cy - s, 2, s * 2), Texture2D.whiteTexture);

        // —— 血条（左下）——
        if (hp != null)
        {
            float ratio = hp.currentHP / hp.maxHP;
            GUI.Box(new Rect(20, Screen.height - 60, 204, 22), "");
            GUI.color = Color.Lerp(Color.red, Color.green, ratio);
            GUI.DrawTexture(new Rect(22, Screen.height - 58, 200 * ratio, 18), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(20, Screen.height - 85, 200, 22), "HP " + Mathf.CeilToInt(hp.currentHP));
        }

        // —— 弹药（右下）——
        if (gun != null)
        {
            string ammo = gun.reloading ? "装弹中..." : gun.currentAmmo + " / " + gun.magSize;
            GUI.Label(new Rect(Screen.width - 150, Screen.height - 55, 130, 30), ammo);
        }

        // —— 击杀数（左上）——
        GUI.Label(new Rect(20, 20, 200, 25), "击杀：" + Health.kills);

        // —— 阵亡提示 ——
        if (hp != null && hp.dead)
            GUI.Label(new Rect(cx - 100, cy - 60, 200, 40), "已阵亡，即将重生…");
    }
}
