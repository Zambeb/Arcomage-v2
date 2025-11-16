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
    public int startingTower = 20;
    public int startingWall = 10;
    public int startingProduction = 2;
    public int startingResources = 3;
    
    [Header("Players")]
    public PlayerData player1;
    public PlayerData player2;
    
    [Header("Card Database")]
    public List<CardData> allCards = new List<CardData>();
    private List<CardData> sharedDeck = new List<CardData>();
    
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
        InitializeDeck();
        
        player1 = new PlayerData() { 
            playerName = "Player 1", 
            playerType = PlayerType.Human,
            tower = startingTower,
            wall = startingWall,
            quarry = startingProduction,
            magic = startingProduction,
            dungeon = startingProduction,
            bricks = startingResources,
            gems = startingResources,
            recruits = startingResources
        };
        player2 = new PlayerData() { 
            playerName = "AI Opponent", 
            playerType = PlayerType.AI,
            tower = startingTower,
            wall = startingWall,
            quarry = startingProduction,
            magic = startingProduction,
            dungeon = startingProduction,
            bricks = startingResources,
            gems = startingResources,
            recruits = startingResources
        };
        
        DrawInitialHand(player1);
        DrawInitialHand(player2);
        
        currentPlayer = player1;
        StartTurn(true);
        
        Debug.Log("Game initialized. Current player: " + currentPlayer.playerName);
    }
    
    private void InitializeDeck()
    {
        sharedDeck = new List<CardData>(allCards);
        ShuffleDeck();
        Debug.Log($"Shared deck initialized with {sharedDeck.Count} cards.");
    }
    
    private void ShuffleDeck()
    {
        System.Random rng = new System.Random();
        int n = sharedDeck.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            CardData value = sharedDeck[k];
            sharedDeck[k] = sharedDeck[n];
            sharedDeck[n] = value;
        }
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
        // Проверяем, есть ли карты в общем пуле
        if (sharedDeck.Count > 0)
        {
            // Берем случайную карту из пула
            int cardIndex = Random.Range(0, sharedDeck.Count);
            CardData randomCard = sharedDeck[cardIndex];
        
            // Удаляем карту из пула и добавляем ее в руку игрока
            sharedDeck.RemoveAt(cardIndex);
            player.hand.Add(randomCard);
        }
        else
        {
            // Этого не должно случиться, если карты всегда возвращаются,
            // но это хорошая проверка на случай, если пул опустеет.
            Debug.LogWarning("Shared deck is empty! No card drawn.");
        }
    }
    
    private void ReturnCardToDeck(CardData card)
    {
        if (card != null)
        {
            int randomPosition = Random.Range(0, sharedDeck.Count + 1);
            sharedDeck.Insert(randomPosition, card);
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
            RefillHandToSix(currentPlayer);
        }

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
        ReturnCardToDeck(card);
        
        UIManager.Instance.UpdateGameState();
        
        CheckWinConditions();
        
        if (!gameOver)
        {
            if (shouldForceDiscard)
            {
                player.forceDiscardNextCard = false;
                Debug.Log("Force discard completed, granting extra turn");
                StartCoroutine(CompleteCardActionAfterDelay(cardDisplay, 1f, false, false));
            }
            else if (card.extraTurn)
            {
                StartCoroutine(CompleteCardActionAfterDelay(cardDisplay, 1f, false, false));
            }
            else
            {
                StartCoroutine(CompleteCardActionAfterDelay(cardDisplay, 1f, true, true));
            }
        }
        else
        {
            cardDisplay.CompleteCardAction();
            isProcessingTurn = false;
        }
    }

    public void HandleAIActionBlocked(PlayerData aiPlayer)
    {
        Debug.Log($"AI turn logic restarting for {aiPlayer.playerName} after blocked action.");
    
        // Сбрасываем флаг, если он был установлен (хотя в DiscardCardAsAI он не устанавливался)
        isProcessingTurn = false; 
    
        // Перезапускаем логику хода AI с небольшой задержкой, 
        // чтобы избежать мгновенного зацикливания или проблем с отрисовкой UI.
        StartCoroutine(StartAIActionAfterDelay(aiPlayer, 0.5f));
    }
    
    private IEnumerator StartAIActionAfterDelay(PlayerData aiPlayer, float delay)
    {
        yield return new WaitForSeconds(delay);
    
        // Убеждаемся, что AI все еще активный игрок
        if (currentPlayer == aiPlayer)
        {
            // Запускаем логику AI снова
            if (aiController != null)
            {
                aiController.MakeAIMove(aiPlayer);
            }
            else
            {
                MakeRandomAIMove(aiPlayer);
            }
        }
    }
    
