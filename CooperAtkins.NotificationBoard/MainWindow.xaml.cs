using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;



namespace CooperAtkins.NotificationBoard
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Label[] lblIndex, lblStatus;
        ComboBox[] cmbSensor;
        Dictionary<string, int> LEDMap = new Dictionary<string, int>();
        int mapIndex = 0;
        int nextMapIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            grd.ColumnDefinitions.Clear();
            grd.RowDefinitions.Clear();

            lblIndex = new Label[8];
            lblStatus = new Label[8];
            cmbSensor = new ComboBox[8];

            /*Create Columns*/
            for (int i = 0; i < 3; i++)
            {
                switch (i)
                {
                    case 0:
                        grd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(50, GridUnitType.Pixel) });
                        break;
                    case 1:
                        grd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(75, GridUnitType.Pixel) });
                        break;
                    default:
                        grd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(195, GridUnitType.Pixel) });
                        break;
                }
            }

            /*Create Rows*/
            for (int i = 0; i < 7; i++)
            {
                grd.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(20, GridUnitType.Star) });
            }

            for (int rows = 0, index = 0; rows <= 7; rows++, index++)
            {
                for (int cols = 0; cols < 3; cols++)
                {
                    switch (cols)
                    {
                        case 0:
                            /*Adding Index Label to grid*/
                            lblIndex[index] = new Label();
                            /*Set label properties*/
                            lblIndex[index].Foreground = Brushes.Red;
                            lblIndex[index].Content = (rows +1).ToString();
                            lblIndex[index].Margin = new Thickness(0, 5, 5, 5);
                            /*c label position in grid row*/
                            Grid.SetRow(lblIndex[index], rows);
                            /*Set label position in grid column*/
                            Grid.SetColumn(lblIndex[index], cols);
                            /*Add label to grid*/
                            grd.Children.Add(lblIndex[index]);
                            break;
                        case 1:
                            /*Adding Status Label to grid*/
                            lblStatus[index] = new Label();
                            /*Set label properties*/
                            lblStatus[index].Foreground = Brushes.Red;
                            lblStatus[index].Margin = new Thickness(0, 5, 5, 5);
                            /*c label position in grid row*/
                            Grid.SetRow(lblStatus[index], rows);
                            /*Set label position in grid column*/
                            Grid.SetColumn(lblStatus[index], cols);
                            /*Add label to grid*/
                            grd.Children.Add(lblStatus[index]);
                            break;
                        case 2:
                            /*Adding Status Label to grid*/
                            cmbSensor[index] = new ComboBox();
                            /*Set label properties*/
                            cmbSensor[index].Foreground = Brushes.Red;
                            cmbSensor[index].Width = 180;
                            cmbSensor[index].Height = 20;
                            cmbSensor[index].Margin = new Thickness(3, 5, 5, 5);
                            /*c label position in grid row*/
                            Grid.SetRow(cmbSensor[index], rows);
                            /*Set label position in grid column*/
                            Grid.SetColumn(cmbSensor[index], cols);
                            /*Add label to grid*/
                            grd.Children.Add(cmbSensor[index]);
                            break;

                    }
                }
            }
            AddSensorsTolist(7, "1003_1_2");

        }

        public void AddSensorsTolist(int bitMask, string sensorAlarmID)
        {
            if (LEDMap.ContainsKey(sensorAlarmID))
            {
                mapIndex = Convert.ToInt32(LEDMap[sensorAlarmID]);
            }
            else
            {
                mapIndex = nextMapIndex;
                nextMapIndex = nextMapIndex + 1;
                LEDMap.Add(sensorAlarmID, mapIndex);
            }

            string probeName = sensorAlarmID.Split('_')[1];
            for (int i = 0; i <= 7; i++)
            {

                if ((bitMask & Convert.ToInt32(Math.Pow(2, i))) != 0)
                {
                    for (int j = 0; j < cmbSensor[i].Items.Count - 1; j++)
                    {
                        ComboBoxItem item = (ComboBoxItem)cmbSensor[i].Items[j];
                        if (item.Content.ToString().ToLower() == probeName.ToLower())
                        {
                            break;
                        }
                    }


                    int idx = cmbSensor[i].Items.Count;
                    ComboBoxItem cmbItem = new ComboBoxItem();
                    cmbSensor[i].Items.Insert(idx, probeName);

                }

            }
            SetLedLights();
        }

        private void SetLedLights()
        {

            for (int i = 0; i <= 7; i++)
            {
                if (cmbSensor[i].Items.Count > 0)
                    lblStatus[i].Background = Brushes.Red;
                else
                    lblStatus[i].Background = Brushes.GreenYellow;
            }
        }
    

    }
}
