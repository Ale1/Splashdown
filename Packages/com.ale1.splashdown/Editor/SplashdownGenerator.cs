using System;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace Splashdown
{
    
    public class SplashdownGenerator
    {
        private static Texture2D texture;

        public static void GenerateSplashdownFile(string targetPath)
        {
            //Create empty placehodler Texture
            texture = new Texture2D(360, 360, TextureFormat.RGBA32, false);
            byte[] bytes = texture.EncodeToPNG();
            string fullPath =  targetPath.Replace("Assets", Application.dataPath);
            //create parent directory if doesnt exist
            //Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException());
            File.WriteAllBytes(fullPath, bytes);
            AssetDatabase.Refresh();

            //Reimport asset as Splasdhown
            SplashdownImporter importer = (SplashdownImporter)AssetImporter.GetAtPath(targetPath);
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            AssetDatabase.ImportAsset(targetPath);
        }
        
        
        public static void CreateTexture(string targetPath)
        {
            // Create a new texture
            texture = new Texture2D(360, 360, TextureFormat.RGBA32, false);

            // Fill the texture with the background color
            Color[] pixels = new Color[texture.width * texture.height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Config.backgroundColor;
            }

            texture.SetPixels(pixels);
            texture.Apply();


            // Calculate the spacing between lines based on how many lines are not empty
            // Set the buffer zone at the top and bottom
            float buffer = texture.height * 0.1f; // 10% of the texture's height
            var nonEmptyLines = Config.LineCount;

            float lineHeight = (texture.height - buffer) / (float)nonEmptyLines;

            // Create a counter for current line
            int currentLine = 0;

            // Add non-empty text lines to the texture
            if (Config.hasLine1)
            {
                AddText(texture, Config.line1,
                    Mathf.FloorToInt(buffer + lineHeight * (nonEmptyLines - 1 - currentLine)));
                texture.Apply();
                currentLine++;
            }

            if (Config.hasLine2)
            {
                AddText(texture, Config.line2,
                    Mathf.FloorToInt(buffer + lineHeight * (nonEmptyLines - 1 - currentLine)));
                texture.Apply();
                currentLine++;
            }

            if (Config.hasLine3)
            {
                AddText(texture, Config.line3,
                    Mathf.FloorToInt(buffer + lineHeight * (nonEmptyLines - 1 - currentLine)));
                texture.Apply();
            }

            AddBorder(Color.white);

            // Save texture to PNG
            byte[] bytes = texture.EncodeToPNG();
            string fullPath =  targetPath.Replace("Assets", Application.dataPath);

            //create parent directory if doesnt exist
            //Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException());
            
            File.WriteAllBytes(fullPath, bytes);

            if (Config.logging) Debug.Log("Texture saved at: " + fullPath);
            AssetDatabase.Refresh();
        }


        private static void AddText(Texture2D texture, string text, int yPosition)
        {
            //todo: use max text width instead of number of characters to decide if truncation is necessary. 
            //remarks: right now since current font is monospaced, text character count is good measure of width, but to support other fonts will need to add individual characer widths.
            if (text.Length > 10)
            {
                text = text.Substring(0, 10);
                if(Config.logging) Debug.LogWarning($"Splashdown ::: text is too long to fit, will be truncated: '{text}...'");
            }

            //var font = Font.CreateDynamicFontFromOSFont("Courier New", FontSize); //todo: fallback to system font if custom one not found.
            Font font = AssetDatabase.LoadAssetAtPath<Font>("Packages/com.Ale1.splashdown/Editor/Splashdown_RobotoMono.ttf");
            if (font == null)
                Debug.Log("no font found");

            var FontSize = font.fontSize;

            // Start text with 5% margin from the left
            int startPosition = Mathf.FloorToInt(texture.width * 0.05f);

            // Let's make the texture's height equal to the font's size plus some extra 
            RenderTexture rt = RenderTexture.GetTemporary(texture.width, (int)(FontSize * 1.1f));
            RenderTexture.active = rt;

            // Clear the RenderTexture to desired color
            GL.Clear(true, true, Config.backgroundColor);

            Material fontMaterial = new Material(Shader.Find("GUI/Text Shader"));

            // Set the text color
            fontMaterial.SetColor("_Color", Config.textColor);

            fontMaterial.mainTexture = font.material.mainTexture;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, rt.width, rt.height, 0);

            fontMaterial.SetPass(0);

            GL.Begin(GL.QUADS);
            font.RequestCharactersInTexture(text, FontSize);

            //Vector3 position = new Vector3(startPosition, font.ascent, 0);
            Vector3 position = new Vector3(startPosition, font.ascent - FontSize * 0.26f, 0);

            for (int i = 0; i < text.Length; i++)
            {
                if (font.GetCharacterInfo(text[i], out CharacterInfo ch, FontSize))
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

        private static void AddBorder(Color color)
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