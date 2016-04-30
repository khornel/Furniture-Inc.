How to install:
Put "Furniture Inc.dll" in "DLLMods" folder with Software Inc.
Put all furniture xml, obj and png files in a subfolder under "DLLMods/Furniture/"

You can find references to UnityEngine.dll, UnityEngineUI.dll and Assembly-CSharp.dll in the Software Inc. install folder under "/Software Inc_Data/Managed/"

TODO:
Expand to enable roomsegment and fence modding

Done:
Add version number manually
Check sub folders of Furniture folder to avoid filename collision from different furniture packages
Manual furniture name collision avoidance
Add important info to the status log in the options menu
Implement proper error handling
Might need to change when/how furniture is loaded to enable adding custom behaviors from other mods
