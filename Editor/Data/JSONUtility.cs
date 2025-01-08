using System.IO;
using System.Text;
using Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace Editor.Data
{
    [InitializeOnLoad]
    public static class JSONUtility 
    {
        const string rootPath = "ProjectSettings";
        const string directoryPath = "Packages/com.definitiveinfinitymedia.googlesheets"; 
        const string filename = "Settings.json";
        private static readonly string filePath = $"{rootPath}/{directoryPath}/{filename}";
        
        public static GoogleSheetsJSONData GoogleSheetsJsonData { get; private set; }
        
        static JSONUtility()
        {
            LoadData();
        }

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
        
        public static void SaveData()
        {
            string data = JsonUtility.ToJson(GoogleSheetsJsonData, true);
            File.WriteAllText(filePath, data);
        }

        public static void UpdateAssembliesToInclude(string toString)
        {
            LoadData();
            GoogleSheetsJsonData.UpdateAssembliesToInclude(toString);
            SaveData();
        }
    }
}