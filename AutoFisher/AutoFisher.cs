using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;
using AutoFisher.Imaging;
using AutoFisher.Windows;
using PInvoke;

namespace AutoFisher;

public class AutoFisher
{
    private const int Radius = 40;
    private static readonly HashSet<Color> PossibleBobberColor = new();

    /// <summary>
    /// Adds all possible bobber colors (i.e., variations of red) to the set of all
    /// possible colors.
    /// </summary>
    static AutoFisher()
    {
        for (var i = 206; i <= 214; i++)
        for (var j = 40; j <= 44; j++)
        for (var k = 40; k <= 44; k++)
            PossibleBobberColor.Add(Color.FromArgb(i, j, k));
    }

    /// <summary>
    /// Whether the autofisher is running.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// The point denoting the top-left corner of the rectangle containing the bobber.
    /// This should be set during the calibration process.
    /// </summary>
    private Point _topLeftPoint;

    /// <summary>
    /// The point denoting the bottom-right corner of the rectangle containing the bobber.
    /// This should be set during the calibration process.
    /// </summary>
    private Point _bottomRightPoint;

    /// <summary>
    /// The timer that is responsible for checking if the bobber is there at consistent intervals.
    /// </summary>
    private readonly Timer _timer;

    /// <summary>
    /// A map containing the average times spent waiting for fish. The key is the time, in seconds,
    /// and the value is the number of times it took to wait for the bobber at that time (key). For
    /// example, the key-value pair, <code>{9.2 -> 5}</code> means that there were 5 instances where
    /// it took 9.2 seconds to catch something.
    /// </summary>
    private readonly Dictionary<double, long> _averageTimes;

    /// <summary>
    /// The last time we reeled something in.
    /// </summary>
    private DateTime _lastTime;

    /// <summary>
    /// The number of times we reeled in the fishing rod.
    /// </summary>
    private long _caught;

    /// <summary>
    /// The GUI width of the game client.
    /// </summary>
    private readonly int _guiWidth;

    /// <summary>
    /// The number of times the bobber was constantly being reeled in/out. A high number
    /// indicates that something is wrong with the placement of the bobber.
    /// </summary>
    private int _successiveIterations;

    /// <summary>
    /// The Minecraft process that we're using.
    /// </summary>
    public Process MinecraftProcess { get; init; }

    /// <summary>
    /// Whether to close the Minecraft process when we're out of focus.
    /// </summary>
    public bool AutoCloseWhenOutOfFocus { get; init; }

    /// <summary>
    /// Creates a new AutoFisher instance.
    /// </summary>
    /// <param name="guiWidth">The GUI width.</param>
    /// <param name="delay">The delay between checks.</param>
    public AutoFisher(int guiWidth = 2, int delay = 500)
    {
        _successiveIterations = 0;
        _guiWidth = guiWidth;
        _lastTime = default;
        IsRunning = false;
        _timer = new Timer(delay is >= 200 and <= 850 ? delay : 850);
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
            _lastTime = DateTime.Now;
            return true;
        }

        ConsoleHelper.WriteLine(ConsoleLogType.Error, "Failed to calibrate. Is your bobber visible?");
        return false;
    }

    /// <summary>
    /// Looks for a bobber and takes action if one was not found.
    /// </summary>
    private async Task CheckBobber()
    {
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
            break;
        }

        if (hasBobber)
        {
            _successiveIterations = 0;
            return;
        }

        _timer.Stop();
        _successiveIterations++;
        // Handle possibility that we're not in position
        if (HandleSuccessiveCatches())
            return;
        
        _caught++;
        PrintCaughtStats();
        
        var diff = DateTime.Now - _lastTime;
        var diffRounded = Math.Round(diff.TotalSeconds, 1);

        if (_averageTimes.ContainsKey(diffRounded)) _averageTimes[diffRounded]++;
        else _averageTimes.Add(diffRounded, 1);

        _lastTime = DateTime.Now;
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
        ConsoleHelper.WriteLine(ConsoleLogType.Info, "Reeled fishing rod out.");
        
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
        var processName = MinecraftProcess is null ? "Active Window" : MinecraftProcess.ProcessName;
        ConsoleHelper.WriteLine(ConsoleLogType.Info, "AutoFisher started.");
        Console.WriteLine($"\tUsing Process: {processName}");
        Console.WriteLine($"\tAuto-Kill when Out of Position: {(AutoCloseWhenOutOfFocus ? "Yes" : "No")}");

        IsRunning = true;
        _timer.Start();
    }

    /// <summary>
    /// Stops the AutoFisher.
    /// </summary>
    private void Stop()
    {
        ConsoleHelper.WriteLine(ConsoleLogType.Info, "AutoFisher has stopped.");
        IsRunning = false;
        _timer.Stop();
    }

    /// <summary>
    /// Disposes the AutoFisher instance.
    /// </summary>
    private void Dispose() => _timer?.Dispose();


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

    /// <summary>
    /// Prints the number of items that have been caught.
    /// </summary>
    private void PrintCaughtStats()
    {
        var timeTaken = Math.Round((DateTime.Now - _lastTime).TotalSeconds, 1);
        ConsoleHelper.WriteLine(ConsoleLogType.Info,
            $"Bobber no longer in view. Caught something in {timeTaken} seconds.");
        Console.WriteLine($"\tAutoFisher has now caught {_caught} item(s).");
    }

    /// <summary>
    /// Prints the current time stats for the autofisher.
    /// </summary>
    private void PrintTimeStats()
    {
        if (_caught % 5 != 0)
            return;

        var (time, numCaught) = CalculateAverage();
        var avg = Math.Round(time, 1);
        ConsoleHelper.WriteLine(
            ConsoleLogType.Info,
            $"Average Time Per Fish: {avg} Seconds ({numCaught} Caught)"
        );
    }

    /// <summary>
    /// Prints the calibration stats.
    /// </summary>
    /// <param name="calibrationStarted">The time when calibration started.</param>
    private void PrintCalibrationStats(DateTime calibrationStarted)
    {
        var timeTaken = Math.Round((DateTime.Now - calibrationStarted).TotalSeconds, 1);
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