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
        rb.gravityScale = 0f; // �߷� ����
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // �浹 ��Ȯ�� ���
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

        // �ݻ� ���� ���
        Vector2 normal = collision.contacts[0].normal;
        Vector2 reflected = Vector2.Reflect(lastVelocity, normal).normalized;

        float minAngleFromHorizontal = 1f; // �ּ� 1�� �̻� ���� ƨ���

        // reflected ������ ���� Ȯ��
        float angle = Vector2.Angle(reflected, Vector2.right); // 0~180�� ����

        // ���� ���� ����(10�� ����)�̸� ������ ����
        if (angle < minAngleFromHorizontal || angle > 180 - minAngleFromHorizontal)
        {
            // ���� ����: y�� �ּ� Ȯ�� + ���� ����
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