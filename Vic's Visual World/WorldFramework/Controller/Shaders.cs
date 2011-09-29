using System;
using System.Diagnostics;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using WorldFramework.Scene;

namespace WorldFramework.Controller
{
    public class Shaders
    {
        public class ShaderTextureParameters
        {
            public TextureUnit Unit { get; private set; }
            public int UnitInteger { get; private set; }

            private ShaderTextureParameters(TextureUnit unit, int integar)
            {
                Unit = unit;
                UnitInteger = integar;
            }

            public static ShaderTextureParameters GetShaderTextureParameters(Type type)
            {
                if (type == typeof (Earth))
                {
                    return new ShaderTextureParameters(TextureUnit.Texture0, 0);
                }
                if (type == typeof(BoundingBox))
                {
                    return new ShaderTextureParameters(TextureUnit.Texture1, 1);
                }
                //Terrain should be the last, multiple textures
                if (type == typeof (Terrain))
                {
                    return new ShaderTextureParameters(TextureUnit.Texture3, 3);
                }
                throw new InvalidOperationException("Type of " + type + " not supported in Shaders");
            }
        }

        //Shader
        private int _vertexShaderObject, _fragmentShaderObject, _programObject;
        private readonly string _vertexShaderFilename;
        private readonly string _fragmentShaderFilename;
        private bool _useShader;
        private bool _initialized;

        public bool UseShader
        {
            get { return _useShader; }
            set
            {
                if (_useShader != value)
                {
                    if (value)
                    {
                        if (!_initialized)
                        {
                            Initialize();
                        }

                        StartShader();
                    }
                    else
                    {
                        StopShader();
                    }
                    _useShader = value;
                }
            }
        }

        public Shaders(string vertexFilename,string fragmentFilename)
        {
            _vertexShaderFilename = vertexFilename;
            _fragmentShaderFilename = fragmentFilename;
        }

        private void Initialize()
        {
            string logInfo;
            //Shaders
            using (var streamReader = new StreamReader(_vertexShaderFilename))
            {
                _vertexShaderObject = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(_vertexShaderObject, streamReader.ReadToEnd());
                GL.CompileShader(_vertexShaderObject);
            }

            GL.GetShaderInfoLog(_vertexShaderObject, out logInfo);
            if (logInfo.Length > 0 && !logInfo.Contains("hardware"))
                Trace.WriteLine("Vertex Shader "+_vertexShaderFilename+" failed!\nLog:\n" + logInfo);
            else
                Trace.WriteLine("Vertex Shader compiled without complaint.");

            using (var streamReader = new StreamReader(_fragmentShaderFilename))
            {
                _fragmentShaderObject = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(_fragmentShaderObject, streamReader.ReadToEnd());
                GL.CompileShader(_fragmentShaderObject);
            }

            GL.GetShaderInfoLog(_fragmentShaderObject, out logInfo);
            if (logInfo.Length > 0 && !logInfo.Contains("hardware"))
                Trace.WriteLine("Fragment Shader " + _fragmentShaderFilename + "  failed!\nLog:\n" + logInfo);
            else
                Trace.WriteLine("Fragment Shader compiled without complaint.");

            _programObject = GL.CreateProgram();
            GL.AttachShader(_programObject, _vertexShaderObject);
            GL.AttachShader(_programObject, _fragmentShaderObject);
            GL.LinkProgram(_programObject);
            GL.DeleteShader(_vertexShaderObject);
            GL.DeleteShader(_fragmentShaderObject);

            var temp = new int[1];
            GL.GetProgram(_programObject, ProgramParameter.LinkStatus, out temp[0]);
            Trace.WriteLine("Linking Program (" + _programObject + ") " + ((temp[0] == 1) ? "succeeded." : "FAILED!"));
            if (temp[0] != 1)
            {
                GL.GetProgramInfoLog(_programObject, out logInfo);
                Trace.WriteLine("Program Log:\n" + logInfo);
            }

            GL.GetProgram(_programObject, ProgramParameter.ActiveAttributes, out temp[0]);
            Trace.WriteLine("Program registered " + temp[0] + " Attributes. (Should be 4: Pos, UV, Normal, Tangent)");

            Trace.WriteLine("Tangent attribute bind location: " + GL.GetAttribLocation(_programObject, "AttributeTangent"));

            Trace.WriteLine("End of Shader build. GL Error: " + GL.GetError());
            _initialized = true;
        }

        public void StartShader()
        {
            //GL.LinkProgram(ProgramObject);
            GL.UseProgram(_programObject);
        }

        public void PassTexture(string key, int texture)
        {
            //GL.UseProgram(_programObject);
            GL.Uniform1(GL.GetUniformLocation(_programObject, key), texture);
        }

        public void PassParameter(string key, Vector3 value)
        {
            GL.Uniform3(GL.GetUniformLocation(_programObject, key), value);
        }

        public void PassParameter(string key, float value)
        {
            GL.Uniform1(GL.GetUniformLocation(_programObject, key), value);
        }

        public void StopShader()
        {
            GL.UseProgram(0);
        }

        public void Destroy()
        {
            GL.DeleteProgram(_programObject);
        }
    }
}