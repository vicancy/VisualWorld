using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using WorldFramework.Controller;
using WorldFramework.Interface;
using BeginMode = OpenTK.Graphics.OpenGL.BeginMode;
using BufferTarget = OpenTK.Graphics.OpenGL.BufferTarget;
using BufferUsageHint = OpenTK.Graphics.OpenGL.BufferUsageHint;
using DrawElementsType = OpenTK.Graphics.OpenGL.DrawElementsType;
using EnableCap = OpenTK.Graphics.OpenGL.EnableCap;
using GL = OpenTK.Graphics.OpenGL.GL;
using InterleavedArrayFormat = OpenTK.Graphics.OpenGL.InterleavedArrayFormat;
using TextureCoordName = OpenTK.Graphics.OpenGL.TextureCoordName;
using TextureEnvMode = OpenTK.Graphics.OpenGL.TextureEnvMode;
using TextureGenMode = OpenTK.Graphics.OpenGL.TextureGenMode;
using TextureGenParameter = OpenTK.Graphics.OpenGL.TextureGenParameter;
using TextureMagFilter = OpenTK.Graphics.OpenGL.TextureMagFilter;
using TextureMinFilter = OpenTK.Graphics.OpenGL.TextureMinFilter;
using TextureTarget = OpenTK.Graphics.OpenGL.TextureTarget;
using TextureWrapMode = OpenTK.Graphics.OpenGL.TextureWrapMode;

namespace WorldFramework.Scene
{
    public class BoundingBox : BaseScene
    {
        private readonly int[] _uTexture = new int[6];
        private readonly float _xLength;
        private readonly float _zLength;
        private readonly float _yLength;
        private readonly float _fCoorX;
        private readonly float _fCoorZ;
        private readonly float _fCoorY;

        private string _texture = "../../Data/Textures/SkyBox/skybox_clean.dds";
        private TextureTarget _textureTarget;
        private uint _textureIndex;
        private VertexBufferObject _boundingBoxVertexBufferObject;
        private VertexC4ubV3f[] _vertices;
        private short[] _indices;
        private Vector3 _minBounding;
        private Vector3 _maxBounding;
        private Shaders _boundingBoxShader = new Shaders("../../Data/Shaders/sky.vert", "../../Data/Shaders/sky.frag");
        private Shaders.ShaderTextureParameters _shaderParameters;
        public BoundingBox(float xLength, float yLength, float zLength, float fCoorX, float fCoorY, float fCoorZ)
        {
            _xLength = xLength;
            _zLength = zLength;
            _yLength = yLength;
            _fCoorX = fCoorX;
            _fCoorZ = fCoorZ;
            _fCoorY = fCoorY;
            MinBounding = new Vector3(-xLength / 2.0f + fCoorX, -yLength / 2.0f + fCoorY, -zLength / 2.0f + fCoorZ);
            MaxBounding = new Vector3(xLength / 2.0f + fCoorX, yLength / 2.0f + fCoorY, zLength / 2.0f + fCoorZ);

        }

        public BoundingBox(Vector3 minBox,Vector3 maxBox)
        {
            MinBounding = minBox;
            MaxBounding = maxBox;
            _xLength = maxBox.X - minBox.X;
            _zLength = maxBox.Y - minBox.Y;
            _yLength = maxBox.Z - minBox.Z;
            _fCoorX = minBox.X / 2 + maxBox.X / 2;
            _fCoorY = minBox.Y / 2 + maxBox.Y / 2;
            _fCoorZ = minBox.Z / 2 + maxBox.Z / 2;
        }

        public Vector3 MinBounding
        {
            get { return _minBounding; }
            set { _minBounding = value; }
        }

        public Vector3 MaxBounding
        {
            get { return _maxBounding; }
            set { _maxBounding = value; }
        }

