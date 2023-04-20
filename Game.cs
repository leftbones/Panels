using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Panels;

class Game {
    public Vector2i WindowSize { get; private set; }
    public Vector2i FieldSize { get; private set; }

    public int PanelSize { get; private set; }

    public Vector2i CursorPos { get; private set; }
    public Vector2i CursorLeft { get { return CursorPos; } }
    public Vector2i CursorRight { get { return new Vector2i(CursorPos.X + 1, CursorPos.Y); } }
    
    public Panel PanelLeft { get { return Panels[CursorLeft.X, CursorLeft.Y]; } set { Panels[CursorLeft.X, CursorLeft.Y] = value; } }
    public Panel PanelRight { get { return Panels[CursorRight.X, CursorRight.Y]; } set { Panels[CursorRight.X, CursorRight.Y] = value; } }

    public Vector2i WindowCenter { get { return WindowSize / 2; } }
    public Vector2i FieldOrigin { get { return WindowCenter - ((FieldSize * PanelSize) / 2); } }

    private float TimerIncrement = 0.4f;
    private float TimerProgress = 0.0f;

    public Panel[,] Panels;

    private Random RNG = new Random();

    private Color[] PanelColors = new Color[] {
        new Color(239, 71, 111, 255),       // Red
        new Color(7, 197, 102, 255),        // Green
        new Color(1, 151, 244, 255),        // Blue
        new Color(255, 206, 92, 255),       // Yellow
        new Color(159, 89, 197, 255)        // Purple
    };

    private Dictionary<string, KeyboardKey> KeyBindings = new Dictionary<string, KeyboardKey>() {
        { "CursorUp", KeyboardKey.KEY_W },
        { "CursorDown", KeyboardKey.KEY_S },
        { "CursorLeft", KeyboardKey.KEY_A },
        { "CursorRight", KeyboardKey.KEY_D },
        { "SwapPanels", KeyboardKey.KEY_SPACE },
    };

    public Game(Vector2i window_size) {
        WindowSize = window_size;
        FieldSize = new Vector2i(6, 12);
        CursorPos = new Vector2i(FieldSize.X / 2 - 1, FieldSize.Y / 2 - 1);
        PanelSize = 32;

        Panels = new Panel[FieldSize.X, FieldSize.Y];
        for (int x = 0; x < FieldSize.X; x++) {
            for (int y = 0; y < FieldSize.Y; y++) {
                int type = y > FieldSize.Y / 2 ? RNG.Next(PanelColors.Length) : -1;
                NewPanel(x, y, type);
            }
        }
    }

    public void Update() {
        // Handle Input
        Input();

        // Progress Timer
        TimerProgress += TimerIncrement;
        if (TimerProgress >= 100.0f) {
            while (true) {
                int PX = RNG.Next(FieldSize.X);
                if (InBounds(PX, 0) && InBounds(PX + 1, 0)) {
                    NewPanel(PX, 0, RNG.Next(PanelColors.Length));
                    NewPanel(PX + 1, 0, RNG.Next(PanelColors.Length));
                    break;
                }
            }
            TimerProgress = 0.0f;
        }

        // Destroy Matches
        for (int y = 0; y < FieldSize.Y; y++) {
            for (int x = 0; x < FieldSize.X; x++) {
                Panel P = Panels[x, y];

                if (P.Destroy)
                    NewPanel(x, y, -1);
            }
        }

        // Update Panels
        for (int y = 0; y < FieldSize.Y; y++) {
            for (int x = 0; x < FieldSize.X; x++) {
                ////
                // Get Panel
                Panel P = Panels[x, y];

                ////
                // Drop Panels
                if (!P.Matched && !P.Moving && y < FieldSize.Y - 1 && Panels[x, y+1].Type == -1)
                    DropPanel(x, y);

                ////
                // Match Vertical
                List<Panel> Matched = new List<Panel>() { P };

                if (IsMatch(x, y-1, P.Type)) {
                    Matched.Add(Panels[x, y-1]);
                    if (IsMatch(x, y-2, P.Type))
                        Matched.Add(Panels[x, y-2]);
                }

                if (IsMatch(x, y+1, P.Type)) {
                    Matched.Add(Panels[x, y+1]);
                    if (IsMatch(x, y+2, P.Type))
                        Matched.Add(Panels[x, y+2]);
                }

                if (Matched.Count >= 3) {
                    foreach (Panel M in Matched)
                        M.Matched = true;
                }

                ////
                // Match Horizontal
                Matched = new List<Panel>() { P };

                if (IsMatch(x-1, y, P.Type)) {
                    Matched.Add(Panels[x-1, y]);
                    if (IsMatch(x-2, y, P.Type))
                        Matched.Add(Panels[x-2, y]);
                }

                if (IsMatch(x+1, y, P.Type)) {
                    Matched.Add(Panels[x+1, y]);
                    if (IsMatch(x+2, y, P.Type))
                        Matched.Add(Panels[x+2, y]);
                }

                if (Matched.Count >= 3) {
                    foreach (Panel M in Matched)
                        M.Matched = true;
                }

                ////
                // Update Panel
                P.Update();
            }
        }
    }

