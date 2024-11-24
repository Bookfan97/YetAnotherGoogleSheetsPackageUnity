using System;
using System.IO;
using Editor.Project_Settings;
using Google.Apis.Sheets.v4.Data;
using UnityEngine;

namespace Editor.Google_Sheets
{
    /// <summary>
    ///     Responsible for downloading data from a specified Google Sheets spreadsheet.
    /// </summary>
    public class GoogleSheetsDownloader
    {
        /// <summary>
        ///     Downloads data from the Google Sheets spreadsheet specified in the project settings.
        /// </summary>
        /// <remarks>
        ///     This method initializes the Google Sheets service, retrieves the spreadsheet data,
        ///     and then generates a CSV file from the downloaded data. If an error occurs during
        ///     the download, an error message is logged and the exception is rethrown.
        /// </remarks>
        /// <exception cref="Exception">Thrown when an error occurs during the download process.</exception>
        public void DownloadData()
        {
            var config = new GoogleSheetsConfig();
            config.InitializeGoogleSheetsService();
            try
            {
                var spreadsheet = config.Service.Spreadsheets.Get(GoogleSheetsSettings.instance.SpreadsheetID);
                spreadsheet.IncludeGridData = true;
                var spreadsheetData = spreadsheet.Execute();

                if (spreadsheetData == null)
                {
                    Debug.LogError($"Failed to download spreadsheet: {GoogleSheetsSettings.instance.SpreadsheetID}");
                    return;
                }

                GenerateCsvFromSpreadsheet(spreadsheetData);
            }
            catch (Exception e)
            {
                GoogleSheetsEditorUtilities.MissingDataPopup(e.Message);
                throw;
            }
        }

        /// <summary>
        ///     Generates a CSV file from the given Google Sheets spreadsheet data.
        /// </summary>
        /// <param name="spreadsheetData">The Google Sheets spreadsheet data to convert into a CSV file.</param>
        /// <exception cref="Exception">Thrown when an error occurs during the CSV generation process.</exception>
        private static void GenerateCsvFromSpreadsheet(Spreadsheet spreadsheetData)
        {
            try
            {
                var csvContents = string.Empty;
                foreach (var row in spreadsheetData.Sheets[0].Data[0].RowData)
                {
                    var rowContents = string.Empty;
                    foreach (var value in row.Values) rowContents += value.FormattedValue + ",";
                    csvContents += rowContents + "\n";
                }

                File.WriteAllText(GoogleSheetsSettings.instance.CsvPath, csvContents);
            }
            catch (Exception e)
            {
                GoogleSheetsEditorUtilities.MissingDataPopup(e.Message);
                throw;
            }
        }
    }
}