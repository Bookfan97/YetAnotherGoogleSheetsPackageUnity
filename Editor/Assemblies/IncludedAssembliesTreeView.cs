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
    /// <summary>
    /// Represents a TreeView for managing and displaying included assemblies in a Unity Editor context.
    /// </summary>
    class IncludedAssembliesTreeView : TreeView
    {
        /// <summary>
        /// A reference to the parent object of type <see cref="GoogleSheetsCustomSettingsIMGUIRegister.GoogleSheetsDataItemDrawer"/>
        /// that is used to facilitate communication or interaction between this TreeView and its parent context.
        /// </summary>
        readonly GoogleSheetsCustomSettingsIMGUIRegister.GoogleSheetsDataItemDrawer m_Parent;
        const float kCheckBoxWidth = 42f;

        /// <summary>
        /// Specifies the current width of the TreeView, dynamically adjusted to accommodate
        /// the longest assembly name or label displayed in the included assemblies list.
        /// </summary>
        public float Width { get; set; } = 100f;

        /// <summary>
        /// A specialized TreeView implementation designed for managing and displaying
        /// a list of included assemblies in the Unity Editor. This class provides functionalities
        /// for selection and visualization in a structured format with support for advanced
        /// operations such as selecting all or specific types of assemblies.
        /// </summary>
        public IncludedAssembliesTreeView(GoogleSheetsCustomSettingsIMGUIRegister.GoogleSheetsDataItemDrawer parent,
            string assembliesToInclude)
            : base(new TreeViewState())
        {
            m_Parent = parent;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            Reload();
        }

        /// <summary>
        /// Determines if multiple items can be selected in the TreeView.
        /// </summary>
        /// <param name="item">The TreeViewItem to check for multi-selection support.</param>
        /// <returns>True if multiple items can be selected; otherwise, false.</returns>
        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        /// <summary>
        /// Constructs the root item of the TreeView. This method initializes
        /// the TreeView structure and populates it with assembly items based
        /// on specified filter criteria or default settings. The implementation
        /// dynamically adapts to the developer mode, loading either internal project
        /// assemblies or all project assemblies.
        /// </summary>
        /// <returns>
        /// A TreeViewItem representing the root of the assembly TreeView hierarchy.
        /// </returns>
        protected override TreeViewItem BuildRoot()
        {
            string[] includeAssemblyFilters = GoogleSheetsHelper.GoogleSheetsCustomSettings.AssembliesToInclude?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            Regex[] includeAssemblies = new Regex[] { };
            if (includeAssemblyFilters != null && includeAssemblyFilters.Any())
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

        /// <summary>
        /// Renders a single row within the TreeView, allowing for custom GUI representation of
        /// each item in the hierarchy. Includes functionality for enabling or disabling the item
        /// through a toggle control.
        /// </summary>
        /// <param name="args">Arguments providing information necessary for rendering the row,
        /// including the row rectangle and label.</param>
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

        /// <summary>
        /// Selects all items in the TreeView, marking them as included assemblies.
        /// </summary>
        public void SelectAll()
        {
            ToggleAll(true);
        }

        /// <summary>
        /// Deselects all items in the tree view. This method ensures that no items remain selected,
        /// providing a way to reset selection states within the included assemblies tree.
        /// </summary>
        public void DeselectAll()
        {
            ToggleAll(false);
        }

        /// <summary>
        /// Selects the assets to be included from the assemblies, using the user-defined
        /// filtered assembly data string. Updates the assembliesToInclude property
        /// within the Google Sheets custom settings and handles the selection process
        /// accordingly.
        /// </summary>
        public void SelectAssets()
        {
            GoogleSheetsHelper.GoogleSheetsCustomSettings.AssembliesToInclude = AssemblyFiltering.GetUserOnlyAssembliesString();
            SelectFromString(GoogleSheetsHelper.GoogleSheetsCustomSettings.AssembliesToInclude);
        }

        /// <summary>
        /// Selects only the assemblies that are part of the packages, updating the
        /// included assemblies configuration. This method uses assembly filtering
        /// to identify package-specific assemblies and ensures that the selection
        /// reflects only those assemblies.
        /// </summary>
        public void SelectPackages()
        {
            GoogleSheetsHelper.GoogleSheetsCustomSettings.AssembliesToInclude = AssemblyFiltering.GetPackagesOnlyAssembliesString();
            SelectFromString(GoogleSheetsHelper.GoogleSheetsCustomSettings.AssembliesToInclude);
        }

        /// <summary>
        /// Updates the state of each item in the TreeView based on the provided string of assembly filters.
        /// Matches are determined using regular expressions created from the filters in the given string.
        /// </summary>
        /// <param name="assembliesToInclude">
        /// A comma-separated string containing the assembly filters to apply. These filters are used
        /// to determine which items in the TreeView should be enabled or disabled based on their name.
        /// </param>
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

        /// <summary>
        /// Toggles the enabled state of all child items in the TreeView based on the specified condition.
        /// </summary>
        /// <param name="enabled">Specifies whether to enable or disable the child items. If true, all child items will be enabled; if false, they will be disabled.</param>
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

        /// <summary>
        /// Applies the changes made in the IncludedAssembliesTreeView by updating the
        /// list of included assemblies and storing them in the GoogleSheetsCustomSettings configuration.
        /// This method consolidates the enabled items in the TreeView into a comma-separated string
        /// and updates the settings with the new value.
        /// </summary>
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

            GoogleSheetsHelper.GoogleSheetsCustomSettings.AssembliesToInclude = sb.ToString();
        }
    }
}