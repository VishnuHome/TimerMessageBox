using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NetEti.CustomControls;

namespace NetEti.DemoApplications
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Standard Konstruktor.
        /// </summary>
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
            TimerMessageBox timerMessageBox = new TimerMessageBox(this);
            timerMessageBox.Buttons = MessageBoxButtons.Cancel;
            timerMessageBox.Icon = MessageBoxIcons.Working;
            timerMessageBox.Caption = "Wichtige Info";
            //string message = "lksfdjlkfdsgjlkjglkfdjglkdjslkjfdlgkjdl"
            //    + Environment.NewLine
            //    + "poipipoipoipoipoipoipoipoipoipoipoipoip"
            //    + Environment.NewLine
            //    + "mnbmnbmnbmnbmnbmnbmbnhjkj,nb,,h,jn,bmhb";
            string message = "Der Zustand wird gespeichert,\r\nMoment bitte ...";
            timerMessageBox.Text = message;
            //umb.LifeTimeMilliSeconds = UserTimerMessageBox.INFINITE; // Default

            // --- Variante 1 (modal) ----------------------------------
            
            timerMessageBox.LifeTimeMilliSeconds = 4000;
            (new TimerMessageBox() { Owner = this, Buttons = MessageBoxButtons.None, LifeTimeMilliSeconds = 1000,
                Icon = MessageBoxIcons.Working, Caption = "DialogResult", Text = timerMessageBox.ShowDialog().ToString()}).Show();
            
            // ---------------------------------------------------------

            // --- Variante 2 (nicht modal) ----------------------------
            // Nur für kleine Pseudo-Waits zu verwenden, z.B. wenn die eigentliche
            // Verarbeitung so schnell ist, dass der Anwender ohne eine Meldung nicht
            // merken würde, dass überhaupt etwas passiert ist. Ansonsten immer
            // Variante 1 verwenden. Die eigentliche Verarbeitung immmer in einem
            // eigenen Thread ausführen.
            /*
            umb.Buttons = MessageBoxButtons.None;
            umb.LifeTimeMilliSeconds = TimerMessageBox.INFINITE;
            umb.Show();
            */
            // Simuliert die eigentliche Verarbeitung
            /*
            Delay(10000).ContinueWith(_ =>
            {
                // Die Verarbeitung ist beendet und schließt jetzt das Meldungsfenster.
                umb?.CloseMessageBox();
            });
            */
            /*
            // Achtung: für Task.Delay ist mindestens Framework 4.5 erforderlich.
            Task.Delay(10000).ContinueWith(_ =>
            {
                umb?.CloseMessageBox();
            });
            */
            // ----------------------------------------------------------
        }

        static Task Delay(int milliseconds)
        {
            var tcs = new TaskCompletionSource<object>();
            new Timer(_ => tcs.SetResult(0)).Change(milliseconds, -1);
            return tcs.Task;
        }
    }
}
