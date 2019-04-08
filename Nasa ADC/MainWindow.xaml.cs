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

namespace Nasa_ADC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool altimeter_view = false;
        public bool position_view = false;
        public bool rot_view = false;
        public bool stage_view = false;
        public bool default_view = false;

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
        public static byte[] theRawBuffer = new byte[22 * 8]; // 22 parameters, 8 bytes eac

        // Constructer
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

        // Method that displays data in UI
        private async void UIWork_DoWork(object sender, DoWorkEventArgs e)
        {
            // "Game Loop"
            while (true)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    if (alti_view.IsPressed)
                    {
                        altimeter_view = true;
                        position_view = false;

                        rot_view = false;
                        euler1.Content = "";
                        euler2.Content = "";
                        euler3.Content = "";

                        stage_view = false;
                        engflag.Content = "";
                    } else if (pos_view.IsPressed)
                    {
                        altimeter_view = false;
                        position_view = true;

                        rot_view = false;
                        euler1.Content = "";
                        euler2.Content = "";
                        euler3.Content = "";

                        stage_view = false;
                        engflag.Content = "";
                    } else if (rotation_view.IsPressed)
                    {
                        altimeter_view = false;
                        position_view = false;
                        pos1c.Content = "";
                        pos2c.Content = "";
                        pos3c.Content = "";

                        rot_view = true;

                        stage_view = false;
                        engflag.Content = "";
                    } else if (staging_view.IsPressed)
                    {
                        altimeter_view = false;
                        position_view = false;
                        pos1c.Content = "";
                        pos2c.Content = "";
                        pos3c.Content = "";

                        rot_view = false;
                        euler1.Content = "";
                        euler2.Content = "";
                        euler3.Content = "";

                        stage_view = true;
                    } else if (restore_view.IsPressed)
                    {
                        altimeter_view = false;
                        position_view = false;
                        pos1c.Content = "";
                        pos2c.Content = "";
                        pos3c.Content = "";

                        rot_view = false;
                        euler1.Content = "";
                        euler2.Content = "";
                        euler3.Content = "";

                        stage_view = false;
                        engflag.Content = "";
                    }

                    if (altimeter_view)
                    {                    
                        pos1c.Content = "Altitude of LAS = " + pos1[0];

                        pos2c.Content = "Altitude of Command Module = " + pos2[0];

                        pos3c.Content = "Altitude of Main Thruster = " + pos3[0];
                    } else if (position_view)
                    {
                        pos1c.Content = "Position of LAS: Latitude = " + pos1[1] + " Longitude = " + pos1[2];

                        pos2c.Content = "Position of LAS: Latitude = " + pos2[1] + " Longitude = " + pos2[2];

                        pos3c.Content = "Position of LAS: Latitude = " + pos3[1] + " Longitude = " + pos3[2];
                    } else if (rot_view)
                    {
                        euler1.Content = "LAS: Yaw = " + GetEulerAngles(quat1).Item1 + " Pitch = " + GetEulerAngles(quat1).Item2 + " Roll = " + GetEulerAngles(quat1).Item3;

                        euler2.Content = "Command Module: Yaw = " + GetEulerAngles(quat2).Item1 + " Pitch = " + GetEulerAngles(quat2).Item2 + " Roll = " + GetEulerAngles(quat2).Item3;

                        euler1.Content = "Main Thruster: Yaw = " + GetEulerAngles(quat3).Item1 + " Pitch = " + GetEulerAngles(quat3).Item2 + " Roll = " + GetEulerAngles(quat3).Item3;
                    } else if (stage_view)
                    {
                        engflag.Content = "Stage = " + engine_flag;
                    }

                    /*
                    pos1c.Content = "Pos1 =" + pos1[0] + "," + pos1[1] + "," + pos1[2];

                    pos2c.Content = "Pos2 =" + pos2[0] + "," + pos2[1] + "," + pos2[2];

                    pos3c.Content = "Pos3 =" + pos3[0] + "," + pos3[1] + "," + pos3[2];

                    quat1c.Content = "Quat1=" + quat1[0] + "," + quat1[1] + "," + quat1[2] + "," + quat1[3];

                    quat2c.Content = "Quat2=" + quat2[0] + "," + quat2[1] + "," + quat2[2] + "," + quat2[3];

                    quat3c.Content = "Quat3=" + quat3[0] + "," + quat3[1] + "," + quat3[2] + "," + quat3[3];

                    euler1.Content = "LAS = " + "Yaw: " + GetEulerAngles(quat1).Item1 + " Pitch: " + GetEulerAngles(quat1).Item2 + " Roll: " + GetEulerAngles(quat1).Item3;

                    euler2.Content = "CM = " + "Yaw: " + GetEulerAngles(quat2).Item1 + " Pitch: " + GetEulerAngles(quat2).Item2 + " Roll: " + GetEulerAngles(quat2).Item3;

                    euler3.Content = "Launch Rocket = " + "Yaw: " + GetEulerAngles(quat3).Item1 + " Pitch: " + GetEulerAngles(quat3).Item2 + " Roll: " + GetEulerAngles(quat3).Item3;

                    engflag.Content = "Engine Flag: " + engine_flag;
                    */
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
        }

        // Extracts data from jar file through multicast
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
