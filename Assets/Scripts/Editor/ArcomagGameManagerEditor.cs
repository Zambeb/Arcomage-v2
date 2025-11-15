using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(ArcomagGameManager))]
public class ArcomagGameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Отрисовываем все стандартные поля GameManager
        DrawDefaultInspector();

        // Получаем ссылку на наш целевой скрипт
        ArcomagGameManager manager = (ArcomagGameManager)target;

        GUILayout.Space(10);
        
        // --- Добавляем кнопку ---
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("AUTO-LOAD ALL CardData Assets"))
        {
            LoadAllCardDataAssets(manager);
        }
        GUI.backgroundColor = Color.white;
        // -----------------------
    }

    private void LoadAllCardDataAssets(ArcomagGameManager manager)
    {
        // 1. Находим все GUID'ы объектов типа CardData во всём проекте
        string[] guids = AssetDatabase.FindAssets("t:CardData");
        
        // 2. Создаем временный список для хранения найденных карт
        List<CardData> foundCards = new List<CardData>();

        foreach (string guid in guids)
        {
            // Получаем путь к активу по GUID
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            // Загружаем актив по пути и пытаемся преобразовать его в CardData
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);
            
            if (card != null)
            {
                foundCards.Add(card);
            }
        }

        // 3. Заполняем список allCards менеджера
        manager.allCards = foundCards;
        
        // 4. Уведомляем Unity, что объект изменился (необходимо для сохранения изменений)
        EditorUtility.SetDirty(manager);
        
        Debug.Log($"Successfully loaded {foundCards.Count} CardData assets into GameManager.allCards.");
    }
}