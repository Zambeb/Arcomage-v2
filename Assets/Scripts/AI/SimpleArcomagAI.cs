// Scripts/AI/SimpleArcomagAI.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimpleArcomagAI : MonoBehaviour
{
    private ArcomagGameManager gameManager;
    private UIManager uiManager;
    private bool isInitialized = false;
    
    private void Start()
    {
        InitializeAI();
    }
    
    private void InitializeAI()
    {
        gameManager = ArcomagGameManager.Instance;
        uiManager = UIManager.Instance;
        
        if (gameManager != null && uiManager != null)
        {
            isInitialized = true;
            Debug.Log("AI initialized successfully");
        }
    }
    
    public void MakeAIMove(PlayerData aiPlayer)
    {
        if (!isInitialized)
        {
            InitializeAI();
        }
        
        if (gameManager == null || aiPlayer == null) return;
        
        StartCoroutine(AIMoveCoroutine(aiPlayer));
    }
    
    private IEnumerator AIMoveCoroutine(PlayerData aiPlayer)
    {
        yield return new WaitForSeconds(1.5f);
        
        var playableCards = aiPlayer.GetPlayableCards();
        
        if (playableCards.Count > 0)
        {
            CardData chosenCard = ChooseCard(playableCards, aiPlayer);
            
            if (chosenCard != null)
            {
                CardDisplay cardDisplay = FindCardDisplay(chosenCard, aiPlayer);
                
                if (cardDisplay != null)
                {
                    Debug.Log($"{aiPlayer.playerName} plays with animation: {chosenCard.cardName}");
                    cardDisplay.PlayCardAsAI();
                }
                else
                {
                    Debug.Log($"{aiPlayer.playerName} plays (fallback): {chosenCard.cardName}");
                    gameManager.PlayCard(chosenCard, aiPlayer);
                }
            }
        }
        else
        {
            if (aiPlayer.hand.Count > 0)
            {
                CardData cardToDiscard = aiPlayer.hand[Random.Range(0, aiPlayer.hand.Count)];
                
                CardDisplay cardDisplay = FindCardDisplay(cardToDiscard, aiPlayer);
                
                if (cardDisplay != null)
                {
                    Debug.Log($"{aiPlayer.playerName} discards with animation: {cardToDiscard.cardName}");
                    cardDisplay.DiscardCardAsAI();
                }
                else
                {
                    // Fallback
                    Debug.Log($"{aiPlayer.playerName} discards (fallback): {cardToDiscard.cardName}");
                    gameManager.DiscardCard(cardToDiscard, aiPlayer);
                }
            }
        }
    }
    
    private CardDisplay FindCardDisplay(CardData card, PlayerData player)
    {
        CardDisplay[] allCardDisplays = FindObjectsOfType<CardDisplay>();
        
        foreach (CardDisplay display in allCardDisplays)
        {
            if (display.GetCardData() == card && display.GetOwner() == player)
            {
                return display;
            }
        }
        
        return null;
    }
    
    private CardData ChooseCard(List<CardData> playableCards, PlayerData aiPlayer)
    {
        if (playableCards == null || playableCards.Count == 0)
            return null;
            
        if (gameManager == null)
        {
            return playableCards[0]; // Fallback
        }
        
        PlayerData humanPlayer = GetHumanPlayer(aiPlayer);
        
        if (humanPlayer == null)
        {
            return playableCards[Random.Range(0, playableCards.Count)]; // Fallback
        }
        
        if (humanPlayer.wall <= 5)
        {
            var attackCards = playableCards.Where(card => 
                card != null && card.effects.Any(effect => 
                    effect.effectType == CardEffectType.DamageBoth || 
                    effect.effectType == CardEffectType.DamageTower ||
                    effect.effectType == CardEffectType.DamageWall
                )).ToList();
                
            if (attackCards.Count > 0)
                return attackCards[Random.Range(0, attackCards.Count)];
        }
        
        if (aiPlayer.tower <= 10)
        {
            var defenseCards = playableCards.Where(card => 
                card != null && card.effects.Any(effect => 
                    effect.effectType == CardEffectType.BuildTower ||
                    effect.effectType == CardEffectType.BuildWall
                )).ToList();
                
            if (defenseCards.Count > 0)
                return defenseCards[Random.Range(0, defenseCards.Count)];
        }
        return playableCards[Random.Range(0, playableCards.Count)];
    }
    
    private PlayerData GetHumanPlayer(PlayerData aiPlayer)
    {
        if (gameManager == null) return null;
        
        if (aiPlayer == gameManager.player1)
            return gameManager.player2;
        else if (aiPlayer == gameManager.player2)
            return gameManager.player1;
        else
            return null;
    }
}