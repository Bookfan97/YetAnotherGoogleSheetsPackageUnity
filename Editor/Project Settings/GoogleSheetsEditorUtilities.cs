using System.IO;
using Editor.Google_Sheets;
using UnityEditor;

namespace Editor.Project_Settings
{
    /// <summary>
    /// Provides utility functions for managing Google Sheets integration within the Unity Editor.
    /// </summary>
    public static class GoogleSheetsEditorUtilities
    {
        /// <summary>
        /// Checks if the Google Sheets configuration contains valid information.
        /// Ensures that the Spreadsheet ID, CSV path, and JSON path are correctly set and valid.
        /// </summary>
        /// <returns>Returns true if all information is valid, otherwise false.</returns>
        public static bool CheckValidInformation()
        {
            if (!IsValidSpreadsheetID()) return false;
            if (!IsValidCSV()) return false;
            if (!IsValidJSON()) return false;
            return true;
        }

        /// <summary>
        /// Validates whether the Spreadsheet ID is correctly set and not empty.
        /// </summary>
        /// <returns>Returns true if the Spreadsheet ID is valid, otherwise false.</returns>
        public static bool IsValidSpreadsheetID()
        {
            if (!string.IsNullOrEmpty(GoogleSheetsHelper.GoogleSheetsCustomSettings.MSpreadsheetID)) return true;
            MissingDataPopup("SpreadsheetID is missing");
            return false;
        }

        /// <summary>
        /// Checks if the CSV file path is valid.
        /// Ensures that the CSV path is not empty, not the default path, and has a '.csv' extension.
        /// </summary>
        /// <returns>Returns true if the CSV path is valid, otherwise false.</returns>
        private static bool IsValidCSV()
        {
            var ext = Path.GetExtension(GoogleSheetsHelper.GoogleSheetsCustomSettings.GetPathForSheet(0));
            if (string.IsNullOrEmpty(GoogleSheetsHelper.GoogleSheetsCustomSettings.GetPathForSheet(0)) ||
                GoogleSheetsHelper.GoogleSheetsCustomSettings.GetPathForSheet(0) == GoogleSheetsHelper.GoogleSheetsCustomSettings.GetDefaultPath())
            {
                MissingDataPopup("Data CSV is missing");
                return false;
            }

            if (ext != ".csv")
            {
                MissingDataPopup("Please enter a valid CSV file path");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates whether the Client Secret JSON path is correctly set and points to a valid JSON file.
        /// </summary>
        /// <returns>Returns true if the Client Secret JSON path is valid, otherwise false.</returns>
        private static bool IsValidJSON()
        {
            var ext = Path.GetExtension(GoogleSheetsHelper.GoogleSheetsCustomSettings.ClientSecretJsonPath);
            if (string.IsNullOrEmpty(GoogleSheetsHelper.GoogleSheetsCustomSettings.ClientSecretJsonPath) ||
                GoogleSheetsHelper.GoogleSheetsCustomSettings.ClientSecretJsonPath == GoogleSheetsHelper.GoogleSheetsCustomSettings.GetDefaultPath())
            {
                MissingDataPopup("Client Secret JSON is missing");
                return false;
            }

            if (ext != ".json")
            {
                MissingDataPopup("Please enter a valid JSON file path");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Displays a popup with the specified message indicating missing data for Google Sheets integration.
        /// Provides options to open project settings or close the popup.
        /// </summary>
        /// <param name="message">The message to be displayed in the popup.</param>
        public static void MissingDataPopup(string message)
        {
            if (EditorUtility.DisplayDialog("Google Sheets", message, "Open Settings", "Close"))
                SettingsService.OpenProjectSettings("Project/Definitive Infinity Media/Google Sheets");
        }
    }
}