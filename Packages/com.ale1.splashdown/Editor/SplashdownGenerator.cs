using System;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace Splashdown.Editor
{
    public static class SplashdownGenerator
    {
        
        const string DefaultFilename = "MySplashdown.splashdown"; 
        
        [MenuItem("Assets/Create/New Splashdown")]
        public static void CreateNewSplashdownFromContextMenu()
        {
            var targetPath = (AssetDatabase.GetAssetPath(Selection.activeObject));
            
            if (string.IsNullOrEmpty(targetPath))
            {
                targetPath = "Assets";
            }
            else if (Directory.Exists(targetPath)) // its a directory.
            {
                targetPath = Path.Combine(targetPath, DefaultFilename);
            }
            else if (Path.GetExtension(targetPath) != "") //path is pointing to a file.  Use same location but use default filename.
            {
                targetPath = targetPath.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
                targetPath = Path.Combine(targetPath, DefaultFilename);
            }

            var options = new Splashdown.Editor.Options(true);
            GenerateSplashdownFile(targetPath, options);
        }
        
        
        public static void GenerateSplashdownFile(string targetPath, Splashdown.Editor.Options options)
        {
            // Apply the customizations here
            var texture = CreateTexture(targetPath, options);
            
            byte[] bytes = texture.EncodeToPNG();
            string fullPath =  targetPath.Replace("Assets", Application.dataPath);
            File.WriteAllBytes(fullPath, bytes);
            AssetDatabase.Refresh();

            // Reimport asset as Splashdown
            SplashdownImporter importer = (SplashdownImporter)AssetImporter.GetAtPath(targetPath);
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            AssetDatabase.ImportAsset(targetPath); //todo: check if this necessary?
        }
        
        public static Texture2D CreateTexture(string targetPath, Splashdown.Editor.Options options)
        {
            // Create a new texture
            var texture = new Texture2D(360, 360, TextureFormat.RGBA32, false);

            if (options == null)
            {
                options = new Splashdown.Editor.Options(true);
            }
            

            // Fill the texture with the background color 
            Color[] pixels = new Color[texture.width * texture.height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = (UnityEngine.Color) options.backgroundColor;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            
            // Calculate the spacing between lines based on how many lines are not empty
            // Set the buffer zone at the top and bottom
            float buffer = texture.height * 0.1f; // 10% of the texture's height
            var nonEmptyLines = options.LineCount;

            float lineHeight = (texture.height - buffer) / (float)nonEmptyLines;

            // Create a counter for current line
            int currentLine = 0;

            // Add non-null text lines to the texture
            if (options.line1 != null)
            {
                texture.AddText(options.line1, Mathf.FloorToInt(buffer + lineHeight * (nonEmptyLines - 1 - currentLine)), options);
                texture.Apply();
                currentLine++;
            }

            if (options.line2 != null)
            {
                texture.AddText(options.line2, Mathf.FloorToInt(buffer + lineHeight * (nonEmptyLines - 1 - currentLine)), options);
                texture.Apply();
                currentLine++;
            }

            if (options.line3 != null)
            {
                texture.AddText(options.line3, Mathf.FloorToInt(buffer + lineHeight * (nonEmptyLines - 1 - currentLine)), options);
                texture.Apply();
            }

            texture.AddBorder(Color.white);

            // Save texture to PNG
            byte[] bytes = texture.EncodeToPNG();
            string fullPath =  targetPath.Replace("Assets", Application.dataPath);

            //create parent directory if doesnt exist
            //Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException());
            
            File.WriteAllBytes(fullPath, bytes);

            return texture;
        }


        private static void AddText(this Texture2D texture, string text, int yPosition, Splashdown.Editor.Options options)
        {
            //todo: use max text width instead of number of characters to decide if truncation is necessary. 
            //remarks: right now since current font is monospaced, text character count is good measure of width, but to support other fonts will need to add individual characer widths.
            if (text.Length > 10)
            {
                text = text.Substring(0, 10);
                Debug.LogWarning($"Splashdown ::: text is too long to fit, will be truncated: '{text}...'");
            }
            
            var font = options.font;
            if (font == null)
            {
                font = Options.DefaultFont;
            }
            var fontSize = font.fontSize;

            // Start text with 5% margin from the left
            int startPosition = Mathf.FloorToInt(texture.width * 0.05f);

            // Let's make the texture's height equal to the font's size plus some extra 
            RenderTexture rt = RenderTexture.GetTemporary(texture.width, (int)(fontSize * 1.1f));
            RenderTexture.active = rt;

            // Clear the RenderTexture to desired color
            GL.Clear(true, true, (UnityEngine.Color) options.backgroundColor);

            Material fontMaterial = new Material(Shader.Find("GUI/Text Shader"));

            // Set the text color
            fontMaterial.SetColor("_Color", (UnityEngine.Color) options.textColor);

            fontMaterial.mainTexture = font.material.mainTexture;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, rt.width, rt.height, 0);

            fontMaterial.SetPass(0);

            GL.Begin(GL.QUADS);
            font.RequestCharactersInTexture(text, fontSize);
            
            Vector3 position = new Vector3(startPosition, font.ascent - fontSize * 0.26f, 0);

            for (int i = 0; i < text.Length; i++)
            {
                if (font.GetCharacterInfo(text[i], out CharacterInfo ch, fontSize))
                {
                    // Adjust x position for next character
                    position.x += ch.advance;

                    // Vertices are defined counter-clockwise
                    GL.TexCoord(ch.uvTopLeft);
                    GL.Vertex3(position.x + ch.minX, position.y - ch.maxY, 0);
                    GL.TexCoord(ch.uvBottomLeft);
                    GL.Vertex3(position.x + ch.minX, position.y - ch.minY, 0);
                    GL.TexCoord(ch.uvBottomRight);
                    GL.Vertex3(position.x + ch.maxX, position.y - ch.minY, 0);
                    GL.TexCoord(ch.uvTopRight);
                    GL.Vertex3(position.x + ch.maxX, position.y - ch.maxY, 0);
                }
            }

            GL.End();
            GL.PopMatrix();

            Texture2D textTexture = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            textTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            textTexture.Apply();

            Color[] pixels = textTexture.GetPixels();
            
            texture.SetPixels(0, yPosition, textTexture.width, textTexture.height, pixels);

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
        }


        private static int GetTextWidth(string text, Font font)
        {
            int totalWidth = 0;

            // Calculate total width of the text
            foreach (var ch in text)
            {
                if (font.GetCharacterInfo(ch, out CharacterInfo chInfo, font.fontSize))
                {
                    totalWidth += chInfo.advance;
                }
            }

            return totalWidth;
        }

        private static void AddBorder(this Texture2D texture, Color color)
        {
            // Define border thickness
            int borderThickness = 13;

            // Define border radius
            int borderRadius = 42;

            // Calculate texture center
            Vector2 center = new Vector2(texture.width / 2f, texture.height / 2f);

            // Process each pixel
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    // Calculate the distance from the pixel to each corner
                    int distToUpperLeft = Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2)));
                    int distToUpperRight =
                        Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(texture.width - x - 1, 2) + Mathf.Pow(y, 2)));
                    int distToLowerLeft =
                        Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(texture.height - y - 1, 2)));
                    int distToLowerRight =
                        Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(texture.width - x - 1, 2) +
                                                    Mathf.Pow(texture.height - y - 1, 2)));

                    // Calculate the distance from the pixel to the center of the texture
                    float distToCenter = Vector2.Distance(new Vector2(x, y), center);

                    // If the pixel is on the edge or within the border area, color it
                    if (x < borderThickness || y < borderThickness || x >= texture.width - borderThickness ||
                        y >= texture.height - borderThickness)
                    {
                        texture.SetPixel(x, y, color);
                    }
                    else if ((distToUpperLeft < borderRadius && x < borderRadius && y < borderRadius) ||
                             (distToUpperRight < borderRadius && x >= texture.width - borderRadius &&
                              y < borderRadius) ||
                             (distToLowerLeft < borderRadius && x < borderRadius &&
                              y >= texture.height - borderRadius) ||
                             (distToLowerRight < borderRadius && x >= texture.width - borderRadius &&
                              y >= texture.height - borderRadius))
                    {
                        // If the pixel is in the border area, near a corner, and also more than certain distance from the center of the texture, color it
                        if (distToCenter > borderRadius - borderThickness)
                        {
                            texture.SetPixel(x, y, color);
                        }
                    }
                }
            }

            // Apply the changes
            texture.Apply();
        }
    }
}