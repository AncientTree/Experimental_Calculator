using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;


namespace 实验计算
{
    class Calculator
    {
        private HSSFWorkbook Workbook { get; set; }
        private string path;
        public int currentSheetIndex = 0;
        public ISheet currentSheet;
        public List<ISheet> Sheets { get; set; } = new List<ISheet>();
        public List<double> current_sheet_force_data = new List<double>();
        public List<double> current_sheet_displacement_data = new List<double>();

        private double current_line_Diameter;
        private double current_line_Height;
        private double current_line_Mass;

        public int SheetCount { get; }

        public Calculator(string path)
        {
            this.path = path;

            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
            { 

                // read sheets
                Workbook = new HSSFWorkbook(file);
                SheetCount = Workbook.NumberOfSheets;
                for (int i = 0; i < SheetCount; i++)
                {
                    Sheets.Add(Workbook.GetSheetAt(i));
                }
            }
        }

        /// <summary>
        /// 计算压缩强度、模量，将结果返回
        /// </summary>
        /// <param name="sheetIndex">计算的第几页 Sheet</param>
        /// <param name="dataRow">传入的 DataRow</param>
        /// <param name="isCalDensity">True: 计算密度；False: 不计算密度</param>
        public Dictionary<String, double> Calculate(int sheetIndex, DataRow dataRow, Boolean isCalDensity = false)
        {
            SheetRead(sheetIndex, dataRow, isCalDensity);


            double areage = Math.PI * Math.Pow((current_line_Diameter / 2), 2) * 10e-6;


            double percent20Force = Calculate_xPercent_Force(0.2);
            double percent50Force = Calculate_xPercent_Force(0.5);
            Dictionary<String, double> result = new Dictionary<string, double>();
            result.Add("Percent20Strength", percent20Force / areage);
            Console.WriteLine(percent20Force / areage);
            result.Add("Percent50Strength", percent50Force / areage);
            result.Add("Percent20Modulus", percent20Force / areage * 5);
            result.Add("Percent50Modulus", percent50Force / areage * 2);

            if (isCalDensity)
            {
                double density = current_line_Mass / (areage * current_line_Height) * 10e-3;
                result.Add("Density", density);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sheetIndex"></param>
        /// <param name="dataRow"></param>
        /// <param name="isCalDensity">True: 计算密度；False: 不计算密度</param>
        /// <returns></returns>


        /// <summary>
        /// 根据传入的位移百分比计算对应该位移的载荷
        /// </summary>
        /// <param name="percent"></param>
        /// <returns></returns>
        private double Calculate_xPercent_Force(double percent)
        {
            double x_percent_displacement = percent * current_line_Height;
            List<double> delta_l_x_percent_s = new List<double>();
            foreach (double delta_l_x_percent in current_sheet_displacement_data)
            {
                delta_l_x_percent_s.Add(Math.Abs(delta_l_x_percent - x_percent_displacement));
            }
            int index = delta_l_x_percent_s.IndexOf(delta_l_x_percent_s.Min());
            Console.WriteLine(index);
            double force = current_sheet_force_data[index] * 10e-3;
            return force;
        }


        /// <summary>
        /// 将用户输入的直径、高、质量读入到变量, 将Sheet中的载荷、位移数据 读入到list中
        /// </summary>
        /// <param name="sheetIndex"></param>
        /// <param name="isCalDensity"></param>
        private void SheetRead(int sheetIndex, DataRow dataRow, Boolean isCalDensity=false)
        {
            currentSheetIndex = sheetIndex;
            // 将用户输入的直径、高、质量读入到变量
            current_line_Diameter = dataRow.GetDiameter();
            current_line_Height = dataRow.GetHeight();
            if (isCalDensity)
            {
                current_line_Mass = dataRow.GetMass();
            }


            // 将Sheet中的载荷、位移数据 读入到list中
            using ( var file = new FileStream(path, FileMode.Open, FileAccess.Read) )
            {
                var rows = Sheets[sheetIndex].GetEnumerator();
                rows.MoveNext(); // 跳过第一行文字
                while (rows.MoveNext())
                {
                    try
                    {
                        HSSFRow row = (HSSFRow)rows.Current;

                        current_sheet_force_data.Add(row.Cells[1].NumericCellValue);
                        current_sheet_displacement_data.Add(row.Cells[2].NumericCellValue);

                        Console.WriteLine("载荷：" + row.Cells[1] + "kN, 位移: " + row.Cells[2] + "mm.");
                        Console.WriteLine("载荷type：" + row.Cells[1].NumericCellValue.ToString() + "kN, 位移type: " + row.Cells[2].GetType() + "mm.");
                    }
                    catch (System.InvalidOperationException)
                    {
                        System.Windows.MessageBox.Show("请检查选择的Excel表格是否正确。");
                        break;
                    }
                    
                }
            }
            currentSheet = Sheets[currentSheetIndex];


            Console.WriteLine(current_sheet_displacement_data.Count.ToString() + current_sheet_force_data.Count);
            Console.WriteLine(current_sheet_displacement_data.Max());
        }
        public void InputRead()
        {

        }
    }
}
