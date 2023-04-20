using Raylib_cs;

namespace Panels;

class Panel {
    public Vector2i ScreenPos { get; private set; }
    public Vector2i TargetPos { get; private set; }
    public int Type { get; set; }

    public bool Moving { get { return ScreenPos != TargetPos; } }
    public bool Matched { get; set; } = false;
    public bool Destroy { get; set; } = false;

    private int DestroyDelay = 5;

    public Panel(Vector2i screen_pos, int type) {
        ScreenPos = screen_pos;
        TargetPos = screen_pos;
        Type = type;
    }

    public void Update() {
        if (Moving) {
            int MX = 0;
            int MY = 0;

            if (ScreenPos.X < TargetPos.X) MX = 4;
            if (ScreenPos.X > TargetPos.X) MX = -4;
            if (ScreenPos.Y < TargetPos.Y) MY = 4;
            if (ScreenPos.Y > TargetPos.Y) MY = -4;

            Move(MX, MY);
        } else if (Matched) {
            if (DestroyDelay > 0)
                DestroyDelay--;
            else
                Destroy = true;
        }
    }

    public void SwapTo(int x, int y) {
        TargetPos = new Vector2i(TargetPos.X  + (x * 32), TargetPos.Y + (y * 32));
    }

    public void Move(int x, int y) {
        ScreenPos = new Vector2i(ScreenPos.X + x, ScreenPos.Y + y);
    }
}