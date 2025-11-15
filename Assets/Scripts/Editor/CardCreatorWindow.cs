using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class CardCreatorWindow : EditorWindow
{
    private CardData newCard;
    private string savePath = "Assets/Cards/";
    private string newCardName = "New_Card";
    private Vector2 scrollPosition;
    private bool isEditing = false;
    private List<CardData> allCards = new List<CardData>();

    [MenuItem("Arcomag/Card Creator Window")]
    public static void ShowWindow()
    {
        GetWindow<CardCreatorWindow>("Card Creator");
    }

    private void OnEnable()
    {
        LoadAllCards();
    }

    private void LoadAllCards()
    {
        // Очищаем текущий список
        allCards.Clear();

        // Проверяем, существует ли папка сохранения
        if (AssetDatabase.IsValidFolder(savePath.TrimEnd('/')))
        {
            // Находим все ассеты типа CardData в папке сохранения
            string[] guids = AssetDatabase.FindAssets("t:CardData", new string[] { savePath.TrimEnd('/') });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);
                if (card != null)
                {
                    allCards.Add(card);
                }
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Arcomag Card Creator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (!isEditing)
        {
            DrawCreationSection();
            DrawCardList();
        }
        else
        {
            DrawEditorSection();
        }
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(this);
        }
    }
    
    private void DrawCreationSection()
    {
        GUILayout.Label("Save Path:", EditorStyles.miniLabel);
        savePath = EditorGUILayout.TextField(savePath);
        GUILayout.Space(5);
        
        GUILayout.Space(10);

        newCardName = EditorGUILayout.TextField("Card Name", newCardName);

        if (GUILayout.Button("1. Create New CardData Asset"))
        {
            CreateNewCardAsset();
        }
    
        GUILayout.Space(5);
        
        GUI.color = Color.yellow;
        if (GUILayout.Button("2. Update All Indices (Resort)"))
        {
            CardDataEditor.UpdateAllAssetsOrder(savePath.TrimEnd('/'));
            LoadAllCards(); 
            Repaint();
        }
        GUI.color = Color.white;

        GUILayout.Space(5);

        if (!AssetDatabase.IsValidFolder(savePath.TrimEnd('/')))
        {
            EditorGUILayout.HelpBox("The save path is invalid or does not exist.", MessageType.Warning);
            if (GUILayout.Button("Create Folder Path"))
            {
                string finalPath = savePath.TrimEnd('/');
                string root = "Assets";
                string[] parts = finalPath.Substring(root.Length + 1).Split('/');

                foreach (string part in parts)
                {
                    string currentPath = root + "/" + part;
                    if (!AssetDatabase.IsValidFolder(currentPath))
                    {
                        AssetDatabase.CreateFolder(root, part);
                    }
                    root = currentPath;
                }
                AssetDatabase.Refresh();
                LoadAllCards(); 
            }
        }
        GUILayout.Space(5);
    }
    
    private void DrawCardList()
    {
        GUILayout.Label("Existing Cards:", EditorStyles.largeLabel);
    
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
    
        if (allCards.Count == 0)
        {
            EditorGUILayout.HelpBox("No CardData assets found in the specified path.", MessageType.Info);
        }
        else
        {
            CardData cardToDelete = null;
        
            // Для нумерации (индекса)
            int index = 1;
            
            var sortedCards = allCards
                .OrderBy(c => c.resourceType)
                .ThenBy(c => c.resourceCost);

            foreach (CardData card in sortedCards)
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
            
                // --- Установка цвета и форматирование названия ---
                // Устанавливаем цвет текста
                GUI.contentColor = GetResourceColor(card.resourceType); 
            
                // Форматирование: [Индекс] Название Карты (Стоимость)
                string cardLabel = $"[{index}] {card.cardName} ({card.resourceCost})";

                // Отображение имени
                EditorGUILayout.LabelField(cardLabel, GUILayout.Width(position.width * 0.5f));
            
                // Восстанавливаем цвет для кнопок, чтобы они не были цветными
                GUI.contentColor = Color.white; 
            
                // Кнопка Edit
                if (GUILayout.Button("Edit", GUILayout.ExpandWidth(true)))
                {
                    newCard = card;
                    newCardName = card.cardName;
                    isEditing = true;
                }

                // Кнопка Delete
                GUI.color = Color.red; 
                if (GUILayout.Button("Delete", GUILayout.ExpandWidth(true)))
                {
                    cardToDelete = card;
                }
                GUI.color = Color.white; 

                EditorGUILayout.EndHorizontal();
            
                index++;
            }

            // Выполняем удаление после цикла foreach
            if (cardToDelete != null)
            {
                DeleteCardAsset(cardToDelete);
            }
        }
    
        EditorGUILayout.EndScrollView();
    }
    
    private Color GetResourceColor(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Bricks:
                return new Color(1f, 0.7f, 0.7f, 1f); // Светло-красный
            case ResourceType.Gems:
                return new Color(0.7f, 0.8f, 1f, 1f); // Светло-синий
            case ResourceType.Recruits:
                return new Color(0.7f, 1f, 0.7f, 1f); // Светло-зеленый
            default:
                return Color.white;
        }
    }
    
    private void DrawEditorSection()
    {
        if (newCard != null)
        {
            GUILayout.Label($"Editing: {newCard.cardName}", EditorStyles.largeLabel);

            // Создаем scroll view для секции редактора, чтобы можно было скроллить при редактировании
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Используем стандартный редактор
            Editor cardEditor = Editor.CreateEditor(newCard);
            cardEditor.OnInspectorGUI();
            
            EditorGUILayout.EndScrollView();
            
            GUILayout.Space(10);

            if (GUILayout.Button("Save and Return to List"))
            {
                AssetDatabase.SaveAssets();
                newCard = null;
                isEditing = false;
                LoadAllCards();
                Repaint();
            }
        }
        else
        {
            // Fallback, если newCard стал null во время редактирования
            isEditing = false;
        }
    }

    private void CreateNewCardAsset()
    {
        newCard = ScriptableObject.CreateInstance<CardData>();
        
        newCard.cardName = newCardName;
        newCard.description = $"A new card named {newCardName}.";
        
        string fullPath = $"{savePath}{newCardName}.asset";
        
        if (File.Exists(fullPath))
        {
            Debug.LogError($"Asset already exists at path: {fullPath}");
            return;
        }
        
        AssetDatabase.CreateAsset(newCard, fullPath);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Переходим в режим редактирования новой карты
        allCards.Add(newCard);
        isEditing = true;
        Repaint(); 

        // Очищаем имя для следующей карты
        newCardName = "New_Card";
    }
    
    private void DeleteCardAsset(CardData card)
    {
        // Безопасная проверка и удаление
        if (card == null) return;
    
        // Подтверждение действия пользователя
        bool confirm = EditorUtility.DisplayDialog(
            "Confirm Deletion",
            $"Are you sure you want to delete the card asset: {card.cardName}?",
            "Delete",
            "Cancel"
        );

        if (confirm)
        {
            string path = AssetDatabase.GetAssetPath(card);
        
            // Удаляем актив из базы данных
            AssetDatabase.DeleteAsset(path);
        
            // Удаляем из локального списка и обновляем отображение
            allCards.Remove(card);
        
            // Принудительно обновляем окна Unity
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Repaint(); 
            Debug.Log($"Card asset '{card.cardName}' deleted successfully.");
        }
    }
    
    private void OnSelectionChange()
    {
        // 1. Вход в режим редактирования (если не редактируем)
        if (!isEditing && Selection.activeObject is CardData card)
        {
            newCard = card;
            newCardName = card.cardName;
            isEditing = true;
            Repaint(); 
        }
        // 2. Обновление имени редактируемой карты (если уже редактируем и выбрали ее же)
        else if (isEditing && Selection.activeObject is CardData selectedCard && selectedCard == newCard)
        {
            // Используем 'selectedCard' вместо 'card'
            newCardName = selectedCard.cardName;
        }
        else if (Selection.activeObject == null || !(Selection.activeObject is GameObject || Selection.activeObject is CardData))
        {
            if (!isEditing)
            {
                LoadAllCards();
            }
        }
        Repaint(); 
    }
    
    private void OnFocus()
    {
        // Обновляем список, когда окно получает фокус, чтобы увидеть внешние изменения
        if (!isEditing)
        {
            LoadAllCards();
        }
    }
}