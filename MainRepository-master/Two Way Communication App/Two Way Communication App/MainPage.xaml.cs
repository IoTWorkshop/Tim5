﻿using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;


namespace Two_Way_Communication_App
{
    
    public sealed partial class MainPage : Page
    {
        
        static string connectionString = "HostName=Tim5.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=+EfTIwEUHbEKgNF7hNnC/WvOBT3soYQaN8Ku7AafB7U=";
        static string iotHubUri = "Tim5.azure-devices.net";
        static string deviceKey = "AgkLpQ0vwjd6k8okj8tIvxEBZMQWiVnyoMGdBe3+i2k=";
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
        static DeviceClient deviceClient;
        static ServiceClient serviceClient;
        
        private static int i = 2;

        public MainPage()
        {
            InitializeComponent();
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("IoTWorkshopApp", deviceKey));
            ReceiveC2dAsync();
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
            await serviceClient.SendAsync("IoTWorkshopRPi", commandMessage);
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
                    LED.Fill = redBrush;
                }
                else
                {
                    LED.Fill = grayBrush;
                }
                await deviceClient.CompleteAsync(receivedMessage);
            }

        }
    }
}
