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
        static public double[] pos1 = new double[3];
        static public double[] pos3 = new double[3];
        static public double[] pos2 = new double[3];
        static public double[] quat1 = new double[4];
        static public double[] quat2 = new double[4];
        static public double[] quat3 = new double[4];
        static public int engine_flag;
        static public int reserved;
        public static byte[] theRawBuffer = new byte[22 * 8]; // 22 parameters, 8 bytes eac
        public MainWindow()
        {
            InitializeComponent();
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            SReceive();
            

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
                Dispatcher.Invoke((Action)(() =>
                {
                    pos1c.InvalidateVisual();
                    pos1c.Content = "Pos1 =" + pos1[0] + "," + pos1[1] + "," + pos1[2];
                    pos2c.InvalidateVisual();
                    pos2c.Content = "Pos2 =" + pos2[0] + "," + pos2[1] + "," + pos2[2];
                    pos3c.InvalidateVisual();
                    pos3c.Content = "Pos3 =" + pos3[0] + "," + pos3[1] + "," + pos3[2];
                    quat1c.InvalidateVisual();
                    quat1c.Content = "Quat1=" + quat1[0] + "," + quat1[1] + "," + quat1[2] + "," + quat1[3];
                    quat2c.InvalidateVisual();
                    quat2c.Content = "Quat2=" + quat2[0] + "," + quat2[1] + "," + quat2[2] + "," + quat2[3];
                    quat3c.InvalidateVisual();
                    quat3c.Content = "Quat3=" + quat3[0] + "," + quat3[1] + "," + quat3[2] + "," + quat3[3];
                }));

                
                await setBuffer(data);
            }
        }
        public static void print_payload()
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

        // reverses the order of bytes in a byte[] (first becomes last, and vice versa)
        public static byte[] reversebuffer(byte[] input)
        {
            byte[] rv = new byte[input.Length];
            //System.out.println(input.length);
            for (int i = 0, j = input.Length - 1; j >= 0; i++, j--)
            { rv[i] = input[j];  /*System.out.println("in["+i+"]="+rv[i]);}*/ }
            return rv;
        }

        // Casts 8 bytes of theRawBuffer (starting at offset) as a double (in LittleEndian form), and returns it //
        public static double extractDouble(int offset)
        {
            byte[] bytes = new byte[8];  // One doubles worth of data
            Array.Copy(theRawBuffer, offset, bytes, 0, 8);
            //bytes = reversebuffer(bytes);
            MemoryStream stream = new MemoryStream();
            double value = BitConverter.ToDouble(bytes, 0);
            /*double ret;
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                foreach (byte b in bytes)
                {
                    writer.Write(b);
                }
                
                using (BinaryReader binReader = new BinaryReader(writer.BaseStream))

                {
                    ret = binReader.ReadDouble();
                }

            }
            */
            return (value);
        }

        // Casts 4 bytes of theRawBuffer (starting at offset) as an int (in LittleEndian form), and returns it //
        public static int extractInt(int offset)
        {
            byte[] bytes = new byte[4];  // One Ints worth of data
            Array.Copy(theRawBuffer, offset, bytes, 0, 4);
            //bytes = reversebuffer(bytes);
            MemoryStream stream = new MemoryStream();
            int ret;
            int value = BitConverter.ToInt32(bytes, 0);
            /*
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(bytes);
                using (BinaryReader binReader = new BinaryReader(writer.BaseStream))

                {
                    ret = binReader.ReadInt32();
                }

            }
            */
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
            print_payload();
        }

        // Place specified value into "offset" location in theRawBuffer. (as 8-bytes in LittleEndian form) //
        public static void packDouble(int offset, double val)
        {
            
            
            byte[] bytes = new byte[8];  // One doubles worth of data
            
            MemoryStream stream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(bytes);
                writer.Write(val);
            }
            bytes = stream.ToArray();
            bytes = reversebuffer(bytes);  // swap to little Endian (JAVA is BE)
            Array.Copy(bytes, 0, theRawBuffer, offset, 8);
        }

        // Place specified value into "offset" location in theRawBuffer. (as 8-bytes in LittleEndian form) //
        public static void packInt(int offset, int val)
        {
            byte[] bytes = new byte[4];  // One doubles worth of data
            
            MemoryStream stream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(bytes);
                writer.Write(val);
            }
            bytes = stream.ToArray();
            bytes = reversebuffer(bytes);  // swap to little Endian (JAVA is BE)
            Array.Copy(bytes, 0, theRawBuffer, offset, 4);
        }

        // Sets the theRawBufffer to have values associated with all the current values of the parameters
        public byte[] packBuffer()
        {
            int offset = 0;
            for (int i = 0; i < 3; i++, offset += 8) { packDouble(offset, pos1[i]); }
            for (int i = 0; i < 3; i++, offset += 8) { packDouble(offset, pos2[i]); }
            for (int i = 0; i < 3; i++, offset += 8) { packDouble(offset, pos3[i]); }
            for (int i = 0; i < 4; i++, offset += 8) { packDouble(offset, quat1[i]); }
            for (int i = 0; i < 4; i++, offset += 8) { packDouble(offset, quat2[i]); }
            for (int i = 0; i < 4; i++, offset += 8) { packDouble(offset, quat3[i]); }
            packInt(offset, engine_flag); offset += 4;
            packInt(offset, reserved); offset += 4;
            return theRawBuffer;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           SReceive();
        }
    }
    
}
