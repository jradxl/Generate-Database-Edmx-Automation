#Generate Database Edmx Automation
=================================

###Automation of the Generate Database Wizard in Entity Frameworks Model First  
  
If you use Model-First in Entity Frameworks you'll know about the Generate Database Wizard and the Update Model from Database Wizard.

When you've made changes to your Model, it is typical to run the Generate Database Wizard to create default mappings and generate the SQL to create the database. This requires manual operation of the Wizard and is quite boring!

The Entity Framework Designer does not support custom field mapping. Using the Generate Database Wizard creates new Ssdl and Msl sections of the Edmx file - you'll recall the first-time overwrite warning dialog that the Wizard shows.

Since a new Ssdl and Msl is generated, why not automate this.
Once a new Ssdl and Msl has been generated and saved to the Edmx, the annoying thing is the Edmx designer is still showing the previous state.

In this repository, I provide a VsPackage containing an MEF loaded Extensibility method to customise the Edmx save, where optionally the Ssdl and Msl can be generated. Also optionally the save of the Edmx can be enabled to refresh the Designer. The VsPackage also provides a new Context Menu item on the Designer surface, called Refresh Designer, which updates the Designer with the new mappings, as an alternative to doing this in the save.
Since the Ssdl and Msl are regenerated on Save, they will be also regenerated on an F5 project build.

In the current version the SQL to create the Database is not generated.

Features:  
Two Properties showing in the Properties window when the Edmx designer surface is selected, under the grouping EdmxFilesTools.  
Enabled - enables generation of the Ssdl and Msl.  
Refresh Designer - enables the refresh of the designer using this new context menu.  

Operation:  
Add one or more .edmx files to your project.   
Use the Generate Database Wizard to provide a database and connection string. This first-time use is the only use you'll need.  
As normal, using the Toolbox, add the Entities and Properties.
Select the Designer surface and enable the two properties in the EDMX File Tools section.  
Now, when the Edmx is saved a new Ssdl and Msl will be created with updated mappings and the Designer will be updated to the new mappings. Validate will not show any errors.


JsrSoft
June 2013
 


