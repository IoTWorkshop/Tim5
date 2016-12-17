using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using System.Threading;
using System.Diagnostics;

namespace Two_Way_Communication_RPi
{
    
    public sealed partial class MainPage : Page
    {
        private I2cDevice Device;
        private Timer periodicTimer;
        private const int LED_PIN = 5;
        private GpioPin pin;
        private GpioPinValue pinValue;
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
        static DeviceClient deviceClient;
        static string iotHubUri = "Tim5.azure-devices.net";
        static string deviceKey = "cOn0AaSWxLBKP5xUCfzIfbH//ylTXyJJgwYrl89iWV0=";
        static ServiceClient serviceClient;
        static string connectionString = "HostName=Tim5.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=+EfTIwEUHbEKgNF7hNnC/WvOBT3soYQaN8Ku7AafB7U=";
        private static int i = 2;
        public MainPage()
        {
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("IoTWorkshopRPi", deviceKey));
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            InitializeComponent();
            initcommunitcation();
            InitGPIO();
            ReceiveC2dAsync();
        }

        private async void initcommunitcation()

        {
            /* // Get a selector string for bus "I2C1"
             string aqs = I2cDevice.GetDeviceSelector("I2C1");

             // Find the I2C bus controller with our selector string
             var dis = await DeviceInformation.FindAllAsync(aqs);
             if (dis.Count == 0)
                 return; // bus not found

             // 0x40 is the I2C device address
             var settings = new I2cConnectionSettings(0x40);

             // Create an I2cDevice with our selected bus controller and I2C settings
             using (I2cDevice device = await I2cDevice.FromIdAsync(dis[0].Id, settings))
             {
                 byte[] writeBuf = { 0x01, 0x02, 0x03, 0x04 };
                 device.Write(writeBuf);
             }
         */

            var settings = new I2cConnectionSettings(0x40); // Arduino address

            settings.BusSpeed = I2cBusSpeed.StandardMode;

            string aqs = I2cDevice.GetDeviceSelector("I2C1");

            var dis = await DeviceInformation.FindAllAsync(aqs);
            Device = await I2cDevice.FromIdAsync(dis[0].Id, settings);

            Debug.WriteLine(dis[0].Id + " " + settings + " " + aqs);
            periodicTimer = new Timer(this.TimerCallback, null, 0, 200); // Create a timmer
            if (Device == null)
            {
                Debug.WriteLine(
                    "Slave address {0} on I2C Controller {1} is currently in use by " +
                    "another application. Please ensure that no other applications are using I2C.",
                    settings.SlaveAddress,
                    dis[0].Id);
                return;
            }

        }

        private void TimerCallback(object state)
        {
            byte[] RegAddrBuf = new byte[] { 0x40 };
            byte[] ReadBuf = new byte[64];
            try {
                Device.Read(ReadBuf); // read the data
            }

            catch (Exception f) {
                Debug.WriteLine(f.Message);
            }

            char[] cArray = System.Text.Encoding.UTF8.GetString(ReadBuf, 0, 64).ToCharArray();  // Converte  Byte to Char
            String c = new String(cArray);
            textBlock.Text = c;
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                pin = null;
                return;
            }

            pin = gpio.OpenPin(LED_PIN);
            pinValue = GpioPinValue.Low;
            pin.Write(pinValue);
            pin.SetDriveMode(GpioPinDriveMode.Output);

        }
        private async void ReceiveC2dAsync()
        {
            while (true)
            {

                Microsoft.Azure.Devices.Client.Message receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;
                string receivedData = Encoding.ASCII.GetString(receivedMessage.GetBytes());

                if (receivedData == "1")
                {
                    pinValue = GpioPinValue.High;
                    pin.Write(pinValue);
                    LED.Fill = redBrush;
                }
                else
                {
                    pinValue = GpioPinValue.Low;
                    pin.Write(pinValue);
                    LED.Fill = grayBrush;
                }
                await deviceClient.CompleteAsync(receivedMessage);
            }

        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            if (i % 2 == 0)
                await SendCloudToDeviceMessageAsync("1");
            else
                await SendCloudToDeviceMessageAsync("0");
            i++;
        }

        private async static Task SendCloudToDeviceMessageAsync(string podaci)
        {
            var commandMessage = new Microsoft.Azure.Devices.Message(Encoding.ASCII.GetBytes(podaci));
            await serviceClient.SendAsync("IoTWorkshopApp", commandMessage);
        }
    }
}
