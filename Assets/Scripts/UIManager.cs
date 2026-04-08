using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    [SerializeField] private GameObject canvas;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float uiTime;
    [SerializeField] private float messageTime;

    private Image menuPanel;
    public static event Action OnStartGameClicked;
    public static event Action OnQuitClicked;


    // Public Methods

    public void ShowGameOverUI() {
        StartCoroutine(GameOverUIRoutine());
    }

    public void ShowChaseStartedAlert() {
        StartCoroutine(ChaseStartUIRoutine());
    }
    
    public void ShowChaseEndedAlert() {
        StartCoroutine(ChaseEndUIRoutine());
    }

    public void SetMenuInActive() {
        menuPanel.gameObject.SetActive(false);
    }


    // Private Methods

    private IEnumerator GameOverUIRoutine() {
        messageText.text = "Game Over";
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(uiTime);
        messageText.gameObject.SetActive(false);
        menuPanel.gameObject.SetActive(true);
    }

    private IEnumerator ChaseStartUIRoutine() {
        messageText.text = "Chase Started";
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(messageTime);
        messageText.gameObject.SetActive(false);
    }

    private IEnumerator ChaseEndUIRoutine() {
        messageText.text = "Chase Ended";
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(messageTime);
        messageText.gameObject.SetActive(false);
    }


    // Lifecycle Methods

    void Start() {
        menuPanel = canvas.GetComponentInChildren<Image>();

        Button[] buttons = menuPanel.GetComponentsInChildren<Button>();
        buttons[0].onClick.AddListener(() => OnStartGameClicked?.Invoke());
        buttons[1].onClick.AddListener(() => OnQuitClicked?.Invoke());

        messageText.gameObject.SetActive(false);
    }
}