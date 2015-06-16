using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Platform;
using OpenTK.Platform.Android;
using Android.Views;
using Android.Content;
using Android.Util;

namespace Texample
{
    class TexampleView : AndroidGameView
    {
        private GLText glText;                             // A GLText Instance

        public TexampleView(Context context) : base(context) {
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);

            GL.Viewport(0, 0, this.Width, this.Height);

            // Setup orthographic projection
            GL.MatrixMode(All.Projection);          // Activate Projection Matrix
            GL.LoadIdentity();                            // Load Identity Matrix
            GL.Ortho(                                    // Set Ortho Projection (Left,Right,Bottom,Top,Front,Back)
               0, this.Width,
               0, this.Height,
               1.0f, -1.0f
            );
        }

        // This gets called when the drawing surface is ready
        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            // Set the background frame color
            GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);

            // Create the GLText
            glText = new GLText(Context.Assets);

            // Load the font from file (set size + padding), creates the texture
            // NOTE: after a successful call to this the font is ready for rendering!
            glText.load("Roboto-Regular.ttf", 14, 2, 2);  // Create Font (Height: 14 Pixels / X+Y Padding 2 Pixels)

            // Run the render loop
            Run();
        }

        // This method is called everytime the context needs
        // to be recreated. Use it to set any egl-specific settings
        // prior to context creation
        //
        // In this particular case, we demonstrate how to set
        // the graphics mode and fallback in case the device doesn't
        // support the defaults
        protected override void CreateFrameBuffer() {
            // the default GraphicsMode that is set consists of (16, 16, 0, 0, 2, false)
            try {
                Log.Verbose("TexampleView", "Loading with default settings");

                // if you don't call this, the context won't be created
                base.CreateFrameBuffer();
                return;
            } catch (Exception ex) {
                Log.Verbose("GLCube", "{0}", ex);
            }

            // this is a graphics setting that sets everything to the lowest mode possible so
            // the device returns a reliable graphics setting.
            try {
                Log.Verbose("TexampleView", "Loading with custom Android settings (low mode)");
                GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false);

                // if you don't call this, the context won't be created
                base.CreateFrameBuffer();
                return;
            } catch (Exception ex) {
                Log.Verbose("TexampleView", "{0}", ex);
            }
            throw new Exception("Can't load egl, aborting");
        }

        // This gets called on each frame render
        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            // Redraw background color
            GL.Clear((int)All.ColorBufferBit);

            // Set to ModelView mode
            GL.MatrixMode(All.Modelview);           // Activate Model View Matrix
            GL.LoadIdentity();                            // Load Identity Matrix

            // enable texture + alpha blending
            // NOTE: this is required for text rendering! we could incorporate it into
            // the GLText class, but then it would be called multiple times (which impacts performance).
            GL.Enable(All.Texture2D);              // Enable Texture Mapping
            GL.Enable(All.Blend);                   // Enable Alpha Blend
            GL.BlendFunc(All.SrcAlpha, All.OneMinusSrcAlpha);  // Set Alpha Blend Function

            // TEST: render the entire font texture
            GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);         // Set Color to Use
            glText.drawTexture(this.Width, this.Height);            // Draw the Entire Texture

            // TEST: render some strings with the font
            glText.begin(1.0f, 1.0f, 1.0f, 1.0f);         // Begin Text Rendering (Set Color WHITE)
            glText.draw("Test String :)", 0, 0);          // Draw Test String
            glText.draw("Line 1", 50, 50);                // Draw Test String
            glText.draw("Line 2", 100, 100);              // Draw Test String
            glText.end();                                   // End Text Rendering

            glText.begin(0.0f, 0.0f, 1.0f, 1.0f);         // Begin Text Rendering (Set Color BLUE)
            glText.draw("More Lines...", 50, 150);        // Draw Test String
            glText.draw("The End.", 50, 150 + glText.getCharHeight());  // Draw Test String
            glText.end();                                   // End Text Rendering

            // disable texture + alpha
            GL.Disable(All.Blend);                  // Disable Alpha Blend
            GL.Disable(All.Texture2D);             // Disable Texture Mapping

            SwapBuffers();
        }
    }
}
