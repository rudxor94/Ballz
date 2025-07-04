using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BallManager : MonoBehaviour
{
    public static BallManager Instance { get; private set; }

    public GameObject ballView;
    public TextMeshPro ballViewText;
    public GameObject addBallText;
    public GameObject fakeBall;

    public float ballShootDelay = 0.1f;
    public float ballRadius = 0.15f;

    public Vector3 initPosition = new Vector3(0, -4.3f, 0);

    private List<Ball> activeBalls = new List<Ball>();
    private bool shoot = false;
    private int totalBalls = 1;
    private int addBall = 0;
    private bool firstBallFinish = false;
    private Vector3 firstBallPosition;

    private bool isAiming = false;
    private Vector3 lineEndPos;
    private LineRenderer aimLine;

    private void Start()
    {
        Instance = this;
        aimLine = GetComponent<LineRenderer>();
    }

    public void Init()
    {
        ballView.gameObject.SetActive(true);
        ballView.transform.position = initPosition;
        foreach (var ball in activeBalls)
        {
            ResourceManager.Instance.DestroyResource("Prefab/Ball", ball.gameObject);
        }
        activeBalls.Clear();

        shoot = false;
        totalBalls = 1;
        addBall = 0;
        firstBallFinish = false;

        ballViewText.text = $"X{totalBalls}";
        addBallText.SetActive(false);
    }

    private void Update()
    {
        if (shoot == false)
        {
            if (GameManager.Instance.state != GameManager.State.Play) return;

            // 마우스 또는 터치 시작
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                if (!IsPointerOverUI())
                {
                    isAiming = true;
                }
            }

            // 드래그 중 (조준선 그리기)
            if (isAiming && Pointer.current != null && Pointer.current.press.isPressed)
            {
                lineEndPos = GetInputWorldPosition();
                UpdateAimLine(ballView.transform.position, lineEndPos);
            }

            // 손을 뗐을 때
            if (isAiming && Pointer.current != null && Pointer.current.press.wasReleasedThisFrame)
            {
                Vector3 inputPos = GetInputWorldPosition(true);
                var shootDir = (inputPos - ballView.transform.position).normalized;

                if (shootDir.y >= 0.02f)
                {
                    ShootAllBalls(shootDir);
                    shoot = true;
                }

                isAiming = false;
                HideAimLine();
            }
        }
        else
        {
            foreach (var ball in activeBalls)
            {
                if (ball.IsMoved())
                {
                    return;
                }
            }

            shoot = false;
            ballView.transform.position = firstBallPosition;
            ballView.SetActive(true);
            fakeBall.SetActive(false);

            if (addBall > 0)
            {
                totalBalls += addBall;
                addBallText.GetComponent<TextMeshPro>().text = $"+{addBall}";
                addBallText.SetActive(true);
                addBallText.GetComponent<Animator>().Play("AddBall", 0);
                addBall = 0;
            }

            ballViewText.text = $"X{totalBalls}";
        }
    }

    public void ShootAllBalls(Vector2 shootDir)
    {
        shoot = true;
        firstBallFinish = false;

        addBallText.SetActive(false);
        StartCoroutine(ShootBallsCoroutine(shootDir));
        ballView.gameObject.SetActive(false);
    }

    private IEnumerator ShootBallsCoroutine(Vector2 dir)
    {
        for (int i = 0; i < totalBalls; i++)
        {
            if (i >= activeBalls.Count)
            {
                var newBall = ResourceManager.Instance.GetResource("Prefab/Ball");
                activeBalls.Add(newBall.GetComponent<Ball>());
            }

            var ball = activeBalls[i];
            ball.transform.position = ballView.transform.position;
            ball.gameObject.SetActive(true);
            ball.ShootBall(dir);

            yield return new WaitForSeconds(ballShootDelay);
        }
    }

    public void SetBallViewPosition(Vector3 position)
    {
        if (firstBallFinish) return;

        firstBallFinish = true;
        firstBallPosition = position;
        fakeBall.SetActive(true);
        fakeBall.transform.position = firstBallPosition;
    }

    public bool IsShoot()
    {
        return shoot;
    }

    public Vector3 GetBallViewPosition()
    {
        return shoot ? firstBallPosition : ballView.transform.position;
    }

    public void AddBall()
    {
        ++addBall;
    }

    private Vector3 GetInputWorldPosition(bool allowLastTouch = false)
    {
        if (Pointer.current != null)
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            return Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        }

        return Vector3.zero;
    }

    private bool IsPointerOverUI()
    {
#if UNITY_WEBGL
        // WebGL에서는 Touchscreen 없을 수도 있으니 Pointer로 체크
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
#else
    if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
    {
        int fingerId = Touchscreen.current.primaryTouch.touchId.ReadValue();
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId);
    }

    return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
#endif
    }

    private void UpdateAimLine(Vector3 from, Vector3 to)
    {
        var layerMask = LayerMask.GetMask("Wall", "BrokenWall");

        var from2D = from;
        var dir = (to - from).normalized;
        var distance = Vector2.Distance(from, to);

        RaycastHit2D hit = Physics2D.CircleCast(from2D, ballRadius, dir, 100f, layerMask);
        if (hit.collider != null)
        {
            to = hit.point;
        }

        // Z값 고정 (2D용)
        from.z = 0;
        to.z = 0;

        aimLine.enabled = dir.y >= 0.02f;
        aimLine.positionCount = 2;
        aimLine.SetPosition(0, to);
        aimLine.SetPosition(1, from);
    }

    private void HideAimLine()
    {
        aimLine.enabled = false;
    }
}