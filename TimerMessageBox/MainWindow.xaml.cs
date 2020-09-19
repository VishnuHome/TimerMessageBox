using System;
using System.Windows;
using System.Windows.Input;

namespace TimerMessageBox
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CmdSaveState_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CmdSaveState_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UserTimerMessageBox umb = new UserTimerMessageBox();
            umb.Buttons = MessageBoxButtons.OK;
            umb.MessageBoxIcon = MessageBoxImage.Exclamation;
            string longMessage = "lksfdjlkfdsgjlkjglkfdjglkdjslkjfdlgkjdl"
                + Environment.NewLine
                + "poipipoipoipoipoipoipoipoipoipoipoipoip"
                + Environment.NewLine
                + "mnbmnbmnbmnbmnbmnbmbnhjkj,nb,,h,jn,bmhb";
            umb.MessageBoxText = longMessage;
            umb.MessageBoxHeader = "wichtige Info";
            umb.Owner = this;
            // --- Variante 1 ----------------------------
            umb.LifeTimeMilliSeconds = 4000;
            umb.ShowDialog();
            // -------------------------------------------

            // --- Variante 2 ----------------------------
            //umb.LifeTimeMilliSeconds = UserTimerMessageBox.INFINITE;
            //umb.Show();
            //Task.Delay(10000).ContinueWith(_ =>
            //{
            //    umb?.CloseMessageBox();
            //});
            // -------------------------------------------
        }

    }
}
