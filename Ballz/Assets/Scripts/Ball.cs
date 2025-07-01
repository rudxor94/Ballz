using UnityEngine;

public class Ball : MonoBehaviour
{
    public float moveSpeed = 10f;
    private Rigidbody2D rb;
    private bool moved = false;
    private Vector2 lastVelocity;

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
            var ballPosition = transform.position;
            var position = BallManager.Instance.GetBallViewPosition();
            ballPosition.y = position.y;
            ballPosition = Vector3.MoveTowards(ballPosition, position, moveSpeed * Time.deltaTime);
            transform.position = ballPosition;

            if ((transform.position - position).magnitude < 0.1f)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void FixedUpdate()
    {
        lastVelocity = rb.linearVelocity;
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

        if (collision.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            AudioManager.Instance.PlaySound("Item");
            rb.linearVelocity = lastVelocity;
            return;
        }

        var normal = collision.contacts[0].normal;
        var newVelocity = rb.linearVelocity.normalized;
        float minAngle = 1f;
        float angle = Vector2.Angle(newVelocity, Vector2.right);

        if (angle < minAngle || angle > 180 - minAngle)
        {
            newVelocity.y = Mathf.Sign(newVelocity.y) * Mathf.Tan(minAngle * Mathf.Deg2Rad);
            rb.linearVelocity = newVelocity * moveSpeed;
        }
    }

    public bool IsMoved()
    {
        return moved;
    }
}