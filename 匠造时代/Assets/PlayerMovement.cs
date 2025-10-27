using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public GameObject player; // ��Ҫ��Rigidbody���
    public float moveForce =30f; // �ƶ�����
    public float jumpForce =10f; // ��Ծ����
    public Camera playerCamera; // ���������
    public float mouseSensitivity =2f; // ���������
    public float cameraPitchMin = -80f; // �����������С�Ƕ�
    public float cameraPitchMax =80f; // ������������Ƕ�

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
        // ��¼���뷽��
        inputDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) inputDir += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) inputDir += Vector3.back;
        if (Input.GetKey(KeyCode.A)) inputDir += Vector3.left;
        if (Input.GetKey(KeyCode.D)) inputDir += Vector3.right;
        inputDir = inputDir.normalized;

        // ��Ծ
        if (Input.GetKeyDown(KeyCode.Space))
            jumpPressed = true;

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Return))
        {
            ToggleFullScreen();
        }

        // ��������ת
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        // ���������ת
        player.transform.Rotate(Vector3.up * mouseX);
        // �����������ת
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, cameraPitchMin, cameraPitchMax);
        if (playerCamera != null)
        {
            playerCamera.transform.localEulerAngles = new Vector3(cameraPitch,0f,0f);
        }
    }

    void FixedUpdate()
    {
        // ֻ��������ʱʩ�������ɿ�ʱ����������
        if (inputDir != Vector3.zero)
        {
            Vector3 worldDir = player.transform.TransformDirection(inputDir);
            rb.AddForce(worldDir * moveForce, ForceMode.Acceleration);
        }

        // ��Ծ����ʵ�֣�����ӵ����⣩
        if (jumpPressed)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpPressed = false;
        }
        if (rb.velocity.y < 0) // �����������˶�ʱ
        {
            rb.velocity += Vector3.down * 10f * Time.fixedDeltaTime; // ���������ٶ�
        }
    }

    void ToggleFullScreen()
    {
#if UNITY_EDITOR
        // �༭��ר�ã��л� Game ��ͼ���
        var assembly = typeof(UnityEditor.EditorWindow).Assembly;
        var type = assembly.GetType("UnityEditor.GameView");
        var gameView = UnityEditor.EditorWindow.GetWindow(type);
        gameView.maximized = !gameView.maximized;
#else
        // ����߼�
        Screen.fullScreen = !Screen.fullScreen;
#endif
    }
}
