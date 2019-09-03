using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace 实验计算
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();

            RefreshComboBox();
        }

        void RefreshComboBox()
        {
            ComboBox1.SelectedIndex =
                Convert.ToInt32(double.Parse(ConfigurationManager.AppSettings["x-Percent-1"]) * 10 - 1);
            ComboBox2.SelectedIndex =
                Convert.ToInt32(double.Parse(ConfigurationManager.AppSettings["x-Percent-2"]) * 10 - 1);
            Console.WriteLine("\n\nFrom 实验计算.Settings.RefreshComboBox(): " + ComboBox1.SelectedIndex);
        }

        private void Settings_Save(object sender, RoutedEventArgs e)
        {
            double comboBox1SelectionRatio = (ComboBox1.SelectedIndex + 1) / 10.0;
            double comboBox2SelectionRatio = (ComboBox2.SelectedIndex + 1) / 10.0;

            Console.WriteLine(""+comboBox1SelectionRatio+", "+comboBox2SelectionRatio);

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            //更改设置
            config.AppSettings.Settings["x-Percent-1"].Value = comboBox1SelectionRatio.ToString();
            config.AppSettings.Settings["x-Percent-2"].Value = comboBox2SelectionRatio.ToString();

            //保存更改
            config.Save(ConfigurationSaveMode.Modified);

            Console.WriteLine(
                $@"保存更改至AppSettings：({config.AppSettings.Settings["x-Percent-1"].Value}, {config.AppSettings.Settings["x-Percent-2"].Value})");
            实验计算.MainWindow mainWindow = (实验计算.MainWindow)Owner;
            Console.WriteLine(mainWindow.Title);

            

            mainWindow.RefreshHeader();

            Close();
        }

        private void Settings_Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
