/*
 * Taken from com.unity.testtools.codecoverage v. 1.2.6
 */
using Editor.Google_Sheets;
using Editor.Project_Settings;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Editor.Assemblies
{
    /// <summary>
    /// Represents a popup window for managing and selecting included assemblies within a Unity Editor context.
    /// </summary>
    /// <remarks>
    /// This class extends the <see cref="PopupWindowContent"/> class and provides a GUI-based interface for
    /// selecting, deselecting, and searching assemblies to include as part of the Google Sheets custom settings.
    /// It is primarily associated with the Google Sheets integration in Unity's project settings.
    /// </remarks>
    class IncludedAssembliesPopupWindow : PopupWindowContent
    {
        /// <summary>
        /// Represents a search field control designed to handle input and manage
        /// search queries within the IncludedAssembliesPopupWindow.
        /// </summary>
        /// <remarks>
        /// Utilized to provide a user interface for entering and managing search
        /// strings related to filtering or narrowing down displayed data.
        /// </remarks>
        readonly SearchField m_SearchField;

        /// <summary>
        /// Represents the tree view control used within the IncludedAssembliesPopupWindow
        /// to display and manage a hierarchical list of assemblies.
        /// </summary>
        /// <remarks>
        /// Provides functionality for rendering, searching, and interacting with
        /// assembly data, including features such as selection and filtering.
        /// </remarks>
        readonly IncludedAssembliesTreeView m_TreeView;

        const float kWindowHeight = 221;

        /// <summary>
        /// Represents the width of the popup window used for selecting included assemblies.
        /// </summary>
        /// <remarks>
        /// Determines the horizontal dimension of the IncludedAssembliesPopupWindow, impacting
        /// the layout and display of its content. This property can be set to customize
        /// the window's size dynamically based on specific requirements.
        /// </remarks>
        public float Width { get; set; }

        /// <summary>
        /// Contains static read-only properties for defining GUI label content and button labels
        /// used in the Included Assemblies Popup Window within the Unity Editor.
        /// </summary>
        /// <remarks>
        /// This class provides localized text and tooltip descriptions to maintain consistency
        /// and clarity in the UI of the Included Assemblies Popup Window. It is used to relay relevant
        /// interaction details such as selecting all assemblies, filtering by asset categories, or deselecting all assemblies.
        /// </remarks>
        static class Styles
        {
            public static readonly GUIContent SelectLabel = EditorGUIUtility.TrTextContent("Select:");
            public static readonly GUIContent SelectAllButtonLabel = EditorGUIUtility.TrTextContent("All", "Click this to select and include all the assemblies in the project. This includes both the assemblies under the 'Assets' folder and packages.\n\nIf searching, it will apply only to the assemblies visible in the list.");
            public static readonly GUIContent SelectAssetsButtonLabel = EditorGUIUtility.TrTextContent("Assets", "Click this to select and include only the assemblies under the 'Assets' folder.\n\nIf searching, it will apply only to the assemblies visible in the list.");
            public static readonly GUIContent SelectPackagesButtonLabel = EditorGUIUtility.TrTextContent("Packages", "Click this to select and include only the Packages' assemblies.\n\nIf searching, it will apply only to the assemblies visible in the list.");
            public static readonly GUIContent DeselectAllButtonLabel = EditorGUIUtility.TrTextContent("Deselect All", "Click this to deselect and exclude all the assemblies.\n\nIf searching, it will apply only to the assemblies visible in the list.");
        }

        /// <summary>
        /// Represents a popup window to manage and display included assemblies related to Google Sheets integration
        /// within the Unity Editor.
        /// This class is derived from the PopupWindowContent and provides functionality
        /// such as searching and displaying a list of assemblies to include.
        /// </summary>
        public IncludedAssembliesPopupWindow(GoogleSheetsCustomSettingsIMGUIRegister.GoogleSheetsDataItemDrawer parent)
        {
            m_SearchField = new SearchField();
            m_TreeView = new IncludedAssembliesTreeView(parent, GoogleSheetsHelper.GoogleSheetsCustomSettings.AssembliesToInclude);
        }

        /// <summary>
        /// Renders the graphical user interface (GUI) for the IncludedAssembliesPopupWindow.
        /// This method is responsible for drawing search fields, selection buttons, and the tree view, allowing interactions
        /// such as selection or deselection of assemblies within the popup window.
        /// </summary>
        /// <param name="rect">Defines the boundary rectangle within which the GUI components are drawn.</param>
        public override void OnGUI(Rect rect)
        {
            const int border = 4;
            const int topPadding = 12;
            const int searchHeight = 20;
            const int buttonHeight = 16;
            const int remainTop = topPadding + searchHeight + buttonHeight + border + border;

            float selectLabelWidth = EditorStyles.boldLabel.CalcSize(Styles.SelectLabel).x;
            float selectAllWidth = EditorStyles.miniButton.CalcSize(Styles.SelectAllButtonLabel).x;
            float selectAssetsWidth = EditorStyles.miniButton.CalcSize(Styles.SelectAssetsButtonLabel).x;
            float selectPackagesWidth = EditorStyles.miniButton.CalcSize(Styles.SelectPackagesButtonLabel).x;
            float deselectAllWidth = EditorStyles.miniButton.CalcSize(Styles.DeselectAllButtonLabel).x;

            Rect searchRect = new Rect(border, topPadding, rect.width - border * 2, searchHeight);
            Rect selectLabelRect = new Rect(border, topPadding + searchHeight + border, selectLabelWidth, searchHeight);
            Rect selectAllRect = new Rect(border + selectLabelWidth + border, topPadding + searchHeight + border, selectAllWidth, buttonHeight);
            Rect selectAssetsRect = new Rect(border + selectLabelWidth + border + selectAllWidth + border, topPadding + searchHeight + border, selectAssetsWidth, buttonHeight);
            Rect selectPackagesRect = new Rect(border + selectLabelWidth + border + selectAllWidth + border + selectAssetsWidth + border, topPadding + searchHeight + border, selectPackagesWidth, buttonHeight);
            Rect deselectAllRect = new Rect(rect.width - border - deselectAllWidth, topPadding + searchHeight + border, deselectAllWidth, buttonHeight);
            Rect remainingRect = new Rect(border, remainTop, rect.width - border * 2, rect.height - remainTop - border);

            m_TreeView.searchString = m_SearchField.OnGUI(searchRect, m_TreeView.searchString);

            GUI.Label(selectLabelRect, Styles.SelectLabel, EditorStyles.boldLabel);

            if (GUI.Button(selectAllRect, Styles.SelectAllButtonLabel, EditorStyles.miniButton))
            {
                m_TreeView.SelectAll();
            }

            if (GUI.Button(deselectAllRect, Styles.DeselectAllButtonLabel, EditorStyles.miniButton))
            {
                m_TreeView.DeselectAll();
            }

            if (GUI.Button(selectAssetsRect, Styles.SelectAssetsButtonLabel, EditorStyles.miniButton))
            {
                m_TreeView.SelectAssets();
            }

            if (GUI.Button(selectPackagesRect, Styles.SelectPackagesButtonLabel, EditorStyles.miniButton))
            {
                m_TreeView.SelectPackages();
            }

            m_TreeView.OnGUI(remainingRect);
        }

        /// <summary>
        /// Determines and returns the size of the popup window as a <see cref="Vector2"/> object.
        /// </summary>
        /// <returns>
        /// A <see cref="Vector2"/> representing the width and height of the popup window.
        /// The width is the greater of the TreeView's width and the specified Width property,
        /// and the height is represented by a constant value.
        /// </returns>
        public override Vector2 GetWindowSize()
        {
            return new Vector2(Mathf.Max(Width, m_TreeView.Width), kWindowHeight);
        }

        /// <summary>
        /// Invoked when the popup window is opened.
        /// Sets focus to the search field to enhance user interaction immediately upon opening.
        /// </summary>
        public override void OnOpen()
        {
            m_SearchField.SetFocus();
            base.OnOpen();
        }
    }
}