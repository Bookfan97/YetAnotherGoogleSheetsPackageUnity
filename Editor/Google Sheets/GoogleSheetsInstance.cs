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
        public void UploadData()
        {
            var uploader = new GoogleSheetsUploader();
            uploader.UploadData();
        }

        /// <summary>
        ///     Downloads data from a Google Sheets spreadsheet to a local CSV file.
        /// </summary>
        public void DownloadData()
        {
            var downloader = new GoogleSheetsDownloader();
            downloader.DownloadData();
        }
    }
}