        #region Interface Methods
        public override void Initialize()
        {
            _vertices=new []
                          {
                              //new VertexN3fV3f(new Vector3(-1,-1,-1), MinBounding.X,MinBounding.Y,MinBounding.Z),
                              //new VertexN3fV3f(new Vector3(-1,-1,1),MinBounding.X,MinBounding.Y,MaxBounding.Z),
                              //new VertexN3fV3f(new Vector3(1,-1,1),MaxBounding.X,MinBounding.Y,MaxBounding.Z),
                              //new VertexN3fV3f(new Vector3(1,-1,-1),MaxBounding.X,MinBounding.Y,MinBounding.Z),
                              //new VertexN3fV3f(new Vector3(-1,1,-1),MinBounding.X,MaxBounding.Y,MinBounding.Z),
                              //new VertexN3fV3f(new Vector3(-1,1,1),MinBounding.X,MaxBounding.Y,MaxBounding.Z),
                              //new VertexN3fV3f(new Vector3(1,1,1),MaxBounding.X,MaxBounding.Y,MaxBounding.Z),
                              //new VertexN3fV3f(new Vector3(1,1,-1),MaxBounding.X,MaxBounding.Y,MinBounding.Z),
                              
                              new VertexC4ubV3f(MinBounding.X,MinBounding.Y,MinBounding.Z,Color.LightSteelBlue),
                              new VertexC4ubV3f(MinBounding.X,MinBounding.Y,MaxBounding.Z,Color.LightSteelBlue),
                              new VertexC4ubV3f(MaxBounding.X,MinBounding.Y,MaxBounding.Z,Color.LightSteelBlue),
                              new VertexC4ubV3f(MaxBounding.X,MinBounding.Y,MinBounding.Z,Color.LightSteelBlue),
                              new VertexC4ubV3f(MinBounding.X,MaxBounding.Y,MinBounding.Z,Color.DodgerBlue),
                              new VertexC4ubV3f(MinBounding.X,MaxBounding.Y,MaxBounding.Z,Color.DodgerBlue),
                              new VertexC4ubV3f(MaxBounding.X,MaxBounding.Y,MaxBounding.Z,Color.DodgerBlue),
                              new VertexC4ubV3f(MaxBounding.X,MaxBounding.Y,MinBounding.Z,Color.DodgerBlue),
 
                          };
            _indices = new short[]
                           {
                               0, 1, 5, 5, 4, 0,
                               1, 2, 6, 6, 5, 1,
                               2, 3, 7, 7, 6, 2,
                               3, 0, 4, 4, 7, 3,
                               4, 5, 6, 6, 7, 4,
                           };

            _boundingBoxVertexBufferObject = VertexBufferObject.LoadVBO(_vertices, _indices, BufferUsageHint.StaticDraw);
            
            Textures.TextureLoaderParameters.FlipImages = false;
            Textures.TextureLoaderParameters.MagnificationFilter = TextureMagFilter.Linear;
            Textures.TextureLoaderParameters.MinificationFilter = TextureMinFilter.Linear;
            Textures.TextureLoaderParameters.WrapModeS = TextureWrapMode.ClampToEdge;
            Textures.TextureLoaderParameters.WrapModeT = TextureWrapMode.ClampToEdge;
            Textures.TextureLoaderParameters.EnvMode = TextureEnvMode.Modulate;
            ImageDDS.LoadFromDisk(_texture, out _textureIndex, out _textureTarget);
            
            _shaderParameters = Shaders.ShaderTextureParameters.GetShaderTextureParameters(this.GetType());
            
            _boundingBoxShader.UseShader = false;
            base.Initialize();

        }

        public override void Draw()
        {
            _boundingBoxShader.UseShader = false;
            GL.PushMatrix();
            //GL.Translate(_fCoorX,_fCoorY,_fCoorZ);
            //GL.MultMatrix(ref _currentRotateMatrix);
            ////GL.Rotate(45,1,0,1);
            //GL.Translate(-_fCoorX,-_fCoorY,-_fCoorZ);
            GL.Enable(EnableCap.ColorArray);
            GL.Enable(EnableCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _boundingBoxVertexBufferObject.VboIdentity);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _boundingBoxVertexBufferObject.EboIdentity);
            //GL.InterleavedArrays(InterleavedArrayFormat.N3fV3f, 0, IntPtr.Zero);
            GL.InterleavedArrays(InterleavedArrayFormat.C4ubV3f, 0, IntPtr.Zero);
            if(_boundingBoxShader.UseShader)
            {
                GL.ActiveTexture(_shaderParameters.Unit);
                GL.BindTexture(_textureTarget, _textureIndex);
                _boundingBoxShader.PassTexture("Sky", _shaderParameters.UnitInteger);
            }
            else
            {
                //GL.Enable(EnableCap.TextureGenS);
                //GL.Enable(EnableCap.TextureGenT);
                //GL.Enable(EnableCap.TextureGenR);
                //GL.TexGen(TextureCoordName.S, TextureGenParameter.TextureGenMode, (int)TextureGenMode.NormalMap);
                //GL.TexGen(TextureCoordName.T, TextureGenParameter.TextureGenMode, (int)TextureGenMode.NormalMap);
                //GL.TexGen(TextureCoordName.R, TextureGenParameter.TextureGenMode, (int)TextureGenMode.NormalMap);

                //GL.Enable(EnableCap.TextureCubeMap);
                //GL.BindTexture(_textureTarget, _textureIndex);
            }
            //GL.Color4(1, 1, 1, 1);
            //GL.Enable(EnableCap.CullFace);
            //GL.CullFace(CullFaceMode.Back);
            GL.DrawElements(BeginMode.Triangles, _boundingBoxVertexBufferObject.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);
            GL.PopMatrix();
            if(!_boundingBoxShader.UseShader)
            {
                //GL.Disable(EnableCap.TextureGenS);
                //GL.Disable(EnableCap.TextureGenT);
                //GL.Disable(EnableCap.TextureGenR);
                   
                //GL.Disable(EnableCap.TextureCubeMap);

            }
            //GL.Disable(EnableCap.CullFace);
            _boundingBoxShader.UseShader = false;

        }

        public override void Destroy()
        {
            if (IsInitialized)
            {
                GL.DeleteTextures(1, ref _textureIndex);
                _boundingBoxShader.Destroy();
            }
        }
        #endregion
    }
}