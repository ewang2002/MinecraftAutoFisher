using System;
using PInvoke;
using static PInvoke.User32;

namespace AutoFisher
{
	[Flags]
	public enum MouseEventFlags
	{
		LeftDown = 0x00000002,
		LeftUp = 0x00000004,
		MiddleDown = 0x00000020,
		MiddleUp = 0x00000040,
		Move = 0x00000001,
		Absolute = 0x00008000,
		RightDown = 0x00000008,
		RightUp = 0x00000010
	}

	public static class MouseOperations
	{
		public static void SetCursorPosition(int x, int y)
		{
			SetCursorPos(x, y);
		}

		public static void SetCursorPosition(POINT point)
		{
			SetCursorPos(point.x, point.y);
		}

		public static POINT GetCursorPosition()
		{
			var gotPoint = GetCursorPos(out var currentMousePoint);
			if (!gotPoint)
				currentMousePoint = new POINT {x = 0, y = 0};
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

			//var position = GetCursorPosition();
			//mouse_event(value, position.x, position.y, 0, IntPtr.Zero);
			SendInput(inputArray.Length, inputArray, sizeof(INPUT));
		}
	}
}