using NetEti.ApplicationEnvironment;
using NetEti.CustomControls;
using System;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace VishnuMessageBox
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Wenn keine Zeilenumbrüche mitgegeben werden, wird als Maximalbreite
        /// für das Meldungsfenster DEFAULTMAXWIDTH Pixel angesetzt.
        /// </summary>
        public const int DEFAULTMAXWIDTH = 600;

        /// <summary>
        /// Wird beim Start der Anwendung durchlaufen.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            TimerMessageBox timerMessageBox = new();
            EvaluateParametersOrDie(e.Args, timerMessageBox);
            switch (timerMessageBox.ShowDialog())
            {
                case System.Windows.MessageBoxResult.None:
                case System.Windows.MessageBoxResult.OK:
                case System.Windows.MessageBoxResult.Yes:
                    Environment.Exit(0);
                    break;
                case System.Windows.MessageBoxResult.Cancel:
                case System.Windows.MessageBoxResult.No:
                    Environment.Exit(1);
                    break;
                default:
                    Environment.Exit(-1);
                    break;
            }
        }

        private void EvaluateParametersOrDie(string[] args, TimerMessageBox timerMessageBox)
        {
            CommandLineAccess commandLineAccess = new();

            MessageBoxIcons messageBoxIcon = MessageBoxIcons.None;
            MessageBoxButtons messageBoxButton = MessageBoxButtons.None;
            int lifeTimeMilliSeconds = TimerMessageBox.INFINITE;
            string? tmpStr = commandLineAccess.GetStringValue("EscalationCounter", "0");
            bool isResetting = Int32.TryParse(tmpStr, out int escalationCounter) && escalationCounter < 0;

            string? caption = commandLineAccess.GetStringValue("Caption", "Information") ?? "Information";
            StringBuilder sb = new StringBuilder();

            // tmpStr = commandLineAccess.GetStringValue("Vishnu.TreeInfo", "");
            // tmpStr = commandLineAccess.GetStringValue("Vishnu.NodeInfo", "");

            tmpStr = commandLineAccess.GetStringValue("LifeTimeMilliSeconds", null);
            if (Int32.TryParse(tmpStr, out int lifetimeMilliSeconds) && lifetimeMilliSeconds > 0)
            {
                lifeTimeMilliSeconds = lifetimeMilliSeconds;
            }

            string msg = commandLineAccess.GetStringValue("Message", null)
                ?? Die<string>("Es muss ein Meldungstext mitgegeben werden.", commandLineAccess.CommandLine);

            tmpStr = commandLineAccess.GetStringValue("MessageNewLine", null);
            string[] messageLines;
            if (!String.IsNullOrEmpty(tmpStr))
            {
                messageLines = msg.Split(tmpStr);
            }
            else
            {
                messageLines = new string[1] { msg };
                timerMessageBox.MaxWidth = DEFAULTMAXWIDTH;
            }
            string delim = "";
            for (int i = 0; i < messageLines.Count(); i++)
            {
                sb.Append(delim + messageLines[i]);
                delim = Environment.NewLine;
            }
            string message = sb.ToString();
            if (isResetting)
            {
                message = "Das Problem ist behoben. Die ursprüngliche Meldung war:"
                        + Environment.NewLine
                        + message;
            }

            messageBoxIcon = MessageBoxIcons.Information;
            if (caption.ToUpper().Contains("ERROR") || caption.ToUpper().Contains("FEHLER")
                || caption.ToUpper().Contains("EXCEPTION"))
            {
                messageBoxIcon = MessageBoxIcons.Error;
            }
            else
            {
                if (caption.ToUpper().StartsWith("WARN") == true)
                {
                    messageBoxIcon = MessageBoxIcons.Exclamation;
                }
            }
            messageBoxButton = MessageBoxButtons.OK;
            if (messageBoxIcon == MessageBoxIcons.Information && lifeTimeMilliSeconds != TimerMessageBox.INFINITE)
            {
                messageBoxIcon = MessageBoxIcons.Working;
                messageBoxButton = MessageBoxButtons.Cancel;
            }

            if (isResetting)
            {
                messageBoxIcon = MessageBoxIcons.Information;
                caption = "Entwarnung";
            }

            tmpStr = commandLineAccess.GetStringValue("Position", null);
            if (tmpStr != null)
            {
                string[] posStrings = Regex.Split(tmpStr, @"[;,|:]");
                if (posStrings.Length == 2)
                {
                    if (!Double.TryParse(posStrings[0].Trim(), out double posX))
                        Die<string>("Die X-Position nicht numerisch!", commandLineAccess.CommandLine);
                    if (!Double.TryParse(posStrings[1].Trim(), out double posY))
                        Die<string>("Die Y-Position nicht numerisch!", commandLineAccess.CommandLine);
                    double maxX = SystemParameters.VirtualScreenWidth - 210;
                    double maxY = SystemParameters.VirtualScreenHeight - 180;
                    double minX = 0;
                    double minY = 0;
                    if (posX > maxX) posX = maxX;
                    if (posX < minX) posX = minX;
                    if (posY > maxY) posY = maxY;
                    if (posY < minY) posY = minY;

                    timerMessageBox.Position = new Point(posX, posY);
                }
            }
            timerMessageBox.Text = message;
            timerMessageBox.Buttons = messageBoxButton;
            timerMessageBox.Icon = messageBoxIcon;
            timerMessageBox.LifeTimeMilliSeconds = lifeTimeMilliSeconds;
            timerMessageBox.Caption = caption;
        }

        private static T Die<T>(string? message, string? commandLine = null)
        {
            string usage = "Syntax:"
                + Environment.NewLine
                + "\t-Message=<Nachricht>"
                + Environment.NewLine
                + "\t[-Caption=<Überschrift>]"
                + Environment.NewLine
                + "\t[-LifeTimeMilliSeconds=<Zeit bis zum Meldungsende>]"
                + Environment.NewLine
                + "\t[-MessageNewLine=<NewLine-Kennung>]"
                + Environment.NewLine
                + "\t[-Position=X;Y]"
                + Environment.NewLine
                + "\t[-EscalationCounter={-n;+n} (negativ: Ursache behoben)]"
                + Environment.NewLine
                + "Beispiel:"
                + Environment.NewLine
                + "\t-Message=\"Server-1:#Zugriffsproblem#Connection-Error...\""
                + Environment.NewLine
                + "\t-Caption=\"SQL-Exception\""
                + Environment.NewLine
                + "\t-MessageNewLine=\"#\"";
            if (commandLine != null)
            {
                usage = "Kommandozeile: " + commandLine + Environment.NewLine + usage;
            }
            MessageBox.Show(message + Environment.NewLine + usage);
            throw new ArgumentException(message + Environment.NewLine + usage);
        }

    }
}
