How to install:
Put "Furniture Inc.dll" in "DLLMods" folder with Software Inc.
Put all furniture xml, obj and png files in a subfolder under "DLLMods/Furniture/"

You can find references to UnityEngine.dll, UnityEngine.UI.dll and Assembly-CSharp.dll in the Software Inc. install folder under "/Software Inc_Data/Managed/"

TODO:
Cache meshes and thumbnails
Expand to enable roomsegment and fence modding


Done:
Ability to remove components in XML file
Ability to set computer mesh
Ability to have mesh hierarchy for meshes and snappoints
Handle errors when reading file and parsing xml
Add version number manually
Check sub folders of Furniture folder to avoid filename collision from different furniture packages
Manual furniture name collision avoidance
Add important info to the status log in the options menu
Implement proper error handling
Might need to change when/how furniture is loaded to enable adding custom behaviors from other mods
