using System;

namespace Editor.Google_Sheets
{
    /// <summary>
    ///     Represents a key-value pair for Google Sheets data.
    /// </summary>
    [Serializable]
    public class DataItem
    {
        /// <summary>
        ///     Represents the unique identifier or key in a key-value pair for Google Sheets data.
        /// </summary>
        public int key;

        /// <summary>
        ///     Represents the numerical index associated with the data item in Google Sheets.
        /// </summary>
        public int index;

        /// <summary>
        ///     Specifies the type name of the ScriptableObject to be used, enabling dynamic resolution and operations such as
        ///     creation, conversion, or manipulation.
        /// </summary>
        public string scriptableObjectType;

        /// <summary>
        ///     Represents a value corresponding to a key in a key-value pair for Google Sheets data.
        /// </summary>
        public string value;

        /// <summary>
        ///     Represents the name of the assembly where the ScriptableObject type is defined.
        /// </summary>
        public string assemblyName;

        /// <summary>
        ///     Retrieves the Type of the ScriptableObject based on the scriptableObjectType and assemblyName.
        /// </summary>
        /// <returns>The Type of the ScriptableObject if found; otherwise, null.</returns>
        public Type GetSOType()
        {
            return Type.GetType($"{scriptableObjectType}, {assemblyName}");
        }
    }
}