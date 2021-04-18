using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using MinecraftAutoFisher.Imaging;
using Timer = System.Timers.Timer;

namespace MinecraftAutoFisher
{
	public class AutoFisher : IDisposable
	{
		private static readonly HashSet<Color> PossibleBobberColor = new();

		static AutoFisher()
		{
			for (var i = 206; i <= 214; i++)
			for (var j = 40; j <= 44; j++)
			for (var k = 40; k <= 44; k++)
				PossibleBobberColor.Add(Color.FromArgb(i, j, k));
		}

		private bool _isRunning;
		private Point _topLeftPoint;
		private Point _bottomRightPoint;
		private readonly Timer _timer;

		public AutoFisher(int delay = 1000)
		{
			_isRunning = false;
			_timer = new Timer(Math.Max(delay, 200));

			_timer.Elapsed += async (_, _) =>
			{
				if (!_isRunning)
				{
					_timer.Stop();
					return;
				}

				using var screenshot = ScreenCapture.CaptureActiveWindow();
				using var img = UnmanagedImage.FromManagedImage(screenshot);

				var hasBobber = false;
				for (var y = _topLeftPoint.Y; y < _bottomRightPoint.Y; y++)
				for (var x = _topLeftPoint.X; x < _bottomRightPoint.X; x++)
				{
					if (!IsValidBobber(img[x, y]))
						continue;
					hasBobber = true;
					goto outLoop;
				}

				outLoop:
				if (hasBobber) return;

				_timer.Stop();
				MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.RightDown);
				MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.RightUp);
				await Task.Delay(TimeSpan.FromMilliseconds(500));
				
				MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.RightDown);
				MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.RightUp);
				await Task.Delay(TimeSpan.FromSeconds(2));
				
				_timer.Start();
			};
		}

		public bool Calibrate()
		{
			using var screenshot = ScreenCapture.CaptureActiveWindow();
			using var img = UnmanagedImage.FromManagedImage(screenshot);
			for (var y = 0; y < img.Height; y++)
			for (var x = 0; x < img.Width; x++)
			{
				if (!IsValidBobber(img[x, y]))
					continue;

				_topLeftPoint = new Point(
					x - 100 <= 0 ? 0 : x - 100,
					y - 100 <= 0 ? 0 : y - 100);
				_bottomRightPoint = new Point(
					x + 100 > img.Width ? img.Width - 1 : x + 100,
					y + 100 > img.Height ? img.Height - 1 : y + 100);
				return true;
			}

			return false;
		}

		public void Run()
		{
			_isRunning = true;
			_timer.Start();
		}

		public void Stop() => _isRunning = false;

		public void Dispose() => _timer?.Dispose();

		private static bool IsValidBobber(Color c1)
			=> PossibleBobberColor.Any(color => c1.IsRgbEqualTo(color));
	}

	public static class ColorHelper
	{
		public static bool IsRgbEqualTo(this Color c1, Color c2)
			=> c1.R == c2.R && c1.G == c2.G && c1.B == c2.B;
	}
}