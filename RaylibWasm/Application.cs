using System.Runtime.InteropServices.JavaScript;
using Raylib_cs;
using RayGUI;
using lib.core;

namespace RaylibWasm
{
    public partial class Application
    {
        /// <summary>
        /// Application entry point
        /// </summary>
        public static void Main()
        {
            Raylib.InitWindow(Game.width, Game.height, Game.title);
            Raylib.SetTargetFPS(60);
            Game.Init();
        }

        /// <summary>
        /// Updates frame
        /// </summary>
        [JSExport]
        public static void UpdateFrame()
        {
            Game.Update();
            Game.Draw();
        }
    }
}
