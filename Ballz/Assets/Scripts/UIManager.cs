using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject titleUI;
    public GameObject menuUI;
    public GameObject playUI;
    public GameObject endUI;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI endScoreText;
    public TextMeshProUGUI endHighScoreText;

    private int score;
    private int highScore;
    private GameManager.State state;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        titleUI.SetActive(true);
        menuUI.SetActive(false);
        playUI.SetActive(false);

        highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = highScore.ToString();
        state = GameManager.State.Play;
    }

    private void Update()
    {
        if (state != GameManager.Instance.state)
        {
            switch (GameManager.Instance.state)
            {
                case GameManager.State.Title:
                    {
                        titleUI.SetActive(true);
                        menuUI.SetActive(false);
                        playUI.SetActive(false);
                        endUI.SetActive(false);
                    }
                    break;

                case GameManager.State.Play:
                case GameManager.State.BallMove:
                case GameManager.State.LineMove:
                    {
                        titleUI.SetActive(false);
                        menuUI.SetActive(false);
                        playUI.SetActive(true);
                        endUI.SetActive(false);
                    }
                    break;

                case GameManager.State.Pause:
                    {
                        titleUI.SetActive(false);
                        menuUI.SetActive(true);
                        playUI.SetActive(false);
                        endUI.SetActive(false);
                    }
                    break;

                case GameManager.State.End:
                    {
                        titleUI.SetActive(false);
                        menuUI.SetActive(false);
                        playUI.SetActive(false);
                        endUI.SetActive(true);

                        var score = GameManager.Instance.GetScore();
                        endScoreText.text = score.ToString();
                        endHighScoreText.text = $"BEST {highScore}";
                    }
                    break;
            }

            state = GameManager.Instance.state;
        }

        if (state == GameManager.State.Play || state == GameManager.State.BallMove || state == GameManager.State.LineMove)
        {
            var score = GameManager.Instance.GetScore();
            if (this.score != score)
            {
                Canvas.ForceUpdateCanvases();
                scoreText.text = "";
                scoreText.text = score.ToString();
                scoreText.ForceMeshUpdate(true);
                if (score > highScore)
                {
                    highScoreText.text = "";
                    highScoreText.text = score.ToString();
                    highScore = score;
                    highScoreText.ForceMeshUpdate(true);
                }
            }
        }
    }
}