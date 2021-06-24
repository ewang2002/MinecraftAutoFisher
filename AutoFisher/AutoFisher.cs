using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		private const int Radius = 40;
		private static readonly HashSet<Color> PossibleBobberColor = new();

		static AutoFisher()
		{
			for (var i = 206; i <= 214; i++)
			for (var j = 40; j <= 44; j++)
			for (var k = 40; k <= 44; k++)
				PossibleBobberColor.Add(Color.FromArgb(i, j, k));
		}

		public bool IsRunning { get; private set; }
		private Point _topLeftPoint;
		private Point _bottomRightPoint;
		private readonly Timer _timer;
		private AutoFisherLogType _logType;
		private Dictionary<double, long> _averageTimes;
		private DateTime _lastTime;
		private long _caught;
		private int _guiWidth;

		private int _successiveIterations;


		public Process MinecraftProcess { get; init; }
		public bool AutoCloseWhenOutOfFocus { get; init; }

		/// <summary>
		/// Creates a new AutoFisher instance.
		/// </summary>
		/// <param name="guiWidth">The GUI width.</param>
		/// <param name="delay">The delay between checks.</param>
		/// <param name="logType">The logging type.</param>
		public AutoFisher(int guiWidth = 2, int delay = 500, AutoFisherLogType logType = AutoFisherLogType.Info)
		{
			_successiveIterations = 0;
			_guiWidth = guiWidth;
			_lastTime = default;
			IsRunning = false;
			_logType = logType;
			_timer = new Timer(delay is >= 200 and <= 850 ? delay : 850);
			if ((int) _logType >= 2)
			{
				ConsoleHelper.WriteLine(
					ConsoleLogType.Info,
					$"Constructor called. Timer set to: {_timer.Interval} Milliseconds."
				);
			}

			_averageTimes = new Dictionary<double, long>();
			_caught = 0;
			_timer.Elapsed += async (_, _) => await CheckBobber();
		}


		/// <summary>
		/// Calibrates the bobber. This will pinpoint the bobber location and make a note for future reference.
		/// </summary>
		/// <returns>Whether a bobber was found.</returns>
		public bool Calibrate()
		{
			if ((int) _logType >= 2)
				ConsoleHelper.WriteLine(ConsoleLogType.Info, "Attempting to calibrate. This might take a while.");

			var currentTime = DateTime.Now;
			using var screenshot = ScreenCapture.CaptureActiveWindow();
			using var img = UnmanagedImage.FromManagedImage(screenshot);
			// Don't want to parse armor, hotbar, or health
			for (var y = 0; y < img.Height - 50 * _guiWidth; y++)
			for (var x = 0; x < img.Width; x++)
			{
				if (!IsValidBobber(img[x, y]))
					continue;

				_topLeftPoint = new Point(
					x - Radius <= 0 ? 0 : x - Radius,
					y - Radius <= 0 ? 0 : y - Radius);
				_bottomRightPoint = new Point(
					x + Radius > img.Width ? img.Width - 1 : x + Radius,
					y + Radius > img.Height ? img.Height - 1 : y + Radius);

				PrintCalibrationStats(currentTime);
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
			if ((int) _logType >= 3)
				ConsoleHelper.WriteLine(ConsoleLogType.Info, "Checking bobber.");


			if (!IsRunning)
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
			if (hasBobber)
			{
				_successiveIterations = 0;
				if ((int) _logType >= 3)
					ConsoleHelper.WriteLine(ConsoleLogType.Info, "Bobber still intact.");

				return;
			}

			_timer.Stop();
			_successiveIterations++;
			// Handle possibility that we're not in position
			if (HandleSuccessiveCatches())
				return;

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

		/// <summary>
		/// Starts the AutoFisher.
		/// </summary>
		public void Run()
		{
			if ((int) _logType >= 2)
			{
				var processName = MinecraftProcess is null ? "Active Window" : MinecraftProcess.ProcessName;
				ConsoleHelper.WriteLine(ConsoleLogType.Info, "AutoFisher started.");
				Console.WriteLine($"\tUsing Process: {processName}");
				Console.WriteLine($"\tAuto-Kill when Out of Position: {(AutoCloseWhenOutOfFocus ? "Yes" : "No")}");
			}

			IsRunning = true;
			_timer.Start();
		}

		/// <summary>
		/// Stops the AutoFisher.
		/// </summary>
		public void Stop()
		{
			if ((int) _logType >= 2) ConsoleHelper.WriteLine(ConsoleLogType.Info, "AutoFisher stopped.");
			IsRunning = false;
			_timer.Stop();
		}

		/// <summary>
		/// Disposes the AutoFisher instance.
		/// </summary>
		public void Dispose() => _timer?.Dispose();


		/// <summary>
		/// Handles any successive catches. Because there's a long cooldown between catches, if the system seems to
		/// catch something right after checking again multiple times, this could imply that the person might be out
		/// of position.
		/// </summary>
		/// <returns>Whether the program was terminated as a result.</returns>
		private bool HandleSuccessiveCatches()
		{
			if (!AutoCloseWhenOutOfFocus) return false;
			
			switch (_successiveIterations)
			{
				case 4:
					if (_logType >= 0)
						ConsoleHelper.WriteLine(
							ConsoleLogType.Error,
							"Bobber appears to be reeled in and out extremely often. Terminating program."
						);

					var processes = Process.GetProcessesByName("javaw");
					if (processes.Length <= 0) return false;
					processes[0].Kill();
					Stop();
					Dispose();
					return true;
				case 3:
					if ((int) _logType >= 1)
						ConsoleHelper.WriteLine(
							ConsoleLogType.Warning,
							"Bobber appears to be reeled in and out very often. Program will terminate after one more fail."
						);
					return false;
			}

			return false;
		}

		/// <summary>
		/// Whether the color is a valid bobber color.
		/// </summary>
		/// <param name="c1">The color.</param>
		/// <returns>Whether the color is a valid bobber color.</returns>
		private static bool IsValidBobber(Color c1)
			=> PossibleBobberColor.Any(color => c1.IsRgbEqualTo(color));

		/// <summary>
		/// Calculates the average time taken to reel in a fish.
		/// </summary>
		/// <returns>A tuple with the average and number of entries.</returns>
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

		#region Print Stats

		private void PrintCaughtStats()
		{
			// >= 2
			if ((int) _logType < 3)
				return;

			if (_lastTime == default)
				ConsoleHelper.WriteLine(ConsoleLogType.Info, $"Bobber gone. Caught something.");
			else
			{
				var timeTaken = Math.Round((DateTime.Now - _lastTime).TotalSeconds, 1);
				ConsoleHelper.WriteLine(ConsoleLogType.Info, $"Bobber gone. Caught something in {timeTaken} seconds.");
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
				case 3:
					Console.WriteLine(
						$"\tAverage Time Per Fish: {avg} Seconds"
					);
					break;
				case 2:
					ConsoleHelper.WriteLine(
						ConsoleLogType.Info,
						$"Average Time Per Fish: {avg} Seconds ({entries} Caught)"
					);
					break;
			}
		}

		private void PrintCalibrationStats(DateTime currentTime)
		{
			if ((int) _logType < 2)
				return;

			var timeTaken = Math.Round((DateTime.Now - currentTime).TotalSeconds, 1);
			ConsoleHelper.WriteLine(ConsoleLogType.Info, $"Successfully calibrated in {timeTaken} seconds.");
			Console.WriteLine($"\tTop-Left Point: ({_topLeftPoint.X}, {_topLeftPoint.Y})");
			Console.WriteLine($"\tTop-Left Point: ({_bottomRightPoint.X}, {_bottomRightPoint.Y})");
		}

		#endregion
	}

	public static class ColorHelper
	{
		public static bool IsRgbEqualTo(this Color c1, Color c2)
			=> c1.R == c2.R && c1.G == c2.G && c1.B == c2.B;
	}
}