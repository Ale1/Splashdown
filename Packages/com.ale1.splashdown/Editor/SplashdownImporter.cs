using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.AssetImporters;
using System.Reflection;
using Codice.Client.BaseCommands;
using UnityEngine.Serialization;


namespace Splashdown.Editor
{
    [ScriptedImporter(1, "splashdown")]
    public class SplashdownImporter : ScriptedImporter
    {
        public bool useDynamicOptions;


        public static Font DefaultFont => AssetDatabase.LoadAssetAtPath<Font>("Packages/com.Ale1.splashdown/Editor/Splashdown_RobotoMono.ttf");
        
        public Splashdown.Options options;
        
        public static Splashdown.Options DeserializeOptions(string pathToSplashdown)
        {
            var serializedOptions = AssetDatabase.LoadAssetAtPath<TextAsset>($"/Options");
            if (serializedOptions != null)
            {
                return JsonUtility.FromJson<Splashdown.Options>(serializedOptions.text);
            }
            return null;
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            ImportWithContext(ctx);
        }
        
        private void ImportWithContext(AssetImportContext ctx)
        {
            
            
            
            if(useDynamicOptions)
                options = FetchDynamicOptions(name);   //todo: filter by targetName

            options = CommandLineInterpreter.ProvideOptions();

            Font font = null;
            if (!String.IsNullOrEmpty(options.fontGUID))
            {
                font = AssetDatabase.LoadAssetAtPath<Font>(
                    AssetDatabase.GUIDToAssetPath(options.fontGUID));
            }

            if(font == null)
            {
                font = DefaultFont;
            }
            
            if(font == null) Debug.LogError("no font found");

            SplashdownGenerator.CreateTexture(ctx.assetPath, options);

            // Load the file as bytes
            var fileData = System.IO.File.ReadAllBytes(ctx.assetPath);

            // Convert the bytes to texture.
            var texture = new Texture2D(320, 320);
            texture.LoadImage(fileData);

            // Create a sprite from the texture
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            sprite.name = "Generated";
            
            //save name of splashdown file in Options
            string filename = System.IO.Path.GetFileNameWithoutExtension(ctx.assetPath);
            this.name = filename;
            options.fileName = filename;
            
            // Set the sprite path in the options
            options.assetPath = $"{ctx.assetPath}";
            
            // Convert Options to a serialized object and save as a sub-asset
            string jsonOptions = JsonUtility.ToJson(options);
            TextAsset serializedOptions = new TextAsset(jsonOptions)
            {
                name = "Options"
            };

            // Add objects to the imported asset
            ctx.AddObjectToAsset("main tex", texture);
            ctx.AddObjectToAsset("main sprite", sprite);
            ctx.AddObjectToAsset("Options", serializedOptions);
            ctx.SetMainObject(texture);
        }
     
        
        /// <summary>
        /// Fetches dynamic Splashdown options by searching all assemblies for a method marked with the Splashdown.OptionsProviderAttribute that has the correct signature.
        /// </summary>
        /// <remarks>
        /// If there are multiple methods in the assemblies that are marked with the Splashdown.OptionsProviderAttribute and have the correct signature,
        /// this method is non-deterministic and it's not guaranteed to return the options from the same method every time.
        /// </remarks>
        /// <returns>
        /// The Splashdown.Options returned by the first valid method it finds, or null if no valid method is found.
        /// </returns>
        private Splashdown.Options FetchDynamicOptions(string targetName) 
        {
            // Get all assemblies in the current AppDomain
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                    {
                        // method has the SplashdownOptionProviderAttribute
                        if (method.GetCustomAttributes(typeof(Splashdown.OptionsProviderAttribute), false).FirstOrDefault() is Splashdown.OptionsProviderAttribute)
                        {
                            // check for correct implementation by reading the return type
                            if (method.ReturnType != typeof(Splashdown.Options))
                            {
                                Debug.LogWarning($"{method} with {nameof(Splashdown.OptionsProviderAttribute)} does not have correct return type ");
                                continue;
                            }

                            // Check the parameters (should take none)
                            var parameters = method.GetParameters();
                            if (parameters.Length != 0)
                            {
                                Debug.LogWarning($"{method} with {nameof(Splashdown.OptionsProviderAttribute)} should not take any parameters ");
                                continue;
                            }

                            // If we get here, the method is valid
                            return (Splashdown.Options)method.Invoke(null, null);
                        }
                    }
                }
            }
            return null; // or throw an exception
        }
        
    }
}