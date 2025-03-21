using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Editor.Assemblies;
using Editor.Data;
using Editor.Google_Sheets;
using UnityEditor;
using UnityEngine;

namespace Editor.Project_Settings
{
    internal static class GoogleSheetsCustomSettingsIMGUIRegister
    {
        // Constants to avoid hardcoding strings
        private const string SPREADSHEET_ID_PROP = "m_spreadsheetID";
        private const string SERIALIZED_DATA_PROP = "m_Data";
        private const string JSON_FILE_EXTENSION = ".json";
        private const string PREFS_KEY = "Client JSON Secret";
        private static string m_AssembliesToInclude;
        private static int m_AssembliesToIncludeLength;

        // GUIContent can remain static, as it does not change
        private static readonly GUIContent AssembliesToIncludeLabel =
            EditorGUIUtility.TrTextContent("Included Assemblies",
                "Specify the assemblies that will be included in the coverage results.\n\nClick the dropdown to view and select or deselect the assemblies.");

        private static readonly GUIContent AssembliesToIncludeDropdownLabel =
            EditorGUIUtility.TrTextContent("Assemblies:", "Displays the list of assemblies to include.");

        private static readonly GUIContent AssembliesToIncludeEmptyDropdownLabel =
            EditorGUIUtility.TrTextContent("Select", "Click to view and manage assemblies.");

        // Store style privately, initialize dynamically
        private static GUIStyle horizontalLine;
        public static SettingsProvider thisSettingsProvider { get; private set; }

