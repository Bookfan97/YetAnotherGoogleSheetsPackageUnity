using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Editor.Google_Sheets;
using Mono.Cecil;
using UnityEditor;
using UnityEngine;

namespace Editor.Utilities
{
    /// <summary>
    ///     Provides utility methods for working with ScriptableObjects in Unity projects.
    ///     Includes functionality for retrieving all ScriptableObject assets, discovering
    ///     classes in assemblies, and listing ScriptableObject types.
    /// </summary>
    public class GetAllScriptableObjects
    {
        /// <summary>
        ///     Finds all ScriptableObject assets in the project and returns their paths.
        /// </summary>
        /// <returns>List of paths to ScriptableObject assets</returns>
        public List<string> GetAllNames()
        {
            // Safely retrieve ScriptableObject GUIDs using AssetDatabase
            var options = AssetDatabase.FindAssets("t:ScriptableObject");
            return options.Select(option => AssetDatabase.GUIDToAssetPath(option)).ToList();
        }

        /// <summary>
        ///     Reads all DLL files in the project and extracts class names, filtering out system or generated classes.
        /// </summary>
        /// <param name="projectPath">Root project path</param>
        /// <returns>List of class names with namespaces</returns>
        public static List<string> GetAllClasses(string projectPath)
        {
            var classNames = new List<string>();

            // Get all .dll files in project directories (including subdirectories)
            var scriptFiles = Directory.GetFiles(projectPath, "*.dll", SearchOption.AllDirectories);

            foreach (var dllFile in scriptFiles)
                try
                {
                    // Use Mono.Cecil to safely and efficiently parse assemblies
                    var assembly = AssemblyDefinition.ReadAssembly(dllFile);

                    foreach (var type in assembly.MainModule.Types)
                    {
                        // Skip generated/system classes or those without a namespace
                        if (type.Name.StartsWith("<") || string.IsNullOrEmpty(type.Namespace)) continue;

                        // Safeguard against null/empty namespaces and use structured naming
                        classNames.Add($"{type.Namespace}.{type.Name}");
                    }
                }
                catch (Exception ex)
                {
                    // Handle potential Mono.Cecil issues systematically and log meaningful messages
                    Debug.LogError($"Failed to analyze {dllFile}: {ex.Message}");
                }

            return classNames;
        }

        /// <summary>
        ///     Finds all classes derived from ScriptableObject across all project assemblies.
        /// </summary>
        /// <returns>Dictionary of ScriptableObject class types</returns>
        public static Dictionary<Type, string> GetAllScriptableObjectClasses()
        {
            var scriptableObjectClasses = new Dictionary<Type, string>();

            try
            {
                // Filter and load only relevant assemblies
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(assembly =>
                        assembly.GetName().Name.StartsWith("Assembly-CSharp")); // Avoid irrelevant assemblies

                foreach (var assembly in assemblies)
                    try
                    {
                        var types = assembly.GetTypes();

                        foreach (var type in types)
                            // Check for non-abstract, ScriptableObject-derived classes
                            if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(ScriptableObject)))
                                // Verify inclusion in relevant assemblies through GoogleSheets settings
                                if (GoogleSheetsHelper.GoogleSheetsCustomSettings?.AssembliesToInclude?
                                        .Contains(type.Assembly.GetName().Name) == true)
                                    scriptableObjectClasses.Add(type, type.Assembly.GetName().Name);
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        // Handle type reflection errors
                        Debug.LogWarning($"Failed to load types from assembly {assembly.GetName().Name}: {ex.Message}");
                    }
            }
            catch (Exception ex)
            {
                // General safeguard for AppDomain assembly retrieval and error logging
                Debug.LogError($"Failed to retrieve ScriptableObject classes: {ex.Message}");
            }

            return scriptableObjectClasses;
        }
    }
}