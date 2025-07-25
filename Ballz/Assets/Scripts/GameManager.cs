using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum State
    {
        Title,
        Play,
        BallMove,
        LineMove,
        Pause,
        End
    }

    public State state;

    private List<BrokenWall> usedWall = new List<BrokenWall>();
    private List<Item> usedItem = new List<Item>();
    private List<AddBall> usedAddBall = new List<AddBall>();
    private int score = 0;
    private float ballMoveTime = 0;
    private bool fast = false;
    private bool warning = false;
    private State beforeState;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Instance = this;
        state = State.Title;
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateState();
    }

    private void UpdateState()
    {
        switch (state)
        {
            case State.Play:
                {
                    if (BallManager.Instance.IsShoot())
                    {
                        state = State.BallMove;
                    }
                }
                break;

            case State.BallMove:
                {
                    ballMoveTime += Time.deltaTime;
                    var unusedWall = new List<BrokenWall>();
                    foreach (var wall in usedWall)
                    {
                        if (wall.IsBroken())
                        {
                            ResourceManager.Instance.DestroyResource("Prefab/BrokenWall", wall.gameObject);
                            EffectManager.Instance.PlayEffect("Broken", wall.transform.position);
                            unusedWall.Add(wall);
                        }
                    }

                    foreach (var wall in unusedWall)
                    {
                        usedWall.Remove(wall);
                    }

                    var unusedItem = new List<Item>();
                    foreach (var item in usedItem)
                    {
                        if (item.IsEat())
                        {
                            CreateAddBall(item.transform.position);
                            ResourceManager.Instance.DestroyResource("Prefab/Item", item.gameObject);
                            EffectManager.Instance.PlayEffect("GetItem", item.transform.position);
                            unusedItem.Add(item);
                        }
                    }

                    foreach (var item in unusedItem)
                    {
                        usedItem.Remove(item);
                    }

                    if (BallManager.Instance.IsShoot() == false)
                    {
                        state = State.LineMove;
                        TurnStart();
                    }
                }
                break;

            case State.LineMove:
                {
                    foreach (var wall in usedWall)
                    {
                        if (wall.GetComponent<BrokenWall>().IsMoved()) return;
                    }
                    state = State.Play;
                }
                break;
        }
    }

    public int GetScore()
    {
        return score;
    }

    public bool ShowFast()
    {
        return fast == false && ballMoveTime >= 5;
    }

    public bool IsFast()
    {
        return fast;
    }

    public bool IsWarning()
    {
        return warning;
    }

    public void SetFast()
    {
        fast = true;
        Time.timeScale = 3;
    }

    public void GameStart()
    {
        ClearGame();
        TurnStart();
        BallManager.Instance.Init();
        state = State.Play;
        Time.timeScale = 1;
    }

    public void ToTitle()
    {
        ClearGame();
        state = State.Title;
    }

    public void ToMenu()
    {
        Time.timeScale = 0;
        beforeState = state;
        state = State.Pause;
    }

    public void Continue()
    {
        state = beforeState;
        Time.timeScale = fast ? 3 : 1;
    }

    public void GameOver()
    {
        var highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (score > highScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
        }
        state = State.End;
    }

    public void ClearGame()
    {
        foreach (var wall in usedWall)
        {
            ResourceManager.Instance.DestroyResource("Prefab/BrokenWall", wall.gameObject);
        }

        foreach (var item in usedItem)
        {
            ResourceManager.Instance.DestroyResource("Prefab/Item", item.gameObject);
        }

        usedWall.Clear();
        usedItem.Clear();
        score = 0;
    }

    public void TurnStart()
    {
        Time.timeScale = 1;
        ballMoveTime = 0;
        fast = false;

        foreach (var addBall in usedAddBall)
        {
            addBall.TurnStartMove();
        }
        usedAddBall.Clear();

        AudioManager.Instance.PlaySound("Next");

        CreateLine();
    }

    public void OnAddBallArrived(AddBall addBall)
    {
        usedAddBall.Remove(addBall);
        ResourceManager.Instance.DestroyResource("Prefab/AddBall", addBall.gameObject);
    }

    public void CreateLine()
    {
        ++score;

        var positions = new List<int> { 1, 2, 3, 4, 5, 6, 7 };

        if (score > 1)
        {
            var itemIndex = Random.Range(0, positions.Count);
            var itemPos = positions[itemIndex];
            positions.RemoveAt(itemIndex);
            CreateItem(itemPos);
        }

        var minWall = Mathf.Min(2 + score / 50, 5);
        var wallCount = Random.Range(minWall, 7);

        for (var i = 1; i <= wallCount; ++i)
        {
            var wallIndex = Random.Range(0, positions.Count);
            var wallPos = positions[wallIndex];
            positions.RemoveAt(wallIndex);
            CreateBrokenWall(wallPos);
        }

        MoveLine();
    }

    public void CreateBrokenWall(int x)
    {
        var newWall = ResourceManager.Instance.GetResource("Prefab/BrokenWall");
        newWall.transform.parent = GameObject.Find("GameScreen").transform;
        newWall.SetActive(true);
        var brokenWall = newWall.GetComponent<BrokenWall>();
        var strongWallChance = Mathf.Min(2 + score / 100, 7);
        brokenWall.Init(x, 8, Random.Range(0, 10) < strongWallChance ? score * 2 : score);

        usedWall.Add(brokenWall);
    }

    public void CreateItem(int x)
    {
        var newItem = ResourceManager.Instance.GetResource("Prefab/Item");
        newItem.transform.parent = GameObject.Find("GameScreen").transform;
        newItem.SetActive(true);
        var item = newItem.GetComponent<Item>();
        item.Init(x, 8);

        usedItem.Add(item);
    }

    public void CreateAddBall(Vector3 position)
    {
        var newAddBall = ResourceManager.Instance.GetResource("Prefab/AddBall");
        newAddBall.transform.parent = GameObject.Find("GameScreen").transform;
        newAddBall.SetActive(true);
        newAddBall.transform.position = position;

        usedAddBall.Add(newAddBall.GetComponent<AddBall>());
    }

    public void MoveLine()
    {
        warning = false;
        foreach (var wall in usedWall)
        {
            var brokenWall = wall.GetComponent<BrokenWall>();
            brokenWall.MoveWall();
            if (brokenWall.warning)
            {
                warning = true;
            }
        }

        foreach (var wall in usedItem)
        {
            wall.GetComponent<Item>().MoveItem();
        }

        state = State.LineMove;
    }
}