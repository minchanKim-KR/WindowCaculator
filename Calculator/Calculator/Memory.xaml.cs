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
    /// Memory.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Memory : Window
    {
        public event EventHandler pressButton;
        public event MemoryEventHandler pressCalButton;
        public event EventHandler clear;
        public Memory()
        {
            InitializeComponent();
            int nameCounter = 0;
            foreach (var item in WpfApp1.MainWindow.memory)
            {
                Grid grd = new Grid();
                grd.Height = 150;

                Button testButton = new Button();
                testButton.Name = $"N{nameCounter}"; //다른 문자 안붙이면 왜 오류남?
                testButton.Content = displayExp(item);
                testButton.FontSize = 10;
                testButton.FontWeight = FontWeights.Bold;
                testButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xEC, 0xEC, 0xEC));

                Thickness thickness;
                thickness.Right = 0;
                testButton.BorderThickness = thickness;
                testButton.Height = 100;

                Canvas btnCanvas = new Canvas();
                
                Button mc = new Button();
                Button mPlus = new Button();
                Button mMinus = new Button();
                mc.Name = $"mc{nameCounter}";
                mPlus.Name = $"mp{nameCounter}";
                mMinus.Name = $"mm{nameCounter}";
                nameCounter++;
                mc.Content = "MC";
                mPlus.Content = "M+";
                mMinus.Content = "M-";
                mc.Height = 30;
                mc.Width = 40;
                mPlus.Height = 30;
                mPlus.Width = 40;
                mMinus.Height = 30;
                mMinus.Width = 40;
                btnCanvas.Children.Add(mc);
                btnCanvas.Children.Add(mPlus);
                btnCanvas.Children.Add(mMinus);
                Canvas.SetBottom(mc, 25);
                Canvas.SetBottom(mPlus, 25);
                Canvas.SetBottom(mMinus, 25);
                Canvas.SetRight(mc, 110);
                Canvas.SetRight(mPlus, 60);
                Canvas.SetRight(mMinus, 10);

                //Grid의 자식 요소
                grd.Children.Add(testButton);
                grd.Children.Add(btnCanvas);

                testButton.Click += PressMainButton;
                mc.Click += PressMcButton;
                mPlus.Click += PressMpButton;
                mMinus.Click += PressMmButton;

                //여백
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
        void PressMainButton(object sender, RoutedEventArgs e)
        {
            int seq = stack.Children.IndexOf((sender as Button).Parent as Grid);
            int size = WpfApp1.MainWindow.memory.Count;

            var item = WpfApp1.MainWindow.memory[size - seq - 1];

            WpfApp1.MainWindow.numBuffer = item;
            WpfApp1.MainWindow.isEqualKeyInput = true;
            WpfApp1.MainWindow.isOperatorInput = true;

            pressButton(); // event
            this.Close();
            return;
        }

        void PressMcButton(object sender, RoutedEventArgs e)
        {
            int seq = stack.Children.IndexOf(((sender as Button).Parent as Canvas).Parent as Grid);
            int size = WpfApp1.MainWindow.memory.Count;

            stack.Children.RemoveAt(seq);
            WpfApp1.MainWindow.memory.RemoveAt(size - seq - 1);

            clear();
        }
        void PressMpButton(object sender, RoutedEventArgs e)
        {
            int seq = stack.Children.IndexOf(((sender as Button).Parent as Canvas).Parent as Grid);
            int size = WpfApp1.MainWindow.memory.Count;
            Button contentBtn = (((sender as Button).Parent as Canvas).Parent as Grid).Children[0] as Button;

            pressCalButton("+", size - seq - 1);

            contentBtn.Content = displayExp(WpfApp1.MainWindow.memory[size - seq - 1]);
        }
        void PressMmButton(object sender, RoutedEventArgs e)
        {
            int seq = stack.Children.IndexOf(((sender as Button).Parent as Canvas).Parent as Grid);
            int size = WpfApp1.MainWindow.memory.Count;
            Button contentBtn = (((sender as Button).Parent as Canvas).Parent as Grid).Children[0] as Button;

            pressCalButton("-", size - seq - 1);

            contentBtn.Content = displayExp(WpfApp1.MainWindow.memory[size - seq - 1]);
        }

        private void RemoveAllData(object sender, RoutedEventArgs e)
        {
            WpfApp1.MainWindow.memory = new List<string>();
            clear();

            this.Close();
        }
    }
}
