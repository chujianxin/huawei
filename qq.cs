using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AWClient.DragDropFramework;
using AWClient.DragDropFrameworkData;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
namespace AWClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    [TemplatePart(Name = "PART_scrollIndex", Type = typeof(ScrollViewer))]  
    public partial class MainWindow : Window
    {
        
        public StringBuilder buf0 = new StringBuilder("");
        public StringBuilder buf1 = new StringBuilder("");
        public string json = "";
        public static int index = 1;
        TextBox txtNode = new TextBox();
        ListBoxItem selectNode = null;
        public static ListBox lIndex =null;
        public static ListBox lbox = null;
        public static ListBox lboxmap = null;
        string olddata="";
        string oldName = "";
        public static Button a = null;
        public static bool isPause = false;
        delegate void DbackgroundWorker_DoWork(object sender, DoWorkEventArgs e);
        Thread t;
       
        public MainWindow()
        {
            InitializeComponent();
            regBHO();
            this.Width = 406;
            FileDropConsumer fileDropDataConsumer =
              new FileDropConsumer(new string[] {
                    "FileDrop",
                    "FileNameW",
                });

            #region L I S T   B O X
            // Data Provider
            ListBoxDataProvider<ListBox, ListBoxItem> listBoxDataProvider =
                new ListBoxDataProvider<ListBox, ListBoxItem>("ListBoxItemObject");

            // Data Consumer
            ListBoxDataConsumer<ListBox, ListBoxItem> listBoxDataConsumer =
                new ListBoxDataConsumer<ListBox, ListBoxItem>(new string[] { "ListBoxItemObject" });

            // Data Consumer of TreeViewItems
            TreeViewItemToListBoxItem<ItemsControl, TreeViewItem> treeViewItemToListBoxItem =
                new TreeViewItemToListBoxItem<ItemsControl, TreeViewItem>(new string[] { "TreeViewItemObject" });

            // Drag Managers
            DragManager dragHelperListBox0 = new DragManager(this.listBox, listBoxDataProvider);


            // Drop Managers
            DropManager dropHelperListBox0 = new DropManager(this.listBox,
                new IDataConsumer[] {
                    listBoxDataConsumer,
                    treeViewItemToListBoxItem,
                    fileDropDataConsumer,
                });

            #endregion
            this.Topmost = true;
            lIndex = lstIndex;
            lbox = listBox;
            lboxmap = listbox;
            t = new Thread(new ThreadStart(receive));
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;

            t.Start();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void regBHO()
        {
            Process proc = null;
            try
            {
                proc = new Process();
                proc.StartInfo.FileName = System.IO.Directory.GetCurrentDirectory()+"//reg.bat";
                proc.StartInfo.Arguments = string.Format("10");//this is argument
                proc.StartInfo.CreateNoWindow = false;
                proc.Start();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void receive()
        {
            try
            {
                int recv;
                int port = 9050;
                byte[] data = new byte[9999];
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);//定义一网络端点
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//定义一个Socket
                sock.Bind(ipep);//Socket与本地的一个终结点相关联

                IPEndPoint isender = new IPEndPoint(IPAddress.Any, 0);//定义要发送的计算机的地址
                EndPoint Remote = (EndPoint)(isender);//

                while (true)
                {
                    data = new byte[9999];

                    recv = sock.ReceiveFrom(data, ref Remote);
                    string jsonData = Encoding.UTF8.GetString(data, 0, recv);
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        var element = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AW>>(jsonData);
                        if (!isPause)
                        {
                            if (listBox.Items.Count >= 1)
                            {
                                ListBoxItem last= (ListBoxItem)listBox.Items[listBox.Items.Count - 1];
                                if (last.Content==null)
                                {
                                    cmdEdit.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, cmdEdit));
                                    listBox.SelectedItem = last;
                                    MessageBox.Show("当前元素Name为空，无法录制下一元素！","提示",MessageBoxButton.OK,MessageBoxImage.Information,MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                    return;
                                }
                            }

                            ListBoxItem item = new ListBoxItem();
                            item.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                            if (element[0].record[0].elements[0].id != null)
                                item.Content = element[0].record[0].elements[0].id;
                            else
                                item.Content = element[0].record[0].elements[0].name;

                            item.Tag = jsonData;

                            foreach (ListBoxItem li in listBox.Items)
                            {
                                var ee = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AW>>(li.Tag.ToString())[0];
                                if (ee.record[0].elements[0].xpath == element[0].record[0].elements[0].xpath)
                                {
                                     element[0].record[0].elements[0].name=ee.record[0].elements[0].name;
                                     item.Tag = Newtonsoft.Json.JsonConvert.SerializeObject(element);
                                     break;
                                }
                            }
                            item.Style = (Style)Resources[element[0].record[0].elements[0].action.ToUpper()];
                            item.FontSize = 16; 

                            item.GotFocus += new RoutedEventHandler(tvi_GotFocus);
                            listBox.Items.Add(item);

                            //add index in UI
                            ListBoxItem itemIndex = new ListBoxItem();
                            itemIndex.Content = index;
                            itemIndex.Style = (Style)Resources["ListBoxItemNum"];
                            lstIndex.Items.Add(itemIndex);
                            index++;
                            //页面打标记
                            utils.CreateTempXml();
                            //创建maps
                            utils.awjson = utils.CreateAW();
                            utils.CreateMap();

                            utils.RefreshMapListBox();

                        }
                    }));

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void tvi_GotFocus(object sender, RoutedEventArgs e)
        {
            if (selectNode == null)
            {
                 
                selectNode = sender as ListBoxItem;
                txtNode = new TextBox();
                if (selectNode.Content != null)
                {
                    txtNode.Text = selectNode.Content.ToString();
                    olddata = selectNode.Content.ToString();
                }
            }
        }
       
        private void buttonClick(object sender, RoutedEventArgs e)
        {
            Fold.ColumnDefinitions[1].Width = new GridLength(0);
            this.Width = 406;
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            Fold.ColumnDefinitions[1].Width = new GridLength(406);
            this.Width = 812;
        }

        private void onSelect(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                selectNode = listBox.SelectedItem as ListBoxItem;

                string json = selectNode.Tag.ToString();
                
                var element = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AW>>(json);

                txtID.Text = element[0].record[0].elements[0].id;

                if (element[0].record[0].elements[0].name == null)
                    txtName.Text = element[0].record[0].elements[0].id;
                else
                    txtName.Text = element[0].record[0].elements[0].name;

                if(txtName.Text=="")
                {
                    txtName.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                }
                else
                {
                    txtName.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                }
                cboType.Text= element[0].record[0].elements[0].action; 
                txtValue.Text = element[0].record[0].elements[0].value; 
                txtComments.Text = element[0].record[0].elements[0].comment; 
                oldName = txtName.Text;
            }
            catch
            {}
        }

        private void OnSaveAWStep(object sender, RoutedEventArgs e)
        {
            if (selectNode == null)
                return;
            string json = selectNode.Tag.ToString();
            var element = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AW>>(json);
            foreach (ListBoxItem item in listBox.Items)
            {
                
                string tag = item.Tag.ToString();
                var ee = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AW>>(tag);

                if (txtName.Text!=oldName && ee[0].record[0].elements[0].name == txtName.Text && ee[0].record[0].fullUrl == element[0].record[0].fullUrl)
                {
                    MessageBox.Show("Element重名,请重新输入!");
                    return;
                }
            } 

            if (!utils.isRightName(txtName.Text))
            {
                MessageBox.Show("Element Name中仅可包含大小写和下划线且不能以数字开头！");
                return;
            }
            Element el = element[0].record[0].elements[0];
            el.id = txtID.Text;
            el.name = txtName.Text;
            el.action = cboType.Text;
            el.value = txtValue.Text;
            el.comment = txtComments.Text;

            string JSON = Newtonsoft.Json.JsonConvert.SerializeObject(element);

            selectNode.Tag = JSON;

            if(txtID.Text!="")
            {
                selectNode.Content = txtID.Text;
            }
            else
            {
                selectNode.Content = txtName.Text;
            }
            if (txtName.Text == "")
            {
                txtName.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
            else
            {
                txtName.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            }

            oldName=txtName.Text;
            utils.awjson = utils.CreateAW();
            utils.CreateMap();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem it = (ListBoxItem)lboxmap.SelectedItem;
            if (it == null)
                return;
            ShowPage showPage = new ShowPage();
            showPage.WindowStartupLocation = WindowStartupLocation.CenterScreen; //使窗口位置在最中心
            showPage.Owner = this;
            showPage.Show();
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void OnNew_Click(object sender, RoutedEventArgs e)
        {
            listBox.Items.Clear();
            listbox.Items.Clear();
            lstIndex.Items.Clear();
            index = 1;
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer sv = e.OriginalSource as ScrollViewer;
            double offset =sv.VerticalOffset;
 
            Decorator border = VisualTreeHelper.GetChild(lstIndex, 0) as Decorator;
            if (border != null)
            {
                ScrollViewer scrollViewer = border.Child as ScrollViewer;
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToVerticalOffset(offset);
                }
            }
        }
    }
}
