using System;
using System.Collections.Generic;
using System.IO;
using Editor.Assemblies;
using Editor.Google_Sheets;
using UnityEditor;
using UnityEngine;

namespace Editor.Project_Settings
{
    static class GoogleSheetsCustomSettingsIMGUIRegister
    {
        public static SettingsProvider thisSettingsProvider { get; private set; }
        
        private static string m_AssembliesToInclude = string.Empty; // Suggest populating dynamically later.
        public static readonly GUIContent AssembliesToIncludeLabel = EditorGUIUtility.TrTextContent("Included Assemblies", "Specify the assemblies that will be included in the coverage results.\n\nClick the dropdown to view and select or deselect the assemblies.");
        public static readonly GUIContent AssembliesToIncludeDropdownLabel = EditorGUIUtility.TrTextContent("Assemblies:", "Displays the list of assemblies to include.");
        public static readonly GUIContent AssembliesToIncludeEmptyDropdownLabel = EditorGUIUtility.TrTextContent("Select", "Click to view and manage assemblies.");
        private static GUIStyle horizontalLine;

        [SettingsProvider]
        public static SettingsProvider CreateGoogleSheetsCustomSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Google Sheets", SettingsScope.Project)
            {
                label = "Google Sheets",
                guiHandler = (searchContext) =>
                {
                    var settings = GoogleSheetsHelper.GetSerializedSettings();
                    if (settings == null)
                    {
                        EditorGUILayout.HelpBox("Failed to load Google Sheets settings.", MessageType.Error);
                        return;
                    }

                    GUILayout.Space(20f);
                    EnsureHorizontalLineStyle();
                    EditorGUILayout.LabelField("Google Sheets Settings", EditorStyles.boldLabel);
                    DrawHorizontalLine();

                    // Ensure property is valid
                    DrawPropertyWithValidation(settings, "m_spreadsheetID", "Spreadsheet ID");

                    DrawFilePathField("Client JSON Secret", ".json");

                    GUILayout.Space(10f);
                    DrawIncludedAssemblies();

                    GUILayout.Space(10f);
                    DrawPropertyWithValidation(settings, "m_Data", "My Data");

                    settings.ApplyModifiedPropertiesWithoutUndo();
                },
                keywords = new HashSet<string>(new[] { "CSV", "Google", "Sheets", "JSON" })
            };

            thisSettingsProvider = provider;
            return provider;
        }

        private static void DrawIncludedAssemblies()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(AssembliesToIncludeLabel);

            Rect buttonRect = EditorGUILayout.GetControlRect(GUILayout.MinWidth(10));
            string displayText = string.IsNullOrEmpty(m_AssembliesToInclude)
                            ? AssembliesToIncludeEmptyDropdownLabel.text
                            : AssembliesToIncludeDropdownLabel.text;

            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(displayText), FocusType.Keyboard, EditorStyles.miniPullDown))
            {
                GUI.FocusControl("");
                PopupWindow.Show(buttonRect, new IncludedAssembliesPopupWindow(new GoogleSheetsDataItemDrawer(), m_AssembliesToInclude));
            }

            GUILayout.EndHorizontal();
        }

        private static void DrawHorizontalLine()
        {
            var currentColor = GUI.color;
            GUI.color = EditorGUIUtility.isProSkin ? Color.grey : Color.black;
            GUILayout.Box(GUIContent.none, horizontalLine);
            GUI.color = currentColor;
        }

        private static void EnsureHorizontalLineStyle()
        {
            if (horizontalLine != null) return;

            horizontalLine = new GUIStyle
            {
                normal = { background = EditorGUIUtility.whiteTexture },
                margin = new RectOffset(0, 0, 4, 4),
                fixedHeight = 3
            };
        }

        private static void DrawFilePathField(string title, string extension)
        {
            EditorGUILayout.BeginHorizontal();

            string jsonPath = EditorPrefs.GetString(GoogleSheetsHelper.k_JSONEditorPref);
            if (string.IsNullOrWhiteSpace(jsonPath))
            {
                jsonPath = GoogleSheetsHelper.GoogleSheetsCustomSettings.GetDefaultPath();
            }

            string newPath = EditorGUILayout.TextField(title, jsonPath);

            if (!string.Equals(jsonPath, newPath, StringComparison.Ordinal))
            {
                EditorPrefs.SetString(GoogleSheetsHelper.k_JSONEditorPref, newPath);
            }

            if (GUILayout.Button("Browse", GUILayout.Width(75)))
            {
                var jsonFilePath = EditorUtility.OpenFilePanel("Select JSON File", jsonPath, extension);
                if (!string.IsNullOrEmpty(jsonFilePath) && Path.GetExtension(jsonFilePath) == extension)
                {
                    EditorPrefs.SetString(GoogleSheetsHelper.k_JSONEditorPref, jsonFilePath);
                }
                else if (!string.IsNullOrEmpty(jsonFilePath))
                {
                    Debug.LogError($"Please select a valid {extension} file.");
                }
            }

            if (GUILayout.Button("Reset", GUILayout.Width(75)))
            {
                EditorPrefs.DeleteKey(GoogleSheetsHelper.k_JSONEditorPref);
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawPropertyWithValidation(SerializedObject settings, string propertyName, string label)
        {
            var property = settings.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label));
            }
            else
            {
                EditorGUILayout.HelpBox($"Property '{propertyName}' was not found in the serialized settings.", MessageType.Error);
            }
        }

        [CustomPropertyDrawer(typeof(DataItem))]
        public class GoogleSheetsDataItemDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);
                EditorGUILayout.BeginHorizontal();

                float fieldWidth = position.width / 5;

                Rect keyRect = new Rect(position.x, position.y, fieldWidth, position.height);
                Rect valueRect = new Rect(position.x + fieldWidth + 5, position.y, fieldWidth, position.height);
                Rect dropdownRect = new Rect(position.x + 2 * (fieldWidth + 5), position.y, fieldWidth, position.height);
                Rect browseRect = new Rect(position.x + 3 * (fieldWidth + 5), position.y, fieldWidth / 2, position.height);
                Rect resetRect = new Rect(position.x + 3.5f * (fieldWidth + 5), position.y, fieldWidth / 2, position.height);

                EditorGUI.PropertyField(keyRect, property.FindPropertyRelative("key"), GUIContent.none);
                EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("value"), GUIContent.none);

                var dropdownLabel = new GUIContent("Scriptable Object");
                int dropdownIndex = property.FindPropertyRelative("index").intValue;
                string[] dropdownOptions = { "Option1", "Option2" }; // Replace dynamic SO population
                property.FindPropertyRelative("index").intValue = EditorGUI.Popup(dropdownRect, dropdownIndex, dropdownOptions);

                if (GUI.Button(browseRect, "Browse"))
                {
                    string jsonFilePath = EditorUtility.OpenFilePanel("Select File", "", "json");
                    if (!string.IsNullOrEmpty(jsonFilePath))
                    {
                        property.FindPropertyRelative("value").stringValue = jsonFilePath;
                    }
                }

                if (GUI.Button(resetRect, "Reset"))
                {
                    property.FindPropertyRelative("value").stringValue = string.Empty;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUI.EndProperty();
            }
        }
    }
}