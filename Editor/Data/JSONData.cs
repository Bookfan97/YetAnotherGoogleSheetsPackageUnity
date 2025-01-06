using System.Collections.Generic;
using System.IO;
using System.Linq;
using Editor.Google_Sheets;
using UnityEngine;

namespace Editor.Data
{
    [System.Serializable]
    public class GoogleSheetsJSONData
    {
        [SerializeField] public string assembliesToInclude;
        [SerializeField] public List<SheetData> sheets;

        public void Initialize()
        {
            sheets = new List<SheetData>();
        }
        
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

            sheets.Add(new SheetData(keyValuePair.Key, keyValuePair.Value.assemblyName, keyValuePair.Value.className, keyValuePair.Value.headers));
        }

        public List<CSVData> GetExistingScriptableObjectforSheet(int dataItemKey)
        {
            foreach (var sheetData in sheets.Where(sheetData => sheetData.GetKey() == dataItemKey))
            {
                return sheetData.csvDatas;
            }

            return new List<CSVData>();
        }

        public void SetExistingScriptableObjectforSheet(List<CSVData> csvDatas, int dataItemKey)
        {
            foreach (var sheetData in sheets.Where(sheetData => sheetData.GetKey() == dataItemKey))
            {
                sheetData.csvDatas = csvDatas;
            }
        }

        public SheetData GetSheetData(int dataItemKey) => sheets.FirstOrDefault(sheetData => sheetData.GetKey() == dataItemKey);

        public void AddSheetData(SheetData sheetData)
        {
            sheets.Add(sheetData);
        }

        public void SetSheetData(SheetData newSheetData, int dataItemKey)
        {
            foreach (var sheetData in sheets.Where(sheetData => sheetData.GetKey() == dataItemKey))
            {
                sheetData.SetValues(newSheetData);
                JSONUtility.SaveData();
                return;
            }
        }
    }

    [System.Serializable]
    public class SheetData
    {
        [SerializeField]
        public int key;
        [SerializeField]
        public string assemblyName;
        [SerializeField]
        public string className;
        [SerializeField]
        public string headers;
        [SerializeField]
        public List<CSVData> csvDatas = new List<CSVData>();

        public SheetData(int key, string assemblyName, string className, string headers)
        {
            this.key = key;
            this.assemblyName = assemblyName;
            this.className = className;
            this.headers = headers;
        }

        public int GetKey() => key;

        public void SetValues(SheetData value)
        {
            assemblyName = value.assemblyName;
            className = value.className;
            headers = value.headers;
            csvDatas = value.csvDatas;
        }
    }

    [System.Serializable]
    public class CSVData
    {
        [SerializeField] public string guid;
        [SerializeField] public int line;
        public string GetGUID() => guid;
        public int GetLine() => line;
    }
}