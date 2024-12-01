using System;
using System.Collections.Generic;
using UnityEngine;

namespace Editor.Google_Sheets
{
    /// <summary>
    /// Holds a collection of data items fetched from Google Sheets.
    /// </summary>
    [SerializeField]
    public class GoogleSheetsData
    {
        /// <summary>
        /// Represents a collection of data items.
        /// </summary>
        public List<DataItem> items;
    }

    /// <summary>
    ///     Represents a key-value pair for localized text.
    /// </summary>
    [Serializable]
    public class DataItem
    {
        /// <summary>
        ///     Represents a key for localized text.
        /// </summary>
        public int key;

        /// <summary>
        ///     Represents the value of a localized text item.
        /// </summary>
        public string value;
    }
}