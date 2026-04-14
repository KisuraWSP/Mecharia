using System.Runtime.InteropServices.JavaScript;
using Raylib_cs;
using RayGUI;

namespace RaylibWasm
{
    public partial class Application
    {
        /// <summary>
        /// Application entry point
        /// </summary>
        public static void Main()
        {
            Raylib.InitWindow(512, 512, "RaylibWasm");
            Raylib.SetTargetFPS(60);
        }

        /// <summary>
        /// Updates frame
        /// </summary>
        [JSExport]
        public static void UpdateFrame()
        {
            Raylib.BeginDrawing();

            Raylib.ClearBackground(Color.White);


            Raylib.EndDrawing();
        }
    }
}
