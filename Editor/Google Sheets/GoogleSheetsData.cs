using System;
using System.Collections.Generic;
using UnityEngine;

namespace Editor.Google_Sheets
{
    /// <summary>
    ///     Represents a key-value pair for Google Sheets data.
    /// </summary>
    [Serializable]
    public class DataItem
    {
        /// <summary>
        /// Represents the unique identifier or key in a key-value pair for Google Sheets data.
        /// </summary>
        public int key;
        public int index;

        /// <summary>
        /// Represents a value corresponding to a key in a key-value pair for Google Sheets data.
        /// </summary>
        public string value;
        
        public ScriptableObject scriptableObject;
    }
}