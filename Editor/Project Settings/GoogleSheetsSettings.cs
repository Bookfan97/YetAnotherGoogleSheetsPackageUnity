using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Editor.Project_Settings
{
    /// <summary>
    /// Provides settings related to Google Sheets integration within the Unity Editor.
    /// </summary>
    [FilePath("ProjectSettings/SceneSelectionOverlaySettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class GoogleSheetsSettings : ScriptableSingleton<GoogleSheetsSettings>
    {
        /// <summary>
        /// The path where Google Sheets settings data is stored.
        /// This path is used to read from and write to the settings file.
        /// </summary>
        private static readonly string dataPath = "ProjectSettings/GoogleSheets.txt";

        /// <summary>
        /// Lock object used to synchronize access to the settings file, ensuring thread safety
        /// during read and write operations.
        /// </summary>
        private static readonly object _fileLock = new();

        /// <summary>
        /// Stores the unique identifier of the Google Spreadsheet.
        /// This ID is used to access and interact with the specific Google Spreadsheet
        /// configured for data management within the Unity Editor.
        /// </summary>
        [SerializeField] private string m_spreadsheetID;

        /// <summary>
        /// The key used to store and retrieve the Google Sheets Client Secret JSON path in Editor preferences.
        /// This key ensures the correct path is stored for accessing Google Sheets API.
        /// </summary>
        public string k_JSONEditorPref = "GoogleSheetsSettings_JSONPath";

        /// <summary>
        /// A dictionary that stores various settings related to Google Sheets integration.
        /// Keys represent the setting names, and values represent the corresponding setting values.
        /// Used internally to manage and persist Google Sheets configuration data.
        /// </summary>
        private Dictionary<string, string> m_settings;

        /// <summary>
        /// The path to the Client Secret JSON file required for Google Sheets API authentication.
        /// This path is retrieved from the Editor preferences and is essential for accessing Google Sheets services.
        /// </summary>
        public string ClientSecretJsonPath => EditorPrefs.GetString(k_JSONEditorPref);

        /// <summary>
        /// The unique identifier for the Google Spreadsheet.
        /// This ID is needed to access, download, and upload data
        /// to the Google Sheets document associated with the project.
        /// </summary>
        public string SpreadsheetID
        {
            get => ReadFromFile("SpreadsheetID");
            set
            {
                Save(true);
                WriteToFile("SpreadsheetID", value);
            }
        }

        /// <summary>
        /// The file path to the CSV file generated from the Google Sheet spreadsheet.
        /// This path is used to read from and write the CSV file during upload and download operations.
        /// </summary>
        public string CsvPath
        {
            get => ReadFromFile("CsvPath");
            set
            {
                Save(true);
                var relativepath = "";
                if (value.StartsWith(Application.dataPath))
                    relativepath = "Assets" + value.Substring(Application.dataPath.Length);
                WriteToFile("CsvPath", relativepath);
            }
        }

        /// <summary>
        /// Returns the default file path used in GoogleSheetsSettings.
        /// </summary>
        /// <returns>A string representing the default path, "Assets/Data".</returns>
        public string GetDefaultPath()
        {
            return "Assets/Data";
        }

        /// <summary>
        /// Writes a key-value pair to the settings file. The key represents a title, and the value is the associated setting.
        /// </summary>
        /// <param name="title">The title or key to identify the setting.</param>
        /// <param name="value">The value associated with the title to be written to the file.</param>
        private void WriteToFile(string title, string value)
        {
            lock (_fileLock)
            {
                if (!File.Exists(dataPath))
                {
                    using (var fs = new FileStream(dataPath, FileMode.Create))
                    {
                        var key = $"{title}: {value}";
                        var info = new UTF8Encoding(true).GetBytes(key);
                        fs.Write(info, 0, info.Length);
                    }
                }
                else
                {
                    var worldDataString = "";
                    if (m_settings == null) InitializeDictionary();

                    if (m_settings != null && !string.IsNullOrEmpty(title) && value != null)
                    {
                        var tempSettings = new Dictionary<string, string>(m_settings);
                        var builder = new StringBuilder();

                        foreach (var setting in tempSettings)
                            if (setting.Key == title)
                            {
                                builder.AppendLine($"{title}: {value}");
                                if (m_settings.TryGetValue(title, out var settingValue))
                                    m_settings[title] = value; // Directly update the value
                            }
                            else
                            {
                                builder.AppendLine($"{setting.Key}: {setting.Value}");
                            }

                        worldDataString = builder.ToString();
                    }

                    File.WriteAllText(dataPath, worldDataString);
                }
            }
        }

        /// <summary>
        /// Initializes the dictionary `m_settings` with key-value pairs
        /// by reading them from the file specified by `dataPath`.
        /// Each line in the file is expected to contain a key and value
        /// separated by a colon (`:`).
        /// </summary>
        private void InitializeDictionary()
        {
            m_settings = new Dictionary<string, string>();
            var lines = File.ReadAllLines(dataPath);
            foreach (var line in lines)
            {
                var separation = line.IndexOf(":", StringComparison.Ordinal);
                var key = line.Substring(0, separation);
                var value = line.Replace($"{key}: ", "");
                m_settings[key] = value;
            }
        }

        /// <summary>
        /// Reads the value associated with the specified title from the settings file.
        /// If the file does not exist or the title is not found, a default value is returned.
        /// </summary>
        /// <param name="title">The title identifying the specific setting to be read.</param>
        /// <returns>A string representing the value associated with the specified title.</returns>
        private string ReadFromFile(string title)
        {
            var value = string.Empty;
            if (!File.Exists(dataPath)) return value;

            var lines = File.ReadAllLines(dataPath);

            if (lines.Length == 0)
            {
                if (m_settings == null) InitializeDictionary();

                if (m_settings != null && m_settings.TryGetValue(title, out var setting))
                    value = setting;
                else
                    m_settings?.Add(title, GetDefaultPath());
            }
            else
            {
                foreach (var line in lines)
                    if (line.Split(':')[0] == title)
                        value = line.Split(':')[1].Replace(" ", "");
            }

            return value;
        }
    }
}