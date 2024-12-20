/*
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DialogueSystem;
using UnityEditor;
using UnityEngine;

public class SOtoCSV
{
    /*
     * Gets all DialogueSO objects and converts data to CSV format
     #1#
    public void CSVtoDialogueLines(string csvPath, bool? skipPopups = false)
    {
        int counter = 0;
        List<string> lines = new List<string>();
        List<string> ids = new List<string>();
        var dialogues = GetAllDialogueSOs();
        int total = dialogues.Length;
        foreach (DialogueSO SOName in dialogues)
        {
            if ((bool)!skipPopups)
            {
                EditorUtility.DisplayProgressBar($"Saving Dialogues", $"Saving Dialogue {counter} / {dialogues.Length}",
                    counter / dialogues.Length);
            }

            if (!ids.Contains(SOName.id))
            {
                counter++;
                string line = $"{SOName.id},{SOName.speaker},{SOName.scene},{SOName.singleUse},{SOName.hasChoices}";
                
                //Check if there are choices available
                if (SOName.hasChoices)
                {
                    //Fill choices first
                    foreach (var pair in SOName.choices)
                    {
                        line += $",{pair.Value},{pair.Key}";
                    }

                    //Fill remaining choice options with blanks
                    for (int i = 0; i < (4 - SOName.choices.Count); i++)
                    {
                        line += $",{String.Empty},{String.Empty}";
                    }
                }
                //Otherwise fill with empty spaces
                else
                {
                    line += $",{String.Empty},{String.Empty},{String.Empty},{String.Empty},{String.Empty},{String.Empty},{String.Empty},{String.Empty}";
                }
                
                for (int i = 0; i <= SOName.dialogue.Count - 1; i++)
                {
                    line = line + $",\"{SOName.dialogue[i]}\"";
                }

                lines.Add(line);
                ids.Add(SOName.id);
            }
            else
            {
                total--;
            }
        }

        SaveToFile(csvPath, lines, "ID,Speaker,Scene,Single_Use,Choices,Choice_Text_1,Choice_ID_1,Choice_Text_2,Choice_ID_2,Choice_Text_3,Choice_ID_3,Choice_Text_4,Choice_ID_4, Dialogue");
        if ((bool)!skipPopups)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Creating Dialogues", $"Saved {counter} of {total} dialogues",
                "OK", "");
        }
    }

    public DialogueSO[] GetAllDialogueSOs()
    {
        Resources.LoadAll<DialogueSO>($"{Application.dataPath}\\Resources\\Dialogues");
        DialogueSO[] dialogues = (DialogueSO[])Resources.FindObjectsOfTypeAll(typeof(DialogueSO));
        return dialogues;
    }

    /*
     * Converts data passed through to CSV formatting and returns the modified string
     #1#
    public string ToCSV(string headers, List<string> data)
    {
        var sb = new StringBuilder(headers);
        foreach(var line in data)
        {
            sb.Append('\n').Append(line);
        }

        return sb.ToString();
    }
    
    /*
     * Saves the converted data back out to the CSV file path passed through
     #1#
    public void SaveToFile (string filePath, List<string> data, string headers)
    {
        try
        {
            var content = ToCSV(headers, data);
#if UNITY_EDITOR
            var folder = Application.streamingAssetsPath;

            if(!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
#else
    var folder = Application.persistentDataPath;
#endif

            string localPath = filePath.Replace($"{Application.dataPath}/", "");
            localPath = "Assets/" + localPath;
            File.WriteAllText(filePath, content);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
*/
