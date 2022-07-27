using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Record.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Record : Window
    {
        public event EventHandler pressButton;
        public Record()
        {
            InitializeComponent();
            int nameCounter = 0;

            foreach (var item in WpfApp1.MainWindow.calList)
            {
                string fn = displayExp(item.firstNum);
                string sn = displayExp(item.secondNum);
                string operAri = item.operatorAri;
                string res = displayExp(item.result);

                Grid grd = new Grid();
                grd.Height = 150;

                Button testButton = new Button();
                testButton.Name = $"N{nameCounter}";nameCounter++; //다른 문자 안붙이면 왜 오류남?
                testButton.Content = $"{fn} {operAri} {sn} =\n\n{res}";
                testButton.FontSize = 10;
                testButton.FontWeight = FontWeights.Bold;
                testButton.HorizontalContentAlignment = HorizontalAlignment.Right;
                testButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xEC, 0xEC, 0xEC));

                Thickness thickness;
                thickness.Right = 0;
                testButton.BorderThickness = thickness;
                testButton.Height = 100;
                testButton.Click += PressButton;

                grd.Children.Add(testButton);

                Grid.SetRow(testButton, 1);
                Grid.SetColumn(testButton, 1);

                stack.Children.Insert(0, grd);
            }
        }
        static void ChangeToInteger(string input, out string sigFigures, out int expo)
        {
            //실수 => 유효 숫자,10^지수, 소수점이 없으면 그냥 리턴한다.
            bool minus = false;

            if (input[0] == '-')
            {
                input = input.Remove(0, 1);
                minus = true;
            }

            if (input.Contains("."))
            {
                int index = input.IndexOf(".");
                int leng = input.Length;

                input = input.Replace(".", "");
                while (input.Length > 1 && input[0] == '0')
                {
                    input = input.Remove(0, 1);
                }
                sigFigures = input;
                expo = -((leng - 1) - index);
            }
            else
            {
                sigFigures = input;
                expo = 0;
            }

            if (minus == true)
                sigFigures = "-" + sigFigures;
        }
        string displayExp(string exp)
        {
            int checker = exp.Length;
            bool minus = false;
            checker = exp.Contains(".") ? --checker : checker;
            checker = exp.Contains("-") ? --checker : checker;

            if (exp[0] == '-')
            {
                minus = true;
                exp = exp.Substring(1);
            }
            if (checker > 16) // 지수표현 포매팅
            {
                string sig;
                int expo;
                ChangeToInteger(exp, out sig, out expo);

                if (sig.Length > 16)
                {
                    expo += sig.Length - 16;
                    sig = sig.Remove(16);
                }

                //길이 재검토
                checker = exp.Length;
                checker = exp.Contains(".") ? --checker : checker;
                if (checker <= 16)
                {
                    return displayExp(exp);
                }


                //오류나서 고쳐야함(아래)
                string sigFigures;

                if (sig.Length < 2)
                    sigFigures = sig;
                else
                    sigFigures = sig[0] + "." + sig.Substring(1);

                expo += sig.Length - 1;
                if (expo == -1)
                    sigFigures = exp;
                else if (expo > 0)
                    sigFigures = sigFigures + "E+" + expo;
                else if (expo < 0)
                    sigFigures = sigFigures + "E" + expo;

                if (minus)
                {
                    sigFigures = "-" + sigFigures;
                }
                return sigFigures;
            }
            else // 일반
            {
                if (exp.Contains(".")) //유한 소수
                {
                    string[] temp = exp.Split('.');
                    temp[0] = String.Format("{0:N0}", Int64.Parse(temp[0]));
                    exp = temp[0] + "." + temp[1];
                }
                else // 정수
                {
                    string temp = exp;
                    temp = String.Format("{0:N0}", Int64.Parse(temp));
                    exp = temp;
                }

                if (minus)
                {
                    exp = "-" + exp;
                }
                return exp;
            }
        }

        void PressButton(object sender, RoutedEventArgs e)
        {
            int name = Int32.Parse((sender as Button).Name.Substring(1));

            var item = WpfApp1.MainWindow.calList[name];

            WpfApp1.MainWindow.firstNum = item.firstNum;
            WpfApp1.MainWindow.secondNum = item.secondNum;
            WpfApp1.MainWindow.operatorAri = item.operatorAri;
            WpfApp1.MainWindow.numBuffer = item.result;
            WpfApp1.MainWindow.isEqualKeyInput = true;
            WpfApp1.MainWindow.isOperatorInput = true;

            pressButton(); // event

            this.Close();
            return;
        }

        private void RemoveAllData(object sender, RoutedEventArgs e)
        {
            WpfApp1.MainWindow.calList = new List<Calculation>();
            
            this.Close();
        }
    }
}
