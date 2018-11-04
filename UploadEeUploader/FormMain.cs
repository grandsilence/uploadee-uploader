using System;
using System.Windows.Forms;

namespace UploadEeUploader
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = null;
            UploadEeClient eeClient = null;

            try
            {
                // Выбираем файл
                fileDialog = new OpenFileDialog();
                var dialogResult = fileDialog.ShowDialog();
                if (dialogResult != DialogResult.OK || fileDialog.FileNames.Length == 0 || string.IsNullOrEmpty(fileDialog.FileNames[0]))
                    return;

                // Загружаем файл
                eeClient = new UploadEeClient();
                tbDownloadLink.Text = eeClient.UploadFile(fileDialog.FileNames[0]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Убираем за собой мусор
                fileDialog?.Dispose();
                eeClient?.Dispose();
            }
        }
    }
}
