# Console Fixer
Fixes the GameConsole's stack trace to show line numbers and file names for ultrakill assemblies.
###### (if you make a mod then you have to put the generated .pdb file next to your mods dll for it to have accurate line numbers)

![](https://raw.githubusercontent.com/Bryan-000/Console-Fixer/refs/heads/main/GitAssets/example.png)

### Features
- Adds correct line numbers to ULTRAKILL StackTrace's
- Fixes issue with StackTrace's being forced into one line in the GameConsole
- Makes GameConsole unity logs look like regular logs
- Setting for Logging BepInEx logs to the GameConsole
- Setting for enabling Debug Build (which also makes all unity logs log to bepinex and gameconsole :3)