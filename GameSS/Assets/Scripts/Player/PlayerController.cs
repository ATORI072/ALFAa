using UnityEngine;

/// <summary>
/// 第三人称玩家控制器：WASD 移动（朝相机方向）、鼠标环绕视角、Shift 奔跑、空格跳跃。
/// 用法：挂到角色上，角色需有 CharacterController 组件（没有会自动添加）。
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    public float walkSpeed = 3f;      // 走路速度
    public float runSpeed = 6f;       // 奔跑速度
    public float jumpHeight = 1.2f;   // 跳跃高度
    public float gravity = -20f;      // 重力

    [Header("视角")]
    public float mouseSensitivity = 3f;   // 鼠标灵敏度
    public float cameraDistance = 4f;     // 相机与角色的距离
    public float cameraHeight = 1.6f;     // 相机对准的高度（肩膀附近）
    public float minPitch = -35f;         // 俯视上限
    public float maxPitch = 60f;          // 仰视上限

    [Header("模型与动画")]
    [Tooltip("把角色模型（带 Animator 的物体）拖到这里；留空会自动在子物体里找")]
    public Transform model;
    Animator anim;

    // ———— 动画参数约定（在 Animator Controller 里按这些名字建参数）————
    // Speed    (Float)   水平移动速度：0=待机 3=走 6=跑
    // Grounded (Bool)    是否在地面上
    // Jump     (Trigger) 起跳瞬间
    // Aim      (Bool)    右键瞄准中
    // Shoot    (Trigger) 开火瞬间（PlayerShooter 触发）
    // Dead     (Bool)    死亡/复活（Health 触发）

    CharacterController cc;
    Transform cam;
    float yaw;        // 水平角
    float pitch = 15f; // 垂直角
    float verticalVel; // 垂直速度

    void Start()
    {
        cc = GetComponent<CharacterController>();
        // 找模型上的 Animator（没指定模型就自动找子物体）
        if (model == null)
        {
            var a = GetComponentInChildren<Animator>();
            if (a != null) model = a.transform;
        }
        if (model != null) anim = model.GetComponentInChildren<Animator>();
        // 自动找主相机；没有就创建一个
        if (Camera.main != null) cam = Camera.main.transform;
        else
        {
            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.AddComponent<Camera>();
            cam = camObj.transform;
        }
        yaw = transform.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;   // 锁定鼠标到屏幕中心
        Cursor.visible = false;
    }

    void Update()
    {
        // —— 鼠标环绕视角 ——
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // 相机位置：绕角色旋转；右键瞄准时拉近并偏右肩视角
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
        bool aiming = Input.GetMouseButton(1);
        float dist = aiming ? cameraDistance * 0.5f : cameraDistance;
        Vector3 pivot = transform.position + Vector3.up * cameraHeight;
        if (aiming) pivot += rot * Vector3.right * 0.5f;
        cam.position = pivot - rot * Vector3.forward * dist;
        cam.rotation = rot;

        // —— WASD 移动（相对相机朝向）——
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S
        Vector3 move = rot * new Vector3(h, 0, v);
        move.y = 0;
        move = Vector3.ClampMagnitude(move, 1f);

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // 角色朝移动方向转身
        if (move.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(move), Time.deltaTime * 12f);

        // —— 重力与跳跃 ——
        if (cc.isGrounded)
        {
            verticalVel = -2f; // 贴地
            if (Input.GetKeyDown(KeyCode.Space))
            {
                verticalVel = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (anim != null) anim.SetTrigger("Jump");
            }
        }
        else verticalVel += gravity * Time.deltaTime;

        // 合并移动
        Vector3 velocity = move * speed + Vector3.up * verticalVel;
        cc.Move(velocity * Time.deltaTime);

        // —— 动画参数更新 ——
        if (anim != null)
        {
            Vector3 hv = velocity; hv.y = 0;
            anim.SetFloat("Speed", hv.magnitude);
            anim.SetBool("Grounded", cc.isGrounded);
            anim.SetBool("Aim", aiming);
        }

        // 按 Esc 释放鼠标
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>射击后坐力：视角上抬（由 PlayerShooter 调用）</summary>
    public void AddRecoil(float amount)
    {
        pitch = Mathf.Clamp(pitch + amount, minPitch, maxPitch);
    }
}
