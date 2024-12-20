using System;
using Editor.Google_Sheets;
using Editor.Project_Settings;
using UnityEditor;
using UnityEditor.Localization.Plugins.Google;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Represents a collection of menu items in the editor.
    /// This class provides functionality to manage and interact with the menu items.
    /// </summary>
    public static class MenuItems
    {
        //static CSVtoSO CSVconverter;
        //static SOtoCSV SOconverter;
        public static string csvPath = "";
        
        /// <summary>
        /// Stores an instance of the GoogleSheetsInstance class, which manages
        /// interactions with Google Sheets, such as uploading and downloading data.
        /// </summary>
        private static GoogleSheetsInstance _instance;

        /// <summary>
        /// Opens the Google Sheet in the default web browser using the Spreadsheet ID from settings.
        /// Checks if the Spreadsheet ID is valid before attempting to open the sheet.
        /// </summary>
        [MenuItem("Tools/Google Sheets/Open Sheet", false, 3)]
        private static void OpenSheet()
        {
            if (!GoogleSheetsEditorUtilities.IsValidSpreadsheetID()) return;
            GoogleSheets.OpenSheetInBrowser(GoogleSheetsHelper.GoogleSheetsCustomSettings.MSpreadsheetID);
        }

        /// <summary>
        /// Uploads data to Google Sheets.
        /// Verifies if the Google Sheets configuration contains valid information
        /// before proceeding with the upload. If valid, initializes an instance
        /// of GoogleSheetsInstance and uploads the data.
        /// </summary>
        [MenuItem("Tools/Google Sheets/Uploader", false, 4)]
        private static void UploadData()
        {
            if (!GoogleSheetsEditorUtilities.CheckValidInformation()) return;
            _instance = new GoogleSheetsInstance();
            _instance.UploadData();
        }

        /// <summary>
        /// Downloads data from the configured Google Spreadsheet.
        /// Ensures that the configuration contains valid information
        /// before attempting to download the data.
        /// </summary>
        [MenuItem("Tools/Google Sheets/Downloader", false, 5)]
        private static void DownloadData()
        {
            if (!GoogleSheetsEditorUtilities.CheckValidInformation()) return;
            _instance = new GoogleSheetsInstance();
            _instance.DownloadData();
        }

        /// <summary>
        /// Opens the Google Sheets preferences in the Unity Project Settings.
        /// </summary>
        [MenuItem("Tools/Google Sheets/Open Preferences", false, 2)]
        private static void OpenPreferences()
        {
            SettingsService.OpenProjectSettings("Project/Google Sheets");
        }

        /// <summary>
        /// Opens the Google Sheets documentation URL in the default web browser.
        /// This method is accessible via the Tools > Google Sheets > Documentation menu item in the Unity Editor.
        /// </summary>
        [MenuItem("Tools/Google Sheets/Documentation", false, 1)]
        private static void OpenDocumentation()
        {
            Application.OpenURL(
                "https://definitive-infinity-media.github.io/com.definitiveinfinitymedia.googlesheets/api/index.html");
        }
        
        /// <summary>
        /// Opens the Google Sheets documentation URL in the default web browser.
        /// This method is accessible via the Tools > Google Sheets > Documentation menu item in the Unity Editor.
        /// </summary>
        [MenuItem("Tools/Google Sheets/ConvertCSVtoSO", false, 6)]
        private static void ConvertCSVtoSO()
        {
            try
            {
                //CSVconverter = new CSVtoSO();
                csvPath = EditorUtility.OpenFilePanel(
                    "Open Dialogue CSV File", 
                    $"{Application.dataPath}\\Resources\\CSV", 
                    "csv"
                );
                if (csvPath.Length != 0)
                {
                    string localPath = csvPath.Replace($"{Application.dataPath}/", "");
                    localPath = "Assets/" + localPath;
                    TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(localPath);
                    /*if (asset != null)
                    {
                        CSVconverter.GenerateScriptableObjects<DialogueSO>(asset);
                    }*/
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
        
        /// <summary>
        /// Opens the Google Sheets documentation URL in the default web browser.
        /// This method is accessible via the Tools > Google Sheets > Documentation menu item in the Unity Editor.
        /// </summary>
        [MenuItem("Tools/Google Sheets/ConvertSOtoCSV", false, 7)]
        private static void ConvertSOtoCSV()
        {
            try
            {
                //SOconverter = new SOtoCSV();
                csvPath = EditorUtility.SaveFilePanel(
                    "Save Dialogue CSV File", 
                    $"{Application.dataPath}\\Resources\\CSV", 
                    $"DialogueData_Updated.csv",
                    "csv"
                );
                if (csvPath.Length != 0)
                {
                    string localPath = csvPath.Replace($"{Application.dataPath}/", "");
                    localPath = "Assets/" + localPath;
                    if (!System.IO.File.Exists(csvPath))
                    {
                        TextAsset text = new TextAsset();
                        AssetDatabase.CreateAsset(text, localPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                    TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(localPath);
                    /*if (asset != null)
                    {
                        SOconverter.CSVtoDialogueLines(csvPath);
                    }*/
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
    }
}