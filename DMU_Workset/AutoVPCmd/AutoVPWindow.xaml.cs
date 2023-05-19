using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DMU_Workset.AutoVPCmd
{
    /// <summary>
    /// Interaction logic for AutoVPWindow.xaml
    /// </summary>
    public partial class AutoVPWindow
    {
        private AutoVPViewModel _viewModel;
        public AutoVPWindow(AutoVPViewModel viewModel)
        {
            InitializeComponent(); 
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void btn_Ok(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
            _viewModel.SaveSetting();
        }

        private void btn_Cancle(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        #region 
        private void OpenWebSite(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://alphabimvn.com/vi");
            }
            catch (Exception)
            {
            }
        }

        private void CustomDevelopment(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("http://bit.ly/3bNeJek");
            }
            catch (Exception)
            {
            }
        }

        private void Feedback(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("mailto:contact@alphabimvn.com");
            }
            catch (Exception)
            {
            }
        }
        #endregion

    }
}
