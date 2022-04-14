# VS add-in for D365FO
An extension to create the security privilege for the current project

* [How to install the extension](#how-to-install-the-extension)
* [Features](#features)
  * [Create Maintain and Inquire Privilege for the current Project](#create-maintain-and-inquire-Privilege-for-the-current-Project)
  
# How to install the extension

**Option 1: Powershell Script**
  ```Powershell
  iex (iwr "https://github.com/iD365FOnt/iD365FOntTools/blob/main/Misc/install.ps1").Content
  ```
  
**Option 2: Manual installation**
1- Download all the dll files from the folder [OutputDlls](OutputDlls) 
2- Create a new folder e.g. c:\iD365FOnt
3- Copy the dll files downloaded into the new folder created
4- Close Visual studio
5- Edit the file: C:\Users\<currentUser>\Documents\Visual Studio Dynamics 365\DynamicsDevConfig.xml
6- Edit the following to the xml file
```xml
<?xml version="1.0" encoding="utf-8"?>
<DynamicsDevConfig xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/dynamics/2012/03/development/configuration">
	<AddInPaths xmlns:d2p1="http://schemas.microsoft.com/2003/10/Serialization/Arrays">
		<d2p1:string>c:\iD365FOnt</d2p1:string>
	</AddInPaths>
</DynamicsDevConfig>
```


# Features

## Create Maintain and Inquire Privilege for the current Project
You can create a Maintain and a View Privilege for all the objects contained in the current Project. Just have to navigate to Extensions / Dynamics365 / Add-ins / Create Maintain and Inquire Privilege for the current Project (iD365FOnt)

This will create both privileges containing all the permissions of the objects contained in the project:

 - Menu Items
 - Tables
 - Data Entities

The name of the privileges will be the Project Name by default with the "Maintain" or "View" suffix.