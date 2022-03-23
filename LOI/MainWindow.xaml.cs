using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Threading;
using System.Windows.Media;
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
            "The program solves the problem of iterating through all possible options for a given grammar.\n\n" +
            "When setting the output rules, it is necessary to include the axiom \"S\", because for the " +
            "program this is the starting place for iterating through the options. An example of what setting " +
            "output rules might look like:\n" +
            "S -> AAb | %\n" +
            "A -> Sa | c\n" +
            "In the output rules, the symbol \"%\" is an empty word. For the program, all characters " +
            "in lowercase and uppercase are considered terminals and non-terminals, respectively.\n\n" +
            "If you need to view the output chain, pressing \"F5\" switches the view between showing " +
            "output chains and not showing.";

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

        private void UndoEdit(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.TextBoxLeft.IsEnabled && this.TextBoxLeft.CanUndo)
            {
                this.TextBoxLeft.Undo();
            }
        }

        private void RedoEdit(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.TextBoxLeft.IsEnabled && this.TextBoxLeft.CanRedo)
            {
                this.TextBoxLeft.Redo();
            }
        }

        private void CutText(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.TextBoxLeft.IsFocused && this.TextBoxLeft.IsSelectionActive)
            {
                this.TextBoxLeft.Cut();
            }
        }

        private void CopyText(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.TextBoxLeft.IsFocused)
            {
                if (this.TextBoxLeft.IsSelectionActive)
                {
                    this.TextBoxLeft.Copy();
                }
            }
            else if (this.TextBoxRight.IsFocused)
            {
                if (this.TextBoxRight.IsSelectionActive)
                {
                    this.TextBoxRight.Copy();
                }
            }
        }

        private void PasteText(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.TextBoxLeft.IsFocused)
            {
                this.TextBoxLeft.Paste();
            }
        }

        private void DeleteText(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.TextBoxLeft.IsFocused)
            {
                double offset = this.TextBoxLeft.VerticalOffset;
                int cursorPosition = this.TextBoxLeft.SelectionStart;
                this.TextBoxLeft.Text = this.TextBoxLeft.Text.Remove(this.TextBoxLeft.SelectionStart, this.TextBoxLeft.SelectionLength);
                this.TextBoxLeft.CaretIndex = cursorPosition;
                this.TextBoxLeft.ScrollToVerticalOffset(offset);
            }
        }

        private void SelectAllText(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.TextBoxLeft.IsEnabled)
            {
                this.TextBoxLeft.SelectAll();
            }
            else if (this.TextBoxRight.IsEnabled)
            {
                this.TextBoxRight.SelectAll();
            }
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
            if (this._openedFileName is null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "LOI file (*.loi)|*.loi"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    this._openedFileName = saveFileDialog.FileName;
                    this.Title = $"LOI - {this._openedFileName}";
                }
            }
            if (this._openedFileName is not null)
            {
                File.WriteAllText(this._openedFileName, this.TextBoxLeft.Text);
                this.Title = $"LOI - {this._openedFileName}";
                this._isContentEdited = false;
            }
        }

        private void ExitApp()
        {
            this.Close();
        }

        public void Dispose()
        {
            this._worker.Dispose();
        }
    }
}