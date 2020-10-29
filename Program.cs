using System;
using System.Device.I2c;
using System.Threading;

namespace i2ctest
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] co2Bytes = new byte[4];
            byte[] tempBytes = new byte[4];
            byte[] rhBytes = new byte[4];

            byte[] startCmd = new byte[] {0x00, 0x10, 0x00, 0x00}; 
            byte[] statusCmd = new byte[] {0x02, 0x02};
            byte[] readCmd = new byte[] {0x03, 0x00};

            Console.WriteLine("I2C Test");

            const int busId = 1;
            const int devAddr = 0x61;

            var con = new I2cConnectionSettings(busId, devAddr);

            var dev = I2cDevice.Create(con);

            dev.Write(startCmd.AsSpan());

            //var dataOut = new byte[] {0x02, 0x02}.AsSpan();
            //dev.Write(dataOut);

            var arr = new byte[18];
            var statusBuffer = new Span<byte>(arr, 0, 2);
            var dataBuffer = new Span<byte>(arr, 0, 18);

            while(true)
            {
                do
                {
                    dev.Write(statusCmd.AsSpan());
                    Thread.Sleep(100);
                    dev.Read(statusBuffer);

                    if (statusBuffer[1] != 1) Thread.Sleep(100);

                } while (statusBuffer[1] != 1);

                dev.Write(readCmd.AsSpan());

                Thread.Sleep(100);

                dev.Read(dataBuffer);

                co2Bytes[0] = dataBuffer[4];
                co2Bytes[1] = dataBuffer[3];
                co2Bytes[2] = dataBuffer[1];
                co2Bytes[3] = dataBuffer[0];

                tempBytes[0] = dataBuffer[10];
                tempBytes[1] = dataBuffer[9];
                tempBytes[2] = dataBuffer[7];
                tempBytes[3] = dataBuffer[6];

                rhBytes[0] = dataBuffer[16];
                rhBytes[1] = dataBuffer[15];
                rhBytes[2] = dataBuffer[13];
                rhBytes[3] = dataBuffer[12];

                Console.WriteLine($"CO2 = {BitConverter.ToSingle(co2Bytes,0)}");
                Console.WriteLine($"Temp = {BitConverter.ToSingle(tempBytes,0)}");
                Console.WriteLine($"R.H. = {BitConverter.ToSingle(rhBytes,0)}");
                Console.WriteLine();


                Thread.Sleep(1000);
            }


        }
    }
}