    public void Draw() {
        // Draw Timer
        DrawRectangleLines(FieldOrigin.X - 3, FieldOrigin.Y - 30, FieldSize.X * PanelSize + 6, 16, Color.WHITE);
        DrawRectangleLines(FieldOrigin.X - 4, FieldOrigin.Y - 31, FieldSize.X * PanelSize + 8, 18, Color.WHITE);

        int Length = (FieldSize.X * PanelSize + 2) * Convert.ToInt32(TimerProgress) / 100;
        DrawRectangle(FieldOrigin.X - 1, FieldOrigin.Y - 28, Length, 12, Color.WHITE);

        // Draw Field
        DrawRectangleLines(FieldOrigin.X - 3, FieldOrigin.Y - 3, FieldSize.X * PanelSize + 6, FieldSize.Y * PanelSize + 6, Color.WHITE);
        DrawRectangleLines(FieldOrigin.X - 4, FieldOrigin.Y - 4, FieldSize.X * PanelSize + 8, FieldSize.Y * PanelSize + 8, Color.WHITE);

        // Draw Background
        DrawRectangle(FieldOrigin.X, FieldOrigin.Y, FieldSize.X * PanelSize, FieldSize.Y * PanelSize, new Color(30, 30, 30, 255));

        // Draw Panels
        foreach (Panel P in Panels) {
            if (P.Type == -1)
                continue;

            Color Base = PanelColors[P.Type];
            Color Shadow = new Color(
                (byte)Math.Clamp(Base.r - 50, 0, 255),
                (byte)Math.Clamp(Base.g - 50, 0, 255),
                (byte)Math.Clamp(Base.b - 50, 0, 255),
                (byte)Base.a
            );

            DrawRectangle(P.ScreenPos.X + 1, P.ScreenPos.Y + 1, PanelSize - 2, PanelSize - 2, Base);
            DrawRectangle(P.ScreenPos.X + 2, P.ScreenPos.Y + 2, PanelSize - 4, PanelSize - 4, Shadow);
        }

        // Draw Cursor
        DrawRectangleLines(FieldOrigin.X + CursorPos.X * PanelSize + 1, FieldOrigin.Y + CursorPos.Y * PanelSize + 1, PanelSize * 2, PanelSize, Color.BLACK);
        DrawRectangleLines(FieldOrigin.X + CursorPos.X * PanelSize, FieldOrigin.Y + CursorPos.Y * PanelSize, PanelSize * 2 + 2, PanelSize + 2, Color.BLACK);

        DrawRectangleLines(FieldOrigin.X + CursorPos.X * PanelSize - 1, FieldOrigin.Y + CursorPos.Y * PanelSize - 1, PanelSize * 2 + 2, PanelSize + 2, Color.WHITE);
        DrawRectangleLines(FieldOrigin.X + CursorPos.X * PanelSize, FieldOrigin.Y + CursorPos.Y * PanelSize, PanelSize * 2, PanelSize, Color.WHITE);
    }

    public void Input() {
        if (IsKeyPressed(KeyBindings["SwapPanels"])) SwapPanels();
        else if (IsKeyPressed(KeyBindings["CursorUp"])) MoveCursor(0, -1);
        else if (IsKeyPressed(KeyBindings["CursorDown"])) MoveCursor(0, 1);
        else if (IsKeyPressed(KeyBindings["CursorLeft"])) MoveCursor(-1, 0);
        else if (IsKeyPressed(KeyBindings["CursorRight"])) MoveCursor(1, 0);
    }

    public bool NewPanel(int x, int y, int type) {
        if (!InBounds(x, y))
            return false;
        
        Panels[x, y] = new Panel(new Vector2i(FieldOrigin.X + (x * PanelSize), FieldOrigin.Y + (y * PanelSize)), type);
        return true;
    }

    public void SwapPanels() {
        Panel PL = PanelLeft;
        Panel PR = PanelRight;
        PL.SwapTo(1, 0);
        PR.SwapTo(-1, 0);
        PanelLeft = PR;
        PanelRight = PL;
    }

    public void DropPanel(int x, int y) {
        Panel PU = Panels[x, y];
        Panel PD = Panels[x, y+1];
        PU.SwapTo(0, 1);
        PD.SwapTo(0, -1);
        Panels[x, y] = PD;
        Panels[x, y+1] = PU;
    }

    public bool IsMatch(int x, int y, int type) {
        if (!InBounds(x, y))
            return false;

        Panel P = Panels[x, y];
        if (!P.Moving && P.Type == type)
            return true;

        return false;
    }

    public void MoveCursor(int x, int y) {
        int DX = CursorPos.X + x;
        int DY = CursorPos.Y + y;

        if (DX < 0 || DX > FieldSize.X - 2 || DY < 0 || DY > FieldSize.Y - 1)
            return;

        CursorPos = new Vector2i(DX, DY);
    }

    public bool InBounds(int x, int y) {
        return x >= 0 && x < FieldSize.X && y >= 0 && y < FieldSize.Y;
    }
}