using System;
using System.Device.I2c;
using System.Threading;

namespace i2ctest
{
    class Program
    {
        static void Main(string[] args)
        {
            var counter = 0;
            
            // declare arrays to convert readings
            byte[] co2Bytes = new byte[4];
            byte[] tempBytes = new byte[4];
            byte[] rhBytes = new byte[4];

            // define commands
            byte[] startCmd = new byte[] {0x00, 0x10, 0x00, 0x00}; 
            byte[] statusCmd = new byte[] {0x02, 0x02};
            byte[] readCmd = new byte[] {0x03, 0x00};

            Console.WriteLine("I2C Test for SCD30 Sensor");

            const int busId = 1;
            const int devAddr = 0x61;

            // set conn. params
            var con = new I2cConnectionSettings(busId, devAddr);

            // create I2C device
            var dev = I2cDevice.Create(con);

            // send start continuous reading command
            dev.Write(startCmd.AsSpan());

            // declare buffers
            var arr = new byte[18];
            var statusBuffer = new Span<byte>(arr, 0, 2);
            var dataBuffer = new Span<byte>(arr, 0, 18);

            while(true)
            {
                // check if reading ready
                do
                {
                    dev.Write(statusCmd.AsSpan());
                    Thread.Sleep(100);
                    dev.Read(statusBuffer);

                    if (statusBuffer[1] != 1) Thread.Sleep(100);

                } while (statusBuffer[1] != 1);

                // request readings
                dev.Write(readCmd.AsSpan());

                Thread.Sleep(100);

                // read data
                dev.Read(dataBuffer);

                // convert to floats
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

                // report to console
                Console.WriteLine($"Readings = {++counter}");
                Console.WriteLine($"CO2 = {BitConverter.ToSingle(co2Bytes,0)}");
                Console.WriteLine($"Temp = {BitConverter.ToSingle(tempBytes,0)}");
                Console.WriteLine($"R.H. = {BitConverter.ToSingle(rhBytes,0)}");
                Console.WriteLine();

                Thread.Sleep(2000);
            }
        }
    }
}
