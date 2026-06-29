using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace LptControlApp
{

    public partial class MainWindow : Window
    {
        private LptManager lpt = new LptManager();
        private ReadStateMachine readSM;
        private WriteStateMachine writeSM;

        public MainWindow()
        {
            InitializeComponent();
            PortSelector.Items.Add("LPT1");
            PortSelector.Items.Add("LPT2");
            PortSelector.Items.Add("LPT3");

            readSM = new ReadStateMachine(lpt, msg => TxtLog.AppendText(msg + "\n"));
            writeSM = new WriteStateMachine(lpt, msg => TxtLog.AppendText(msg + "\n"));
        }

        private void RunReadStateMachine_Click(object sender, RoutedEventArgs e)
        {
            if (readSM == null)
            {
                TxtLog.AppendText("READ State Machine not initialized.\n");
                return;
            }

            readSM.Start();

            Task.Run(() =>
            {
                while (readSM.State != ReadState.Idle)
                {
                    readSM.Step();
                    Thread.Sleep(100);
                }
            });

            TxtLog.AppendText("READ State Machine started.\n");
        }

        private void RunWriteStateMachine_Click(object sender, RoutedEventArgs e)
        {
            if (writeSM == null)
            {
                TxtLog.AppendText("WRITE State Machine not initialized.\n");
                return;
            }

            byte[] data = { 0x1A, 0x2B };

            writeSM.Start(data);

            Task.Run(() =>
            {
                while (writeSM.State != WriteState.Idle)
                {
                    writeSM.Step();
                    Thread.Sleep(100);
                }
            });

            TxtLog.AppendText("WRITE State Machine started.\n");
        }

        private void BuildPacket_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Convert fields
                byte sof = AsciiEncoder.Encode(PktSOF.Text)[0];
                byte cmd = AsciiEncoder.Encode(PktCMD.Text)[0];

                byte[] addr = AsciiEncoder.Encode(PktADDR.Text);
                byte len = Convert.ToByte(PktLEN.Text, 16);

                byte[] data = AsciiEncoder.Encode(PktDATA.Text.Replace(" ", ""));

                // Build packet (SOF CMD ADDR LEN DATA CHK EOF)
                List<byte> packet = new List<byte>();
                packet.Add(sof);
                packet.Add(cmd);
                packet.AddRange(addr);
                packet.Add(len);
                packet.AddRange(data);

                // Compute checksum
                byte chk = 0;
                foreach (byte b in packet)
                    chk ^= b;

                PktCHK.Text = chk.ToString("X2");

                packet.Add(chk);
                packet.Add(Commands.EOF[0]);

                TxtLog.AppendText("Packet built: " + BitConverter.ToString(packet.ToArray()) + "\n");
            }
            catch (Exception ex)
            {
                TxtLog.AppendText("Packet build error: " + ex.Message + "\n");
            }
        }

        private void SendPacket_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Rebuild packet using BuildPacket_Click logic
                BuildPacket_Click(null, null);

                // Extract final packet from log (last line)
                string last = TxtLog.Text.Split('\n').LastOrDefault();
                if (!last.StartsWith("Packet built:"))
                {
                    TxtLog.AppendText("No packet to send.\n");
                    return;
                }

                string hex = last.Replace("Packet built: ", "").Trim();
                byte[] bytes = hex.Split('-').Select(h => Convert.ToByte(h, 16)).ToArray();

                bool ok = lpt.Send(bytes);
                TxtLog.AppendText(ok ? "Packet sent.\n" : "Packet send failed.\n");
            }
            catch (Exception ex)
            {
                TxtLog.AppendText("Packet send error: " + ex.Message + "\n");
            }
        }



        private void SendAscii_Click(object sender, RoutedEventArgs e)
        {
            string ascii = AsciiInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(ascii))
            {
                TxtLog.AppendText("ASCII console: No input.\n");
                return;
            }

            byte[] bytes = AsciiEncoder.Encode(ascii);

            if (bytes.Length == 0)
            {
                TxtLog.AppendText("ASCII console: Invalid ASCII/HEX input.\n");
                return;
            }

            bool ok = lpt.Send(bytes);

            TxtLog.AppendText(ok
                ? $"ASCII console: Sent [{ascii}] → {BitConverter.ToString(bytes)}\n"
                : $"ASCII console: Failed to send [{ascii}]\n");
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (PortSelector.SelectedItem is not string port)
            {
                TxtLog.AppendText("Select a port first.\n");
                return;
            }

            bool ok = lpt.Connect(@"\\.\\" + port);
            LblStatus.Content = ok ? "Status: Connected" : "Status: Failed";
            TxtLog.AppendText(ok ? $"Connected to {port}\n" : $"Failed to connect to {port}\n");
        }


        private void BtnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            lpt.Disconnect();
            LblStatus.Content = "Status: Disconnected";
            TxtLog.AppendText("Disconnected.\n");
        }

        private void SendSOF_Click(object sender, RoutedEventArgs e)
        {
            SendCommand(Commands.SOF, "SOF");
        }

        private void SendCMD_Click(object sender, RoutedEventArgs e)
        {
            SendCommand(Commands.CMD, "CMD");
        }

        private void SendDATA_Click(object sender, RoutedEventArgs e)
        {
            SendCommand(Commands.DATA, "DATA");
        }

        private void SendEOF_Click(object sender, RoutedEventArgs e)
        {
            SendCommand(Commands.EOF, "EOF");
        }

        private void SendCommand(byte[] cmd, string name)
        {
            bool ok = lpt.Send(cmd);
            TxtLog.AppendText(ok ? $"Sent {name}\n" : $"Failed to send {name}\n");
        }
    } 
}
