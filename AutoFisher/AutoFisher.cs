using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;
using AutoFisher.Imaging;
using PInvoke;

namespace AutoFisher
{
	public class AutoFisher
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
		private AutoFisherLogType _logType;

		private Dictionary<double, long> _averageTimes;
		private DateTime _lastTime;

		private long _caught;

		/// <summary>
		/// Creates a new AutoFisher instance.
		/// </summary>
		/// <param name="delay">The delay between checks.</param>
		/// <param name="logType">The logging type.</param>
		public AutoFisher(int delay = 1000, AutoFisherLogType logType = AutoFisherLogType.Info)
		{
			_lastTime = default;
			_isRunning = false;
			_timer = new Timer(Math.Max(delay, 200));
			_averageTimes = new Dictionary<double, long>();
			_logType = logType;
			_caught = 0;
			_timer.Elapsed += async (_, _) => await CheckBobber();
		}

		/// <summary>
		/// Calibrates the bobber. This will pinpoint the bobber location and make a note for future reference.
		/// </summary>
		/// <returns>Whether a bobber was found.</returns>
		public bool Calibrate()
		{
			if ((int) _logType >= 1)
				ConsoleHelper.WriteLine(ConsoleLogType.Info, "Attempting to calibrate. This might take a while.");

			var currentTime = DateTime.Now;
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

				if ((int) _logType >= 1)
				{
					var timeTaken = Math.Round((DateTime.Now - currentTime).TotalSeconds, 1);
					ConsoleHelper.WriteLine(ConsoleLogType.Info, $"Successfully calibrated in {timeTaken} seconds.");
					Console.WriteLine($"\tTop-Left Point: ({_topLeftPoint.X}, {_topLeftPoint.Y})");
					Console.WriteLine($"\tTop-Left Point: ({_bottomRightPoint.X}, {_bottomRightPoint.Y})");
				}

				return true;
			}

			if (_logType >= 0)
				ConsoleHelper.WriteLine(ConsoleLogType.Error, "Failed to calibrate. Is your bobber visible?");
			return false;
		}

		/// <summary>
		/// Looks for a bobber and takes action if one was not found.
		/// </summary>
		private async Task CheckBobber()
		{
			if (!_isRunning)
			{
				_timer.Stop();
				return;
			}

			_timer.Stop();

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
			if (hasBobber)
			{
				_timer.Start();
				return;
			}

			_caught++;
			PrintCaughtStats();

			if (_lastTime == default)
				_lastTime = DateTime.Now;
			else
			{
				var diff = DateTime.Now - _lastTime;
				var diffRounded = Math.Round(diff.TotalSeconds, 1);

				if (_averageTimes.ContainsKey(diffRounded)) _averageTimes[diffRounded]++;
				else _averageTimes.Add(diffRounded, 1);

				_lastTime = DateTime.Now;
			}

			// Display average time per fish.
			PrintTimeStats();

			// Send initial right-click to reel in.
			MouseOperations.SendMouseEvent(User32.MOUSEEVENTF.MOUSEEVENTF_RIGHTDOWN);
			MouseOperations.SendMouseEvent(User32.MOUSEEVENTF.MOUSEEVENTF_RIGHTUP);

			// Delay here so we can reel the rod back out without any issue. Without this, the
			// bobber might decide to go closer to the player than expected.
			await Task.Delay(TimeSpan.FromMilliseconds(750));

			// Send another right-click to reel out.
			MouseOperations.SendMouseEvent(User32.MOUSEEVENTF.MOUSEEVENTF_RIGHTDOWN);
			MouseOperations.SendMouseEvent(User32.MOUSEEVENTF.MOUSEEVENTF_RIGHTUP);

			// Additional cooldown since the bobber might go back underwater for a bit of time.
			// Usually after 2.5 seconds, the bobber rests above water.
			await Task.Delay(TimeSpan.FromSeconds(2.5));

			_timer.Start();
		}

		public void Run()
		{
			if ((int) _logType >= 1) ConsoleHelper.WriteLine(ConsoleLogType.Info, "AutoFisher started.");
			_isRunning = true;
			_timer.Start();
		}

		public void Stop()
		{
			if ((int) _logType >= 1) ConsoleHelper.WriteLine(ConsoleLogType.Info, "AutoFisher stopped.");
			_isRunning = false;
		}

		public void Dispose() => _timer?.Dispose();

		private static bool IsValidBobber(Color c1)
			=> PossibleBobberColor.Any(color => c1.IsRgbEqualTo(color));

		private (double avg, long entries) CalculateAverage()
		{
			var sum = 0D;
			var size = 0L;
			foreach (var (k, v) in _averageTimes)
			{
				sum += k * v;
				size += v;
			}

			return (sum / size, size);
		}

		private void PrintCaughtStats()
		{
			if ((int) _logType < 2) 
				return;
			
			if (_lastTime == default)
				ConsoleHelper.WriteLine(ConsoleLogType.Info, $"Caught something.");
			else
			{
				var timeTaken = Math.Round((DateTime.Now - _lastTime).TotalSeconds, 1);
				ConsoleHelper.WriteLine(ConsoleLogType.Info, $"Caught something in {timeTaken} seconds.");
			}

			Console.WriteLine($"\tAutoFisher has now caught {_caught} item(s).");
		}

		private void PrintTimeStats()
		{
			if (_caught % 10 != 0) 
				return;
			
			var calculation = CalculateAverage();
			var avg = Math.Round(calculation.avg, 1);
			var entries = calculation.entries;
			switch ((int) _logType)
			{
				case >= 2:
					Console.WriteLine(
						$"\tAverage Time Per Fish: {avg} Seconds"
					);
					break;
				case >= 1:
					ConsoleHelper.WriteLine(
						ConsoleLogType.Info,
						$"Average Time Per Fish: {avg} Seconds ({entries} Caught)"
					);
					break;
			}
		}
	}

	public static class ColorHelper
	{
		public static bool IsRgbEqualTo(this Color c1, Color c2)
			=> c1.R == c2.R && c1.G == c2.G && c1.B == c2.B;
	}
}