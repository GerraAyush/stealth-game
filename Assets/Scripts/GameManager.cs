using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject canvas;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float uiTime;
    [SerializeField] private float messageTime;
    private Player player;
    private bool beingChased = false;
    private Guard[] guards;
    private Image menuPanel;

    void Start()
    {
        menuPanel = canvas.GetComponentInChildren<Image>();
        Button[] buttons = menuPanel.GetComponentsInChildren<Button>();
        Button startButton = buttons[0];
        Button exitButton = buttons[1];

        startButton.onClick.AddListener(StartGame);
        exitButton.onClick.AddListener(QuitGame);

        player = GameObject.Find("Player").GetComponent<Player>();
        guards = GameObject.Find("Guards").GetComponentsInChildren<Guard>();
        messageText.gameObject.SetActive(false);
    }

    public void GameOver() {
        StartCoroutine(EndGame());
    }

    IEnumerator EndGame() {
        messageText.text = "Game Over";
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(uiTime);
        messageText.gameObject.SetActive(false);
        menuPanel.gameObject.SetActive(true);
    }

    public void StartGame()
    {
        menuPanel.gameObject.SetActive(false);
        player.Reset();
        foreach (Guard guard in guards) {
            guard.Reset();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator ChaseStart() {
        messageText.text = "Chase Started";
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(messageTime);
        messageText.gameObject.SetActive(false);
    }

    IEnumerator ChaseEnd() {
        messageText.text = "Chase Ended";
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(messageTime);
        messageText.gameObject.SetActive(false);
    }

    
    public void AlertChase(bool end) {
        if (end) {
            if (!beingChased) {
                return;
            }

            beingChased = false;
            StartCoroutine(ChaseEnd());
        }

        else {
            if (beingChased) {
                return;
            }

            beingChased = true;
            StartCoroutine(ChaseStart());
        }

    }
}
