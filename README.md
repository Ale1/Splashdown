
# 🟥 NOTICE 🟥
This is WIP and version 1.0.0 will be released soon on OpenNPM! (Expected July 30 - 2023)

〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️〰️

![SplashdownLogo](https://github.com/Ale1/Splashdown/assets/4612160/37fe6e8a-52dc-44cd-ae0a-321c54c53c1b)

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

![Screenshot 2023-07-28 at 12 18 28](https://github.com/Ale1/Splashdown/assets/4612160/34a23189-ec5d-4801-9b9e-aa2ede8b6833)


Congrats! you generated your first custom Icon


This generated sprite can be used as a placeholder app icon or logo in your Build Settings.
By setting hitting `Activate` button, the playerSettings will start using this sprite as its icon and splash image:


|![Screenshot 2023-07-28 at 14 07 51](https://github.com/Ale1/Splashdown/assets/4612160/d371c966-d9ae-40e7-9481-9a85d9d1f5c8)|
|:--:| 
| *you can remove it by selecting "Deactive" in the splashdown editor* |



<b>If you wish to automatate the contents of the icon, proceed... </b>
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


## (3) Add splashdown to your build pipeline 

Any splashdown file that is set to "ACTIVE" will regenerate when unity is building. This means the latest values from dynamic options will be used, as well as adding the splash and/or logo to your Player Settings.  
If you dont desire this behaviour, simply leave the splashdown files in "INACTIVE" state.  You are always welcome to drag-and-drop the generated sprite as needed to your player prefs if you want full manual control. 


## (4) Add Splashdown to your build pipeline through CLI

```
//below will activate the splashdown file with name "MySplashdown" and apply the options

_yourUnityPath_ -batchmode -quit -projectPath _yourProjectPath_ -executeMethod Splashdown.Editor.CommandLineInterpreter.SetSplashOptions -name MySplashdown -l1 hello -l2 banana -l3 world
```

```
//Example for MacOs
 /Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectPath ~/Desktop/Splashdown -executeMethod Splashdown.Editor.CLI.SetSplashOptions -name MySplashdown -activeSplash -l1 hello -l2 cruel -l3 world
```

optional flags:
```
-enableDynamic //sets dynamic options to false
-disableDynamic //sets dynamic options to true
```


Note: Since you can provide options through multiple avenues, keep in mind the order of priority (most to least):

(1) Dynamic Options  

(2) CLI Options

(2) Options set manually in inspector

(3) Default Options



# Advanced Customization
## Managing multiple Splashdown files
+ WIP


## Switching Fonts
+ WIP

## Customizing the Border
+ WIP

## using a texture as background or watermark
+ WIP


# Supported Unity Versions

| Version  | Supported         |
| -------- | ----------------- |
| < 2021.X | ✖️ not supported  |
| 2021.X   | ✅ supported      |
| 2022.X   | ❔ untested       |
| 2023.X   | ❔ untested       |


# FAQ
+ **My Dynamic options are overriding the options passed through CLI!?**  

You can disable the dynamic options with the optional flag provided.  You can also simply have the DynamicOptions only override a certain line (e.g line1) and leave the rest for the CLI options to fill. 

+ **I want to use the generated logo for something else, how can I extract the texture/sprite from the splashdown file?**
  
A Sprite is generated and saved as a sub-asset of the splashdown file. you can copy it to your clipboard from the project hierarchy, and paste it elsewhere in your project to get a clone.

+ I want to use 2 splashdown files: one for the app icon and a separate one for splash logo. How do I do that? (WIP)
+ Can I use a transparent color background ? (WIP)

+ **Can I use asian alphabets (e.g kanji) ?**   

Yes!  but you will likely need to provide your own font as the built-in fonts are very limited in the amount of characters available.  See instructions for switching fonts. 


  
