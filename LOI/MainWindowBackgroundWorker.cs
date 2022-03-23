using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Common.Extensions;

namespace LOI
{
    public partial class MainWindow : IDisposable
    {
        private readonly int _timeDelayBetweenPrint = 5;

        private void InitializeBackgroundWorker()
        {
            this._worker = new();
            this._worker.WorkerReportsProgress = true;
            this._worker.WorkerSupportsCancellation = true;
            this._worker.DoWork += this.Worker_DoWork;
            this._worker.ProgressChanged += this.Worker_ProgressChanged;
            this._worker.RunWorkerCompleted += this.Worker_RunWorkerCompleted;
        }
        
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var rules = this._parser.Parse(e.Argument as string);
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
                if (_worker.CancellationPending)
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
                        this._worker.ReportProgress(1, new Tuple<string, List<string>>(current, currentPath));
                        counterResults++;
                        Thread.Sleep(this._timeDelayBetweenPrint);
                    }

                    counter = 0;
                }

                counter += 1;
            }

            e.Result = counterResults != 1000;
        }

        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            bool flagSelectAll = this.TextBoxRight.Text.Length == this.TextBoxRight.SelectionLength;

            var value = e.UserState as Tuple<string, List<string>>;
            if (this._lineCounter != 1)
            {
                this.TextBoxRight.Text += "\r\n";
            }
            this.TextBoxRight.Text += $"{this._lineCounter}. " +
                                      (value?.Item1 == "" ? "\"Empty string\"" : value?.Item1);

            if (this._isPathShown)
            {
                if (value != null)
                    this.TextBoxRight.Text += "\r\n  " + string.Join(" => ", value.Item2) +
                                              $" => {(value?.Item1 == "" ? "\"Empty string\"" : value?.Item1)}\n";
            }

            this.TextBoxInfo.Text = $"[INFO] Processing... Find {_lineCounter} options.";
            this._lineCounter++;

            if (flagSelectAll)
            {
                this.TextBoxRight.SelectAll();
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                this.TextBoxInfo.Text = $"[ERROR] Please check the correctness of the grammar you entered.";
            }
            else if (!e.Cancelled && e.Result != null)
            {
                this.TextBoxInfo.Text = (bool)e.Result ?
                    ($"[INFO] All possible options have been checked! Totally found {_lineCounter - 1}" + (_lineCounter == 2 ? "option." : "options.")) : 
                    "[INFO] Stopped! Checked 1000 options!";
            }
        }
    }
}