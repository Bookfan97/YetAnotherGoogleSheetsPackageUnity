/*
 * Taken from com.unity.testtools.codecoverage v. 1.2.6
 */
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Editor.Google_Sheets;
using Editor.Project_Settings;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Editor.Assemblies
{
    class IncludedAssembliesTreeView : TreeView
    {
        readonly GoogleSheetsCustomSettingsIMGUIRegister.GoogleSheetsDataItemDrawer m_Parent;
        const float kCheckBoxWidth = 42f;

        public float Width { get; set; } = 100f;

        public IncludedAssembliesTreeView(GoogleSheetsCustomSettingsIMGUIRegister.GoogleSheetsDataItemDrawer parent, string assembliesToInclude)
            : base(new TreeViewState())
        {
            m_Parent = parent;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            Reload();
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override TreeViewItem BuildRoot()
        {
            string[] includeAssemblyFilters = GoogleSheetsHelper.GoogleSheetsCustomSettings.assembliesToInclude?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            Regex[] includeAssemblies = new Regex[] { };
            if (includeAssemblyFilters.Any())
            {
                includeAssemblies = includeAssemblyFilters
                    .Select(f => AssemblyFiltering.CreateFilterRegex(f))
                    .ToArray();
            }

            TreeViewItem root = new TreeViewItem(-1, -1);

            bool developerMode = EditorPrefs.GetBool("DeveloperMode", false);

            if (developerMode)
            {
                System.Reflection.Assembly[] assemblies = AssemblyFiltering.GetAllProjectAssembliesInternal();               
                int assembliesLength = assemblies.Length;

                GUIContent textContent = new GUIContent();
                for (int i = 0; i < assembliesLength; ++i)
                {
                    System.Reflection.Assembly assembly = assemblies[i];
                    bool enabled = includeAssemblies.Any(f => f.IsMatch(assembly.GetName().Name.ToLowerInvariant()));
                    root.AddChild(new AssembliesTreeViewItem() { id = i + 1, displayName = assembly.GetName().Name, Enabled = enabled });

                    textContent.text = assembly.GetName().Name;
                    float itemWidth = TreeView.DefaultStyles.label.CalcSize(textContent).x + kCheckBoxWidth;
                    if (Width < itemWidth)
                        Width = itemWidth;
                    
                }
            }
            else
            {
                Assembly[] assemblies = AssemblyFiltering.GetAllProjectAssemblies();
                int assembliesLength = assemblies.Length;

                GUIContent textContent = new GUIContent();
                for (int i = 0; i < assembliesLength; ++i)
                {
                    Assembly assembly = assemblies[i];
                    bool enabled = (bool)includeAssemblies?.Any(f => f.IsMatch(assembly.name.ToLowerInvariant()));
                    root.AddChild(new AssembliesTreeViewItem() { id = i + 1, displayName = assembly.name, Enabled = enabled });

                    textContent.text = assembly.name;
                    float itemWidth = TreeView.DefaultStyles.label.CalcSize(textContent).x + kCheckBoxWidth;
                    if (Width < itemWidth)
                        Width = itemWidth;
                }
            }

            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            AssembliesTreeViewItem item = args.item as AssembliesTreeViewItem;
            EditorGUI.BeginChangeCheck();
            bool enabled = EditorGUI.ToggleLeft(args.rowRect, args.label, item.Enabled);
            if (EditorGUI.EndChangeCheck())
            {
                item.Enabled = enabled;
                ApplyChanges();
            }
        }

        public void SelectAll()
        {
            ToggleAll(true);
        }

        public void DeselectAll()
        {
            ToggleAll(false);
        }

        public void SelectAssets()
        {
            GoogleSheetsHelper.GoogleSheetsCustomSettings.assembliesToInclude = AssemblyFiltering.GetUserOnlyAssembliesString();
            SelectFromString(GoogleSheetsHelper.GoogleSheetsCustomSettings.assembliesToInclude);
        }

        public void SelectPackages()
        {
            GoogleSheetsHelper.GoogleSheetsCustomSettings.assembliesToInclude = AssemblyFiltering.GetPackagesOnlyAssembliesString();
            SelectFromString(GoogleSheetsHelper.GoogleSheetsCustomSettings.assembliesToInclude);
        }

        private void SelectFromString(string assembliesToInclude)
        {
            string[] includeAssemblyFilters = assembliesToInclude.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            Regex[] includeAssemblies = includeAssemblyFilters
                .Select(f => AssemblyFiltering.CreateFilterRegex(f))
                .ToArray();

            foreach (var child in rootItem.children)
            {
                AssembliesTreeViewItem childItem = child as AssembliesTreeViewItem;

                bool enabled = includeAssemblies.Any(f => f.IsMatch(childItem.displayName.ToLowerInvariant()));
                if (searchString == null)
                    childItem.Enabled = enabled;
                else if (DoesItemMatchSearch(child, searchString))
                    childItem.Enabled = enabled;
            }

            ApplyChanges();
        }

        private void ToggleAll(bool enabled)
        {
            foreach (var child in rootItem.children)
            {
                AssembliesTreeViewItem childItem = child as AssembliesTreeViewItem;
                if (searchString == null)
                    childItem.Enabled = enabled;
                else if (DoesItemMatchSearch(child, searchString))
                    childItem.Enabled = enabled;
            }

            ApplyChanges();
        }

        void ApplyChanges()
        {
            StringBuilder sb = new StringBuilder();
            foreach(var child in rootItem.children)
            {
                AssembliesTreeViewItem childItem = child as AssembliesTreeViewItem;
                if (childItem.Enabled)
                {
                    if (sb.Length > 0)
                        sb.Append(",");

                    sb.Append(childItem.displayName);
                }
            }

            GoogleSheetsHelper.GoogleSheetsCustomSettings.assembliesToInclude = sb.ToString();
        }
    }

    class AssembliesTreeViewItem : TreeViewItem
    {
        public bool Enabled { get; set; }
    }
}