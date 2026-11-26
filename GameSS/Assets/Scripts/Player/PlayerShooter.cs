using UnityEngine;
using System.Collections;

/// <summary>
/// 玩家射击：左键开火（射线命中）、R 换弹、枪口曳光、后坐力。
/// 准星对准即命中；曳光为占位的亮线，之后可换真实子弹模型。
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerShooter : MonoBehaviour
{
    [Header("武器参数")]
    public float damage = 25f;      // 单发伤害
    public float fireRate = 8f;     // 每秒发数
    public int magSize = 30;        // 弹匣容量
    public float reloadTime = 1.5f; // 换弹秒数
    public float range = 100f;      // 射程
    public float recoil = 0.6f;     // 单发后坐力（视角上抬量）
    public Transform muzzle;        // 枪口位置（曳光起点）

    [HideInInspector] public int currentAmmo;
    [HideInInspector] public bool reloading;

    PlayerController pc;
    Animator anim;   // 模型上的 Animator（开火时触发 Shoot）
    float nextFireTime;
    float reloadEndTime;

    void Start()
    {
        pc = GetComponent<PlayerController>();
        anim = GetComponentInChildren<Animator>();
        currentAmmo = magSize;
    }

    void Update()
    {
        // 换弹中：到点补满
        if (reloading)
        {
            if (Time.time >= reloadEndTime) { reloading = false; currentAmmo = magSize; }
            return;
        }

        // R 换弹
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < magSize)
        {
            reloading = true;
            reloadEndTime = Time.time + reloadTime;
            return;
        }

        // 左键按住连发
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime && currentAmmo > 0)
        {
            nextFireTime = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        currentAmmo--;
        pc.AddRecoil(recoil);
        if (anim != null) anim.SetTrigger("Shoot");

        // 从相机中心发射线，命中即伤害
        Camera cam = Camera.main;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Vector3 hitPoint = ray.origin + ray.direction * range;

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            hitPoint = hit.point;
            var hp = hit.collider.GetComponentInParent<Health>();
            if (hp != null) hp.TakeDamage(damage);
        }

        // 曳光（占位亮线）
        Vector3 from = muzzle != null ? muzzle.position : ray.origin + ray.direction * 1f;
        StartCoroutine(Tracer(from, hitPoint));
    }

    IEnumerator Tracer(Vector3 from, Vector3 to)
    {
        var go = new GameObject("Tracer");
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);
        lr.startWidth = 0.03f; lr.endWidth = 0.03f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = new Color(1f, 0.8f, 0.3f);
        yield return new WaitForSeconds(0.05f);
        Destroy(go);
    }
}
