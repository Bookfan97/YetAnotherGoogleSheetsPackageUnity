using System.Collections.Generic;
using Editor.Google_Sheets;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Project_Settings
{
    public class GoogleSheetsCustomSettings : ScriptableObject
    {
        [SerializeField] private List<DataItem> m_Data;
        [SerializeField] private string m_spreadsheetID;

        public List<DataItem> Data
        {
            get => m_Data;
            set => m_Data = value;
        }

        public string MSpreadsheetID => m_spreadsheetID;

        public string ClientSecretJsonPath => EditorPrefs.GetString(GoogleSheetsHelper.k_JSONEditorPref, "ClientSecret");

        public string GetPathForSheet(int spreadsheetID) => m_Data.Find(x => x.key == spreadsheetID).value;

        public string GetDefaultPath() => "Assets/Data";
    }
}