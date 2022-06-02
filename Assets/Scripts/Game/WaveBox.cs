using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WaveBox : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Color[] waveBoxColors = new Color[7];
    private string[] enemyNames = { "normal", "immune", "group", "fast", "flying", "swarm", "boss" };
    private EnemyType type;
    private GameScript main;
    [SerializeField] private RectTransform rectTransform;
    private int level;
    public void Init(GameScript main, EnemyType type, int level)
    {
        this.main = main;
        this.type = type;
        typeText.text = enemyNames[(int)type];
        this.level = level;
        levelText.text = "" + (level+1);
        background.color = waveBoxColors[(int)type];
    }
    public void Tick()
    {
        rectTransform.anchoredPosition = Vector2.MoveTowards(rectTransform.anchoredPosition, rectTransform.anchoredPosition + (Vector2.left*50), main.GetWaveBoxSpeed() * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("WaveTrigger"))
        {
            main.NewWave(type, level);
        }
        else if (collision.CompareTag("NewWaveBoxTrigger"))
        {
            main.NewWaveBox();
        }
        else if (collision.CompareTag("WaveBoxKill"))
        {
            main.KillWaveBox(gameObject);
        }
    }
    public void Disable()
    {
        gameObject.SetActive(false);
    }
}
