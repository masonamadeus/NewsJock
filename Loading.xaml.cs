using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Shapes;
using System.Diagnostics;

namespace NewsBuddy
{
    /// <summary>
    /// Interaction logic for Loading.xaml
    /// </summary>
    public partial class Loading : Window
    {
        // i know this is fucked up okay don't come at me im new.
        private Thread thread;
        private Loading window;
        public bool canAbort;

        public Loading()
        {
            InitializeComponent();
        }

        public void NewBar()
        {
            this.thread = new Thread(this.RunThread);
            this.thread.IsBackground = true;
            this.thread.SetApartmentState(ApartmentState.STA);
            this.thread.Start();
        }

        public void RunThread()
        {
            this.window = new Loading();
            this.window.Closed += new EventHandler(Window_Closed);
            this.window.ShowDialog();
        }


        public int progress = 0;
        public void TaskDone()
        {
            if (this.window != null)
            {
                this.window.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)
                    (() =>
                    {
                        this.window.Close();
                    }));
                while (!canAbort) { };              
            }
            this.thread.Abort();
        }

        public void UpdateProgress(string task, int progress, bool indeterminate = false)
        {
            if (this.window != null)
            {
                this.window.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)
                    (()=>
                    {
                        this.window.progDesc.Content = task;
                        this.window.progBar.IsIndeterminate = indeterminate;
                        if (indeterminate == false)
                        {
                            this.progBar.Value += progress;
                        }
                    }));
            }
           
            Trace.WriteLine("Doing " + task + " adding " + progress.ToString());
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Dispatcher.CurrentDispatcher.InvokeShutdown();
            this.canAbort = true;
        }
    }






}
