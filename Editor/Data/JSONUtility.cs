using System.IO;
using System.Text;
using Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace Editor.Data
{
    /// <summary>
    /// Utility class for handling JSON data related to Google Sheets in Unity editor.
    /// </summary>
    [InitializeOnLoad]
    public static class JSONUtility
    {
        /// <summary>
        /// Path to the root directory for project settings.
        /// </summary>
        const string rootPath = "ProjectSettings";

        /// <summary>
        /// Path to the directory containing the Google Sheets package.
        /// </summary>
        const string directoryPath = "Packages/com.definitiveinfinitymedia.googlesheets";

        /// <summary>
        /// Name of the settings file.
        /// </summary>
        const string filename = "Settings.json";

        /// <summary>
        /// Full path to the settings file.
        /// </summary>
        private static readonly string filePath = $"{rootPath}/{directoryPath}/{filename}";

        /// <summary>
        /// Data object representing the Google Sheets JSON data.
        /// </summary>
        public static GoogleSheetsJSONData GoogleSheetsJsonData { get; private set; }

        /// <summary>
        /// Static constructor to initialize the JSON utility by loading data.
        /// </summary>
        static JSONUtility()
        {
            LoadData();
        }

        /// <summary>
        /// Loads the Google Sheets JSON data from the settings file.
        /// If the file or directories do not exist, they are created and initialized with default data.
        /// </summary>
        public static void LoadData()
        {
            if (!Directory.Exists($"{rootPath}/Packages"))
            {
                Directory.CreateDirectory($"{rootPath}/Packages");
            }

            if (!Directory.Exists($"{rootPath}/Packages/com.definitiveinfinitymedia.googlesheets"))
            {
                Directory.CreateDirectory($"{rootPath}/Packages/com.definitiveinfinitymedia.googlesheets");
            }

            if (!File.Exists(filePath))
            {
                FileStream fs = new FileStream(filePath, FileMode.Create);

                GoogleSheetsJsonData = new GoogleSheetsJSONData();

                string worldDataString = JsonUtility.ToJson(GoogleSheetsJsonData, true);

                byte[] worldDataBytes = Encoding.UTF8.GetBytes(worldDataString);

                fs.Write(worldDataBytes);

                fs.Close();
                GoogleSheetsJsonData.Initialize();
            }
            else
            {
                string data = File.ReadAllText(filePath);
                GoogleSheetsJsonData = JsonUtility.FromJson<GoogleSheetsJSONData>(data);
            }
        }

        /// <summary>
        /// Saves the current Google Sheets JSON data to the settings file.
        /// </summary>
        public static void SaveData()
        {
            string data = JsonUtility.ToJson(GoogleSheetsJsonData, true);
            File.WriteAllText(filePath, data);
        }

        /// <summary>
        /// Updates the assemblies to include in the Google Sheets JSON data and saves the changes.
        /// </summary>
        /// <param name="toString">The string representation of the assemblies to include.</param>
        public static void UpdateAssembliesToInclude(string toString)
        {
            LoadData();
            GoogleSheetsJsonData.UpdateAssembliesToInclude(toString);
            SaveData();
        }
    }
}
