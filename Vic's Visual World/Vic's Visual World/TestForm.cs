using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using WorldFramework.Scene;
using ClearBufferMask = OpenTK.Graphics.OpenGL.ClearBufferMask;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using WorldFramework.Controller;
using EnableCap = OpenTK.Graphics.OpenGL.EnableCap;
using GL = OpenTK.Graphics.OpenGL.GL;
using MatrixMode = OpenTK.Graphics.OpenGL.MatrixMode;

namespace VirtualWorld
{
    public partial class TestForm : Form
    {
        Coordinates scene = new Coordinates(200);
        //private SkyBox scene = new SkyBox(100, 100, 100, 0, 0, 0);
        private bool _contextLoaded;
        public TestForm()
        {
            InitializeComponent();
            this.Load += new EventHandler(TestForm_Load);
            glControl1.Load += new EventHandler(glControl1_Load);
            glControl1.Paint += new PaintEventHandler(glControl1_Paint);
            glControl1.Resize += new EventHandler(glControl1_Resize);
        }

        void TestForm_Load(object sender, EventArgs e)
        {
            _contextLoaded = true;
            glControl1_Load(sender, e);
            glControl1.Invalidate();
        }

        void glControl1_Resize(object sender, EventArgs e)
        {
            if (!_contextLoaded) return;
            SetupViewport();
            glControl1.Invalidate();
        }

        void SetupViewport()
        {

            int width = glControl1.Width;
            int height = glControl1.Height;
            float aspectRatio = width / (float)height;

            GL.Viewport(0, 0, width, height);
            Matrix4 perpective;

            perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 1000);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perpective);
            Glu.LookAt(0.0, 0.0, 100.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0);
        }
        void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (!_contextLoaded) return;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Draw();
            glControl1.SwapBuffers();
        }

        void glControl1_Load(object sender, EventArgs e)
        {
            if (!_contextLoaded) return;
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.DepthTest);
            GL.ClearDepth(1.0f);

            scene.Initialize();
            SetupViewport();
            Application.Idle += new EventHandler(Application_Idle);
        }

        void Application_Idle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                Draw();
                glControl1.Invalidate();
            }
        }
        void Draw()
        {
            scene.Draw();
        }
    }
}
