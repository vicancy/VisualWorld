using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using WorldFramework.Controller;
using WorldFramework.Interface;

namespace WorldFramework.Scene
{
    public class Terrain : BaseScene
    {
        #region Variables

        private Random random = new Random();
        private string terrainFile = "Height.txt";
        private int _xNumber = 121; //Number of Vertices along x
        private int _zNumber = 121;
        private float _xDistance = 10; //Distance between two neighbor vertices
        private float _zDistance = 10;
        private float _heightScale = 1f;
        private VertexBufferObject _terrainVertexBufferObject;
        private int maxHeight, minHeight;
        private VertexT2fC3fV3f[] _triangleVertices;

        private int[] vertexHeight;

        private short[] triangleElements;
        private List<short> rectangleIndices = new List<short>();
        private Shaders _terrainShader = new Shaders(@"../../Data/Shaders/terrain.vert", @"../../Data/Shaders/terrain.frag");
        private int[] _textureIndices;
        private Shaders.ShaderTextureParameters _shaderParameters;

        private string[] _textures = new string[]
                                         {
                                             "\\terrain_detail_NM.bmp",
                                             "\\sandfloor009a.jpg",
                                             "\\terrain_rocky_map_1024.png",
                                             "\\terrain_grass_map_1024.png",
                                             "\\terrain_water_caustics.jpg",
                                             "\\test_heightmap512_2_diffusemap.jpg",
                                         };

        private string[] _shaderKeys = new string[]
                                                 {
                                                     "texNormalHeightMap",
                                                     "texDiffuse0",
                                                     "texDiffuse1",
                                                     "texDiffuse2",
                                                     "texWaterCaustics",
                                                     "texDiffuseMap",
                                                 };
        private Vector2 _sunAngle = new Vector2(2, 1);
        private Vector4 _sunVector;
        private Vector4 _sunColor;

        #endregion

        public Terrain()
        {
            _sunVector = new Vector4((float)(-Math.Cos(_sunAngle.X) * Math.Sin(_sunAngle.Y)),
                                     (float)(-Math.Cos(_sunAngle.Y)),
                                     (float)(-Math.Sin(_sunAngle.X) * Math.Sin(_sunAngle.Y)),
                                     0);
            _sunColor = Vector4.Lerp(new Vector4(1, 0.5f, 0, 1), new Vector4(1, 1, 0.8f, 1),
                                     0.25f + (float) Math.Cos(_sunAngle.Y)*0.75f);
            _textureIndices=new int[_textures.Length];
        }

        public Terrain(float heightScale, float x, float z)
            : this()
        {
            _heightScale = heightScale;
            _xDistance = x;
            _zDistance = z;

        }

        public Terrain(float heightScale, float x, float z, int xNumber, int zNumber)
            : this(heightScale, x, z)
        {
            _xNumber = xNumber;
            _zNumber = zNumber;
        }

        #region Interface Methods

        public override void Initialize()
        {
            _terrainShader.UseShader = true;
            _triangleVertices = new VertexT2fC3fV3f[_xNumber * _zNumber];

            vertexHeight = LoadTerrain(terrainFile, out minHeight, out maxHeight);
            if (vertexHeight != null)
            {
                InitializeVertices();
                triangleElements = rectangleIndices.ToArray();
                _terrainVertexBufferObject = VertexBufferObject.LoadVBO(_triangleVertices, triangleElements, BufferUsageHint.StaticDraw);
            }
            _shaderParameters = Shaders.ShaderTextureParameters.GetShaderTextureParameters(this.GetType());

            Textures.LoadGLTextures(_textures, ref _textureIndices);

            GL.Enable(EnableCap.Light0);
            GL.Light(LightName.Light0, LightParameter.SpotDirection, 0);
            GL.Light(LightName.Light0, LightParameter.Position, _sunVector);
            GL.Light(LightName.Light0, LightParameter.Ambient, Color.White);
            GL.Light(LightName.Light0, LightParameter.Diffuse, _sunColor);
            GL.Light(LightName.Light0, LightParameter.Specular, _sunColor);
            base.Initialize();
        }

