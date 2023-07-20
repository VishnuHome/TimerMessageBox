using NetEti.ApplicationEnvironment;
using NetEti.CustomControls;
using System;
using System.Linq;
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
            string caption = string.Empty;

            string? tmpStr = commandLineAccess.GetStringValue("Vishnu.EscalationCounter", "1");
            if (!Int32.TryParse(tmpStr, out int escalationCounter)) Die<string>("Der EscalationCounter ist nicht numerisch!");
            caption = commandLineAccess.GetStringValue("Caption", "Information") ?? "Information";
            StringBuilder sb = new StringBuilder();
            string delim = "";
            if (escalationCounter < 0)
            {
                caption = "Entwarnung";
                sb.Append("Das Problem ist behoben. Die ursprüngliche Meldung war:");
                delim = Environment.NewLine;
            }

            // tmpStr = commandLineAccess.GetStringValue("Vishnu.TreeInfo", "");
            // tmpStr = commandLineAccess.GetStringValue("Vishnu.NodeInfo", "");

            tmpStr = commandLineAccess.GetStringValue("LifeTimeMilliSeconds", null);
            if (Int32.TryParse(tmpStr, out int lifetimeMilliSeconds) && lifetimeMilliSeconds > 0)
            {
                lifeTimeMilliSeconds = lifetimeMilliSeconds;
            }

            string msg = commandLineAccess.GetStringValue("Message", null)
                ?? Die<string>("Es muss ein Meldungstext mitgegeben werden.");

            tmpStr = commandLineAccess.GetStringValue("MessageNewLine", null);
            string[] messageLines;
            if (!String.IsNullOrEmpty(tmpStr))
            {
                messageLines = msg.Split(tmpStr);
            }
            else
            {
                messageLines = new string[1] { msg };
            }
            for (int i = 0; i < messageLines.Count(); i++)
            {
                sb.Append(delim + messageLines[i]);
                delim = Environment.NewLine;
            }
            string message = sb.ToString();
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
                    caption = "Warnung";
                    messageBoxIcon = MessageBoxIcons.Exclamation;
                }
            }
            messageBoxButton = MessageBoxButtons.OK;
            if (messageBoxIcon == MessageBoxIcons.Information && lifeTimeMilliSeconds != TimerMessageBox.INFINITE)
            {
                messageBoxIcon = MessageBoxIcons.Working;
                messageBoxButton = MessageBoxButtons.Cancel;
            }

            tmpStr = commandLineAccess.GetStringValue("Position", null);
            if (tmpStr != null)
            {
                string[] posStrings = Regex.Split(tmpStr, @"[;,|:]");
                if (posStrings.Length == 2)
                {
                    if (!Double.TryParse(posStrings[0].Trim(), out double posX)) Die<string>("Die X-Position nicht numerisch!");
                    if (!Double.TryParse(posStrings[1].Trim(), out double posY)) Die<string>("Die Y-Position nicht numerisch!");
                    double maxX = System.Windows.SystemParameters.VirtualScreenWidth - 210;
                    double maxY = System.Windows.SystemParameters.VirtualScreenHeight - 180;
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

        private static T Die<T>(string? message)
        {
            string usage = "Parameter: "
                + "[Vishnu.EscalationCounter=<Aufruf-Zähler>] [Vishnu.TreeInfo=<Tree-Info>] [Vishnu.NodeInfo=<Node-Info>]"
                + " Message=<Nachricht>" + " [Caption=<Überschrift>] + [LifeTimeMilliSeconds=<Zeit bis zum Meldungsende>]"
                + " [MessageNewLine=<NewLine-Kennung>] [Position=X;Y]"
                + Environment.NewLine + "Beispiel:  Vishnu.EscalationCounter=-1 Vishnu.TreeInfo=\"Tree 1\" Vishnu.NodeInfo=\"Root\""
                + " Message=\"Server-1 Exception: # Da ist was schiefgelaufen # Connection-Error...\" Caption=\"SQL-Exception\""
                + " MessageNewLine=\"#\"";
            MessageBox.Show(message + Environment.NewLine + usage);
            throw new ArgumentException(message + Environment.NewLine + usage);
        }

    }
}
