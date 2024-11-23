using System;
using Editor.Project_Settings;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using UnityEditor.Localization.Plugins.Google;

namespace Editor.Google_Sheets
{
    /// <summary>
    ///     Represents the configuration settings for accessing and using the Google Sheets API.
    /// </summary>
    public class GoogleSheetsConfig : IGoogleSheetsService
    {
        /// <summary>
        ///     A read-only array containing the scopes required to access Google Sheets and Google Drive APIs.
        /// </summary>
        /// <remarks>
        ///     These scopes determine the permissions granted to the application for accessing and modifying Google Sheets
        ///     and Google Drive contents. The scopes include:
        ///     - SheetsService.Scope.Spreadsheets: Full access to Google Sheets.
        ///     - SheetsService.Scope.Drive: Full access to the user's Google Drive.
        ///     - SheetsService.Scope.DriveFile: Access to specific files in the user's Google Drive.
        /// </remarks>
        private readonly string[] _scopes =
        {
            SheetsService.Scope.Spreadsheets, SheetsService.Scope.Drive, SheetsService.Scope.DriveFile
        };

        /// <summary>
        ///     Gets the instance of the <see cref="SheetsService" /> used for interacting with the Google Sheets API.
        /// </summary>
        /// <remarks>
        ///     This property is initialized via the <see cref="InitializeGoogleSheetsService" /> method, which sets up the
        ///     necessary credentials and scopes required to utilize the Google Sheets API.
        /// </remarks>
        public SheetsService Service { get; private set; }

        /// <summary>
        ///     Initializes the Google Sheets service instance with the necessary credentials and scopes.
        /// </summary>
        /// <remarks>
        ///     This method reads the client secret JSON path from the Google Sheets settings, creates credentials,
        ///     and assigns them to a new instance of the SheetsService. This service instance is then used
        ///     for subsequent API interactions such as uploading and downloading data from Google Sheets.
        /// </remarks>
        public void InitializeGoogleSheetsService()
        {
            try
            {
                var credentials = GoogleCredential.FromFile(GoogleSheetsSettings.instance.ClientSecretJsonPath)
                    .CreateScoped(_scopes);

                Service = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credentials
                });
            }
            catch (Exception e)
            {
                GoogleSheetsEditorUtilities.MissingDataPopup(e.Message);
                throw;
            }
        }
    }
}