using System.Collections.Generic;
using Editor.Google_Sheets;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Project_Settings
{
    /// <summary>
    /// Represents the configuration settings necessary for integrating Google Sheets within a Unity project.
    /// Manages various parameters like spreadsheet ID and data items required for handling Google Sheets operations.
    /// </summary>
    public class GoogleSheetsCustomSettings : ScriptableObject
    {
        /// <summary>
        /// 
        /// </summary>
        [SerializeField] private List<DataItem> m_Data;

        /// <summary>
        /// Holds the unique identifier of the Google Spreadsheet being accessed or managed.
        /// This identifier is used to authenticate and reference the specific spreadsheet
        /// for various operations like reading from or writing to the Google Sheets within
        /// the Unity project.
        /// </summary>
        [SerializeField] private string m_spreadsheetID;

        /// <summary>
        /// Specifies a semicolon-separated list of assembly names to be included
        /// during the runtime compilation or evaluation process of the Google Sheets integration.
        /// This setting allows for dynamic referencing of specific assemblies required for
        /// the tailored handling of spreadsheet data within the Unity project.
        /// </summary>
        public string assembliesToInclude { get; set; }

        /// <summary>
        /// Provides access to the collection of data items utilized for Google Sheets operations
        /// within the Unity project. Each item in the list represents a key-value pair that
        /// facilitates the interaction and data exchange between the application and the Google Sheets.
        /// </summary>
        public List<DataItem> Data
        {
            get => m_Data;
            set => m_Data = value;
        }

        /// <summary>
        /// Provides access to the unique identifier of the current Google Spreadsheet.
        /// This identifier is essential for executing operations such as opening, updating,
        /// or downloading data from the spreadsheet within the Unity editor environment.
        /// </summary>
        public string MSpreadsheetID => m_spreadsheetID;

        /// <summary>
        /// Specifies the file path to the JSON file containing the client secret credentials
        /// required for authenticating API requests to Google Sheets within the Unity project.
        /// This path is used to locate the necessary credentials for establishing authorized
        /// access from the Unity editor or application.
        /// </summary>
        public string ClientSecretJsonPath =>
            EditorPrefs.GetString(GoogleSheetsHelper.k_JSONEditorPref, "ClientSecret");

        /// <summary>
        /// Retrieves the file path associated with the specified spreadsheet ID.
        /// </summary>
        /// <param name="spreadsheetID">The unique identifier of the spreadsheet.</param>
        /// <returns>The file path as a string linked to the provided spreadsheet ID.</returns>
        public string GetPathForSheet(int spreadsheetID) => m_Data.Find(x => x.key == spreadsheetID).value;

        /// <summary>
        /// Retrieves the default file path used for storing data.
        /// </summary>
        /// <returns>The string representing the default path for data assets.</returns>
        public string GetDefaultPath() => "Assets/Data";
    }
}