        public static string AssembliesToInclude
        {
            get => m_AssembliesToInclude;
            set
            {
                m_AssembliesToInclude = value.TrimStart(',').TrimEnd(',');
                m_AssembliesToIncludeLength = m_AssembliesToInclude.Length;
                JSONUtility.GoogleSheetsJsonData.assembliesToInclude = m_AssembliesToInclude;
                GoogleSheetsHelper.GoogleSheetsCustomSettings.AssembliesToInclude = m_AssembliesToInclude;
                //CoveragePreferences.instance.SetString("IncludeAssemblies", m_AssembliesToInclude);
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateGoogleSheetsCustomSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Google Sheets", SettingsScope.Project)
            {
                label = "Google Sheets",
                guiHandler = searchContext =>
                {
                    var settings = GoogleSheetsHelper.GetSerializedSettings();
                    if (settings == null)
                    {
                        // Provide clear feedback if settings are missing
                        EditorGUILayout.HelpBox(
                            "Failed to load Google Sheets settings. Ensure it is properly configured.",
                            MessageType.Error);
                        return;
                    }

                    GUILayout.Space(20f);
                    EnsureHorizontalLineStyle();
                    DrawHorizontalLine();
                    GUILayout.Space(10f);
                    EditorPrefs.SetBool(GoogleSheetsHelper.k_debugLogEditorPref,
                        EditorGUILayout.Toggle("Show Debug Logs",
                            EditorPrefs.GetBool(GoogleSheetsHelper.k_debugLogEditorPref, false)));
                    GUILayout.Space(10f);
                    // Drawing serialized properties with validation
                    DrawPropertyWithValidation(settings, SPREADSHEET_ID_PROP, "Spreadsheet ID");

                    // File path field that handles file-browsing with validation
                    DrawFilePathField(PREFS_KEY, JSON_FILE_EXTENSION);

                    GUILayout.Space(10f);

                    // Included assemblies dropdown with validation
                    DrawIncludedAssemblies();

                    GUILayout.Space(10f);

                    // Drawing additional properties with validation
                    DrawPropertyWithValidation(settings, SERIALIZED_DATA_PROP, "Sheet Data");

                    // Apply changes without undo history
                    settings.ApplyModifiedProperties();

                    AssembliesToInclude = JSONUtility.GoogleSheetsJsonData.assembliesToInclude;
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

            // Safely retrieve the assemblies string
            var assembliesToInclude = AssembliesToInclude;
            //GoogleSheetsHelper.GoogleSheetsCustomSettings?.AssembliesToInclude ?? string.Empty;
            var displayText = string.IsNullOrEmpty(assembliesToInclude)
                ? AssembliesToIncludeEmptyDropdownLabel.text
                : $"{AssembliesToIncludeDropdownLabel.text}{assembliesToInclude}";

            var buttonRect = EditorGUILayout.GetControlRect(GUILayout.MinWidth(10));
            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(displayText), FocusType.Keyboard,
                    EditorStyles.miniPullDown))
            {
                GUI.FocusControl(""); // Reset focus for better UX
                PopupWindow.Show(buttonRect,
                    new IncludedAssembliesPopupWindow(new GoogleSheetsDataItemDrawer(), AssembliesToInclude));
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

            // Safely retrieve the JSON path and its default fallback
            var jsonPath = EditorPrefs.GetString(GoogleSheetsHelper.k_JSONEditorPref);
            if (string.IsNullOrWhiteSpace(jsonPath))
                jsonPath = GoogleSheetsHelper.GoogleSheetsCustomSettings?.GetDefaultPath() ?? "Path unavailable";

            // Text field validation and update logic
            var newPath = EditorGUILayout.TextField(title, jsonPath);
            if (!string.Equals(jsonPath, newPath, StringComparison.Ordinal))
                EditorPrefs.SetString(GoogleSheetsHelper.k_JSONEditorPref, newPath);

            // File browsing with extension validation
            if (GUILayout.Button("Browse", GUILayout.Width(75)))
            {
                var jsonFilePath = EditorUtility.OpenFilePanel("Select JSON File", jsonPath, extension);
                if (!string.IsNullOrEmpty(jsonFilePath) && Path.GetExtension(jsonFilePath)
                        .Equals(extension, StringComparison.OrdinalIgnoreCase))
                    EditorPrefs.SetString(GoogleSheetsHelper.k_JSONEditorPref, jsonFilePath);
                else if (!string.IsNullOrEmpty(jsonFilePath))
                    Debug.LogError("Invalid file selected. Please select a valid JSON file.");
            }

            // Reset settings, with user feedback
            if (GUILayout.Button("Reset", GUILayout.Width(75)))
            {
                EditorPrefs.DeleteKey(GoogleSheetsHelper.k_JSONEditorPref);
                Debug.Log("Settings have been reset to default.");
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawPropertyWithValidation(SerializedObject settings, string propertyName, string label)
        {
            // Ensure property exists in serialized object
            var property = settings.FindProperty(propertyName);
            if (property != null)
                EditorGUILayout.PropertyField(property, new GUIContent(label));
            else
                EditorGUILayout.HelpBox($"Property '{propertyName}' not found. Verify settings are correct.",
                    MessageType.Warning);
        }

        [CustomPropertyDrawer(typeof(DataItem))]
        public class GoogleSheetsDataItemDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                var fieldWidth = position.width / 5;

                // Rects for nested fields
                var keyRect = new Rect(position.x, position.y, fieldWidth, position.height);
                var valueRect = new Rect(position.x + fieldWidth + 5, position.y, fieldWidth, position.height);
                var dropdownRect = new Rect(position.x + 2 * (fieldWidth + 5), position.y, fieldWidth,
                    position.height);
                var browseRect = new Rect(position.x + 3 * (fieldWidth + 5), position.y, fieldWidth / 2,
                    position.height);
                var resetRect = new Rect(position.x + 3.5f * (fieldWidth + 5), position.y, fieldWidth / 2,
                    position.height);

                // Ensure valid properties
                var keyProp = property.FindPropertyRelative("key");
                var valueProp = property.FindPropertyRelative("value");
                var indexProp = property.FindPropertyRelative("index");
                var typeProp = property.FindPropertyRelative("scriptableObjectType");
                var assemblyProp = property.FindPropertyRelative("assemblyName");
                var typeValueProp = property.FindPropertyRelative("type");
                if (keyProp != null && valueProp != null && indexProp != null && typeProp != null)
                {
                    // Draw fields
                    EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
                    EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);
                    //UpdateJSON(keyProp, assemblyProp, typeProp);

                    // Populate dropdown menu
                    var dropdownOptions = Array.Empty<string>();
                    if (GoogleSheetsHelper.GoogleSheetsCustomSettings.allTypes != null &&
                        GoogleSheetsHelper.GoogleSheetsCustomSettings.allTypes.Count != 0)
                    {
                        dropdownOptions = GoogleSheetsHelper.GoogleSheetsCustomSettings.scriptableObjects
                            .Select(obj => obj.Key.Name).ToArray();
                        indexProp.intValue = EditorGUI.Popup(dropdownRect, indexProp.intValue, dropdownOptions);
                        typeProp.stringValue = GoogleSheetsHelper.GoogleSheetsCustomSettings
                            .allTypes[indexProp.intValue].Name;
                        assemblyProp.stringValue = GoogleSheetsHelper.GoogleSheetsCustomSettings
                            .allTypes[indexProp.intValue].Assembly.GetName().Name;
                        UpdateJSON(keyProp, assemblyProp, typeProp);
                    }

                    // Browse button for file selection
                    if (GUI.Button(browseRect, "Browse"))
                    {
                        var filePath = EditorUtility.OpenFilePanel("Select File", "", "csv");
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            if (filePath.Contains(Application.dataPath))
                                filePath = filePath.Replace(Application.dataPath, "Assets");
                            valueProp.stringValue = filePath;
                        }
                    }

                    // Reset button to clear value
                    if (GUI.Button(resetRect, "Reset")) valueProp.stringValue = string.Empty;
                }

                EditorGUI.EndProperty();
            }

            private static void UpdateJSON(SerializedProperty keyProp, SerializedProperty assemblyProp,
                SerializedProperty typeProp)
            {
                JSONUtility.GoogleSheetsJsonData.Add(new KeyValuePair<int, SheetData>(
                    keyProp.intValue,
                    new SheetData(keyProp.intValue, assemblyProp.stringValue, typeProp.stringValue, "")
                ));
                //JSONUtility.SaveData();
            }
        }
    }
}