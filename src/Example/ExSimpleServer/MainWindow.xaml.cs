using InflySocket;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ExSimpleServer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly InflyServerSimple _server = new InflyServerSimple();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _server.Close();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _server.OnNewConnectedEvent += Server_OnNewConnectedEvent;
            _server.OnReceiveMessageEvent += Server_OnReceiveMessageEvent;
            _server.OnCloseEvent += Server_OnCloseEvent;
        }

        private void Server_OnCloseEvent(SessionBase newClient)
        {
            Dispatcher?.BeginInvoke(new Action(() =>
            {
                txbMsg.AppendText($"已断开：{newClient.EndPoint}{'\n'}");
            }));
        }

        private void Server_OnReceiveMessageEvent(string msg)
        {
            Dispatcher?.BeginInvoke(new Action(() =>
            {
                txbMsg.AppendText($"收到消息：{msg}{'\n'}");
            }));
        }

        private void Server_OnNewConnectedEvent(SessionBase newClient)
        {
            Dispatcher?.BeginInvoke(new Action(() =>
            {
                txbClients.AppendText($"新连接：{newClient.EndPoint}{'\n'}");
            }));
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            var isStart = _server.Listen(9999);
            btnStart.Content = isStart ? "已启动" : "启动";
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            _server.Send(txbSend.Text+'\n');
            txbMsg.AppendText($"发送消息：{txbSend.Text}{'\n'}");
        }

        private void TxbMsg_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            txbMsg.ScrollToEnd();
        }
    }
}
