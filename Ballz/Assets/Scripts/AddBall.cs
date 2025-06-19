using UnityEngine;

public class AddBall : MonoBehaviour
{
    public float moveSpeed = 10f;
    private bool turnStartMove = false;

    public void OnEnable()
    {
        turnStartMove = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (transform.position.y > -4.3f)
        {
            transform.Translate(new Vector3(0, -Time.deltaTime * moveSpeed, 0));
            if (transform.position.y <= -4.3f)
            {
                var position = transform.position;
                transform.position = new Vector3(position.x, -4.3f, position.z);
            }
        }

        if (turnStartMove)
        {
            var targetPos = BallManager.Instance.GetBallViewPosition();
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            {
                GameManager.Instance.OnAddBallArrived(this);
            }
        }
    }

    public void TurnStartMove()
    {
        turnStartMove = true;
    }
}