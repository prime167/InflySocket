using InflySocket;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ExSimpleClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly InflyClientSimple _client = new InflyClientSimple();

        public MainWindow()
        {
            InitializeComponent();

            _client.OnReceiveMessageEvent += Client_OnReceiveMessageEvent;
            _client.OnConnectedEvent += Client_OnConnectedEvent;
            _client.OnCloseEvent += Client_OnCloseEvent;
        }

        private void Client_OnReceiveMessageEvent(string msg)
        {
            Dispatcher?.BeginInvoke(new Action(() =>
            {
                txbMsg.AppendText($"收到消息：{msg}{'\n'}");
            }));
        }

        private void Client_OnConnectedEvent()
        {
            Dispatcher?.BeginInvoke(new Action(() =>
            {
                txbMsg.AppendText($"已连接{'\n'}");
                btnConnect.Content = "已连接";
            }));
        }

        private void Client_OnCloseEvent()
        {
            Dispatcher?.BeginInvoke(new Action(() =>
            {
                txbMsg.AppendText($"已断开{'\n'}");
                btnConnect.Content = "连接";
            }));
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            _client.Connect(txbIP.Text, int.Parse(txbPort.Text));
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            _client.Send(txbSend.Text+'\n');
            txbMsg.AppendText($"发送消息：{txbSend.Text}{'\n'}");
        }

        private void TxbMsg_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            txbMsg.ScrollToEnd();
        }
    }
}
