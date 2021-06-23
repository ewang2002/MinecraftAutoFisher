using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFisher
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			Console.WriteLine("Ready?");
			Console.ReadLine();
			await Task.Delay(5000);
			var autoFisher = new AutoFisher(350, AutoFisherLogType.Verbose);
			if (!autoFisher.Calibrate())
			{
				Console.WriteLine("Something went wrong.");
				return;
			}
			Console.WriteLine("Starting.");
			autoFisher.Run();
			await Task.Delay(-1);
		}
	}
}