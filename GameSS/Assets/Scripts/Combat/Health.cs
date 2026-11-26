using UnityEngine;
using System.Collections;

/// <summary>
/// 通用生命组件：玩家和敌人共用。
/// 玩家死亡 → 倒计时后回出生点复活；敌人死亡 → 销毁并累计击杀数。
/// </summary>
public class Health : MonoBehaviour
{
    [Header("基础")]
    public float maxHP = 100f;
    public bool isPlayer;           // 勾选=玩家，不勾=敌人
    public float respawnDelay = 3f; // 玩家重生等待秒数

    [HideInInspector] public float currentHP;
    [HideInInspector] public bool dead;

    public static int kills;        // 击杀计数（HUD 显示用）

    Animator anim;                  // 模型上的 Animator（死亡/复活时触发 Dead）

    void Awake()
    {
        currentHP = maxHP;
        anim = GetComponentInChildren<Animator>();
    }

    /// <summary>受伤入口：子弹、近战都调这里</summary>
    public void TakeDamage(float dmg)
    {
        if (dead) return;
        currentHP -= dmg;
        if (currentHP <= 0) { currentHP = 0; Die(); }
    }

    void Die()
    {
        dead = true;
        if (anim != null) anim.SetBool("Dead", true);
        if (isPlayer) StartCoroutine(Respawn());
        else { kills++; Destroy(gameObject, 0.3f); }
    }

    IEnumerator Respawn()
    {
        // 死亡期间禁用操作
        var pc = GetComponent<PlayerController>();
        var ps = GetComponent<PlayerShooter>();
        if (pc) pc.enabled = false;
        if (ps) ps.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        // 回 PlayerSpawn 复活（传送前必须关碰撞体）
        var spawn = GameObject.Find("PlayerSpawn");
        if (spawn != null)
        {
            var cc = GetComponent<CharacterController>();
            if (cc) cc.enabled = false;
            transform.position = spawn.transform.position + Vector3.up * 0.5f;
            if (cc) cc.enabled = true;
        }
        currentHP = maxHP;
        dead = false;
        if (anim != null) anim.SetBool("Dead", false);
        if (pc) pc.enabled = true;
        if (ps) ps.enabled = true;
    }
}