public void DiscardCardWithAnimation(CardData card, PlayerData player, CardDisplay cardDisplay)
{
    if (isProcessingTurn || gameOver) return;
    
    if (card.isUndiscardable) 
    {
        Debug.LogWarning($"{player.playerName} attempted to discard non-discardable card: {card.cardName}. Action blocked.");
        isProcessingTurn = false;
        return;
    }
    
    isProcessingTurn = true;
    
    Debug.Log($"{player.playerName} discards with animation: {card.cardName}");
    
    player.hand.Remove(card);
    ReturnCardToDeck(card);
    
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
    ReturnCardToDeck(card);
        
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
    
    if (card.isUndiscardable) 
    {
        Debug.LogWarning($"{player.playerName} attempted to discard non-discardable card: {card.cardName}. Action blocked.");
        return;
    }
        
    Debug.Log($"{player.playerName} discards: {card.cardName}");
        
    player.hand.Remove(card);
    ReturnCardToDeck(card);
        
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
            if (effect.effectType == CardEffectType.ApplyEffectToLowestWall)
            {
                ApplyLowestWallEffect(effect);
                continue; // Пропускаем стандартный ApplySingleEffect
            }
            
            if (effect.effectType == CardEffectType.SetProductionToMax)
            {
                SetProductionToMaxValue(effect);
                continue; 
            }
            
            if (effect.effectType == CardEffectType.ConditionalDamageTargetSwap)
            {
                ApplyConditionalDamageSwap(effect, player);
                continue;
            }
            
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
    
    private void ApplyConditionalDamageSwap(CardEffect effect, PlayerData selfPlayer)
    {
        PlayerData opponentPlayer = GetOpponent(selfPlayer);
    
        // Проверяем, что условие карты соответствует желаемому (If Tower > enemy Wall)
        if (effect.condition != ConditionType.SelfTowerGreaterThanOppnoentWall)
        {
            Debug.LogError("ConditionalDamageSwap used with wrong condition type. Must be SelfTowerGreaterThanOppnoentWall.");
            return;
        }
    
        // Проверяем условие
        bool conditionMet = (selfPlayer.tower > opponentPlayer.wall);
    
        int damage = effect.value; // Базовое значение (8)
    
        if (conditionMet)
        {
            // Условие выполнено: Damage to Enemy TOWER
            damage = effect.alternativeValue; // Используем альтернативное значение (например, 8)
            opponentPlayer.tower = Mathf.Max(0, opponentPlayer.tower - damage);
            Debug.Log($"Conditional Damage Met: {selfPlayer.playerName} dealt {damage} damage to opponent's TOWER.");
        }
        else
        {
            // Условие НЕ выполнено: Damage to Enemy WALL
            damage = effect.value; // Используем базовое значение (например, 8)
            opponentPlayer.wall = Mathf.Max(0, opponentPlayer.wall - damage);
            Debug.Log($"Conditional Damage Not Met: {selfPlayer.playerName} dealt {damage} damage to opponent's WALL.");
        }
    }
    private void SetProductionToMaxValue(CardEffect effect)
    {
        ResourceType targetType = effect.modifyResourceType;
        int prod1, prod2;

        // 1. Находим текущие значения производства
        switch (targetType)
        {
            case ResourceType.Bricks:
                prod1 = player1.quarry;
                prod2 = player2.quarry;
                break;
            case ResourceType.Gems:
                prod1 = player1.magic; // <--- Ваш целевой ресурс
                prod2 = player2.magic;
                break;
            case ResourceType.Recruits:
                prod1 = player1.dungeon;
                prod2 = player2.dungeon;
                break;
            default:
                Debug.LogError($"Invalid ResourceType {targetType} for SetProductionToMax.");
                return;
        }

        // 2. Определяем максимальное значение
        int maxValue = Mathf.Max(prod1, prod2);
    
        Debug.Log($"SetProductionToMax: Target={targetType}. Max Value Found: {maxValue}");

        // 3. Применяем максимальное значение обоим игрокам
        switch (targetType)
        {
            case ResourceType.Bricks:
                player1.quarry = maxValue;
                player2.quarry = maxValue;
                break;
            case ResourceType.Gems:
                player1.magic = maxValue;
                player2.magic = maxValue;
                break;
            case ResourceType.Recruits:
                player1.dungeon = maxValue;
                player2.dungeon = maxValue;
                break;
        }
    }
    
    private void ApplyLowestWallEffect(CardEffect effect)
    {
        int wall1 = player1.wall;
        int wall2 = player2.wall;
    
        List<PlayerData> lowestWallPlayers = new List<PlayerData>();
    
        if (wall1 < wall2)
        {
            // Player 1 имеет наименьшую стену
            lowestWallPlayers.Add(player1);
        }
        else if (wall2 < wall1)
        {
            // Player 2 имеет наименьшую стену
            lowestWallPlayers.Add(player2);
        }
        else
        {
            // Стены равны (Wall1 == Wall2)
            lowestWallPlayers.Add(player1);
            lowestWallPlayers.Add(player2);
        }

        // --- Применение Эффектов ---
    
        // Эффект 1: Modify Production (-1 Dungeon)
        int dungeonModification = effect.value; // -1
        ResourceType dungeonType = effect.modifyResourceType; // Recruits
    
        // Эффект 2: Damage Tower (2 Damage)
        int towerDamage = effect.alternativeValue; // 2

        foreach (PlayerData target in lowestWallPlayers)
        {
            Debug.Log($"Applying Lowest Wall effect to: {target.playerName}");
        
            // 1. Применяем Modify Production (-1 Dungeon/Recruits)
            ModifyProduction(target, dungeonType, dungeonModification);
        
            // 2. Применяем Damage Tower (2 Damage)
            target.tower = Mathf.Max(0, target.tower - towerDamage);
        }
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
    
    private bool CheckCardCondition(CardEffect effect, PlayerData selfPlayer, PlayerData opponentPlayer, PlayerData target)
    {
        // Note: Use 'target' only for conditions related to the effect's target (like TargetWallBelow).

        switch (effect.condition)
        {
            case ConditionType.TargetWallBelow:
                return target.wall < effect.conditionValue;

            case ConditionType.SelfProductionGreaterThanOpponent:
                var (selfG, opponentG) = GetProductionValues(selfPlayer, opponentPlayer, effect.resourceType);
                return selfG > opponentG;

            case ConditionType.SelfProductionLessThanOpponent:
                var (selfL, opponentL) = GetProductionValues(selfPlayer, opponentPlayer, effect.resourceType);
                return selfL < opponentL;

            case ConditionType.SelfTowerLowerThanOpponent:
                return selfPlayer.tower < opponentPlayer.tower;
        
            case ConditionType.SelfTowerGreaterThanOppnoentWall:
                return selfPlayer.tower > opponentPlayer.wall;
            case ConditionType.SelfWallGreaterThanOpponent:
                return selfPlayer.wall > opponentPlayer.wall;

            default:
                return false;
        }
    }

    private void ApplySingleEffect(CardEffect effect, PlayerData target)
    {
        PlayerData selfPlayer = GetCurrentPlayer();
        PlayerData opponentPlayer = GetOpponent(selfPlayer);
    
        int finalValue = effect.value;
        bool isSpecialSetEffect = effect.effectType == CardEffectType.SetProductionToOpponent;
    
        bool conditionMet = false;
        if (effect.hasCondition || isSpecialSetEffect)
        {
            conditionMet = CheckCardCondition(effect, selfPlayer, opponentPlayer, target);
        
            if (effect.hasCondition && !isSpecialSetEffect)
            {
                finalValue = conditionMet ? effect.alternativeValue : effect.value;
                Debug.Log($"Condition {(conditionMet ? "met" : "NOT met")}: using {(conditionMet ? "alternative" : "base")} value {finalValue}");
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
            case CardEffectType.SwapWall:
                (selfPlayer.wall, opponentPlayer.wall) = (opponentPlayer.wall, selfPlayer.wall);
                Debug.Log($"Wall Swap performed! {selfPlayer.playerName} Wall: {selfPlayer.wall}, {opponentPlayer.playerName} Wall: {opponentPlayer.wall}");
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
        const int MIN_PRODUCTION = 1;
        
        switch (type)
        {
            case ResourceType.Bricks:
                target.quarry = Mathf.Max(MIN_PRODUCTION, target.quarry + value);
                break;
            case ResourceType.Gems:
                target.magic = Mathf.Max(MIN_PRODUCTION, target.magic + value);
                break;
            case ResourceType.Recruits:
                target.dungeon = Mathf.Max(MIN_PRODUCTION, target.dungeon + value);
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