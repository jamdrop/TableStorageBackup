namespace AntsCode.TableStorageBackup
{
    partial class BlobBrowserForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BlobBrowserForm));
            this.SplitContainer = new System.Windows.Forms.SplitContainer();
            this.ContainerList = new System.Windows.Forms.ListView();
            this.BlobList = new System.Windows.Forms.ListView();
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // SplitContainer
            // 
            this.SplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.SplitContainer.Location = new System.Drawing.Point(0, 0);
            this.SplitContainer.Name = "SplitContainer";
            // 
            // SplitContainer.Panel1
            // 
            this.SplitContainer.Panel1.Controls.Add(this.ContainerList);
            // 
            // SplitContainer.Panel2
            // 
            this.SplitContainer.Panel2.Controls.Add(this.BlobList);
            this.SplitContainer.Size = new System.Drawing.Size(673, 401);
            this.SplitContainer.SplitterDistance = 224;
            this.SplitContainer.TabIndex = 0;
            // 
            // ContainerList
            // 
            this.ContainerList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ContainerList.Location = new System.Drawing.Point(0, 0);
            this.ContainerList.Name = "ContainerList";
            this.ContainerList.Size = new System.Drawing.Size(224, 401);
            this.ContainerList.TabIndex = 9;
            this.ContainerList.UseCompatibleStateImageBehavior = false;
            this.ContainerList.View = System.Windows.Forms.View.List;
            this.ContainerList.SelectedIndexChanged += new System.EventHandler(this.ContainerList_SelectedIndexChanged);
            // 
            // BlobList
            // 
            this.BlobList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BlobList.Location = new System.Drawing.Point(0, 0);
            this.BlobList.Name = "BlobList";
            this.BlobList.Size = new System.Drawing.Size(445, 401);
            this.BlobList.TabIndex = 0;
            this.BlobList.UseCompatibleStateImageBehavior = false;
            this.BlobList.View = System.Windows.Forms.View.List;
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.Location = new System.Drawing.Point(489, 407);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(83, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(578, 407);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(83, 23);
            this.CancelButton.TabIndex = 2;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            // 
            // BlobBrowserForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(673, 437);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.SplitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BlobBrowserForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Blob Storage Browser";
            this.Load += new System.EventHandler(this.BlobBrowserForm_Load);
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel2.ResumeLayout(false);
            this.SplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer SplitContainer;
        private System.Windows.Forms.ListView ContainerList;
        private System.Windows.Forms.ListView BlobList;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button CancelButton;



    }
}