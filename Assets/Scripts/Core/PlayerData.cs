// Scripts/Core/PlayerData.cs

using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PlayerData
{
    public string playerName;
    public PlayerType playerType;
    
    public int tower = 20;
    public int wall = 10;
    
    public int bricks = 5;
    public int gems = 5;
    public int recruits = 5;
    
    public int quarry = 2;
    public int magic = 2;
    public int dungeon = 2;
    
    public bool forceDiscardNextCard = false;
    public int extraActionsRemaining = 0;
    
    public List<CardData> hand = new List<CardData>();
    
    public void ProduceResources()
    {
        bricks += quarry;
        gems += magic;
        recruits += dungeon;
    }
    
    public bool CanPlayCard(CardData card)
    {
        return GetResourceAmount(card.resourceType) >= card.resourceCost;
    }
    
    public List<CardData> GetPlayableCards()
    {
        return hand.Where(card => CanPlayCard(card)).ToList();
    }
    
    private int GetResourceAmount(ResourceType type)
    {
        return type switch
        {
            ResourceType.Bricks => bricks,
            ResourceType.Gems => gems,
            ResourceType.Recruits => recruits,
            _ => 0
        };
    }
}