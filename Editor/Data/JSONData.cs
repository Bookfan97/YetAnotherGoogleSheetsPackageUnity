using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Editor.Google_Sheets;
using UnityEngine;

namespace Editor.Data
{
    /// <summary>
    /// Container class that contains data for all scriptable object types linked to Google Sheets
    /// </summary>
    [System.Serializable]
    public class GoogleSheetsJSONData
    {
        /// <summary>
        /// Assemblies to search for classes in
        /// </summary>
        [SerializeField] public string assembliesToInclude;

        /// <summary>
        /// A list of all sheetdata
        /// </summary>
        [SerializeField] public List<SheetData> sheets;

        /// <summary>
        /// Initialization method
        /// </summary>
        public void Initialize()
        {
            sheets = new List<SheetData>();
        }

        /// <summary>
        /// Adds a new key-value pair to the sheets list or updates an existing one
        /// </summary>
        /// <param name="keyValuePair">The key-value pair to add or update</param>
        public void Add(KeyValuePair<int, SheetData> keyValuePair)
        {
            if (keyValuePair.Value != null)
            {
                for (int i = 0; i < sheets.Count; i++)
                {
                    if (sheets[i].GetKey() == keyValuePair.Key)
                    {
                        sheets.RemoveAt(i);
                        return;
                    }
                }
            }

            foreach (var t in sheets.Where(t => t.GetKey() == keyValuePair.Key))
            {
                t.SetValues(keyValuePair.Value);
                return;
            }

            sheets.Add(new SheetData(keyValuePair.Key, keyValuePair.Value.assemblyName, keyValuePair.Value.className,
                keyValuePair.Value.headers));
        }

        /// <summary>
        /// Gets the existing scriptable objects for a specific sheet
        /// </summary>
        /// <param name="dataItemKey">The key of the sheet</param>
        /// <returns>A list of CSVData representing the existing scriptable objects</returns>
        public List<CSVData> GetExistingScriptableObjectforSheet(int dataItemKey)
        {
            foreach (var sheetData in sheets.Where(sheetData => sheetData.GetKey() == dataItemKey))
            {
                return sheetData.csvDatas;
            }

            return new List<CSVData>();
        }

        /// <summary>
        /// Sets the existing scriptable objects for a specific sheet
        /// </summary>
        /// <param name="csvDatas">The list of CSVData to set</param>
        /// <param name="dataItemKey">The key of the sheet</param>
        public void SetExistingScriptableObjectforSheet(List<CSVData> csvDatas, int dataItemKey)
        {
            foreach (var sheetData in sheets.Where(sheetData => sheetData.GetKey() == dataItemKey))
            {
                sheetData.csvDatas = csvDatas;
            }
        }

        /// <summary>
        /// Gets the sheet data for a specific key
        /// </summary>
        /// <param name="dataItemKey">The key of the sheet</param>
        /// <returns>The SheetData for the specified key</returns>
        public SheetData GetSheetData(int dataItemKey) =>
            sheets.FirstOrDefault(sheetData => sheetData.GetKey() == dataItemKey);

        /// <summary>
        /// Adds a new sheet data to the list
        /// </summary>
        /// <param name="sheetData">The SheetData to add</param>
        public void AddSheetData(SheetData sheetData)
        {
            sheets.Add(sheetData);
        }

        /// <summary>
        /// Sets the sheet data for a specific key
        /// </summary>
        /// <param name="newSheetData">The new SheetData to set</param>
        /// <param name="dataItemKey">The key of the sheet</param>
        public void SetSheetData(SheetData newSheetData, int dataItemKey)
        {
            foreach (var sheetData in sheets.Where(sheetData => sheetData.GetKey() == dataItemKey))
            {
                sheetData.SetValues(newSheetData);
                JSONUtility.SaveData();
                return;
            }
        }

