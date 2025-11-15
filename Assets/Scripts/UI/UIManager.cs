// Scripts/UI/UIManager.cs
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [Header("Player UI")]
    public PlayerUI player1UI;
    public PlayerUI player2UI;
    
    [Header("Game Over")]
    public GameObject gameOverPanel;
    public Text gameOverText;
    
    [Header("Turn Info")]
    public Text turnInfoText;
    
    [Header("Debug")]
    public Button debugButton;
    
    private void Awake()
    {
        Instance = this;
    }
    
    
    public void UpdateGameState()
    {
        UpdatePlayerUI(player1UI, ArcomagGameManager.Instance.player1);
        UpdatePlayerUI(player2UI, ArcomagGameManager.Instance.player2);
        UpdateHands();
        UpdateTurnInfo();
        
        ImageFill[] fillComponents = FindObjectsOfType<ImageFill>();
    
        foreach (ImageFill fillComponent in fillComponents)
        {
            fillComponent.UpdateFill();
        }
    }
    
    private void UpdatePlayerUI(PlayerUI ui, PlayerData player)
    {
        if (ui != null)
        {
            ui.UpdatePlayerData(player);
        }
    }
    
    private void UpdateHands()
    {
        if (player1UI != null)
            player1UI.UpdateHand(ArcomagGameManager.Instance.player1);
        
        if (player2UI != null)
            player2UI.UpdateHand(ArcomagGameManager.Instance.player2);
    }
    
    private void UpdateTurnInfo()
    {
        if (turnInfoText != null)
        {
            PlayerData currentPlayer = ArcomagGameManager.Instance.GetCurrentPlayer();
            string playerName = currentPlayer.playerName;
            string turnType = currentPlayer.playerType == PlayerType.AI ? "AI Turn" : "Your Turn";
            
            turnInfoText.text = $"{playerName} - {turnType}";

            turnInfoText.color = currentPlayer.playerType == PlayerType.AI ? Color.red : Color.green;
        }
    }
    
    public void ShowGameOver(string message)
    {
        if (gameOverPanel != null && gameOverText != null)
        {
            gameOverText.text = message;
            gameOverPanel.SetActive(true);
            
            if (player1UI != null) player1UI.SetHandVisibility(false);
            if (player2UI != null) player2UI.SetHandVisibility(false);
        }
    }
    
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}