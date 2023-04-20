using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Panels;

class Program {
    static void Main(string[] args) {
        ////
        // Setup
        Vector2i WindowSize = new Vector2i(800, 600);
        InitWindow(WindowSize.X, WindowSize.Y, "Panels");
        SetTargetFPS(60);

        var Game = new Game(WindowSize);

        ////
        // Game Loop
        while (!WindowShouldClose()) {
            ////
            // Update
            Game.Update();

            ////
            // Draw
            BeginDrawing();
            ClearBackground(new Color(40, 40, 40, 255));

            Game.Draw();

            EndDrawing();
        }

        ////
        // Exit
        CloseWindow();
    }
}
