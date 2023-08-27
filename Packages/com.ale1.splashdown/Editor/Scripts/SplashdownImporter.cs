// ReSharper disable RedundantUsingDirective
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.AssetImporters;
using System.Reflection;
using UnityEngine.Serialization;
using Options = Splashdown.Editor.Options;
using OptionsProviderAttribute = Splashdown.OptionsProviderAttribute;


namespace Splashdown.Editor
{
    [ScriptedImporter(1, Constants.SplashdownFileType)]
    [HelpURL("https://github.com/Ale1/Splashdown#readme")]
    public class SplashdownImporter : ScriptedImporter
    {
        [HideInInspector] [SerializeField] protected bool ActiveSplash;
        [HideInInspector] [SerializeField] protected bool ActiveIcon;
        public bool IsSplashActive => ActiveSplash;
        public bool IsIconActive => ActiveIcon;

        public bool useDynamicOptions;

        [HideInInspector] public Options inspectorOptions;

        private Texture2D thumbnail =>
            AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.ale1.splashdown/Editor/splashdown_thumbnail.png");

        public SplashdownImporter()
        {
            SplashdownEvents.OnIconStateActivated -= IconStateListener;
            SplashdownEvents.OnIconStateActivated += IconStateListener;
        }

        ~SplashdownImporter() // Destructor
        {
            SplashdownEvents.OnIconStateActivated -= IconStateListener;
        }

        public void SetActiveSplash(bool val)
        {
            ActiveSplash = val;
        }

        public void SetActiveIconWithEvent(bool val)
        {
            if (ActiveIcon != val)
            {
                ActiveIcon = val;
                if (ActiveIcon) SplashdownEvents.OnIconStateActivated.Invoke(this);
            }
        }

        public string GetGuid => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this));


        /// <summary>
        /// Deserializes options for a splashdown asset from the asset database.
        /// </summary>
        /// <param name="pathToSplashdown">The path to the splashdown asset.</param>
        /// <returns>Returns the deserialized options, or null if they do not exist.</returns>
        public static Splashdown.Editor.Options DeserializeOptions(string pathToSplashdown)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(pathToSplashdown);
            foreach (var asset in assets)
            {
                if (asset is TextAsset textAsset && textAsset.name == Constants.GeneratedOptionsName)
                {
                    return JsonUtility.FromJson<Splashdown.Editor.Options>(textAsset.text);
                }
            }

            return null;
        }

        /// <summary>
        /// Forces all splashdown importers to update.
        /// </summary>
        public static void RefreshAllImporters()
        {
            // Get all '.splashdown' asset paths in the project.
            var splashdownPaths = AssetDatabase.FindAssets("", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => Path.GetExtension(path) == Constants.SplashdownExtension);

            foreach (var splashdownPath in splashdownPaths)
            {
                // Force reimport of each '.splashdown' asset.
                AssetDatabase.ImportAsset(splashdownPath, ImportAssetOptions.ForceUpdate);
            }
        }


        /// <summary>
        /// The main import function that is called when a '.splashdown' file is imported.
        /// </summary>
        /// <param name="ctx">The asset import context.</param>
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

            string filename = System.IO.Path.GetFileNameWithoutExtension(ctx.assetPath);
            this.name = filename;
            //save name of splashdown file in Options
            options.source = filename;
            // Set the sprite path in the options
            options.assetPath = $"{ctx.assetPath}";

            if (inspectorOptions != null)
                options.UpdateWith(inspectorOptions);


            if (useDynamicOptions)
            {
                var dynamicOptions = SplashdownUtils.FetchDynamicOptions(name);
                if (dynamicOptions != null) options.UpdateWith(dynamicOptions);
            }


            var key = Constants.EditorPrefsKey + "." + this.name;
            if (EditorPrefs.HasKey(key))
            {
                var cliOptionsJson = EditorPrefs.GetString(key);
                var newOptions = JsonUtility.FromJson<Options>(cliOptionsJson);
                options.UpdateWith(newOptions);

                EditorApplication.delayCall += () => { EditorPrefs.DeleteKey(key); };
            }

            SplashdownGenerator.CreateTexture(ctx.assetPath, options);

            // Load the file as bytes
            var fileData = System.IO.File.ReadAllBytes(ctx.assetPath);

            // Convert the bytes to texture.
            var texture = new Texture2D(320, 320);
            texture.LoadImage(fileData);
            texture.name = "GeneratedTexture";

            // Create a sprite from the texture
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            sprite.name = Constants.GeneratedSpriteName;

            // Convert Options to a serialized object and save as a sub-asset
            string jsonOptions = JsonUtility.ToJson(options);
            TextAsset serializedOptions = new TextAsset(jsonOptions)
            {
                name = Constants.GeneratedOptionsName
            };

            // Add objects to the imported asset
            ctx.AddObjectToAsset("main tex", texture);
            ctx.AddObjectToAsset("main sprite", sprite, thumbnail);
            ctx.AddObjectToAsset("Options", serializedOptions);
            ctx.SetMainObject(sprite);
        }


        public Options FetchDynamicOptions()
        {
            if (!String.IsNullOrWhiteSpace(this.name))
            {
                return SplashdownUtils.FetchDynamicOptions(this.name);
            }

            return null;
        }


        /// <summary>
        /// Listens to the icon state activation event. If the event is fired by another importer, it deactivates its own icon state.
        /// </summary>
        /// <param name="emiter">The importer that emitted the event.</param>
        private void IconStateListener(AssetImporter emitter)
        {
            if (ActiveIcon == true && emitter != this)
            {
                ActiveIcon = false;
            }
        }
    }
}