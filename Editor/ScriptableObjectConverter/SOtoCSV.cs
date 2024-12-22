using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Editor.Google_Sheets;
using UnityEditor;
using UnityEngine;

namespace Editor.ScriptableObjectConverter
{
    public class SOtoCSV
    {
        /*
     * Gets all DialogueSO objects and converts data to CSV format
     */
        public void CSVtoScriptableObjects(Type scriptableObjectType, string csvPath, bool? skipPopups = false)
        {
            int counter = 0;
            List<string> lines = new List<string>();
            List<string> ids = new List<string>();
            var existingItems = Resources.FindObjectsOfTypeAll(scriptableObjectType);
            Debug.Log($"Found {existingItems.Length} existing objects of type {scriptableObjectType.Name}.");
            Dictionary<int, string> headers = GetHeaders(scriptableObjectType);
            string headerString = "";
            
            foreach (var header in headers)
            {
                headerString += header.Value + ",";
            }
            
            foreach (ScriptableObject SOName in existingItems)
            {
                var values = GetValuesFromScriptableObject(SOName, headers);
                // Convert the values into a single CSV-compatible row format
                string csvRow = string.Join(",", values);

                // Add the row to the output list (assuming `lines` is accessible here)
                lines.Add(csvRow);
                
                if ((bool)!skipPopups)
                {
                    EditorUtility.DisplayProgressBar($"Saving Dialogues", $"Saving Dialogue {counter} / {existingItems.Length}",
                        counter / existingItems.Length);
                    counter++;
                }
            }
            
            SaveToFile(csvPath, lines, headerString);
            if ((bool)!skipPopups)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Creating Dialogues", $"Saved {counter} of {existingItems.Length} dialogues",
                    "OK", "");
            }
        }

        private List<string> GetValuesFromScriptableObject(ScriptableObject soName, Dictionary<int, string> headers)
        {
            if (soName == null || headers == null || headers.Count == 0)
            {
                Debug.LogError(
                    "Invalid input to GetValuesFromScriptableObject. ScriptableObject or headers are null/empty.");
                return null;
            }

            List<string> values = new List<string>();

            // Iterate through each header (field/property name) and get corresponding value
            foreach (var header in headers)
            {
                var fieldName = header.Value; // The field name from headers dictionary
                var fieldInfo = soName.GetType().GetField(fieldName); // Get field by name using reflection
                var propertyInfo =
                    soName.GetType().GetProperty(fieldName); // If it's a property, use reflection to get it

                object value = null;

                if (fieldInfo != null)
                {
                    value = fieldInfo.GetValue(soName); // Get the value of the field
                }
                else if (propertyInfo != null)
                {
                    value = propertyInfo.GetValue(soName); // Get the value of the property
                }

                // If value is null, use an empty string as a placeholder
                values.Add(value != null ? value.ToString() : string.Empty);
            }

            return values;
        }

        private Dictionary<int, string> GetHeaders(Type scriptableObjectType)
        {
            // Dictionary to hold index and names
            Dictionary<int, string> headers = new Dictionary<int, string>();

            // Use reflection to get all fields and properties of the ScriptableObject type
            var fields = scriptableObjectType.GetFields();
            var properties = scriptableObjectType.GetProperties();

            int index = 0;

            // Add all fields to the headers dictionary
            foreach (var field in fields)
            {
                headers.Add(index++, field.Name);
            }

            /*// Add all properties to the headers dictionary
            foreach (var property in properties)
            {
                headers.Add(index++, property.Name);
            }*/

            return headers;
        }

        /*
         * Converts data passed through to CSV formatting and returns the modified string
         */
        public string ToCSV(string headers, List<string> data)
        {
            var sb = new StringBuilder(headers);
            foreach(var line in data)
            {
                sb.Append('\n').Append(line);
            }

            return sb.ToString();
        }
    
        /*
     * Saves the converted data back out to the CSV file path passed through
     */
        public void SaveToFile (string filePath, List<string> data, string headers)
        {
            try
            {
                var content = ToCSV(headers, data);
#if UNITY_EDITOR
                var folder = Application.streamingAssetsPath;

                if(!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
#else
    var folder = Application.persistentDataPath;
#endif

                string localPath = filePath.Replace($"{Application.dataPath}/", "");
                localPath = "Assets/" + localPath;
                File.WriteAllText(filePath, content);

#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void CSVtoScriptableObjects(DataItem dataItem)
        {
            if (string.IsNullOrEmpty(dataItem.scriptableObjectType))
            {
                Debug.LogError("scriptableObjectType is null or empty.");
                return;
            }

            // Attempt to resolve the type
            Type scriptableObjectType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.Name == dataItem.scriptableObjectType);

            if (scriptableObjectType == null)
            {
                Debug.LogError(
                    $"Failed to resolve scriptableObjectType: {dataItem.scriptableObjectType}. Make sure the type name is fully qualified and the assembly is loaded.");
                return;
            }

            // Check if the resolved type is a ScriptableObject
            if (!typeof(ScriptableObject).IsAssignableFrom(scriptableObjectType))
            {
                Debug.LogError($"Resolved type {scriptableObjectType} is not a ScriptableObject.");
                return;
            }

            if (string.IsNullOrEmpty(dataItem.value) || !File.Exists(dataItem.value))
            {
                Debug.LogError("Invalid file path provided for CSV generation.");
                return;
            }

            Debug.Log($"Generating {scriptableObjectType.Name} objects...");
            CSVtoScriptableObjects(scriptableObjectType, dataItem.value);
        }
    }
}
