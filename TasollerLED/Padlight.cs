using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Linq;

namespace TasollerLED
{
    internal class Padlight
    {
        public static UsbDevice MyUsbDevice;
        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(7375, 9011);

        //入力データ
        private static byte[] SliderTouchData = new byte[32]; //初めの３つの邪魔なデータを取り除いて扱いやすいようにしたタッチデータ
        private static byte[] SliderAirData = new byte[0];

        //timer
        Timer timer = new Timer();

        //なぜか普通にインスタンス化してもだめなので
        public static SliderPadBase[] sliderPadBase = SliderPadBase.GetInstancedSliderPadBase();
        public static SliderAirBase[] sliderAirBase = SliderAirBase.GetInstancedAirDataBase();

        public void TimerStart()//1ms
        {
            timer.UpdateTimer(1, 1);
        }

        public void TimerStop()
        {
            timer.StopTimer();
        }

        public static void AirTick()
        {
            //            Sensor Data
            //              [6] 32
            //              [5] 16
            //              [4] 8
            //              [3] 4
            //              [2] 2
            //              [1] 1
            int airdata = 0;
            //byte[] から int に出来なかったので
            for (int i = 0; i < SliderAirData.Length; i++)
            {
                airdata = SliderAirData[i];
            }

            var countdata = new int[6];

            if (airdata != 0)
            {

                if (airdata % 32 == 0)
                {
                    airdata -= 32;
                    countdata[5]++;
                }

                if (airdata % 16 == 0)
                {
                    airdata -= 16;
                    countdata[4]++;
                }

                if (airdata % 8 == 0)
                {
                    airdata -= 8;
                    countdata[3]++;
                }

                if (airdata % 4 == 0)
                {
                    airdata -= 4;
                    countdata[2]++;
                }

                if (airdata % 2 == 0)
                {
                    airdata -= 2;
                    countdata[1]++;
                }

                if (airdata % 1 == 0)
                {
                    airdata -= 1;
                    countdata[0]++;
                }
            }

            for (int i = 0; i < countdata.Length; i++)
            {
                Console.Write(countdata[i] + ":");
                if (countdata[i] != 0)
                {
                    if (sliderAirBase[i].KEY_DATA.isdown == false)
                    {
                        KB_EVENT.KeyDown(sliderAirBase[i].KEY_DATA);
                        sliderAirBase[i].KEY_DATA.isdown = true;
                    }
                }
                else
                {
                    if (sliderAirBase[i].KEY_DATA.isdown)
                    {
                        KB_EVENT.KeyUp(sliderAirBase[i].KEY_DATA);
                        sliderAirBase[i].KEY_DATA.isdown = false;
                    }
                }
            }
        }

        public static void SliderTick()
        {
            for (int i = 0; i < SliderTouchData.Length; i++)
            {
                //色リセット
                sliderPadBase[i].R = 0;
                sliderPadBase[i].G = 20;
                sliderPadBase[i].B = 0;

                //センサーの上か下かを検知
                if (i % 2 == 0)
                {
                    //下
                    if (SliderTouchData[i] > 10)
                    {
                        sliderPadBase[i].R = 100; // 光る
                        //キーを離している状態のキーにしか実行しないようにする
                        if (!sliderPadBase[i].KEY_DATA.isdown)
                        {
                            KB_EVENT.KeyDown(sliderPadBase[i].KEY_DATA);
                            sliderPadBase[i].KEY_DATA.isdown = true;
                        }
                    }
                    else
                    {
                        //キーを押している状態のキーにしか実行しないようにする
                        if (sliderPadBase[i].KEY_DATA.isdown)
                        {
                            KB_EVENT.KeyUp(sliderPadBase[i].KEY_DATA);
                            sliderPadBase[i].KEY_DATA.isdown = false;
                        }
                    }
                }
                else
                {
                    //上
                    if (SliderTouchData[i] > 10)
                    {
                        sliderPadBase[i - 1].G = 100; // 光る
                        //キーを離している状態のキーにしか実行しないようにする
                        if (!sliderPadBase[i].KEY_DATA.isdown)
                        {
                            KB_EVENT.KeyDown(sliderPadBase[i].KEY_DATA);
                            sliderPadBase[i].KEY_DATA.isdown = true;
                        }
                    }
                    else
                    {
                        //キーを押している状態のキーにしか実行しないようにする
                        if (sliderPadBase[i].KEY_DATA.isdown)
                        {
                            KB_EVENT.KeyUp(sliderPadBase[i].KEY_DATA);
                            sliderPadBase[i].KEY_DATA.isdown = false;
                        }
                    }
                }
            }
        }

        public static void WriteColorTick()
        {
            UsbEndpointWriter writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep03);
            byte[] data = new byte[240];
            data[0] = (byte)'B';
            data[1] = (byte)'L';
            data[2] = (byte)'\x00';

            //色の設定
            //for (int i = 0; i < sliderPadBase.Length; i++)
            //{
            //    sliderPadBase[i].R = 0;
            //    sliderPadBase[i].G = 20;
            //    sliderPadBase[i].B = 0;
            //}

            //後から変更も可
            //sliderPadBase[20].B = 250;

            //送るデータをsliderPadBaseから代入
            foreach (var pad in sliderPadBase)
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
                SliderAirData = readBuffer.Skip(3).Take(1).ToArray();
                SliderTouchData = readBuffer.Skip(4).Reverse().ToArray();
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

    public class SliderAirBase
    {
        public static SliderAirBase[] GetInstancedAirDataBase(int len = 6)
        {
            SliderAirBase[] ret = new SliderAirBase[len];
            KB_EVENT kB_EVENT = new KB_EVENT();
            for (int i = 0; i < len; i++)
            {
                ret[i] = new SliderAirBase
                {
                    KEY_DATA = kB_EVENT.Get_KeyDataInstance(KeyCode.TasollerAirkeymap[i])
                };
            }

            return ret;
        }

        public KEY_DATA KEY_DATA;
    }

    public class SliderPadBase
    {
        public static SliderPadBase[] GetInstancedSliderPadBase(int len = 32)
        {
            SliderPadBase[] ret = new SliderPadBase[len];
            KB_EVENT kB_EVENT = new KB_EVENT();
            for (int i = 0; i < len; i++)
            {
                ret[i] = new SliderPadBase
                {
                    padnumber = i + 1,
                    KEY_DATA = kB_EVENT.Get_KeyDataInstance(KeyCode.Tasoller32KeyMapReversed[i])
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
