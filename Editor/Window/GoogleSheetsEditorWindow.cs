using System;
using Editor.Google_Sheets;
using Editor.Project_Settings;
using Editor.ScriptableObjectConverter;
using UnityEditor;
using UnityEngine;

namespace Editor.Window
{
    public class GoogleSheetsEditorWindow : EditorWindow
    {
        /// <summary>
        ///     Stores an instance of the GoogleSheetsInstance class, which manages
        ///     interactions with Google Sheets, such as uploading and downloading data.
        /// </summary>
        private static GoogleSheetsInstance _instance;

        private void OnGUI()
        {
            EditorGUIUtility.SetIconSize(new Vector2(15, 15));
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            var banner = (Texture)AssetDatabase.LoadAssetAtPath(
                "Packages/com.definitiveinfinitymedia.googlesheets/images/google-sheets.png", typeof(Texture));
            GUILayout.Box(banner, new GUIStyle
            {
                fixedWidth = 20,
                fixedHeight = 20
            });
            GUILayout.Label("Google Sheets Editor", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            // EditorGUILayout.LabelField(
            //     "Load or create new lists of keys and values. Make sure each language you are using has the same set of keys and the values for each language are the localized items for that specific audience.",
            //     GUIHelper.GUIMessageStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Sheet", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent
                {
                    text = "Open Google Sheets",
                    tooltip = "Opens the sheet in the browser window",
                    image = EditorGUIUtility.IconContent("Linked").image
                })) OpenSheet();

            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Settings", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            // if (GUI.Button(new Rect(selectionRect.x + 200, selectionRect.y + 2, 14, 14),
            //         EditorGUIUtility.IconContent("Prefab Icon").image, GUIStyle.none))

            if (GUILayout.Button(new GUIContent
                {
                    text = "Open Preferences",
                    tooltip = "Open the preferences in the editor window",
                    image = EditorGUIUtility.IconContent("_Popup@2x").image
                })) OpenPreferences();

            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Upload / Download all from Sheet", EditorStyles.boldLabel);

