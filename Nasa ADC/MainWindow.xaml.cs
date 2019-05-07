using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.Windows.Shapes.Path;


namespace Nasa_ADC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public double pitch = 0;
        public double yaw = 0;
        public double roll = 0;
        static public double[] pos1 = new double[3];
        static public double[] pos3 = new double[3];
        static public double[] pos2 = new double[3];
        static public double[] quat1 = new double[4];
        static public double[] quat2 = new double[4];
        static public double[] quat3 = new double[4];
        static public int engine_flag;
        static public int reserved;
        static public double MET;
        public static byte[] theRawBuffer = new byte[22 * 8]; // 22 parameters, 8 bytes eac
        public MainWindow()
        {
            InitializeComponent();
            BackgroundWorker RecvWork = new BackgroundWorker();
            RecvWork.DoWork += RecvWork_DoWork;
            RecvWork.RunWorkerAsync();
            BackgroundWorker UIWork = new BackgroundWorker();
            UIWork.DoWork += UIWork_DoWork;
            UIWork.RunWorkerAsync();
        }

        private async void UIWork_DoWork(object sender, DoWorkEventArgs e)
        {
           
            while (true)
            {
                //euler2.Text = "Command Module: Yaw = " + ToDegree(GetEulerAngles(quat2).Item1) + " Pitch = " + ToDegree(GetEulerAngles(quat2).Item2) + " Roll = " + ToDegree(GetEulerAngles(quat2).Item3);
                var yawd = ToDegree(GetEulerAngles(quat1).Item1);
                var pitchd = ToDegree(GetEulerAngles(quat1).Item2);
                var rolld = ToDegree(GetEulerAngles(quat1).Item3);
                //233
                //258
                Dispatcher.Invoke((Action)(() =>
                {
                    
                    rPoint.Margin = new Thickness(689 + ((yawd) / 2), 133 + (((pitchd - 90) / 2) * -1), 0, 0);
                    bruh.Text = yawd.ToString();
                    bruh2.Text = pitchd - 90 + "";
                    bruh3.Text = rolld.ToString();
                    rHeight.Margin = new Thickness(233, 258 + (-pos1[0] / 500), 0, 0);
                    rHL.Margin = new Thickness(244, 258 + (-pos1[0] / 500), 0, 0);
                    tHeight.Margin = new Thickness(244, 237 + (-pos1[0] / 500), 0, 0);
                    tHeight.Text = pos1[0] + "ft";
                }));
                await Task.Delay(1);
            }
        }

        private void RecvWork_DoWork(object sender, DoWorkEventArgs e)
        {
            SReceive();
            

        }



        public Tuple<double, double, double> GetEulerAngles(double[] q)
        {

            double sinr_cosp = +2.0 * (q[0] * q[1] + q[2] * q[3]);
            double cosr_cosp = +1.0 - 2.0 * (q[1] * q[1] + q[2] * q[2]);
            roll = Math.Atan2(sinr_cosp, cosr_cosp);

            double sinp = +2.0 * (q[0] * q[2] - q[3] * q[1]);
            if (Math.Abs(sinp) >= 1)
            {
                if (sinp > 0)
                {
                    pitch = Math.PI / 2; // use 90 degrees if out of range
                }
                if (sinp < 0)
                {
                    pitch = -Math.PI / 2;
                }
            }
            else
            {
                pitch = Math.Asin(sinp);
            }

            double siny_cosp = +2.0 * (q[0] * q[3] + q[1] * q[2]);
            double cosy_cosp = +1.0 - 2.0 * (q[2] * q[2] + q[3] * q[3]);
            yaw = Math.Atan2(siny_cosp, cosy_cosp);

            return Tuple.Create(yaw, pitch, roll);

            /*
            double w2 = q[0] * q[0];
            double x2 = q[1] * q[1];
            double y2 = q[2] * q[2];
            double z2 = q[3] * q[3];
            double unitLength = w2 + x2 + y2 + z2;    // Normalised == 1, otherwise correction divisor.
            double abcd = q[0] * q[1] + q[2] * q[3];
            double eps = 1e-7; // Epsilon
            double pi = Math.PI;   
            if (abcd > (0.5 - eps) * unitLength)
            {

                yaw = 2 * Math.Atan2(q[2], q[2]);
                pitch = pi;
                roll = 0;

                return Tuple.Create(yaw, pitch, roll);
            }
            else if (abcd < (-0.5 + eps) * unitLength)
            {
                yaw = -2 * Math.Atan2(q[2], q[0]);
                pitch = -pi;
                roll = 0;

                return Tuple.Create(yaw, pitch, roll);
            }
            else
            {
                double adbc = q[0] * q[3] - q[1] * q[2];
                double acbd = q[0] * q[2] - q[1] * q[3];
                yaw = Math.Atan2(2 * adbc, 1 - 2 * (z2 + x2));
                pitch = Math.Asin(2 * abcd / unitLength);
                roll = Math.Atan2(2 * acbd, 1 - 2 * (y2 + x2));

                return Tuple.Create(yaw, pitch, roll);
            }
			*/
        }

        public async Task SReceive()
        {
            UdpClient client = new UdpClient();

            client.ExclusiveAddressUse = false;
            IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 42055);

            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.ExclusiveAddressUse = false;

            client.Client.Bind(localEp);

            IPAddress multicastaddress = IPAddress.Parse("237.7.7.7");
            client.JoinMulticastGroup(multicastaddress);

            

            while (true)
            {
                Byte[] data = client.Receive(ref localEp);
                

                
                await setBuffer(data);
            }
        }
        public static void print_payload() // for debug purposes
        {
            System.Diagnostics.Debug.WriteLine("Pos1 =" + pos1[0] + "," + pos1[1] + "," + pos1[2]);
            System.Diagnostics.Debug.WriteLine("Pos2 =" + pos2[0] + "," + pos2[1] + "," + pos2[2]);
            System.Diagnostics.Debug.WriteLine("Pos3 =" + pos3[0] + "," + pos3[1] + "," + pos3[2]);
            System.Diagnostics.Debug.WriteLine("Quat1=" + quat1[0] + "," + quat1[1] + "," + quat1[2] + "," + quat1[3]);
            System.Diagnostics.Debug.WriteLine("Quat2=" + quat2[0] + "," + quat2[1] + "," + quat2[2] + "," + quat2[3]);
            System.Diagnostics.Debug.WriteLine("Quat3=" + quat3[0] + "," + quat3[1] + "," + quat3[2] + "," + quat3[3]);
            System.Diagnostics.Debug.WriteLine("Eng_Flag=" + engine_flag + ", reservered=" + reserved);
            
        }

        // Sets all fields to 0
        public void payload()
        {
            for (int i = 0; i < 3; i++) { pos1[i] = 0; pos2[i] = 0; pos3[i] = 0; }
            for (int i = 0; i < 4; i++) { quat1[i] = 0; quat2[i] = 0; quat3[i] = 0; }
            engine_flag = 0; reserved = 0;
        }

        // sets theRawBuffer[] to have same value as the specified newBuffer
        public async Task setBuffer(byte[] newBuffer)
        {
            int cpySize = Math.Min(theRawBuffer.Length, newBuffer.Length);
            Array.Copy(newBuffer, 0, theRawBuffer, 0, cpySize);
            unpackBuffer();
        }

        

        // Casts 8 bytes of theRawBuffer (starting at offset) as a double (in LittleEndian form), and returns it //
        public static double extractDouble(int offset)
        {
            byte[] bytes = new byte[8];  // One doubles worth of data
            Array.Copy(theRawBuffer, offset, bytes, 0, 8);
            
           
            double value = BitConverter.ToDouble(bytes, 0);
            
            return (value);
        }

        // Casts 4 bytes of theRawBuffer (starting at offset) as an int (in LittleEndian form), and returns it //
        public static int extractInt(int offset)
        {
            byte[] bytes = new byte[4];  // One Ints worth of data
            Array.Copy(theRawBuffer, offset, bytes, 0, 4);
            
            
            
            int value = BitConverter.ToInt32(bytes, 0);
            
            return (value);
        }

        public double ToDegree(double rad)
        {
            double ang = rad * (180 / Math.PI);
            return ang;
        }

        // Sets all the parameters to the values that they have in theRawBuffer buffer
        public static void unpackBuffer()
        {
            int offset = 0;
            
            for (int i = 0; i < 3; i++, offset += 8) { pos1[i] = extractDouble(offset); }
            for (int i = 0; i < 3; i++, offset += 8) { pos2[i] = extractDouble(offset); }
            for (int i = 0; i < 3; i++, offset += 8) { pos3[i] = extractDouble(offset); }
            for (int i = 0; i < 4; i++, offset += 8) { quat1[i] = extractDouble(offset); }
            for (int i = 0; i < 4; i++, offset += 8) { quat2[i] = extractDouble(offset); }
            for (int i = 0; i < 4; i++, offset += 8) { quat3[i] = extractDouble(offset); }
            engine_flag = extractInt(offset); offset += 4;
            reserved = extractInt(offset); offset += 4;
            //print_payload();
        }

    }
    
}
