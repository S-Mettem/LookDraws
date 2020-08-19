using System;
using System.Windows;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media;

namespace LookDraws
{
    public partial class ChooseFolderWindow : Window
    {
        MainWindow MAIN_WINDOW; // Needed for closing the app 

        // List of Draws DataList needed for supply, changing and provide all data from listview (DrawsList) 
        public ObservableCollection<Draw> DataList = new ObservableCollection<Draw>();

        // Supply all opened ChangeTextWindow's
        // Needed to control Rename_Button
        public List<ChangeTextWindow> OPENED_WINDOWS = new List<ChangeTextWindow>();
        private bool _PAUSE_FLAG = true;


        /* Windows Methods */
        public ChooseFolderWindow(MainWindow mw)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.MAIN_WINDOW = mw;

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.MarkOut_ListView();
            Check_ListView();

        }
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
                if (this.Button_Delete.IsEnabled == true)
                    this.Button_Delete_Click(sender, e);

            if (e.Key == System.Windows.Input.Key.F5)
                if (this.Button_Find.IsEnabled == true)
                    this.Button_Find_Click(sender, e);

            if (e.Key == System.Windows.Input.Key.F6)
                if (this.Button_Rename.IsEnabled == true)
                    this.Button_Rename_Click(sender, e);

            if (e.Key == System.Windows.Input.Key.Escape)
                if (this.Button_Pause.IsEnabled == true)
                    this.Button_Pause_Click(sender, e);
            if (e.Key == System.Windows.Input.Key.F1)
                this.Menu_About_Click(sender, e);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MAIN_WINDOW.Close();
        }


        /* Buttons and Menu CLick Methods  */
        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            // Event Handler Click for Button_Add
            // Initialization dialog to choose directory 
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "Выберите папку для добавления изображений",
            };

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                // Get folder(s)
                var folders = openFileDialog.FileNames;

                // Start getting files from folder(s)
                foreach (String folder in folders)
                {
                    String[] files_from_folder = Directory.GetFiles(@folder); // Get files

                    // Start checking to right extension
                    foreach (String file in files_from_folder)
                        // Get files with most popular image extension 
                        if (file.EndsWith(".jpg") || file.EndsWith(".tif") || file.EndsWith(".png") || file.EndsWith("bmp"))
                        {
                            // if file had that extension
                            // that create class Draw with path of the that file
                            Draw d = new Draw(file);
                            // Add to List
                            DataList.Add(d);
                        }
                }
                // When all files in the list 
                // Add them to user inteface
                DrawsList.ItemsSource = DataList;
            }
        }
        private void Button_Add_From_File_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = "Выберите изображения для добавления",
                EnsureFileExists = true,
                Multiselect = true,
            };

            openFileDialog.Filters.Add(new CommonFileDialogFilter("Images Files | ", "*.jpg; *.tif; *.png; .bmp"));

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var files = openFileDialog.FileNames;

                foreach (String file in files)
                {
                    Draw d = new Draw(file.Trim());
                    DataList.Add(d);
                }

                DrawsList.ItemsSource = DataList;
            }
        }
        private void Menu_Add_File_Click(object sender, RoutedEventArgs e)
        {
            this.Button_Add_From_File_Click(sender, e);
        }
        private void Menu_Add_Folder_Click(object sender, RoutedEventArgs e)
        {
            this.Button_Add_Click(sender, e);
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            // Event Hendler Click for Button_Delete
            while (DrawsList.SelectedItems.Count > 0)
                DataList.RemoveAt(DrawsList.Items.IndexOf(DrawsList.SelectedItems[0]));

            DrawsList.ItemsSource = new List<int>();
            DrawsList.ItemsSource = DataList;
        }

        async private void Button_Find_Click(object sender, RoutedEventArgs e)
        {

            Switch_Buttons(false); // Disable a Buttons
            this.pbStatus.IsIndeterminate = true; // Start progress bar
            this._PAUSE_FLAG = false;

            // Declare a process
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = "dist/script.exe",
                RedirectStandardOutput = true,
            };
            await Task.Run(() =>
            {
                // Start find draws names
                for (int i = 0; i < DataList.Count; i++)
                {
                    if (DataList[i].name_of_draw != null) continue; // Check for exitstens is already finded name
                    if (_PAUSE_FLAG == true) break; // if true Stop the process

                    // Change state of progress label
                    Dispatcher.Invoke(new ThreadStart(delegate
                    {
                        Label_Find.Content = "Поиск названия для файла " + Path.GetFileName(@DataList[i].path_to_draw);
                    }));

                    // Start processing find name of draw
                    startInfo.Arguments = '"' + DataList[i].path_to_draw + '"';
                    Process process = Process.Start(startInfo);

                    // Save result of work
                    try
                    {
                        DataList[i].name_of_draw = process.StandardOutput.ReadLine().ToString().Trim();

                    }
                    catch (NullReferenceException)
                    {
                        DataList[i].name_of_draw = "Текст не найден";
                    }

                    Dispatcher.Invoke(new ThreadStart(delegate
                    {
                        DrawsList.ItemsSource = new List<int>();
                        DrawsList.ItemsSource = DataList;
                    }));
                }

            });

            
            Label_Find.Content = "Ожидание начала поиска..."; // Change text for progress Label
            Switch_Buttons(true); // Enable a buttons
            
            this.pbStatus.IsIndeterminate = false; // Stop progress bar 
        }
        private void Menu_Find_Click(object sender, RoutedEventArgs e)
        {
            this.Button_Find_Click(sender, e);
        }

        private void Button_Pause_Click(object sender, RoutedEventArgs e)
        {
            _PAUSE_FLAG = true;
        }
        private void Menu_Pause_Click(object sender, RoutedEventArgs e)
        {
            this.Button_Pause_Click(sender, e);
        }

        private void Button_Rename_Click(object sender, RoutedEventArgs e)
        {
            // Ask the user if he is sure of the file names
            if (MessageBox.Show("Вы уверены с названиями файлов", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Disable all button
                Switch_Buttons(false);

                // Start renaming files
                foreach (var d in DataList)
                {
                    // Get current directiory of file
                    String _currentDir = Path.GetDirectoryName(@d.path_to_draw);

                    // Get extension of file
                    String _format = Path.GetExtension(@d.path_to_draw);

                    // Check to existens of file
                    try
                    {
                        // Rename file
                        File.Move(d.path_to_draw, Path.Combine(_currentDir, d.name_of_draw + _format));
                    }
                    catch (FileNotFoundException)
                    {
                        continue;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        MessageBox.Show("Название \"" + d.name_of_draw + "\" содержит недопустимые симовлы" +
                            "\n  Такими символами являются: \\, /, :, *, ?, \", <, >, |");
                        Switch_Buttons(true);
                        return;
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Название \"" + d.name_of_draw + "\" содержит недопустимые симовлы" +
                            "\n  Такими символами являются: \\, /, :, *, ?, \", <, >, |");
                        Switch_Buttons(true);
                        return;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("Недопустимое название чертежа\nНазвание: " + d.name_of_draw);
                        Switch_Buttons(true);
                        return;

                    }
                }
                // Warning user to success
                MessageBox.Show("Done", "Success", MessageBoxButton.OK);
                // Clear list
                DataList.Clear();
                DrawsList.ItemsSource = DataList;
                // Enable all buttons
                Switch_Buttons(true);
            }
        }
        private void Menu_Rename_Click(object sender, RoutedEventArgs e)
        {
            this.Button_Rename_Click(sender, e);
        }

        private void Menu_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Menu_About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Программа Look Draws, version 2.0\n" +
                "Разработчик: Гадалов Илья\n" +
                "Программа необходима для расшифровки названия чертежей и\n" +
                "формирования базы данных, используя эти названия.");
        }


        /* Support Methods */
        private void MarkOut_ListView()
        {
            // Set names and bingings to columns
            List<string> columnsNames = new List<string>() { "Путь до чертежа", "Название чертежа" };
            List<string> columnsBinigs = new List<string>(columnsNames.Count) { "path_to_draw", "name_of_draw" };

            // Create gridView where the columns were placed
            GridView gv = new GridView();
            gv.AllowsColumnReorder = true;

            // Start mark out the ListView
            for (int i = 0; i < columnsNames.Count; i++)
            {
                GridViewColumn gvc = new GridViewColumn();
                gvc.Header = columnsNames[i];
                gvc.DisplayMemberBinding = new Binding(columnsBinigs[i]);
                gvc.Width = this.Width / columnsNames.Count;
                gv.Columns.Add(gvc);
            }

            DrawsList.View = gv;
        }
        async private void Check_ListView()
        {
            /*
            That function checking every 1 second to existens of finded names of all draws 
            If is exist: enable button; not: disable
            */
            while (true)
            {
                await Task.Delay(500);
                if (DataList.Count > 0 && OPENED_WINDOWS.Count < 1)
                {
                    int count = 0;
                    foreach (Draw d in DataList)
                        if (d.name_of_draw != null) count++;

                    if (count == DataList.Count)
                    {
                        this.Button_Rename.IsEnabled = true;
                        this.Menu_Rename.IsEnabled = true;
                    }
                    else
                    {
                        this.Button_Rename.IsEnabled = false;
                        this.Menu_Rename.IsEnabled = false;
                    }
                }
                else
                {
                    this.Button_Rename.IsEnabled = false;
                    this.Menu_Rename.IsEnabled = false;
                }

                if (_PAUSE_FLAG == false)
                {
                    this.Button_Pause.IsEnabled = true;
                    this.Menu_Pause.IsEnabled = true;
                }
                else {
                    this.Button_Pause.IsEnabled = false;
                    this.Menu_Pause.IsEnabled = false;
                } 

                if (this.Button_Add.IsEnabled == false) this.Button_Add.Background = Brushes.Gray;
                else this.Button_Add.Background = Brushes.White;

                if (this.Button_Delete.IsEnabled == false) this.Button_Delete.Background = Brushes.Gray;
                else this.Button_Delete.Background = Brushes.White;

                if (this.Button_Find.IsEnabled == false) this.Button_Find.Background = Brushes.Gray;
                else this.Button_Find.Background = Brushes.White;

                if (this.Button_Pause.IsEnabled == false) this.Button_Pause.Background = Brushes.Gray;
                else this.Button_Pause.Background = Brushes.White;

                if (this.Button_Rename.IsEnabled == false) this.Button_Rename.Background = Brushes.Gray;
                else this.Button_Rename.Background = Brushes.White;

                if (this.Button_Add_From_File.IsEnabled == false) this.Button_Add_From_File.Background = Brushes.Gray;
                else this.Button_Add_From_File.Background = Brushes.White;
            }
        }
        private void Switch_Buttons(bool enable)
        {
            if (enable == true)
            {
                this.Button_Find.IsEnabled = true;
                this.Button_Delete.IsEnabled = true;
                this.Button_Add.IsEnabled = true;
                this.Button_Add_From_File.IsEnabled = true;

                this.Menu_Rename.IsEnabled = true;
                this.Menu_Find.IsEnabled = true;
                this.Menu_Add_File.IsEnabled = true;
                this.Menu_Add_Folder.IsEnabled = true;
                this.Menu_Find.IsEnabled = true;
            }
            else
            {
                this.Button_Find.IsEnabled = false;
                this.Button_Delete.IsEnabled = false;
                this.Button_Add.IsEnabled = false;
                this.Button_Add_From_File.IsEnabled = false;

                this.Menu_Rename.IsEnabled = false;
                this.Menu_Find.IsEnabled = false;
                this.Menu_Add_File.IsEnabled = false;
                this.Menu_Add_Folder.IsEnabled = false;
                this.Menu_Find.IsEnabled = false;
            }
        }


        /* Methods to ListView Elements */
        private void DrawsList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DrawsList.SelectedItems.Count == 1)
            {
                int _indexItem = DrawsList.Items.IndexOf(DrawsList.SelectedItems[0]);

                ChangeTextWindow ctw = new ChangeTextWindow(this, _indexItem);
                ctw.Show();
                OPENED_WINDOWS.Add(ctw);
            }

        }
    }
}
