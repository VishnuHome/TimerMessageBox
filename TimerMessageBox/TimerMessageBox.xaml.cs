﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace NetEti.CustomControls
{
    /// <summary>
    /// Gibt die Buttons an, die in der MessageBox angezeigt werden.
    /// </summary>
    public enum MessageBoxButtons
    {
        /// <summary>Es soll kein Button angezeigt werden.</summary>
        None = 9,
        /// <summary>Es soll ein OK-Button angezeigt werden.</summary>
        OK = 0,
        /// <summary>Es soll ein Abbrechen-Button angezeigt werden.</summary>
        Cancel = 2,
        /// <summary>Es sollen ein OK-Button und ein Abbrechen-Button angezeigt werden.</summary>
        OKCancel = 1,
        /// <summary>Es sollen ein Ja-Button und ein Nein-Button angezeigt werden.</summary>
        YesNo = 4,
        /// <summary>Es sollen ein Ja-Button, ein Nein-Button und ein Abbrechen angezeigt werden.</summary>
        YesNoCancel = 3
    }

    /// <summary>
    /// Gibt das Symbol an, das in der MessageBox angezeigt wird.
    /// </summary>
    public enum MessageBoxIcons
    {
        /// <summary>Es wird kein Symbol angezeigt.</summary>
        None = 0,
        /// <summary>Das Meldungsfeld enthält ein Symbol, das aus einem weißen X in einem Kreis mit rotem Hintergrund besteht.</summary>
        Hand = 16,
        /// <summary>Das Meldungsfeld enthält ein Symbol, das aus einem weißen X in einem Kreis mit rotem Hintergrund besteht.</summary>
        Stop = 16,
        /// <summary>Das Meldungsfeld enthält ein Symbol, das aus einem weißen X in einem Kreis mit rotem Hintergrund besteht.</summary>
        Error = 16,
        /// <summary>
        /// Das Meldungsfeld enthält ein Symbol, das aus einem weißen Fragezeichen in einem blauen Kreis besteht.</summary>
        Question = 32,
        /// <summary>Das Meldungsfeld enthält ein Symbol, das aus einem schwarzen Ausrufezeichen in einem Dreieck mit orangem Hintergrund besteht.</summary>
        Exclamation = 48,
        /// <summary>Das Meldungsfeld enthält ein Symbol, das aus einem schwarzen Ausrufezeichen in einem Dreieck mit orangem Hintergrund besteht.</summary>
        Warning = 48,
        /// <summary>Das Meldungsfeld enthält ein Symbol, das aus einem weißen Kleinbuchstaben i in einem blauen Kreis besteht.</summary>
        Asterisk = 64,
        /// <summary>Das Meldungsfeld enthält ein Symbol, das aus einem weißen Kleinbuchstaben i in einem blauen Kreis besteht.</summary>
        Information = 64,
        /// <summary>Das Meldungsfeld enthält ein animiertes Symbol aus drei goldenen Zahnrädern für eine laufende Verarbeitung.</summary>
        Working = 128
    }

    /// <summary>
    /// Interaktionslogik für TimerMessageBox.xaml
    /// 13.02.2024 Erik Nagel: Windows.DragMove bei Erhaltung der Reaktionsfähigkeit anderer Controls (Buttons)
    ///                        implementiert (DelayedDragMove).
    /// </summary>
    public partial class TimerMessageBox : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Das INotifyPropertyChanged-Event: dient der UI zur Erkennung
        /// veränderter Properties.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Namensvergabe "Infinite" an den LifetimeMilliSeconds-Wert 0.
        /// </summary>
        public const int INFINITE = 0;

        /// <summary>
        /// Ist LifeTimeMilliSeconds größer als  0, dann schließt sich
        /// das Meldungsfenster nach LifeTimeMilliSeconds,
        /// Default: INFINITE (= 0).
        /// </summary>
        public int LifeTimeMilliSeconds { get; set; }

        /// <summary>
        /// Die Fenster-Überschrift, Default: "Information".
        /// </summary>
        public string? Caption {
            get
            {
                return this._caption;
            }
            set
            {
                if (this._caption != value)
                {
                    this._caption = value;
                    this.OnPropertyChanged("Caption");
                }
            }
        }

        /// <summary>
        /// Die Meldung, Default: "".
        /// </summary>
        public string? Text {
            get
            {
                return this._text;
            }
            set
            {
                if (this._text != value)
                {
                    this._text = value;
                    this.OnPropertyChanged(nameof(Text));
                }
            }
        }

        /// <summary>
        /// Das Icon links oben im Meldungsfenster.
        /// </summary>
        public string? WindowIconPath
        {
            get
            {
                return this._windowIconPath;
            }
            set
            {
                if (this._windowIconPath != value)
                {
                    this._windowIconPath = value;
                    this.OnPropertyChanged(nameof(WindowIconPath));
                }
            }
        }

        /// <summary>
        /// Das Symbol zur Meldung.
        /// </summary>
        public Uri? ImageUri
        {
            get
            {
                return this._imageUri;
            }
            set
            {
                if (this._imageUri != value)
                {
                    this._imageUri = value;
                    this.OnPropertyChanged(nameof(ImageUri));
                }
            }
        }

        /// <summary>
        /// Die Buttons der UserTimerMessageBox:
        /// None, OK, Cancel, OKCancel, YesNo, YesNoCancel
        /// </summary>
        public MessageBoxButtons? Buttons
        {
            get
            {
                return this._buttons;
            }
            set
            {
                this._buttons = value;
                this.SetButtons();
                this.OnPropertyChanged("ButtonsPanel");
            }
        }

        /// <summary>
        /// Das Icon der UserTimerMessageBox.
        /// </summary>
        public new MessageBoxIcons? Icon
        {
            get
            {
                return this._icon;
            }
            set
            {
                if (this._icon != value || this._icon == MessageBoxIcons.None)
                {
                    this._icon = value;
                    this.SetIcon();
                    this.OnPropertyChanged("MessageBoxIconImage");
                    this.OnPropertyChanged("MessageBoxIconAnimatedImage");
                }
            }
        }

        /// <summary>
        /// Die Startposition des Meldungsfensters.
        /// </summary>
        public Point Position {
            set
            {
                if (this._position != value)
                {
                    this._position = value;
                    this.Left = this._position.X;
                    this.Top = this._position.Y;
                    this._isPositionSet = true;
                    this.OnPropertyChanged("Position");
                }
            }
        }

        /// <summary>
        /// Das Ergebnis aus dieser MessageBox;
        /// None, OK, Cancel, Yes, No.
        /// </summary>
        public MessageBoxResult Result { get; set; }

        /// <summary>
        /// Standard Konstruktor - setzt Defaults für LifeTimeMilliSeconds,
        /// Caption, Text, Buttons, Icon, Result. 
        /// </summary>
        /// <param name="owner">Besitzendes Window oder (bei Page) null.</param>
        public TimerMessageBox(Window? owner = null)
        {
            if (owner != null)
            {
                this.Owner = owner;
            }
            InitializeComponent();
            this.WindowIconPath = "Media/Vishnu_18.png";
            this.LifeTimeMilliSeconds = INFINITE;
			this.Icon = MessageBoxIcons.Information;
            this.ResizeMode = ResizeMode.NoResize;
            this._position = new(-1, -1);
            // this.PreviewMouseLeftButtonDown += delegate { this.DragMove(); };
            this.PreviewMouseLeftButtonDown += this.previewMouseLeftButtonDown;
        }

        /// <summary>
        /// Zeigt die TimerMessageBox modal an.
        /// </summary>
        public new void Show()
        {
            if (this.Owner != null)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                if (this._isPositionSet)
                {
                    this.WindowStartupLocation = WindowStartupLocation.Manual;
                }
                else
                {
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            // MessageBox.Show($"bin hier TimerMessageBox.Show: {this.LifeTimeMilliSeconds}"); // 03.03.2024 Nagel+- DEBUG
            base.Show();
        }

        /// <summary>
        /// Zeigt die TimerMessageBox modal an und gibt ein MessageBoxResult zurück
        /// (None, OK, Cancel, Yes, No).
        /// </summary>
        /// <returns>Das MessageBoxResult: None, OK, Cancel, Yes, No</returns>
        public new MessageBoxResult ShowDialog()
        {
            if (this.Owner != null)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                if (this._isPositionSet)
                {
                    this.WindowStartupLocation = WindowStartupLocation.Manual;
                }
                else
                {
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            // MessageBox.Show($"bin hier TimerMessageBox.ShowDialog: {this.LifeTimeMilliSeconds}"); // 03.03.2024 Nagel+- DEBUG
            base.ShowDialog();
            return this.Result;
        }

        /// <summary>
        /// Kann zum Schließen der MessageBox von außen aufgerufen werden (Thread-safe).
        /// </summary>
        /// <returns>Das MessageBoxResult: None, OK, Cancel, Yes, No</returns>
        public void CloseMessageBox()
        {
            this.Dispatcher.Invoke(new Action(() => 
                {
                    // MessageBox.Show($"TimerMessageBox vor Close"); // 03.03.2024 Nagel+- DEBUG
                    this.Close();
                }
            ));
        }

        /// <summary>
        /// Löst bei Änderung der Property INotifyPropertyChanged aus.
        /// </summary>
        /// <param name="propertyName">Name der geänderten Property.</param>
        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetIcon()
        {
            // BitmapImage messageBoxIconImage = new BitmapImage();
            Uri? iconUri = null;
            this.MessageBoxIconImage.Visibility = Visibility.Visible;
            this.MessageBoxIconAnimatedImage.Visibility = Visibility.Hidden;
            switch (this.Icon)
            {
                case MessageBoxIcons.Hand & MessageBoxIcons.Stop & MessageBoxIcons.Error:
                    //messageBoxIconImage = Bitmap.FromHicon(SystemIcons.Hand.Handle);
                    iconUri = new Uri("Media/process-stop-5.png", UriKind.RelativeOrAbsolute);
                    break;
                case MessageBoxIcons.Question:
                    iconUri = new Uri("Media/dialog-question-2.png", UriKind.RelativeOrAbsolute);
                    break;
                case MessageBoxIcons.Exclamation & MessageBoxIcons.Warning:
                    iconUri = new Uri("Media/dialog-important-2.png", UriKind.RelativeOrAbsolute);
                    break;
                case MessageBoxIcons.Asterisk & MessageBoxIcons.Information:
                    iconUri = new Uri("Media/dialog-information-3.png", UriKind.RelativeOrAbsolute);
                    break;
                case MessageBoxIcons.Working:
                    //iconUri = new Uri("/TimerMessageBox;component/Media/gears2.gif", UriKind.RelativeOrAbsolute);
                    this.MessageBoxIconImage.Visibility = Visibility.Hidden;
                    this.MessageBoxIconAnimatedImage.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
            if (iconUri != null)
            {
                //this.BitmapImageSource = this.BitmapToBitmapImage(messageBoxIconImage);
                this._imageUri = iconUri;
                this.MessageBoxIconImage.Visibility = Visibility.Visible;
                this.OnPropertyChanged("ImageUri");
            }
            else
            {
                this.MessageBoxIconImage.Visibility = Visibility.Collapsed;
            }
        }

        private void SetButtons()
        {
            this.BtnOk.Visibility = Visibility.Collapsed;
            this.BtnYes.Visibility = Visibility.Collapsed;
            this.BtnNo.Visibility = Visibility.Collapsed;
            this.BtnCancel.Visibility = Visibility.Collapsed;
            this.ButtonsPanel.Visibility = Visibility.Visible;
            switch (this.Buttons)
            {
                case MessageBoxButtons.None:
                    this.ButtonsPanel.Visibility = Visibility.Collapsed;
                    
                    break;
                case MessageBoxButtons.OK:
                    this.BtnOk.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButtons.OKCancel:
                    this.BtnOk.Visibility = Visibility.Visible;
                    this.BtnCancel.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButtons.Cancel:
                    this.BtnCancel.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButtons.YesNo:
                    this.BtnYes.Visibility = Visibility.Visible;
                    this.BtnNo.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButtons.YesNoCancel:
                    this.BtnYes.Visibility = Visibility.Visible;
                    this.BtnNo.Visibility = Visibility.Visible;
                    this.BtnCancel.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private Uri? _imageUri { get; set; }
        private MessageBoxButtons? _buttons;
        private MessageBoxIcons? _icon;
        private string? _text;
        private string? _caption;
        private Timer? _messageBoxTimer;
        private string? _windowIconPath;
        private Point _position;
        private bool _isPositionSet;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.WindowStartupLocation == WindowStartupLocation.Manual)
            {
                // Rufe Measure und Arrange auf, um die gewünschte Größe zu berechnen
                Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));

                // Erhalte die gewünschte Größe aus der DesiredSize-Eigenschaft
                double desiredWidth = DesiredSize.Width;
                double desiredHeight = DesiredSize.Height;

                double desiredLeft = this._position.X - desiredWidth / 2.0;
                desiredLeft = desiredLeft < 0 ? 0 : desiredLeft;
                double desiredTop = this._position.Y - desiredHeight / 2.0;
                desiredTop = desiredTop < 0 ? 0 : desiredTop;

                this.Left = desiredLeft;
                this.Top = desiredTop;
            }
            if (this.LifeTimeMilliSeconds > 0)
            {
                _messageBoxTimer = new Timer();
                _messageBoxTimer.Interval = this.LifeTimeMilliSeconds;
                _messageBoxTimer.Elapsed += new ElapsedEventHandler(t_Elapsed);
                _messageBoxTimer.Start();
            }
        }

        private void t_Elapsed(object? sender, ElapsedEventArgs e)
        {
            this._messageBoxTimer?.Stop();
            this.CloseMessageBox();
        }

        private void CmdBtnCopy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sender as Button)?.Visibility == Visibility.Visible;
        }

        private void CmdBtnCopy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.SetText(this.MessageTextBlock.Text);
        }

        private void CmdBtnMinimize_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sender as Button)?.Visibility == Visibility.Visible;
        }

        private void CmdBtnMinimize_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CmdBtnClose_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sender as Button)?.Visibility == Visibility.Visible;
        }

        private void CmdBtnClose_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void CmdBtnOk_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sender as Button)?.Visibility == Visibility.Visible;
        }

        private void CmdBtnOk_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Result = MessageBoxResult.OK;
            this.CloseMessageBox();
        }

        private void CmdBtnYes_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sender as Button)?.Visibility == Visibility.Visible;
        }

        private void CmdBtnYes_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Result = MessageBoxResult.Yes;
            this.CloseMessageBox();
        }

        private void CmdBtnNo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sender as Button)?.Visibility == Visibility.Visible;
        }

        private void CmdBtnNo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Result = MessageBoxResult.No;
            this.CloseMessageBox();
        }

        private void CmdBtnCancel_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sender as Button)?.Visibility == Visibility.Visible;
        }

        private void CmdBtnCancel_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Result = MessageBoxResult.Cancel;
            this.CloseMessageBox();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //this.PreviewMouseLeftButtonDown -= this.previewMouseLeftButtonDown;
            //this._messageBoxTimer?.Dispose();
        }

        /*
        private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            BitmapImage bitmapImage = null;
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }
        */

        /// <summary>
        /// Linke Maustaste: 
        ///   dient zum Ziehen des Meldungsfensters.
        /// </summary>
        /// <param name="sender">Element, in dem das Event zuerst auftritt.</param>
        /// <param name="e">Weitergehende Informationen zum Event.</param>
        private void previewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            /*
            if (((Keyboard.Modifiers & ModifierKeys.Alt) > 0)
                || ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
                || ((Keyboard.Modifiers & ModifierKeys.Shift) > 0))
            {
                return; // Let it be free for others.
            }
            */

            // this.DragMove();
            // Window.DragMove() blockiert Mausklicks auf Buttons in Controls in
            // anderen Assemblies (UserControl, z.B. ZoomBox).
            // Deshalb muss DragMove() verzögert gestartet werden, um Buttons noch
            // die Möglichkeit zu geben, vorher zu reagieren.
            this.DelayedDragMove();
        }

        private void DelayedDragMove()
        {
            Task.Run(new Action(() => {
                Task.Delay(200).Wait(); // Important: if the application doesn't react on mouseclicks any longer,
                                        // set this delay to a higher value (100 won't work).
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed)
                        {
                            this.DragMove();
                        }
                    } catch (InvalidOperationException) { }
                }), DispatcherPriority.Send);
            }));
        }

    }
}
