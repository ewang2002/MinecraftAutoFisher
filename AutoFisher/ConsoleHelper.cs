using System;

namespace AutoFisher
{
	public class ConsoleHelper
	{
		/// <summary>
		/// Calls Console.WriteLine with a foreground color and log type.
		/// </summary>
		/// <typeparam name="T">The input type.</typeparam>
		/// <param name="input">The input to be logged.</param>
		/// <param name="foreground">The color that the output should be.</param>
		/// <param name="type">The logging type.</param>
		public static void WriteLine<T>(ConsoleLogType type, T input)
		{
			var time = DateTime.Now;
			var color = type switch
			{
				ConsoleLogType.Error => ConsoleColor.Red,
				ConsoleLogType.Warning => ConsoleColor.Yellow,
				ConsoleLogType.Info => ConsoleColor.White,
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};

			var logMsg = time.ToString("[HH:mm:ss] ") + type switch
			{
				ConsoleLogType.Error => "[Error] ",
				ConsoleLogType.Info => "[Info] ",
				ConsoleLogType.Warning => "[Warn] ",
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			} + input;

			Console.ForegroundColor = color;
			Console.WriteLine(logMsg);
			Console.ResetColor();
		}
	}
}