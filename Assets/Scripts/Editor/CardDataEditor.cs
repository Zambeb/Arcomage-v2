using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardData))] 
public class CardDataEditor : Editor
{
    private CardData _targetCard;

    private void OnEnable()
    {
        _targetCard = (CardData)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if (GUI.changed)
        {
            UpdateAssetName();
            EditorUtility.SetDirty(_targetCard);
        }
    }

    private void UpdateAssetName()
    {
        if (_targetCard == null || string.IsNullOrEmpty(_targetCard.cardName))
        {
            return;
        }
        
        string assetPath = AssetDatabase.GetAssetPath(_targetCard);
        
        string folderPath = System.IO.Path.GetDirectoryName(assetPath);
        string[] existingAssets = AssetDatabase.FindAssets("t:CardData", new string[] { folderPath });
        
        int index = -1;
        for (int i = 0; i < existingAssets.Length; i++)
        {
            string currentAssetPath = AssetDatabase.GUIDToAssetPath(existingAssets[i]);
            if (currentAssetPath == assetPath)
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
    }
}