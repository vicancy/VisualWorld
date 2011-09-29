using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Tao.OpenGl;
using Tao.Platform.Windows;
using TheWorldForm;
using WorldFramework.Controller;
using WorldFramework.Interface;

namespace WorldFramework
{
    public partial class TheWorld : UserControl,IObject
    {

        #region Fields
        private static  String Title = "NeHe's ";
        private static  IntPtr hDC;                                              // Private GDI Device Context
        private static  IntPtr hRC;                                              // Permanent Rendering Context
        private static Form1 form;                                               // Our Current Windows Form
        private static PictureBox pc1;
        private static  bool[] keys = new bool[256];                             // Array Used For The Keyboard Routine
        private static  bool active = true;                                      // Window Active Flag, Set To True By Default
        private static  bool fullscreen = true;
        private static Button button1;                                  // Fullscreen Flag, Set To Fullscreen Mode By Default
        private static  bool done = false;                                       // Bool Variable To Exit Main Loop
        private static  int bordwidth = 100;
        private static  int bordheight = 100;
        private static  string FontInput = "Input Here";
        private static  int filter = 2;
        private static  int[] texture = new int[6];  //texture[0]: font texture; texture[1]: bump texture
        private static  string[] sceneTextureFilename = {"NeHe.Lesson20.Logo.bmp",
                                          "NeHe.Lesson20.Mask1.bmp",
                                          "NeHe.Lesson20.Image1.bmp",
                                          "NeHe.Lesson20.Mask2.bmp",
                                          "NeHe.Lesson20.Image2.bmp"};
        /////////////////////////////////////////

        //////////////blend
        private static bool blend;  //混合
        private static bool bp = false; //'B' pressed?
        //////// //////////////////////////
                    
        private static  Fonts fts;
        //private static RawHeightMapDraw rmd;
        ////////////////////////////////
        ////////////////////////////////

        private PictureBox pictureBox;
        #endregion
        public EventHandler Activated, Closing, Deactivate;

        public TheWorld()
        {
            InitializeComponent();

            this.CreateParams.ClassStyle = this.CreateParams.ClassStyle | // Redraw On Size, And Own DC For Window.
                                           User.CS_HREDRAW | User.CS_VREDRAW | User.CS_OWNDC;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true); // No Need To Erase Form Background
            this.SetStyle(ControlStyles.DoubleBuffer, true); // Buffer Control
            this.SetStyle(ControlStyles.Opaque, true); // No Need To Draw Form Background
            this.SetStyle(ControlStyles.ResizeRedraw, true); // Redraw On Resize
            this.SetStyle(ControlStyles.UserPaint, true); // We'll Handle Painting Ourselves

            this.Activated += new EventHandler(this.Form_Activated);            // On Activate Event Call Form_Activated
            this.Deactivate += new EventHandler(this.Form_Deactivate);          // On Deactivate Event Call Form_Deactivate
            this.KeyDown += new KeyEventHandler(this.Form_KeyDown);             // On KeyDown Event Call Form_KeyDown
            this.KeyUp += new KeyEventHandler(this.Form_KeyUp);                 // On KeyUp Event Call Form_KeyUp
            this.Resize += new EventHandler(this.Form_Resize);                  // On Resize Event Call Form_Resize

        }

