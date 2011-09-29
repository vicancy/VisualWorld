using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using WorldFramework.Controller;
using WorldFramework.Interface;

namespace WorldFramework.Scene
{
    public class StarrySky:BaseScene
    {
        #region Variables
        private const int MAX_PARTICLE = 1000;
        private TextureUnit TMU1_Unit = TextureUnit.Texture1;
        private int TMU1_UnitInteger = 1;
        private Particle[] particles = new Particle[MAX_PARTICLE];
        Random random = new Random();
        private VertexBufferObject _vertexBufferObjectHanle;
        private int texture;
        private VertexT2C4ubV3f[] TexturedVerticeArrays;
        private List<VertexT2C4ubV3f> TexturedVertices
           = new List<VertexT2C4ubV3f>
                  {
                      new VertexT2C4ubV3f(0.0f,0.0f,0.0f,0.0f,0.0f,Color.FromArgb(100,3,42,32)),
                      new VertexT2C4ubV3f(1.0f,0.0f,1.0f,0.0f,0.0f,Color.FromArgb(100,3,42,32)),
                      new VertexT2C4ubV3f(1.0f,1.0f,1.0f,1.0f,0.0f,Color.FromArgb(100,3,42,32)),
                      new VertexT2C4ubV3f(0.0f,1.0f,0.0f,1.0f,0.0f,Color.FromArgb(100,3,42,32)),
                          
                      new VertexT2C4ubV3f(0.0f,0.0f,8.0f,8.0f,-10.0f,Color.FromArgb(200,33,142,32)),
                      new VertexT2C4ubV3f(1.0f,0.0f,10.0f,8.0f,-10.0f,Color.FromArgb(200,33,142,32)),
                      new VertexT2C4ubV3f(1.0f,1.0f,10.0f,10.0f,-10.0f,Color.FromArgb(200,33,142,32)),
                      new VertexT2C4ubV3f(0.0f,1.0f,8.0f,10.0f,-10.0f,Color.FromArgb(200,33,142,32)),
                  };

        private List<short> TexturedElements = new List<short>
                                                        {
                                                            0, 1, 2, 2, 3, 0,
                                                            4,5,6,6,7,4,
                                                        };
        #endregion
        public override void Initialize()
        {
            InitializeParticles();
            ConstructParticles();
            Textures.LoadGLTextures("Star.bmp", ref texture);
            TexturedVerticeArrays = TexturedVertices.ToArray();
            _vertexBufferObjectHanle = VertexBufferObject.LoadVBO(TexturedVerticeArrays, TexturedElements.ToArray(),
                                                  BufferUsageHint.StreamDraw);
            base.Initialize();
        }

        public override void Draw()
        {
            GL.PushMatrix();
            GL.Enable(EnableCap.ColorArray);
            GL.Enable(EnableCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObjectHanle.VboIdentity);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vertexBufferObjectHanle.EboIdentity);
            GL.InterleavedArrays(InterleavedArrayFormat.T2fC4ubV3f, 0, IntPtr.Zero);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texture);
          
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);

            GL.Disable(EnableCap.DepthTest);

            GL.BufferData(BufferTarget.ArrayBuffer,
                          (IntPtr) (TexturedVerticeArrays.Length*BlittableValueType.StrideOf(TexturedVerticeArrays)),
                          IntPtr.Zero, BufferUsageHint.StreamDraw);
            UpdateVertices();
            GL.BufferData(BufferTarget.ArrayBuffer,
                          (IntPtr) (TexturedVerticeArrays.Length*BlittableValueType.StrideOf(TexturedVerticeArrays)),
                          TexturedVerticeArrays, BufferUsageHint.StreamDraw);

            GL.DrawElements(BeginMode.Triangles, _vertexBufferObjectHanle.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);
            GL.PopMatrix();

            GL.Disable(EnableCap.Blend);

            GL.Enable(EnableCap.DepthTest);

            GL.Disable(EnableCap.Texture2D);
        }

        public override void Destroy()
        {
            GL.DeleteBuffers(1, ref _vertexBufferObjectHanle.VboIdentity);
            GL.DeleteBuffers(1, ref _vertexBufferObjectHanle.EboIdentity);
        }

        private void UpdateVertices()
        {
            int i = random.Next(0, TexturedVerticeArrays.Length - 3);
            i = (i >> 2) << 2;
            //for (int i = 0; i < TexturedVerticeArrays.Length; i += 4)
            {
                //float xBias = random.Next(-1, 1);
                //float yBias = random.Next(-1, 1);
                //float zBias = random.Next(-1, 1);
                //TexturedVerticeArrays[i].Position.X += xBias;
                //TexturedVerticeArrays[i].Position.Y += yBias;
                //TexturedVerticeArrays[i].Position.Z += zBias;
                //TexturedVerticeArrays[i + 1].Position.X += xBias;
                //TexturedVerticeArrays[i + 1].Position.Y += yBias;
                //TexturedVerticeArrays[i + 1].Position.Z += zBias;
                //TexturedVerticeArrays[i + 2].Position.X += xBias;
                //TexturedVerticeArrays[i + 2].Position.Y += yBias;
                //TexturedVerticeArrays[i + 2].Position.Z += zBias;
                //TexturedVerticeArrays[i + 3].Position.X += xBias;
                //TexturedVerticeArrays[i + 3].Position.Y += yBias;
                //TexturedVerticeArrays[i + 3].Position.Z += zBias;
                uint color = TexturedVerticeArrays[i].Color;
                uint newColor = color & (uint)0xffffff | ((uint)random.Next(0, 256) << 24);

                TexturedVerticeArrays[i].Color = newColor;
                TexturedVerticeArrays[i + 1].Color = newColor;
                TexturedVerticeArrays[i + 2].Color = newColor;
                TexturedVerticeArrays[i + 3].Color = newColor;

            }
        }
        private void InitializeParticles()
        {
            //Set data for particles

            for (int i = 0; i < MAX_PARTICLE; i++)
            {
                particles[i].X = random.Next(-150, 150);
                particles[i].Y = random.Next(-100, 100);
                particles[i].Z = 10;
                particles[i].Radias = random.Next(1, 100) / 100.0f;
                particles[i].Color = Color.FromArgb(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256),
                                                    random.Next(0, 256));
            }

        }
        private void ConstructParticles()
        {
            foreach (var particle in particles)
            {
                TexturedVertices.Add(new VertexT2C4ubV3f(0.0f, 0.0f, particle.X - particle.Radias,
                                                               particle.Y - particle.Radias, particle.Z, particle.Color));
                TexturedVertices.Add(new VertexT2C4ubV3f(1.0f, 0.0f, particle.X + particle.Radias,
                                                               particle.Y - particle.Radias, particle.Z, particle.Color));
                TexturedVertices.Add(new VertexT2C4ubV3f(1.0f, 1.0f, particle.X + particle.Radias,
                                                               particle.Y + particle.Radias, particle.Z, particle.Color));
                TexturedVertices.Add(new VertexT2C4ubV3f(0.0f, 1.0f, particle.X - particle.Radias,
                                                               particle.Y + particle.Radias, particle.Z, particle.Color));

                short index = (short)(TexturedVertices.Count - 4);
                TexturedElements.Add(index);
                TexturedElements.Add((short)(index + 1));
                TexturedElements.Add((short)(index + 2));
                TexturedElements.Add((short)(index + 2));
                TexturedElements.Add((short)(index + 3));
                TexturedElements.Add(index);
            }
        }
    }
}