            // if (localizedData != null)
            // {
            //     var serializedObject = new SerializedObject(this);
            //     var serializedProperty = serializedObject.FindProperty("localizedData");
            //     EditorGUILayout.PropertyField(serializedProperty, true);
            //     serializedObject.ApplyModifiedProperties();
            // }

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent
                {
                    text = "Download All",
                    tooltip = "Download all data from the Google Sheets",
                    image = EditorGUIUtility.IconContent("Download-Available").image
                })) DownloadAll();

            if (GUILayout.Button(new GUIContent
                {
                    text = "Upload All",
                    tooltip = "Upload all data from the Google Sheets",
                    image = EditorGUIUtility.IconContent("Update-Available").image
                })) UploadAll();

            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            foreach (var dataItem in GoogleSheetsHelper.GoogleSheetsCustomSettings.Data)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(dataItem.scriptableObjectType, GUILayout.Width(100));
                if (GUILayout.Button(new GUIContent
                    {
                        text = "Download",
                        tooltip = $"Download All {dataItem.scriptableObjectType} data from Sheet to the CSV file",
                        image = EditorGUIUtility.IconContent("Download-Available").image
                    }))
                    try
                    {
                        if (!GoogleSheetsEditorUtilities.CheckValidInformation()) return;
                        _instance = new GoogleSheetsInstance();
                        _instance.DownloadSheetData(dataItem);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        throw;
                    }

                ;
                if (GUILayout.Button(new GUIContent
                    {
                        text = "Upload",
                        tooltip = $"Upload All {dataItem.scriptableObjectType} data from CSV to the Google Sheet",
                        image = EditorGUIUtility.IconContent("Update-Available").image
                    }))
                    try
                    {
                        if (!GoogleSheetsEditorUtilities.CheckValidInformation()) return;
                        _instance = new GoogleSheetsInstance();
                        _instance.UploadSheetData(dataItem);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        throw;
                    }

                ;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Data Management", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();

            // if (localizedData != null)
            //     if (GUILayout.Button("Save Language - JSON"))
            //         SaveGameDataJSON();

            if (GUILayout.Button(new GUIContent
                {
                    text = "Convert all SOs to CSVs",
                    tooltip = "Gets all scriptable objects in the project and copies the values to CSV files",
                    image = EditorGUIUtility.IconContent("TextAsset Icon").image
                })) ConvertSOtoCSV();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            // if (localizedData != null)
            //     if (GUILayout.Button("Save Language - Strings"))
            //         SaveGameDataStrings();

            if (GUILayout.Button(new GUIContent
                {
                    text = "Convert all CSVs to SOs",
                    tooltip = "Gets all CSV files in the project and copies the values into scriptable objects",
                    image = EditorGUIUtility.IconContent("ScriptableObject Icon").image
                })) ConvertCSVtoSO();


            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            foreach (var dataItem in GoogleSheetsHelper.GoogleSheetsCustomSettings.Data)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(dataItem.scriptableObjectType, GUILayout.Width(100));
                if (GUILayout.Button(new GUIContent
                    {
                        text = "CSVs to SOs",
                        tooltip =
                            $"Copy All {dataItem.scriptableObjectType} values from the CSV file to scriptable objects",
                        image = EditorGUIUtility.IconContent("ScriptableObject Icon").image
                    }))
                    try
                    {
                        var converter = new CSVtoSO();
                        converter.Generate(dataItem);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        throw;
                    }

                ;
                if (GUILayout.Button(new GUIContent
                    {
                        text = "SOs to CSVs",
                        tooltip =
                            $"Copy All {dataItem.scriptableObjectType} values from the scriptable objects to the CSV file",
                        image = EditorGUIUtility.IconContent("TextAsset Icon").image
                    }))
                    try
                    {
                        var converter = new SOtoCSV();
                        converter.ScriptableObjectsToCSV(dataItem);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        throw;
                    }

                ;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        [MenuItem("Tools/Google Sheets/Editor Window")]
        private static void Init()
        {
            GetWindow(typeof(GoogleSheetsEditorWindow), false, "Google Sheets", true).Show();
        }

        // private void Upload(DataItem dataItem)
        // {
        //     throw new NotImplementedException();
        // }
        //
        // private void Download(DataItem dataItem)
        // {
        //     throw new NotImplementedException();
        // }

        /// <summary>
        ///     Opens the Google Sheet in the default web browser using the Spreadsheet ID from settings.
        ///     Checks if the Spreadsheet ID is valid before attempting to open the sheet.
        /// </summary>
        private void OpenSheet()
        {
            if (!GoogleSheetsEditorUtilities.IsValidSpreadsheetID()) return;
            Application.OpenURL(
                $"https://docs.google.com/spreadsheets/d/{GoogleSheetsHelper.GoogleSheetsCustomSettings.MSpreadsheetID}");
        }

        /// <summary>
        ///     Uploads data to Google Sheets.
        ///     Verifies if the Google Sheets configuration contains valid information
        ///     before proceeding with the upload. If valid, initializes an instance
        ///     of GoogleSheetsInstance and uploads the data.
        /// </summary>
        private void UploadAll()
        {
            if (!GoogleSheetsEditorUtilities.CheckValidInformation()) return;
            _instance = new GoogleSheetsInstance();
            _instance.UploadAllData();
        }

        /// <summary>
        ///     Downloads data from the configured Google Spreadsheet.
        ///     Ensures that the configuration contains valid information
        ///     before attempting to download the data.
        /// </summary>
        private void DownloadAll()
        {
            if (!GoogleSheetsEditorUtilities.CheckValidInformation()) return;
            _instance = new GoogleSheetsInstance();
            _instance.DownloadAllData();
        }

        /// <summary>
        ///     Opens the Google Sheets preferences in the Unity Project Settings.
        /// </summary>
        private void OpenPreferences()
        {
            SettingsService.OpenProjectSettings("Project/Google Sheets");
        }

        private void ConvertCSVtoSO()
        {
            try
            {
                foreach (var dataItem in GoogleSheetsHelper.GoogleSheetsCustomSettings.Data)
                {
                    var converter = new CSVtoSO();
                    converter.Generate(dataItem);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        private void ConvertSOtoCSV()
        {
            try
            {
                foreach (var dataItem in GoogleSheetsHelper.GoogleSheetsCustomSettings.Data)
                {
                    var converter = new SOtoCSV();
                    converter.ScriptableObjectsToCSV(dataItem);
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