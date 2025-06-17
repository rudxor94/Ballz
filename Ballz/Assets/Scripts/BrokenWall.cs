using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class CountColor
{
    public int threshold;
    public Color color;
}

public class BrokenWall : MonoBehaviour
{
    public TextMeshPro text;
    public List<CountColor> countColors = new();

    private int x;
    private int y;
    private int count;
    private bool moved;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(int x, int y, int count)
    {
        this.x = x;
        this.y = y;
        transform.position = new Vector3(x - 4, y - 4, 0);
        this.count = count;
        text.text = count.ToString();
        UpdateTargetColor();
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

    public bool IsBroken()
    {
        return count <= 0;
    }

    public void MoveWall()
    {
        --y;
        moved = true;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            GameManager.Instance.GameOver();
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {
            AudioManager.Instance.PlaySound("Break");
            if (GameManager.Instance.state == GameManager.State.BallMove)
            {
                count -= 1;
                text.text = count.ToString();
                UpdateTargetColor();
            }
        }
    }

    private void UpdateTargetColor()
    {
        if (countColors == null || countColors.Count == 0) return;

        // 정렬 보장
        countColors.Sort((a, b) => a.threshold.CompareTo(b.threshold));

        Color result = countColors[countColors.Count - 1].color; // default: 제일 높은 값

        for (int i = 0; i < countColors.Count - 1; i++)
        {
            var low = countColors[i];
            var high = countColors[i + 1];

            if (count <= high.threshold)
            {
                float t = Mathf.InverseLerp(low.threshold, high.threshold, count);
                result = Color.Lerp(low.color, high.color, t);
                break;
            }
        }

        spriteRenderer.color = result;
    }
}