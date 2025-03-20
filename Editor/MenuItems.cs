using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    ///     Represents a collection of menu items in the editor.
    ///     This class provides functionality to manage and interact with the menu items.
    /// </summary>
    public static class MenuItems
    {
        /// <summary>
        ///     Opens the Google Sheets preferences in the Unity Project Settings.
        /// </summary>
        [MenuItem("Tools/Google Sheets/Open Preferences", false, 2)]
        private static void OpenPreferences()
        {
            SettingsService.OpenProjectSettings("Project/Google Sheets");
        }

        // /// <summary>
        // ///     Opens the Google Sheets documentation URL in the default web browser.
        // ///     This method is accessible via the Tools > Google Sheets > Documentation menu item in the Unity Editor.
        // /// </summary>
        // [MenuItem("Tools/Google Sheets/Documentation", false, 1)]
        // private static void OpenDocumentation()
        // {
        //     Application.OpenURL(
        //         "https://definitive-infinity-media.github.io/com.definitiveinfinitymedia.googlesheets/api/index.html");
        // }
    }
}