        public void Initialize()
        {
            CreateGLWindow("", this.Width, this.Height, 32, false);
           
        }
        public void Draw()
        {
            DrawGLScene();                                          // Draw The Scene
            Gdi.SwapBuffers(hDC);                                   // Swap Buffers (Double Buffering)
        }
        public void Destroy()
        {
           
        }
        private  static  bool DrawGLScene()
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);        // Clear Screen And Depth Buffer
            Gl.glLoadIdentity();                                                // Reset The Current Modelview Matrix
            fts.glPaint(0, 0, FontInput, 0);
            //rmd.glDraw();
            return true;
        }

        #region bool InitGL()
        /// <summary>
        ///     All setup for OpenGL goes here.
        /// </summary>
        /// <returns>
        ///     <c>true</c> on successful initialization, otherwise <c>false</c>.
        /// </returns>
        private static  bool InitGL()
        {

            Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glShadeModel(Gl.GL_SMOOTH);                                      // Enable Smooth Shading
            Gl.glClearColor(0.0f, 0.0f, 0.0f, 0.0f);                                     // Black Background
            Gl.glClearDepth(1);                                                 // Depth Buffer Setup
            Gl.glEnable(Gl.GL_DEPTH_TEST);                                      // Enables Depth Testing
            Gl.glDepthFunc(Gl.GL_LEQUAL);                                       // The Type Of Depth Testing To Do
            Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_NICEST);         // Really Nice Perspective Calculations


            fts = new Fonts();
            //rmd = new RawHeightMapDraw();
            //BuildLists();
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            return true;
        }
        #endregion bool InitGL()

        #region bool CreateGLWindow(string title, int width, int height, int bits, bool fullscreenflag)
        /// <summary>
        ///     Creates our OpenGL Window.
        /// </summary>
        /// <param name="title">
        ///     The title to appear at the top of the window.
        /// </param>
        /// <param name="width">
        ///     The width of the GL window or fullscreen mode.
        /// </param>
        /// <param name="height">
        ///     The height of the GL window or fullscreen mode.
        /// </param>
        /// <param name="bits">
        ///     The number of bits to use for color (8/16/24/32).
        /// </param>
        /// <param name="fullscreenflag">
        ///     Use fullscreen mode (<c>true</c>) or windowed mode (<c>false</c>).
        /// </param>
        /// <returns>
        ///     <c>true</c> on successful window creation, otherwise <c>false</c>.
        /// </returns>
        private static bool CreateGLWindow(string title, int width, int height, int bits, bool fullscreenflag)
        {
                      int pixelFormat;                                                    // Holds The Results After Searching For A Match
            fullscreen = fullscreenflag;                                        // Set The Global Fullscreen Flag
            form = null;                                                        // Null The Form

            GC.Collect();                                                       // Request A Collection
            // This Forces A Swap
            Kernel.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);

            if (fullscreen)
            {                                                    // Attempt Fullscreen Mode?
                Gdi.DEVMODE dmScreenSettings = new Gdi.DEVMODE();               // Device Mode
                // Size Of The Devmode Structure
                dmScreenSettings.dmSize = (short)Marshal.SizeOf(dmScreenSettings);
                dmScreenSettings.dmPelsWidth = width;                           // Selected Screen Width
                dmScreenSettings.dmPelsHeight = height ;                         // Selected Screen Height
                dmScreenSettings.dmBitsPerPel = bits;                           // Selected Bits Per Pixel
                dmScreenSettings.dmFields = Gdi.DM_BITSPERPEL | Gdi.DM_PELSWIDTH | Gdi.DM_PELSHEIGHT;

                // Try To Set Selected Mode And Get Results.  NOTE: CDS_FULLSCREEN Gets Rid Of Start Bar.
                if (User.ChangeDisplaySettings(ref dmScreenSettings, User.CDS_FULLSCREEN) != User.DISP_CHANGE_SUCCESSFUL)
                {
                    // If The Mode Fails, Offer Two Options.  Quit Or Use Windowed Mode.
                    if (MessageBox.Show("The Requested Fullscreen Mode Is Not Supported By\nYour Video Card.  Use Windowed Mode Instead?", "NeHe GL",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    {
                        fullscreen = false;                                     // Windowed Mode Selected.  Fullscreen = false
                    }
                    else
                    {
                        // Pop up A Message Box Lessing User Know The Program Is Closing.
                        MessageBox.Show("Program Will Now Close.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return false;                                           // Return false
                    }
                }
            }

            form = new Form1();                                             // Create The Window
            
            pc1 = new PictureBox();
            form.Child = pc1;
            if (fullscreen)
            {                                                    // Are We Still In Fullscreen Mode?
                form.FormBorderStyle = FormBorderStyle.None;                    // No Border
                //Cursor.Hide();                                                  // Hide Mouse Pointer
               
            }
            else
            {                                                              // If Windowed
                form.FormBorderStyle = FormBorderStyle.Sizable;                 // Sizable
                Cursor.Show();                                                  // Show Mouse Pointer
            }

            form.Width = width+bordwidth ;                                                 // Set Window Width
            form.Height = height+bordheight ;                                               // Set Window Height
            
            //pc1.Location = new System.Drawing.Point(42, 42);
            
            //pc1.Width = width ;
            //pc1.Height =height  ;
            //pc1.Width = width-100;
            //pc1.Height = height-100 ;
            //form.Width = 100 + width;
            //form.Height = 100 + height;
            form.Text = title;                                                  // Set Window Title

            Gdi.PIXELFORMATDESCRIPTOR pfd = new Gdi.PIXELFORMATDESCRIPTOR();    // pfd Tells Windows How We Want Things To Be
            pfd.nSize = (short)Marshal.SizeOf(pfd);                            // Size Of This Pixel Format Descriptor
            pfd.nVersion = 1;                                                   // Version Number
            pfd.dwFlags = Gdi.PFD_DRAW_TO_WINDOW|                            // Format Must Support Window
                Gdi.PFD_SUPPORT_OPENGL |                                        // Format Must Support OpenGL
                Gdi.PFD_DOUBLEBUFFER;                                           // Format Must Support Double Buffering
            pfd.iPixelType = (byte)Gdi.PFD_TYPE_RGBA;                          // Request An RGBA Format
            pfd.cColorBits = (byte)bits;                                       // Select Our Color Depth
            pfd.cRedBits = 0;                                                   // Color Bits Ignored
            pfd.cRedShift = 0;
            pfd.cGreenBits = 0;
            pfd.cGreenShift = 0;
            pfd.cBlueBits = 0;
            pfd.cBlueShift = 0;
            pfd.cAlphaBits = 0;                                                 // No Alpha Buffer
            pfd.cAlphaShift = 0;                                                // Shift Bit Ignored
            pfd.cAccumBits = 0;                                                 // No Accumulation Buffer
            pfd.cAccumRedBits = 0;                                              // Accumulation Bits Ignored
            pfd.cAccumGreenBits = 0;
            pfd.cAccumBlueBits = 0;
            pfd.cAccumAlphaBits = 0;
            pfd.cDepthBits = 16;                                                // 16Bit Z-Buffer (Depth Buffer)
            pfd.cStencilBits = 0;                                               // No Stencil Buffer
            pfd.cAuxBuffers = 0;                                                // No Auxiliary Buffer
            pfd.iLayerType = (byte)Gdi.PFD_MAIN_PLANE;                         // Main Drawing Layer
            pfd.bReserved = 0;                                                  // Reserved
            pfd.dwLayerMask = 0;                                                // Layer Masks Ignored
            pfd.dwVisibleMask = 0;
            pfd.dwDamageMask = 0;
            hDC = User.GetDC(pc1.Handle);
            //hDC = User.GetDC(form.Handle);                                      // Attempt To Get A Device Context
            if (hDC == IntPtr.Zero)
            {                                            // Did We Get A Device Context?
                KillGLWindow();                                                 // Reset The Display
                MessageBox.Show("Can't Create A GL Device Context.", "ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            pixelFormat = Gdi.ChoosePixelFormat(hDC, ref pfd);                  // Attempt To Find An Appropriate Pixel Format
            if (pixelFormat == 0)
            {                                              // Did Windows Find A Matching Pixel Format?
                KillGLWindow();                                                 // Reset The Display
                MessageBox.Show("Can't Find A Suitable PixelFormat.", "ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!Gdi.SetPixelFormat(hDC, pixelFormat, ref pfd))
            {                // Are We Able To Set The Pixel Format?
                KillGLWindow();                                                 // Reset The Display
                MessageBox.Show("Can't Set The PixelFormat.", "ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            hRC = Wgl.wglCreateContext(hDC);                                    // Attempt To Get The Rendering Context
            if (hRC == IntPtr.Zero)
            {                                            // Are We Able To Get A Rendering Context?
                KillGLWindow();                                                 // Reset The Display
                MessageBox.Show("Can't Create A GL Rendering Context.", "ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!Wgl.wglMakeCurrent(hDC, hRC))
            {                                 // Try To Activate The Rendering Context
                KillGLWindow();                                                 // Reset The Display
                MessageBox.Show("Can't Activate The GL Rendering Context.", "ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            form.Show();                                                        // Show The Window
            
            form.TopMost = true;                                                // Topmost Window
            form.Focus();                                                       // Focus The Window
           // pc1.Focus();
            if (fullscreen)
            {                                                    // This Shouldn't Be Necessary, But Is
                Cursor.Hide();
            }
            ReSizeGLScene(width, height);                                       // Set Up Our Perspective GL Screen

            if (!InitGL())
            {                                                     // Initialize Our Newly Created GL Window
                KillGLWindow();                                                 // Reset The Display
                MessageBox.Show("Initialization Failed.", "ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;                                                        // Success
        }
        #endregion bool CreateGLWindow(string title, int width, int height, int bits, bool fullscreenflag)


        #region ReSizeGLScene(int width, int height)
        /// <summary>
        ///     Resizes and initializes the GL window.
        /// </summary>
        /// <param name="width">
        ///     The new window width.
        /// </param>
        /// <param name="height">
        ///     The new window height.
        /// </param>
        private static void ReSizeGLScene(int width, int height)
        {
            if (height == 0)
            {                                                   // Prevent A Divide By Zero...
                height = 1;                                                     // By Making Height Equal To One
            }

            Gl.glViewport(0, 0, width, height);                                 // Reset The Current Viewport
            Gl.glMatrixMode(Gl.GL_PROJECTION);                                  // Select The Projection Matrix
            Gl.glLoadIdentity();                                                // Reset The Projection Matrix

            Glu.gluPerspective(45, width / (double)height, 0.1, 1000);          // Calculate The Aspect Ratio Of The Window
            Gl.glMatrixMode(Gl.GL_MODELVIEW);                                   // Select The Modelview Matrix
            Gl.glLoadIdentity();                                                // Reset The Modelview Matrix
        }
        #endregion ReSizeGLScene(int width, int height)

        #region KillGLWindow()
        /// <summary>
        ///     Properly kill the window.
        /// </summary>
        private static void KillGLWindow()
        {

            if (fullscreen)
            {                                                    // Are We In Fullscreen Mode?
                User.ChangeDisplaySettings(IntPtr.Zero, 0);                     // If So, Switch Back To The Desktop
                Cursor.Show();                                                  // Show Mouse Pointer
            }

            if (hRC != IntPtr.Zero)
            {                                            // Do We Have A Rendering Context?
                if (!Wgl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero))
                {             // Are We Able To Release The DC and RC Contexts?
                    MessageBox.Show("Release Of DC And RC Failed.", "SHUTDOWN ERROR",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (!Wgl.wglDeleteContext(hRC))
                {                                // Are We Able To Delete The RC?
                    MessageBox.Show("Release Rendering Context Failed.", "SHUTDOWN ERROR",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                hRC = IntPtr.Zero;                                              // Set RC To Null
            }

            if (hDC != IntPtr.Zero)
            {                                            // Do We Have A Device Context?
                if (form != null && !form.IsDisposed)
                {                          // Do We Have A Window?
                    if (form.Handle != IntPtr.Zero)
                    {                            // Do We Have A Window Handle?
                        if (!User.ReleaseDC(form.Handle, hDC))
                        {                 // Are We Able To Release The DC?
                            MessageBox.Show("Release Device Context Failed.", "SHUTDOWN ERROR",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }

                hDC = IntPtr.Zero;                                              // Set DC To Null
            }

            if (form != null)
            {                                                  // Do We Have A Windows Form?
                form.Hide();                                                    // Hide The Window
                form.Close();                                                   // Close The Form
                form = null;                                                    // Set form To Null
            }
            fts.KillFont();

        }
        #endregion KillGLWindow()

        #region Form_Activated
        /// <summary>
        ///     Handles the form's activated event.
        /// </summary>
        /// <param name="sender">
        ///     The event sender.
        /// </param>
        /// <param name="e">
        ///     The event arguments.
        /// </param>
        private void Form_Activated(object sender, EventArgs e)
        {
            active = true;                                                      // Program Is Active
            // form.Focus();
        }
        #endregion Form_Activated

        #region Form_Deactivate(object sender, EventArgs e)
        /// <summary>
        ///     Handles the form's deactivate event.
        /// </summary>
        /// <param name="sender">
        ///     The event sender.
        /// </param>
        /// <param name="e">
        ///     The event arguments.
        /// </param>
        private void Form_Deactivate(object sender, EventArgs e)
        {
            active = false;                                                     // Program Is No Longer Active
        }
        #endregion Form_Deactivate(object sender, EventArgs e)

        #region Form_KeyDown(object sender, KeyEventArgs e)
        /// <summary>
        ///     Handles the form's key down event.
        /// </summary>
        /// <param name="sender">
        ///     The event sender.
        /// </param>
        /// <param name="e">
        ///     The event arguments.
        /// </param>
        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            keys[e.KeyValue] = true;                                            // Key Has Been Pressed, Mark It As true
        }
        #endregion Form_KeyDown(object sender, KeyEventArgs e)

        #region Form_KeyUp(object sender, KeyEventArgs e)
        /// <summary>
        ///     Handles the form's key down event.
        /// </summary>
        /// <param name="sender">
        ///     The event sender.
        /// </param>
        /// <param name="e">
        ///     The event arguments.
        /// </param>
        private void Form_KeyUp(object sender, KeyEventArgs e)
        {
            keys[e.KeyValue] = false;                                           // Key Has Been Released, Mark It As false
        }
        #endregion Form_KeyUp(object sender, KeyEventArgs e)

        #region Form_Resize(object sender, EventArgs e)
        /// <summary>
        ///     Handles the form's resize event.
        /// </summary>
        /// <param name="sender">
        ///     The event sender.
        /// </param>
        /// <param name="e">
        ///     The event arguments.
        /// </param>
        private void Form_Resize(object sender, EventArgs e)
        {
            if (pictureBox != null)
            {

                if (!fullscreen)
                {
                    pictureBox.Width = Width - bordwidth;
                    pictureBox.Height = Height - bordheight;
                }
                else
                {
                    pictureBox.Dock = DockStyle.Fill;
                }
                ReSizeGLScene(pictureBox.Width, pictureBox.Height);
            }
        }

        #endregion Form_Resize(object sender, EventArgs e)

        //获取方向键
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if ((keyData == Keys.Up) || (keyData == Keys.Down) || (keyData == Keys.Left) || (keyData == Keys.Right))
                return false;

            return base.ProcessDialogKey(keyData);
        }

        [STAThread]
        static void Main()
        {
            // Ask The User Which Screen Mode They Prefer
            if (MessageBox.Show("Would You Like To Run In Fullscreen Mode?", "Start FullScreen?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                fullscreen = false;                                             // Windowed Mode
            }

            // Create Our OpenGL Window
            if (!CreateGLWindow(Title, 800, 600, 16, fullscreen))
            {
                return;                                                         // Quit If Window Was Not Created
            }

            while (!done)
            {                                                      // Loop That Runs While done = false
                Application.DoEvents();                                         // Process Events

                if (active && (form != null))
                {                                  // Program Active?
                    if (keys[(int)Keys.Escape])
                    {                               // Was ESC Pressed?
                        done = true;                                            // ESC Signalled A Quit
                    }
                    else
                    {                                                      // Not Time To Quit, Update Screen
                        DrawGLScene();                                          // Draw The Scene

                        Gdi.SwapBuffers(hDC);                                   // Swap Buffers (Double Buffering)


                        if (keys[(int)Keys.B] && !bp)
                        {
                            bp = true;
                            blend = !blend;
                            if (blend)
                            {
                                Gl.glEnable(Gl.GL_BLEND);
                                Gl.glDisable(Gl.GL_DEPTH_TEST);

                            }
                            else
                            {
                                Gl.glDisable(Gl.GL_BLEND);
                                Gl.glEnable(Gl.GL_DEPTH_TEST);
                            }
                        }
                        if (!keys[(int)Keys.B])
                        {
                            bp = false;
                        }


                    }
                }

                if (keys[(int)Keys.F1])
                {                                       // Is F1 Being Pressed?
                    keys[(int)Keys.F1] = false;                                // If So Make Key false
                    KillGLWindow();                                             // Kill Our Current Window
                    fullscreen = !fullscreen;                                   // Toggle Fullscreen / Windowed Mode
                    // Recreate Our OpenGL Window
                    if (!CreateGLWindow(Title, 800, 600, 16, fullscreen))
                    {
                        return;                                                 // Quit If Window Was Not Created
                    }
                    done = false;                                               // We're Not Done Yet
                }
            }

            // Shutdown
            KillGLWindow();                                                     // Kill The Window
            return;                                                             // Exit The Program
        }
    }
}
