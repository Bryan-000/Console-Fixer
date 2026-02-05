# Console Fixer
Fixes the GameConsole's stack trace to show line numbers and file names for ultrakill assemblies.
###### (For your own assemblies you must either add the .pdb file next to the dll or add <DebugType>embedded</DebugType> to the csproj.)

![](https://raw.githubusercontent.com/Bryan-000/Console-Fixer/refs/heads/main/GitAssets/example.png)

### Features
* Adds correct line numbers to ULTRAKILL StackTrace's
* Fixes issue with StackTrace's being forced into one line in the GameConsole
* Makes GameConsole unity logs look like regular logs
* Setting for Logging BepInEx logs to the GameConsole
* Setting for enabling Debug Build, which does:
    * Logging for non-error unity debug logs
    * A bunch of classes log stuff that they usually wouldnt
    * Experimental arm rotation sandbox cheat that allows you to rotate things with the mover arm by pressing [E]
    * Extra debugging sandbox cheats
    * Extra debugging console commands
    * Console Error badge that counts how many errors have occurred
    * Lets the scene command load "special" scenes (whatever that means)
    * Random F1 cursor lockstate debug
    * For some reason every frame enables then disables every enemy in EnemyTracker.GetCurrentEnemies()
    * Uhh doesnt save your rank...? okay maybe i should get rid of some of these
