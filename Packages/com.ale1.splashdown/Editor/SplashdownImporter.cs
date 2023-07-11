
using UnityEngine;
using UnityEditor.AssetImporters;
using UnityEditor;

namespace Splashdown
{
    [ScriptedImporter(1, "splashdown")]
    public class SplashdownImporter : ScriptedImporter
    {
        public bool includeInSplash;
        public bool includeInAppIcon;
        public bool enableLogging;

        public Color backgroundColor = Color.black;
        public Color textColor = new (1f, 1f, 0.6f, 1f);
        
        public string line1;
        public string line2;
        public string line3;

        public Sprite Sprite;
        
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            SplashdownGenerator.CreateTexture(ctx.assetPath);

            if (includeInSplash)
            {
                //todo: add or remove logo to splash
            }

            if (includeInAppIcon)
            {
                //todo: add or remove sprite to icons
            }

                // Load the file as bytes
            var fileData = System.IO.File.ReadAllBytes(ctx.assetPath);

            //convert the bytes to texture.
            var texture = new Texture2D(320, 320);
            texture.LoadImage(fileData);

            // Create a sprite from the texture
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            sprite.name = "Generated";

            // Use the AssetImportContext to add objects to the imported asset
            ctx.AddObjectToAsset("main tex", texture);
            ctx.AddObjectToAsset("main sprite", sprite);
            ctx.SetMainObject(texture);
            
            //save the sprite for easy retrieval later
            Sprite = sprite;
        }
        
    }
}