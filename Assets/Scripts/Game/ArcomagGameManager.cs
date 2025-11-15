// Scripts/Game/ArcomagGameManager.cs

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArcomagGameManager : MonoBehaviour
{
    public static ArcomagGameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int winTowerHeight = 50;
    public int winResourceAmount = 100;
    public int startingHandSize = 6;
    
    [Header("Players")]
    public PlayerData player1;
    public PlayerData player2;
    
    [Header("Card Database")]
    public List<CardData> allCards = new List<CardData>();
    
    [Header("AI Settings")]
    public SimpleArcomagAI aiController;
    
    private PlayerData currentPlayer;
    private bool gameOver = false;
    
    private bool isProcessingTurn = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("GameManager Instance set");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (aiController == null)
        {
            aiController = FindObjectOfType<SimpleArcomagAI>();
            if (aiController == null)
            {
                Debug.LogWarning("AI Controller not assigned, creating one...");
                GameObject aiObject = new GameObject("AIController");
                aiController = aiObject.AddComponent<SimpleArcomagAI>();
            }
        }
    }
    
    private void Start()
    {
        Debug.Log("GameManager Start called");
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        player1 = new PlayerData() { 
            playerName = "Player 1", 
            playerType = PlayerType.Human 
        };
        player2 = new PlayerData() { 
            playerName = "AI Opponent", 
            playerType = PlayerType.AI 
        };
        
        DrawInitialHand(player1);
        DrawInitialHand(player2);
        
        currentPlayer = player1;
        StartTurn(true);
        
        Debug.Log("Game initialized. Current player: " + currentPlayer.playerName);
    }
    
    private PlayerData GetOpponent(PlayerData player)
    {
        return (player == player1) ? player2 : player1;
    }
    
    private (int selfProd, int oppProd) GetProductionValues(PlayerData selfPlayer, PlayerData opponentPlayer, ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Bricks:
                return (selfPlayer.quarry, opponentPlayer.quarry);
            case ResourceType.Gems:
                return (selfPlayer.magic, opponentPlayer.magic);
            case ResourceType.Recruits:
                return (selfPlayer.dungeon, opponentPlayer.dungeon);
            default:
                return (0, 0);
        }
    }
    
    private void DrawInitialHand(PlayerData player)
    {
        player.hand.Clear();
        for (int i = 0; i < startingHandSize; i++)
        {
            DrawCard(player);
        }
    }
    
    public void DrawCard(PlayerData player)
    {
        if (allCards.Count > 0)
        {
            CardData randomCard = allCards[Random.Range(0, allCards.Count)];
            player.hand.Add(randomCard);
        }
    }
    
    private void StartTurn(bool produceResources = true)
    {
        if (currentPlayer == null)
        {
            return;
        }
        
        Debug.Log($"StartTurn: {currentPlayer.playerName}'s turn (produceResources: {produceResources})");
        
        if (produceResources)
        {
            currentPlayer.ProduceResources();
        }
        
        RefillHandToSix(currentPlayer);
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateGameState();
        }
        else
        {
            Debug.LogWarning("UIManager Instance is null!");
        }
        
        if (currentPlayer.playerType == PlayerType.AI)
        {
            Debug.Log("AI turn started");
            if (aiController != null)
            {
                aiController.MakeAIMove(currentPlayer);
            }
            else
            {
                Debug.LogError("AI Controller is null!");
                MakeRandomAIMove(currentPlayer);
            }
        }
        else
        {
            Debug.Log("Human turn started");
        }
    }
    
    private void RefillHandToSix(PlayerData player)
    {
        if (player.hand.Count < startingHandSize)
        {
            int cardsToDraw = startingHandSize - player.hand.Count;
            Debug.Log($"Refilling hand from {player.hand.Count} to {startingHandSize} cards. Drawing {cardsToDraw} cards.");
            
            for (int i = 0; i < cardsToDraw; i++)
            {
                DrawCard(player);
            }
        }
        else
        {
            Debug.Log($"Hand already has {player.hand.Count} cards, no need to refill.");
        }
    }
    
    private void MakeRandomAIMove(PlayerData aiPlayer)
    {
        var playableCards = aiPlayer.GetPlayableCards();
        
        if (playableCards.Count > 0)
        {
            CardData randomCard = playableCards[Random.Range(0, playableCards.Count)];
            PlayCard(randomCard, aiPlayer);
        }
        else if (aiPlayer.hand.Count > 0)
        {
            CardData cardToDiscard = aiPlayer.hand[Random.Range(0, aiPlayer.hand.Count)];
            DiscardCard(cardToDiscard, aiPlayer);
        }
    }
    
    public void PlayCardWithAnimation(CardData card, PlayerData player, CardDisplay cardDisplay)
    {
        if (isProcessingTurn || !player.CanPlayCard(card) || gameOver) return;
        
        isProcessingTurn = true;
        
        bool shouldForceDiscard = player.forceDiscardNextCard;
        
        Debug.Log($"{player.playerName} plays with animation: {card.cardName} (forceDiscard: {shouldForceDiscard})");
        
        if (!shouldForceDiscard)
        {
            PayCardCost(card, player);
            ApplyCardEffects(card, player);
        }
        else
        {
            Debug.Log("Force discard - skipping cost and effects");
        }
        
        player.hand.Remove(card);
        
        UIManager.Instance.UpdateGameState();
        
        CheckWinConditions();
        
        if (!gameOver)
        {
            if (shouldForceDiscard)
            {
                player.forceDiscardNextCard = false;
                Debug.Log("Force discard completed, granting extra turn");
                StartCoroutine(CompleteCardActionAfterDelay(cardDisplay, 0.1f, false, false));
            }
            else if (card.extraTurn)
            {
                StartCoroutine(CompleteCardActionAfterDelay(cardDisplay, 0.1f, false, false));
            }
            else
            {
                StartCoroutine(CompleteCardActionAfterDelay(cardDisplay, 0.1f, true, true));
            }
        }
        else
        {
            cardDisplay.CompleteCardAction();
            isProcessingTurn = false;
        }
    }

