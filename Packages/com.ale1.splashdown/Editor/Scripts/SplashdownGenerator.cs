using UnityEngine;
using System.IO;
using UnityEditor;

namespace Splashdown.Editor
{
    public static class SplashdownGenerator
    {
        private const string DefaultFilename = "MySplashdown";
        const string DefaultFilenameWithExtension = DefaultFilename + Constants.SplashdownExtension; 
        
        [MenuItem("Assets/Create/New Splashdown")]
        public static void CreateNewSplashdownFromContextMenu()
        {
            var counter = 0;
            
            //check if default filename is safe to use
            var filename = DefaultFilename;

            while (SplashdownExists(filename))
            {
                counter++;
                filename = DefaultFilename + "_" + counter;
            }
            
            var targetPath = (AssetDatabase.GetAssetPath(Selection.activeObject));
            
            if (string.IsNullOrEmpty(targetPath))
            {
                targetPath = "Assets";
            }
            else if (Directory.Exists(targetPath)) // its a directory.
            {
                targetPath = Path.Combine(targetPath, filename +Constants.SplashdownExtension);
            }
            else if (Path.GetExtension(targetPath) != "") //path is pointing to a file.  Use same location but use default filename.
            {
                targetPath = targetPath.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
                targetPath = Path.Combine(targetPath, filename+Constants.SplashdownExtension);
            }

            var options = new Splashdown.Editor.Options(true);
            GenerateSplashdownFile(targetPath, options);
        }

        public static Texture2D CreateTexture(string targetPath, Splashdown.Editor.Options options)
        {
            // Create a new texture
            var texture = new Texture2D(Splashdown.Constants.DefaultWidth, Constants.DefaultHeight, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

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

            //Generate background texture if ref provided
            var backgroundTexture = options.BackgroundTexture;
            if (backgroundTexture != null && backgroundTexture.isReadable)
            {
                //safer to work with copy of texture to avoid reimport loops.
                Texture2D copyTexture = new Texture2D(backgroundTexture.width, backgroundTexture.height);
                copyTexture.SetPixels(backgroundTexture.GetPixels());
                copyTexture.Apply();
                
                var resizedTexture = copyTexture.ResizeTexture(Constants.DefaultWidth, Constants.DefaultHeight);
                Color[] backgroundPixels = resizedTexture.GetPixels();
                texture.SetPixels(backgroundPixels);
                texture.Apply();
            }

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

            File.WriteAllBytes(fullPath, bytes);

            return texture;
        }
        
        
        private static Texture2D TrimTextureToSquare(this Texture2D sourceTexture)
        {
            // Find the size of the square region to crop
            int size = Mathf.Min(sourceTexture.width, sourceTexture.height);
            
            int startX = (sourceTexture.width - size) / 2;
            int startY = (sourceTexture.height - size) / 2;

            // Create a temporary render texture with the square size
            RenderTexture renderTexture = RenderTexture.GetTemporary(size, size);

            // Blit the source texture into the temporary render texture, cropping it to a square
            Graphics.Blit(sourceTexture, renderTexture, new Vector2(1, 1), new Vector2(-startX / (float)sourceTexture.width, -startY / (float)sourceTexture.height));

            // Set the temporary render texture as the active one
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;

            // Read the pixels from the temporary render texture into a new Texture2D
            Texture2D croppedTexture = new Texture2D(size, size);
            croppedTexture.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            croppedTexture.Apply();

            // Release the temporary render texture and restore the previous active render texture
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTexture);

            return croppedTexture;
        }
        
