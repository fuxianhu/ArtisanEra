using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public GameObject player; // 需要有Rigidbody组件
    public float moveForce =30f; // 移动推力
    public float jumpForce =10f; // 跳跃推力
    public Camera playerCamera; // 摄像机引用
    public float mouseSensitivity =2f; // 鼠标灵敏度
    public float cameraPitchMin = -80f; // 摄像机俯仰最小角度
    public float cameraPitchMax =80f; // 摄像机俯仰最大角度

    private Rigidbody rb;
    private Vector3 inputDir;
    private bool jumpPressed;
    private float cameraPitch =0f;

    void Start()
    {
        rb = player.GetComponent<Rigidbody>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        // 记录输入方向
        inputDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) inputDir += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) inputDir += Vector3.back;
        if (Input.GetKey(KeyCode.A)) inputDir += Vector3.left;
        if (Input.GetKey(KeyCode.D)) inputDir += Vector3.right;
        inputDir = inputDir.normalized;

        // 跳跃
        if (Input.GetKeyDown(KeyCode.Space))
            jumpPressed = true;

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Return))
        {
            ToggleFullScreen();
        }

        // 鼠标控制旋转
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        // 玩家左右旋转
        player.transform.Rotate(Vector3.up * mouseX);
        // 摄像机上下旋转
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, cameraPitchMin, cameraPitchMax);
        if (playerCamera != null)
        {
            playerCamera.transform.localEulerAngles = new Vector3(cameraPitch,0f,0f);
        }
    }

    void FixedUpdate()
    {
        // 只在有输入时施加力，松开时靠阻力减速
        if (inputDir != Vector3.zero)
        {
            Vector3 worldDir = player.transform.TransformDirection(inputDir);
            rb.AddForce(worldDir * moveForce, ForceMode.Acceleration);
        }

        // 跳跃（简单实现，建议加地面检测）
        if (jumpPressed)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpPressed = false;
        }
        if (rb.velocity.y < 0) // 当物体向下运动时
        {
            rb.velocity += Vector3.down * 10f * Time.fixedDeltaTime; // 增加下落速度
        }
    }

    void ToggleFullScreen()
    {
#if UNITY_EDITOR
        // 编辑器专用：切换 Game 视图最大化
        var assembly = typeof(UnityEditor.EditorWindow).Assembly;
        var type = assembly.GetType("UnityEditor.GameView");
        var gameView = UnityEditor.EditorWindow.GetWindow(type);
        gameView.maximized = !gameView.maximized;
#else
        // 真机逻辑
        Screen.fullScreen = !Screen.fullScreen;
#endif
    }
}
