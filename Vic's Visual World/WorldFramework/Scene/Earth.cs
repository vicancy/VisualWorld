using System;
using OpenTK;
using WorldFramework.Controller;
using WorldFramework.Interface;
using WorldFramework.Shapes;
using EnableCap = OpenTK.Graphics.OpenGL.EnableCap;
using GL = OpenTK.Graphics.OpenGL.GL;
using MaterialFace = OpenTK.Graphics.OpenGL.MaterialFace;
using PolygonMode = OpenTK.Graphics.OpenGL.PolygonMode;
using TextureCoordName = OpenTK.Graphics.OpenGL.TextureCoordName;
using TextureEnvMode = OpenTK.Graphics.OpenGL.TextureEnvMode;
using TextureGenMode = OpenTK.Graphics.OpenGL.TextureGenMode;
using TextureGenParameter = OpenTK.Graphics.OpenGL.TextureGenParameter;
using TextureMagFilter = OpenTK.Graphics.OpenGL.TextureMagFilter;
using TextureMinFilter = OpenTK.Graphics.OpenGL.TextureMinFilter;
using TextureTarget = OpenTK.Graphics.OpenGL.TextureTarget;
using TextureUnit = OpenTK.Graphics.OpenGL.TextureUnit;
using TextureWrapMode = OpenTK.Graphics.OpenGL.TextureWrapMode;

namespace WorldFramework.Scene
{
    public class Earth : BaseScene
    {
        #region Variables

        //Textures
        private Shaders.ShaderTextureParameters _shaderParameters;
        private string TMU0Filename = "../../Data/Textures/earth_cubemap.dds";
        private uint TMU0_Handler;
        private TextureTarget TMU0_Target;

        private Shaders _earthShader = new Shaders("../../Data/Shaders/earth.vert",
                                                   "../../Data/Shaders/earth.frag");

        //DL
        private DrawableShape sphere;
        public float Radius { get; set; }

        #endregion

        #region Interface Methods

        public override void Initialize()
        {
            _earthShader.UseShader = true;

            GL.Disable(EnableCap.Dither);
            GL.Enable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
            //GL.PolygonMode(MaterialFace.Back, PolygonMode.Line);
            //Textures
            Textures.TextureLoaderParameters.FlipImages = false;
            Textures.TextureLoaderParameters.MagnificationFilter = TextureMagFilter.Linear;
            Textures.TextureLoaderParameters.MinificationFilter = TextureMinFilter.Linear;
            Textures.TextureLoaderParameters.WrapModeS = TextureWrapMode.ClampToEdge;
            Textures.TextureLoaderParameters.WrapModeT = TextureWrapMode.ClampToEdge;
            Textures.TextureLoaderParameters.EnvMode = TextureEnvMode.Modulate;
            ImageDDS.LoadFromDisk(TMU0Filename, out TMU0_Handler, out TMU0_Target);
            _shaderParameters = Shaders.ShaderTextureParameters.GetShaderTextureParameters(this.GetType());
            Radius = 30;
            sphere = new SlicedSphere(Radius, Vector3d.Zero, SlicedSphere.eSubdivisions.Five,
                                      new SlicedSphere.eDir[] {SlicedSphere.eDir.All}, true);
            base.Initialize();
        }

        public override void Draw()
        {
            //GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            _earthShader.UseShader = true;
            GL.Enable(EnableCap.DepthTest);
            if (_earthShader.UseShader)
            {
                GL.ActiveTexture(_shaderParameters.Unit);
                GL.BindTexture(TMU0_Target, TMU0_Handler);
                _earthShader.PassTexture("Earth", _shaderParameters.UnitInteger);
            }
            else
            {
                GL.Enable(EnableCap.TextureGenS);
                GL.Enable(EnableCap.TextureGenT);
                GL.Enable(EnableCap.TextureGenR);
                GL.TexGen(TextureCoordName.S, TextureGenParameter.TextureGenMode, (int) TextureGenMode.NormalMap);
                GL.TexGen(TextureCoordName.T, TextureGenParameter.TextureGenMode, (int) TextureGenMode.NormalMap);
                GL.TexGen(TextureCoordName.R, TextureGenParameter.TextureGenMode, (int) TextureGenMode.NormalMap);

                GL.Enable(EnableCap.TextureCubeMap);
                GL.BindTexture(TMU0_Target, TMU0_Handler);
            }

            GL.Color3(1f, 1f, 1f);
            sphere.Draw();
            if (!_earthShader.UseShader)
            {
                GL.Disable(EnableCap.TextureCubeMap);
                GL.Disable(EnableCap.TextureGenS);
                GL.Disable(EnableCap.TextureGenT);
                GL.Disable(EnableCap.TextureGenR);
            }
            GL.Disable(EnableCap.DepthTest);
            _earthShader.UseShader = false;
            //GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
        }

        public override void Destroy()
        {
            if (IsInitialized)
            {
                sphere.Dispose();
                GL.DeleteTextures(1, ref TMU0_Handler);
                _earthShader.Destroy();
            }
        }

        public static void GetLocation(float radius, ref Vector3 vertexCoordinate, out Orientation orientation)
        {
            orientation = new Orientation();
            //Set up the base vertex
            //Suppose the vertex(0,0) is (startLongitude, 0N): 0N is definitely true;
            float startLongitude = -90;

            //Calculate Latitude: + N; - S
            orientation.latitude = MathHelper.RadiansToDegrees((float) Math.Asin(vertexCoordinate.Y/radius));
            //Calculate Longitude: x + (30E,180E,150W); x- W; z + (0+30 ; z- 180
            //x^2+z^2
            float currentRadius =
                (float) Math.Sqrt(vertexCoordinate.X*vertexCoordinate.X + vertexCoordinate.Z*vertexCoordinate.Z);
            orientation.longitude = (vertexCoordinate.Z >= 0)
                                        ? (360 +
                                           MathHelper.RadiansToDegrees(
                                               (float) Math.Asin(vertexCoordinate.X/currentRadius)) + startLongitude)%
                                          360
                                        : 180 -
                                          MathHelper.RadiansToDegrees(
                                              (float) Math.Asin(vertexCoordinate.X/currentRadius)) + startLongitude;


        }

        /// <summary>
        /// Get vertexPosition on the Earth
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="orientation"></param>
        /// <param name="vertexCoordinate"></param>
        public static void GetPosition(float radius, ref Orientation orientation, out Vector3 vertexCoordinate)
        {
            vertexCoordinate = new Vector3();
            //Set up the base vertex
            //Suppose the vertex(0,0) is (startLongitude, 0N): 0N is definitely true;
            float startLongitude = -90;
            float bigSin = (float) Math.Sin(MathHelper.DegreesToRadians(orientation.latitude));
            vertexCoordinate.Y = bigSin*radius;

            float currentRadius = radius*(float) Math.Sqrt(1 - bigSin*bigSin);
            float theta = orientation.longitude - startLongitude;
            float smallSin = (float) Math.Sin(MathHelper.DegreesToRadians(theta));
            vertexCoordinate.X = smallSin*currentRadius;
            vertexCoordinate.Z = -currentRadius*(float) Math.Sqrt(1 - smallSin*smallSin);
        }

        #endregion

        #region Helper Methods

        #endregion

    }
}