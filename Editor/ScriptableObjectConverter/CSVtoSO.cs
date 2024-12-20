/*
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

public class CSVtoSO
{
    public Dictionary<string, string> AddedIDs { get; set; }
    private char lineSeparator = '\n';
    private char surround = '"';
    Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
    public bool Success { get; private set; }

    /*
     * A generic function that creates scriptable objects based on the type T that is passed through
     * to the class
     #1#
    public void GenerateScriptableObjects<T>(TextAsset csvFile, bool? skipPopups = false) where T : ScriptableObject
    {
        string[] lines = csvFile.text.Split(lineSeparator);
        AddedIDs = new Dictionary<string, string>();
        int counter = 0, successCounter = 0;
        //string[] allLines = File.ReadAllLines(Application.dataPath + csvPath);
        
        //TODO: This section needs to be removed from this class
        Resources.LoadAll<DialogueSO>($"{Application.dataPath}\\Resources\\Dialogues");
        DialogueSO[] dialogues = (DialogueSO[])Resources.FindObjectsOfTypeAll(typeof(DialogueSO));
        foreach (var dialogue in dialogues)
        {
            if (AddedIDs.ContainsKey(dialogue.id))
            {
                continue;
            }
            AddedIDs.Add(dialogue.id, dialogue.id);
        }
        
        foreach (string s in lines)
        {
            if ((bool)!skipPopups)
            {
                counter++;
                EditorUtility.DisplayProgressBar($"Creating {typeof(T)}",
                    $"Loading {typeof(T)} {counter} / {lines.Length - 1}",
                    counter / (lines.Length - 1));
            }
            
            //Assumes there is a header line, skips over this for data
            if (s.Equals(lines[0]))
            {
                counter--;
                continue;
            }
            
            string[] splitData = CSVParser.Split(s);

            for (int f = 0; f < splitData.Length; f++)
            {
                splitData[f] = splitData[f].TrimStart(' ', surround);
                splitData[f] = splitData[f].TrimEnd(surround);
            }

            //Checks if the data is blank
            if (splitData.Length == 1)
            {
                counter--;
                break;
            }
            
            var scriptableObject = ScriptableObject.CreateInstance<T>();

            switch (scriptableObject)
            {
                case DialogueSO:
                    Success = CreateDialogueSO(scriptableObject as DialogueSO, splitData);
                    break;
            }

            if (Success)
            {
                successCounter++;
            }
        }

        AssetDatabase.SaveAssets();
        if (skipPopups != null && (bool)!skipPopups)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Creating Dialogues", $"Created {successCounter} of {counter} {typeof(T)}",
                "OK", "");
        }
    }

    /*
     * Creates a DialogueSO based on the line passed through, and stores in the passed through scriptableObject.
     * This method is specific to this type of scriptableObject
     #1#
    private bool CreateDialogueSO(DialogueSO scriptableObject, string[] splitData)
    { 
        string folder = "Assets/Resources/Dialogues";
        if (splitData.Length <= 1)
        {
            Debug.LogError($"No data was found");
            return false;
        }
                
        scriptableObject.id = GetGuid(splitData[0]);
        scriptableObject.speaker = splitData[1];
        scriptableObject.scene = splitData[2];
        scriptableObject.singleUse = CheckForBool(splitData[3]);
        scriptableObject.hasChoices = CheckForBool(splitData[4]);
        scriptableObject.choices = new SerializableDictionary<string, string>();

        /* Splits the choices cells#1#
        if (scriptableObject.hasChoices)
        {
            for (int i = 0; i < 8; i += 2)
            {
                string id = splitData[i + 6];
                string text = splitData[i + 5];
                if (!id.Equals("") || !text.Equals(""))
                {
                    id = GetGuid(id);
                    scriptableObject.choices.Add(id, text);
                }
            }
        }

        /* Splits the dialogue cells#1#
        scriptableObject.dialogue = new List<string>();
        for (int i = 13; i < splitData.Length; i++)
        {
            if (!splitData[i].Equals(""))// || !splitData[i].Equals("\"\r\""))
            {
                scriptableObject.dialogue.Add(splitData[i]);
            }
            else
            {
                i = splitData.Length;
            }
        }

        var folderPath = $"{folder}/{scriptableObject.speaker}/{scriptableObject.scene}";
        if(!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        AssetDatabase.CreateAsset(scriptableObject, $"{folderPath}/{scriptableObject.id}.asset");
        return true;
    }
    
    /*Creates a GUID for a Scriptable Object#1#
    public string GetGuid(string splitData)
    {
        if (AddedIDs.ContainsKey(splitData))
        {
            foreach (var id in AddedIDs)
            {
                if (id.Key.Equals(splitData))
                {
                    splitData = id.Value;
                    break;
                }
            }
        }
        //Checks that the ID number both does not exist, and is a valid number
        else if (splitData == "" || splitData.Length != 32)
        {
            string newID = Guid.NewGuid().ToString("N");
            AddedIDs.Add(splitData, newID);
            splitData = newID;
        }
        return splitData;
    }

    /*Logic for converting a string into a boolean#1#
    public bool CheckForBool(string inputString)
    {
        if (inputString.ToLower() == "true")
        {
            return true;
        }
        return false;
    }
}
*/
