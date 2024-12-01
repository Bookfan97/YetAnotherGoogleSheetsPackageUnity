using System.Collections.Generic;
using System.IO;
using Editor.Project_Settings;
using UnityEditor;
using UnityEngine;

namespace Editor.Google_Sheets
{
    public class GoogleSheetsHelper
    {
        private const string k_MyCustomSettingsPath = "Assets/Settings/GoogleSheetsCustomSettings.asset";
        public static string k_JSONEditorPref { get; private set; } = "DefinitiveInfinityMediaGoogleSheetsSettings_JSONPath";

        public static GoogleSheetsCustomSettings GoogleSheetsCustomSettings => GetOrCreateSettings();

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

        internal static SerializedObject GetSerializedSettings() => new(GetOrCreateSettings());
    }
}