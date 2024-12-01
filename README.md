<!-- Improved compatibility of back to top link: See: https://github.com/othneildrew/Best-README-Template/pull/73 -->
<a id="readme-top"></a>

<!-- PROJECT SHIELDS -->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]

<a href='https://ko-fi.com/U7U2WUGIS' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://storage.ko-fi.com/cdn/kofi6.png?v=6' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>
<a href="https://www.buymeacoffee.com/ndaygamedev" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me A Coffee" style="height: 36px !important;width: 160px !important;" ></a>
<!-- PROJECT LOGO -->
<br />
<div align="center">

<h3 align="center">
    <a href="https://github.com/Bookfan97/YetAnotherGoogleSheetsPackageUnity">
        Yet Another Google Sheets Package for Unity 
    </a>
</h3>

  <p align="center">
    A package for Unity that syncs data between a local csv and the Google Sheet
    <br />
    <a href="https://bookfan97.github.io/YetAnotherGoogleSheetsPackageUnity/manual/"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://github.com/Bookfan97/YetAnotherGoogleSheetsPackageUnity/issues/new?labels=bug&template=bug-report---.md">Report Bug</a>
    ·
    <a href="https://github.com/Bookfan97/YetAnotherGoogleSheetsPackageUnity/issues/new?labels=enhancement&template=feature-request---.md">Request Feature</a>
  </p>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

This package was inspired by a video from [Dan Pos](https://www.youtube.com/@DanPos), where he implemented a similar system using the [Python Scripting](https://docs.unity3d.com/Packages/com.unity.scripting.python@7.0/manual/index.html) package. However, this package is being deprecated in Unity 6.1. I wanted to use this similar system, but rewrite it using native C# code in the editor. This package uses the .dll from the [Localization](https://docs.unity3d.com/Packages/com.unity.localization@1.5/manual/index.html) package for the Google Sheets API.  

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- GETTING STARTED -->
## Getting Started

This is an example of how you may give instructions on setting up your project locally.
To get a local copy up and running follow these simple example steps.

### Prerequisites

Create Project: https://console.cloud.google.com/projectcreate
Once the project is created, open the project in the dashboard. Then select **APIS and Services** -> **Google Drive API**, and enable this permission.

While under the **API & Services Page**, select on the left **Credentials**->**Manage Service Accounts** and create a service account. Keep in mind to make this account the owner.

Once the account is created, in the dropdown select **Manage Keys** then **Add Key**. For the key type, select **JSON** as this will be used in the Editor to verify the account. 

Share the spreadsheet to the email address created with the service account, with the editor role.

Enable API: https://console.cloud.google.com/apis/api/sheets.googleapis.com

Get Spreadsheet ID and the Sheet IDs from the URL:
  ```
  https://docs.google.com/spreadsheets/d/[Your Spreadsheet ID Here]/edit?gid=[Your Sheet ID Here]]#gid=[Your Sheet ID Here]]
  ```

### Installation

Use [UPM](https://docs.unity3d.com/Manual/upm-ui.html) to install the package via the following git URL: `https://github.com/Bookfan97/YetAnotherGoogleSheetsPackageUnity.git` or download and import the [unity package]() manually.

Set the correct variables in either the project settings or Tools > Google Sheets > Open Preferences

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- USAGE EXAMPLES -->
## Usage

Syncing Data between Unity Editor and a Google Spreadsheet

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ROADMAP -->
## Roadmap

- [X] Upload/Download Support
- [ ] CSV -> Scriptable Objects Support
- [X] Multiple Sheet Support
- [ ] Sheet Styling

See the [open issues](https://github.com/Bookfan97/YetAnotherGoogleSheetsPackageUnity/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Top contributors:

<a href="https://github.com/Bookfan97/YetAnotherGoogleSheetsPackageUnity/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=Bookfan97/YetAnotherGoogleSheetsPackageUnity" alt="contrib.rocks image" />
</a>



<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTACT -->
## Contact

Project Link: [https://github.com/Bookfan97/YetAnotherGoogleSheetsPackageUnity](https://github.com/Bookfan97/YetAnotherGoogleSheetsPackageUnity)

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

* [Original Video from Dan Pos](https://www.youtube.com/watch?v=QFTyDsEDsBI)

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/Bookfan97/YetAnotherGoogleSheetsPackageUnity.svg?style=for-the-badge
[contributors-url]: https://github.com/Bookfan97/YetAnotherGoogleSheetsPackageUnity/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/Bookfan97/YetAnotherGoogleSheetsPackageUnity.svg?style=for-the-badge
[forks-url]: https://github.com/Bookfan97/YetAnotherGoogleSheetsPackageUnity/network/members
[stars-shield]: https://img.shields.io/github/stars/Bookfan97/YetAnotherGoogleSheetsPackageUnity.svg?style=for-the-badge
[stars-url]: https://github.com/Bookfan97/YetAnotherGoogleSheetsPackageUnity/stargazers
[issues-shield]: https://img.shields.io/github/issues/Bookfan97/YetAnotherGoogleSheetsPackageUnity.svg?style=for-the-badge
[issues-url]: https://github.com/Bookfan97/YetAnotherGoogleSheetsPackageUnity/issues
[license-shield]: https://img.shields.io/github/license/Bookfan97/YetAnotherGoogleSheetsPackageUnity.svg?style=for-the-badge
[license-url]: https://github.com/Bookfan97/YetAnotherGoogleSheetsPackageUnity/blob/master/LICENSE.txt

[product-screenshot]: images/screenshot.png
