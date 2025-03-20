using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Editor.Data;
using Editor.Google_Sheets;
using Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace Editor.ScriptableObjectConverter
{
    /// <summary>
    ///     A utility class for converting CSV data into Unity ScriptableObjects.
    /// </summary>
    /// <remarks>
    ///     This class reads CSV files and creates instances of the specified ScriptableObject type based on the data in the
    ///     file.
    ///     It provides functionality to handle the conversion process, ensuring that types and file paths are correctly
    ///     validated.
    /// </remarks>
    public class CSVtoSO
    {
        /// <summary>
        ///     A dictionary that stores mapping between original identifiers and their corresponding unique GUIDs.
        /// </summary>
        /// <remarks>
        ///     This property is used during the conversion process to ensure each generated ScriptableObject has a unique
        ///     identifier.
        ///     The key represents the original identifier (e.g., from the CSV data), and the value is the assigned GUID.
        ///     These mappings are preserved for potential reuse during subsequent operations or validations.
        /// </remarks>
        private const int ValidGuidLength = 32;

        private const string DefaultDataFolderPath = "Assets/Data";
        private static readonly Regex CSVParser = new(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
        private Dictionary<string, ScriptableObject> AddedIDs { get; set; } // GUID, SO

        /// <summary>
        ///     Indicates whether the CSV to ScriptableObject conversion process was successful.
        /// </summary>
        /// <remarks>
        ///     This property reflects the outcome of the most recent conversion operation.
        ///     It is set to true if the conversion completes at least one ScriptableObject creation successfully,
        ///     even if there are warnings or skipped rows due to mismatched data. A value of false usually indicates
        ///     either a failure to create any ScriptableObjects or errors that interrupted the process.
        /// </remarks>
        public bool Success { get; private set; }

        /// <summary>
        ///     Generates ScriptableObject instances from data stored in a CSV file.
        /// </summary>
        /// <param name="type">The type of ScriptableObject to create. Must inherit from ScriptableObject.</param>
        /// <param name="csvFile">The file path of the CSV file containing the data to populate the ScriptableObjects.</param>
        /// <param name="dataItemKey"></param>
        private void GenerateScriptableObjects(Type type, string csvFile, int dataItemKey)
        {
            JSONUtility.LoadData();
            var dataItems = new Dictionary<ScriptableObject, CSVData>();
            var sheetData = JSONUtility.GoogleSheetsJsonData.GetSheetData(dataItemKey);
            if (sheetData == null)
            {
                sheetData = new SheetData(dataItemKey, type.AssemblyQualifiedName, type.Name, "");
                JSONUtility.GoogleSheetsJsonData.AddSheetData(sheetData);
            }

            var csvDatas = JSONUtility.GoogleSheetsJsonData.GetExistingScriptableObjectforSheet(dataItemKey);
            var scriptableObjectforSheet =
                GoogleSheetsHelper.GoogleSheetsCustomSettings.GetScriptableObjectforSheet(dataItemKey);
            var lines = File.ReadAllText(csvFile).Split("\n");

            if (lines.Length <= 1)
            {
                Debug.LogError("CSV file does not contain any valid data.");
                return;
            }

            if (lines[^1] == "")
            {
                var lineList = new List<string>();
                lineList = lines.ToList();
                lineList.RemoveAt(lines.Length - 1);
                lines = lineList.ToArray();
            }

            int counter = 0, successCounter = 0;

            // Detect existing instances of the type
            var existingItems = Resources.FindObjectsOfTypeAll(type);

            /*foreach (var existingItem in existingItems)
            {
                string assetPath = AssetDatabase.GetAssetPath(existingItem);
                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                AddedIDs.Add(guid, (ScriptableObject) existingItem);
            }*/

            if (GoogleSheetsHelper.GoogleSheetsCustomSettings.ShowDebugLogs)
                Debug.Log($"Found {existingItems.Length} existing objects of type {type.Name}.");

            var header = lines[0];
            sheetData.headers = header;
            var headerData = header.Split(',');
            if (headerData[^1] == "")
            {
                var headerDataList = new List<string>();
                headerDataList = headerData.ToList();
                headerDataList.RemoveAt(headerData.Length - 1);
                headerData = headerDataList.ToArray();
            }

            // Reflection: Map CSV fields to ScriptableObject fields or properties
            var fields = type.GetFields();
            var properties = type.GetProperties();
            var fieldMap = new Dictionary<int, string>();

            for (var i = 0; i < headerData.Length; i++)
            {
                var columnName = headerData[i].Trim();
                var matchingField =
                    fields.FirstOrDefault(f => f.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                var matchingProperty =
                    properties.FirstOrDefault(p => p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                if (matchingField != null)
                {
                    fieldMap.Add(i, matchingField.Name);
                    if (GoogleSheetsHelper.GoogleSheetsCustomSettings.ShowDebugLogs)
                        Debug.Log($"Matched field: {matchingField.Name}");
                }
                else if (matchingProperty != null)
                {
                    fieldMap.Add(i, matchingProperty.Name);
                    if (GoogleSheetsHelper.GoogleSheetsCustomSettings.ShowDebugLogs)
                        Debug.Log($"Matched property: {matchingProperty.Name}");
                }
                else
                {
                    Debug.LogWarning($"No matching field or property found for column: {headerData[i]}");
                }
            }

            try
            {
                for (var i = 1; i < lines.Length; i++)
                {
                    var isNewSO = false;
                    var line = lines[i].Trim();

                    counter++;
#if UNITY_EDITOR
                    EditorUtility.DisplayProgressBar($"Creating {type.Name}",
                        $"Processing line {counter} of {lines.Length - 1}",
                        (float)counter / (lines.Length - 1));
#endif

                    var splitData = line.ParseLine();
                    //CSVParser.Split(line).Select(field => field.Trim('\"')).ToArray();

                    if (splitData.Length != headerData.Length)
                    {
                        Debug.LogWarning(
                            $"Line {i} has mismatched column count. Expected {headerData.Length}, got {splitData.Length}. Skipping...");
                        continue;
                    }

                    var loadedGUID = sheetData.CheckSavedGUID(i);
                    isNewSO = loadedGUID == string.Empty;
                    var scriptableObject = loadedGUID != string.Empty
                        ? AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(loadedGUID))
                        : ScriptableObject.CreateInstance(type);
                    //var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(scriptableObject));
                    var csvData = loadedGUID == string.Empty
                        ? new CSVData()
                        : csvDatas.FirstOrDefault(data => data.GetGUID() == loadedGUID);
                    csvData.line = i;
                    dataItems.Add(scriptableObject, csvData);

                    scriptableObjectforSheet[i] = line;
                    for (var columnIndex = 0; columnIndex < splitData.Length; columnIndex++)
                    {
                        if (!fieldMap.TryGetValue(columnIndex, out var mappedField))
                        {
                            Debug.LogWarning(
                                $"Skipping unmapped column at index {columnIndex}: {headerData[columnIndex]}");
                            continue;
                        }

                        try
                        {
                            var field = type.GetField(mappedField);
                            var property = type.GetProperty(mappedField);

                            if (field != null)
                            {
                                if (field.FieldType == typeof(Vector2))
                                {
                                    var stringValue = splitData[columnIndex];
                                    stringValue = stringValue.Replace("(", "").Replace(")", "");
                                    var temp = stringValue.Split(',');
                                    var floatx = Convert.ToSingle(temp[0]);
                                    var floaty = Convert.ToSingle(temp[1]);
                                    var vectorValue = new Vector2(floatx, floaty);
                                    field.SetValue(scriptableObject, vectorValue);
                                }
                                else if (field.FieldType == typeof(Vector3))
                                {
                                    var stringValue = splitData[columnIndex];
                                    stringValue = stringValue.Replace("(", "").Replace(")", "");
                                    var temp = stringValue.Split(',');
                                    var floatx = Convert.ToSingle(temp[0]);
                                    var floaty = Convert.ToSingle(temp[1]);
                                    var floatz = Convert.ToSingle(temp[2]);
                                    var vectorValue = new Vector3(floatx, floaty, floatz);
                                    field.SetValue(scriptableObject, vectorValue);
                                }
                                //Skip for now
                                else if (field.FieldType == typeof(Array) || field.FieldType.IsArray ||
                                         field.FieldType.BaseType == typeof(Enum))
                                {
                                }
                                else
                                {
                                    var value = Convert.ChangeType(splitData[columnIndex], field.FieldType);
                                    field.SetValue(scriptableObject, value);
                                }
                            }
                            else if (property != null && property.CanWrite)
                            {
                                if (property.PropertyType == typeof(Vector2))
                                {
                                    var stringValue = splitData[columnIndex];
                                    stringValue = stringValue.Replace("(", "").Replace(")", "");
                                    var temp = stringValue.Split(',');
                                    var floatx = Convert.ToSingle(temp[0]);
                                    var floaty = Convert.ToSingle(temp[1]);
                                    var vectorValue = new Vector2(floatx, floaty);
                                    property.SetValue(scriptableObject, vectorValue);
                                }
                                else if (property.PropertyType == typeof(Vector3))
                                {
                                    var stringValue = splitData[columnIndex];
                                    stringValue = stringValue.Replace("(", "").Replace(")", "");
                                    var temp = stringValue.Split(',');
                                    var floatx = Convert.ToSingle(temp[0]);
                                    var floaty = Convert.ToSingle(temp[1]);
                                    var floatz = Convert.ToSingle(temp[2]);
                                    var vectorValue = new Vector3(floatx, floaty, floatz);
                                    property.SetValue(scriptableObject, vectorValue);
                                }
                                //Skip for now
                                else if (property.PropertyType == typeof(Array) || property.PropertyType.IsArray ||
                                         property.PropertyType == typeof(Enum))
                                {
                                }
                                else
                                {
                                    var value = Convert.ChangeType(splitData[columnIndex], property.PropertyType);
                                    property.SetValue(scriptableObject, value);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(
                                $"Error setting value for field/property {mappedField} on line {i}: {ex.Message}");
                        }
                    }

                    Success = true;
                    successCounter++;

                    if (!AssetDatabase.IsValidFolder(DefaultDataFolderPath))
                    {
                        var parentFolder = Path.GetDirectoryName(DefaultDataFolderPath);
                        AssetDatabase.CreateFolder(parentFolder, Path.GetFileName(DefaultDataFolderPath));
                    }

                    var subfolder = type.Name;
                    if (!AssetDatabase.IsValidFolder($"{DefaultDataFolderPath}/{subfolder}"))
                    {
                        var parentFolder = Path.GetDirectoryName($"{DefaultDataFolderPath}/{subfolder}");
                        AssetDatabase.CreateFolder(parentFolder,
                            Path.GetFileName($"{DefaultDataFolderPath}/{subfolder}"));
                    }

                    if (isNewSO)
                    {
                        var assetPath = $"{DefaultDataFolderPath}/{subfolder}/{type.Name}_{i}.asset";
                        AssetDatabase.CreateAsset(scriptableObject, assetPath);
                        if (GoogleSheetsHelper.GoogleSheetsCustomSettings.ShowDebugLogs)
                            Debug.Log($"Created ScriptableObject: {scriptableObject.name} at {assetPath}");
                    }

                    csvDatas.Add(csvData);
                }

                AssetDatabase.SaveAssets();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error generating ScriptableObjects: {ex.Message}");
            }
            finally
            {
                AssetDatabase.SaveAssets();
#if UNITY_EDITOR
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("ScriptableObjects Creation Complete",
                    $"Successfully created {successCounter} out of {counter} objects.", "OK");
#endif
                var csvDatasToSave = new List<CSVData>();
                foreach (var dataItem in dataItems)
                {
                    dataItem.Value.guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(dataItem.Key));
                    csvDatasToSave.Add(dataItem.Value);
                }

                sheetData.csvDatas.Clear();
                sheetData.csvDatas.AddRange(csvDatasToSave);
                JSONUtility.GoogleSheetsJsonData.SetSheetData(sheetData, dataItemKey);
            }
        }

        /// <summary>
        ///     Generates ScriptableObject instances using the specified data item information.
        /// </summary>
        /// <param name="dataItem">
        ///     The data item containing the scriptableObjectType and CSV file path required for generating the
        ///     ScriptableObjects.
        /// </param>
        public void Generate(DataItem dataItem)
        {
            if (string.IsNullOrEmpty(dataItem.scriptableObjectType))
            {
                Debug.LogError("scriptableObjectType is null or empty.");
                return;
            }

            var scriptableObjectType = GoogleSheetsHelper.CheckType(dataItem.scriptableObjectType);

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


            GenerateScriptableObjects(scriptableObjectType, dataItem.value, dataItem.key);
        }
    }
}