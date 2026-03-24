using UnityEngine;
using Enumerators;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private UIManager uiManager;
    public int CountOfChasers { get; private set; }
    private Player player;
    private Guard[] guards;


    // Public Methods

    public void notify(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Player_Being_Chased:
                CountOfChasers++;

                if (!player.IsBeingChased())
                {
                    player.SetBeingChased();
                    uiManager.ShowChaseStartedAlert();
                }
                break;

            case GameState.Player_Juked_Chased:
                CountOfChasers--;

                if (player.IsBeingChased() && CountOfChasers == 0)
                {
                    player.UnSetBeingChased();
                    uiManager.ShowChaseEndedAlert();
                }
                break;

            case GameState.Player_Got_Caught:
                player.DisableMovement();
                foreach (Guard guard in guards)
                {
                    guard.DisableMovement();
                }
                uiManager.ShowGameOverUI();
                break;

        }
    }


    // Private Methods

    private void StartGame()
    {
        uiManager.SetMenuInActive();

        player.Reset();
        foreach (Guard guard in guards)
        {
            guard.Reset();
        }

        player.EnableMovement();
        foreach (Guard guard in guards)
        {
            guard.EnableMovement();
        }
    }

    private void QuitGame()
    {
        Application.Quit();
    }


    // Lifecycle Methods

    void Awake()
    {
        guards = FindObjectsByType<Guard>(FindObjectsSortMode.None);

        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    void OnEnable()
    {
        UIManager.OnStartGameClicked += StartGame;
        UIManager.OnStartGameClicked += QuitGame;
    }
}
