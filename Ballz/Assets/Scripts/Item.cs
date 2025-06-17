using TMPro;
using UnityEngine;

public class Item : MonoBehaviour
{
    private int x;
    private int y;
    private bool moved;
    private bool eat;

    public void Init(int x, int y)
    {
        this.x = x;
        this.y = y;
        eat = false;
        transform.position = new Vector3(x - 4, y - 4, 0);
    }

    // Update is called once per frame
    private void Update()
    {
        if (moved == true)
        {
            transform.Translate(new Vector3(0, -Time.deltaTime * 2, 0), Space.World);
            if (transform.position.y <= y - 4)
            {
                transform.position = new Vector3(x - 4, y - 4, 0);
                moved = false;
            }
        }
    }

    public bool IsMoved()
    {
        return moved;
    }

    public bool IsEat()
    {
        return eat;
    }

    public void MoveItem()
    {
        --y;
        moved = true;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {
            if (GameManager.Instance.state == GameManager.State.BallMove)
            {
                eat = true;
                BallManager.Instance.AddBall();
            }
        }
    }
}