public void DiscardCardWithAnimation(CardData card, PlayerData player, CardDisplay cardDisplay)
{
    if (isProcessingTurn || gameOver) return;
    
    isProcessingTurn = true;
    
    Debug.Log($"{player.playerName} discards with animation: {card.cardName}");
    
    player.hand.Remove(card);
    
    UIManager.Instance.UpdateGameState();
    
    if (!gameOver)
    {
        StartCoroutine(CompleteCardActionAfterDelay(cardDisplay, 0.1f, true, true));
    }
    else
    {
        cardDisplay.CompleteCardAction();
        isProcessingTurn = false;
    }
}

private IEnumerator CompleteCardActionAfterDelay(CardDisplay cardDisplay, float delay, bool switchTurn, bool produceResources)
{
    yield return new WaitForSeconds(delay);
    
    if (cardDisplay != null)
    {
        cardDisplay.CompleteCardAction();
    }
    
    UIManager.Instance.UpdateGameState();
    
    if (switchTurn && !gameOver)
    {
        currentPlayer = (currentPlayer == player1) ? player2 : player1;
    }
    
    isProcessingTurn = false;
    
    if (!gameOver)
    {
        StartTurn(produceResources);
    }
}

public void PlayCard(CardData card, PlayerData player)
{
    if (!player.CanPlayCard(card) || gameOver) return;
        
    Debug.Log($"{player.playerName} plays: {card.cardName}");
    
    PayCardCost(card, player);
    ApplyCardEffects(card, player);
    player.hand.Remove(card);
        
    Debug.Log($"Card played. Hand count: {player.hand.Count}");
    
    UIManager.Instance.UpdateGameState();
    CheckWinConditions();
        
    if (!gameOver)
    {
        if (!card.extraTurn)
        {
            currentPlayer = (currentPlayer == player1) ? player2 : player1;
            StartTurn(true); 
        }
        else
        {
            StartTurn(false);
        }
    }
}

