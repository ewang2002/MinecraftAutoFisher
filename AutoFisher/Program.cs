using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFisher;

public static class Program
{
	private static string _instructions = new StringBuilder()
		.Append("Ready to start AutoFisher? Here's a few things to keep in mind.")
		.AppendLine()
		.Append(
			"\t1. Once started, you cannot physically use your computer for anything else. Doing so may stop the AutoFisher. ")
		.Append(
			"You may run other background tasks while AutoFisher is running, as long as it doesn't move (or block) the Minecraft window.")
		.AppendLine()
		.Append(
			"\t2. Keep your Minecraft process visible at all times. Once calibrated, do NOT move the Minecraft window.")
		.AppendLine()
		.Append("\t3. To stop the AutoFisher, you will need to close the AutoFisher console window.")
		.AppendLine()
		.Append(
			"\t4. It is recommended that you keep the AutoFisher console window next to (but not overlapping) your Minecraft process.")
		.AppendLine()
		.Append(
			"\t5. If you plan on AFKing for a long time, you might want to build a protective glass box around you. ")
		.AppendLine()
		.Append("\t6. This program will consume quite a bit of CPU.")
		.AppendLine()
		.Append("\t7. If running on a server, you are responsible for anything that may happen to your account.")
		.AppendLine()
		.Append(
			"\t8. The program will tell you when to reel out your fishing rod. Once this happens, do NOT touch your mouse.")
		.AppendLine()
		.Append("When you are ready, press [Enter]. After that, you will have 5 seconds to prepare your rod. Read ")
		.Append("the directions from the console carefully and prepare to reel out your rod.")
		.ToString();

	public static async Task Main()
	{
		var mcFolder = Path.Join("C:", "Users", Environment.UserName, "AppData", "Roaming", ".minecraft");
		ConsoleHelper.WriteLine(ConsoleLogType.Info, $"What Minecraft folder do you want to use? [{mcFolder}]");
		var folderToUse = Console.ReadLine();
		mcFolder = string.IsNullOrEmpty(folderToUse) ? mcFolder : folderToUse;
		if (!Directory.Exists(mcFolder))
		{
			ConsoleHelper.WriteLine(
				ConsoleLogType.Error,
				$"The directory, {mcFolder}, does not exist. Restart the program and try again."
			);
			ProcessExit();
			return;
		}

		if (Directory.GetFiles(mcFolder).All(x => !x.EndsWith("options.txt")))
		{
			ConsoleHelper.WriteLine(
				ConsoleLogType.Error,
				$"The directory, {mcFolder}, does not have an \"options.txt\" file."
			);
			ProcessExit();
			return;
		}

		var minecraftProcesses = Process.GetProcesses()
			.Where(x => x.ProcessName.StartsWith("javaw"))
			.ToArray();
		switch (minecraftProcesses.Length)
		{
			case 0:
			{
				ConsoleHelper.WriteLine(
					ConsoleLogType.Error,
					"No Minecraft processes are running right now. Open Minecraft and try again."
				);
				ProcessExit();
				return;
			}
			case >= 2:
			{
				ConsoleHelper.WriteLine(
					ConsoleLogType.Error,
					"You have multiple Minecraft processes open. Please close all but one process."
				);
				ProcessExit();
				return;
			}
		}

		var correctProcess = minecraftProcesses[0];

		var options = await File.ReadAllLinesAsync(Path.Join(mcFolder, "options.txt"));
		var guiWidthIdx = Array.FindIndex(options, x => x.StartsWith("guiScale:"));
		if (guiWidthIdx == -1)
		{
			ConsoleHelper.WriteLine(
				ConsoleLogType.Error,
				$"The \"options.txt\" file does not have a valid \"guiScale\" property."
			);
			
			ProcessExit();
			return;
		}

		var gui = int.Parse(options[guiWidthIdx].Split(":")[1]);

		var delay = 350;
		ConsoleHelper.WriteLine(
			ConsoleLogType.Info,
			"What should the delay between bobber check be? Minimum is 250 MS and maximum is 850 MS. [350]"
		);
		delay = int.TryParse(Console.ReadLine(), out var y) && y is >= 250 and <= 850
			? y
			: delay;
			
			
		ConsoleHelper.WriteLine(
			ConsoleLogType.Info,
			"Do you want to automatically close Minecraft when you are out of position for too long? [y]/n"
		);
		var closeMinecraftFoc = (Console.ReadLine()?.ToLower() ?? string.Empty) != "n";

		ConsoleHelper.WriteLine(
			ConsoleLogType.Info,
			_instructions
		);
		Console.ReadLine();

		for (var a = 5; a >= 0; a--)
		{
			ConsoleHelper.WriteLine(ConsoleLogType.Info, $"You have {a} seconds remaining.");
			if (a == 2)
				Console.WriteLine("\tReel your fishing rod out now.");
			await Task.Delay(1000);
		}
			
		Console.Clear();

		var autoFisher = new AutoFisher(gui, delay)
		{
			MinecraftProcess = correctProcess,
			AutoCloseWhenOutOfFocus = closeMinecraftFoc
		};

		if (!autoFisher.Calibrate())
		{
			ConsoleHelper.WriteLine(
				ConsoleLogType.Error,
				"Failed to calibrate. Are you sure you reeled your rod out?"
			);
			return;
		}

		autoFisher.Run();
		while (autoFisher.IsRunning)
		{
			// Infinite loop to keep the auto fisher running.
		}

		correctProcess.Dispose();
	}

	/// <summary>
	/// Prompts the user that the program is ready to exit/
	/// </summary>
	public static void ProcessExit()
	{
		ConsoleHelper.WriteLine(ConsoleLogType.Info, "Press [Enter] to exit this program.");
		Console.ReadLine();
	}
}