#region Namespaces
using System.Windows.Forms;
using System;
using System.IO;
using Newtonsoft.Json;

#endregion

namespace AlphaBIM
{
    /// <summary>
    /// Generic class. Tham khảo: https://viblo.asia/p/su-dung-generics-trong-c-924lJDvNKPM
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SettingRepository<T> where T : new()
    {
        /// <summary>
        /// Tạo một instance của class T, có các properties được lưu trong file đường dẫn pathToSettingFile
        /// </summary>
        /// <param name="pathToSettingFile">path to file setting</param>
        /// <returns></returns>
        public T GetSetting(string pathToSettingFile)
        {
            T result;
            if (File.Exists(pathToSettingFile))
            {
                using (StreamReader file = File.OpenText(pathToSettingFile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    result = (T)serializer.Deserialize(file, typeof(T));
                }
                return result;
            }

            // đường dẫn pathToSettingFile không tồn tại, SaveDefautSetting để tạo đường dẫn pathToSettingFile
            SaveDefautSetting(new T(), pathToSettingFile);
            return GetSetting(pathToSettingFile);
        }

        /// <summary>
        /// Tạo thư mục chứa file setting
        /// </summary>
        private void SaveDefautSetting(T settingObject, string pathToSettingFile)
        {
            string json = JsonConvert.SerializeObject(settingObject, Newtonsoft.Json.Formatting.Indented);
            if (!Directory.Exists(AlphaBIMConstraint.SettingFolder)) // Kiểm tra thư mục Setting có tồn tại?
            {
                Directory.CreateDirectory(AlphaBIMConstraint.SettingFolder);
            }

            File.WriteAllText(pathToSettingFile, json);
        }

        /// <summary>
        /// Save the current value of viewModel to file setting, using save dialog to choose the path if pathToSettingFile = null
        /// </summary>
        /// <param name="fileToSave">current value of viewModel to save</param>
        /// <param name="pathToSettingFile">the path to save file setting</param>
        public void SaveSetting(T fileToSave, string pathToSettingFile = null)
        {
            string json = null;
            if (string.IsNullOrEmpty(pathToSettingFile))
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "JSON file(*.json)|*.json";
                sfd.Title = "Save File Setting";
                sfd.FileName = "xxxSetting";
                sfd.DefaultExt = ".json";
                sfd.InitialDirectory = AlphaBIMConstraint.SettingFolder;

                sfd.RestoreDirectory = true;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string path = sfd.FileName;
                    json = JsonConvert.SerializeObject(fileToSave, Formatting.Indented);
                    File.WriteAllText(path, json);
                }
            }
            else
            {
                json = JsonConvert.SerializeObject(fileToSave, Formatting.Indented);
                File.WriteAllText(pathToSettingFile, json);
            }
        }

        /// <summary>
        /// Lấy về model setting được lưu trong file setting, sử dụng open dialog để mở file setting
        /// </summary>
        /// <returns></returns>
        public T GetSetting()
        {
            T result;
            StreamReader file = null;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments); //-->Document
            openFileDialog.Filter = "JSON file(*.json)|*.json";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((file = File.OpenText(openFileDialog.FileName)) != null)
                    {
                        using (file)
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            result = (T)serializer.Deserialize(file, typeof(T));
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
            return new T();
        }
    }
}

