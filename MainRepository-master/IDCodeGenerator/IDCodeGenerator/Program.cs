using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using System.Threading.Tasks;

namespace IDCodeGenerator
{
    class Program
    {
        static RegistryManager registryManager;
        //connectionString = "{iothub connection string}";
        static string connectionString = "HostName=Tim5.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=+EfTIwEUHbEKgNF7hNnC/WvOBT3soYQaN8Ku7AafB7U=";
        static void Main(string[] args)
        {
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            AddDeviceAsync().Wait();
            Console.ReadLine();
        }

        private static async Task AddDeviceAsync()
        {
            string deviceId = "IoTWorkshopApp";
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
        }
    }
}
