namespace PriceGenerator
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.buttonStartTimer = new System.Windows.Forms.Button();
			this.buttonFullReload = new System.Windows.Forms.Button();
			this.buttonDeal = new System.Windows.Forms.Button();
			this.buttonStartWithCleaning = new System.Windows.Forms.Button();
			this.buttonRenewDeal = new System.Windows.Forms.Button();
			this.buttonRenewParameters = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// buttonStartTimer
			// 
			this.buttonStartTimer.Location = new System.Drawing.Point(115, 111);
			this.buttonStartTimer.Name = "buttonStartTimer";
			this.buttonStartTimer.Size = new System.Drawing.Size(75, 23);
			this.buttonStartTimer.TabIndex = 2;
			this.buttonStartTimer.Text = "Start";
			this.buttonStartTimer.UseVisualStyleBackColor = true;
			this.buttonStartTimer.Visible = false;
			this.buttonStartTimer.Click += new System.EventHandler(this.buttonStartTimer_Click);
			// 
			// buttonFullReload
			// 
			this.buttonFullReload.Location = new System.Drawing.Point(115, 198);
			this.buttonFullReload.Name = "buttonFullReload";
			this.buttonFullReload.Size = new System.Drawing.Size(157, 23);
			this.buttonFullReload.TabIndex = 3;
			this.buttonFullReload.Text = "Reload all products";
			this.buttonFullReload.UseVisualStyleBackColor = true;
			this.buttonFullReload.Visible = false;
			this.buttonFullReload.Click += new System.EventHandler(this.buttonFullReload_Click);
			// 
			// buttonDeal
			// 
			this.buttonDeal.Location = new System.Drawing.Point(115, 227);
			this.buttonDeal.Name = "buttonDeal";
			this.buttonDeal.Size = new System.Drawing.Size(157, 23);
			this.buttonDeal.TabIndex = 4;
			this.buttonDeal.Text = "Deal";
			this.buttonDeal.UseVisualStyleBackColor = true;
			this.buttonDeal.Visible = false;
			this.buttonDeal.Click += new System.EventHandler(this.buttonDeal_Click);
			// 
			// buttonStartWithCleaning
			// 
			this.buttonStartWithCleaning.Location = new System.Drawing.Point(115, 169);
			this.buttonStartWithCleaning.Name = "buttonStartWithCleaning";
			this.buttonStartWithCleaning.Size = new System.Drawing.Size(122, 23);
			this.buttonStartWithCleaning.TabIndex = 5;
			this.buttonStartWithCleaning.Text = "Clear and Start";
			this.buttonStartWithCleaning.UseVisualStyleBackColor = true;
			this.buttonStartWithCleaning.Visible = false;
			this.buttonStartWithCleaning.Click += new System.EventHandler(this.buttonStartWithCleaning_Click);
			// 
			// buttonRenewDeal
			// 
			this.buttonRenewDeal.Location = new System.Drawing.Point(115, 140);
			this.buttonRenewDeal.Name = "buttonRenewDeal";
			this.buttonRenewDeal.Size = new System.Drawing.Size(75, 23);
			this.buttonRenewDeal.TabIndex = 6;
			this.buttonRenewDeal.Text = "Renew Deal";
			this.buttonRenewDeal.UseVisualStyleBackColor = true;
			this.buttonRenewDeal.Visible = false;
			this.buttonRenewDeal.Click += new System.EventHandler(this.button1_Click);
			// 
			// buttonRenewParameters
			// 
			this.buttonRenewParameters.Location = new System.Drawing.Point(23, 21);
			this.buttonRenewParameters.Name = "buttonRenewParameters";
			this.buttonRenewParameters.Size = new System.Drawing.Size(75, 23);
			this.buttonRenewParameters.TabIndex = 7;
			this.buttonRenewParameters.Text = "Renew parameters";
			this.buttonRenewParameters.UseVisualStyleBackColor = true;
			this.buttonRenewParameters.Click += new System.EventHandler(this.buttonRenewParameters_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this.buttonRenewParameters);
			this.Controls.Add(this.buttonRenewDeal);
			this.Controls.Add(this.buttonStartWithCleaning);
			this.Controls.Add(this.buttonDeal);
			this.Controls.Add(this.buttonFullReload);
			this.Controls.Add(this.buttonStartTimer);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonStartTimer;
        private System.Windows.Forms.Button buttonFullReload;
        private System.Windows.Forms.Button buttonDeal;
        private System.Windows.Forms.Button buttonStartWithCleaning;
		private System.Windows.Forms.Button buttonRenewDeal;
		private System.Windows.Forms.Button buttonRenewParameters;
	}
}

