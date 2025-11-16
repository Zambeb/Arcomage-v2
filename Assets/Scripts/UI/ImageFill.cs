using UnityEngine;
using UnityEngine.UI;

public enum TargetPlayerID
{
    Player1 = 1,
    Player2 = 2
}
public class ImageFill : MonoBehaviour
{
    public Image image;
    public TargetPlayerID playerID;
    public TargetStat statType;
    
    private ArcomagGameManager gameManager; 
    private PlayerData targetPlayer;
    private float cachedMaxValue;

    private void Awake()
    {
        
        if (image == null)
        {
            image = GetComponent<Image>();
        }
    }

    private void Start()
    {
        gameManager = ArcomagGameManager.Instance;
        
        if (gameManager == null) return;

        cachedMaxValue = gameManager.winTowerHeight; 
        if (statType == TargetStat.Wall && cachedMaxValue < 1) 
        {
            cachedMaxValue = 50; 
        }
        
        UpdateFill(); 
    }
    
    public void UpdateFill()
    {
        if (gameManager == null)
        {
            gameManager = ArcomagGameManager.Instance;
            if (gameManager == null) return; 
        }

        if (playerID == TargetPlayerID.Player1)
        {
            targetPlayer = gameManager.player1;
        }
        else if (playerID == TargetPlayerID.Player2)
        {
            targetPlayer = gameManager.player2;
        }
        // ------------------------------------

        if (targetPlayer == null || image == null || cachedMaxValue <= 0)
        {
            image.fillAmount = 0;
            return;
        }

        float currentValue = 0;
        
        switch (statType)
        {
            case TargetStat.Tower:
                currentValue = targetPlayer.tower;
                break;
            
            case TargetStat.Wall:
                currentValue = targetPlayer.wall;
                break;
        }

        image.fillAmount = Mathf.Clamp01(currentValue / cachedMaxValue);
    }
}
