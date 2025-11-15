// Resources/Enums/GameEnums.cs
public enum ResourceType
{
    Bricks,
    Gems,
    Recruits
}

public enum TargetStat
{
    Tower,
    Wall
}

public enum TargetType
{
    Self,
    Opponent,
    Both
}

public enum CardEffectType
{
    DamageWall,
    DamageTower,
    DamageBoth,
    ModifyResource,
    ModifyProduction,
    SetProductionToOpponent,
    BuildWall,
    BuildTower,
    DrawCard,
    DiscardCard,
    ForceDiscardNextCard, 
    RemoveForceDiscard,
    ApplyEffectToLowestWall,
    SwapWall,
    SetProductionToMax,
    ConditionalDamageTargetSwap
}

public enum PlayerType
{
    Human,
    AI
}

public enum ConditionType
{
    None,
    TargetWallBelow, 
    SelfProductionGreaterThanOpponent,
    SelfProductionLessThanOpponent,
    SelfTowerLowerThanOpponent,
    SelfTowerGreaterThanOppnoentWall
}