using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Resources;
using System.Text;
using OpenTK.Graphics.OpenGL;
using WorldFramework.Interface;

namespace WorldFramework.Controller
{
    public class Fonts:IObject
    {
        private TextureUnit TMU2_Unit = TextureUnit.Texture2;
        private int TMU2_UnitInteger = 2;
        /// <summary>
        /// Construction
        /// </summary>
        /// <param name="filename">Where you load your fonts.bmp</param>
        public Fonts()
            : this(0, 0, "Default", false)
        {

        }

        public Fonts(int x, int y, string text, bool italic)
        {
            X = x;
            Y = y;
            Text = text;
            Italic = italic;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public string Text { get; set; }
        public bool Italic { get; set; }
       public void Initialize()
        {
           //Whether texture has been loaded
            if (texture == 0)
            {
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
                Bitmap textureImage =
                (new ResourceManager("WorldFramework.Properties.Resources", typeof(Fonts).Assembly)).GetObject("Font")
                as Bitmap;

                Textures.LoadGLTextures(textureImage, ref texture);
                BuildFont();
            }
        }

       public void Draw()
       {
           glPaint(X, Y, Text, Italic ? 1 : 0);
       }

       public void Destroy()
       {
           KillFont();
       }
        #region static BuildFont
        private static int fontbase;
        private static int texture = 0;

        private static void BuildFont()
        {
            float cx;   //holds our x character coord
            float cy;   //..........y....
            fontbase = GL.GenLists(256);
            //GL.BindTexture(TextureTarget.Texture2D, texture);
            for (int i = 0; i < 256; i++)
            {
                cx = (float)(i % 16) / 16.0f;   //x position of current character
                cy = (float)(i / 16) / 16.0f;   //y...
                GL.NewList(fontbase + i, ListMode.Compile);
                GL.Begin(BeginMode.Quads);
                {
                    GL.TexCoord2(cx, 1 - cy - 0.0625f);  //Texture coord(bottom left)
                    GL.Vertex2(0, 0);
                    GL.TexCoord2(cx + 0.0625f, 1 - cy - 0.0625f);
                    GL.Vertex2(16, 0);   //bottom right
                    GL.TexCoord2(cx + 0.0625f, 1 - cy);
                    GL.Vertex2(16, 16);  //top right;
                    GL.TexCoord2(cx, 1 - cy);
                    GL.Vertex2(0, 16);   //top left                
                }
                GL.End();
                GL.Translate(10, 0, 0);
                GL.EndList();
            }

        }
        #endregion
        #region KillFont
        /// <summary>
        /// Call it when you are going to kill the fonts
        /// </summary>
        private void KillFont()
        {
            GL.DeleteLists(fontbase, 256);
        }
        #endregion
        #region glPaint
        /// <summary>
        /// where all of our drawing is down
        /// </summary>
        /// <param name="x">xposition on the screen</param>
        /// <param name="y">yposition on the screen, 0 at the bottom</param>
        /// <param name="text">what we want to print</param>
        /// <param name="set">set to 0 if normal character set wanted, 1 if italicized</param>
        private void glPaint(int x, int y, string text, int set)
        {

            if (set > 1)
            {
                set = 1;    //make sure set is 0 or 1
            }
            GL.LoadIdentity();
            
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest); //for blending to work better
            GL.MatrixMode(MatrixMode.Projection);  //select the projection matrix
            GL.PushMatrix();  //store the projection matrix
            GL.LoadIdentity();    //reset the projection matrix
            GL.Ortho(0, 800, 0, 600, -1, 1);  //set up an ortho screen
            GL.MatrixMode(MatrixMode.Modelview);   //select the modelview matrix
            GL.PushMatrix();  //store the modelview matrix
            GL.LoadIdentity();    //reset the modelview matrix
            GL.Translate(x, y, 0);   //position the text(0,0-bottom left);
            GL.ListBase(fontbase - 32 + (128 * set)); //choose the font set

            byte[] bytes = new byte[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                bytes[i] = (byte)text[i];
            }
            GL.CallLists(text.Length,ListNameType.UnsignedByte, bytes);
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();   //Restore the old projection matrix;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
        }

        #endregion
    }
}
