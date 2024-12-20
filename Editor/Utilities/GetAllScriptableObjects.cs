using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Editor.Utilities;
using Mono.Cecil;
using UnityEditor;
using UnityEngine;

namespace Editor.Utilities
{
    public class GetAllScriptableObjects
    {
        public List<string> GetAllNames()
        {
            string[] options = AssetDatabase.FindAssets("t:ScriptableObject");
            return options.Select(option => AssetDatabase.GUIDToAssetPath(option)).ToList();
        }
        
        
        public static List<string> GetAllClasses(string projectPath)
        {
            // List to store class names
            List<string> classNames = new List<string>();

            // Get all script files in the project
            string[] scriptFiles = Directory.GetFiles(projectPath, "*.dll", SearchOption.TopDirectoryOnly);

            foreach (string dllFile in scriptFiles)
            {
                try
                {
                    // Use Mono.Cecil to read the assembly
                    var assembly = AssemblyDefinition.ReadAssembly(dllFile);

                    // Iterate through all types in the assembly
                    foreach (TypeDefinition type in assembly.MainModule.Types)
                    {
                        // Skip system or generated classes
                        if (type.Name.StartsWith("<") || type.Namespace == null) continue;

                        // Add the class name to the list
                        classNames.Add($"{type.Namespace} - {type.Name}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"Failed to analyze {dllFile}: {ex.Message}");
                }
            }

            return classNames;
        }
        
        [MenuItem("Tools/List All Classes")]
        public static void ListClasses()
        {
            string projectPath = Application.dataPath; // Adjust path as necessary
            var classes = GetAllClasses(projectPath);

            foreach (var className in classes)
            {
                Debug.Log($"Class: {className}");
            }
        
            Debug.Log($"Total Classes Found: {classes.Count}");
        }
        
        
        /// <summary>
        /// Finds all classes that inherit from ScriptableObject in the project.
        /// </summary>
        /// <returns>A list of names of all ScriptableObject-derived classes in the project.</returns>
        public static List<Type> GetAllScriptableObjectClasses()
        {
            // Store all relevant classes
            List<Type> scriptableObjectClasses = new List<Type>();

            // Get all assemblies loaded in the current domain
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                // Get all types in the assembly
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    // Check if the type is a class, not abstract, and derives from ScriptableObject
                    if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(ScriptableObject)))// && type.Assembly.FullName.Equals("Assembly-CSharp"))
                    {
                        Debug.Log(type.Assembly);
                        scriptableObjectClasses.Add(type);
                        Debug.Log(type.FullName);
                    }
                }
            }

            return scriptableObjectClasses;
        }
    }
}

public class ScriptableObjectClassFinderEditor : EditorWindow
{
    private List<Type> _scriptableObjectClasses;

    [MenuItem("Tools/Find ScriptableObject Classes")]
    public static void ShowWindow()
    {
        GetWindow<ScriptableObjectClassFinderEditor>("Scriptable Object Finder");
    }

    private void OnEnable()
    {
        // Get all ScriptableObject classes when the window is opened
        _scriptableObjectClasses = GetAllScriptableObjects.GetAllScriptableObjectClasses();
    }

    private void OnGUI()
    {
        // Display the list of ScriptableObject-derived classes
        GUILayout.Label("ScriptableObject Classes in Project", EditorStyles.boldLabel);

        if (_scriptableObjectClasses == null || _scriptableObjectClasses.Count == 0)
        {
            GUILayout.Label("No ScriptableObject classes found.");
            return;
        }

        foreach (var scriptableObjectClass in _scriptableObjectClasses)
        {
            GUILayout.Label(scriptableObjectClass.FullName);
        }
    }
}