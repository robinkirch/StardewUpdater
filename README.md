# StardewUpdater
An updater for SMAPI and mods of Stardew Valley. Automatic version detection with backups

## Introduction
----
This project is to provide a unified updater for StardewValley. In addition to the mod updates, 
updates for SMAPI itself should be managed, up to prefabricated mod packages as a configuration 
file.

During updates dependencies as well as backups shall be managed. Only mods from Nexusmod will 
be managed automatically, because this project will be updated via the API of Nexusmod 
(https://app.swaggerhub.com/apis-docs/NexusMods/nexus-mods_public_api_params_in_form_data/1.0).

In current version Gog and Steam installations are recognized. Tests for MacOS are still pending, 
while Windows works fine.
----

## How it works and what you need
----
Based on C#, .NETFramework v4.X and Windows Presentation Foundation (WPF for short), a lean UI is used.

To use the project a custom apikey is used. This must be obtained from Nexusmod and belong to a premium account, otherwise the Api for the files can not be accessed.

The project can also be used for games other than Stardew Valley. For this, the gameId and the gameName must also be adapted. All be found in Form1.cs on top.
```c#
private readonly string _apikey = "XXXX";
```

```c#
private readonly string gameId = "1303";
```

```c#
private readonly string gameName = "Stardew Valley";
```

For other games currently all Webrequest across the project have to be edited.
----

##NuGet packages

| Plugin | Version | Project |
| ------ | ------ | ------ |
| DotNetZip | 1.16.0 | https://github.com/haf/DotNetZip.Semverd |
| Newtonsoft.Json | 13.0.1 | https://www.newtonsoft.com/json |
| SevenZipExtractor | 1.0.16 | https://github.com/adoconnection/SevenZipExtractor |

## Changelogs
----
### Version 0.3.0 before Repository-upload
- Implement Download for Mods

### Version 0.2.5 before Repository-upload
- Implemented Configuration in all Methods
- Restructured UI

### Version 0.2.0 before Repository-upload
- Added Configuration class
- Implemented Base Methods
  - Steam installations across multiple drives can be found
  - GoG currently not implemented
- Added finer UI structure
- fixed Invoke error

### Version 0.1.0 before Repository-upload
- Added Mods class
- Added Base Methods
- Added Base Layout for rough structure
----
