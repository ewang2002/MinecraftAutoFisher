using PInvoke;
using static PInvoke.User32;

namespace AutoFisher.Windows;

public static class MouseOperations
{
    public static POINT GetCursorPosition()
    {
        var gotPoint = GetCursorPos(out var currentMousePoint);
        if (!gotPoint)
            currentMousePoint = new POINT { x = 0, y = 0 };
        return currentMousePoint;
    }

    private static readonly MOUSEEVENTF InvalidMouseEventMask = MOUSEEVENTF.MOUSEEVENTF_WHEEL
                                                                | MOUSEEVENTF.MOUSEEVENTF_XDOWN
                                                                | MOUSEEVENTF.MOUSEEVENTF_XUP;

    public static unsafe void SendMouseEvent(MOUSEEVENTF value)
    {
        if ((value & InvalidMouseEventMask) != 0) return;

        var position = GetCursorPosition();
        var inputArray = new INPUT[]
        {
            new()
            {
                type = InputType.INPUT_MOUSE,
                Inputs = new INPUT.InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dx = position.x,
                        dy = position.y,
                        mouseData = 0,
                        dwFlags = value
                    }
                }
            }
        };

        SendInput(inputArray.Length, inputArray, sizeof(INPUT));
    }
}