public void DiscardCard(CardData card, PlayerData player)
{
    if (gameOver) return;
        
    Debug.Log($"{player.playerName} discards: {card.cardName}");
        
    player.hand.Remove(card);
        
    Debug.Log($"Card discarded. Hand count: {player.hand.Count}");
    
    UIManager.Instance.UpdateGameState();
        
    if (!gameOver)
    {
        currentPlayer = (currentPlayer == player1) ? player2 : player1;
        StartTurn(true);
    }
}
    
    private IEnumerator SwitchTurnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
    
        currentPlayer = (currentPlayer == player1) ? player2 : player1;
        isProcessingTurn = false;
        StartTurn(true);
    }

    private IEnumerator ResetTurnProcessingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isProcessingTurn = false;
    }
    
    private IEnumerator StartTurnAfterDelay(float delay, bool produceResources = true)
    {
        yield return new WaitForSeconds(delay);
        StartTurn(produceResources);
    }
    
    private void PayCardCost(CardData card, PlayerData player)
    {
        switch (card.resourceType)
        {
            case ResourceType.Bricks:
                player.bricks -= card.resourceCost;
                break;
            case ResourceType.Gems:
                player.gems -= card.resourceCost;
                break;
            case ResourceType.Recruits:
                player.recruits -= card.resourceCost;
                break;
        }
    }
    
    private void ApplyCardEffects(CardData card, PlayerData player)
    {
        PlayerData targetPlayer;
        PlayerData opponent = (player == player1) ? player2 : player1;
        
        foreach (var effect in card.effects)
        {
            switch (effect.target)
            {
                case TargetType.Self:
                    targetPlayer = player;
                    break;
                case TargetType.Opponent:
                    targetPlayer = opponent;
                    break;
                case TargetType.Both:
                    ApplySingleEffect(effect, player);
                    ApplySingleEffect(effect, opponent);
                    continue;
                default:
                    targetPlayer = player;
                    break;
            }
            
            ApplySingleEffect(effect, targetPlayer);
        }
        ProcessSpecialCardLogic(card, player);
    }
    
    private void ProcessSpecialCardLogic(CardData card, PlayerData player)
    {
        bool hasDrawEffect = card.effects.Any(e => e.effectType == CardEffectType.DrawCard);
        bool hasForceDiscardEffect = card.effects.Any(e => e.effectType == CardEffectType.ForceDiscardNextCard);
        
        if (hasDrawEffect && hasForceDiscardEffect)
        {
            player.forceDiscardNextCard = true;
        }
        
        if (card.extraTurn)
        {
            Debug.Log($"Card {card.cardName} grants extra turn");
        }
    }
    
    // Scripts/Game/ArcomagGameManager.cs

    private void ApplySingleEffect(CardEffect effect, PlayerData target)
    {
        PlayerData selfPlayer = GetCurrentPlayer();
        PlayerData opponentPlayer = GetOpponent(selfPlayer);

        bool conditionMet = false; 
        int finalValue = effect.value;
        
        bool isSpecialSetEffect = effect.effectType == CardEffectType.SetProductionToOpponent;

        if (effect.hasCondition || isSpecialSetEffect)
        {
            if (effect.condition == ConditionType.TargetWallBelow)
            {
                if (target.wall < effect.conditionValue)
                {
                    conditionMet = true;
                }
            }
            else if (effect.condition == ConditionType.SelfProductionGreaterThanOpponent)
            {
                var (self, opponent) = GetProductionValues(selfPlayer, opponentPlayer, effect.resourceType);
                if (self > opponent)
                {
                    conditionMet = true;
                }
            }
            else if (effect.condition == ConditionType.SelfProductionLessThanOpponent)
            {
                var (self, opponent) = GetProductionValues(selfPlayer, opponentPlayer, effect.resourceType);
                if (self < opponent)
                {
                    conditionMet = true;
                }
            }
            
            if (effect.hasCondition && !isSpecialSetEffect)
            {
                if (conditionMet)
                {
                    finalValue = effect.alternativeValue;
                    Debug.Log($"Condition met: using alternative value {finalValue}");
                }
                else
                {
                    finalValue = effect.value;
                    Debug.Log($"Condition NOT met: using base value {finalValue}");
                }
            }
        }
        
        switch (effect.effectType)
        {
            case CardEffectType.DamageWall:
                target.wall = Mathf.Max(0, target.wall - finalValue);
                break;
            case CardEffectType.DamageTower:
                target.tower = Mathf.Max(0, target.tower - finalValue);
                break;
            case CardEffectType.DamageBoth:
                ApplyDamageToBoth(target, finalValue);
                break;
            case CardEffectType.ModifyResource:
                ModifyResource(target, effect.modifyResourceType, finalValue);
                break;
            case CardEffectType.ModifyProduction:
                ModifyProduction(target, effect.modifyResourceType, finalValue);
                break;
            case CardEffectType.BuildWall:
                target.wall += finalValue;
                break;
            case CardEffectType.BuildTower:
                target.tower += finalValue;
                break;
            case CardEffectType.DrawCard:
                DrawCard(target);
                break;
            case CardEffectType.DiscardCard:
                break;
            case CardEffectType.ForceDiscardNextCard:
                target.forceDiscardNextCard = true;
                break;
            case CardEffectType.RemoveForceDiscard:
                target.forceDiscardNextCard = false;
                break;
            case CardEffectType.SetProductionToOpponent:
                if (conditionMet)
                {
                    PlayerData opponent = GetOpponent(selfPlayer); 
                    
                    switch (effect.modifyResourceType)
                    {
                        case ResourceType.Bricks:
                            selfPlayer.quarry = opponent.quarry;
                            Debug.Log($"SetProductionToOpponent: Quarry set to {opponent.quarry}");
                            break;
                        case ResourceType.Gems:
                            selfPlayer.magic = opponent.magic;
                            Debug.Log($"SetProductionToOpponent: Magic set to {opponent.magic}");
                            break;
                        case ResourceType.Recruits:
                            selfPlayer.dungeon = opponent.dungeon;
                            Debug.Log($"SetProductionToOpponent: Dungeon set to {opponent.dungeon}");
                            break;
                    }
                }
                break;
        }
    }
    
    private void ApplyDamageToBoth(PlayerData target, int damage)
    {
        int remainingDamage = damage;
        
        if (target.wall > 0)
        {
            int wallDamage = Mathf.Min(target.wall, remainingDamage);
            target.wall -= wallDamage;
            remainingDamage -= wallDamage;
        }
        
        if (remainingDamage > 0)
        {
            target.tower = Mathf.Max(0, target.tower - remainingDamage);
        }
    }
    
    private void ModifyResource(PlayerData target, ResourceType type, int value)
    {
        switch (type)
        {
            case ResourceType.Bricks:
                target.bricks = Mathf.Max(0, target.bricks + value);
                break;
            case ResourceType.Gems:
                target.gems = Mathf.Max(0, target.gems + value);
                break;
            case ResourceType.Recruits:
                target.recruits = Mathf.Max(0, target.recruits + value);
                break;
        }
    }
    
    private void ModifyProduction(PlayerData target, ResourceType type, int value)
    {
        switch (type)
        {
            case ResourceType.Bricks:
                target.quarry = Mathf.Max(0, target.quarry + value);
                break;
            case ResourceType.Gems:
                target.magic = Mathf.Max(0, target.magic + value);
                break;
            case ResourceType.Recruits:
                target.dungeon = Mathf.Max(0, target.dungeon + value);
                break;
        }
    }
    
    private void CheckWinConditions()
    {
        if (player1.tower >= winTowerHeight)
        {
            EndGame("Player 1 wins by building a tall tower!");
            return;
        }
        if (player2.tower >= winTowerHeight)
        {
            EndGame("Player 2 wins by building a tall tower!");
            return;
        }
        
        if (player1.bricks >= winResourceAmount || player1.gems >= winResourceAmount || player1.recruits >= winResourceAmount)
        {
            EndGame("Player 1 wins by resource accumulation!");
            return;
        }
        if (player2.bricks >= winResourceAmount || player2.gems >= winResourceAmount || player2.recruits >= winResourceAmount)
        {
            EndGame("Player 2 wins by resource accumulation!");
            return;
        }
        
        if (player2.tower <= 0)
        {
            EndGame("Player 1 wins by destroying opponent's tower!");
            return;
        }
        if (player1.tower <= 0)
        {
            EndGame("Player 2 wins by destroying opponent's tower!");
            return;
        }
    }
    
    private void EndGame(string message)
    {
        gameOver = true;
        UIManager.Instance.ShowGameOver(message);
    }
    
    public bool IsCurrentPlayer(PlayerData player)
    {
        return currentPlayer == player;
    }
    public PlayerData GetCurrentPlayer()
    {
        return currentPlayer;
    }
}