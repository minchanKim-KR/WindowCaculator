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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public delegate void EventHandler();
    public delegate void MemoryEventHandler(string oper,int index);
    struct Calculation
    {
        public string? firstNum;
        public string? secondNum;
        public string? operatorAri;
        public string result;
    }
    public partial class MainWindow : Window
    {
        internal static string numBuffer = "0";

        internal static string? firstNum = null;
        internal static string? secondNum = null;
        internal static string? operatorAri = null;

        // 키 입력에 따라 동작 처리(Input함수)
        internal static bool isOperatorInput = false;
        bool isNumKeyInput = false;
        internal static bool isEqualKeyInput = false;

        bool isOneOutOfXInput = false;
        string correction = "";

        internal static List<Calculation> calList = new List<Calculation>();
        internal static List<string> memory = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
        }
        void NumKeyInput(int num)
        {
            isNumKeyInput = true;
            isOneOutOfXInput = false;

            if (isEqualKeyInput == true)
            {
                txtExp.Content = "";
                isEqualKeyInput = false;
            }
            if (isOperatorInput == true)
            {
                numBuffer = "0";
                isOperatorInput = false;
            }

            if(numBuffer.Length < 16 && numBuffer != "0")
            {
                numBuffer += String.Format("{0}",num);
            }
            else if(numBuffer.Length < 16 && numBuffer == "0")
            {
                numBuffer = String.Format("{0}", num);
            }
        }
        void CharKeyInput(string key)
        {
            isOneOutOfXInput = false;

            if (key == ".")
            {
                if (isEqualKeyInput == true)
                {
                    txtExp.Content = "";
                    isEqualKeyInput = false;
                }
                if (isOperatorInput == true) // 연산자 입력 후에는 0.으로 초기화
                {
                    numBuffer = "0.";
                    isOperatorInput = false;
                    isNumKeyInput = true;
                }
                if (numBuffer.Length < 16 && !(numBuffer.Contains(".")))
                {
                    numBuffer += ".";
                }
            }
            else if(key == "del")
            {
                if (isEqualKeyInput == true)
                {
                    txtExp.Content = "";
                    isEqualKeyInput = false;
                }
                if (isOperatorInput == true) // 연산자 입력 다음 동작x
                {
                    return;
                }

                if(numBuffer.Length == 2 && numBuffer[0] == '-')
                    numBuffer = "0";
                else if (numBuffer != "0" && numBuffer.Length > 1)
                    numBuffer = numBuffer.Remove(numBuffer.Length - 1);
                else if (numBuffer.Length == 1)
                    numBuffer = "0";
            }
        }
        void IsCalculateReady(string oper) //제일 중요, 출력부 담당.
        {
            if (operatorAri == "/" && numBuffer == "0")
                return;

            isOneOutOfXInput = false;
            if (numBuffer[^1] == '.')//.으로 끝나면 정수로 변환
            {
                numBuffer = numBuffer.Remove(numBuffer.Length - 1);
                txtResult.Content = numBuffer;
            }

            if (firstNum == null && operatorAri == null) //초기 입력
            {
                firstNum = numBuffer;
                operatorAri = oper;
                txtExp.Content = displayExp(firstNum) + " " + operatorAri;
                //연산자 입력 플래그
                isOperatorInput = true;
                isNumKeyInput = false;
            }
            else if(firstNum != null && operatorAri != null && secondNum == null) // 값 대입 후 입력
            {
                if(operatorAri == "=") //ex) 입력이 "123 =" 상태이면 연산자 변경
                {
                    operatorAri = oper;
                    firstNum = numBuffer;
                    txtExp.Content = displayExp(firstNum) + " " + operatorAri;
                    isOperatorInput = true;
                    displayValue();
                    return;
                }

                if(oper == "=")
                {
                    secondNum = numBuffer;
                    txtExp.Content =
                        displayExp(firstNum) + " " + operatorAri+ " " + displayExp(secondNum) + " " + "=";

                    //이후 연산을 수행한다.
                    CalculateRealNum(operatorAri);
                    SaveCalculation();
                    isOperatorInput = true;
                    isEqualKeyInput = true;
                }
                else // +,-,*,/
                {
                    if(isNumKeyInput == false) // 연산자 변경
                    {
                        operatorAri = oper;
                        txtExp.Content = displayExp(firstNum) + " " + operatorAri;
                        numBuffer = firstNum; // CE구현
                        isOperatorInput = true;
                    }
                    else //수를 입력 후 연산자 입력
                    {
                        // 연산 수행 이후 다음 연산 준비
                        secondNum = numBuffer;
                        CalculateRealNum(operatorAri);
                        SaveCalculation();
                        firstNum = numBuffer;
                        operatorAri = oper;
                        txtExp.Content = displayExp(firstNum) + " " + operatorAri;

                        displayValue();
                        secondNum = null;

                        isNumKeyInput = false;
                        isOperatorInput = true;
                    }
                }
            }
            else if(firstNum != null && operatorAri != null && secondNum != null)
            { 
                if(oper == "=")
                {
                    firstNum = numBuffer;
                    numBuffer = secondNum;
                    secondNum = null;
                    IsCalculateReady(oper);
                }
                else
                {
                    firstNum = null;
                    secondNum = null;
                    operatorAri = null;
                    isEqualKeyInput=false;
                    IsCalculateReady(oper);
                }
            }
        }
        void SaveCalculation()
        {
            Calculation temp;
            temp.firstNum = firstNum;
            temp.secondNum = secondNum;
            temp.operatorAri = operatorAri;
            temp.result = numBuffer;

            calList.Add(temp);
        }
        public void PressedButtonInRecord()
        {
            txtExp.Content =
                        displayExp(firstNum) + " " + operatorAri + " " + displayExp(secondNum) + " " + "=";
            displayValue();
        }
        public void PressedMainButtonInMemory()
        {
            txtExp.Content = "";
            displayValue();
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
        void CalculateRealNum(string oper)
        {
            string fNum;
            string sNum;
            string result = "";

            int fIndex = 0;
            int sIndex = 0;
            int Index = 0;
         
            bool minus = false;

            ChangeToInteger(firstNum, out fNum, out fIndex);
            ChangeToInteger(secondNum, out sNum, out sIndex);
            // 1234E-4

            if (oper == "+" || oper == "-")
                if (fIndex > sIndex)
                {
                   while (fIndex != sIndex)
                   {
                       fNum += "0";
                       fIndex--;
                   }
                }
                else
                {
                    while (fIndex != sIndex)
                    {
                        sNum += "0";
                        sIndex--;
                    }
                }

            if(oper == "+" || oper == "-")
            {                    
                result = CalculateInteger(oper,fNum,sNum);

                if (result[0] == '-')
                {
                    result = result.Remove(0, 1);
                    minus = true;
                }

                while (result.Length <= -fIndex) // ex) 1234E-6 -> 0001234
                    result = "0" + result;

                result = result.Insert(result.Length + fIndex, "."); 
                // 7-6 = 1 -> 0.001234

                if (result.Contains(".")) //자리 정리 => ex) 123.,1230.00 -> 123,1230
                {
                    while (result.Length > 1 && result[^1] == '0')
                        result = result.Remove(result.Length - 1,1);
                    if (result[^1] == '.')
                        result = result.Remove(result.Length - 1, 1);
                }

                if (minus)
                    result = "-" + result;
                   
            }
            else if(oper == "*")
            {
                result = CalculateInteger(oper, fNum, sNum);

                if (result[0] == '-')
                {
                    result = result.Remove(0, 1);
                    minus = true;
                }

                if(oper == "*")
                    Index = fIndex + sIndex;

                if (Index >= 0)
                    for (int i = 0; i < Index; i++)
                        result += "0";
                else
                {
                    while (result.Length <= -Index)
                        result = "0" + result;

                    result = result.Insert(result.Length + Index, ".");
                }

                if (result.Contains(".")) //자리 정리 => ex) 123.,1230.00 -> 123,1230
                {
                    while (result.Length > 1 && result[^1] == '0')
                        result = result.Remove(result.Length - 1, 1);
                    if (result[^1] == '.')
                        result = result.Remove(result.Length - 1, 1);
                }

                if (minus)
                    result = "-" + result;
            }
            else if(oper == "/")
            {
                string dNum;
                int dIndex;

                // 나누기 연산후 실수가 나올 수 있음.
                result = CalculateInteger(oper, fNum, sNum);
                ChangeToInteger(result, out dNum, out dIndex);

                result = dNum;

                if (result[0] == '-')
                {
                    result = result.Remove(0, 1);
                    minus = true;
                }

                Index = fIndex - sIndex + dIndex;

                if (Index >= 0)
                    for (int i = 0; i < Index; i++)
                        result += "0";
                else
                {
                    while (result.Length <= -Index)
                        result = "0" + result;

                    result = result.Insert(result.Length + Index, ".");
                }

                if (result.Contains(".")) //자리 정리 => ex) 123.,1230.00 -> 123,1230
                {
                    while (result.Length > 1 && result[^1] == '0')
                        result = result.Remove(result.Length - 1, 1);
                    if (result[^1] == '.')
                        result = result.Remove(result.Length - 1, 1);
                }

                if (minus)
                    result = "-" + result;
            }
                numBuffer = result;
        }
        string CalculateInteger(string oper,string fn,string sn) //두 정수 계산 함수, 양 음수처리 여기서 진행
        {
            try
            {
                if (fn[0] == '-' && sn[0] == '-') // 두 음수
                {
                    if(oper == "+")
                    {
                        // - (A + B)
                        return "-" + Add(fn.Remove(0,1), sn.Remove(0,1));
                    }
                    else if (oper == "-")
                    {
                        // B - A
                        return Subtract(sn.Remove(0,1), fn.Remove(0,1));
                    }
                    else if (oper == "*")
                    {
                        // -A * -B
                        return Multiply(fn.Remove(0,1), sn.Remove(0,1));
                    }
                    else if (oper == "/")
                    {
                        // -A / -B
                        return Divide(fn.Remove(0,1), sn.Remove(0,1));
                    }
                }
                else if (fn[0] == '-' && sn[0] != '-') // 음수,양수
                {
                    if (oper == "+")
                    {
                        // B + -A
                        return Subtract(sn, fn.Remove(0,1));
                    }
                    else if (oper == "-")
                    {
                        // -A - B
                        return "-" + Add(fn.Remove(0, 1), sn);
                    }
                    else if (oper == "*")
                    {
                        // -A * B
                        return "-" + Multiply(fn.Remove(0, 1), sn);
                    }
                    else if (oper == "/")
                    {
                        // -A / B
                        return "-" + Divide(fn.Remove(0, 1), sn);
                    }
                }
                else if(fn[0] != '-' && sn[0] == '-') // 양수,음수
                {
                    if (oper == "+")
                    {
                        // A + -B
                        return Subtract(fn, sn.Remove(0, 1));
                    }
                    else if (oper == "-")
                    {
                        // A - -B => A + B
                        return Add(sn.Remove(0, 1), fn);
                    }
                    else if (oper == "*")
                    {
                        // A * -B
                        return "-" + Multiply(fn, sn.Remove(0, 1));
                    }
                    else if (oper == "/")
                    {
                        //A / -B
                        return "-" + Divide(fn, sn.Remove(0, 1));
                    }
                }
                else // 두 양수
                {
                    if (oper == "+")
                    {
                        return Add(fn, sn);
                    }
                    else if(oper == "-")
                    {
                        return Subtract(fn, sn);
                    }
                    else if(oper == "*")
                    {
                        return Multiply(fn, sn);
                    }
                    else if(oper == "/")
                    {
                        return Divide(fn, sn);
                    }
                }
            }
            catch(Exception e)
            {
                return e.ToString();
            }

            return "error : CalculateInteger()";
        }
        static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
        static string Add(string a, string b)
        {
            string buffer1 = a;
            string buffer2 = b;
            string result = "";

            if (buffer1.Length < buffer2.Length)
            {
                string temp = buffer1;
                buffer1 = buffer2;
                buffer2 = temp;
            }

            buffer1 = Reverse(buffer1);
            buffer2 = Reverse(buffer2);

            int num1, num2, resultNum, carry = 0;

            for (; buffer2.Length > 0;)
            {
                num1 = Int32.Parse(buffer1[0].ToString());
                num2 = Int32.Parse(buffer2[0].ToString());
                resultNum = num1 + num2 + carry;

                if (resultNum > 9)
                {
                    carry = 1;
                    resultNum -= 10;
                }
                else
                {
                    carry = 0;
                }

                result += resultNum.ToString();

                buffer1 = buffer1.Remove(0, 1);
                buffer2 = buffer2.Remove(0, 1);
            }

            for (; buffer1.Length > 0;)
            {
                num1 = Int32.Parse(buffer1[0].ToString());
                resultNum = num1 + carry;

                if (resultNum > 9)
                {
                    carry = 1;
                    resultNum -= 10;
                }
                else
                {
                    carry = 0;
                }

                result += resultNum.ToString();

                buffer1 = buffer1.Remove(0, 1);
            }

            if (carry != 0)
                result += carry.ToString();

            result = Reverse(result);

            while (result.Length > 1 && result[0] == '0')
                result = result.Remove(0, 1);

            return result;
        }
        static string ChangeToTensComplement(string num, int cri) //9의 보수 + 1 = 10의 보수
        {
            string value = Reverse(num);
            string result = "";

            for (int i = 0; i < cri; i++)
            {
                int extractedNum;

                if (value.Length > 0)
                {
                    extractedNum = 9 - Int32.Parse(value[0].ToString());
                    result += extractedNum.ToString();
                    value = value.Remove(0, 1);
                }
                else
                {
                    result += "9";
                }
            }

            result = Reverse(result);
            result = Add(result, "1");

            while (result[0] == '0')
                result = result.Remove(0, 1);

            return result;
        }
        static string Subtract(string a, string b) // A - B => A + 10의보수(B) => 후 처리
        {
            int criterion = a.Length > b.Length ? a.Length : b.Length;
            string result = Add(a, ChangeToTensComplement(b, criterion));

            if (result.Length > criterion)
                result = result.Remove(0, 1);
            else
                result = "-" + ChangeToTensComplement(result, criterion);

            while (result.Length > 1 && result[0] == '0')
                result = result.Remove(0, 1);

            return result;
        }
        static string Multiply(string a, string b) //Karatsuba
        {
            string x = a;
            string y = b;
            string result;

            if (x.Length < y.Length)
            {
                string temp = x;
                x = y;
                y = temp;
            }

            while (x.Length > y.Length)
            {
                y = "0" + y;
            }

            if (x.Length % 2 == 1)
            {
                x = "0" + x;
                y = "0" + y;
            }
            // ex) 1234 * 5678
            string x1 = x.Substring(0, x.Length / 2); //12
            string x0 = x.Substring(x.Length / 2); //34
            string y1 = y.Substring(0, y.Length / 2); //56
            string y0 = y.Substring(y.Length / 2); //78

            string z2, z1, z0;
            if (x.Length > 2 && y.Length > 2)
            // z2 = x1*y1
            // z0 = x0*y0
            // z1 = (x1 + x0)*(y1 + y0) - z2 - z0
            {
                z2 = Multiply(x1, y1);

                z0 = Multiply(x0, y0);

                z1 = Multiply(Add(x1, x0), Add(y1, y0));
                z1 = Subtract(z1, z2);
                z1 = Subtract(z1, z0);
            }
            else
            {
                z2 = (Int32.Parse(x1) * Int32.Parse(y1)).ToString();
                z0 = (Int32.Parse(x0) * Int32.Parse(y0)).ToString();

                z1 = ((Int32.Parse(x1) + Int32.Parse(x0)) *
                    (Int32.Parse(y1) + Int32.Parse(y0))
                    - Int32.Parse(z0) - Int32.Parse(z2)).ToString();
            }

            for (int i = 0; i < x.Length / 2; i++)
                z1 += "0";

            for (int i = 0; i < x.Length; i++)
                z2 += "0";

            result = Add(z2, Add(z0, z1));

            return result;
        }
        static string Divide(string x, string y,int under = 15)
        {
            string result;

            if (x == "0")
                return "0";
            else if (y == "0")
                return "DivideError";

            if (Subtract(x, y)[0] == '-')
            {
                result = "0" + DivideReturnDecimal(x, y);
            }
            else
            {
                string q, r; //quotient, remainder
                DivideReturnInteger(x, y, out q, out r);
                result = q;

                if (r != "0")
                    result += DivideReturnDecimal(r, y);
            }

            return result;
        }
        static void DivideReturnInteger(string x, string y, out string quo, out string rem)
        {
            string dividend = x;
            string divisor = y;

            int lengOfQuotient;
            string quotient = "";

            string quotientFinal = "0";//몫
            string remainder = ""; //나머지

            while (Subtract(dividend, divisor)[0] != '-')
            {
                lengOfQuotient = dividend.Length;

                for (int i = 1; i < dividend.Length + 1; i++)//divide one
                {
                    string dividendBuffer = dividend.Substring(0, i);
                    lengOfQuotient--;

                    if (Subtract(dividendBuffer, divisor)[0] == '-')
                    {
                        continue;
                    }
                    else // 나눌 수 있음
                    {
                        int num = 0;
                        while (dividendBuffer[0] != '-')
                        {
                            dividendBuffer = Subtract(dividendBuffer, divisor);
                            num++;
                        }
                        num--;
                        //몫
                        quotient = num.ToString();

                        for (int j = 0; j < lengOfQuotient; j++)
                        {
                            quotient += "0";
                        }
                        //나머지
                        remainder = Subtract(dividend, Multiply(quotient, divisor));

                        break;
                    }
                }
                while (remainder.Length > 1 && remainder[0] == '0')
                    remainder = remainder.Remove(0, 1);

                quotientFinal = Add(quotientFinal, quotient);
                dividend = remainder;
            }

            quo = quotientFinal;
            rem = remainder;
            //Console.WriteLine($"quotient : {quotientFinal}");
            //Console.WriteLine($"remainder : {remainder}");
        }
        static string DivideReturnDecimal(string x, string y, int under = 15)
        {
            string dividend = x + "0";
            string divisor = y;

            string quotient = ".";
            int count = 0;

            for (int i = 1; i < 100; i++)
            {
                if (Subtract(dividend, divisor)[0] == '-')
                {
                    quotient += "0";
                    dividend += "0";
                }
                else
                {
                    int num = 0;
                    string dividendBefore = "";
                    while (dividend[0] != '-')
                    {
                        dividendBefore = dividend;
                        dividend = Subtract(dividend, divisor);
                        num++;
                    }
                    num--;
                    dividend = dividendBefore + "0";
                    quotient += num.ToString();

                    while (dividend.Length > 1 && dividend[0] == '0')
                        dividend = dividend.Remove(0, 1);

                    count++;
                    if (dividend == "0" || count >= under)
                        break;
                }
            }

            //Console.WriteLine($"quotient : {quotient}");
            return quotient;
        }
        static string BSearch(string num, string low, string high)
        {
            //초기는 해당 값이 들어와야한다.
            //string low = "0";
            //string high = num;

            Func<string, string> checker = (x) =>
            {
                string next = Add(x, "1");
                string result = "";

                if (Subtract(num, Multiply(x, x))[0] != '-')
                    result += "T1";

                if (Subtract(num, Multiply(next, next))[0] == '-')
                    result += "T2";

                return result;
            };

            string mid;
            DivideReturnInteger(Add(low, high), "2", out mid, out string trashCan);

            string result = checker(mid);
            if (result.Equals("T1T2")) //찾음
            {
                return mid;
            }
            else if (result.Equals("T1")) // 찾은 수 보다 높은 값이 필요
            {
                return BSearch(num, Add(mid, "1"), high);
            }
            else if (result.Equals("T2")) // 찾은 수 보다 낮은 값이 필요
            {
                return BSearch(num, low, Subtract(mid, "1"));
            }
            return "error";
        }
        static string SquareRoot(string realNum, int under = 15) //소수점 하나당 * 100
        {
            //input : 정수 output : 실수
            string guessNum = "0"; // 최대 자릿수에 맞춰 근삿값을 찾는다.
            string divisor = "1"; // 분리된 소수점, ex) num = 124.2564 => num : 1,242,564 div : 10000
            Func<string, string> point = (x) => {
                for (int i = 0; i < under; i++)
                    x += "0";
                return x;
            };

            if (realNum[0] == '-')
                return "잘못된 입력입니다.";

            if (realNum.Contains('.'))
            {
                ChangeToInteger(realNum, out realNum, out int expo);
                if (expo < 0)
                    expo = -expo;
                else
                    return "error : SquareRoot -> ChangeToInteger()";

                for (int i = 0; i < expo; i++)
                    divisor += "0";

                divisor = SquareRoot(divisor);
            }

            for (int i = 0; i < under; i++)
                realNum += "00";

            guessNum = BSearch(realNum, "0", realNum);

            // ex) root(123.45) = root(12345) / root(100), 실수 a / b라고 해보자.
            // 실수인 분자 분모를 정수로 바꾼 후 계산

            string numerator = Divide(guessNum, point("1"));
            string denominator = divisor;
            ChangeToInteger(numerator, out numerator, out int expoNum); // 정수로 변환된 a, 자릿수
            ChangeToInteger(denominator, out denominator, out int expoDeno);
            // 정수로 변환된 b, 자릿수
            // (12345/100) / (512345/10) = (12345/100) * (10/512345) = (12345 * 10) / (512345*100)

            for (int i = 0; i < -expoDeno; i++)
                numerator += "0";
            for (int i = 0; i < -expoNum; i++)
                denominator += "0";

            string result = Divide(numerator, denominator);

            return Divide(numerator, denominator);
        }
        void displayValue() //키보드로 입력한 값 출력(포매팅)
        {
            txtResult.FontSize = 25;
            int checker = numBuffer.Length;
            bool minus = false;
            checker = numBuffer.Contains(".") ? --checker : checker;
            checker = numBuffer.Contains("-") ? --checker : checker;

            if (numBuffer[0] == '-')
            {
                minus = true;
                numBuffer = numBuffer.Substring(1);
            }
            if (checker > 16) // 지수표현 포매팅
            {
                string sig;
                int expo;
                ChangeToInteger(numBuffer, out sig, out expo);

                if (sig.Length > 16)
                {
                    expo += sig.Length - 16;
                    sig = sig.Remove(16);
                }
                
                if (expo >= 0) // 정수
                {
                    numBuffer = sig;
                    for (int i = 0; i < expo; i++)
                        numBuffer += "0";
                }
                else // 실수
                {
                    if (sig.Length > -expo) // 
                    {
                        numBuffer = sig.Insert(sig.Length - -expo,".");
                    }
                    else // 유효자리 길이보다 .의 위치가 더 높다. ex) 0.123,0.0123
                    {
                        int numOfZero = -expo - sig.Length;
                        string front = "0.";
                        for (int i = 0; i < numOfZero; i++)
                            front += "0";
                        numBuffer = front + sig;
                    }
                }
                //길이 재검토
                checker = numBuffer.Length;
                checker = numBuffer.Contains(".") ? --checker : checker;
                if(checker <= 16)
                {
                    displayValue();
                    return;
                }


                //오류나서 고쳐야함(아래)
                string sigFigures;

                if (sig.Length < 2)
                    sigFigures = sig;
                else
                    sigFigures = sig[0] + "." + sig.Substring(1);

                expo += sig.Length - 1;
                if (expo == -1)
                    sigFigures = numBuffer;
                else if (expo > 0)
                    sigFigures = sigFigures + "E+" + expo;
                else if (expo < 0)
                    sigFigures = sigFigures + "E" + expo;

                if (minus)
                {
                    numBuffer = "-" + numBuffer;
                    sigFigures = "-" + sigFigures;
                }
                txtResult.Content = sigFigures;
            }
            else // 일반
            {
                if (numBuffer.Contains(".")) //유한 소수
                {
                    string[] temp = numBuffer.Split('.');
                    temp[0] = String.Format("{0:N0}", Int64.Parse(temp[0]));
                    txtResult.Content = temp[0] + "." + temp[1];
                }
                else // 정수
                {
                    string temp = numBuffer;
                    temp = String.Format("{0:N0}", Int64.Parse(temp));
                    txtResult.Content = temp;
                }

                if (minus)
                {
                    numBuffer = "-" + numBuffer;
                    txtResult.Content = "-" + txtResult.Content;
                }
            }
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
        private void Key7_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(7);

            displayValue();
        }

        private void Key8_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(8);

            displayValue();
        }

        private void Key9_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(9);

            displayValue();
        }

        private void Key4_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(4);

            displayValue();
        }

        private void Key5_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(5);

            displayValue();
        }

        private void Key6_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(6);

            displayValue();
        }

        private void Key1_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(1);

            displayValue();
        }

        private void Key2_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(2);

            displayValue();
        }

        private void Key3_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(3);

            displayValue();
        }

        private void Key0_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(0);

            displayValue();
        }

        private void KeyDot_Input(object sender, RoutedEventArgs e)
        {
            CharKeyInput(".");

            displayValue();
        }

        private void KeyDel_Input(object sender, RoutedEventArgs e)
        {
            CharKeyInput("del");

            displayValue();
        }

        private void Calc_Plus(object sender, RoutedEventArgs e)
        {
            IsCalculateReady("+");
        }

        private void Calc_Minus(object sender, RoutedEventArgs e)
        {
            IsCalculateReady("-");
        }

        private void Calc_Mul(object sender, RoutedEventArgs e)
        {
            IsCalculateReady("*");
        }

        private void Calc_Div(object sender, RoutedEventArgs e)
        {
                IsCalculateReady("/");
        }

        private void ReturnResult(object sender, RoutedEventArgs e)
        {
            IsCalculateReady("=");
            displayValue();
        }

        private void ChangeMark(object sender, RoutedEventArgs e) 
            // = 연산 이후,연산자 입력 후 negate() 구현?
        {
            if (numBuffer == "0" || numBuffer == "0.")
                return;

            isOneOutOfXInput = false;

            if (isEqualKeyInput == true)
            {
                txtExp.Content = "";
                isEqualKeyInput = false;
            }

            if (numBuffer[0] == '-')
            {
                numBuffer = numBuffer.Remove(0, 1);
                displayValue();
            }
            else
            {
                numBuffer = "-" + numBuffer;
                displayValue();
            }
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            numBuffer = "0";

            firstNum = null;
            secondNum = null;
            operatorAri = null;

            isOperatorInput = false;
            isNumKeyInput = false;
            isEqualKeyInput = false;
            isOneOutOfXInput = false;

            displayValue();
            txtExp.Content = "";
        }

        private void ClearEntry(object sender, RoutedEventArgs e)
        {
            isOneOutOfXInput = false;

            if (firstNum == null && operatorAri == null) //초기 입력
            {
                numBuffer = "0";
                displayValue();

            }
            else if (firstNum != null && operatorAri != null && secondNum == null) // 값 대입 후 입력
            {
                numBuffer = "0";
                displayValue();
                isNumKeyInput = false;
            }
            else if (firstNum != null && operatorAri != null && secondNum != null)
            {
                txtExp.Content = "";
                numBuffer = "0";
                displayValue();
            }
        }

        private void SquareRoot(object sender, RoutedEventArgs e)
        {
            if (numBuffer == "0")
                return;

            if (isEqualKeyInput == true)
            {
                txtExp.Content = "";
                isEqualKeyInput = false;
            }
            isOneOutOfXInput = false;
            isOperatorInput = true;
            //너무 느리네..

            if (numBuffer[0] == '-')
            {
                return;
            }
            else
            {
                numBuffer = SquareRoot(numBuffer);
                displayValue();
            }
        }

        private void OneOutOfX(object sender, RoutedEventArgs e)
        {
            //보정 버퍼 있음
            if (numBuffer == "0")
                return;

            if (isEqualKeyInput == true)
            {
                txtExp.Content = "";
                isEqualKeyInput = false;
            }

            isOperatorInput = true;

            if (isOneOutOfXInput == true)
            {
                string temp = numBuffer;
                numBuffer = correction;
                correction = temp;
            }
            else
            {
                correction = numBuffer;

                string firstBuffer = firstNum;
                string secondBuffer = secondNum;
                firstNum = "1";
                secondNum = numBuffer;
                CalculateRealNum("/");
                firstNum = firstBuffer;
                secondNum = secondBuffer;
                isOneOutOfXInput = true;
            }          
            displayValue();
        }

        private void Square(object sender, RoutedEventArgs e)
        {
            if (numBuffer == "0")
                return;

            if (isEqualKeyInput == true)
            {
                txtExp.Content = "";
                isEqualKeyInput = false;
            }
            isOneOutOfXInput = false;
            isOperatorInput = true;

            string firstBuffer = firstNum;
            string secondBuffer = secondNum;
            firstNum = numBuffer;
            secondNum = numBuffer;
            CalculateRealNum("*");
            firstNum = firstBuffer;
            secondNum = secondBuffer;

            displayValue();
        }

        private void Percent(object sender, RoutedEventArgs e)
        {
            // 원래 기능이랑 약간 다르다.
            if (numBuffer == "0")
                return;

            isOneOutOfXInput = false;
            if (isEqualKeyInput == true)
            {
                txtExp.Content = "";
                isEqualKeyInput = false;
            }

            isOneOutOfXInput = false;
            isOperatorInput = true;

            string firstBuffer = firstNum;
            string secondBuffer = secondNum;
            firstNum = numBuffer;
            secondNum = "100";
            CalculateRealNum("/");
            firstNum = firstBuffer;
            secondNum = secondBuffer;

            displayValue();
        }

        private void PageOnRecord(object sender, RoutedEventArgs e)
        {
            Record recordPage = new Record();
            recordPage.Owner = this;
            recordPage.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            recordPage.pressButton += new EventHandler(PressedButtonInRecord);
            recordPage.ShowDialog();
        }

        public void MemoryCalculateEvent(string oper,int memIndex)
        {
            //메모리,버퍼 연산 -> 다시 그 자리에 저장
            string fNum;
            string sNum;
            string result = "";

            int fIndex = 0;
            int sIndex = 0;


            bool minus = false;

            ChangeToInteger(memory[memIndex], out fNum, out fIndex);
            ChangeToInteger(numBuffer, out sNum, out sIndex);
            // 1234E-4

            if (fIndex > sIndex)
            {
                while (fIndex != sIndex)
                {
                    fNum += "0";
                    fIndex--;
                }
            }
            else
            {
                while (fIndex != sIndex)
                {
                    sNum += "0";
                    sIndex--;
                }
            }

            result = CalculateInteger(oper, fNum, sNum); //정수 합or차

            if (result[0] == '-')
            {
                result = result.Remove(0, 1);
                minus = true;
            }

            while (result.Length <= -fIndex) // ex) 1234E-6 -> 0001234
                result = "0" + result;

            result = result.Insert(result.Length + fIndex, ".");
            // 7-6 = 1 -> 0.001234

            if (result.Contains(".")) //자리 정리 => ex) 123.,1230.00 -> 123,1230
            {
                while (result.Length > 1 && result[^1] == '0')
                    result = result.Remove(result.Length - 1, 1);
                if (result[^1] == '.')
                    result = result.Remove(result.Length - 1, 1);
            }

            if (minus)
                result = "-" + result;

            memory[memIndex] = result;
        }
        public void isMemoryClear()
        {
            if(memory.Count == 0)
            {
                btnMC.IsEnabled = false;
                btnMR.IsEnabled = false;
                btnMRec.IsEnabled = false;
            }
        }
        //Memory
        private void MemorySave(object sender, RoutedEventArgs e)
        {
            memory.Add(numBuffer);
            if(memory.Count > 0)
            {
                btnMC.IsEnabled = true;
                btnMR.IsEnabled = true;
                btnMRec.IsEnabled = true;
            }
        }

        private void MemoryPlus(object sender, RoutedEventArgs e)
        {
            if (memory.Count > 0)
            {
                string fNum;
                string sNum;
                string result = "";

                int fIndex = 0;
                int sIndex = 0;

                bool minus = false;

                ChangeToInteger(numBuffer, out fNum, out fIndex);
                ChangeToInteger(memory[memory.Count - 1], out sNum, out sIndex);
                // 1234E-4

                if (fIndex > sIndex)
                {
                    while (fIndex != sIndex)
                    {
                        fNum += "0";
                        fIndex--;
                    }
                }
                else
                {
                    while (fIndex != sIndex)
                    {
                        sNum += "0";
                        sIndex--;
                    }
                }

                result = CalculateInteger("+", fNum, sNum);

                if (result[0] == '-')
                {
                    result = result.Remove(0, 1);
                    minus = true;
                }

                while (result.Length <= -fIndex) // ex) 1234E-6 -> 0001234
                    result = "0" + result;

                result = result.Insert(result.Length + fIndex, ".");
                // 7-6 = 1 -> 0.001234

                if (result.Contains(".")) //자리 정리 => ex) 123.,1230.00 -> 123,1230
                {
                    while (result.Length > 1 && result[^1] == '0')
                        result = result.Remove(result.Length - 1, 1);
                    if (result[^1] == '.')
                        result = result.Remove(result.Length - 1, 1);
                }

                if (minus)
                    result = "-" + result;

                memory[memory.Count - 1] = result;
            }
            else
            {
                memory.Add(numBuffer);
            }

            if (memory.Count > 0)
            {
                btnMC.IsEnabled = true;
                btnMR.IsEnabled = true;
                btnMRec.IsEnabled = true;
            }
        }

        private void MemoryMinus(object sender, RoutedEventArgs e)
        {
            if (memory.Count > 0)
            {
                if (memory.Count > 0)
                {
                    string fNum;
                    string sNum;
                    string result = "";

                    int fIndex = 0;
                    int sIndex = 0;

                    bool minus = false;

                    ChangeToInteger(memory[memory.Count - 1], out fNum, out fIndex);
                    ChangeToInteger(numBuffer, out sNum, out sIndex);
                    // 1234E-4

                    if (fIndex > sIndex)
                    {
                        while (fIndex != sIndex)
                        {
                            fNum += "0";
                            fIndex--;
                        }
                    }
                    else
                    {
                        while (fIndex != sIndex)
                        {
                            sNum += "0";
                            sIndex--;
                        }
                    }

                    result = CalculateInteger("-", fNum, sNum);

                    if (result[0] == '-')
                    {
                        result = result.Remove(0, 1);
                        minus = true;
                    }

                    while (result.Length <= -fIndex) // ex) 1234E-6 -> 0001234
                        result = "0" + result;

                    result = result.Insert(result.Length + fIndex, ".");
                    // 7-6 = 1 -> 0.001234

                    if (result.Contains(".")) //자리 정리 => ex) 123.,1230.00 -> 123,1230
                    {
                        while (result.Length > 1 && result[^1] == '0')
                            result = result.Remove(result.Length - 1, 1);
                        if (result[^1] == '.')
                            result = result.Remove(result.Length - 1, 1);
                    }

                    if (minus)
                        result = "-" + result;

                    memory[memory.Count - 1] = result;
                }
                else
                {
                    memory.Add(numBuffer);
                }
            }
            else
            {
                if (numBuffer[0] == '-')
                {
                    memory.Add(numBuffer.Substring(1));
                }
                else
                {
                    memory.Add("-" + numBuffer);
                }
            }

            if (memory.Count > 0)
            {
                btnMC.IsEnabled = true;
                btnMR.IsEnabled = true;
                btnMRec.IsEnabled = true;
            }
        }

        private void MemoryRead(object sender, RoutedEventArgs e)
        {
            numBuffer = memory[memory.Count - 1];
            displayValue();
        }

        private void MemoryClear(object sender, RoutedEventArgs e)
        {
            memory = new List<string>();
            btnMC.IsEnabled = false;
            btnMR.IsEnabled = false;
            btnMRec.IsEnabled = false;
        }

        private void MemoryList(object sender, RoutedEventArgs e)
        {
            Memory memory = new Memory();
            memory.Owner = this;
            memory.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            memory.pressButton += new EventHandler(PressedMainButtonInMemory);
            memory.pressCalButton += new MemoryEventHandler(MemoryCalculateEvent);
            memory.clear += new EventHandler(isMemoryClear);
            memory.ShowDialog();
        }

        private void ChangeToExchangeRate(object sender, RoutedEventArgs e)
        {
            exrate.Visibility = Visibility.Visible;
            std.Visibility = Visibility.Hidden;
            Record.Visibility = Visibility.Hidden;
            Type.Content = "환율";
        }

        private void ChangeToStandard(object sender, RoutedEventArgs e)
        {
            std.Visibility = Visibility.Visible;
            exrate.Visibility = Visibility.Hidden;
            Record.Visibility = Visibility.Visible;
            Type.Content = "표준";
        }
    }
}
