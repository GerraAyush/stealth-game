using UnityEngine;
using Enumerators;

public class GameManager : MonoBehaviour
{
    
    [SerializeField] private UIManager uiManager;
    private Player player;
    private GuardManager guardManager;
    

    // Public Methods

    public void notify(GameState gameState) {
        switch(gameState) {
            case GameState.Player_Being_Chased:
                if (!player.IsBeingChased()) {
                    player.SetBeingChased();
                    uiManager.ShowChaseStartedAlert();
                }
                break;

            case GameState.Player_Juked_Chased:
                if (player.IsBeingChased()) {
                    player.UnSetBeingChased();
                    uiManager.ShowChaseEndedAlert();
                }
                break;

            case GameState.Player_Got_Caught:
                player.DisableMovement();
                guardManager.DisableGuardMovements();
                uiManager.ShowGameOverUI();
                break;
            
        }
    }


    // Private Methods

    private void StartGame()
    {
        uiManager.SetMenuInActive();

        player.Reset();
        guardManager.ResetGuards();

        player.EnableMovement();
        guardManager.EnableGuardMovements();
    }

    private void QuitGame()
    {
        Application.Quit();
    }


    // Lifecycle Methods

    void Awake() {
        guardManager = GameObject.Find("GuardManager").GetComponent<GuardManager>();
    }

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    void OnEnable() {
        UIManager.OnStartGameClicked += StartGame;
        UIManager.OnStartGameClicked += QuitGame;
    }
}
