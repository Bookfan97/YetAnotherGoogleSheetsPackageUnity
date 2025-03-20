namespace Editor.Google_Sheets
{
    /// <summary>
    ///     Manages the interactions with Google Sheets by providing functionalities for uploading and downloading data.
    /// </summary>
    public class GoogleSheetsInstance
    {
        /// <summary>
        ///     Uploads data from a local CSV file to a Google Sheets spreadsheet.
        /// </summary>
        public void UploadAllData()
        {
            var uploader = new GoogleSheetsUploader();
            uploader.UploadAllData();
        }

        /// <summary>
        ///     Downloads data from a Google Sheets spreadsheet to a local CSV file.
        /// </summary>
        public void DownloadAllData()
        {
            var downloader = new GoogleSheetsDownloader();
            downloader.DownloadAllData();
        }

        /// <summary>
        ///     Uploads data from a local CSV file to a Google Sheets spreadsheet.
        /// </summary>
        /// <param name="dataItem"></param>
        public void UploadSheetData(DataItem dataItem)
        {
            var uploader = new GoogleSheetsUploader();
            uploader.UploadSheetData(dataItem);
        }

        /// <summary>
        ///     Downloads data from a Google Sheets spreadsheet to a local CSV file.
        /// </summary>
        /// <param name="dataItem"></param>
        public void DownloadSheetData(DataItem dataItem)
        {
            var downloader = new GoogleSheetsDownloader();
            downloader.DownloadSheetData(dataItem);
        }
    }
}