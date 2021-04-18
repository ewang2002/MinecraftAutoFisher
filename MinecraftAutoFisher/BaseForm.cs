using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinecraftAutoFisher
{
	public partial class BaseForm : Form
	{
		private AutoFisher _autoFisher;

		public BaseForm()
		{
			InitializeComponent();
		}

		private async void StartButton_Click(object sender, EventArgs e)
		{
			var validDelay = int.TryParse(DelayScreenshotTextBox.Text, out var num);
			StartButton.Enabled = false;
			StatusBox.Text = "Starting in 5 seconds. Please go to Minecraft and reel your fishing rod.";
			await Task.Delay(TimeSpan.FromSeconds(5));
			_autoFisher = new AutoFisher(validDelay ? num : 1000);
			var res = _autoFisher.Calibrate();
			if (!res)
			{
				StatusBox.Text = "Failed to calibrate. Try again.";
				StartButton.Enabled = true;
				return;
			}

			StopButton.Enabled = true;
			StatusBox.Text = "AutoFisher started. Do not interact with your mouse.";
			_autoFisher.Run();
		}

		private void StopButton_Click(object sender, EventArgs e)
		{
			if (_autoFisher is null)
				return;
			_autoFisher.Stop();

			StartButton.Enabled = true;
			StopButton.Enabled = false;
			StatusBox.Text = "Stopped AutoFisher.";
			_autoFisher.Dispose();
			_autoFisher = null;
		}
	}
}
