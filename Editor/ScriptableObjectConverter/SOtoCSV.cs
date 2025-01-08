using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Editor.Data;
using Editor.Google_Sheets;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Editor.ScriptableObjectConverter
{
    /// <summary>
    /// The SOtoCSV class provides functionality to convert ScriptableObject data
    /// to CSV format and save it to a file. It also supports creating ScriptableObject
    /// instances from CSV data.
    /// </summary>
    public class SOtoCSV
    {
        /// <summary>
        /// Converts CSV data into ScriptableObjects and populates their fields based on the CSV content.
        /// </summary>
        /// <param name="scriptableObjectType">The type of ScriptableObject to create and populate.</param>
        /// <param name="dataItem">The file path to the CSV file containing the data.</param>
        /// <param name="skipPopups">Determines if popups (e.g., progress bars or dialog messages) should be skipped during the operation. Defaults to false.</param>
        public void ScriptableObjectsToCSV(Type scriptableObjectType, DataItem dataItem, bool? skipPopups = false)
        {
            int counter = 0;
            List<string> ids = new List<string>();
            List<string> lines = new List<string>();
            //Load(ResourcesPath, scriptableObjectType);
            List<Object> existingItems = AssetDatabase.FindAssets("t:" + scriptableObjectType.Name).Select(guid => AssetDatabase.GUIDToAssetPath(guid)).Select(path => AssetDatabase.LoadAssetAtPath(path, scriptableObjectType)).Cast<Object>().ToList();

            string[] orderedLines = new string[existingItems.Count];
            JSONUtility.LoadData();
            SheetData sheetData = JSONUtility.GoogleSheetsJsonData.GetSheetData(dataItem.key);
            if(sheetData == null)
            {
                sheetData = new SheetData(dataItem.key, dataItem.assemblyName, scriptableObjectType.Name, "");
                JSONUtility.GoogleSheetsJsonData.AddSheetData(sheetData);
            }
            
            if (GoogleSheetsHelper.GoogleSheetsCustomSettings.ShowDebugLogs)
            {
                Debug.Log($"Found {existingItems.Count} existing objects of type {scriptableObjectType.Name}.");
            }
            Dictionary<int, string> headers = GetHeaders(scriptableObjectType);
            string headerString = "";
            
            foreach (var header in headers)
            {
                headerString += header.Value + ",";
            }
            
            Dictionary<string, string> newScriptableObjects = new Dictionary<string, string>();
            int tempCounter = 0;
            
            foreach (ScriptableObject SOName in existingItems)
            {
                var values = GetValuesFromScriptableObject(SOName, headers);
                // Convert the values into a single CSV-compatible row format
                string csvRow = string.Join(",", values);
                int index;
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(SOName));
                index = sheetData.GetIndexForGUID(guid);

                if (index == -1)
                {
                    newScriptableObjects.Add(guid, csvRow);
                }
                else
                {
                    orderedLines[index - 1] = csvRow;
                    tempCounter++;
                }
                
                if ((bool)!skipPopups)
                {
                    EditorUtility.DisplayProgressBar($"Saving Dialogues", $"Saving Dialogue {counter} / {existingItems.Count}",
                        counter / existingItems.Count);
                    counter++;
                }
            }

            int addedCounter = 1;
            foreach (KeyValuePair<string, string> kvp in newScriptableObjects)
            {
                int lineIndex = tempCounter + addedCounter;
                orderedLines[lineIndex - 1] = kvp.Value;
                sheetData.csvDatas.Add(new CSVData
                {
                    guid = kvp.Key,
                    line = lineIndex,
                });
            }
            
            lines = new List<string>(orderedLines.ToArray());
            SaveToFile(dataItem.value, lines, headerString);
            
            if ((bool)!skipPopups)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Creating Dialogues", $"Saved {counter} of {existingItems.Count} dialogues",
                    "OK", "");
            }
            
            JSONUtility.SaveData();
        }

        /// <summary>
        /// Extracts values from a given ScriptableObject based on a dictionary of header mappings and
        /// returns them as a list of strings, representing its fields or properties.
        /// </summary>
        /// <param name="soName">The ScriptableObject instance from which to extract values.</param>
        /// <param name="headers">A dictionary mapping header indices to field or property names in the ScriptableObject.</param>
        /// <returns>A list of string values corresponding to the specified fields or properties in the ScriptableObject. Returns null if the input is invalid.</returns>
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

        /// <summary>
        /// Retrieves the headers (fields) of a specified ScriptableObject type and maps them to their respective indices.
        /// </summary>
        /// <param name="scriptableObjectType">The type of ScriptableObject to analyze and retrieve the headers from.</param>
        /// <returns>A dictionary where the keys are indices and the values are the names of the fields of the ScriptableObject.</returns>
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

        /// <summary>
        /// Converts a collection of data lines and a set of headers into a single CSV-formatted string.
        /// </summary>
        /// <param name="headers">A string representing the headers for the CSV file, usually separated by a delimiter such as a comma.</param>
        /// <param name="data">A list of data lines where each line corresponds to a row of the CSV file.</param>
        /// <returns>A string containing the headers followed by the data rows, formatted as a CSV.</returns>
        public string ToCSV(string headers, List<string> data)
        {
            var sb = new StringBuilder(headers);
            foreach (var line in data)
            {
                sb.Append('\n').Append(line);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Saves data to a specified file in CSV format using the provided headers and data content.
        /// </summary>
        /// <param name="filePath">The full file path where the CSV content will be saved.</param>
        /// <param name="data">A list of strings representing the data rows to be saved in the file.</param>
        /// <param name="headers">A string representing the headers for the CSV file.</param>
        public void SaveToFile(string filePath, List<string> data, string headers)
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

        /// <summary>
        /// Converts data from a CSV file into ScriptableObjects and populates their fields based on the provided data item.
        /// </summary>
        /// <param name="dataItem">An instance of DataItem containing information about the ScriptableObject type and the path to the CSV file.</param>
        public void ScriptableObjectsToCSV(DataItem dataItem)
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

            if (GoogleSheetsHelper.GoogleSheetsCustomSettings.ShowDebugLogs)
            {
                Debug.Log($"Generating {scriptableObjectType.Name} objects...");
            }

            ScriptableObjectsToCSV(scriptableObjectType, dataItem);
        }
    }
}
