using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace LookDraws
{
    /// <summary>
    /// Interaction logic for ChangeTextWindow.xaml
    /// </summary>
    public partial class ChangeTextWindow : Window
    {
        ChooseFolderWindow PREVIOUS_WINDOW;
        int ITEM_INDEX;

        public ChangeTextWindow(ChooseFolderWindow PW, int ii)
        {
            InitializeComponent();
            PREVIOUS_WINDOW = PW;
            ITEM_INDEX = ii;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = PREVIOUS_WINDOW.DataList[ITEM_INDEX].path_to_draw;

            if(PREVIOUS_WINDOW.DataList[ITEM_INDEX].name_of_draw == null)
            {
                this.Text_Draw.Text = "Нет текста";
            }
            this.Text_Draw.Text = PREVIOUS_WINDOW.DataList[ITEM_INDEX].name_of_draw;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(this.Text_Draw.Text.Trim() != "")
            {
                PREVIOUS_WINDOW.DataList[ITEM_INDEX].name_of_draw = this.Text_Draw.Text;

                PREVIOUS_WINDOW.DrawsList.ItemsSource = new List<int>();
                PREVIOUS_WINDOW.DrawsList.ItemsSource = PREVIOUS_WINDOW.DataList;

                this.Close();
            }
            else
            {
                PREVIOUS_WINDOW.DataList[ITEM_INDEX].name_of_draw = null;

                PREVIOUS_WINDOW.DrawsList.ItemsSource = new List<int>();
                PREVIOUS_WINDOW.DrawsList.ItemsSource = PREVIOUS_WINDOW.DataList;

                this.Close();
            }
        }
        private void Button_Open_Image_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(@PREVIOUS_WINDOW.DataList[ITEM_INDEX].path_to_draw)
            {
                UseShellExecute = true,
            };
            process.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PREVIOUS_WINDOW.OPENED_WINDOWS.Remove(this);
        }

        
    }
}
