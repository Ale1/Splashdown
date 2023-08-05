

| ![253230074-37fe6e8a-52dc-44cd-ae0a-321c54c53c1b](https://github.com/Ale1/Splashdown/assets/4612160/530b18c7-23b7-4bf4-b30b-5318b9c4bd62) | [![openupm](https://img.shields.io/npm/v/com.ale1.splashdown?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.ale1.splashdown/) <br> [![github](https://img.shields.io/static/v1?label=GitHub&message=Ale1.Splashdown)](https://github.com/Ale1/Splashdown) |
|-|-|


# Splashdown
A Unity open-source splash and icon tool that will generate custom icons with user-provided text.  Allows for dynamic scripted content and to be incorporated into your build pipeline. 

Its purpose is to aid in quick iteration of unity mobile games, and allow no-code team members such as Q.A testers, designers and general playtesters to quickly identify the build variant that is installed on their mobile device.  
A quick glimpse at the generated icons and logos can give build release information without the need for opening the app or creating custom unity splashes.  
The customizable text can be used to show things like date, author, build version, unity version, short release notes, beta tags, disclaimers, copyright notices, etc. 


| ![splashdown1](https://github.com/Ale1/Splashdown/assets/4612160/292d322b-bc9f-4154-a9ba-581f51c466ca) | ![IMG_B492065383A8-1](https://github.com/Ale1/Splashdown/assets/4612160/9f02fcfd-6790-4f02-abb6-71b9c405ff01) |




# Installation
## Install via OpenUPM (recommended)

1) Install openupm-cli via npm if you havent installed it before. => `npm install -g openupm-cli`)
2) Install com.ale1.splashdown via command line. Make sure to run this command at the root of your Unity project =>openupm add com.ale1.splashdown

## Install via Git URL

Add the following line to the dependencies section of your project's manifest.json file

"com.ale1.splashdown": "git+https://github.com/Ale1.Splashdown.git#1.0.0"
The 1.0.0 represents the version tag to be fetched. Although it is discouraged, you can replace with `master` to always fetch the latest.


# Getting Started 
## (1) Hello World
Right-click on any location within the Assets Folder.  Select `Create > New Splashdown`.   
A Splashdown file will appear. 

Fill in the Splashdown Importer window and hit `Apply` button. 

