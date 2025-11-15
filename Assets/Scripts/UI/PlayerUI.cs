// Scripts/UI/PlayerUI.cs
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Player Stats")]
    public Text towerText;
    public Text wallText;
    
    public Text bricksText;
    public Text gemsText;
    public Text recruitsText;
    
    public Text quarryText;
    public Text magicText;
    public Text dungeonText;
    
    [Header("Hand Container")]
    public Transform handContainer;
    public GameObject cardPrefab;
    
    [Header("Player Indicator")]
    public GameObject turnIndicator;
    
    [Header("Player Info")]
    public GameObject playerPanel;
    public bool isHumanPlayer = false;
    
    [Header("AI Card Settings")]
    public Color aiCardColor = new Color(0.8f, 0.8f, 1f, 0.8f);
    
    public void UpdatePlayerData(PlayerData player)
    {
        if (player == null) return;
        
        // Update basic stats
        if (towerText != null) towerText.text = $"Tower: {player.tower}";
        if (wallText != null) wallText.text = $"Wall: {player.wall}";
        
        // Update resources
        if (bricksText != null) bricksText.text = $"Bricks: {player.bricks}";
        if (gemsText != null) gemsText.text = $"Gems: {player.gems}";
        if (recruitsText != null) recruitsText.text = $"Recruits: {player.recruits}";
        
        // Update production
        if (quarryText != null) quarryText.text = $"Quarry: {player.quarry}";
        if (magicText != null) magicText.text = $"Magic: {player.magic}";
        if (dungeonText != null) dungeonText.text = $"Dungeon: {player.dungeon}";
        
        bool isCurrentPlayer = ArcomagGameManager.Instance != null && ArcomagGameManager.Instance.IsCurrentPlayer(player);
        UpdatePlayerVisibility(isCurrentPlayer);
    }
    
    private void UpdatePlayerVisibility(bool isCurrentPlayer)
    {
        if (turnIndicator != null)
        {
            turnIndicator.SetActive(isCurrentPlayer);
        }
        
        if (handContainer != null)
        {
            handContainer.gameObject.SetActive(isCurrentPlayer);
        }
        
        if (playerPanel != null)
        {
            CanvasGroup canvasGroup = playerPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = playerPanel.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = isCurrentPlayer ? 1f : 0.7f;
        }
    }
    
    public void UpdateHand(PlayerData player)
    {
        if (handContainer == null || player == null) return;
        
        foreach (Transform child in handContainer)
        {
            Destroy(child.gameObject);
        }
        
        bool isCurrentPlayer = ArcomagGameManager.Instance != null && ArcomagGameManager.Instance.IsCurrentPlayer(player);
        
        if (isCurrentPlayer && player.hand != null)
        {
            foreach (CardData card in player.hand)
            {
                if (card == null) continue;
                
                GameObject cardObject = Instantiate(cardPrefab, handContainer);
                CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();
                if (cardDisplay != null)
                {
                    cardDisplay.Initialize(card, player);
                    
                    if (player.playerType == PlayerType.AI)
                    {
                        cardDisplay.SetAsAICard(aiCardColor);
                    }
                }
            }
        }
    }
    
    public void SetHandVisibility(bool visible)
    {
        if (handContainer != null)
        {
            handContainer.gameObject.SetActive(visible);
        }
    }
}