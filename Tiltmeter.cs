using EasyModbus;
using System;
using System.Collections.Generic;

namespace ModbusRTU_Viewer
{
    public class Tiltmeter
    {
        private Int16 SinXH; // Higher part of sin X 
        private UInt16 SinXL; // Lower part of sin X 
        private Int16 SinYH; // Higher part of sin Y 
        private UInt16 SinYL; // Lower part of sin Y 
        private Int16 CalTH; // Higher part of temperature 
        private UInt16 CalTL; // Lower part of temperature

        private Int16 RawXH; // Higher part of Raw X 
        private UInt16 RawXL; // Lower part of Raw X 
        private Int16 RawYH; // Higher part of Raw Y 
        private UInt16 RawYL; // Lower part of Raw Y 
        private Int16 RawTH; // Higher part of temperature 
        private UInt16 RawTL; // Lower part of temperature

        public enum Options
        {
            RawTemperature,
            RawX,
            RawY,
            SinX,
            SinY,
            Temperature,
            Null
        }

        private float Temperature
        {
            get
            {
                int v = CalTH;
                v <<= 16;
                v |= CalTL;
                return v / 65536.0f;
            }
        }

        private float SinX
        {
            get
            {
                int v = SinXH;
                v <<=16;
                v |= SinXL;
                return v / 65536.0f;
            }
        }

        private float SinY
        {
            get {
                int v = SinYH;
                v <<=16;
                v |=SinYL;
                return v / 65536.0f;
            }
        }

        public Tiltmeter()
        {

        }

        public List<String> get(Options wanted, ModbusClient client)
        {
            getValues(client);
            List<String> list = new List<String>();

            switch (wanted)
            {
                case Options.RawTemperature:
                    list.Add(RawTH.ToString("X4") + " " + RawTL.ToString("X4"));
                    list.Add(RawTH.ToString() + " " + RawTL.ToString());
                    break;
                case Options.RawX:
                    list.Add(RawXH.ToString("X4") + " " + RawXL.ToString("X4"));
                    list.Add(RawXH.ToString() + " " + RawXL.ToString());
                    break;
                case Options.RawY:
                    list.Add(RawYH.ToString("X4") + " " + RawYL.ToString("X4"));
                    list.Add(RawYH.ToString() + " " + RawYL.ToString());
                    break;
                case Options.SinX:
                    list.Add(SinXH.ToString("X4") + " " + SinXL.ToString("X4"));
                    list.Add(SinX.ToString("0.00"));
                    break;
                case Options.SinY:
                    list.Add(SinYH.ToString("X4") + " " + SinYL.ToString("X4"));
                    list.Add(SinY.ToString("0.00"));
                    break;
                case Options.Temperature:
                    list.Add(CalTH.ToString("X4")+" "+ CalTL.ToString("X4"));
                    list.Add(Temperature.ToString("0.00"));
                        break;
                default:
                    break;
            }
            return list;
        }

        private bool getValues(ModbusClient client)
        {
            try
            {
                var _addr = UInt32.Parse("120", System.Globalization.NumberStyles.HexNumber);
                var __addr = int.Parse(_addr + "");
                var I = client.ReadInputRegisters(__addr, 6);
                if (I.Length != 6) return false;
                SinXH = (Int16)I[0];
                SinXL = (UInt16)I[1];
                SinYH =(Int16)I[2];
                SinYL = (UInt16)I[3];
                CalTH = (Int16)I[4];
                CalTL = (UInt16)I[5];
            }
            catch (Exception)
            {
                return false;
            }
            try
            {
                var _addr = UInt32.Parse("110", System.Globalization.NumberStyles.HexNumber);
                var __addr = int.Parse(_addr + "");
                var I = client.ReadInputRegisters(__addr, 6);
                if (I.Length != 6) return false;
                RawXH = (Int16)I[0];
                RawXL = (UInt16)I[1];
                RawYH = (Int16)I[2];
                RawYL = (UInt16)I[3];
                RawTH = (Int16)I[4];
                RawTL = (UInt16)I[5];
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
