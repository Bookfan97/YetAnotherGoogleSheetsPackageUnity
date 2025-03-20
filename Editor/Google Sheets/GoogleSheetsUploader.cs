using System;
using System.Collections.Generic;
using System.IO;
using Editor.Project_Settings;
using Google.Apis.Sheets.v4.Data;
using UnityEngine;

namespace Editor.Google_Sheets
{
    /// <summary>
    ///     A utility class that handles the uploading of data from all local CSV file to each Google Sheets spreadsheet.
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
        public void UploadAllData()
        {
            var config = new GoogleSheetsConfig();
            config.InitializeGoogleSheetsService();
            var requests = new List<Request>();

            foreach (var data in GoogleSheetsHelper.GoogleSheetsCustomSettings.Data)
            {
                var csvContents =
                    File.ReadAllText(GoogleSheetsHelper.GoogleSheetsCustomSettings.GetPathForSheet(data.key));
                var request = new Request
                {
                    PasteData = new PasteDataRequest
                    {
                        Coordinate = new GridCoordinate
                        {
                            SheetId = data.key
                        },
                        Data = csvContents,
                        Type = "PASTE_NORMAL",
                        Delimiter = ","
                    }
                };
                requests.Add(request);
            }


            var requestBody = new BatchUpdateSpreadsheetRequest
            {
                Requests = requests.ToArray()
            };

            var batchUpdateReq =
                config.Service.Spreadsheets.BatchUpdate(requestBody,
                    GoogleSheetsHelper.GoogleSheetsCustomSettings.MSpreadsheetID);
            try
            {
                var batchUpdateSpreadsheetResponse = batchUpdateReq.Execute();
                if (batchUpdateSpreadsheetResponse == null)
                    Debug.LogError(
                        $"Failed to update spreadsheet: {GoogleSheetsHelper.GoogleSheetsCustomSettings.MSpreadsheetID}");
            }
            catch (Exception e)
            {
                GoogleSheetsEditorUtilities.MissingDataPopup(e.Message);
                throw;
            }
        }

        /// <summary>
        ///     Uploads data from a local CSV file to a specified Google Sheets spreadsheet.
        /// </summary>
        /// <remarks>
        ///     This method reads the contents of a CSV file located at the path specified in the project settings.
        ///     It then uses the Google Sheets API to paste this data into the first sheet of the spreadsheet.
        ///     The method logs an error if the update fails.
        /// </remarks>
        /// <param name="dataItem"></param>
        public void UploadSheetData(DataItem dataItem)
        {
            var config = new GoogleSheetsConfig();
            config.InitializeGoogleSheetsService();
            var requests = new List<Request>();
            var csvContents =
                File.ReadAllText(GoogleSheetsHelper.GoogleSheetsCustomSettings.GetPathForSheet(dataItem.key));

            var request = new Request
            {
                PasteData = new PasteDataRequest
                {
                    Coordinate = new GridCoordinate
                    {
                        SheetId = dataItem.key
                    },
                    Data = csvContents,
                    Type = "PASTE_NORMAL",
                    Delimiter = ","
                }
            };
            requests.Add(request);
            var requestBody = new BatchUpdateSpreadsheetRequest
            {
                Requests = requests.ToArray()
            };

            var batchUpdateReq =
                config.Service.Spreadsheets.BatchUpdate(requestBody,
                    GoogleSheetsHelper.GoogleSheetsCustomSettings.MSpreadsheetID);
            try
            {
                var batchUpdateSpreadsheetResponse = batchUpdateReq.Execute();
                if (batchUpdateSpreadsheetResponse == null)
                    Debug.LogError(
                        $"Failed to update spreadsheet: {GoogleSheetsHelper.GoogleSheetsCustomSettings.MSpreadsheetID}");
            }
            catch (Exception e)
            {
                GoogleSheetsEditorUtilities.MissingDataPopup(e.Message);
                throw;
            }
        }
    }
}