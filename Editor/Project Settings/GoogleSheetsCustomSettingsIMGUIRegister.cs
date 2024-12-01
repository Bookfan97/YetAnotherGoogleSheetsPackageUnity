using System;
using System.Collections.Generic;
using Editor.Google_Sheets;
using UnityEditor;
using UnityEngine;

namespace Editor.Project_Settings
{
    /// <summary>
    /// GoogleSheetsCustomSettingsIMGUIRegister is responsible for registering a custom settings provider
    /// within Unity's Project Settings window specifically for Google Sheets integration.
    /// It defines the GUI elements and structure using IMGUI to allow users to input and manage
    /// configuration details such as the Spreadsheet ID and Client JSON Secret for Google Sheets integration.
    /// </summary>
    static class GoogleSheetsCustomSettingsIMGUIRegister
    {
        /// <summary>
        /// A GUIStyle object used for drawing horizontal lines in the Unity Editor's GUI. It is configured
        /// with specific styling properties such as background texture and margin, and utilized to enhance
        /// the visual separation of sections within custom editor windows.
        /// </summary>
        static GUIStyle horizontalLine;

        /// <summary>
        /// Creates a custom SettingsProvider for Google Sheets settings within Unity's Project Settings window.
        /// Configures the GUI layout for displaying and modifying Google Sheets related configurations.
        /// </summary>
        /// <returns>
        /// Returns a SettingsProvider object configured to display Google Sheets settings with IMGUI rendering.
        /// </returns>
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
                keywords = new HashSet<string>(new[] { "CSV", "Google", "Sheets", "JSON" })
            };

        
            return provider;
        }

        /// <summary>
        /// Creates and configures a GUIStyle for drawing horizontal lines in the Unity Editor's GUI,
        /// and invokes a method to draw a horizontal line using the created style.
        /// </summary>
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

        /// <summary>
        /// Draws a horizontal line with the specified color in the Unity Editor's GUI.
        /// </summary>
        /// <param name="color">The color to use for the horizontal line.</param>
        private static void HorizontalLine(Color color)
        {
            var c = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, horizontalLine);
            GUI.color = c;
        }

        /// <summary>
        /// Displays a text field for file path input and buttons for browsing and resetting the file path associated with the specified JSON file.
        /// </summary>
        /// <param name="title">The label to display next to the file path text field.</param>
        /// <param name="extension">The file extension filter to use when browsing for a file.</param>
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

        /// <summary>
        /// GoogleSheetsDataItemDrawer is a custom property drawer for DataItem objects, used within Unity's Editor.
        /// This drawer allows for customized GUI elements to be drawn in the Inspector window for properties of type DataItem.
        /// </summary>
        [CustomPropertyDrawer(typeof(DataItem))]
        public class GoogleSheetsDataItemDrawer : PropertyDrawer
        {
            /// <summary>
            /// Overrides the default GUI rendering for the DataItem property, providing custom layout and functionality
            /// within the Unity Editor.
            /// </summary>
            /// <param name="position">The rectangle on the screen to use for rendering the property.</param>
            /// <param name="property">The SerializedProperty representing the DataItem being drawn in the inspector.</param>
            /// <param name="label">The label of the property field being rendered.</param>
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