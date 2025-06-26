using UnityEngine;

public class Ball : MonoBehaviour
{
    public float moveSpeed = 10f;
    private Rigidbody2D rb;
    private bool moved = false;
    private Vector2 lastVelocity;
    private bool alreadyCollision;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // 중력 제거
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 충돌 정확도 향상
    }

    private void Update()
    {
        if (moved == false)
        {
            var position = BallManager.Instance.GetBallViewPosition();
            transform.position = Vector3.MoveTowards(transform.position, position, moveSpeed * Time.deltaTime);
            if ((transform.position - position).magnitude < 0.1f)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void FixedUpdate()
    {
        lastVelocity = rb.linearVelocity;
        alreadyCollision = false;
    }

    public void ShootBall(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * moveSpeed;
        moved = true;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            moved = false;
            rb.linearVelocity = Vector2.zero;
            var position = transform.position;
            transform.position = new Vector3(position.x, -4.3f, position.z);

            BallManager.Instance.SetBallViewPosition(transform.position);

            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {
            rb.linearVelocity = lastVelocity;
            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            AudioManager.Instance.PlaySound("Item");
            rb.linearVelocity = lastVelocity;
            return;
        }

        if (alreadyCollision)
        {
            rb.linearVelocity = lastVelocity;
            return;
        }

        // 반사 벡터 계산
        Vector2 normal = collision.contacts[0].normal;
        Vector2 reflected = Vector2.Reflect(lastVelocity, normal).normalized;

        float minAngleFromHorizontal = 1f; // 최소 1도 이상 위로 튕기게

        // reflected 벡터의 각도 확인
        float angle = Vector2.Angle(reflected, Vector2.right); // 0~180도 사이

        // 만약 거의 수평(10도 이하)이면 강제로 보정
        if (angle < minAngleFromHorizontal || angle > 180 - minAngleFromHorizontal)
        {
            // 보정 방향: y값 최소 확보 + 방향 유지
            reflected.y = Mathf.Sign(reflected.y) * Mathf.Tan(minAngleFromHorizontal * Mathf.Deg2Rad);
            reflected = reflected.normalized;
        }

        rb.linearVelocity = reflected * moveSpeed;
        lastVelocity = rb.linearVelocity;
        alreadyCollision = true;
    }

    public bool IsMoved()
    {
        return moved;
    }
}