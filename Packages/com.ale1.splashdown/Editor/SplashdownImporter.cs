
using Codice.Client.BaseCommands;
using Splashdown.Editor;
using UnityEditor;
using UnityEngine;
using UnityEditor.AssetImporters;

namespace Splashdown.Editor
{
    [ScriptedImporter(1, "splashdown")]
    public class SplashdownImporter : ScriptedImporter
    {

        public Sprite Sprite;
        public SplashdownOptions Options;

        private string assetPath;
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            Debug.Log("banana");

            if (Options == null)
                Options = new SplashdownOptions();
            
            SplashdownGenerator.CreateTexture(ctx.assetPath, Options);
            
            //cache asset path for later use
            assetPath = ctx.assetPath;
            
            if (Options.useAsSplash)
            {
                //todo: add or remove logo to splash
            }

            if (Options.useAsAppIcon)
            {
                //todo: add or remove sprite to icons
            }

            // Load the file as bytes
            var fileData = System.IO.File.ReadAllBytes(ctx.assetPath);

            // Convert the bytes to texture.
            var texture = new Texture2D(320, 320);
            texture.LoadImage(fileData);

            // Create a sprite from the texture
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            sprite.name = "Generated";

            // Add objects to the imported asset
            ctx.AddObjectToAsset("main tex", texture);
            ctx.AddObjectToAsset("main sprite", sprite);
            ctx.SetMainObject(texture);

            // Save the sprite and Config for easy retrieval later
            Sprite = sprite;
            
            AssetDatabase.Refresh();

        }
        
        
       
    }
}