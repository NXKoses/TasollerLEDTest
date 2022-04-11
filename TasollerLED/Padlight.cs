using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Linq;
using static TasollerLED.Timer;

namespace TasollerLED
{
    internal class Padlight
    {
        public static UsbDevice MyUsbDevice;
        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(7375, 9011);
        public static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        //入力データ
        private static byte[] TouchData = new byte[32]; //初めの３つの邪魔なデータを取り除いて扱いやすいようにしたタッチデータ
        private static byte[] AirData = new byte[0];  //[0]しかないけどこれしかやり方知らない

        //timer
        //static Timer Sendtimer = new Timer();
        //static Timer Readtimer = new Timer();

        Timer timer = new Timer();

        //なぜか普通にインスタンス化してもだめなので
        public static PadData[] padColor = PadData.GetinstancedPadcolor();

        public void TimerStart()//16ms
        {
            timer.UpdateTimer(1, 1);
        }
        public void TimerStop()
        {
            timer.StopTimer();
        }
        public static void SendTick()
        {
            UsbEndpointWriter writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep03);
            byte[] data = new byte[240];
            data[0] = (byte)'B';
            data[1] = (byte)'L';
            data[2] = (byte)'\x00';

            for (int i = 0; i < TouchData.Length; i++)
            {
                //色リセット
                padColor[i].R = 0;
                padColor[i].G = 20;
                padColor[i].B = 0;

                //センサーの上か下かを検知
                if (i % 2 == 0)
                {
                    //下
                    if (TouchData[i] > 10)
                    {
                        padColor[i].R = 100; // 光る
                        //キーを離している状態のキーにしか実行しないようにする
                        if (!padColor[i].KEY_DATA.isdown)
                        {
                            KB_EVENT.KeyDown(padColor[i].KEY_DATA.key);
                            padColor[i].KEY_DATA.isdown = true;
                        }
                    }
                    else
                    {
                        //キーを押している状態のキーにしか実行しないようにする
                        if (padColor[i].KEY_DATA.isdown)
                        {
                            KB_EVENT.KeyUp(padColor[i].KEY_DATA);
                            padColor[i].KEY_DATA.isdown = false;
                        }
                    }
                }
                else
                {
                    //上
                    if (TouchData[i] > 10)
                    {
                        padColor[i - 1].G = 100; // 光る
                        //キーを離している状態のキーにしか実行しないようにする
                        if (!padColor[i].KEY_DATA.isdown)
                        {
                            KB_EVENT.KeyDown(padColor[i].KEY_DATA.key);
                            padColor[i].KEY_DATA.isdown = true;
                        }
                    }
                    else
                    {
                        //キーを押している状態のキーにしか実行しないようにする
                        if (padColor[i].KEY_DATA.isdown)
                        {
                            KB_EVENT.KeyUp(padColor[i].KEY_DATA);
                            padColor[i].KEY_DATA.isdown = false;
                        }
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

            ErrorCode ec = writer.Write(data, 2000, out int bytesWritten);
            if (ec != ErrorCode.None) throw new Exception(UsbDevice.LastErrorString);
        }

        public static void ReadTick()
        {
            UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep04);
            byte[] readBuffer = new byte[36];

            reader.Read(readBuffer, 1000, out int bytesRead);

            if (bytesRead != 0)
            {
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
                if (MyUsbDevice is IUsbDevice wholeUsbDevice)
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

    public class PadData
    {
        public static PadData[] GetinstancedPadcolor(int len = 32)
        {
            PadData[] ret = new PadData[len];
            KB_EVENT kB_EVENT = new KB_EVENT();
            for (int i = 0; i < len; i++)
            {
                ret[i] = new PadData
                {
                    padnumber = i + 1,
                    KEY_DATA = kB_EVENT.Get_instance(KeyCode.Tasoller32KeyMap[i])
                };
            }

            return ret;
        }

        public KEY_DATA KEY_DATA;

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
