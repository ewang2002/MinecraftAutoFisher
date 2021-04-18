
namespace MinecraftAutoFisher
{
	partial class BaseForm
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseForm));
			this.StartButton = new System.Windows.Forms.Button();
			this.StopButton = new System.Windows.Forms.Button();
			this.StatusBox = new System.Windows.Forms.TextBox();
			this.DelayScreenshotTextBox = new System.Windows.Forms.TextBox();
			this.DelayBetweenCheckLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// StartButton
			// 
			this.StartButton.Font = new System.Drawing.Font("Gadugi", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.StartButton.Location = new System.Drawing.Point(4, 138);
			this.StartButton.Name = "StartButton";
			this.StartButton.Size = new System.Drawing.Size(157, 63);
			this.StartButton.TabIndex = 0;
			this.StartButton.Text = "Start";
			this.StartButton.UseVisualStyleBackColor = true;
			this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
			// 
			// StopButton
			// 
			this.StopButton.Enabled = false;
			this.StopButton.Font = new System.Drawing.Font("Gadugi", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.StopButton.Location = new System.Drawing.Point(196, 139);
			this.StopButton.Name = "StopButton";
			this.StopButton.Size = new System.Drawing.Size(157, 63);
			this.StopButton.TabIndex = 1;
			this.StopButton.Text = "Stop";
			this.StopButton.UseVisualStyleBackColor = true;
			this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
			// 
			// StatusBox
			// 
			this.StatusBox.Location = new System.Drawing.Point(4, 41);
			this.StatusBox.Multiline = true;
			this.StatusBox.Name = "StatusBox";
			this.StatusBox.ReadOnly = true;
			this.StatusBox.Size = new System.Drawing.Size(349, 91);
			this.StatusBox.TabIndex = 2;
			// 
			// DelayScreenshotTextBox
			// 
			this.DelayScreenshotTextBox.Location = new System.Drawing.Point(164, 8);
			this.DelayScreenshotTextBox.Name = "DelayScreenshotTextBox";
			this.DelayScreenshotTextBox.Size = new System.Drawing.Size(189, 27);
			this.DelayScreenshotTextBox.TabIndex = 3;
			this.DelayScreenshotTextBox.Text = "1000";
			// 
			// DelayBetweenCheckLabel
			// 
			this.DelayBetweenCheckLabel.AutoSize = true;
			this.DelayBetweenCheckLabel.Location = new System.Drawing.Point(4, 11);
			this.DelayBetweenCheckLabel.Name = "DelayBetweenCheckLabel";
			this.DelayBetweenCheckLabel.Size = new System.Drawing.Size(154, 20);
			this.DelayBetweenCheckLabel.TabIndex = 4;
			this.DelayBetweenCheckLabel.Text = "Delay Between Check:";
			// 
			// BaseForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(360, 205);
			this.Controls.Add(this.DelayBetweenCheckLabel);
			this.Controls.Add(this.DelayScreenshotTextBox);
			this.Controls.Add(this.StatusBox);
			this.Controls.Add(this.StopButton);
			this.Controls.Add(this.StartButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "BaseForm";
			this.Text = "AutoFisher";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button StartButton;
		private System.Windows.Forms.Button StopButton;
		private System.Windows.Forms.TextBox StatusBox;
		private System.Windows.Forms.TextBox DelayScreenshotTextBox;
		private System.Windows.Forms.Label DelayBetweenCheckLabel;
	}
}

