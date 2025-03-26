// Creates a wizard that lets you update or replace the game's save file.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SaveEditorWizard : ScriptableWizard
{
    private const string PrimaryButtonName = "Overwrite Save Data";
    private const string SecondaryButtonName = "Apply Added Assets";

    // Serialized Data
    [Tooltip("This is the actual data object that will be used to create the save file.")]
    public SaveDataV1 saveData = new();

    [Tooltip("Lists of assets to add to the save data by reference instead of copying string IDs manually")]
    public AssetRefList assetsToAdd = new();

    // Private helper fields
    private ISaveReadWriter _saveReadWriter = new JsonSaveReadWriter();
    private bool _displayJson = true;
    private string _saveJson;

    [MenuItem("Draggin/Save Editor Wizard")]
    private static void CreateWizard()
    {
        var wizard = DisplayWizard<SaveEditorWizard>("Edit Save Data", PrimaryButtonName, SecondaryButtonName);
        wizard.LoadSaveData();
        wizard.OnWizardUpdate();
    }

    protected override bool DrawWizardGUI()
    {
        bool dirty = false;
        if (GUILayout.Button("Reset To Defaults"))
        {
            ResetSaveData();
            dirty = true;
        }

        _displayJson = EditorGUILayout.Foldout(_displayJson, "Display Json");
        if (_displayJson)
        {
            // Wrap in disabled group to make text field readonly
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextArea(_saveJson);
            EditorGUI.EndDisabledGroup();

            // copying from a disabled text area is finicky. Provide alternative.
            if (EditorGUILayout.LinkButton("Copy Json to Clipboard"))
            {
                EditorGUIUtility.systemCopyBuffer = _saveJson;
            }
        }

        EditorGUILayout.HelpBox(
            new GUIContent(
                "Asset IDs for equipped cosmetics and challenge progress can be entered in two ways: " +
                $"\n - (Recommended) Adding the assets to the lists within the \"{nameof(assetsToAdd)}\" field."+
                $"\n - Manually adding the ID strings within the \"{nameof(saveData)}\" field." 
            )
        );

        // Draw serialized fields
        dirty |= base.DrawWizardGUI();
        return dirty;
    }

    // Built-in event function run when information serialized by the wizard is updated 
    private void OnWizardUpdate()
    {
        _saveJson = JsonUtility.ToJson(saveData, prettyPrint: true);
    }

    // Built-in event function run when the wizard's secondary button is pressed
    private void OnWizardOtherButton()
    {
        ApplyAddedAssets();
    }

    // Built-in event function run when the wizard's create button is pressed
    protected void OnWizardCreate()
    {
        // Apply outstanding assets to save file before closing window
        ApplyAddedAssets(); 

        // Write save data to save path
        const string path = GameManager.SaveDataPath;
        _saveReadWriter.SaveGameDataToFile(path, saveData);
        helpString = "Save Data overwritten!";
        Debug.Log($"Save Editor Wizard: {helpString}");
    }

    private void ResetSaveData()
    {
        saveData = new SaveDataV1();
    }

    /// <summary> Load existing save data from the game's save path </summary>
    private void LoadSaveData()
    {
        const string path = GameManager.SaveDataPath;
        bool foundSave = _saveReadWriter.TryLoadGameDataFromFile(path, out SaveDataV1 loadedData);
        if (foundSave)
        {
            saveData = loadedData;
            helpString = "Save data loaded successfully.";
        }
        else
        {
            helpString = "No existing save data found.";
        }

        Debug.Log($"Save Editor Wizard: {helpString}");
    }

    /// <summary> Add the data from the asset lists to the cached save data object </summary>
    private void ApplyAddedAssets()
    {
        List<string> newCosmeticList = new(saveData.equippedCosmeticIds);
        List<ChallengeProgressSaveData> newChallengeList = new(saveData.inProgressChallenges);

        // Add equipped cosmetic assets to save data if not already present 
        foreach (BaseCosmeticData item in assetsToAdd.equippedCosmeticsToAdd)
        {
            if (!item || newCosmeticList.Contains(item.uniqueName)) { continue; }

            newCosmeticList.Add(item.uniqueName);
        }
        assetsToAdd.equippedCosmeticsToAdd.Clear();


        // Add challenge progress to save data if not already present, otherwise set its progress appropriately
        foreach (SaveEditorChallengeProgress item in assetsToAdd.challengeProgressToAdd)
        {
            AddOrUpdateChallengeProgress(listToUpdate: newChallengeList, newData: item);
        }
        assetsToAdd.challengeProgressToAdd.Clear();

        saveData.equippedCosmeticIds = newCosmeticList.ToArray();
        saveData.inProgressChallenges = newChallengeList.ToArray();
        _saveJson = JsonUtility.ToJson(saveData, true);
    }

    /// <summary>
    /// Add challenge progress to supplied list if not already present,
    /// otherwise set the current entry's progress appropriately
    /// </summary>
    /// <param name="listToUpdate">List to add or update an entry within</param>
    /// <param name="newData">New data to add to the list</param>
    private static void AddOrUpdateChallengeProgress(List<ChallengeProgressSaveData> listToUpdate, SaveEditorChallengeProgress newData)
    {
        if (!newData.challenge) { return; }

        int newProgress = (newData.setCompleteInsteadOfProgress)
            ? newData.challenge.requiredProgress
            : newData.progress;

        ChallengeProgressSaveData existingProgress = listToUpdate.Find(x => x.id == newData.challenge.uniqueId);
        if (existingProgress != null)
        {
            existingProgress.progress = newProgress;
        }
        else
        {
            listToUpdate.Add(new ChallengeProgressSaveData
                {
                    id = newData.challenge.uniqueId,
                    progress = newProgress
                }
            );
        }
    }

    // Helper Classes for easy serialization.
    [System.Serializable] 
    public class AssetRefList
    {
        [Tooltip("Cosmetic data to add to the save file")]
        public List<BaseCosmeticData> equippedCosmeticsToAdd = new();
        
        [Tooltip("Challenge progress data to add to the save file")]
        public List<SaveEditorChallengeProgress> challengeProgressToAdd = new();
    }

    [System.Serializable] 
    public class SaveEditorChallengeProgress
    {
        [Tooltip("Challenge asset associated with this challenge")]
        public ChallengeData challenge;

        [Tooltip("Progress towards this challenge. Ignored if setCompleteInsteadOfProgress is true")]
        public int progress;

        [Tooltip("If true, progress will be ignored and the max progress for this challenge will be set instead")]
        public bool setCompleteInsteadOfProgress;
    }
}