![Screenshot 2023-07-30 at 11 44 55](https://github.com/Ale1/Splashdown/assets/4612160/664ff1b0-5b9e-4b14-b2f3-61316d96eb7f)


Congrats! you generated your first custom Logo/Icon

### (2) Using it as Splash Logo
This generated sprite can be used as a logo in your Player Settings.
By setting hitting `Activate Splash` button, the playerSettings will start using this sprite as its splash image:

|![Screenshot 2023-07-30 at 11 42 27](https://github.com/Ale1/Splashdown/assets/4612160/63fa7c90-bde9-4bba-bdae-91d648395565)|
|:--:| 
| *you can remove it by selecting "Deactive" in the splashdown editor* |

### (3) Using it as App Icon
The same generated sprite can be used as an app icon when you build.  Similarly to Splash logos, you just need to set its icon state to active by pressing "Activate Icon:
> :warning:  **Unlike splash logos, activating an icon does NOT apply it to Player Settings right away.**  It will wait for the start of a app build process.  This is by design, as the icon is meant to be a temporary placeholder during development and applying icons during build process allows for a safer restore of your previous icons after build process is complete.   


## (4) Dynamic Options

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

Dynamic options can accept an optional parameter `name` that filters by filename (without extension), so it will only apply to specific Spashdowns. E.g: 

```csharp
[Splashdown.OptionsProvider("MyBlueLogo")]
//will only affect options of splashdown with name: 'MyBlueLogo.splashdown'
public static Splashdown.Options ProvideSplashdownOptions() => new()
{ 
         ... 
}
```



## (5) Add splashdown to your builds 


Any splashdown file that is set to `ACTIVE` will regenerate when unity is building. 
This means the latest values from dynamic options will be used, as well as adding the splash and/or logo to your Player Settings.  
If you dont desire this behaviour, simply leave the splashdown files in `INACTIVE` state.   


## (6) Add Splashdown to your build pipeline through CLI


below will activate the splashdown file with the provided filename, and apply the options. 
```shell
_yourUnityPath_ -batchmode -quit -projectPath _yourProjectPath_ -executeMethod Splashdown.Editor.CLI.SetSplashOptions -name MySplashdown -l1 hello -l2 banana -l3 world -size 44
```

```shell
//Mandatory param:
-name MySplashdown // the name of the splashdown file to apply. note that the default name is "MySplashdown" but you should replace with target filename if you've changed it in your project.

//Optional Flags:
-enable_splash    // use as splash logo
-enable_icon      // use as app icon
-disable_splash   // remove logo from splash
-disable_icon     // remove icons and restore previous icons
-enableDynamic    //sets dynamic options to false
-disableDynamic   //sets dynamic options to true
-size //sets the target font size (will be clamped if outside valid range)
```

Example for MacOS:
```shell
 /Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectPath ~/Desktop/MyCoolGame -executeMethod Splashdown.Editor.CLI.SetSplashOptions -name MySplashdown -activeSplash -l1 hello -l2 cruel -l3 world - size 44
```
Example for Windows:
```shell
"C:\Program Files\Unity\Editor\Unity.exe" -quit -batchmode -projectPath "C:\Users\UserName\Documents\MyProject" -executeMethod Splashdown.Editor.CLI.SetSplashOptions -name "MySplashdown" -disableDynamic -l1 "Banana" -l2 "Yellow" -size 52
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
For App icons, the Player settings dont allow for multiple icons for each category.  Hence, when you activate an Icon, the system will check for other existing activated icons and disable those first.    
This silent behaviour will be modified in the future to introduce a warning when trying to activate an icon when another icon is already active. 


## Switching Fonts
You can switch the font used by using the Font Selector field in the splashdown inspector. Any font asset in your project can be used.   The label above the font selector should update to show the path to the font currently being used.  Keep in mind the font Selector field is always shown empty as its used for drag-and-dropping font asset files.  
After switching fonts, you will likey need to adjust the font size to fit your needs.  A feature to auto-resize the text will be introduced in a future release.  


## Customizing the Border
Customizing the border is possible, but the feature is not high-priority in the roadmap.  If there is a use-case where modifying the borders provides an additional benefit other than cosmetic, pls open an issue describing your use-case and I will gladly look into it.  


## using a texture as background or watermark
+ WIP.  Is in the roadmap and will be added in a future release. 


# Supported Unity Versions

| Version   | Supported                |  Notes                                                       |
| --------  | ------------------------ | -------------------------------------------------------------|
| < 2021.X  | ✖️ not supported         |  will support if error also applies to higher unity versions |   
| 2021.X    | ✅ supported             |  pls submit issue if encountering problems                   |
| 2022.X    | ❔ supported (untested)  |  (same as above)                                             |
| 2023.X    | ❔ supported (untested)  |  (same as above)                                             |


# FAQ
+ **My Dynamic options are overriding the options passed through CLI!?**  

You can disable the dynamic options with the optional flag provided.  You can also use Dynamic Options optional filter parameter or simply have the DynamicOptions only override a certain line and leave the rest for the CLI options to fill.  See Conflicting Options Resolution section. 

+ **I want to use the generated logo for something else, how can I extract the texture/sprite from the splashdown file?**
  
A Sprite is generated and saved as a sub-asset of the splashdown file. you can copy it to your clipboard from the project hierarchy, and paste it elsewhere in your project to get a clone.

+ **Can I use asian alphabets (e.g kanji, Akson Thai, Hangul) ?**   

Yes!  but you will likely need to provide your own font as the built-in fonts are very limited in the amount of characters available.  The package comes with NanumGothic as a sample font that is compatible with korean and RobotoMono (the default font) is compatible with cyrillic.  
There are plenty of free fonts available that will work well with non-latin languages. I have not included them in this package to avoid bloating the size of the package with unecessary fonts. 
See instructions for adding & switching fonts and feel free to open an issue if you are unable to get your preferred language working properly. 


# License

MIT License. Refer to the LICENSE.md file.

  
