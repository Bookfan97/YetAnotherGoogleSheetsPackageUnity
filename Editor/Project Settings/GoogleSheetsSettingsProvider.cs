using UnityEditor;
using UnityEngine;

namespace Editor.Project_Settings
{
    /// <summary>
    /// Provides project settings for interacting with Google Sheets within Unity.
    /// </summary>
    public class GoogleSheetsSettingsProvider : SettingsProvider
    {
        /// <summary>
        /// A GUIStyle object used to define the appearance and behavior of horizontal lines
        /// drawn in the Google Sheets settings UI within Unity.
        /// </summary>
        private static GUIStyle horizontalLine;

        /// <summary>
        /// Represents the width used for GUI layout elements in the Google Sheets settings UI within Unity.
        /// </summary>
        private static readonly float _width = 300f;

        /// <summary>
        /// Provides project settings for interacting with Google Sheets within Unity.
        /// </summary>
        private GoogleSheetsSettingsProvider(string path, SettingsScope scope) : base(path,
            scope)
        {
        }

        /// <summary>
        /// Renders the settings user interface for the Google Sheets settings provider in Unity's Project Settings.
        /// </summary>
        /// <param name="searchContext">A search field to filter the settings displayed in the Project Settings window.</param>
        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            GUILayout.Space(20f);
            CreateSectionHeader("Settings");
            GoogleSheetsContent();
        }

        /// <summary>
        /// Creates and returns a settings provider for Google Sheets project settings in Unity Editor.
        /// </summary>
        /// <returns>A settings provider configured for Google Sheets project settings.</returns>
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new GoogleSheetsSettingsProvider("Project/Google Sheets",
                SettingsScope.Project);
        }

        #region Content

        /// <summary>
        /// Adds a section header to the settings UI in Unity's Project Settings.
        /// </summary>
        /// <param name="headerName">The title of the section header to be displayed.</param>
        private static void CreateSectionHeader(string headerName)
        {
            CreateHorizontalLine();
            EditorGUILayout.LabelField(headerName, EditorStyles.boldLabel);
            CreateHorizontalLine();
        }

        /// <summary>
        /// Renders the UI content for configuring Google Sheets integration settings in Unity Editor.
        /// Includes options to specify paths for CSV and JSON files, as well as the Google Sheets Spreadsheet ID.
        /// </summary>
        private static void GoogleSheetsContent()
        {
            #region CSV

            EditorGUILayout.BeginHorizontal();

            var csvPath = GoogleSheetsSettings.instance.CsvPath;
            if (string.IsNullOrWhiteSpace(csvPath)) csvPath = GoogleSheetsSettings.instance.GetDefaultPath();

            var changedCsvPath = EditorGUILayout.TextField("CSV Path", csvPath);

            if (string.CompareOrdinal(csvPath, changedCsvPath) != 0)
                GoogleSheetsSettings.instance.CsvPath = changedCsvPath;

            GUILayout.Space(10f);
            if (GUILayout.Button("Browse", GUILayout.Width(75)))
            {
                var csvFilePath = EditorUtility.OpenFilePanel(
                    "Save Screenshots Directory",
                    GoogleSheetsSettings.instance.GetDefaultPath(),
                    "csv"
                );
                if (string.CompareOrdinal(csvPath, csvFilePath) != 0)
                    GoogleSheetsSettings.instance.CsvPath = csvFilePath;
            }


            GUILayout.Space(10f);
            if (GUILayout.Button("Reset", GUILayout.Width(75)))
                GoogleSheetsSettings.instance.CsvPath = GoogleSheetsSettings.instance.GetDefaultPath();
            EditorGUILayout.EndHorizontal();

            #endregion

            #region JSON

            EditorGUILayout.BeginHorizontal();

            var jsonPath = EditorPrefs.GetString(GoogleSheetsSettings.instance.k_JSONEditorPref);
            if (string.IsNullOrWhiteSpace(jsonPath)) jsonPath = GoogleSheetsSettings.instance.GetDefaultPath();

            var changedPath = EditorGUILayout.TextField("Client JSON Secret", jsonPath);

            if (string.CompareOrdinal(jsonPath, changedPath) != 0)
                EditorPrefs.SetString(GoogleSheetsSettings.instance.k_JSONEditorPref, changedPath);

            GUILayout.Space(10f);
            if (GUILayout.Button("Browse", GUILayout.Width(75)))
            {
                var jsonFilePath = EditorUtility.OpenFilePanel(
                    "Save Screenshots Directory",
                    GoogleSheetsSettings.instance.GetDefaultPath(),
                    "json"
                );
                if (string.CompareOrdinal(jsonPath, jsonFilePath) != 0)
                    EditorPrefs.SetString(GoogleSheetsSettings.instance.k_JSONEditorPref, jsonFilePath);
            }


            GUILayout.Space(10f);
            if (GUILayout.Button("Reset", GUILayout.Width(75)))
                EditorPrefs.DeleteKey(GoogleSheetsSettings.instance.k_JSONEditorPref);

            EditorGUILayout.EndHorizontal();

            #endregion

            var instanceSheetID = GoogleSheetsSettings.instance.SpreadsheetID;
            var sheetID = EditorGUILayout.TextField("Spreadsheet ID", instanceSheetID, GUILayout.Width(_width));
            if (instanceSheetID != sheetID) GoogleSheetsSettings.instance.SpreadsheetID = sheetID;
        }

        #endregion

        #region GUI_Configuration

        /// <summary>
        /// Draws a horizontal line with the specified color.
        /// </summary>
        /// <param name="color">The color of the horizontal line.</param>
        private static void HorizontalLine(Color color)
        {
            var c = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, horizontalLine);
            GUI.color = c;
        }

        /// <summary>
        /// Initializes and creates a horizontal line with a default grey color
        /// to visually separate sections in the Google Sheets settings UI.
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

        #endregion
    }
}