# Heimdallr

## Getting Started

1. Clone the project
2. Init the submodules with `git submodules update --init --recursive`
3. Open the project with Unity

## Project Structure

```
UnityClient
-> Assets
--> 3rdparty - Libs and whatever
---> unityro-core - Contains RO specific logic for rendering, moving, etc
---> unityro-io - Contains GRF specific code
---> unityro-net - Contains code needed to connect to emulators
--> Configuration - Project configuration files
--> Plugins - DLLs folders
--> Resources - Textures, Shaders, Models, ScriptableObjects etc