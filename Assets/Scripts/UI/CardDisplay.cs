// Scripts/UI/CardDisplay.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CardDisplay : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Elements")]
    public Image cardImage;
    public Image resourceIcon;
    public Text nameText;
    public Text costText;
    public Text descriptionText;
    
    [Header("Resource Icons")]
    public Sprite bricksIcon;
    public Sprite gemsIcon;
    public Sprite recruitsIcon;
    
    [Header("Animation Settings")]
    public float moveDuration = 0.5f;
    public float displayDuration = 0.5f;
    
    private CardData cardData;
    private PlayerData owner;
    private Button button;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private bool isAnimating = false;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Transform originalParent;
    private LayoutElement layoutElement;
    private Image backgroundImage;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        layoutElement = GetComponent<LayoutElement>();
        backgroundImage = GetComponent<Image>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        if (button != null)
        {
            button.onClick.AddListener(OnLeftClick);
        }
    }
    
    public void Initialize(CardData data, PlayerData player)
    {
        cardData = data;
        owner = player;
        
        if (cardImage != null && data.pixelArtSprite != null)
            cardImage.sprite = data.pixelArtSprite;
        
        if (nameText != null)
            nameText.text = data.cardName;
        
        if (costText != null)
            costText.text = data.resourceCost.ToString();
        
        if (descriptionText != null)
            descriptionText.text = data.description;
        
        if (resourceIcon != null)
        {
            resourceIcon.sprite = data.resourceType switch
            {
                ResourceType.Bricks => bricksIcon,
                ResourceType.Gems => gemsIcon,
                ResourceType.Recruits => recruitsIcon,
                _ => null
            };
        }
        
        originalScale = transform.localScale;
        UpdateInteractivity();
    }
    
    public void SetAsAICard(Color aiColor)
    {
        if (button != null)
        {
            button.interactable = false; 
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = aiColor;
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.8f;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (owner != null && owner.playerType == PlayerType.AI) return;
        
        if (isAnimating || owner == null || cardData == null) return;
        
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }
    
    private void OnLeftClick()
    {
        if (owner != null && owner.playerType == PlayerType.AI) return;
    
        if (isAnimating || owner == null || cardData == null) return;
    
        Debug.Log($"Card left clicked: {cardData.cardName}");
    
        if (ArcomagGameManager.Instance.IsCurrentPlayer(owner))
        {
            if (owner.forceDiscardNextCard)
            {
                Debug.Log($"FORCE DISCARD MODE: {cardData.cardName} will be discarded");
                StartCoroutine(AnimateCardAndPlay(false));
            }
            else if (owner.CanPlayCard(cardData))
            {
                Debug.Log($"Playing card normally: {cardData.cardName}");
                StartCoroutine(AnimateCardAndPlay(false));
            }
            else
            {
                Debug.Log("Not enough resources to play this card!");
            }
        }
        else
        {
            Debug.Log("Not your turn!");
        }
    }
    
    private void OnRightClick()
    {
        if (owner != null && owner.playerType == PlayerType.AI) return;
        
        if (isAnimating || owner == null || cardData == null) return;
        
        Debug.Log($"Card right clicked: {cardData.cardName}");
        
        if (ArcomagGameManager.Instance.IsCurrentPlayer(owner))
        {
            if (owner.forceDiscardNextCard)
            {
                Debug.Log("Cannot manually discard - waiting for mandatory card action.");
                return; 
            }
            
            if (cardData.isUndiscardable) 
            {
                Debug.LogWarning($"Cannot discard '{cardData.cardName}'. This card must be played.");
                return;
            }
            
            Debug.Log($"Discarding card: {cardData.cardName}");
            StartCoroutine(AnimateCardAndPlay(true));
        }
        else
        {
            Debug.Log("Not your turn!");
        }
    }
    
    public void PlayCardAsAI()
    {
        if (isAnimating || owner == null || cardData == null) return;
        
        Debug.Log($"AI playing card: {cardData.cardName}");
        StartCoroutine(AnimateCardAndPlay(false));
    }
    
    public void DiscardCardAsAI()
    {
        if (isAnimating || owner == null || cardData == null) return;
        
        if (cardData.isUndiscardable) 
        {
            Debug.LogWarning($"AI attempted to discard non-discardable card: {cardData.cardName}. Action blocked.");
            if (owner.playerType == PlayerType.AI && ArcomagGameManager.Instance.IsCurrentPlayer(owner))
            {
                // Вызываем новый метод, который перезапустит логику AI
                ArcomagGameManager.Instance.HandleAIActionBlocked(owner);
            }
            // -------------------------------------
            return;
        }
        
        Debug.Log($"AI discarding card: {cardData.cardName}");
        StartCoroutine(AnimateCardAndPlay(true));
    }
    
    private IEnumerator AnimateCardAndPlay(bool isDiscard)
    {
        isAnimating = true;
        
        bool isVisualDiscard = isDiscard || (owner.forceDiscardNextCard && ArcomagGameManager.Instance.IsCurrentPlayer(owner));
        
        if (button != null) button.interactable = false;
        
        originalPosition = rectTransform.position;
        originalParent = transform.parent;
        
        Debug.Log($"Original position: {originalPosition}");
        
        if (layoutElement != null)
        {
            layoutElement.ignoreLayout = true;
        }
        
        Transform canvasTransform = GetComponentInParent<Canvas>().transform;
        transform.SetParent(canvasTransform, true);
        transform.SetAsLastSibling(); 
        
        Canvas.ForceUpdateCanvases();
        
        rectTransform.position = originalPosition;
        
        Debug.Log($"Position after moving to canvas: {rectTransform.position}");
        
        Vector3 targetPosition = GetScreenCenter();
        
        Debug.Log($"Target position: {targetPosition}");
        
        float elapsedTime = 0f;
        
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            
            rectTransform.position = Vector3.Lerp(originalPosition, targetPosition, t);
            
            if (isVisualDiscard && canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0.5f, t);
            }
            
            transform.localScale = originalScale * Mathf.Lerp(1f, 1.2f, t);
            
            yield return null;
        }
        
        rectTransform.position = targetPosition;
        if (isVisualDiscard && canvasGroup != null)
        {
            canvasGroup.alpha = 0.5f;
        }
        
        Debug.Log("Animation completed, waiting before action...");
        Debug.Log("Executing card action...");
        
        if (isDiscard)
        {
            ArcomagGameManager.Instance.DiscardCardWithAnimation(cardData, owner, this);
        }
        else
        {
            ArcomagGameManager.Instance.PlayCardWithAnimation(cardData, owner, this);
        }
        
        yield return new WaitForSeconds(displayDuration);
        
        transform.localScale = originalScale;
    }
    
    private Vector3 GetScreenCenter()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return Vector3.zero;
        }
        
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect == null)
        {
            Debug.LogError("Canvas RectTransform not found!");
            return Vector3.zero;
        }
        
        Vector3 center = canvasRect.position;
        
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            center = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        }
        
        Debug.Log($"Screen center: {center}");
        return center;
    }
    
    public void CompleteCardAction()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
        isAnimating = false;
    }
    
    private void UpdateInteractivity()
    {
        if (button != null)
        {
            bool canInteract = owner != null && owner.playerType == PlayerType.Human;
            bool canPlay = owner != null && owner.CanPlayCard(cardData);
            button.interactable = canInteract && canPlay && !isAnimating;
            
            Image bg = GetComponent<Image>();
            if (bg != null && owner != null && owner.playerType == PlayerType.Human)
            {
                bg.color = canPlay ? Color.white : new Color(1, 1, 1, 0.5f);
            }
        }
    }
    
    
    
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnLeftClick);
        }
    }
    
    public CardData GetCardData()
    {
        return cardData;
    }

    public PlayerData GetOwner()
    {
        return owner;
    }
}