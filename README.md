# StardewUpdater
An updater for SMAPI and mods of Stardew Valley. Automatic version detection with backups, settings and dependency checking

## Introduction
----
This project is to provide a unified updater for StardewValley. In addition to the mod updates, 
updates for SMAPI itself should be managed, up to prefabricated mod packages as a configuration 
file.

During updates dependencies as well as backups shall be managed. Only mods from Nexusmod will 
be managed automatically, because this project will be updated via the API of Nexusmod 
(https://app.swaggerhub.com/apis-docs/NexusMods/nexus-mods_public_api_params_in_form_data/1.0).

In current version GoG and Steam installations will be recognized. However, through the configuration file, 
the installation path can be specified in the later release and Epic can be used like other installation 
locations. Tests for MacOS are still pending, while Windows will work.
----

## How it works and what you need
----
Based on C#, .NETFramework v4.X and Windows Presentation Foundation (WPF for short), a lean UI is used.

To use the project a custom apikey is used. This must be obtained from Nexusmod and belong to a premium account, otherwise the Api for the files can not be accessed.

The project can, in its base, also be used for other games than Stardew Valley. For this the gameId and the gameName have to be adapted as well. All can be found in StardewUpdater.cs in the upper area. With later releases the project can become more Stardew specific.

In the constructor, the appsettings.json is generated during the initialization process. Here default values can be adjusted.
WebSearchStringForLatestVersion is used for retrieving the SMAPI Version from Github. InstallationScript cant be changed to define where the Programm should search for SteamGamesFolders.

```c#
sw.WriteLine("\"WebSearchStringForLatestVersion\": \"href=\\\"/Pathoschild/SMAPI/releases/tag/\",");
sw.WriteLine("\"ApiKey\": \"xxxxx\",");
sw.WriteLine("\"GameName\": \"Stardew Valley\",");
sw.WriteLine("\"SteamGameId\": \"413150\",");
sw.WriteLine("\"InstallationScript\": \"C:/Program Files (x86)/Steam/steamapps/libraryfolders.vdf\",");
```
----

## NuGet packages

| Plugin | Version | Project |
| ------ | ------ | ------ |
| DotNetZip | 1.16.0 | https://github.com/haf/DotNetZip.Semverd |
| Newtonsoft.Json | 13.0.1 | https://www.newtonsoft.com/json |
| SevenZipExtractor | 1.0.16 | https://github.com/adoconnection/SevenZipExtractor |
| NLog | 4.7.15 | https://nlog-project.org/ |
| Gameloop.VDF | 0.6.2 | https://github.com/shravan2x/Gameloop.Vdf |


## INFORMATION
```diff
- The project is currently paused due to missing data for different machines. Basically the program works 
- as described in the changelogs. Depending on the machine, different errors can occur as also described below.

- I am always available for further questions and enhancements. Contact: social@kirch.tech
```

## Changelogs
----
#### Known Issues
- Dependencies sometimes are missinterpreted as Mods and show up
- Sometimes the init process is stuck in the getInstallationFolder Method
- Background work is still not done correctly
- SMAPI Update is stuck
 
### Version 0.5.0
- Removed static appsettings file
- Added Special Folder Documents(Personal) with Configuration and appsettings
- Added Logging with NLog in all Methods
- Added User Interaction when Programm can't find or delete something
- Implemented UpgradeWindow
- Optimised Delete Process with FileSystemWatcher
- Optimised Steam Folder Game Detection with vdf file

### Version 0.4.0
- Implement SettingsPage
- Implement Export of local ModConfiguration
- Added appsettings for simplyfied reaction to changes of third parties
- Added ExtensionMethods to cleanup code
- Implement CheckConfigurationFile
- Fully implement getAllMods to get 90% of Mod directories (some are strange build... Will be fixed in future Update)
- Implement setNewestSmapiVersion to start the SMAPI Update process automatically
- Implement Download and Install button. Downloads an SVU-ModConfigImport.json file and runs through it configuration
- Implement Update button. Now Updates all selected Mods, if possible
- Implement Backup functionality
- Implement Delete functionality of selected Mods
- Fixed: Now reads Configfile correctly
- Refactoring implementation of MainWindow (Mainly on Invokes and Process of Method calling)

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
