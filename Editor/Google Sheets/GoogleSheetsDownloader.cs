using System;
using System.IO;
using System.Linq;
using Editor.Data;
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
        ///     Downloads data from all the Google Sheets spreadsheet specified in the project settings.
        /// </summary>
        /// <remarks>
        ///     This method initializes the Google Sheets service, retrieves the spreadsheet data,
        ///     and then generates a CSV file from the downloaded data. If an error occurs during
        ///     the download, an error message is logged and the exception is rethrown.
        /// </remarks>
        /// <exception cref="Exception">Thrown when an error occurs during the download process.</exception>
        public void DownloadAllData()
        {
            var config = new GoogleSheetsConfig();
            config.InitializeGoogleSheetsService();
            try
            {
                var spreadsheet =
                    config.Service.Spreadsheets.Get(GoogleSheetsHelper.GoogleSheetsCustomSettings.MSpreadsheetID);
                spreadsheet.IncludeGridData = true;
                var spreadsheetData = spreadsheet.Execute();

                if (spreadsheetData == null)
                {
                    Debug.LogError(
                        $"Failed to download spreadsheet: {GoogleSheetsHelper.GoogleSheetsCustomSettings.MSpreadsheetID}");
                    return;
                }

                GenerateCsvsFromSpreadsheets(spreadsheetData);
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
        private static void GenerateCsvsFromSpreadsheets(Spreadsheet spreadsheetData)
        {
            try
            {
                var index = 0;
                foreach (var sheet in spreadsheetData.Sheets)
                {
                    if (index < GoogleSheetsHelper.GoogleSheetsCustomSettings.Data.Count) break;
                    if (sheet == null) continue;
                    if (JSONUtility.GoogleSheetsJsonData.CheckSheetIDInSheets(sheet.Properties.SheetId)) continue;
                    var csvContents = string.Empty;
                    if (sheet.Properties.SheetId == null) continue;
                    var sheetCSVPath = string.Empty;

                    sheetCSVPath = GoogleSheetsHelper.GoogleSheetsCustomSettings
                        .GetPathForSheet((int)sheet.Properties.SheetId);
                    foreach (var row in sheet.Data[0].RowData)
                    {
                        var rowContents = string.Empty;
                        foreach (var value in row.Values) rowContents += value.FormattedValue + ",";
                        csvContents += rowContents + "\n";
                    }

                    File.WriteAllText(sheetCSVPath, csvContents);
                    index++;
                }
            }
            catch (Exception e)
            {
                GoogleSheetsEditorUtilities.MissingDataPopup(e.Message);
                throw;
            }
        }

        /// <summary>
        ///     Downloads data from the Google Sheets spreadsheet specified in the parameters.
        /// </summary>
        /// <remarks>
        ///     This method initializes the Google Sheets service, retrieves the spreadsheet data,
        ///     and then generates a CSV file from the downloaded data. If an error occurs during
        ///     the download, an error message is logged and the exception is rethrown.
        /// </remarks>
        /// <exception cref="Exception">Thrown when an error occurs during the download process.</exception>
        public void DownloadSheetData(DataItem dataItem)
        {
            var config = new GoogleSheetsConfig();
            config.InitializeGoogleSheetsService();
            try
            {
                var spreadsheet =
                    config.Service.Spreadsheets.Get(GoogleSheetsHelper.GoogleSheetsCustomSettings.MSpreadsheetID);
                spreadsheet.IncludeGridData = true;
                var spreadsheetData = spreadsheet.Execute();

                if (spreadsheetData == null)
                {
                    Debug.LogError(
                        $"Failed to download spreadsheet: {GoogleSheetsHelper.GoogleSheetsCustomSettings.MSpreadsheetID}");
                    return;
                }

                GenerateCsvFromSpreadsheet(spreadsheetData, dataItem);
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
        /// <param name="dataItem"></param>
        /// <exception cref="Exception">Thrown when an error occurs during the CSV generation process.</exception>
        private void GenerateCsvFromSpreadsheet(Spreadsheet spreadsheetData, DataItem dataItem)
        {
            try
            {
                if (dataItem == null && JSONUtility.GoogleSheetsJsonData.CheckSheetIDInSheets(dataItem.index)) return;
                var sheetCSVPath = string.Empty;
                var csvContents = string.Empty;
                var sheet = spreadsheetData.Sheets.FirstOrDefault(i => i.Properties.SheetId == dataItem.key);
                if (sheet.Properties.SheetId != null)
                    sheetCSVPath = GoogleSheetsHelper.GoogleSheetsCustomSettings
                        .GetPathForSheet((int)sheet.Properties.SheetId);
                foreach (var row in sheet.Data[0].RowData)
                {
                    var rowContents = string.Empty;
                    foreach (var value in row.Values) rowContents += value.FormattedValue + ",";
                    csvContents += rowContents + "\n";
                }

                File.WriteAllText(sheetCSVPath, csvContents);
            }
            catch (Exception e)
            {
                GoogleSheetsEditorUtilities.MissingDataPopup(e.Message);
                throw;
            }
        }
    }
}