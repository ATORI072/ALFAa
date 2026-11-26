using UnityEngine;

/// <summary>
/// 敌人AI（占位）：巡逻 → 发现玩家追击 → 近身攻击。
/// 模型暂用胶囊，之后换模型时此脚本不用动。
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class EnemyAI : MonoBehaviour
{
    [Header("感知")]
    public float detectRange = 15f;   // 发现玩家的距离
    public float attackRange = 1.8f;  // 进入攻击的距离
    [Header("移动")]
    public float moveSpeed = 3.5f;
    public float patrolRadius = 6f;   // 巡逻范围（以出生点为圆心）
    [Header("攻击")]
    public float damage = 10f;
    public float attackCooldown = 1f;

    CharacterController cc;
    Transform player;
    Health playerHP;
    Vector3 spawnPos;
    Vector3 patrolTarget;
    float nextAttackTime;
    float verticalVel;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        spawnPos = transform.position;
        PickPatrolTarget();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) { player = p.transform; playerHP = p.GetComponent<Health>(); }
    }

    void Update()
    {
        // 玩家不存在或已死亡：回去巡逻
        if (player == null || (playerHP != null && playerHP.dead)) { Patrol(); ApplyGravity(); return; }

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackRange) Attack();
        else if (dist <= detectRange && CanSeePlayer()) Chase();
        else Patrol();

        ApplyGravity();
    }

    // 视线检测：中间有墙挡住就看不到
    bool CanSeePlayer()
    {
        Vector3 eye = transform.position + Vector3.up * 1.5f;
        Vector3 target = player.position + Vector3.up * 1.2f;
        if (Physics.Raycast(eye, target - eye, out RaycastHit hit, detectRange))
            return hit.transform == player || hit.transform.IsChildOf(player);
        return false;
    }

    void Chase() { MoveTowards(player.position, moveSpeed); }

    void Attack()
    {
        FaceTo(player.position);
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            if (playerHP != null) playerHP.TakeDamage(damage);
        }
    }

    void Patrol()
    {
        if (Vector3.Distance(transform.position, patrolTarget) < 1f) PickPatrolTarget();
        MoveTowards(patrolTarget, moveSpeed * 0.5f);
    }

    void PickPatrolTarget()
    {
        Vector2 c = Random.insideUnitCircle * patrolRadius;
        patrolTarget = spawnPos + new Vector3(c.x, 0, c.y);
    }

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 dir = target - transform.position; dir.y = 0;
        if (dir.sqrMagnitude < 0.01f) return;
        dir.Normalize();
        FaceTo(target);
        cc.Move(dir * speed * Time.deltaTime);
    }

    void FaceTo(Vector3 target)
    {
        Vector3 dir = target - transform.position; dir.y = 0;
        if (dir.sqrMagnitude < 0.01f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(dir), Time.deltaTime * 8f);
    }

    void ApplyGravity()
    {
        verticalVel = cc.isGrounded ? -2f : verticalVel - 20f * Time.deltaTime;
        cc.Move(Vector3.up * verticalVel * Time.deltaTime);
    }
}