        /// <summary>
        /// Checks if a sheet ID exists in the sheets list
        /// </summary>
        /// <param name="propertiesSheetId">The sheet ID to check</param>
        /// <returns>True if the sheet ID exists, otherwise false</returns>
        public bool CheckSheetIDInSheets(int? propertiesSheetId) =>
            propertiesSheetId != null && sheets.Any(sheet => sheet.GetKey() == propertiesSheetId);

        /// <summary>
        /// Updates the assemblies to include
        /// </summary>
        /// <param name="toString">The new assemblies to include</param>
        public void UpdateAssembliesToInclude(string toString)
        {
            assembliesToInclude = toString;
        }
    }

    /// <summary>
    /// Represents data for a single sheet
    /// </summary>
    [System.Serializable]
    public class SheetData
    {
        /// <summary>
        /// The key of the sheet
        /// </summary>
        [SerializeField] public int key;

        /// <summary>
        /// The name of the assembly
        /// </summary>
        [SerializeField] public string assemblyName;

        /// <summary>
        /// The name of the class
        /// </summary>
        [SerializeField] public string className;

        /// <summary>
        /// The headers of the sheet
        /// </summary>
        [SerializeField] public string headers;

        /// <summary>
        /// The list of CSV data
        /// </summary>
        [SerializeField] public List<CSVData> csvDatas = new List<CSVData>();

        /// <summary>
        /// Initializes a new instance of the SheetData class
        /// </summary>
        /// <param name="key">The key of the sheet</param>
        /// <param name="assemblyName">The name of the assembly</param>
        /// <param name="className">The name of the class</param>
        /// <param name="headers">The headers of the sheet</param>
        public SheetData(int key, string assemblyName, string className, string headers)
        {
            this.key = key;
            this.assemblyName = assemblyName;
            this.className = className;
            this.headers = headers;
        }

        /// <summary>
        /// Gets the key of the sheet
        /// </summary>
        /// <returns>The key of the sheet</returns>
        public int GetKey() => key;

        /// <summary>
        /// Sets the values of the sheet data
        /// </summary>
        /// <param name="value">The new values to set</param>
        public void SetValues(SheetData value)
        {
            assemblyName = value.assemblyName;
            className = value.className;
            headers = value.headers;
            csvDatas = value.csvDatas;
        }

        /// <summary>
        /// Checks if a GUID exists in the CSV data
        /// </summary>
        /// <param name="i">The line number to check</param>
        /// <returns>True if the GUID exists, otherwise false</returns>
        public bool CheckGUID(int i) => csvDatas.Any(data => data.line == i);

        /// <summary>
        /// Checks the saved GUID for a specific line
        /// </summary>
        /// <param name="i">The line number to check</param>
        /// <returns>The GUID if it exists, otherwise an empty string</returns>
        public string CheckSavedGUID(int i)
        {
            foreach (var data in csvDatas)
            {
                if (data.line == i)
                {
                    return data.guid;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the index for a specific GUID
        /// </summary>
        /// <param name="guid">The GUID to check</param>
        /// <returns>The index of the GUID if it exists, otherwise -1</returns>
        public int GetIndexForGUID(string guid)
        {
            foreach (var data in csvDatas)
            {
                if (data.guid == guid)
                {
                    return data.line;
                }
            }

            return -1;
        }
    }

    /// <summary>
    /// Represents data for a single CSV entry
    /// </summary>
    [System.Serializable]
    public class CSVData
    {
        /// <summary>
        /// The GUID of the CSV entry
        /// </summary>
        [SerializeField] public string guid;

        /// <summary>
        /// The line number of the CSV entry
        /// </summary>
        [SerializeField] public int line;

        /// <summary>
        /// Gets the GUID of the CSV entry
        /// </summary>
        /// <returns>The GUID of the CSV entry</returns>
        public string GetGUID() => guid;

        /// <summary>
        /// Gets the line number of the CSV entry
        /// </summary>
        /// <returns>The line number of the CSV entry</returns>
        public int GetLine() => line;
    }
}