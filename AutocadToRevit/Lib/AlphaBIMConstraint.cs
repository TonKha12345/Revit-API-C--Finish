using System;
using System.IO;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;

namespace AlphaBIM
{
    public class AlphaBIMConstraint
    {
        #region Khai báo các Field Constraint

        public string ContentsFolder;
        public string ResourcesFolder;
        public string HelpFolder;
        public string ImageFolder;
        public string SettingFolder;
        public string DllFolder;
        public BitmapImage IconWindow;
        public string HelperPath;

        #endregion

        public AlphaBIMConstraint(ControlledApplication a = null)
        {
            #region   Khởi tạo giá trị cho các Field Constraint
            SettingFolder =
                @"C:\ProgramData\Autodesk\ApplicationPlugins\Q'AppsTraining.bundle\Contents\Resources\Setting";
            ResourcesFolder = 
                "C:\\ProgramData\\Autodesk\\ApplicationPlugins\\Q'AppsTraining.bundle\\Contents\\Resources";
            HelpFolder = Path.Combine(ResourcesFolder, "Help");
            ImageFolder = Path.Combine(ResourcesFolder, "Image");
            HelperPath = Path.Combine(HelpFolder, "Q'AppsHelper.pdf");

            string iconWindowPath = Path.Combine(ImageFolder, "About.ico");
            Uri iconWindowUri = new Uri(iconWindowPath, UriKind.Relative);
            IconWindow = new BitmapImage(iconWindowUri);

            if (a != null)
            {
                switch (a.VersionNumber)
                {
                    case "2017":
                        DllFolder = Path.Combine(ContentsFolder, "2017", "dll");
                        break;
                    case "2018":
                        DllFolder = Path.Combine(ContentsFolder, "2018", "dll");
                        break;
                    case "2019":
                        DllFolder = Path.Combine(ContentsFolder, "2019", "dll");
                        break;
                    case "2020":
                        DllFolder = Path.Combine(ContentsFolder, "2020", "dll");
                        break;
                    case "2021":
                        DllFolder = Path.Combine(ContentsFolder, "2021", "dll");
                        break;
                    case "2022":
                        DllFolder = Path.Combine(ContentsFolder, "2022", "dll");
                        break;
                    case "2023":
                        DllFolder = Path.Combine(ContentsFolder, "2023", "dll");
                        break;
                    case "2024":
                        DllFolder = Path.Combine(ContentsFolder, "2024", "dll");
                        break;
                    case "2025":
                        DllFolder = Path.Combine(ContentsFolder, "2025", "dll");
                        break;
                }
            }

            #endregion Khai báo các biến
        }
    }
}