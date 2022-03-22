using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Threading;
using Common.Parsers;

namespace LOI
{
    public partial class MainWindow
    {
        private bool _isPathShown = false;
        private bool _isContentEdited = false;
        private string _openedFileName = null;
        private int _lineCounter = 1;
        
        private readonly IParser _parser;
        private BackgroundWorker _worker;

        private readonly string _helpMessageText =
            "Программа решает задачу перебора всевозможных вариантов для заданной грамматики.\n" +
            "При задании правил вывода обязательно надо включить аксиому \"S\", т.к. для программы это является " +
            "начальным местом для перебора вариантов. Пример, как может выглядеть задание правил вывода:\n" +
            "S -> AAb | %\n" +
            "A -> Sa | c\n" +
            "В правилах вывода символ \"%\" обозначает пустое слово. Для программы все символы в нижнем регистре " +
            "и верхнем регистрах считаются соответственно треминалами и нетерминалами.\n" +
            "Если необходимо посмотреть цепочку вывода, то нажатие \"F5\" переключает вид между показом цепочек " +
            "вывода и не показом.";

        public MainWindow()
        {
            this._parser = new SplitParser();
            
            this.InitializeBackgroundWorker();
            this.InitializeComponent();
        }

        private void TextBoxLeft_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isContentEdited)
            {
                this._isContentEdited = true;
                this.Title = $"LOI - *" + (_openedFileName ?? "Unnamed");
            }

            if (_worker.IsBusy)
            {
                _worker.CancelAsync();
            }
            this.TextBoxRight.Clear();
            this._lineCounter = 1;
                
            try
            {
                Thread.Sleep(50);
                this._worker.RunWorkerAsync(this.TextBoxLeft.Text);
            }
            catch
            {
                this._worker.Dispose();
                this.InitializeBackgroundWorker();
                this._worker.RunWorkerAsync(this.TextBoxLeft.Text);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                this._isPathShown = !this._isPathShown;
                this.TextBoxLeft_OnTextChanged(null, null);
            }
        }

        private void CreateNewFile(object sender, ExecutedRoutedEventArgs e)
        {
            this.CreateNewFile();
        }

        private void OpenFile(object sender, ExecutedRoutedEventArgs e)
        {
            this.OpenFile();
        }
        
        private void SaveFile(object sender, ExecutedRoutedEventArgs e)
        {
            this.SaveFile();
        }

        private void ExitApp(object sender, ExecutedRoutedEventArgs e)
        {
            this.ExitApp();
        }
        
        private void ShowHelp(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this._helpMessageText, "Help", 
                MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.Yes);
        }

        private void CreateNewFile()
        {
            if (this._isContentEdited)
            {
                this.SaveFile();
                this._isContentEdited = false;
                this.CreateNewFile();
            }
            else
            {
                this.TextBoxLeft.Clear();
                this.Title = "LOI - Unnamed";
            }
        }

        private void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                FileName = "temp.loi",
                Filter = "LOI file (*.loi)|*.loi"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this._openedFileName = openFileDialog.FileName;
            }
            if (_openedFileName is not null)
            {
                this.TextBoxLeft.Text = File.ReadAllText(_openedFileName);
                this.Title = $"LOI - {_openedFileName}";
                this._isContentEdited = false;
            }
        }

        private void SaveFile()
        {
            if (_openedFileName is null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "LOI file (*.loi)|*.loi"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    this._openedFileName = saveFileDialog.FileName;
                    this.Title = $"LOI - {_openedFileName}";
                }
            }
            if (_openedFileName is not null)
            {
                File.WriteAllText(_openedFileName, this.TextBoxLeft.Text);
                this.Title = $"LOI - {_openedFileName}";
                this._isContentEdited = false;
            }
        }

        private void ExitApp()
        {
            this.Close();
        }
    }
}