# VS add-in for D365FO
An extension to create the security privilege for the current project

* [How to install the extension](#how-to-install-the-extension)
* [Features](#features)
  * [Create Maintain and Inquire Privilege for the current Project](#create-maintain-and-inquire-Privilege-for-the-current-Project)
  
# How to install the extension
  
**Option 1: Manual installation**
1. Download all the dll files from the folder [OutputDlls](OutputDlls) 
2. Close Visual studio
3. Locate the folder where Visual Studio has the Dynamics 365 extensions by searching "Microsoft.Dynamics.Framework.Tools*" in the following location: C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\Extensions
4. Copy the dll files downloaded into the folder
5. Open Visual studio

# Features

## Create Maintain and Inquire Privilege for the current Project
You can create a Maintain and a View Privilege for each of the objects contained in the current Project. Just have to navigate to Extensions / Dynamics365 / Add-ins / Create Maintain and Inquire Privilege for the current Project (iD365FOnt)

This will create both privileges containing all the permissions of the objects contained in the project:

 - Menu Items

The name of the privileges will be the object Name by default with the "Maintain" or "View" preffix.
