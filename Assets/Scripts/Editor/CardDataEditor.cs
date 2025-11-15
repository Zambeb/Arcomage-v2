using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardData))] 
public class CardDataEditor : Editor
{
    private CardData _targetCard;

    private void OnEnable()
    {
        _targetCard = target as CardData;
    }

    public override void OnInspectorGUI()
    {
        if (_targetCard == null)
        {
            base.OnInspectorGUI();
            return;
        }
    
        base.OnInspectorGUI();
    
        if (GUI.changed)
        {
            UpdateAssetName();
            EditorUtility.SetDirty(_targetCard);
        }
    }
    
    private void UpdateTargetAssetName()
    {
        if (string.IsNullOrEmpty(_targetCard.cardName)) return;

        string assetPath = AssetDatabase.GetAssetPath(_targetCard);
        string folderPath = System.IO.Path.GetDirectoryName(assetPath);
        
        UpdateAllAssetsOrder(folderPath); 
    }
    
    public static void UpdateAllAssetsOrder(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder(folderPath)) return;
        
        string[] guids = AssetDatabase.FindAssets("t:CardData", new string[] { folderPath });
        var allCards = new List<CardData>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (card != null)
            {
                allCards.Add(card);
            }
        }
        
        var sortedCards = allCards
            .OrderBy(c => c.resourceType) 
            .ThenBy(c => c.resourceCost) 
            .ToList();
        
        for (int i = 0; i < sortedCards.Count; i++)
        {
            CardData cardToRename = sortedCards[i];
            string path = AssetDatabase.GetAssetPath(cardToRename);
            
            string newName = $"[{i + 1:00}] {cardToRename.cardName}";
            
            if (cardToRename.name != newName)
            {
                AssetDatabase.RenameAsset(path, newName);
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void UpdateAssetName()
    {
        if (_targetCard == null || string.IsNullOrEmpty(_targetCard.cardName))
        {
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(_targetCard);
        
        string folderPath = System.IO.Path.GetDirectoryName(assetPath);
        string[] guids = AssetDatabase.FindAssets("t:CardData", new string[] { folderPath });
        
        var allCards = new List<CardData>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (card != null)
            {
                allCards.Add(card);
            }
        }
        
        var sortedCards = allCards
            .OrderBy(c => c.resourceType) 
            .ThenBy(c => c.resourceCost) 
            .ToList();

        int index = -1;
        for (int i = 0; i < sortedCards.Count; i++)
        {
            if (sortedCards[i] == _targetCard)
            {
                index = i + 1; 
                break;
            }
        }
        
        string newName = (index > 0) 
            ? $"[{index:00}] {_targetCard.cardName}" 
            : _targetCard.cardName;
        
        if (_targetCard.name != newName)
        {
            AssetDatabase.RenameAsset(assetPath, newName);
        }
        
        for (int i = index; i < sortedCards.Count; i++)
        {
            CardData cardToRename = sortedCards[i];
            string path = AssetDatabase.GetAssetPath(cardToRename);
            string name = $"[{i + 1:00}] {cardToRename.cardName}";
            
            if (cardToRename.name != name)
            {
                AssetDatabase.RenameAsset(path, name);
            }
        }
    }
}