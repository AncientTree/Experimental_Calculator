using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;
using System.IO;

namespace 实验计算
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window

    {
        public Configuration config;

        private static int pos { set; get; } = 0;
        private List<DataRow> dataRows = new List<DataRow>();
        private Calculator calculator;

        public MainWindow()
        {

            InitializeComponent();
            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            InitLayout();
        }
        
        public void AddRow()
        {
            if (dataRows.Count >= 9)
            {
                return;
            }
            DataRow dataRow = new DataRow(dataRows.Count + 1);
            dataRow.Place(grid);
            dataRows.Add(dataRow);
        }
        public void AddRow(object sender, RoutedEventArgs e)
        {
            AddRow();
        }

        public void RemoveRow()
        {
            if (dataRows.Count == 1)
            {
                return;
            }
            // 如果box非空
            if (dataRows[dataRows.Count-1].DiameterBox.Text.Trim()!=""
                || dataRows[dataRows.Count - 1].MassBox.Text.Trim() != ""
                || dataRows[dataRows.Count - 1].HeightBox.Text.Trim() != "")
            {
                MessageBoxResult result = MessageBox.Show(String.Format("将要删除第{0:G}行，但此行还有已输入的数据，确认继续吗？", dataRows.Count), "确认移除行", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    // 删行
                    dataRows.Last().Remove(grid);
                    dataRows.RemoveAt(dataRows.Count - 1);
                }
            }
            // 否则如果box空
            else
            {
                // 删行
                dataRows.Last().Remove(grid);
                dataRows.RemoveAt(dataRows.Count - 1);
            }
            
        }
        public void RemoveRow(object sender, RoutedEventArgs e)
        {
            RemoveRow();
        }

        public void buttonCalculate()
        {
            try
            {
                if (input_file_path.Text.Trim() != "")
                {
                    
                    // 加入变量用于计算五个平均值，变量名含average，但到输出时才除以行数取得平均值
                    double averageDensity = 0;
                    double averageXPercent1Strength = 0;
                    double averageXPercent2Strength = 0;
                    double averageXPercent1Modulus = 0;
                    double averageXPercent2Modulus = 0;

                    foreach (DataRow row in dataRows)
                    {
                        if (row.GetDiameter() == 0)
                        {
                            MessageBox.Show("留空行会导致平均值计算错误，请移除多余的空行！");
                            return;
                        }
                        calculator = new Calculator(input_file_path.Text);
                        Dictionary<String, double> result = calculator.Calculate(dataRows.IndexOf(row), row, row.GetMass() != 0);

                        // 更新平均数变量
                        averageDensity += result["Density"];
                        averageXPercent1Strength += result["xPercent1Strength"];
                        averageXPercent2Strength += result["xPercent2Strength"];
                        averageXPercent1Modulus += result["xPercent1Modulus"];
                        averageXPercent2Modulus += result["xPercent2Modulus"];

                        /*
                        testBlock.Text += calculator.Sheets[0].GetRow(2).GetCell(0).NumericCellValue.ToString();
                        testBlock.Text += ("\n" + row.GetDiameter());
                        testBlock.Text += ("\n" + row.GetHeight());
                        testBlock.Text += ("\n" + row.GetMass());
                        testBlock.Text += ("\nPercent20Strength " + result["Percent20Strength"]);
                        testBlock.Text += ("\nPercent50Strength " + result["Percent50Strength"]);
                        testBlock.Text += ("\nPercent20Modulus " + result["Percent20Modulus"]);
                        testBlock.Text += ("\nPercent50Modulus " + result["Percent50Modulus"]);
                        */


                        // 更新TextBox
                        row.Set20Strength(result["xPercent1Strength"]);
                        row.Set50Strength(result["xPercent2Strength"]);
                        row.Set20Modulus(result["xPercent1Modulus"]);
                        row.Set50Modulus(result["xPercent2Modulus"]);
                        if (row.GetMass() != 0)
                        {
                            row.SetDensity(result["Density"]);
                        }
                    }

                    // 数据行循环结束，输出平均数
                    testBlock.Text += ("\n平均密度：" + $"{averageDensity / dataRows.Count:G3}");
                    testBlock.Text += ("\n平均" + XPercent1StrengthTextBlock.Text + "：" + $"{averageXPercent1Strength / dataRows.Count:G3}");
                    testBlock.Text += ("\n平均" + XPercent2StrengthTextBlock.Text + "：" + $"{averageXPercent2Strength / dataRows.Count:G3}");
                    testBlock.Text += ("\n平均" + XPercent1ModulusTextBlock.Text + "：" + String.Format("{0:G3}", averageXPercent1Modulus / dataRows.Count));
                    testBlock.Text += ("\n平均" + XPercent2ModulusTextBlock.Text + "：" + String.Format("{0:G3}", averageXPercent2Modulus / dataRows.Count));

                }
                else
                {
                    MessageBox.Show("未选择Excel文件！请点击“打开文件”按钮选择。");
                }
            }
            catch (System.InvalidOperationException)
            {

            }
            
        }
        public void buttonCalculate(object sender, RoutedEventArgs e)
        {
            buttonCalculate();
        }

        /// <summary>
        /// 创建窗口时新建三行数据
        /// </summary>
        private void InitLayout()
        {
            for (int i = 1; i < 4; i++)
            {
                AddRow();
            }

            RefreshHeader();
        }
        
        private void Open_xls_file_button_Click(object sender, RoutedEventArgs e)
        {

            String initialDirectory = @"C:\";
            if (Directory.Exists(ConfigurationManager.AppSettings["initialDirectory"]))
            {
                initialDirectory = ConfigurationManager.AppSettings["initialDirectory"];
            }

            Microsoft.Win32.OpenFileDialog dialog =
                            new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Excel 2003表格文件|*.xls";
            dialog.FilterIndex = 1;
            dialog.InitialDirectory = initialDirectory;
            if (dialog.ShowDialog() == true)
            {
                //此处做你想做的事 
                input_file_path.Text = dialog.FileName;
                // 如果同时选择的路径与配置文件不符，则更新配置文件以记录此次选择的路径
                if (initialDirectory != System.IO.Path.GetDirectoryName(dialog.FileName))
                {
                    initialDirectory = System.IO.Path.GetDirectoryName(dialog.FileName);
                    config.AppSettings.Settings.Remove("initialDirectory");
                    config.AppSettings.Settings.Add("initialDirectory", initialDirectory);
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                    MessageBox.Show("已记住路径" + initialDirectory);
                }
            }
            else
            {

            }
            
        }

        private void About_OnClick(object sender, RoutedEventArgs e)
        {
            About aboutWin = new About {Owner = this};
            aboutWin.ShowDialog();
        }

        private void Settings_OnClick(object sender, RoutedEventArgs e)
        {
            Settings settingsWin = new Settings {Owner = this};
            settingsWin.ShowDialog();

        }

        public void RefreshHeader()
        {
            ConfigurationManager.RefreshSection("appSettings");
            double xPercent1 = double.Parse(ConfigurationManager.AppSettings["x-Percent-1"]);
            double xPercent2 = double.Parse(ConfigurationManager.AppSettings["x-Percent-2"]);

            Console.WriteLine(
                $@"读取设定自AppSettings：({ConfigurationManager.AppSettings["x-Percent-1"]}, {ConfigurationManager.AppSettings["x-Percent-2"]})");
            //以下方式不支持刷新
//            Console.WriteLine(
//                $@"读取设定自AppSettings：({config.AppSettings.Settings["x-Percent-1"].Value}, {config.AppSettings.Settings["x-Percent-2"].Value})");
            XPercent1StrengthTextBlock.Text = "强度" + xPercent1.ToString("P0");
            XPercent2StrengthTextBlock.Text = "强度" + xPercent2.ToString("P0");

            XPercent1ModulusTextBlock.Text = "模量" + xPercent1.ToString("P0");
            XPercent2ModulusTextBlock.Text = "模量" + xPercent2.ToString("P0");


        }
    }


    public class DataRow
    {
        int rowNumber;

        private TextBox NumberBlock = new TextBox();
        public TextBox DiameterBox = new TextBox();
        public TextBox HeightBox = new TextBox();
        public TextBox MassBox = new TextBox();
        private TextBox DensityBox = new TextBox();
        private TextBox Percent20Strength = new TextBox();
        private TextBox Percent50Strength = new TextBox();
        private TextBox Percent20Modulus = new TextBox();
        private TextBox Percent50Modulus = new TextBox();

        private LinkedList<TextBox> allLinkedList = new LinkedList<TextBox>();
        private LinkedList<TextBox> inputLinkedList = new LinkedList<TextBox>();
        

        private Dictionary<String, float> keyValuePairs = new Dictionary<String, float>();

        public DataRow(int number)
        {
            rowNumber = number;
            NumberBlock.Text = number.ToString();

            NumberBlock.IsReadOnly = true;
            DensityBox.IsReadOnly = true;
            Percent20Strength.IsReadOnly = true;
            Percent50Strength.IsReadOnly = true;
            Percent20Modulus.IsReadOnly = true;
            Percent50Modulus.IsReadOnly = true;

            {// 一行所有控件加入到 allLinkedList
                allLinkedList.AddLast(NumberBlock);
                allLinkedList.AddLast(DiameterBox);
                allLinkedList.AddLast(HeightBox);
                allLinkedList.AddLast(MassBox);
                allLinkedList.AddLast(DensityBox);
                allLinkedList.AddLast(Percent20Strength);
                allLinkedList.AddLast(Percent50Strength);
                allLinkedList.AddLast(Percent20Modulus);
                allLinkedList.AddLast(Percent50Modulus);
            }

            {// 三个需要用户输入的控件加入到 inputLinkedList
                inputLinkedList.AddLast(DiameterBox);
                inputLinkedList.AddLast(HeightBox);
                inputLinkedList.AddLast(MassBox);
            }

        }

        
        public void Place(Grid grid)
        {
            int i = 0;
            foreach(TextBox textBox in allLinkedList)
            {
                if (i == 0)
                {
                    textBox.Width = 30;
                }
                else
                {
                    textBox.Width = 60;
                }
                if (i == 1 || i == 2 || i == 3)
                {
                    textBox.BorderThickness = new Thickness(0, 0, 0, 1);
                    
                }
                else
                {
                    textBox.BorderThickness = new Thickness(0, 0, 0, 0);
                    textBox.IsTabStop = false;
                }
                
                textBox.FontSize = 13;
                //textBox.Padding = new Thickness(2, 2, 2, 2);
                textBox.TextAlignment = TextAlignment.Center;
                textBox.HorizontalAlignment = HorizontalAlignment.Center;
                textBox.VerticalAlignment = VerticalAlignment.Center;
                grid.Children.Add(textBox);
                textBox.SetValue(Grid.ColumnProperty, i);
                textBox.SetValue(Grid.RowProperty, rowNumber);
                i++;
            }


            
        }

        public void Remove(Grid grid)
        {
            foreach (var textBox in allLinkedList)
            {
                grid.Children.Remove(textBox);
            }
        }

        

        public double GetDiameter()
        {
            if (DiameterBox.Text == "")
            {
                return 0;
            }
            return Double.Parse(DiameterBox.Text);
        }

        public double GetHeight()
        {
            if (HeightBox.Text == "")
            {
                return 0;
            }
            return Double.Parse(HeightBox.Text);
        }

        public double GetMass()
        {
            if (MassBox.Text=="")
            {
                return 0;
            }
            return Double.Parse(MassBox.Text);
        }

        public void SetDensity(double value)
        {
            DensityBox.Text = String.Format("{0:G3}", value);
        }
        public void Set20Strength(double value)
        {
            Percent20Strength.Text = String.Format("{0:G3}", value);
        }
        public void Set50Strength(double value)
        {
            Percent50Strength.Text = String.Format("{0:G3}", value);

        }
        public void Set20Modulus(double value)
        {
            Percent20Modulus.Text = String.Format("{0:G3}", value);
        }
        public void Set50Modulus(double value)
        {
            Percent50Modulus.Text = String.Format("{0:G3}", value);
        }
    }
}
