
## 🟥 NOTICE 🟥
_This is WIP and version 1.0.0 will be released soon on OpenNPM! (Expected July 31 - 2023)_


![253230074-37fe6e8a-52dc-44cd-ae0a-321c54c53c1b](https://github.com/Ale1/Splashdown/assets/4612160/530b18c7-23b7-4bf4-b30b-5318b9c4bd62)



A Unity open-source splash and icon tool that will generate custom icons with user-provided text.  Allows for dynamic scripted content and to be incorporated into your build pipeline. 

Its purpose is to aid in quick iteration of unity mobile games, and allow no-code team members such as Q.A testers, designers and general playtesters to quickly identify the build variant that is installed on their mobile device.  
A quick glimpse at the generated icons and logos can give build release information without the need for opening the app or creating custom unity splashes.  
The customizable text can be used to show things like date, author, build version, unity version, short release notes, beta tags, disclaimers, copyright notices, etc. 


| ![splashdown1](https://github.com/Ale1/Splashdown/assets/4612160/292d322b-bc9f-4154-a9ba-581f51c466ca) | ![IMG_B492065383A8-1](https://github.com/Ale1/Splashdown/assets/4612160/9f02fcfd-6790-4f02-abb6-71b9c405ff01) |




# Installation
wip



# Getting Started 
## (1) Hello World
Right-click on any location within the Assets Folder.  Select `Create > New Splashdown`.   
A Splashdown file will appear. 

Fill in the Splashdown Importer window and hit `Apply` button. 

![Screenshot 2023-07-30 at 11 44 55](https://github.com/Ale1/Splashdown/assets/4612160/664ff1b0-5b9e-4b14-b2f3-61316d96eb7f)


Congrats! you generated your first custom Logo/Icon

### Using it as Splash Logo
This generated sprite can be used as a logo in your Player Settings.
By setting hitting `Activate Splash` button, the playerSettings will start using this sprite as its splash image:

|![Screenshot 2023-07-30 at 11 42 27](https://github.com/Ale1/Splashdown/assets/4612160/63fa7c90-bde9-4bba-bdae-91d648395565)|
|:--:| 
| *you can remove it by selecting "Deactive" in the splashdown editor* |

### Using it as App Icon
The same generated sprite can be used as an app icon when you build.  Similarly to Splash logos, you just need to set its icon state to active by pressing "Activate Icon:
> :warning:  **Unlike splash logos, activating an icon does NOT apply it to Player Settings right away.**  It will wait for you to build and do it during the build process.  This is by design, as the icon is meant to be a temporary placeholder during development and applying icons during build process allows for a safer restore of your previous icons after build process is complete.   




<b>If you wish to automatate the contents of the Logo/Icon, proceed... </b>
<br/><br/>

## (2) Dynamic Options

The Dynamic Options feature will allow you to quickly update the splashdown file without manually typing in info. Its particularly useful for allowing splashdown to keep track of Dates or build versions. 
Create a script like below and place it anywhere in your project. Though its recommended that you place it in an `Editor` folder. 
The splashdown importer will automatically invoke methods with the `[Splashdown.OptionsProvider]` attribute whenever its refreshed. 
Dynamically created Options will override any manual inputs in hte splashdown importer.

```csharp

public static class Example  
{
        [Splashdown.OptionsProvider] //Method with this atrribute must return a Splashdown.Options
        public static Splashdown.Options ProvideSplashdownOptions() => new()
        { 
                line1 = Application.unityVersion, // e.g 2021.3.4f
                line2 = DateTime.Now.ToShortDateString(),
                line3 = Application.version, //e.g 2.1.0
                textColor = Color.red,

                // no need to set all of available options. Those without values will use the manual values instead.
                // backgroundColor = Color.blue
        };
}
```


<b>If you wish to automatically include the splashdown icon on every build, proceed....</b>
<br/><br/>


## (3) Add splashdown to your builds 


Any splashdown file that is set to `ACTIVE` will regenerate when unity is building. 
This means the latest values from dynamic options will be used, as well as adding the splash and/or logo to your Player Settings.  
If you dont desire this behaviour, simply leave the splashdown files in `INACTIVE` state.  
If you always want full manual control, simply drag-and-drop the generated sprite as needed to your Player Settings. 


## (4) Add Splashdown to your build pipeline through CLI


below will activate the splashdown file with the provided filename, and apply the options. 
```shell
_yourUnityPath_ -batchmode -quit -projectPath _yourProjectPath_ -executeMethod Splashdown.Editor.CLI.SetSplashOptions -name MySplashdown -l1 hello -l2 banana -l3 world
```

```shell
//Mandatory param:
-name MySplashdown // the name of the splashdown file to apply. note that the default name is "MySplashdown" but you can replace with target filename.

//Optional Flags:
-enable_splash // use as splash logo
-enable_icon // use as app icon
-disable_splash // remove logo from splash
-disable_icon") // remove icons and restore previous icons
-enableDynamic //sets dynamic options to false
-disableDynamic //sets dynamic options to true
```

Example for MacOS:
```shell
 /Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectPath ~/Desktop/MyCoolGame -executeMethod Splashdown.Editor.CLI.SetSplashOptions -name MySplashdown -activeSplash -l1 hello -l2 cruel -l3 world
```
Example for Windows:
```shell
"C:\Program Files\Unity\Editor\Unity.exe" -quit -batchmode -projectPath "C:\Users\UserName\Documents\MyProject" -executeMethod Splashdown.Editor.CLI.SetSplashOptions -name "MySplashdown" -disableDynamic -l1 "Banana"
MyEditorScript.PerformBuild
```

# Advanced Customization


## Conflicting Options Resolution

Since you can provide options through multiple avenues, keep in mind the order of priority below (most to least).  

(1) Dynamic Options  

(2) CLI Options

(2) Options set manually in inspector

(3) Default Options

Options with higher Priority will override ones with lower priority. 


## Managing multiple Splashdown files

Its possible to have multiple Splashdown assets in your project.   Keep in mind that they all must have unique names. 
How the system handles multiple Splashdown files varies between splash logos and app icons: 

### Multiple Splash logos
You can have any number of splashdown files set to "Active Splash".  All the Splash logos will be present sequentially in your splash screen. 
When you activate/deactivate a splash in the Splashdown inspector, it is automatically added or removed from your Player Settings in editor-time.

### Multiple App Icons
For App icons, the Player settings dont allow for multiple icons for each category.  Hence, when you activate an Icon, the system will check for other existing activated icons and disable those first.  Hence, it will never allow you to have more than one icon activated at a time.  This silent behaviour will likely be modified in the future so a warning appears when trying to activate an icon when another icon is already active in that role. 


## Switching Fonts [Experimental]
The switching fonts feature is still WIP and its not guaranteed to work for custom provided fonts.  For now, I recommend sticking to the default Roboto_mono font.   New built-in fonts will be provided in the next release and the ability to use custom fonts is high-priority in the roadmap for future releases. 

## Customizing the Border
Customizing the border is possible, but the effect is purely cosmetic so as a feature it is not high-priority in the roadmap.  If there is a use-case where modifying the borders provides an additional benefit other than cosmetic, pls open an issue describing your use-case and I will gladly look into it.  


## using a texture as background or watermark
+ WIP


# Supported Unity Versions

| Version   | Supported                |
| --------  | ------------------------ |
| < 2021.X  | ✖️ not supported         |
| 2021.X    | ✅ supported             |
| 2022.X    | ❔ supported (untested)  |
| 2023.X    | ❔ supported (untested)  |


# FAQ
+ **My Dynamic options are overriding the options passed through CLI!?**  

You can disable the dynamic options with the optional flag provided.  You can also simply have the DynamicOptions only override a certain line (e.g line1) and leave the rest for the CLI options to fill. See Conflicting Options Resolution section. 

+ **I want to use the generated logo for something else, how can I extract the texture/sprite from the splashdown file?**
  
A Sprite is generated and saved as a sub-asset of the splashdown file. you can copy it to your clipboard from the project hierarchy, and paste it elsewhere in your project to get a clone.

+ **Can I use asian alphabets (e.g kanji) ?**   

Yes!  but you will likely need to provide your own font as the built-in fonts are very limited in the amount of characters available.  See instructions for switching fonts and keep in mind feature is still experimental. 


  
