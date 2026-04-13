using System.Runtime.InteropServices.JavaScript;
using Raylib_cs;
using RayGUI;

namespace RaylibWasm
{
    public partial class Application
    {
        public static float x = 0.5f;
        private static Texture2D logo;
        
        /// <summary>
        /// Application entry point
        /// </summary>
        public static void Main()
        {
            Raylib.InitWindow(512, 512, "RaylibWasm");
            Raylib.SetTargetFPS(60);
            
            logo = Raylib.LoadTexture("Resources/raylib_logo.png");
        }

        /// <summary>
        /// Updates frame
        /// </summary>
        [JSExport]
        public static void UpdateFrame()
        {
            Raylib.BeginDrawing();

            Raylib.ClearBackground(Color.White);

            Raylib.DrawFPS(4, 4);
            Raylib.DrawText("All systems operational!", 4, 32, 20, Color.Maroon);
            RayGui.GuiButton(new(100, 100, 100, 100), "hallo");
            
            RayGui.GuiSlider(new(100, 200, 100, 100), "min", "max", ref x, 0, 1); 
            
            Raylib.DrawTexture(logo, 4, 64, Color.White);

            Raylib.EndDrawing();
        }
    }
}
