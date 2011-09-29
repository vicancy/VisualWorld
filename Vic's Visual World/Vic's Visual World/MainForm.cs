using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using WorldFramework.Controller;
using WorldFramework.Interface;
using WorldFramework.Scene;
using ClearBufferMask = OpenTK.Graphics.OpenGL.ClearBufferMask;
using EnableCap = OpenTK.Graphics.OpenGL.EnableCap;
using GetPName = OpenTK.Graphics.OpenGL.GetPName;
using GL = OpenTK.Graphics.OpenGL.GL;
using HintMode = OpenTK.Graphics.OpenGL.HintMode;
using HintTarget = OpenTK.Graphics.OpenGL.HintTarget;
using MatrixMode = OpenTK.Graphics.OpenGL.MatrixMode;
using Orientation = WorldFramework.Controller.Orientation;

namespace VirtualWorld
{
    public partial class MainForm : Form
    {
        #region Variables
        private uint _currentFps;
        private double _totalMilliseconds;
        private int _left = 20;
        private float _scale = 1.4f;//(1-60) how to continiously magnify
        private int _delta; //to Unit
        private int _leftKey; //to Unit
        private int _rightKey; //to Unit
        private int _upKey; //to Unit
        private int _downKey; //to Unit
        private bool _contextLoaded;
        private bool _earthLoaded;
        private bool _clickOutOfSphere;
        private bool _beginTerrainAnimation;
        private bool _isAnimationEnabled = false;
        private readonly Stopwatch _stopWatch = new Stopwatch();
        private Vector3 _startVec;
        private float _xLong;
        private float _yLong;
        private float _rotateAngle;
        private float _yMove;
        private float _zMove;
        private float _initRadius;
        private const float ChangeLevel = 61;
        private Vector3 _eyePosition;
        private readonly double[] _projectionMatrix = new double[16];
        private readonly double[] _viewmodelMatrix = new double[16];
        private readonly int[] _viewportMatrix = new int[16];
        private readonly double[] _terrainProjectionMatrix = new double[16];
        private ArcBall _terrainArcball;
        private StarrySky _starrySky = new StarrySky();
        private Earth _earth = new Earth();
        private BoundingBox _boundingBox = //new BoundingBox(20, 20, 20, 0, 0, -50);
            new BoundingBox(new Vector3(-500, -200, -500), new Vector3(500, 800, 500));
        private Coordinates _worldCoordinate = new Coordinates(600);
        private Coordinates _terrainCoordinate = new Coordinates(600, Color.Fuchsia, Color.ForestGreen,
                                                                 Color.DarkSlateGray);

        private Terrain _terrain = new Terrain();
        private Fonts _fonts = new Fonts();

        private readonly List<IObject> _universeScene = new List<IObject>();

        private readonly List<IObject> _universeObjects = new List<IObject>();

        private readonly List<IObject> _worldScene = new List<IObject>();

        private readonly List<IObject> _worldObjects = new List<IObject>();

        private readonly List<IObject> _sceneHelper = new List<IObject>();


        private CurrentSceneView _currentView = CurrentSceneView.EarthView;
        #endregion

        public MainForm()
        {
            InitializeComponent();
            webBrowser1.Navigate(Application.StartupPath + @"\GoogleMap.htm");
            InitializeScenes();
        }

        private void InitializeScenes()
        {
            _universeScene.Add(_starrySky);
            _universeObjects.Add(_earth);
            _worldScene.Add(_boundingBox);
            _worldScene.Add(_worldCoordinate);
            _worldObjects.Add(_terrain);
            _worldObjects.Add(_terrainCoordinate);
            _sceneHelper.Add(_fonts);
        }

        private void ApplicationIdle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                _stopWatch.Stop();
                double milliseconds = _stopWatch.Elapsed.TotalMilliseconds;

