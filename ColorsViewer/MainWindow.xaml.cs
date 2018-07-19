using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace ColorsViewer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<ColorDictItem> _colorList;
        private ColorDictItem _selColorItem;

        private Binding _rectWidthBinding, _rectHeightBinding;

        public MainWindow()
        {
            InitializeComponent();

            _colorList = new List<ColorDictItem>();
            listForeColors.DisplayMemberPath = "Name";
            listForeColors.SelectedValuePath = "Color";
            listBackColors.DisplayMemberPath = "Name";
            listBackColors.SelectedValuePath = "Color";

            _rectWidthBinding = new Binding() { Source = nudWidth, Path = new PropertyPath("Value", null) };
            _rectHeightBinding = new Binding() { Source = nudHeight, Path = new PropertyPath("Value", null) };
            rbtToClipByName.IsChecked = true;

            // init filling
            rbtColors.IsChecked = true;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            fillControlsSource();
        }

        private void listBackColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ColorDictItem selectedItem = (ColorDictItem)e.AddedItems[0];
                this.Background = selectedItem.GetSolidBrush();
            }
        }

        private void listForeColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ColorDictItem selectedItem = (ColorDictItem)e.AddedItems[0];
                this.Foreground = selectedItem.GetSolidBrush();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            string key = mi.Header.ToString();
            if (key.Contains("Back"))
                listBackColors.SelectedValue = _selColorItem.Color;
            else
                listForeColors.SelectedValue = _selColorItem.Color;
        }

        private void wrapPanel_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // get stack panel
            DependencyObject d1 = VisualTreeHelper.GetParent((e.Source as DependencyObject));
            if (d1 is StackPanel)
            {
                StackPanel panel = (StackPanel)d1;
                _selColorItem = (ColorDictItem)panel.Tag;
            }
        }

        private void btnCopyToClibboard_Click(object sender, RoutedEventArgs e)
        {
            string sClipboard = "";

            ColorDictItem foreColItem = (ColorDictItem)listForeColors.SelectedItem;
            ColorDictItem backColItem = (ColorDictItem)listBackColors.SelectedItem;

            if (rbtToClipByName.IsChecked??false)
                sClipboard = foreColItem.Name + "|" + backColItem.Name;
            else if (rbtToClipByRGB.IsChecked ?? false)
                sClipboard = foreColItem.ToStringRGB(";") + "|" + backColItem.ToStringRGB(";");

            Clipboard.Clear();
            Clipboard.SetText(sClipboard, TextDataFormat.Text);

            MessageBox.Show(string.Format("В буфер скопирована строка\n\n\t{0}", sClipboard), "Copy to Clipboard", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void fillControlsSource()
        {
            reFillColorList();

            listForeColors.ItemsSource = null;
            listForeColors.Items.Clear();
            listForeColors.ItemsSource = _colorList;
            listForeColors.SelectedValue = ((SolidColorBrush)this.Foreground).Color;

            listBackColors.ItemsSource = null;
            listBackColors.Items.Clear();
            listBackColors.ItemsSource = _colorList;
            listBackColors.SelectedValue = ((SolidColorBrush)this.Background).Color;

            // заполнить wrapPanel
            wrapPanel.Children.Clear();
            foreach (ColorDictItem item in _colorList)
            {
                StackPanel stp = new StackPanel();
                stp.Margin = new Thickness(8);
                stp.Tag = item;

                Rectangle rect = new Rectangle() { Fill = item.GetSolidBrush() };
                rect.SetBinding(Rectangle.WidthProperty, _rectWidthBinding);
                rect.SetBinding(Rectangle.HeightProperty, _rectHeightBinding);
                stp.Children.Add(rect);

                stp.Children.Add(new TextBlock()
                {
                    Text = item.Name,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                });
                stp.Children.Add(new TextBlock()
                {
                    Text = "(" + item.ToStringRGB(",") + ")",
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                });

                wrapPanel.Children.Add(stp);
            }
        }

        private void reFillColorList()
        {
            Type tMain = null;
            if (rbtColors.IsChecked ?? false) tMain = typeof(Colors);
            else if (rbtSystemColors.IsChecked ?? false) tMain = typeof(SystemColors);
            if (tMain == null) return;

            _colorList.Clear();
            PropertyInfo[] pi = tMain.GetProperties(BindingFlags.Static | BindingFlags.Public);
            foreach (PropertyInfo item in pi)
            {
                if (item.PropertyType.Name != "Color") continue;
                _colorList.Add(new ColorDictItem()
                {
                    Name = item.Name,
                    Color = (Color)item.GetValue(null)
                });
            }
        }


        private class ColorDictItem
        {
            public string Name { get; set; }
            public Color Color { get; set; }

            public SolidColorBrush GetSolidBrush()
            {
                return new SolidColorBrush(this.Color);
            }

            public string ToStringRGB(string delimiter = ",")
            {
                return string.Format("{0}{3}{1}{3}{2}", this.Color.R, this.Color.G, this.Color.B, delimiter);
            }
        }

    }  // class
}
