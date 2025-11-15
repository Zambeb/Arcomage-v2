// Scripts/Cards/CardData.cs

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Arcomag/Card")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName;
    [TextArea(3, 10)]
    public string description;
    public Sprite pixelArtSprite;
    
    [Header("Cost")]
    public ResourceType resourceType;
    public int resourceCost;
    
    [Header("Effects")]
    public List<CardEffect> effects = new List<CardEffect>();
    
    [Header("Game Flow")]
    public bool extraTurn = false;
    public bool discardInsteadOfPlay = false;
    public bool isUndiscardable = false;
}