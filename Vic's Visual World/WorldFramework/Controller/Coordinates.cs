using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using OpenTK.Graphics.OpenGL;
using WorldFramework.Interface;

namespace WorldFramework.Controller
{
    public class Coordinates:BaseScene
    {
        private float _length;
        private Color _xColor = Color.DodgerBlue;
        private Color _yColor = Color.CadetBlue;
        private Color _zColor = Color.Firebrick;
        private VertexC4ubV3f[] _lines;
        private short[] _indices;

        private VertexBufferObject _coordVertexBufferObject;
        public Coordinates(float length,Color xColor, Color yColor, Color zColor):this(length)
        {
            _xColor = xColor;
            _yColor = yColor;
            _zColor = zColor;
        }

        public Coordinates(float length)
        {
            _length = length;
        }

        public override void Initialize()
        {
            _lines=new []
                       {
                           new VertexC4ubV3f(-_length,0,0,_xColor),
                           new VertexC4ubV3f(_length,0,0,_xColor),
                           new VertexC4ubV3f(0,-_length,0,_yColor),
                           new VertexC4ubV3f(0,_length,0,_yColor),
                           new VertexC4ubV3f(0,0,-_length,_zColor),
                           new VertexC4ubV3f(0,0,_length,_zColor),
 
                       };
            _indices=new short[]
                         {
                             0,1,
                             2,3,
                             4,5
                         };
            _coordVertexBufferObject = WorldFramework.VertexBufferObject.LoadVBO(_lines, _indices, BufferUsageHint.StaticDraw);
            base.Initialize();
        }

        public override void Draw()
        {
            GL.Disable(EnableCap.Texture2D);
            GL.Enable(EnableCap.ColorArray);
            GL.Enable(EnableCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _coordVertexBufferObject.VboIdentity);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _coordVertexBufferObject.EboIdentity);
            GL.InterleavedArrays(InterleavedArrayFormat.C4ubV3f, 0, IntPtr.Zero);
            GL.DrawElements(BeginMode.Lines, _coordVertexBufferObject.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);

        }

        public override void Destroy()
        {
            
        }
    }
}
