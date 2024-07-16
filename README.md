WIP. Open source Unity editor tool for auto building with custom presets.
This open source version is a refactor of a private tool, has some known issues that make it not ready for production yet. If need the original private version, send me a message to: lucas.leonardo.conti@gmail.com.

Reedme under construction too.

Do you had a lot of steps for building your Unity project? Then this is your tool.

How to use:
- Create a "Build Wizard" → "New Preset".
- Configurate the steps you want to use.
- Open "Wizard Runner window", select your preset and press "Execute Runner".
- All the steps will show on the console log.

![image](https://github.com/user-attachments/assets/2c138607-c024-4a1e-b41c-dcd13499b8f6)

**Know Issues**
- Repository don't persist data before the Change Platform step.
- Wizard Preset dropdown don't show all the avalible steps because an assembly problem (for example the Oculus platform steps).
- Wizard Runner don't had final visual.
- Wizard Runner show the Runner log on console and not on the Runner window.
