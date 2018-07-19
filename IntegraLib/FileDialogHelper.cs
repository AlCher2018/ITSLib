using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IntegraLib
{
    public static class FileDialogHelper
    {
        public static string SearchOpenFile(string fileExt, string initFileDir = null, string initFileName = null)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog()
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = fileExt,
                Filter = string.Format("{0} file|*.{1}|all files|*.*", fileExt.ToUpper(), fileExt),
                FilterIndex = 1,
                Multiselect = false,
                Title = string.Format("Открыть {0} файл", fileExt.ToUpper()),
                InitialDirectory = initFileDir,
                FileName = initFileName
            };

            string fileName = null;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) fileName = ofd.FileName;
            ofd.Dispose();

            return fileName;
        }

        public static string SearchSaveFile(string fileExt, string initFileDir = null, string initFileName = null)
        {
            System.Windows.Forms.SaveFileDialog ofd = new System.Windows.Forms.SaveFileDialog()
            {
                AddExtension = true,
                CheckFileExists = false,
                CheckPathExists = true,
                DefaultExt = fileExt,
                Filter = string.Format("{0} file|*.{1}", fileExt.ToUpper(), fileExt),
                FilterIndex = 1,
                Title = string.Format("Сохранить {0} файл", fileExt.ToUpper()),
                InitialDirectory = initFileDir,
                FileName = initFileName
            };
            if (!initFileName.IsNull()) ofd.FileName = initFileName;

            string fileName = null;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) fileName = ofd.FileName;
            ofd.Dispose();

            return fileName;
        }



    }  // class FileDialogHelper
}
