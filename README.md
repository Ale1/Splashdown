
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

Fill in the Splashdown Importer window and hit `Apply`.  

![Screenshot 2023-07-12 at 21 06 20](https://github.com/Ale1/Splashdown/assets/4612160/c7a415bc-0d9f-4810-a977-b892e0540f37)

Congrats! you generated your first custom Icon

This file can be used as a placeholder app icon or logo in your Build Settings.
If you are happy to just set the app icon or splash manually then you are all set!   


| ![Screenshot 2023-07-12 at 22 10 42](https://github.com/Ale1/Splashdown/assets/4612160/1ec61486-ab92-432a-b274-e037de82f433) |
|:--:| 
| *can use splashdown manually by dragging and dropping the Splashdown file in Player settings* |



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

WIP : how to force splashdown to refresh via script. 

<b>If you wish to automatically include the splashdown icon on every build, proceed....</b>
<br/><br/>


## (3) Add Splashdown to your build pipeline through CLI

_yourUnityPath_ -batchmode -quit -projectPath _yourProjectPath_ -executeMethod Splashdown.Editor.CommandLineInterpreter.SetSplashWithOptions -name MySplashdown -l1 hello -l2 banana -l3 world

# Advanced Customization
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
+ I want to use the generated logo for something else, how can I extract the texture/sprite from the splashdown file? (WIP)
+ I want to use 2 splashdown files: one for the app icon and a separate one for splash logo. How do I do that? (WIP)
+ Can I use a transparent color background ? 
+ Can I use asian alphabets (e.g kanji) ?   
   Yes!  but you will likely need to provide your own font as the built-in fonts are very limited in the amount of characters available.  See instructions for switching fonts. 


  