                _currentFps++;
                _stopWatch.Reset();
                _totalMilliseconds += milliseconds;
                if (_totalMilliseconds > 1000)
                {
                    label1.Text = _currentFps.ToString();
                    _totalMilliseconds -= 1000;
                    _currentFps = 0;

                }
                _stopWatch.Start();
                DrawScene();
                glControl1.Invalidate();
            }
        }

        private void SetupViewport()
        {
            int width = glControl1.Width;
            int height = glControl1.Height;
            float aspectRatio = width / (float)height;

            if (_terrainArcball==null)
            {
                _terrainArcball=new ArcBall(width,height);
            }
            else
            {
                _terrainArcball.SetBounds(width, height);                
            }

            GL.Viewport(0, 0, width, height);
            Matrix4 perpective;
            switch (_currentView)
            {
                case CurrentSceneView.UniversalView:
                    perpective = Matrix4.CreateOrthographic(100 * aspectRatio, 100, 1, 1000);
                    break;
                case CurrentSceneView.EarthView:
                    perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 1000);
                    break;
                default:
                    perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 1000);
                    break;
            }
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perpective);
            Glu.LookAt(0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0);
            if (_currentView == CurrentSceneView.UniversalView)
            {
                GL.GetDouble(GetPName.ProjectionMatrix, _projectionMatrix);
                GL.GetDouble(GetPName.ModelviewMatrix, _viewmodelMatrix);
                GL.GetInteger(GetPName.Viewport, _viewportMatrix);
            }
            //GL.GetFloat(GetPName.ProjectionMatrix, projectionMatrix);
            //GL.GetFloat(GetPName.ModelviewMatrix,viewmodelMatrix);
        }

        void TransformScene(CurrentSceneView view)
        {
            switch (view)
            {
                case CurrentSceneView.UniversalView:
                    //Should move to place corresponding to the earth position
                    //Currently just remove the control parameters
                    _leftKey = _rightKey = _upKey = _downKey = _delta = 0;
                    _rotateAngle = _yMove = _zMove = 0;
                    _beginTerrainAnimation = false;
                    _currentView = CurrentSceneView.UniversalView;
                    break;
                case CurrentSceneView.EarthView:
                    _currentView = CurrentSceneView.EarthView;
                    break;
            }
        }

        private void DrawScene()
        {
            //@2011.9.11: Currently disabled to focus on single view mode scene 
            //TransformScene(_scale >= ChangeLevel ? CurrentSceneView.EarthView : CurrentSceneView.UniversalView);

            Matrix4 perpective;
            switch (_currentView)
            {
                case CurrentSceneView.UniversalView:
                    #region case UniversalView
                    perpective = Matrix4.CreateOrthographic(100 * glControl1.Width / (float)glControl1.Height, 100, 1, 2000);
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.PushMatrix();
                    GL.LoadIdentity();
                    GL.MultMatrix(ref perpective);
                    Glu.LookAt(0.0, 0.0, 2000.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0);
                    GL.GetDouble(GetPName.ProjectionMatrix, _projectionMatrix);
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.LoadIdentity();

                    foreach (BaseScene obj in _universeScene)
                    {
                        if (!obj.IsInitialized)
                        {
                            obj.Initialize();
                        }
                    }
                    foreach (BaseScene obj in _universeObjects)
                    {
                        if (!obj.IsInitialized)
                        {
                            obj.Initialize();
                            if (obj is Earth)
                            {
                                _initRadius = (obj as Earth).Radius;
                            }
                        }
                    }
                    foreach (IObject obj in _universeScene)
                    {
                        obj.Draw();
                    }

                    #region Arcball and the objects
                   
                    GL.PushMatrix();
                    // 计算新的旋转矩阵，即：M = E · R = R
                    GL.Rotate(_theta, _axis[0], _axis[1], _axis[2]);

                    // 左乘上前一次的矩阵，即：M = R · L
                    GL.MultMatrix(_lastMatrix);
                    //保存此次处理结果，即：L = M
                    GL.GetFloat(GetPName.ModelviewMatrix, _lastMatrix);
                    //Rotate to China
                    //Animations to be added
                    GL.Rotate(-212, 0, 1, 0);
                    GL.Rotate(-32, 1, 0, 0);
                    GL.GetFloat(GetPName.ModelviewMatrix, _earthViewMatrix);
                    _earthLoaded = true;
                    GL.Scale(_scale, _scale, _scale);
                    _earth.Radius = _initRadius * _scale;
                    foreach (IObject obj in _universeObjects)
                    {
                        obj.Draw();
                    }
                    _theta = 0.0f;
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.PopMatrix();

                    #endregion

                    //DrawTerrain();
                    foreach (IObject obj in _sceneHelper)
                    {
                        obj.Draw();
                    }
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.PopMatrix();

                    #endregion
                    break;
                case CurrentSceneView.EarthView:
                    #region case EarthView
                    perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                      glControl1.Width / (float)glControl1.Height, 1,
                                                                      1000);
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.PushMatrix();
                    GL.LoadIdentity();
                    GL.LoadMatrix(ref perpective);
                    Glu.LookAt(0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0);
                    DrawTerrain();
                    foreach (IObject obj in _sceneHelper)
                    {
                        obj.Draw();
                    }
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.PopMatrix();

                    #endregion case EarthView

                    break;
            }
        }

        private void DrawTerrain()
        {
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                       glControl1.Width / (float)glControl1.Height, 1,
                                                                       1000);
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.LoadMatrix(ref perpective);
            //Glu.LookAt(0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0);
            GL.GetDouble(GetPName.ProjectionMatrix, _terrainProjectionMatrix);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
            foreach (BaseScene obj in _worldScene)
            {
                if (!obj.IsInitialized)
                {
                    obj.Initialize();
                }
            }

            foreach (BaseScene obj in _worldObjects)
            {
                if (!obj.IsInitialized)
                {
                    obj.Initialize();
                }
            }

            if (_currentView == CurrentSceneView.UniversalView)
            {
                _startVec = GetTerrainPosition(new Orientation(121, 30));
                Vector3 rightDownVec = GetTerrainPosition(new Orientation(123, 30));
                Vector3 leftUpVec = GetTerrainPosition(new Orientation(121, 32));
                _xLong = VectorDistance(ref rightDownVec, ref _startVec);
                _yLong = VectorDistance(ref leftUpVec, ref _startVec);
                //GL.Rotate(90,1,0,0);
                //GL.Translate(-60 + rightKey - leftKey, -60 + upKey - downKey, delta * 10);
                GL.Translate(_startVec);
                GL.Scale(_xLong / 121.0, _yLong / 121.0, 1);
            }
            else if (_currentView == CurrentSceneView.EarthView)
            {


                //GL.Rotate(90,1,0,0);
                //Add Animation for Rotating
                if (_isAnimationEnabled)
                {
                    if (!_beginTerrainAnimation)
                    {
                        if (_rotateAngle < 60)
                        {
                            _rotateAngle += 60/2000.0f;
                        }
                        if (_yMove < 60)
                        {
                            _yMove += 60/2000.0f;
                        }
                        if (_zMove < 30)
                        {
                            _zMove += 30/2000.0f;
                        }
                        if (_rotateAngle >= 60 && _yMove >= 60 && _zMove >= 30) _beginTerrainAnimation = true;
                    }
                    GL.Rotate(-_rotateAngle, 1, 0, 0);
                    GL.Translate(_startVec);
                    GL.Translate(_rightKey - _leftKey, _upKey - _downKey - _yMove, _delta + _zMove);
                    GL.Scale(_xLong/121.0, _yLong/121.0, 1);
                } 

                //GL.Translate(_rightKey - _leftKey, _upKey - _downKey - _yMove, _delta + _zMove);
            }
           GL.PushMatrix();

           GL.MultMatrix(ref _terrainArcball.CurrentRotateMatrix);
            foreach (IObject obj in _worldScene)
            {
                obj.Draw();
            }
            GL.PopMatrix();
            GL.PushMatrix();
            GL.Translate(_rightKey - _leftKey, _upKey - _downKey - _yMove, _delta + _zMove);

            GL.MultMatrix(ref _terrainArcball.CurrentRotateMatrix);
            foreach (IObject obj in _worldObjects)
            {
                obj.Draw();
            }
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
        }

        private static float VectorDistance(ref Vector3 vec1, ref Vector3 vec2)
        {
            return (float)
            Math.Sqrt((vec2.X - vec1.X) * (vec2.X - vec1.X)
                      + (vec2.Y - vec1.Y) * (vec2.Y - vec1.Y)
                      + (vec2.Z - vec1.Z) * (vec2.Z - vec1.Z));
        }

        //Transform orientation in Earth to position to draw terrain
        Vector3 GetTerrainPosition(Orientation orient)
        {
            Vector3 vec, winVec, winLocation;
            Earth.GetPosition(_earth.Radius, ref orient, out vec);
            Vector3 vec1 = Vector3.Transform(vec, FloatToMatrix4(_earthViewMatrix));// + "  " + newVec.ToString() 

            Glu.Project(vec1, _viewmodelMatrix, _projectionMatrix, _viewportMatrix, out winVec);
            winVec.Z = 0.99f; //Depth in projection view
            Glu.UnProject(winVec, _viewmodelMatrix, _terrainProjectionMatrix, _viewportMatrix, out winLocation);
            return winLocation;
        }

        private bool WithinEastSea()
        {
            Orientation orientation;
            LocationFromScreen(glControl1.Width / 2, glControl1.Height / 2, out orientation);
            if (orientation.latitude >= 30 && orientation.latitude <= 32
                && orientation.longitude >= 121 && orientation.longitude <= 123)
            {
                return true;
            }
            return false;
        }

        #region GLControl Event Handler
        private void GlControl1Load(object sender, EventArgs e)
        {
            if (!_contextLoaded) return;
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.DepthTest);
            GL.ClearDepth(1.0f);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            foreach (IObject obj in _sceneHelper)
            {
                obj.Initialize();
            }

            SetupViewport();

            Application.Idle += ApplicationIdle;
            _stopWatch.Start();

        }

        private void GlControl1Paint(object sender, PaintEventArgs e)
        {
            if (!_contextLoaded) return;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            DrawScene();
            glControl1.SwapBuffers();
        }
        private void GlControl1KeyDown(object sender, KeyEventArgs e)
        {
            if (!_contextLoaded) return;
            if (_currentView == CurrentSceneView.EarthView)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        _leftKey++;
                        break;
                    case Keys.D:
                        _rightKey++;
                        break;
                    case Keys.W:
                        _upKey++;
                        break;
                    case Keys.S:
                        _downKey++;
                        break;

                }
            }
            if (e.KeyCode == Keys.Space)
            {
                _left++;
                glControl1.Invalidate();
            }
        }

        private void GlControl1Resize(object sender, EventArgs e)
        {
            SetupViewport();
            glControl1.Invalidate();
        }

        private void GlControl1MouseDown(object sender, MouseEventArgs e)
        {
            if (!_contextLoaded) return;
            //Vector4 vectorIn = new Vector4(new Vector2(e.X, e.Y));
            //Vector4 vectorOut = new Vector4();
            //UnProject(ref vectorIn, FloatToMatrix4(earthModelview), FloatToMatrix4(projectionview), viewportview,
            //          ref vectorOut);
            if (_currentView == CurrentSceneView.UniversalView)
            {
                if (!Hemishere(e.X, e.Y, _curPos))
                {
                    _clickOutOfSphere = true;
                }
                _lastPos[0] = _curPos[0];
                _lastPos[1] = _curPos[1];
                _lastPos[2] = _curPos[2];
            }
            else if (_currentView == CurrentSceneView.EarthView)
            {
                _terrainArcball.MouseDown(e.Location);
            }
        }


        private void GlControl1MouseMove(object sender, MouseEventArgs e)
        {
            if (!_contextLoaded) return;
            if (_currentView == CurrentSceneView.UniversalView)
            {
                if (_earthLoaded)
                {
                    Orientation orientation;
                    LocationFromScreen(e.X, e.Y, out orientation);
                }

                if (MouseButtons.Left == e.Button)
                {
                    //Vector4 vectorIn = new Vector4(new Vector2(e.X, e.Y));
                    //Vector4 vectorOut=new Vector4();
                    //UnProject(ref vectorIn, FloatToMatrix4(earthModelview), FloatToMatrix4(projectionview), viewportview,
                    //          ref vectorOut);
                    Motion(e.X, e.Y);
                    glControl1.Invalidate();
                }
            }
            else if (_currentView == CurrentSceneView.EarthView)
            {
                if (MouseButtons.Left == e.Button)
                {
                    int width = glControl1.Width;
                    int height = glControl1.Height;
                    _terrainArcball.MouseMoveRotation(e.Location);
                }
            }
        }

        private void GlControl1MouseUp(object sender, MouseEventArgs e)
        {
            if (!_contextLoaded) return;
            if (_currentView == CurrentSceneView.UniversalView)
            {
                if (_earthLoaded)
                {
                    Orientation orientation;
                    LocationFromScreen(e.X, e.Y, out orientation);
                }

                if (MouseButtons.Left == e.Button)
                {
                    //Vector4 vectorIn = new Vector4(new Vector2(e.X, e.Y));
                    //Vector4 vectorOut=new Vector4();
                    //UnProject(ref vectorIn, FloatToMatrix4(earthModelview), FloatToMatrix4(projectionview), viewportview,
                    //          ref vectorOut);
                    Motion(e.X, e.Y);
                    glControl1.Invalidate();
                }
            }
            else if (_currentView == CurrentSceneView.EarthView)
            {
                if (MouseButtons.Left == e.Button)
                {
                    int width = glControl1.Width;
                    int height = glControl1.Height;
                    //_boundingBox.MouseMove(e.Location);
                }
            }
        }
        private void GlControl1MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //if (currentView == CurrentSceneView.UniversalView)
            {
                _scale += 10.0f;
            }
            _scale = (_scale > ChangeLevel) ? ChangeLevel : _scale;
        }

        void GlControl1MouseWheel(object sender, MouseEventArgs e)
        {
            //if(currentView==CurrentSceneView.UniversalView)
            {
                _scale += e.Delta / 100.0f;
                _scale = (_scale < 0.24f) ? 0.12f : _scale;
            }

            _scale = (_scale > ChangeLevel) ? ChangeLevel : _scale;
            if (_currentView == CurrentSceneView.EarthView)
            {
                _delta += e.Delta / 12;
            }
        }

        #endregion

        #region Arcball

        // 旋转角度
        float _theta;

        // 旋转轴
        readonly float[] _axis = { 1.0f, 0.0f, 0.0f };

        // 鼠标上次和当前坐标（映射到单位半球面）
        readonly float[] _lastPos = { 0.0f, 0.0f, 0.0f };
        readonly float[] _curPos = { 0.0f, 0.0f, 0.0f };

        // 上一次转换矩阵
        readonly float[] _lastMatrix =
		{
			1.0f, 0.0f, 0.0f, 0.0f,
			0.0f, 1.0f, 0.0f, 0.0f,
			0.0f, 0.0f, 1.0f, 0.0f,
			0.0f, 0.0f, 0.0f, 1.0f
		};

        private readonly float[] _earthViewMatrix = new float[16];

        void Motion(int x, int y)
        {
            // 计算当前的鼠标单位半球面坐标
            if (!Hemishere(x, y, _curPos))
            {
                return;
            }
            if (_clickOutOfSphere)
            {
                _lastPos[0] = _curPos[0];
                _lastPos[1] = _curPos[1];
                _lastPos[2] = _curPos[2];
                _clickOutOfSphere = false;
            }
            // 计算移动量的三个方向分量
            float dx = _curPos[0] - _lastPos[0];
            float dy = _curPos[1] - _lastPos[1];
            float dz = _curPos[2] - _lastPos[2];
            // 如果有移动
            if ((0.0f != dx) || (0.0f != dy) || (0.0f != dz))
            {
                // 计算移动距离，用来近似移动的球面距离
                var d = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
                // 通过移动距离计算移动的角度
                _theta = d * 60.0f;
                // 计算移动平面的法向量，即：lastPos × curPos
                _axis[0] = _lastPos[1] * _curPos[2] - _lastPos[2] * _curPos[1];
                _axis[1] = _lastPos[2] * _curPos[0] - _lastPos[0] * _curPos[2];
                _axis[2] = _lastPos[0] * _curPos[1] - _lastPos[1] * _curPos[0];
                // 记录当前的鼠标单位半球面坐标
                _lastPos[0] = _curPos[0];
                _lastPos[1] = _curPos[1];
                _lastPos[2] = _curPos[2];
            }
        }

        bool LocationFromScreen(int x, int y, out Vector3 vectorOut)
        {
            var vectorIn = new Vector3(x, glControl1.Height - y, 0);
            Glu.UnProject(vectorIn, _viewmodelMatrix, _projectionMatrix, _viewportMatrix, out vectorOut);
            float radius = _earth.Radius;
            double sum = radius * radius - vectorOut.X * vectorOut.X - vectorOut.Y * vectorOut.Y;
            if (sum >= 0)
            {
                vectorOut.Z = (float)Math.Sqrt(sum);
                //Matrix4 earthVert = Matrix4.Invert(FloatToMatrix4(earthViewMatrix));
                //Vector3 newVec = Vector3.Transform(vectorOut, earthVert);
                //Orientation orient;
                //Earth.GetLocation(radius, ref newVec, out orient);
                //(helpers[0] as Fonts).Text = orient.ToString() + "  " + newVec.ToString() + "  " + vectorOut.ToString();

            }
            else
            {
                //(helpers[0] as Fonts).Text = vectorIn.ToString() + "  " + "Out of the sphere";
                return false;
            }
            vectorOut.X = vectorOut.X / radius;
            vectorOut.Y = vectorOut.Y / radius;
            vectorOut.Z = vectorOut.Z / radius;
            return true;
        }

        bool LocationFromScreen(int x, int y, out Orientation orient)
        {
            Vector3 vectorOut;//=new Vector3();
            orient = new Orientation();
            var vectorIn = new Vector3(x, glControl1.Height - y, 0);
            Glu.UnProject(vectorIn, _viewmodelMatrix, _projectionMatrix, _viewportMatrix, out vectorOut);
            float radius = _earth.Radius;
            double sum = radius * radius - vectorOut.X * vectorOut.X - vectorOut.Y * vectorOut.Y;
            if (sum >= 0)
            {
                vectorOut.Z = (float)Math.Sqrt(sum);
                Matrix4 earthVert = Matrix4.Invert(FloatToMatrix4(_earthViewMatrix));
                Vector3 newVec = Vector3.Transform(vectorOut, earthVert);

                Earth.GetLocation(radius, ref newVec, out orient);
                _fonts.Text = orient + "  " + vectorOut;

            }
            else
            {
                _fonts.Text = vectorIn + "  " + "Out of the sphere";
                return false;
            }
            //vectorOut.X = vectorOut.X / radius;
            //vectorOut.Y = vectorOut.Y / radius;
            //vectorOut.Z = vectorOut.Z / radius;
            return true;
        }
        bool Hemishere(int x, int y, float[] v)
        {
            Vector3 vectorOut;
            if (LocationFromScreen(x, y, out vectorOut))
            {

                v[0] = vectorOut.X;
                v[1] = vectorOut.Y;
                v[2] = vectorOut.Z;
                return true;
            }
            return false;
            //float z;
            // 计算x, y坐标
            //v[0] = (float)x * 2.0f - (float)d;
            //v[1] = (float)d - (float)y * 2.0f;
            // 计算z坐标
            //z = d * d - v[0] * v[0] - v[1] * v[1];
            //if (z < 0)
            //{
            // return false;
            //}
            //v[2] = (float)Math.Sqrt(z);
            //// 单位化
            //v[0] /= (float)d;
            //v[1] /= (float)d;
            //v[2] /= (float)d;
            //return true;
        }

        int GetSquareLength()
        {
            return this.Bounds.Width > this.Bounds.Height ? this.Bounds.Width : this.Bounds.Height;
        }
        #endregion

        #region Form Event Handler
        private void MainFormLoad(object sender, EventArgs e)
        {
            //MessageBox.Show(GL.GetString(StringName.Version));

            //Add here to avoid a bug of OpenTK that glControl.Load never triggerred
            _contextLoaded = true;
            GlControl1Load(sender, e);
            glControl1.MouseWheel += GlControl1MouseWheel;
        }

        private void MainFormClosed(object sender, EventArgs e)
        {
            if (_currentView == CurrentSceneView.EarthView)
            {
                foreach (IObject obj in _worldScene)
                {
                    obj.Destroy();
                }

                foreach (IObject obj in _worldObjects)
                {
                    obj.Destroy();
                }
            }
            else if (_currentView == CurrentSceneView.UniversalView)
            {

                foreach (IObject obj in _universeScene)
                {
                    obj.Destroy();
                }

                foreach (IObject obj in _universeObjects)
                {
                    obj.Destroy();
                }
            }


            foreach (IObject obj in _sceneHelper)
            {
                obj.Destroy();
            }

            glControl1.MouseWheel -= GlControl1MouseWheel;
        }

        #endregion

        #region Animation
        void RotateAnimation(float angle, Vector3 axisRotate, int milliseconds)
        {
            GL.Rotate(angle, axisRotate);
        }
        void ScaleAnimation(float scaleTo, int milliseconds)
        {

        }
        #endregion

        #region Helper Methods
        public bool UnProject(ref Vector4 vectorIn, Matrix4 modelMatrix, Matrix4 projMatrix,
                              float[] viewport, ref Vector4 vectorOut)
        {

            Matrix4 p = Matrix4.Mult(modelMatrix, projMatrix);
            Matrix4 finalMatrix = Matrix4.Invert(p);
            // Map x and y from window coordinates 
            vectorIn.X = (vectorIn.X - viewport[0]) / viewport[2];
            vectorIn.Y = (vectorIn.Y - viewport[1]) / viewport[3];
            // Map to range -1 to 1 
            vectorIn.X = vectorIn.X * 2.0f - 1.0f;
            vectorIn.Y = vectorIn.Y * 2.0f - 1.0f;
            vectorIn.Z = vectorIn.Z * 2.0f - 1.0f;
            vectorIn.W = 1.0f;
            vectorOut = Vector4.Transform(vectorIn, finalMatrix);

            if (vectorOut.W == 0.0f) return false;

            vectorOut.X /= vectorOut.W;
            vectorOut.Y /= vectorOut.W;
            vectorOut.Z /= vectorOut.W;

            return true;

        }
        public float[] Matrix4ToFloat(Matrix4 tempMatrix)
        {
            var temp = new float[16];
            temp[0] = tempMatrix.M11;
            temp[1] = tempMatrix.M12;
            temp[2] = tempMatrix.M13;
            temp[3] = tempMatrix.M14;
            temp[4] = tempMatrix.M21;
            temp[5] = tempMatrix.M22;
            temp[6] = tempMatrix.M23;
            temp[7] = tempMatrix.M24;
            temp[8] = tempMatrix.M31;
            temp[9] = tempMatrix.M32;
            temp[10] = tempMatrix.M33;
            temp[11] = tempMatrix.M34;
            temp[12] = tempMatrix.M41;
            temp[13] = tempMatrix.M42;
            temp[14] = tempMatrix.M43;
            temp[15] = tempMatrix.M44;
            return temp;
        }
        public Matrix4 FloatToMatrix4(float[] temp)
        {
            if (temp.Length != 16) return new Matrix4();
            return new Matrix4(
                temp[0], temp[1], temp[2], temp[3], temp[4],
                temp[5], temp[6], temp[7], temp[8], temp[9],
                temp[10], temp[11], temp[12], temp[13], temp[14],
                temp[15]);
        }
        #endregion

        public enum CurrentSceneView
        {
            UniversalView, //Stars and the earth
            EarthView, //the earth and clouds
            TravelView, //Along the land
            ScientificView, //With coordinates and data
        }
    }
}
