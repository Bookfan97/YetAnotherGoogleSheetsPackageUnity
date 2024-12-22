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
    public class GetAllScriptableObjects
    {
        /// <summary>
        /// Finds all ScriptableObject assets in the project and returns their paths.
        /// </summary>
        /// <returns>List of paths to ScriptableObject assets</returns>
        public List<string> GetAllNames()
        {
            // Safely retrieve ScriptableObject GUIDs using AssetDatabase
            string[] options = AssetDatabase.FindAssets("t:ScriptableObject");
            return options.Select(option => AssetDatabase.GUIDToAssetPath(option)).ToList();
        }

        /// <summary>
        /// Reads all DLL files in the project and extracts class names, filtering out system or generated classes.
        /// </summary>
        /// <param name="projectPath">Root project path</param>
        /// <returns>List of class names with namespaces</returns>
        public static List<string> GetAllClasses(string projectPath)
        {
            List<string> classNames = new List<string>();

            // Get all .dll files in project directories (including subdirectories)
            string[] scriptFiles = Directory.GetFiles(projectPath, "*.dll", SearchOption.AllDirectories);

            foreach (string dllFile in scriptFiles)
            {
                try
                {
                    // Use Mono.Cecil to safely and efficiently parse assemblies
                    var assembly = AssemblyDefinition.ReadAssembly(dllFile);

                    foreach (TypeDefinition type in assembly.MainModule.Types)
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
            }

            return classNames;
        }

        /// <summary>
        /// Editor menu function to list all class names from project assemblies.
        /// </summary>
        [MenuItem("Tools/List All Classes")]
        public static void ListClasses()
        {
            string projectPath = Path.Combine(Application.dataPath, ".."); // Safely set root project path
            var classes = GetAllClasses(projectPath);

            foreach (var className in classes)
            {
                Debug.Log($"Class: {className}");
            }

            Debug.Log($"Total Classes Found: {classes.Count}");
        }

        /// <summary>
        /// Finds all classes derived from ScriptableObject across all project assemblies.
        /// </summary>
        /// <returns>List of ScriptableObject class types</returns>
        public static List<Type> GetAllScriptableObjectClasses()
        {
            List<Type> scriptableObjectClasses = new List<Type>();

            try
            {
                // Filter and load only relevant assemblies
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(assembly =>
                        assembly.GetName().Name.StartsWith("Assembly-CSharp")); // Avoid irrelevant assemblies

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var types = assembly.GetTypes();

                        foreach (var type in types)
                        {
                            // Check for non-abstract, ScriptableObject-derived classes
                            if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(ScriptableObject)))
                            {
                                // Verify inclusion in relevant assemblies through GoogleSheets settings
                                if (GoogleSheetsHelper.GoogleSheetsCustomSettings?.assembliesToInclude?
                                        .Contains(type.Assembly.GetName().Name) == true)
                                {
                                    scriptableObjectClasses.Add(type);
                                }
                            }
                        }
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        // Handle type reflection errors
                        Debug.LogWarning($"Failed to load types from assembly {assembly.GetName().Name}: {ex.Message}");
                    }
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

    public class ScriptableObjectClassFinderEditor : EditorWindow
    {
        private List<Type> _scriptableObjectClasses;
        private Vector2 _scrollPosition; // Required for enabling scrollable GUI

        [MenuItem("Tools/Find ScriptableObject Classes")]
        public static void ShowWindow()
        {
            GetWindow<ScriptableObjectClassFinderEditor>("Scriptable Object Finder");
        }

        /*private void OnEnable()
        {
            try
            {
                // Load relevant classes when the window is opened
                _scriptableObjectClasses = GetAllScriptableObjects.GetAllScriptableObjectClasses();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading ScriptableObject classes: {ex.Message}");
                _scriptableObjectClasses = new List<Type>();
            }
        }*/

        private void OnGUI()
        {
            GUILayout.Label("ScriptableObject Classes in Project", EditorStyles.boldLabel);

            if (_scriptableObjectClasses == null || _scriptableObjectClasses.Count == 0)
            {
                GUILayout.Label("No ScriptableObject classes found.");
                return;
            }

            // Enable scrolling for large lists
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var scriptableObjectClass in _scriptableObjectClasses)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(scriptableObjectClass.ToString());

                // Add an interactive Ping button for each class
                if (GUILayout.Button("Ping", GUILayout.Width(50)))
                {
                    // Load and ping the relevant asset (if paths are linked)
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(
                        scriptableObjectClass.ToString()); // Replace with valid asset paths if applicable
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}