/*********************************************************************
 *
 *   @ 현대증권 ExpertPlus C# Sample Source
 *   
 *   최초 작성일 : 2014.11.19
 *   
 * 
 *   참조 DLL : .\Cyber21Plus\ExpertPlus\YFExpertPlus.dll
 *   
 *   객체 및 함수구현 참고문서    : .\Cyber21Plus\ExpertPlus\Help\YouFirst ExpertPlus Objct 및 함수에 대한 설명.pdf
 *   TR Code 및 형식구현 참조문서 : .\Cyber21Plus\ExpertPlus\Help\서비스 TR 입출력 형식.pdf
 *  
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using YFExpertPlus;  // ExpertPlus Interface 사용 (참조에 YFExpertPlus.dll를 등록하고 사용)

namespace ExpertPlus_Sample
{
    public partial class Form1 : Form
    {
        // (중요) 외부 프로그램을 시작전 반드시 HTS ExpertPlus에 로그인을 하시고 실행하십시오.

        private static readonly Dictionary<int, string> TRCodeListDic = new Dictionary<int, string>
        {
            {0, "TL0001"}, {1, "TL0002"}, {2, "TL0003"}, 
            {3, "TL0004"}, {4, "TL0005"}, {5, "TL0006"}, 
            {6, "TL0007"}, {7, "TL1001"}
        };

        private static readonly Dictionary<int, Dictionary<string, int>> TRGubunDic = new Dictionary<int, Dictionary<string, int>>
        {
            {0, new Dictionary<string, int>{{"All", 9}}}, {1, new Dictionary<string, int>{{"KOSPI", 0}}},
            {2, new Dictionary<string, int>{{"KOSDAG", 1}}}, {3, new Dictionary<string, int>{{"ETF", 2}}}
        };

        private static readonly Dictionary<int, string> TROrderTypeDic = new Dictionary<int, string>
        {
            {0, "지정가"}, {1, "시장가"}, {2, "조건부지정가"}, 
            {3, "시간외종가"}, {4, "최유리지정가"}, {5, "최우선지정가"}
        };

        private static readonly Dictionary<int, Dictionary<string, string>> ExchCCodeDic = new Dictionary<int, Dictionary<string, string>>
        {
            {0, new Dictionary<string, string>{{"USD", "0"}}}, {1, new Dictionary<string, string>{{"CNY", "8"}}}, {2, new Dictionary<string, string>{{"HKD", "3"}}}
        };

        public YFRequestData yfData = new YFRequestData();        // 조회성 정보를 받기 위한 객체
        public YFRequestData yfUnderData = new YFRequestData();   // 종목시세를 받기 위한 객체
        public YFRequestData yfAccountData = new YFRequestData(); // 계좌 정보를 받기 위한 객체
        
        public YFRequestData yfOrderListData = new YFRequestData();     


        public YFOrder yfOrderData = new YFOrder();   // 주식 주문을 위한 객체

        public YFReal yfRealData = new YFReal();                  // 시세 Real 데이터를 받기 위한 객체
        public YFReal yfAccountRealData = new YFReal();           // 계좌등록 Real 데이터를 받기 위한 객체

        public YFValueList yfValueList = new YFValueList();       // 종목리스트, 주문내역 등 리스트 데이터를 테이블 형태로 보관하기 위한 객체
        public YFValues yfValues = new YFValues();                // 현재가 실시간 데이터 등 단일 조회 데이터를 테이블 형태로 보관하기 위한 객체
        public YFValues yfAccountValues = new YFValues();

        float? fLast, fChange, fChgRate = 0;
        double? dDeposit, dAvailWithdrAmt, dCashAvailOrd = 0;
        public string trcode = null;

        public List<string> RealAccountList = new List<string>();

        System.Collections.ArrayList orderNumList = new System.Collections.ArrayList();
        System.Collections.ArrayList orderCodeList = new System.Collections.ArrayList();

        public Form1()
        {
            InitializeComponent();

            YFAllInit(); // 초기화
        }

        public void YFAllInit()
        {
            yfData.ComInit(); //COM DLL 초기화 함수( 처음 화면 로딩 시 반드시 호출 필요)
            yfData.GSComInit(0); //COM DLL 초기화 함수( 처음 화면 로딩 시 반드시 호출 필요)
            yfData.ReceiveData += new IYFRequestDataEvents_ReceiveDataEventHandler(yfData_ReceiveData); // 서버에 요청한 값을 받기 위한 EventHandler 등록

            yfUnderData.ComInit(); //COM DLL 초기화 함수
            yfUnderData.ReceiveData += new IYFRequestDataEvents_ReceiveDataEventHandler(yfUnderData_ReceiveData); // 시세 데이터를 받기 위한 EventHandler 등록

            yfAccountData.ComInit(); //COM DLL 초기화 함수
            yfAccountData.ReceiveData += new IYFRequestDataEvents_ReceiveDataEventHandler(yfAccountData_ReceiveData); // 계좌 데이터를 받기 위한 EventHandler 등록

            
            yfOrderListData.ComInit(); //COM DLL 초기화 함수
            yfOrderListData.ReceiveData += new IYFRequestDataEvents_ReceiveDataEventHandler(yfOrderListData_ReceiveData); // 계좌 데이터를 받기 위한 EventHandler 등록

            yfOrderData.ReceiveData += new IYFOrderEvents_ReceiveDataEventHandler(yfOrderData_ReceiveData);

            // Real Data를 받기위한 객체(YFReal Class)는 초기화 하지 않아도 됨
            yfRealData.ReceiveData += new IYFRealEvents_ReceiveDataEventHandler(yfRealData_ReceiveData); // Real 데이터를 받기 위한 EventHandler 등록

            yfAccountRealData.ReceiveData += new IYFRealEvents_ReceiveDataEventHandler(yfAccountRealData_ReceiveData);

            for (int i = 0; i < TRCodeListDic.Count; i++) {
                comboBox1.Items.Add(TRCodeListDic[i]);
            }

            for (int i = 0; i < TRGubunDic.Count; i++) {
                comboBox2.Items.Add(TRGubunDic[i].Keys.First());
            }

            for (int i = 0; i < yfAccountData.AccountCount(); i++) {
                comboBox3.Items.Add(yfAccountData.AccountItem(i) as string);
            }

            for (int i = 0; i < TROrderTypeDic.Count; i++) {
                comboBox4.Items.Add(string.Format("{0} : {1}", i + 1, TROrderTypeDic[i]));
            }

            for (int i = 0; i < ExchCCodeDic.Count; i++)
            {
                comboBox5.Items.Add(ExchCCodeDic[i].Keys.First());
            }

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0;
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            trcode = comboBox1.SelectedItem as string;

            yfData.RequestInit();

            if (comboBox1.SelectedIndex == 0) {  // 일반주식 종목리스트 일 경우에 MarketType 입력
                yfData.SetData("MarketType", TRGubunDic[comboBox2.SelectedIndex][comboBox2.SelectedItem as string]);
            }
            else if (comboBox1.SelectedIndex == 7)
            {
                yfData.SetData("Type", System.Convert.ToSingle(TRGubunDic[comboBox2.SelectedIndex][comboBox2.SelectedItem as string]));
            }
            // TR 요청 메서드
            yfData.RequestData(comboBox1.SelectedItem as string, 0);
            listBox1.Items.Add(string.Format("Start"));
        }
        
        private void yfData_ReceiveData(string TrCode, string Value, string ValueList, int NextFlag, int SelectCount, string MsgCode, string MsgName)
        {
            listBox1.Items.Add(string.Format("End"));
            // yfData Receive Event발생
            listBox1.Items.Add(string.Format("Data -> TrCode : [{0}], Value : {1}", TrCode, ValueList.Substring(0, 90).Trim()));
            listBox2.Items.Add(string.Format("TrCode : [{0}], MsgCode : [{1}], MsgName : [{2}]", TrCode, MsgCode, MsgName));
            
            listboxscrolldown(listBox1);
            listboxscrolldown(listBox2);

            if (TrCode == "TO7001")
            {
                yfValues.SetValueData(yfData.GetKorValueHeader(TrCode), Value);
                textBox16.Text = System.Convert.ToString(yfValues.GetValue(1));
                listBox2.Items.Add(string.Format("환율조회 -> TrCode : [{0}], MsgCode : [{1}], MsgName : [{2}]", TrCode, MsgCode, MsgName));
                return;
            }

            int i, j = 0;

            // ReceiveData Setting -> 반복구문의 데이터는 ValueList에 저장됨
            yfValueList.SetListData(yfData.GetKorValueListHeader(TrCode), ValueList, SelectCount-1);

            dataGridView1.RowCount = yfValueList.RowCount();
            dataGridView1.ColumnCount = yfValueList.ColCount();

            // Column Header Setting
            for (i = 0; i < yfValueList.ColCount(); i++) {
                dataGridView1.Columns[i].Name = yfValueList.GetColName(i) as string;
            }

            // Data Setting
            yfValueList.RowFirst();

            for (i = 0; i < yfValueList.RowCount(); i++) {
                for (j = 0; j < yfValueList.ColCount(); j++) {
                    dataGridView1[j, i].Value = (yfValueList.GetRowDataCell(j) as string).Trim();
                }

                yfValueList.RowNext();
            }          
        }

        private void yfUnderData_ReceiveData(string TrCode, string Value, string ValueList, int NextFlag, int SelectCount, string MsgCode, string MsgName)
        {
            listBox1.Items.Add(string.Format("Data -> TrCode : [{0}], Value : {1}", TrCode, Value));
            listBox2.Items.Add(string.Format("TrCode : [{0}], MsgCode : [{1}], MsgName : [{2}]", TrCode, MsgCode, MsgName, Value));

            listboxscrolldown(listBox1);
            listboxscrolldown(listBox2);

            if (TrCode == "TQ2101") {
                // 반복구문이 아닌 데이터는 Value에 저장됨
                yfValues.SetValueData(yfUnderData.GetKorValueHeader("TQ2101"), Value);

                // GetValue로 받아오는 데이터는 전부 Object형식이므로 무조건 형변환을 해주어야 함
                // TR 입출력 형식에서 Type이 Float일 경우에 숫자가 작을경우 float로 형변환, 숫자가 클경우에는 double로 형변환을 해주어야 함
                fLast = System.Convert.ToSingle(yfValues.GetValue(18));
                fChange = System.Convert.ToSingle(yfValues.GetValue(19));
                fChgRate = System.Convert.ToSingle(yfValues.GetValue(20));

                textBox2.Text = string.Format("{0:#,0.00}", fLast);
                textBox3.Text = string.Format("{0:#,0.00}", fChange);
                textBox4.Text = string.Format("{0:0.00}%", fChgRate);

                textBox14.Text = string.Format("{0:#,0.00}", fLast);
            } else {
                yfValues.SetValueData(yfUnderData.GetKorValueHeader("TQ1001"), Value);

                fLast = System.Convert.ToSingle(yfValues.GetValue(27));
                fChange = System.Convert.ToSingle(yfValues.GetValue(32));
                fChgRate = System.Convert.ToSingle(yfValues.GetValue(33));

                textBox2.Text = string.Format("{0:#,0}", fLast);
                textBox3.Text = string.Format("{0:#,0}", fChange);
                textBox4.Text = string.Format("{0:0.00}%", fChgRate);

                numericUpDown2.Value = System.Convert.ToDecimal(fLast);
            }
        }

        private void yfAccountData_ReceiveData(string TrCode, string Value, string ValueList, int NextFlag, int SelectCount, string MsgCode, string MsgName)
        {
            listBox1.Items.Add(string.Format("Data -> TrCode : [{0}], Value : {1}", TrCode, Value));
            listBox2.Items.Add(string.Format("TrCode : [{0}], MsgCode : [{1}], MsgName : [{2}]", TrCode, MsgCode, MsgName));

            listboxscrolldown(listBox1);
            listboxscrolldown(listBox2);

            if (TrCode == "TA2006") { // 선물옵션 예수금
                yfAccountValues.SetValueData(yfAccountData.GetKorValueHeader("TA2006"), Value);

                dDeposit = System.Convert.ToDouble(yfAccountValues.GetValue(2));
                dAvailWithdrAmt = System.Convert.ToDouble(yfAccountValues.GetValue(8));
                dCashAvailOrd = System.Convert.ToDouble(yfAccountValues.GetValue(6));

                textBox6.Text = string.Format("{0:#,0}", dDeposit);
                textBox7.Text = string.Format("{0:#,0}", dAvailWithdrAmt);
                textBox8.Text = string.Format("{0:#,0}", dCashAvailOrd);

                RealAccountRQ(true);
            } else { // 종합계좌잔고현황
                yfAccountValues.SetValueData(yfAccountData.GetKorValueHeader("TA1001"), Value);

                dDeposit = System.Convert.ToDouble(yfAccountValues.GetValue(0));
                dAvailWithdrAmt = System.Convert.ToDouble(yfAccountValues.GetValue(1));
                dCashAvailOrd = System.Convert.ToDouble(yfAccountValues.GetValue(4));

                textBox6.Text = string.Format("{0:#,0}", dDeposit);
                textBox7.Text = string.Format("{0:#,0}", dAvailWithdrAmt);
                textBox8.Text = string.Format("{0:#,0}", dCashAvailOrd);

                // 일반주식 잔고내역, 일반주식 주식내역 실시간 변동 내역 등록
                RealAccountRQ(false);
            }
        }

        private void yfOrderListData_ReceiveData(string TrCode, string Value, string ValueList, int NextFlag, int SelectCount, string MsgCode, string MsgName)
        {
            listBox1.Items.Add(string.Format("Data -> TrCode : [{0}], Value : {1}", TrCode, Value));
            listBox2.Items.Add(string.Format("TrCode : [{0}], MsgCode : [{1}], MsgName : [{2}]", TrCode, MsgCode, MsgName));

            yfValueList.SetListData(yfData.GetKorValueListHeader(TrCode), ValueList, 0);

            yfValueList.RowFirst();
            for (int i = 0; i < yfValueList.RowCount(); i++)
            {
                if (System.Convert.ToSingle(yfValueList.GetRowDataCell(16)) == 1)
                {
                    orderNumList.Add(System.Convert.ToSingle(yfValueList.GetRowDataCell(1)));
                    orderCodeList.Add(System.Convert.ToString(yfValueList.GetRowDataCell(3)));
                }
                yfValueList.RowNext();
            }

            if (NextFlag == 1)
            {
                yfOrderListData.RequestData("TA1006", 1);
                Console.Write("Get Next OrderList");
                return;
            }
            else
            {
                Console.Write("OrderList Fin");
                for (int i = 0; i < orderNumList.Count; i++)
                {
                    yfOrderData.RequestInit();
                    yfOrderData.SetData("Account", yfAccountData.AccountItem(comboBox3.SelectedIndex) as string);
                    yfOrderData.SetData("Password", textBox5.Text);
                    yfOrderData.SetData("Code", System.Convert.ToString(orderCodeList[i]).Trim().Substring(1));
                    yfOrderData.SetData("OrderQty", 0);
                    yfOrderData.SetData("OrderPr", 0);
                    yfOrderData.SetData("OrderType", "1");
                    yfOrderData.SetData("AllPartCls", "1");
                    yfOrderData.SetData("OrgOrderNo", System.Convert.ToString(orderNumList[i]));

                    yfOrderData.RequestData("TO1104");
                }
                
                return;
            }
        }

        private void yfOrderData_ReceiveData(string TrCode, string Value, string MsgCode, string MsgName)
        {
            if (TrCode == "TO2101") { // 지수선물 매수/매도 주문
                listBox2.Items.Add(string.Format("지수선물 매수주문 -> TrCode : [{0}], MsgCode : [{1}], MsgName : [{2}]", TrCode, MsgCode, MsgName));
            } 
            else if (TrCode == "TO1104")
            {
                listBox2.Items.Add(string.Format("주식 취소주문 -> TrCode : [{0}], MsgCode : [{1}], MsgName : [{2}]", TrCode, MsgCode, MsgName));
            }
            else {
                listBox2.Items.Add(string.Format("주식 매수주문 -> TrCode : [{0}], MsgCode : [{1}], MsgName : [{2}]", TrCode, MsgCode, MsgName));
            }

            listboxscrolldown(listBox2);
        }

        private void yfRealData_ReceiveData(string TrCode, string Value, string MsgCode, string MsgName)
        {
            listBox1.Items.Add(string.Format("Real Data -> TrCode : [{0}], Value : {1}", TrCode, Value));
            listBox2.Items.Add(string.Format("TrCode : [{0}], MsgCode : [{1}], MsgName : [{2}]", TrCode, MsgCode, MsgName));

            listboxscrolldown(listBox1);
            listboxscrolldown(listBox2);
            if (TrCode == "RQ8003")
            {

            }
            else if (TrCode == "RQ2101") {
                yfValues.SetValueData(yfRealData.GetKorValueHeader("RQ2101"), Value);

                fLast = System.Convert.ToSingle(yfValues.GetValue(4)) / 100;
                fChange = System.Convert.ToSingle(yfValues.GetValue(9)) / 100;
                fChgRate = System.Convert.ToSingle(yfValues.GetValue(53));

                textBox2.Text = string.Format("{0:#,0.00}", fLast);
                textBox3.Text = string.Format("{0:#,0.00}", fChange);
                textBox4.Text = string.Format("{0:#.00}%", fChgRate);
            } else {
                yfValues.SetValueData(yfRealData.GetKorValueHeader("RQ1101"), Value);

                fLast = System.Convert.ToSingle(yfValues.GetValue(5));
                fChange = System.Convert.ToSingle(yfValues.GetValue(3));
                fChgRate = System.Convert.ToSingle(yfValues.GetValue(4)) / 100;

                textBox2.Text = string.Format("{0:#,0}", fLast);
                textBox3.Text = string.Format("{0:#,0}", fChange);
                textBox4.Text = string.Format("{0:#.00}%", fChgRate);
            }
        }

        private void yfAccountRealData_ReceiveData(string TrCode, string Value, string MsgCode, string MsgName)
        {
            listBox3.Items.Add(string.Format("Account Real Data -> TrCode : [{0}], Account : {1}, Value : {2}", TrCode, yfAccountValues.GetValue(0), Value));
            listboxscrolldown(listBox3);

            //yfAccountValues.SetValueData(yfAccountRealData.GetKorValueHeader(TrCode), Value);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            yfData.ReceiveData -= yfData_ReceiveData; // 화면이 닫힐때에는 등록된 EventHandler를 해제함
            yfAccountData.ReceiveData -= yfAccountData_ReceiveData;
            yfOrderData.ReceiveData -= yfOrderData_ReceiveData;

            yfRealData.AllDeleteReal(); // 등록된 리얼 Data는 화면이 종료될 때 반드시 해제를 해주어야 함
            yfRealData.ReceiveData -= yfRealData_ReceiveData;

            yfAccountRealData.AllDeleteReal(); // 등록된 리얼 Data는 화면이 종료될 때 반드시 해제를 해주어야 함
            yfAccountRealData.ReceiveData -= yfAccountRealData_ReceiveData;
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0) {
                comboBox2.Visible = true;
                label6.Visible = true;
            }
            else if (comboBox1.SelectedIndex == 7)
            {
                comboBox2.Visible = true;
                label6.Visible = true;
            }
            else {
                comboBox2.Visible = false;
                label6.Visible = false;
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)  // 더블클릭 이벤트
        {
            if (!string.IsNullOrEmpty(trcode)) {
                if (trcode == "TL0003") { // 선물옵션
                    textBox1.Text = dataGridView1[1, dataGridView1.SelectedCells[0].RowIndex].Value as string;
                    textBox12.Text = dataGridView1[1, dataGridView1.SelectedCells[0].RowIndex].Value as string;
                    textBox11.Text = dataGridView1[0, dataGridView1.SelectedCells[0].RowIndex].Value as string;

                    yfUnderData.RequestInit();
                    yfUnderData.SetData("Code", dataGridView1[0, dataGridView1.SelectedCells[0].RowIndex].Value as string);
                    yfUnderData.RequestData("TQ2101", 0);

                    // Real시세 등록 (KOSPI200 선물 Real)
                    yfRealData.AllDeleteReal(); // Real Data를 모두 해제하고 다시 등록함
                    yfRealData.AddRealCode(dataGridView1[0, dataGridView1.SelectedCells[0].RowIndex].Value as string, "RQ2101");
                } else {
                    textBox1.Text = dataGridView1[2, dataGridView1.SelectedCells[0].RowIndex].Value as string;
                    textBox9.Text = dataGridView1[2, dataGridView1.SelectedCells[0].RowIndex].Value as string;
                    textBox10.Text = dataGridView1[1, dataGridView1.SelectedCells[0].RowIndex].Value as string;

                    // 시세 받아오기
                    yfUnderData.RequestInit();
                    yfUnderData.SetData("Code", dataGridView1[1, dataGridView1.SelectedCells[0].RowIndex].Value as string);
                    yfUnderData.RequestData("TQ1001", 0);

                    // Real시세 등록 (주식 Real)
                    yfRealData.AllDeleteReal(); // Real Data를 모두 해제하고 다시 등록함
                    yfRealData.AddRealCode(dataGridView1[1, dataGridView1.SelectedCells[0].RowIndex].Value as string, "RQ1101");
                }
            }
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter) {
                TrAccountRQ(); // 계좌정보
                TrFutureAccountRQ(); // 선물옵션계좌정보
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TrAccountRQ(); // 계좌정보
            TrFutureAccountRQ(); // 선물옵션계좌정보
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // 주식매수
            if (!string.IsNullOrEmpty(textBox5.Text)) {
                yfOrderData.RequestInit();
                yfOrderData.SetData("Account", yfAccountData.AccountItem(comboBox3.SelectedIndex) as string);
                yfOrderData.SetData("Password", textBox5.Text);
                yfOrderData.SetData("Code", textBox10.Text);
                yfOrderData.SetData("OrderQty", System.Convert.ToSingle(numericUpDown1.Value));
                yfOrderData.SetData("OrderPr", System.Convert.ToSingle(numericUpDown2.Value));
                yfOrderData.SetData("OrderType", string.Format("{0}",comboBox4.SelectedIndex + 1));

                yfOrderData.RequestData("TO1102");
            } else {
                MessageBox.Show("비밀번호를 입력해주십시오.");
            }
        }

        private void TrAccountRQ()
        {
            if (!string.IsNullOrEmpty(textBox5.Text)) { // 종합계좌잔고현황
                yfAccountData.RequestInit();
                yfAccountData.SetData("Account", yfAccountData.AccountItem(comboBox3.SelectedIndex) as string);
                yfAccountData.SetData("Password", textBox5.Text);
                yfAccountData.SetData("StockSecCode", "0");
                yfAccountData.RequestData("TA1001", 0);
            } else {
                MessageBox.Show("비밀번호를 입력해주십시오.");
            }
        }

        private void TrFutureAccountRQ()
        {
            if (!string.IsNullOrEmpty(textBox5.Text)) { // 선물옵션 예수금
                yfAccountData.RequestInit();
                yfAccountData.SetData("Account", yfAccountData.AccountItem(comboBox3.SelectedIndex) as string);
                yfAccountData.SetData("Password", textBox5.Text);
                yfAccountData.RequestData("TA2006", 0);
            } else {
                MessageBox.Show("비밀번호를 입력해주십시오.");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int loop_count = System.Convert.ToInt32(textBox15.Text);
            listBox1.Items.Add(string.Format("Start time: [{0}]", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")));
            for (int i = 0; i < loop_count; i++)
            {
                TrAccountRQ(); // 계좌정보
            }
            listBox1.Items.Add(string.Format("End time: [{0}]", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")));
        }

        private void button6_Click(object sender, EventArgs e)
        {
            int loop_count = System.Convert.ToInt32(textBox15.Text);
            listBox1.Items.Add(string.Format("Start time: [{0}]", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")));
            for (int i = 0; i < 150; i++)
            {
                button3_Click(sender, e);
            }
            listBox1.Items.Add(string.Format("End time: [{0}]", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")));
        }
        
        private void button7_Click(object sender, EventArgs e)
        {
            orderNumList.Clear();

            yfOrderListData.RequestInit();
            yfOrderListData.SetData("Account", yfAccountData.AccountItem(comboBox3.SelectedIndex) as string);
            yfOrderListData.SetData("Password", textBox5.Text);
            yfOrderListData.SetData("SecCode", "1");
            yfOrderListData.SetData("QrySec", "1");
            yfOrderListData.SetData("OrderDate", "20200723");
            yfOrderListData.SetData("Code", "001510");

            yfOrderListData.RequestData("TA1006", 0);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            yfData.RequestInit();
            yfData.SetData("Account", yfAccountData.AccountItem(comboBox3.SelectedIndex) as string);
            yfData.SetData("Password", textBox5.Text);
            yfData.SetData("ExchCCode", ExchCCodeDic[comboBox5.SelectedIndex][comboBox5.SelectedItem as string]);
            yfData.SetData("JbCode", "1");
            yfData.SetData("AplcExch", "0");
            yfData.SetData("FcrncyAmt", "0");

            yfData.RequestData("TO7001", 0);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string exCode = "";
            string requestTrCode = "";
            string selectedNationCode = ExchCCodeDic[comboBox5.SelectedIndex][comboBox5.SelectedItem as string];
            if (selectedNationCode == "0")
            {
                exCode = "US";
                requestTrCode = "TO6301";
            }
            else if (selectedNationCode == "8")
            {
                exCode = "CN";
                requestTrCode = "TO6201";
            }
            else
            {
                exCode = "HK";
                requestTrCode = "TO6101";
            }
            yfOrderData.RequestInit();
            yfOrderData.SetData("Account", yfAccountData.AccountItem(comboBox3.SelectedIndex) as string);
            yfOrderData.SetData("Password", textBox5.Text);
            yfOrderData.SetData("ExCode", exCode);
            yfOrderData.SetData("TrdType", "02");
            yfOrderData.SetData("Code", textBox10.Text);
            yfOrderData.SetData("OrderQty", System.Convert.ToSingle(numericUpDown1.Value));
            yfOrderData.SetData("OrderPr", System.Convert.ToSingle(numericUpDown2.Value));
            yfOrderData.SetData("OrderType", "2");
            yfOrderData.SetData("AplcExchR", textBox16.Text);

            yfOrderData.RequestData(requestTrCode);
        }

        private void RealAccountRQ(bool futureYN)
        {
            string trcode1, trcode2;

            if (futureYN == true) {
                trcode1 = "RA2001";
                trcode2 = "RA2002";
            } else {
                trcode1 = "RA1001";
                trcode2 = "RA1002";
            }

            if (RealAccountList.Contains(yfAccountData.AccountItem(comboBox3.SelectedIndex) as string)) {
                // 등록된 계좌가 있다면 Real RQ를 전송하지 않는다.
                listBox3.Items.Add(string.Format("[이미 Real등록된 계좌 : {0}]", yfAccountData.AccountItem(comboBox3.SelectedIndex) as string));

                listboxscrolldown(listBox3);
            } else {
                // 등록된 계좌가 없다면 List에 추가하고 Real을 등록함
                RealAccountList.Add(yfAccountData.AccountItem(comboBox3.SelectedIndex) as string);

                yfAccountRealData.AddAccount(yfAccountData.AccountItem(comboBox3.SelectedIndex) as string, trcode1);
                listBox3.Items.Add(string.Format("잔고내역 Real 등록 : TrCode : [{0}], Account : {1}", trcode1, yfAccountData.AccountItem(comboBox3.SelectedIndex) as string));

                yfAccountRealData.AddAccount(yfAccountData.AccountItem(comboBox3.SelectedIndex) as string, trcode2);
                listBox3.Items.Add(string.Format("주문내역 Real 등록 : TrCode : [{0}], Account : {1}", trcode2, yfAccountData.AccountItem(comboBox3.SelectedIndex) as string));

                listboxscrolldown(listBox3);
            }

            /* Linq 사용했을 경우
            if ( (from item in RealAccountList
                  where item == yfAccountData.AccountItem(comboBox3.SelectedIndex) as string
                  select item).Count() == 0 ) {
            }
            */
        }

        private void comboBox4_TextChanged(object sender, EventArgs e)
        {
            if ((comboBox4.SelectedIndex == 1) || (comboBox4.SelectedIndex == 4) || (comboBox4.SelectedIndex == 5)) {
                numericUpDown2.Value = 0;
                numericUpDown2.Enabled = false;
            } else {
                if (fLast != 0) {
                    numericUpDown2.Value = System.Convert.ToDecimal(fLast);
                    numericUpDown2.Enabled = true;
                } else {
                    numericUpDown2.Value = 0;
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // 선물매수
            if (!string.IsNullOrEmpty(textBox5.Text)) {
                yfOrderData.RequestInit();
                yfOrderData.SetData("Account", yfAccountData.AccountItem(comboBox3.SelectedIndex) as string);
                yfOrderData.SetData("Password", textBox5.Text);
                yfOrderData.SetData("Code", textBox11.Text);
                yfOrderData.SetData("TrdType", "2");
                yfOrderData.SetData("OrderType", "1");
                yfOrderData.SetData("OrderQty", System.Convert.ToSingle(numericUpDown4.Value));
                yfOrderData.SetData("OrderPr", System.Convert.ToSingle(textBox14.Text));

                yfOrderData.RequestData("TO2101");
            } else {
                MessageBox.Show("비밀번호를 입력해주십시오.");
            }

        }

        private void listboxscrolldown(ListBox lb)
        {
            lb.SelectedIndex = lb.Items.Count - 1;
            lb.ClearSelected();
        }
    }
}
