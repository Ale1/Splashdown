using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.AssetImporters;
using System.Reflection;
using UnityEngine.Serialization;


namespace Splashdown.Editor
{
    [ScriptedImporter(1, "splashdown")]
    public class SplashdownImporter : ScriptedImporter
    {
        [HideInInspector] public bool Activated;
        public bool useDynamicOptions;
        

        [HideInInspector] public Options inspectorOptions;

        public static Splashdown.Editor.Options DeserializeOptions(string pathToSplashdown)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(pathToSplashdown);
            foreach (var asset in assets)
            {
                if (asset is TextAsset textAsset && textAsset.name == "Options")
                {
                    return JsonUtility.FromJson<Splashdown.Editor.Options>(textAsset.text);
                }
            }

            return null;
        }

        public static void RefreshAllImporters()
        {
            // Get all '.splashdown' asset paths in the project.
            var splashdownPaths = AssetDatabase.FindAssets("", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => Path.GetExtension(path) == ".splashdown");

            foreach (var splashdownPath in splashdownPaths)
            {
                // Force reimport of each '.splashdown' asset.
                AssetDatabase.ImportAsset(splashdownPath, ImportAssetOptions.ForceUpdate);
            }
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            ImportWithContext(ctx);
        }

        private void ImportWithContext(AssetImportContext ctx)
        { 
            Options options;
            
            // Load the deserialized options
            var deserializedOptions = DeserializeOptions(ctx.assetPath);
            if (deserializedOptions != null)
            {
                options = deserializedOptions;
            }
            else
            {
                options = new Options(true);
            }
            
            if(inspectorOptions != null)
                options.UpdateWith(inspectorOptions);

            if (useDynamicOptions)
            {
                ApplyDynamicOptions(options, name); ///todo: name filtering
            }

         
            string filename = System.IO.Path.GetFileNameWithoutExtension(ctx.assetPath);
            this.name = filename;
            //save name of splashdown file in Options
            options.fileName = filename;
            // Set the sprite path in the options
            options.assetPath = $"{ctx.assetPath}";

            var key = "com.ale1.Splashdown." + this.name;
            if(EditorPrefs.HasKey(key))
            {
                var cliOptionsJson = EditorPrefs.GetString(key);
                var newOptions = JsonUtility.FromJson<Options>(cliOptionsJson);
                options.UpdateWith(newOptions);

                EditorApplication.delayCall += () =>
                {
                    EditorPrefs.DeleteKey(key);
                };
            }
            

            Font font = null;
            if (!String.IsNullOrEmpty(options.fontGUID))
            {
                font = AssetDatabase.LoadAssetAtPath<Font>(
                    AssetDatabase.GUIDToAssetPath(options.fontGUID));
            }


            if (font == null) Debug.LogError("no font found");

            SplashdownGenerator.CreateTexture(ctx.assetPath, options);

            // Load the file as bytes
            var fileData = System.IO.File.ReadAllBytes(ctx.assetPath);

            // Convert the bytes to texture.
            var texture = new Texture2D(320, 320);
            texture.LoadImage(fileData);

            // Create a sprite from the texture
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            sprite.name = "Generated";
            
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
            Debug.Log("saving");
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
        private bool ApplyDynamicOptions(Options options, string targetName)
        {
            // Get all assemblies in the current AppDomain
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                           BindingFlags.Static))
                    {
                        // method has the SplashdownOptionProviderAttribute
                        if (method.GetCustomAttributes(typeof(Splashdown.OptionsProviderAttribute), false)
                                .FirstOrDefault() is Splashdown.OptionsProviderAttribute)
                        {
                            // check for correct implementation by reading the return type
                            if (method.ReturnType != typeof(Splashdown.Editor.Options))
                            {
                                Debug.LogWarning(
                                    $"{method} with {nameof(Splashdown.OptionsProviderAttribute)} does not have correct return type ");
                                continue;
                            }

                            // Check the parameters (should take none)
                            var parameters = method.GetParameters();
                            if (parameters.Length != 0)
                            {
                                Debug.LogWarning(
                                    $"{method} with {nameof(Splashdown.OptionsProviderAttribute)} should not take any parameters ");
                                continue;
                            }

                            // If we get here, the method is valid

                            var dynamicOptions = (Splashdown.Editor.Options)method.Invoke(null, null);

                            options.UpdateWith(dynamicOptions);

                            return true;
                        }
                    }
                }
            }

            return false; // or throw an exception
        }
    }
}