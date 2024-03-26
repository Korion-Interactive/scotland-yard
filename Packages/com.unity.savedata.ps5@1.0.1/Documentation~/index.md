# Save Data package

The Save Data package provides access to the PlayStation 5 save system.
 
The package contains the following:

* A C# API which provides asynchronous calls to the PS5 save data system, including the PS5 save data system dialogs
* A native built prx library which provides the native plugin between the C# API and the calls to the PS5 Sony libraries. The C++ source code for this library is also available in the `source~` folder. 
 
The purpose of this package is to provide access to the Save Data system, which is further documented on Sony's developer website. For more information, see the [External links](ExternalLinks.md) page.

This documentation __doesn't__ cover any TRC (Technical Requirements Checklist) that your title must adhere to. Please check the Sony documentation for latest compliance requirements.

For an overview of supported features, see documentation on [C# API features](Features.md)

HTML Documentation for this package is only available locally, in the package folder, due to Sony's NDA requirements for PS5 development. For more information, see [Accessing package documentation](ViewHTML.md).
