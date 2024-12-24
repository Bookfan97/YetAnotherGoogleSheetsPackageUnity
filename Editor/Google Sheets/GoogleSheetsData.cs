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

        /// <summary>
        /// Represents the numerical index associated with the data item in Google Sheets.
        /// </summary>
        public int index;

        /// <summary>
        /// Specifies the type name of the ScriptableObject to be used, enabling dynamic resolution and operations such as creation, conversion, or manipulation.
        /// </summary>
        public string scriptableObjectType;

        /// <summary>
        /// Represents a value corresponding to a key in a key-value pair for Google Sheets data.
        /// </summary>
        public string value;

        /// <summary>
        /// Holds a reference to a Unity ScriptableObject, enabling enhanced data organization, management, and integration within the Google Sheets data workflow.
        /// </summary>
        public ScriptableObject scriptableObject;
    }
}