        public override void Draw()
        {

            GL.PushMatrix();
            _terrainShader.UseShader = true;

            //GL.Enable(EnableCap.DepthTest);  
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Translate(-_xDistance*_xNumber/2, -10, -_zDistance*_zNumber/2);
            //GL.Enable(EnableCap.TextureCoordArray);
            GL.Enable(EnableCap.ColorArray);
            GL.Enable(EnableCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _terrainVertexBufferObject.VboIdentity);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _terrainVertexBufferObject.EboIdentity);
            GL.InterleavedArrays(InterleavedArrayFormat.T2fC4ubV3f, 0, IntPtr.Zero);
            
            
            if(_terrainShader.UseShader)
            {
                _terrainShader.PassParameter("bbox_min", new Vector3(0, minHeight*_heightScale, 0));
                _terrainShader.PassParameter("bbox_max", new Vector3(_xDistance*_xNumber, maxHeight*_heightScale, _zDistance*_zNumber));
                _terrainShader.PassParameter("detail_scale", 50);
                _terrainShader.PassParameter("diffuse_scale", 50);
                _terrainShader.PassParameter("water_height", -5);
                _terrainShader.PassParameter("water_reflection_rendering", 0);
                _terrainShader.PassParameter("time", DateTime.Now.Ticks);
                _terrainShader.PassParameter("depth_map_size",512);
                _terrainShader.PassParameter("fog_color", new Vector3(0.7f, 0.7f, 0.9f));
                for (int i = 0; i < _shaderKeys.Length; i++)
                {
                    GL.ActiveTexture(_shaderParameters.Unit + i);
                    GL.BindTexture(TextureTarget.Texture2D, _textureIndices[i]);
                    _terrainShader.PassTexture(_shaderKeys[i], _shaderParameters.UnitInteger + i);
                }

            }
            else
            {
                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, _textureIndices[2]);              
            }

            //GL.Enable(EnableCap.Blend);
            //GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
            //GL.Disable(EnableCap.DepthTest);
            //GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
           // GL.PolygonMode(MaterialFace.Back, PolygonMode.Line);
            //GL.Rotate(-90, 1, 0, 0);
            GL.DrawElements(BeginMode.Triangles, _terrainVertexBufferObject.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);
            if (!_terrainShader.UseShader)
            {
                GL.Disable(EnableCap.Texture2D);
            };
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Light0);
            GL.Disable(EnableCap.Fog);
            GL.PopMatrix();
            _terrainShader.UseShader = false;

            //GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
        }

        public override void Destroy()
        {
            if (IsInitialized)
            {
                GL.DeleteTextures(_textureIndices.Length, _textureIndices);
                _terrainShader.Destroy();
            }
        }

        private void InitializeVertices()
        {
            for (short k = 0; k < _zNumber; k++)
            {
                for (short i = 0; i < _xNumber; i++)
                {
                    int currentHeight = vertexHeight[i + _xNumber*k];
                    _triangleVertices[i + _xNumber*k] = new VertexT2fC3fV3f(i/(float) _xNumber, k/(float) _zNumber,
                                                                                  _xDistance*i,
                                                                                  currentHeight*_heightScale,
                                                                                  _zDistance*k,
                                                                                  Color.FromArgb(
                                                                                      (currentHeight - minHeight)*255/
                                                                                      (maxHeight - minHeight),
                                                                                      (currentHeight - minHeight)*255/
                                                                                      (maxHeight - minHeight),
                                                                                      (currentHeight - minHeight)*255/
                                                                                      (maxHeight - minHeight))

                        );
                    if ((i < _xNumber - 1) && (k < _zNumber - 1))
                        rectangleIndices.AddRange(new short[]
                                                      {
                                                          (short) (i + _xNumber*k), (short) (i + _xNumber*k + 1),
                                                          (short) (i + _xNumber*k + 1 + _xNumber),
                                                          (short) (i + _xNumber*k + 1 + _xNumber),
                                                          (short) (i + _xNumber*k + _xNumber), (short) (i + _xNumber*k)
                                                      });
                }
            }


        }

        #endregion

        #region Helper Methods

        private int[] LoadTerrain(string fileName, out int min, out int max)
        {
            min = 0;
            max = 0;
            if (fileName == null || fileName == string.Empty)
            {
                // Make Sure A Filename Was Given
                return null; // If Not Return Null
            }

            string fileName1 = string.Format("Data{0}{1}", // Look For Data\Filename
                                             Path.DirectorySeparatorChar, fileName);
            string fileName2 = string.Format("{0}{1}{0}{1}Data{1}{2}", // Look For ..\..\Data\Filename
                                             "..", Path.DirectorySeparatorChar, fileName);

            // Make Sure The File Exists In One Of The Usual Directories
            if (!File.Exists(fileName) && !File.Exists(fileName1) && !File.Exists(fileName2))
            {
                return null; // If Not Return Null
            }

            if (File.Exists(fileName))
            {
                // Does The File Exist Here?
                return ReadData(fileName, out min, out max); // Load The Bitmap
            }
            else if (File.Exists(fileName1))
            {
                // Does The File Exist Here?
                return ReadData(fileName1, out min, out max); // Load The Bitmap
            }
            else if (File.Exists(fileName2))
            {
                // Does The File Exist Here?
                return ReadData(fileName2, out min, out max); // Load The Bitmap
            }

            return null;
        }

        private int[] ReadData(string fileName, out int min, out int max)
        {
            min = 999;
            max = -999;
            List<int> heightLists = new List<int>();
            using (StreamReader streamReader = new StreamReader(fileName))
            {
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        int data = int.Parse(line);
                        if (data < min) min = data;
                        if (data > max) max = data;
                        heightLists.Add(int.Parse(line));
                    }
                }
            }
            return heightLists.ToArray();
        }
        #endregion
    }
}