        private static Texture2D ResizeTexture(this Texture2D sourceTexture, int targetWidth, int targetHeight)
        {
            Texture2D resultTexture = new Texture2D(targetWidth, targetHeight);
            float scaleX = (float)sourceTexture.width / targetWidth;
            float scaleY = (float)sourceTexture.height / targetHeight;
            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    int srcX = Mathf.Min(sourceTexture.width - 1, (int)(x * scaleX));
                    int srcY = Mathf.Min(sourceTexture.height - 1, (int)(y * scaleY));
                    resultTexture.SetPixel(x, y, sourceTexture.GetPixel(srcX, srcY));
                }
            }
            resultTexture.Apply();
            return resultTexture;
        }
        
        private static void AddText(this Texture2D texture, string text, int yPosition, Splashdown.Editor.Options options)
        {
            var font = options.Font;
            if (font == null)
            {
                font = Options.DefaultFont;
            }
            var fontSize =  options.TargetFontSize.hasValue ? (int) options.TargetFontSize.ToInt() : font.fontSize;

            // Start text with 8% margin from the left
            var margin = 0.08f;
            int startPosition = Mathf.FloorToInt(texture.width * margin);
            
            // calculate the maximum width of the text to fit on the texture
            int maxWidth = Mathf.FloorToInt(texture.width * (1-margin)) - startPosition;

            while (GetTextWidth(text, font, fontSize) > maxWidth)
            {
                if (text.Length > 1)
                {
                    text = text.Substring(0, text.Length - 1);
                }
                else
                {
                    Debug.LogWarning("Splashdown ::: text is too long to fit, and could not be truncated enough: '" + text + "'");
                    return;
                }
            }

            // Let's make the texture's height equal to the font's size plus some extra 
            RenderTexture rt = RenderTexture.GetTemporary(texture.width, (int)(fontSize * 1.2f));
            RenderTexture.active = rt;

            // Clear the RenderTexture to desired color
            GL.Clear(true, true, Color.clear);

            Material fontMaterial = new Material(Shader.Find("GUI/Text Shader"));

            // Set the text color
            fontMaterial.SetColor("_Color", (UnityEngine.Color) options.textColor);

            fontMaterial.mainTexture = font.material.mainTexture;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, rt.width, rt.height, 0);

            fontMaterial.SetPass(0);

            GL.Begin(GL.QUADS);
            font.RequestCharactersInTexture(text, fontSize);
            
            
            for (int i = 0; i < text.Length; i++)
            {
                if (font.GetCharacterInfo(text[i], out CharacterInfo ch, fontSize))
                {
                    //give extra room to descenders (hence the 0.85f)
                    Vector3 position = new Vector3(startPosition, fontSize * 0.85f, 0);

                    // Vertices are defined counter-clockwise
                    GL.TexCoord(ch.uvTopLeft);
                    GL.Vertex3(position.x + ch.minX, position.y - ch.maxY, 0);
                    GL.TexCoord(ch.uvBottomLeft);
                    GL.Vertex3(position.x + ch.minX, position.y - ch.minY, 0);
                    GL.TexCoord(ch.uvBottomRight);
                    GL.Vertex3(position.x + ch.maxX, position.y - ch.minY, 0);
                    GL.TexCoord(ch.uvTopRight);
                    GL.Vertex3(position.x + ch.maxX, position.y - ch.maxY, 0);

                    // Adjust x position for next character
                    startPosition += ch.advance;
                }
            }

            GL.End();
            GL.PopMatrix();

            
            Texture2D textTexture = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            textTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            textTexture.Apply();

            //Blend pixels to respect transparency
            Color[] textPixels = textTexture.GetPixels();
            Color[] originalPixels = texture.GetPixels(0, yPosition, textTexture.width, textTexture.height);

            for (int i = 0; i < textPixels.Length; i++) {
                float alpha = textPixels[i].a; // the alpha of the text pixel
                originalPixels[i] = textPixels[i] * alpha + originalPixels[i] * (1f - alpha); // blending the pixels
            }

            texture.SetPixels(0, yPosition, textTexture.width, textTexture.height, originalPixels);

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
        }
        

        ///<summary>
        /// Get Text width if using baked-in fontSize
        /// </summary>
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
        
        private static int GetTextWidth(string text, Font font, int fontSize)
        {
            int totalWidth = 0;

            // Calculate total width of the text
            foreach (var ch in text)
            {
                if (font.GetCharacterInfo(ch, out CharacterInfo chInfo, fontSize))
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
        
        private static void GenerateSplashdownFile(string targetPath, Splashdown.Editor.Options options)
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
        }

        
        /// <summary>
        /// Check if file with the same name (including extension .splashdown) already exists. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool SplashdownExists(string name)
        {
            string[] files = Directory.GetFiles(Application.dataPath, $"*{Constants.SplashdownExtension}", SearchOption.AllDirectories);
            
            foreach (string file in files)
            {
                string filename = Path.GetFileName(file);
                if (Path.GetFileNameWithoutExtension(filename).Equals(name))
                {
                    return true;
                }
            }
            return false;
        }
    }
}