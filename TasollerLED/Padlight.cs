using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Linq;
using System.Timers;
using Timer = System.Timers.Timer;

namespace TasollerLED
{
    internal class Padlight
    {
        public static UsbDevice MyUsbDevice;
        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(7375, 9011);

        //入力データ
        private static byte[] TouchData = new byte[32]; //初めの３つの邪魔なデータを取り除いて扱いやすいようにしたタッチデータ
        private static byte[] AirData = new byte[0];  //[0]しかないけどこれしかやり方知らない

        //timer
        static Timer Sendtimer = new Timer();
        static Timer Readtimer = new Timer();

        //なぜか普通にインスタンス化してもだめなので
        public static PadColor[] padColor = PadColor.GetinstansedPadcolor();

        public static void TimerStart(int readinterval = 1, int sendinterval = 16)//16ms
        {
            Sendtimer.Elapsed += new ElapsedEventHandler(SendTick);
            Readtimer.Elapsed += new ElapsedEventHandler(ReadTick);
            Readtimer.Interval = readinterval;
            Sendtimer.Interval = sendinterval;
            Sendtimer.Start();
            Readtimer.Start();
        }
        public static void TimerStop()
        {
            Sendtimer.Stop();
            Readtimer.Stop();
        }
        static void SendTick(object sender, ElapsedEventArgs e)
        {
            UsbEndpointWriter writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep03);
            KeySend keySend = new KeySend();
            int bytesWritten;
            byte[] data = new byte[240];
            data[0] = (byte)'B';
            data[1] = (byte)'L';
            data[2] = (byte)'\x00';

            //スライダーの右から処理したいので反転させます
            //Array.Reverse(TouchData);   //こいつが有効だとバグる

            for (int i = 0; i < TouchData.Length; i++)
            {
                //色リセット
                padColor[i].R = 0;
                padColor[i].G = 0;
                padColor[i].B = 0;

                //センサーの上か下かを検知
                if (i % 2 == 0)
                {
                    //下
                    if (TouchData[i] > 10)
                    {
                        padColor[i].R = 100; //優しく光る
                        keySend.KeyDown(padColor[i].INPUT.ki.wVk);
                    }
                    else
                    {
                        keySend.KeyUp(padColor[i].INPUT);
                    }
                }
                else
                {
                    //上
                    if (TouchData[i] > 10)
                    {
                        padColor[i - 1].G = 100; //光る
                        keySend.KeyDown(padColor[i].INPUT.ki.wVk);
                    }
                    else
                    {
                        keySend.KeyUp(padColor[i].INPUT);
                    }
                }
            }


            //色の設定
            //for (int i = 0; i < padColor.Length; i++)
            //{
            //    padColor[i].R = 0;
            //    padColor[i].G = 20;
            //    padColor[i].B = 0;
            //}

            //後から変更も可
            //padColor[20].B = 250;

            //送るデータをpadColorから代入
            foreach (var pad in padColor)
            {
                data[pad.Gbyte] = pad.G;
                data[pad.Rbyte] = pad.R;
                data[pad.Bbyte] = pad.B;
            }

            ErrorCode ec = writer.Write(data, 2000, out bytesWritten);
            if (ec != ErrorCode.None) throw new Exception(UsbDevice.LastErrorString);
        }

        static void ReadTick(object sender, ElapsedEventArgs e)
        {
            UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep04);
            byte[] readBuffer = new byte[36];
            int bytesRead;

            // If the device hasn't sent data in the last 5 seconds,
            // a timeout error (ec = IoTimedOut) will occur. 
            reader.Read(readBuffer, 1000, out bytesRead);

            if (bytesRead != 0)
            {
                string bufferitem = "";
                //try
                //{
                //    foreach (var item in readBuffer)
                //    {
                //        bufferitem += item.ToString() + ",";
                //    }
                //}
                //finally
                //{
                //    bufferitem += Environment.NewLine;
                //};

                AirData = readBuffer.Skip(3).Take(1).ToArray();
                TouchData = readBuffer.Skip(4).Reverse().ToArray();
            }
        }
        public static Tuple<ErrorCode, string> DeviceSetUp()
        {
            ErrorCode ec = ErrorCode.None;
            try
            {
                MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

                if (MyUsbDevice == null) throw new Exception("Device Not Found.");
                IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    wholeUsbDevice.SetConfiguration(1);
                    wholeUsbDevice.ClaimInterface(0);
                }
            }
            catch (Exception ex)
            {
                return Tuple.Create(ec, ex.ToString());
            }
            return Tuple.Create(ec, "接続");
        }
    }

    public class PadColor
    {
        static public PadColor[] GetinstansedPadcolor(int len = 32)
        {
            PadColor[] ret = new PadColor[len];
            KeySend keySend = new KeySend();
            for (int i = 0; i < len; i++)
            {
                ret[i] = new PadColor
                {
                    padnumber = i + 1,
                    INPUT = keySend.GetInstanseINPUT(KeyCode.Tasoller32KeyMap[i])
                };
            }

            return ret;
        }

        public KeySend.INPUT INPUT;

        public int padnumber = 0;

        public byte G = 0;
        public byte R = 0;
        public byte B = 0;

        public int Gbyte
        {
            get => padnumber * 3;
        }

        public int Rbyte
        {
            get => padnumber * 3 + 1;
        }

        public int Bbyte
        {
            get => padnumber * 3 + 2;
        }
    }
}
