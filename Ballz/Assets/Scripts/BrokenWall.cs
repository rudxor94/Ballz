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
    public bool warning = false;

    private int x;
    private int y;
    private int count;
    private int initialCount;
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
        initialCount = count;
        text.text = count.ToString();
        warning = false;
        UpdateTargetColor();
    }

    // Update is called once per frame
    private void LateUpdate()
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

        warning = y <= 1;
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

        Color result = countColors[0].color; // 기본값: 가장 낮은 색
        var colorCount = initialCount;

        for (int i = countColors.Count - 1; i > 0; i--)
        {
            var lower = countColors[i - 1];
            var upper = countColors[i];

            if (initialCount >= upper.threshold)
            {
                if (initialCount == count)
                {
                    result = upper.color; // 초기값이 구간 이상이고 아직 안 맞았으면 유지
                    break;
                }
                else if (initialCount > count && count > lower.threshold)
                {
                    float t = Mathf.InverseLerp(colorCount, lower.threshold, count);
                    result = Color.Lerp(upper.color, lower.color, t);
                    break;
                }
                colorCount -= (upper.threshold - lower.threshold);
            }
        }

        spriteRenderer.color = result;
    }
}