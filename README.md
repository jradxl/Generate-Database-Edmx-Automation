#Generate Database Edmx Automation
=================================

###Automation of the Generate Database Wizard in Entity Frameworks Model First  
  
If you use Model-First in Entity Frameworks you'll know about the Generate Database Wizard and the Update Model from Database Wizard.

When you've made changes to your Model, it is typical to run the Generate Database Wizard to create default mappings and generate the SQL to create the database. This requires manual operation of the Wizard and is quite boring!

The Entity Framework Designer does not support custom field mapping. Using the Generate Database Wizard creates new Ssdl and Msl sections of the Edmx file - you'll recall the first-time overwrite warning dialog that the Wizard shows.

Since a new Ssdl and Msl is generated, why not automate this.
Once a new Ssdl and Msl has been generated and saved to the Edmx, the annoying thing is the Edmx designer is still showing the previous state.

In this repository, I provide a VsPackage containing an MEF loaded Extensibility method to customise the Edmx save, where optionally the Ssdl and Msl can be generated. The VsPackage also provides a new Context Menu item on the Designer surface, called Refresh Designer, which updates the Designer with the new mappings.

A future development will be the optional Designer update when the Edmx is saved.

In the current version the SQL to create the Database is not generated.

JsrSoft
June 2013
 


