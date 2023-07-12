# NOTICE
This is WIP and version 1.0.0 will be released soon on OpenNPM! (Exepcted July 25 - 2023)

# Splashdown
A Unity open-source splash and icon tool

# Installation
wip



# Getting Started 
## Hello World
Right-click on the Assets Folder.  Select `Create > New Splashdown`.   
A Splashdown file will appear.  Feel free to move the file to a subfolder (as long as its a child of Assets folder). 

Fill in the Splashdown Importer window and hit `Apply`.  


![Screenshot 2023-07-12 at 21 06 20](https://github.com/Ale1/Splashdown/assets/4612160/40368e54-3d43-42a5-9b29-74854b740e4d)

Congrats! you generated your first custom Icon

The child sub-asset of this file is Sprie you can now can use as a placeholder App icon, or as a logo in your Splash screen. 
If you are happy to just set the app icon or splash manually in Player Settings and fill the content manually, then you are all set!. 

<b>If you wish to automatate the contents of the icon, proceed... </b>
<br/><br/>

## Dynamic Options

Create a script like below and place it anywhere in your project. Though its recommended that you place it in an `Editor` folder. 
The splashdown importer will automatically invoke methods with the `[Splashdown.OptionsProvider]` attribute whenever its refreshed. 
Dynamically created Options will override any manual inputs in hte splashdown importer.

```
//Recommended to place in Editor folder
public static class Example  
{
        [Splashdown.OptionsProvider] //Method with this atrribute must return a Splashdown.Options
        public static Splashdown.Options ProvideSplashdownOptions() => new()
        { 
                line1 = Application.unityVersion, // e.g 2021.3.4f
                line2 = DateTime.Now.ToShortDateString(),
                line3 = Application.version, //e.g 2.1.0
                textColor = Color.red,
                backgroundColor = Color.blue
        };
}
```

WIP : how to force splashdown to refresh via script. 

<b>If you wish to automatically include the splashdown icon on every build, proceed....</b>
<br/><br/>

# Step 3: Add Splashdown to your build pipeline
WIP


# FAQ
wip
