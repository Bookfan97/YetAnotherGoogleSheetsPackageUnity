using System;
using System.Collections.Generic;
using Editor.Google_Sheets;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Project_Settings
{
    public class GoogleSheetsCustomSettings : ScriptableObject
    {
        [SerializeField] public List<DataItem> m_Data;
        private Dictionary<string, string> m_settings;
        [SerializeField] private string m_spreadsheetID;
        private static readonly object _fileLock = new object();

        public string MSpreadsheetID => m_spreadsheetID;

        public string ClientSecretJsonPath => EditorPrefs.GetString(GoogleSheetsHelper.k_JSONEditorPref, "ClientSecret");

        public string GetPathForSheet(int spreadsheetID)
        {
            return m_Data.Find(x => x.key == spreadsheetID).value;
        }
        
        //Temporary 
        //public string CsvPath { get; set; }

        public string GetDefaultPath() => "Assets/Data";
    }

// Register a SettingsProvider using IMGUI for the drawing framework:
    static class MyCustomSettingsIMGUIRegister
    {
        static GUIStyle horizontalLine;
        [SettingsProvider]
        public static SettingsProvider CreateGoogleSheetsCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Project Settings window.
            var provider = new SettingsProvider("Project/Google Sheets", SettingsScope.Project)
            {
                // By default the last token of the path is used as display name if no label is provided.
                label = "Google Sheets",
                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = (searchContext) =>
                {
                    var settings = GoogleSheetsHelper.GetSerializedSettings();
                    GUILayout.Space(20f);
                    CreateHorizontalLine();
                    EditorGUILayout.LabelField("Google Sheets Settings", EditorStyles.boldLabel);
                    CreateHorizontalLine();
                    EditorGUILayout.PropertyField(settings.FindProperty("m_spreadsheetID"), new GUIContent("Spreadsheet ID"));
                    GetFilePath("Client JSON Secret", ".json");
                    GUILayout.Space(10f);
                    EditorGUILayout.PropertyField(settings.FindProperty("m_Data"), new GUIContent("My Data"));
                    settings.ApplyModifiedPropertiesWithoutUndo();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "CSV", "Google", "Sheets" })
            };

        
            return provider;
        }
        
        private static void CreateHorizontalLine()
        {
            horizontalLine = new GUIStyle
            {
                normal =
                {
                    background = EditorGUIUtility.whiteTexture
                },
                margin = new RectOffset(0, 0, 4, 4),
                fixedHeight = 3
            };

            HorizontalLine(Color.grey);
        }

        private static void HorizontalLine(Color color)
        {
            var c = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, horizontalLine);
            GUI.color = c;
        }

        private static void GetFilePath(string title, string extension)
        {
            EditorGUILayout.BeginHorizontal();
            
            string jsonPath = EditorPrefs.GetString(GoogleSheetsHelper.k_JSONEditorPref);
            if (string.IsNullOrWhiteSpace(jsonPath))
            {
                jsonPath = GoogleSheetsHelper.GoogleSheetsCustomSettings.GetDefaultPath();
            }

            string changedPath = EditorGUILayout.TextField(title,jsonPath);

            if (String.CompareOrdinal(jsonPath, changedPath) != 0)
            {
                EditorPrefs.SetString(GoogleSheetsHelper.k_JSONEditorPref, changedPath);
            }

            GUILayout.Space(10f);
            if (GUILayout.Button("Browse", GUILayout.Width(75)))
            {
                var jsonFilePath = EditorUtility.OpenFilePanel(
                    "Save Screenshots Directory", 
                    GoogleSheetsHelper.GoogleSheetsCustomSettings.GetDefaultPath(), 
                    extension
                );
                if (String.CompareOrdinal(jsonPath, jsonFilePath) != 0)
                {
                    EditorPrefs.SetString(GoogleSheetsHelper.k_JSONEditorPref, jsonFilePath);
                }
            }
            
            
            GUILayout.Space(10f);
            if (GUILayout.Button("Reset", GUILayout.Width(75)))
            {
                EditorPrefs.DeleteKey(GoogleSheetsHelper.k_JSONEditorPref);
            }
            
            EditorGUILayout.EndHorizontal();
        }
    
        [CustomPropertyDrawer(typeof(DataItem))]
        public class LocalizationItemDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                EditorGUILayout.BeginHorizontal();
                var keyRect = new Rect(position.x + 30, position.y, 100, position.height);
                var labelRect = new Rect(position.x + 165, position.y, 60, position.height);
                var valueRect = new Rect(position.x + 205, position.y, 240, position.height);
                var browseButtonRext = new Rect(position.x + 205 + 240, position.y, 75, position.height);
                var resetButtonRext = new Rect(position.x + 205 + 240 + 75, position.y, 75, position.height);

                EditorGUI.LabelField(position, "Key", "");
                EditorGUI.PropertyField(keyRect, property.FindPropertyRelative("key"), GUIContent.none);
                EditorGUI.LabelField(labelRect, "Value", "");
                EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("value"), GUIContent.none);
                if (GUI.Button(browseButtonRext, "Browse"))
                {
                    var jsonFilePath = EditorUtility.OpenFilePanel(
                        "Save Screenshots Directory", 
                        GoogleSheetsHelper.GoogleSheetsCustomSettings.GetDefaultPath(), 
                        ".csv"
                    );
                    if (String.CompareOrdinal("jsonPath", jsonFilePath) != 0)
                    {
                        property.FindPropertyRelative("value").stringValue = jsonFilePath;
                    }
                }

                if (GUI.Button(resetButtonRext, "Reset"))
                {
                    EditorPrefs.DeleteKey(GoogleSheetsHelper.k_JSONEditorPref);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.EndProperty();
            }
        }
    }
}