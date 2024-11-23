using System;
using System.IO;
using Editor.Project_Settings;
using Google.Apis.Sheets.v4.Data;
using UnityEngine;

namespace Editor.Google_Sheets
{
    /// <summary>
    ///     A utility class that handles the uploading of data from a local CSV file to a Google Sheets spreadsheet.
    ///     This class reads CSV data from a configured file path and uses the Google Sheets API to paste the data into a
    ///     specified spreadsheet.
    /// </summary>
    public class GoogleSheetsUploader
    {
        /// <summary>
        ///     Uploads data from a local CSV file to a specified Google Sheets spreadsheet.
        /// </summary>
        /// <remarks>
        ///     This method reads the contents of a CSV file located at the path specified in the project settings.
        ///     It then uses the Google Sheets API to paste this data into the first sheet of the spreadsheet.
        ///     The method logs an error if the update fails.
        /// </remarks>
        public void UploadData()
        {
            var config = new GoogleSheetsConfig();
            config.InitializeGoogleSheetsService();

            var csvContents = File.ReadAllText(GoogleSheetsSettings.instance.CsvPath);

            var requestBody = new BatchUpdateSpreadsheetRequest
            {
                Requests = new Request[]
                {
                    new()
                    {
                        PasteData = new PasteDataRequest
                        {
                            Coordinate = new GridCoordinate
                            {
                                SheetId = 0
                            },
                            Data = csvContents,
                            Type = "PASTE_NORMAL",
                            Delimiter = ","
                        }
                    }
                }
            };

            var batchUpdateReq =
                config.Service.Spreadsheets.BatchUpdate(requestBody, GoogleSheetsSettings.instance.SpreadsheetID);
            try
            {
                var batchUpdateSpreadsheetResponse = batchUpdateReq.Execute();
                if (batchUpdateSpreadsheetResponse == null)
                    Debug.LogError($"Failed to update spreadsheet: {GoogleSheetsSettings.instance.SpreadsheetID}");
            }
            catch (Exception e)
            {
                GoogleSheetsEditorUtilities.MissingDataPopup(e.Message);
                throw;
            }
        }
    }
}