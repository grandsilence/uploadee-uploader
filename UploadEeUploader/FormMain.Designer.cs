namespace UploadEeUploader
{
    partial class FormMain
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnUpload = new System.Windows.Forms.Button();
            this.tbDownloadLink = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnUpload
            // 
            this.btnUpload.Location = new System.Drawing.Point(12, 38);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(113, 33);
            this.btnUpload.TabIndex = 0;
            this.btnUpload.Text = "Загрузить";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // tbDownloadLink
            // 
            this.tbDownloadLink.Location = new System.Drawing.Point(12, 12);
            this.tbDownloadLink.Name = "tbDownloadLink";
            this.tbDownloadLink.ReadOnly = true;
            this.tbDownloadLink.Size = new System.Drawing.Size(298, 20);
            this.tbDownloadLink.TabIndex = 1;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(344, 87);
            this.Controls.Add(this.tbDownloadLink);
            this.Controls.Add(this.btnUpload);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Upload.ee Uploader";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.TextBox tbDownloadLink;
    }
}

