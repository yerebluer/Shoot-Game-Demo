using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    enum number2String
    {
        One,Two,Three,Four,Five,Six
    }
    public Image fadePlane;
    public GameObject gameOverUI;

    public RectTransform newWaveBanner;
    public Text newWaveTitle;
    public Text newEnemyCount;
    public Text scoreUI;
    public Text gameOverScoreUI;
    public RectTransform healthBar;

    Spwaner spwaner;
    Player player;
    private void Awake()
    {
        healthBar.localScale = new Vector3(1, 1, 1);
        spwaner = FindObjectOfType<Spwaner>();
        spwaner.OnNewWave += OnNewWave;
    }
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.OnDeath += OnGameOver;
        }
    }
    private void Update()
    {
        scoreUI.text = ScoreKeeper.score.ToString("D6");
        float healthPercent = 0;
        if (player != null)
        {
            healthPercent = player.health / player.startingHealth;
        }
        healthBar.localScale = new Vector3(healthPercent, 1, 1);

    }
    void OnNewWave(int WaveNumber)
    {
        newWaveTitle.text = "-Wave " + (number2String)WaveNumber + "-";
        newEnemyCount.text = "Enemies : " + ((spwaner.waves[WaveNumber].infinite)? "Infinate":spwaner.waves[WaveNumber].enermyCount+"");
        StopCoroutine("AnimateNewWaveBanner");
        StartCoroutine("AnimateNewWaveBanner");
    }
    IEnumerator AnimateNewWaveBanner()
    {
        float delayTime = 1.5f;
        float speed = 3f;
        float animatePercent = 0;
        int dir = 1;
        float endDelayTime = Time.time+1/speed+delayTime;

        while (animatePercent >= 0)
        {
            animatePercent += Time.deltaTime * speed * dir;
            if (animatePercent >= 1)
            {
                animatePercent = 1;
                if (Time.time > endDelayTime)
                {
                    dir = -1;
                }
            }
            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-100, 80, animatePercent);
            yield return null;
        }
    }
    void OnGameOver()
    {
        Cursor.visible = true;
        StartCoroutine(Fade(Color.clear,new Color(0,0,0,.95f),1.5f));
        gameOverScoreUI.text = scoreUI.text;
        scoreUI.gameObject.SetActive(false);
        healthBar.transform.parent.gameObject.SetActive(false);
        gameOverUI.SetActive(true);
    }
    IEnumerator Fade(Color from,Color to,float time)
    {
        float speed = 1 / time;
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }

    //UI input
    public void StartNewGame()
    {
        Cursor.visible = false;
        SceneManager.LoadScene("Game");
        //Application.LoadLevel("Game");
    }
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
