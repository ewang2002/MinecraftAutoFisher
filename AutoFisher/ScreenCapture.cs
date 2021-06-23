using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using static PInvoke.User32;

namespace AutoFisher
{
	public static class ScreenCapture
	{
		public static Bitmap CaptureActiveWindow()
			=> CaptureWindow(GetForegroundWindow());
		
		public static Bitmap CaptureSpecificWindow([NotNull] Process process)
			=> CaptureWindow(process.MainWindowHandle);


		public static Bitmap CaptureWindow(IntPtr handle)
		{
			var windowInfo = new WINDOWINFO();
			GetWindowInfo(handle, ref windowInfo);
			var rect = windowInfo.rcClient;
			var bounds = new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
			var result = new Bitmap(bounds.Width, bounds.Height);

			using var graphics = Graphics.FromImage(result);
			graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
			return result;
		}
	}
}