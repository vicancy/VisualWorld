//#define DynamicVBO
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace WorldFramework
{
    public class VertexBufferObject
    {
        public int VboIdentity;
        public int EboIdentity;
        public int NumElements;
        
        #region VBO
        public static VertexBufferObject LoadVBO<TVertex>(TVertex[] vertices, short[] elements, BufferUsageHint bufferUsageHint)
            where TVertex : struct
        {
            VertexBufferObject handle = new VertexBufferObject();
            int size;
            GL.GenBuffers(1, out handle.VboIdentity);
            GL.BindBuffer(BufferTarget.ArrayBuffer, handle.VboIdentity);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (vertices.Length*BlittableValueType.StrideOf(vertices)),
                          vertices, bufferUsageHint);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * BlittableValueType.StrideOf(vertices) != size)
            {
                throw new ApplicationException("Vertex data not uploaded correctly");
            }
            GL.GenBuffers(1, out handle.EboIdentity);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.EboIdentity);

            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * sizeof(short)), elements,
                          BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (elements.Length * sizeof(short) != size)
            {
                throw new ApplicationException("Vertex data not uploaded correctly");
            }

            handle.NumElements = elements.Length;

            return handle;

        }
        private void Draw(VertexBufferObject handle)
        {
            GL.PushMatrix();
            GL.Enable(EnableCap.ColorArray);
            GL.Enable(EnableCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, handle.VboIdentity);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.EboIdentity);
            //GL.InterleavedArrays(InterleavedArrayFormat.C4ubV3f, 0, IntPtr.Zero);
            GL.InterleavedArrays(InterleavedArrayFormat.T2fC4ubV3f, 0, IntPtr.Zero);

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
            GL.Disable(EnableCap.DepthTest);

#if DynamicVBO
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(TexturedVerticeArrays.Length * BlittableValueType.StrideOf(TexturedVerticeArrays)),
                          IntPtr.Zero, BufferUsageHint.StreamDraw);
            UpdateVertices();
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(TexturedVerticeArrays.Length * BlittableValueType.StrideOf(TexturedVerticeArrays)),
                          TexturedVerticeArrays, BufferUsageHint.StreamDraw);
            
#endif
            GL.DrawElements(BeginMode.Triangles, handle.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);
            GL.PopMatrix();
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Blend);
        }

        
        #endregion
    }
}
