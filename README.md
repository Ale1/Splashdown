```diff 
---------------------------------------------------------------------------------
```
# NOTICE
This is WIP and version 1.0.0 will be released soon on OpenNPM! (Exepcted July 25 - 2023)
```diff 
---------------------------------------------------------------------------------
```

# Splashdown
A Unity open-source splash and icon tool that will generate custom icons with user-provided text.  Allows for dynamic scripted content and to be incorporated into your build pipeline. 

Its purpose is to aid in quick iteration of unity mobile games, and allow no-code team members such as external Q.A testers, designers and general playtesters to quickly identify the app they are testing on their mobile device.  
A quick glimpse at the generated icons and logos can give build information without the need for opening the app or creating custom unity splashes. 




# Installation
wip



# Getting Started 
## Hello World
Right-click on the Assets Folder.  Select `Create > New Splashdown`.   
A Splashdown file will appear.  Feel free to move the file to a subfolder (as long as its a child of Assets folder). 

Fill in the Splashdown Importer window and hit `Apply`.  

![Screenshot 2023-07-12 at 21 06 20](https://github.com/Ale1/Splashdown/assets/4612160/c7a415bc-0d9f-4810-a977-b892e0540f37)

Congrats! you generated your first custom Icon

The child sub-asset of this file is Sprie you can now can use as a placeholder App icon, or as a logo in your Splash screen. 
If you are happy to just set the app icon or splash manually then you are all set!   



| ![Screenshot 2023-07-12 at 22 10 42](https://github.com/Ale1/Splashdown/assets/4612160/1ec61486-ab92-432a-b274-e037de82f433) |
|:--:| 
| *can use splashdown manually by dragging and dropping the Splashdown file in Player settings* |



<b>If you wish to automatate the contents of the icon, proceed... </b>
<br/><br/>

## Dynamic Options

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

# Step 3: Add Splashdown to your build pipeline
WIP


# FAQ
wip
