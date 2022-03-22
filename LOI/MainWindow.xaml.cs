using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Threading;
using Common.Extensions;
using Common.Parsers;

namespace LOI
{
    public partial class MainWindow : Window
    {
        private bool IsPathShown = false;
        private string fileName = null;
        private int LineCounter = 1;
        
        private bool isContentEdited = false;

        private IParser parser;
        private BackgroundWorker worker;

        public MainWindow()
        {
            this.parser = new SplitParser();
            
            this.InitializeBackgroundWorker();
            this.InitializeComponent();
        }

        private void InitializeBackgroundWorker()
        {
            this.worker = new BackgroundWorker();
            this.worker.WorkerReportsProgress = true;
            this.worker.WorkerSupportsCancellation = true;
            this.worker.DoWork += this.worker_DoWork;
            this.worker.RunWorkerCompleted += this.worker_RunWorkerCompleted;
            this.worker.ProgressChanged += this.worker_ProgressChanged;
        }

        private void TextBoxLeft_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isContentEdited)
            {
                this.isContentEdited = true;
                this.Title = $"LOI - *" + (fileName ?? "Unnamed");
            }

            if (worker.IsBusy)
            {
                worker.CancelAsync();
            }
            this.TextBoxRight.Clear();
            this.LineCounter = 1;
                
            try
            {
                Thread.Sleep(50);
                this.worker.RunWorkerAsync(this.TextBoxLeft.Text);
            }
            catch
            {
                this.worker.Dispose();
                this.InitializeBackgroundWorker();
                this.worker.RunWorkerAsync(this.TextBoxLeft.Text);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                this.IsPathShown = !this.IsPathShown;
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

        private void CreateNewFile()
        {
            if (isContentEdited)
            {
                this.SaveFile();
                this.isContentEdited = false;
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
                Filter = "LOI file (*.loi)|*.loi"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.fileName = openFileDialog.FileName;
            }
            if (fileName is not null)
            {
                this.TextBoxLeft.Text = File.ReadAllText(fileName);
                this.Title = $"LOI - {fileName}";
                this.isContentEdited = false;
            }
        }

        private void SaveFile()
        {
            if (fileName is null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "LOI file (*.loi)|*.loi"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    this.fileName = saveFileDialog.FileName;
                    this.Title = $"LOI - {fileName}";
                }
            }
            if (fileName is not null)
            {
                File.WriteAllText(fileName, this.TextBoxLeft.Text);
                this.Title = $"LOI - {fileName}";
                isContentEdited = false;
            }
        }

        private void ExitApp()
        {
            this.Close();
        }

        private void ShowHelp(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Программа решает задачу перебора всевозможных вариантов для заданной грамматики.\n" + 
                "При задании правил вывода обязательно надо включить аксиому \"S\", т.к. для программы это является " +
                "начальным местом для перебора вариантов. Пример, как может выглядеть задание правил вывода:\n" +
                "S -> AAb | %\n" +
                "A -> Sa | c\n" +
                "В правилах вывода символ \"%\" обозначает пустое слово. Для программы все символы в нижнем регистре " +
                "и верхнем регистрах считаются соответственно треминалами и нетерминалами.\n" +
                "Если необходимо посмотреть цепочку вывода, то нажатие \"F5\" переключает вид между показом цепочек " +
                "вывода и не показом.", 
                "Help", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.Yes);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var rules = this.parser.Parse(e.Argument as string);
            if (rules is null)
            {
                e.Cancel = true;
                return;
            }

            var queue = new Queue<string>();
            var queuePaths = new Queue<List<string>>();
            queue.Enqueue("S");
            queuePaths.Enqueue(new List<string>());
            HashSet<string> visited = new HashSet<string>();

            int counterResults = 0;
            int counter = 0;

            while (queue.Count > 0 && counterResults < 1000)
            {
                if (worker.CancellationPending)
                {
                    return;
                }

                var current = queue.Dequeue();
                var currentPath = queuePaths.Dequeue();
                visited.Add(current);

                bool flag = true;
                foreach (var rule in rules)
                {
                    if (current.Contains(rule.Item1))
                    {
                        flag = false;
                        foreach (var right in rule.Item2)
                        {
                            foreach (var index in current.IndexesOf(rule.Item1))
                            {
                                string next = current.Substring(0, index) +
                                              right +
                                              current.Substring(index + rule.Item1.Length);
                                next = next.Replace("%", "");
                                if (!visited.Contains(next) && !queue.Contains(next))
                                {
                                    queue.Enqueue(next);
                                    var newPath = new List<string>(currentPath);
                                    newPath.Add(current);
                                    queuePaths.Enqueue(newPath);
                                }
                            }
                        }
                    }
                }

                if (flag)
                {
                    if (current == current.ToLower())
                    {
                        worker.ReportProgress(1, new Tuple<string, List<string>>(current, currentPath));
                        counterResults++;
                        Thread.Sleep(5);
                    }

                    counter = 0;
                }

                counter += 1;
            }

            e.Result = counterResults != 1000;
        }
        
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Result != null)
            {
                if ((bool)e.Result)
                {
                    this.TextBoxRight.Text = $"# All possible options have been checked!\r\n{this.TextBoxRight.Text}";
                }
                else
                {
                    this.TextBoxRight.Text = $"# Stopped! Checked 1000 options!\r\n{this.TextBoxRight.Text}";
                }
            }
        }

        private void worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            var value = e.UserState as Tuple<string, List<string>>;
            if (this.LineCounter != 1)
            {
                this.TextBoxRight.Text += "\r\n";
            }
            this.TextBoxRight.Text += $"{this.LineCounter}. " + (value?.Item1 == "" ? "\"Пустая строка\"" : value?.Item1);
            
            if (this.IsPathShown)
            {
                this.TextBoxRight.Text += "\r\n  " + string.Join(" => ", value.Item2) + 
                                          $" => {(value?.Item1 == "" ? "\"Пустая строка\"" : value?.Item1)}\n";
            }

            this.LineCounter++;
        }
    }
}