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

using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Calculator
{
    /// <summary>
    /// ExchangeRate.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ExchangeRate : Page
    {
        // 한국수출입은행 OpenAPI 인증키
        string privateKey = "aJxGgsHIYcpstkZbIyxLGRVI09EekyG9";

        string item1Value = "0";
        string item2Value = "0";
        
        // 환율
        decimal[] itemRate = {1, 1};

        // RequestJsonData()에서 파싱
        List<string> dataIndex;
        List<string> unitIndex;
        Dictionary<string, decimal> exchangeData; // [국가,통화명], 환율(원 단위)

        // 선택 항목
        bool cursorOnItem1 = true;

        //선택 항목 변경 이벤트 관리
        bool isInputChanged = false; // 콤보 박스 or 선택 항목 바뀜
        public ExchangeRate()
        {
            InitializeComponent();
            Item1.FontWeight = FontWeights.Bold;
            UpdateExData(true);
        }

        private void NumKeyInput(int num)
            // 숫자 입력
        {
            if(isInputChanged)
            {
                if (cursorOnItem1)
                    item1Value = "0";
                else
                    item2Value = "0";

                isInputChanged = false;
            }

            if(cursorOnItem1 == true)
                // Item1
            {
                if (item1Value.Contains("."))
                    if (item1Value.Split(".")[^1].Length >= 2)
                        return;

                if (item1Value.Length < 15 && item1Value != "0")
                {
                    item1Value += String.Format("{0}", num);
                }
                else if (item1Value.Length < 15 && item1Value == "0")
                {
                    item1Value = String.Format("{0}", num);
                }


                // 입력 후 환율에 따른 Item2값 변경
                item2Value = CalculateExchangeRate(item1Value, cursorOnItem1);
            }
            else
                // Item2
            {
                if (item2Value.Contains("."))
                    if (item2Value.Split(".")[^1].Length >= 2)
                        return;

                if (item2Value.Length < 15 && item2Value != "0")
                {
                    item2Value += String.Format("{0}", num);
                }
                else if (item2Value.Length < 15 && item2Value == "0")
                {
                    item2Value = String.Format("{0}", num);
                }

                // 입력 후 환율에 따른 Item1값 변경
                item1Value = CalculateExchangeRate(item2Value, cursorOnItem1);
            }

            DisplayValue();
        }
        private void UpdateExData(bool init)
            // 환율 정보 업데이트
        {
            // combobox 항목 사용 중에는 제거 불가능.
            //cbx1.Items.Clear();
            //cbx2.Items.Clear();

            if(RequestJsonData(out JArray? jArray, out string message) == true && jArray != null)
            {
                dataIndex = new List<string>();
                unitIndex = new List<string>();
                exchangeData = new Dictionary<string, decimal>();

                foreach (var item in jArray.Children())
                    // 국가명,통화명,매매기준율
                {
                    dataIndex.Add(item["cur_nm"].Value<string>()); 
                    unitIndex.Add(item["cur_unit"].Value<string>());                    
                    exchangeData.Add(item["cur_nm"].Value<string>(), item["deal_bas_r"].Value<decimal>());
                }
                cbx1.ItemsSource = dataIndex;
                cbx2.ItemsSource = dataIndex;

                DateTime date = DateTime.Now;
                updateTimeLB.Content = "업데이트됨 " + date.ToString("yyyy-MM-dd tt hh:mm:ss");
                IsCursorOn();

                if (init)
                {
                    if(dataIndex.Count >= 23)
                    {
                        cbx1.SelectedIndex = 13;
                        cbx2.SelectedIndex = 22;
                    }
                }
            }
            else
                // 문제 발생
            {
                curUnitLB1.Content = "";
                curUnitLB2.Content = "";
                exchangeRateLB.Content = "";
                updateTimeLB.Content = message;
            }
        }
        private void UpdateRateData(string curNM, int index,bool isItem1)
            // 선택 통화 업데이트
        {
            if(isItem1)
            {
                // 환율 업데이트
                itemRate[1] = exchangeData[curNM];

                // 통화 마크
                curUnitLB1.Content = unitIndex[index];
            }
            else
            {
                itemRate[0] = exchangeData[curNM];
                curUnitLB2.Content = unitIndex[index];
            }

            IsCursorOn();
        }
        private bool RequestJsonData(out JArray? result, out string message)
            // Json
        {
            try
            {
                DateTime date = DateTime.Now;               

                string url =
                    "https://www.koreaexim.go.kr/site/program/financial/exchangeJSON" +
                    $"?authkey={privateKey}&searchdate={date.ToString("yyyyMMdd")}&data=AP01";

                // 요청 패킷 제작
                HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url);

                // 요청(연결)
                using (HttpWebResponse hwrResult = hwr.GetResponse() as HttpWebResponse)
                {
                    using (Stream sr = hwrResult.GetResponseStream())
                    {
                        using (StreamReader srd = new StreamReader(sr))
                        {
                            string text = srd.ReadToEnd();
                            JArray jArray = JArray.Parse(text); // jarray ,jobject

                            foreach (var item in jArray.Children())
                            {
                                int? responseResult = item["result"].Value<Int32>();
                                if (responseResult == 1)
                                {
                                    result = jArray;
                                    message = "success";
                                    return true;
                                }
                                else
                                {
                                    result = null;
                                    // 1 : 성공, 2 : DATA코드 오류, 3 : 인증코드 오류, 4 : 일일제한횟수 마감
                                    if (responseResult == 2)
                                        message = "잘못된 호출입니다."; // 잘못된 호출(url형식 문제)
                                    else if (responseResult == 3)
                                        message = "인증 키 문제가 발생하였습니다."; // 인증 오류
                                    else if (responseResult == 4)
                                        message = "일일제한횟수가 마감되었습니다."; // 일일 호출 제한
                                    else
                                        message = "unknown";

                                    return false;
                                }
                            }
                        }
                    }                
                }
                result = null;
                message = "null";
                return false;
            }
            catch(Exception e)
            {
                result = null;
                message = e.Message;
                return false;
            }    
        }
        private string CalculateExchangeRate(string item, bool isItem1)
            // 입력한 값에 대한 환율 계산
        {
            decimal otherValue = decimal.Parse(item);
            
            if(isItem1)
            {
                otherValue = otherValue * itemRate[1] / itemRate[0];
            }
            else
            {
                otherValue = otherValue * itemRate[0] / itemRate[1];
            }

            return otherValue.ToString();
        }
        private void DisplayValue()
            // item1,2의 출력할 포맷을 관리
        {
            // string.Format("{0:N2}",decimal.Parse(item1Value);

            if (item1Value.Contains('.'))
            {
                var temp = item1Value.Split('.');
                Item1.Text = String.Format("{0:N0}", decimal.Parse(temp[0])) + ".";

                int valid = 0;
                for(int i = 0; i < temp[1].Length; i++)
                {
                    if (temp[1][i] != '0')
                    {
                        valid += 2;
                        break;
                    }
                    valid++;
                }

                if (temp[1].Length >= valid)
                    Item1.Text += temp[1].Substring(0, valid);
                else
                    Item1.Text += temp[1];
            }
            else
            {
                Item1.Text = string.Format("{0:N0}", decimal.Parse(item1Value));
            }

            if (item2Value.Contains('.'))
            {
                var temp = item2Value.Split('.');
                Item2.Text = string.Format("{0:N0}", decimal.Parse(temp[0])) + ".";

                int valid = 0;
                for (int i = 0; i < temp[1].Length; i++)
                {
                    if (temp[1][i] != '0')
                    {
                        valid += 2;
                        break;
                    }
                    valid++;
                }

                if (temp[1].Length >= valid)
                    Item2.Text += temp[1].Substring(0, valid);
                else
                    Item2.Text += temp[1];
            }
            else
                Item2.Text = string.Format("{0:N0}", decimal.Parse(item2Value));
        }
        private void IsCursorOn()
            // 선택한 항목 bold체, 비율 표시
        {
            if(cursorOnItem1)
            {
                Item1.FontWeight = FontWeights.Bold;
                Item2.FontWeight = FontWeights.Light;

                // 유효 3번째까지 표시
                int valid = 0;
                var temp = (itemRate[1] / itemRate[0]).ToString().Split('.')[^1];
                for(int x = 0;x< temp.Length;x++)
                {
                    if (temp[x] != '0')
                    {
                        valid += 3;
                        break;
                    }
                    valid++;
                }

                string format = "{0:F" + valid + "}";
                exchangeRateLB.Content = "1 " + curUnitLB1.Content + " = " +
                   curUnitLB2.Content + " " + string.Format(format, itemRate[1] / itemRate[0]);
            }
            else
            {
                Item1.FontWeight = FontWeights.Light;
                Item2.FontWeight = FontWeights.Bold;

                // 유효 3번째까지 표시
                int valid = 0;
                var temp = (itemRate[0] / itemRate[1]).ToString().Split('.')[^1];
                for (int x = 0; x < temp.Length; x++)
                {
                    if (temp[x] != '0')
                    {
                        valid += 3;
                        break;
                    }
                    valid++;
                }

                string format = "{0:F" + valid + "}";
                exchangeRateLB.Content = "1 " + curUnitLB2.Content + " = " +
                   curUnitLB1.Content + " " + string.Format(format, itemRate[0] / itemRate[1]);
            }
        }
        private void MouseDownItem1(object sender, MouseButtonEventArgs e)
        {
            if (cursorOnItem1 == false)
                isInputChanged = true;
            cursorOnItem1 = true;
            IsCursorOn();
        }

        private void MouseDownItem2(object sender, MouseButtonEventArgs e)
        {
            if (cursorOnItem1 == true)
                isInputChanged = true;
            cursorOnItem1 = false;
            IsCursorOn();
        }

        private void Key0_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(0);
        }

        private void Key1_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(1);
        }

        private void Key2_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(2);
        }

        private void Key3_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(3);
        }

        private void Key4_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(4);
        }

        private void Key5_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(5);
        }

        private void Key6_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(6);
        }

        private void Key7_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(7);
        }

        private void Key8_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(8);
        }

        private void Key9_Input(object sender, RoutedEventArgs e)
        {
            NumKeyInput(9);
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            item1Value = "0";
            item2Value = "0";
            DisplayValue();
        }

        private void EraseNum(object sender, RoutedEventArgs e)
        {
            if(cursorOnItem1)
            {
                if(item1Value == "0" || item1Value.Length == 1)
                {
                    item1Value = "0";
                }
                else
                {
                    item1Value = item1Value.Substring(0, item1Value.Length - 1);
                    if (item1Value[^1].Equals('.'))
                        item1Value = item1Value.Substring(0, item1Value.Length - 1);
                }
                item2Value = CalculateExchangeRate(item1Value, cursorOnItem1);
            }
            else
            {
                if (item2Value == "0" || item2Value.Length == 1)
                {
                    item2Value = "0";
                }
                else
                {
                    item2Value = item2Value.Substring(0, item2Value.Length - 1);
                    if (item2Value[^1].Equals('.'))
                        item2Value = item2Value.Substring(0, item2Value.Length - 1);
                }
                item1Value = CalculateExchangeRate(item2Value, cursorOnItem1);
            }

            DisplayValue();
        }

        private void cbx1SelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox currentComboBox = sender as ComboBox;
            if (currentComboBox != null)
            {               
                UpdateRateData(currentComboBox.SelectedValue.ToString(), currentComboBox.SelectedIndex,true);
            }
        }
        private void cbx2SelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox currentComboBox = sender as ComboBox;
            if (currentComboBox != null)
            {
                UpdateRateData(currentComboBox.SelectedValue.ToString(), currentComboBox.SelectedIndex, false);
            }
        }
        private void KeyDot_Input(object sender, RoutedEventArgs e)
        {
            if(cursorOnItem1)
            {
                if (item1Value.Contains("."))
                    return;
                else
                {
                    item1Value += ".";
                    Item1.Text += ".";
                }
            }
            else
            {
                if (item2Value.Contains("."))
                    return;
                else
                {
                    item2Value += ".";
                    Item2.Text += ".";
                }
            }
        }
        private void UpdateButton(object sender, RoutedEventArgs e)
        {
            UpdateExData(false);
        }
    }
}
