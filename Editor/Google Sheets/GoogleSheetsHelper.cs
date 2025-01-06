using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Editor.Project_Settings;
using UnityEditor;
using UnityEngine;

namespace Editor.Google_Sheets
{
    /// <summary>
    /// Provides helper methods and properties for managing Google Sheets settings within the editor.
    /// </summary>
    public class GoogleSheetsHelper
    {
        /// <summary>
        /// Constant string representing the file path for storing the GoogleSheetsCustomSettings asset.
        /// </summary>
        /// <remarks>
        /// This path is used to load or save the GoogleSheetsCustomSettings asset within the Unity project.
        /// It is a predefined location in the assets directory where the settings asset is maintained.
        /// </remarks>
        private const string k_MyCustomSettingsPath = "Assets/Settings/GoogleSheetsCustomSettings.asset";

        /// <summary>
        /// The <c>k_JSONEditorPref</c> field is used as the key for storing and retrieving
        /// the file path to the JSON file containing client secret information within
        /// the Unity Editor preferences. This string acts as an identifier for the
        /// preference setting, ensuring that the proper file path is maintained
        /// throughout various editor sessions.
        /// </summary>
        public static string k_JSONEditorPref { get; private set; } = "DefinitiveInfinityMediaGoogleSheetsSettings_JSONPath";

        /// <summary>
        /// Constant string representing the preference key for enabling or disabling debug logs related to Google Sheets settings in the editor.
        /// </summary>
        /// <remarks>
        /// This key is used to store and retrieve the user preference for logging debug information in the Unity editor
        /// while working with Google Sheets integrations. It ensures consistent behavior and settings across editor sessions.
        /// </remarks>
        public static string k_debugLogEditorPref { get; private set; } = "DefinitiveInfinityMediaGoogleSheetsSettings_DebugLogs";

        /// <summary>
        /// Property to get the instance of GoogleSheetsCustomSettings.
        /// </summary>
        /// <remarks>
        /// This property retrieves or creates an instance of the GoogleSheetsCustomSettings
        /// which is used to store and manage custom settings related to Google Sheets integration.
        /// </remarks>
        public static GoogleSheetsCustomSettings GoogleSheetsCustomSettings => GetOrCreateSettings();

        /// <summary>
        /// Retrieves the GoogleSheetsCustomSettings asset from the predefined path.
        /// If the asset does not exist, it creates a new instance of GoogleSheetsCustomSettings, assigns default values,
        /// and saves it in the appropriate directory.
        /// </summary>
        /// <returns>
        /// The GoogleSheetsCustomSettings instance, either loaded from the asset path or newly created.
        /// </returns>
        private static GoogleSheetsCustomSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<GoogleSheetsCustomSettings>(k_MyCustomSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<GoogleSheetsCustomSettings>();
                settings.Data = new List<DataItem>();
                if(!Directory.Exists("Assets/Settings"))
                {	
                    //if it doesn't, create it
                    Directory.CreateDirectory("Assets/Settings");

                }
                AssetDatabase.CreateAsset(settings, k_MyCustomSettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        /// <summary>
        /// Retrieves a serialized object for Google Sheets custom settings.
        /// </summary>
        /// <returns>A SerializedObject representing the Google Sheets custom settings.</returns>
        internal static SerializedObject GetSerializedSettings() => new(GetOrCreateSettings());

        public static Type CheckType(string typeName)
        {
            if (GoogleSheetsCustomSettings.scriptableObjects == null)
            {
                Debug.LogError("GoogleSheetsCustomSettings.scriptableObjects is null");
                return null;
            }
            return (from typePair in GoogleSheetsCustomSettings.scriptableObjects where typePair.Key.Name.Contains(typeName) select typePair.Key).FirstOrDefault();
        }
    }
}