using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace WorldFramework.Controller
{
    public class Textures
    {
        #region Bitmap LoadBMP(string fileName)
        /// <summary>
        ///     Loads a bitmap image.
        /// </summary>
        /// <param name="fileName">
        ///     The filename to load.
        /// </param>
        /// <returns>
        ///     The bitmap if it exists, otherwise <c>null</c>.
        /// </returns>
        private static Bitmap LoadBMP(string fileName)
        {
            if (fileName == null || fileName == string.Empty)
            {                  // Make Sure A Filename Was Given
                return null;                                                    // If Not Return Null
            }

            string fileName1 = string.Format("Data{0}{1}",                      // Look For Data\Filename
                Path.DirectorySeparatorChar, fileName);
            string fileName2 = string.Format("{0}{1}{0}{1}Data{1}Textures{1}{2}",          // Look For ..\..\Data\Filename
                "..", Path.DirectorySeparatorChar, fileName);

            // Make Sure The File Exists In One Of The Usual Directories
            if (!File.Exists(fileName) && !File.Exists(fileName1) && !File.Exists(fileName2))
            {
                return null;                                                    // If Not Return Null
            }

            if (File.Exists(fileName))
            {                                         // Does The File Exist Here?
                return new Bitmap(fileName);                                    // Load The Bitmap
            }
            else if (File.Exists(fileName1))
            {                                   // Does The File Exist Here?
                return new Bitmap(fileName1);                                   // Load The Bitmap
            }
            else if (File.Exists(fileName2))
            {                                   // Does The File Exist Here?
                return new Bitmap(fileName2);                                   // Load The Bitmap
            }

            return null;                                                        // If Load Failed Return Null
        }
        #endregion Bitmap LoadBMP(string fileName)

        #region bool LoadGLTextures()
        /// <summary>
        ///     Load bitmaps and convert to textures.
        /// </summary>
        /// <returns>
        ///     <c>true</c> on success, otherwise <c>false</c>.
        /// </returns>
        public static bool LoadGLTextures(string[] picfilename, ref int[] texture)
        {
            if (picfilename.Length != texture.Length) return false;
            for (int i = 0; i < picfilename.Length; i++)
            {
                if (!LoadGLTextures(picfilename[i], ref texture[i])) return false;
            }
            return true;
        }

        public static bool LoadGLTextures(string picfilename, ref int texture)
        {
            Bitmap textureImage = LoadBMP(picfilename);
            if (textureImage == null) return false;
            GL.GenTextures(1, out texture); // Create The Texture
            textureImage.RotateFlip(RotateFlipType.RotateNoneFlipY); // Flip The Bitmap Along The Y-Axis
            // Rectangle For Locking The Bitmap In Memory
            Rectangle rectangle = new Rectangle(0, 0, textureImage.Width, textureImage.Height);
            // Get The Bitmap's Pixel Data From The Locked Bitmap
            BitmapData bitmapData = textureImage.LockBits(rectangle, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            //Nearest
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, textureImage.Width, textureImage.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                            PixelType.UnsignedByte, bitmapData.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            textureImage.UnlockBits(bitmapData); // Unlock The Pixel Data From Memory
            textureImage.Dispose(); // Dispose The Bitmap
            return true;
        }

        public static bool LoadGLTextures(Bitmap textureImage, ref int texture)
        {
            if (textureImage == null) return false;
            GL.GenTextures(1, out texture); // Create The Texture
            textureImage.RotateFlip(RotateFlipType.RotateNoneFlipY); // Flip The Bitmap Along The Y-Axis
            // Rectangle For Locking The Bitmap In Memory
            Rectangle rectangle = new Rectangle(0, 0, textureImage.Width, textureImage.Height);
            // Get The Bitmap's Pixel Data From The Locked Bitmap
            BitmapData bitmapData = textureImage.LockBits(rectangle, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            //Nearest
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, textureImage.Width, textureImage.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                            PixelType.UnsignedByte, bitmapData.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            textureImage.UnlockBits(bitmapData); // Unlock The Pixel Data From Memory
            textureImage.Dispose(); // Dispose The Bitmap
            return true;
        }
        #endregion bool LoadGLTextures()

        public static class TextureLoaderParameters
        {
            /// <summary>(Debug Aid, should be set to false) If set to false only Errors will be printed. If set to true, debug information (Warnings and Queries) will be printed in addition to Errors.</summary>
            public static bool Verbose = false;

            /// <summary>Always-valid fallback parameter for GL.BindTexture (Default: 0). This number will be returned if loading the Texture failed. You can set this to a checkerboard texture or similar, which you have already loaded.</summary>
            public static uint OpenGLDefaultTexture = 0;

            /// <summary>Compressed formats must have a border of 0, so this is constant.</summary>
            public const int Border = 0;

            /// <summary>false==DirectX TexCoords, true==OpenGL TexCoords (Default: true)</summary>
            public static bool FlipImages = true;

            /// <summary>When enabled, will use Glu to create MipMaps for images loaded with GDI+ (Default: false)</summary>
            public static bool BuildMipmapsForUncompressed = false;

            /// <summary>Selects the Magnification filter for following Textures to be loaded. (Default: Nearest)</summary>
            public static TextureMagFilter MagnificationFilter = TextureMagFilter.Nearest;

            /// <summary>Selects the Minification filter for following Textures to be loaded. (Default: Nearest)</summary>
            public static TextureMinFilter MinificationFilter = TextureMinFilter.Nearest;

            /// <summary>Selects the S Wrapping for following Textures to be loaded. (Default: Repeat)</summary>
            public static TextureWrapMode WrapModeS = TextureWrapMode.Repeat;

            /// <summary>Selects the T Wrapping for following Textures to be loaded. (Default: Repeat)</summary>
            public static TextureWrapMode WrapModeT = TextureWrapMode.Repeat;

            /// <summary>Selects the Texture Environment Mode for the following Textures to be loaded. Default: Modulate)</summary>
            public static TextureEnvMode EnvMode = TextureEnvMode.Modulate;
        }


    }
}
