// Scripts/Cards/CardEffect.cs

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardEffect
{
    public CardEffectType effectType;
    public TargetType target;
    public int value;
    public ResourceType modifyResourceType;
    
    public bool hasCondition = false;
    public ConditionType condition; 
    public ResourceType resourceType;
    public int conditionValue; 
    public int alternativeValue;
}

