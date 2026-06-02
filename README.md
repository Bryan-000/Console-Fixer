# Console Fixer
Fixes bugs and issues with Ultrakill's GameConsole (F8) and makes it better for mod development.

![meow](https://raw.githubusercontent.com/Bryan-000/Console-Fixer/main/GitAssets/example.png "ur a gay twink :3c")

### Features
* Adds correct line numbers to ULTRAKILL StackTraces.
* Makes GameConsole Unity logs look like regular GameConsole logs.
* Disables log filtering. (Filtering can be modified in Settings)
    * (For adding line numbers to your mod's logs, add `<DebugType>embedded</DebugType>` to your `.csproj`).
* Adds assembly names to Unity StackTraces for global types.
* Logs BepInEx logs to the GameConsole (can be disabled via Settings).
* Expands StackTraces in the GameConsole across multiple lines for readability.
* Tricks the game into believing it's running in a Debug Build, which does:
    * Extra debugging sandbox cheats
    * Extra debugging console commands
    * Ultrakill log's a lot of information that it usually wouldn't
    * A console error badge that counts errors
    * Allows the GameConsole 'scene' command to load "special" scenes
    * An experimental arm rotation sandbox cheat for rotating things with the mover arm
###### (Can be disabled via Settings)

##### Settings can be modified (without needing to reload) via PluginConfigurator, Configgy, R2MM, and BepInEx's `.cfg` files.