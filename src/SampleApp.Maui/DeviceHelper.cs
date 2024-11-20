using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NmeaParser;
#if WINDOWS
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
#endif

namespace SampleApp.Maui
{
    public class DeviceInfo
    {
        public DeviceInfo(string name, Func<Task<NmeaDevice>> createMethod) { 
            DisplayName = name;
            CreateMethod = createMethod;
        }
        public Func<Task<NmeaDevice>> CreateMethod { get; }
        public string DisplayName { get; }
        public override string ToString() => DisplayName;
    }

    internal static class DeviceHelper
    {
        public static async Task<List<DeviceInfo>> LoadDevices()
        {
            List<DeviceInfo> deviceList = new List<DeviceInfo>();
#if WINDOWS
            //Get list of devices
            var btdevices = await NmeaParser.BluetoothDevice.GetBluetoothSerialDevicesAsync();
            foreach (var item in btdevices)
            {
                deviceList.Add(new DeviceInfo($"{item.Device.Name} (Bluetooth)", () => Task.FromResult<NmeaParser.NmeaDevice>(new NmeaParser.BluetoothDevice(item))));

            }

            // Find serial devices
            foreach(var port in System.IO.Ports.SerialPort.GetPortNames().OrderBy(s => s))
            {
                var serialDevice = new System.IO.Ports.SerialPort(port, 9600);
                deviceList.Add(new DeviceInfo($"Serial Port {port} @ 9600 baud", () => Task.FromResult<NmeaParser.NmeaDevice>(new NmeaParser.SerialPortDevice(serialDevice))));
            }
#elif ANDROID
            var permission = await Permissions.RequestAsync<Permissions.Bluetooth>();
                deviceList.Add(new DeviceInfo("System NMEA device",
                    async () =>
                    {
                        var permissionLocation = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                        if (permissionLocation == PermissionStatus.Granted)
                            return new NmeaParser.SystemNmeaDevice(Maui.MainApplication.Context);
                        throw new InvalidOperationException("Access not granted");
                    }));
            if (permission == PermissionStatus.Granted)
            {
                foreach (var d in NmeaParser.BluetoothDevice.GetBluetoothSerialDevices(Maui.MainApplication.Context))
                {
                    deviceList.Add(new DeviceInfo(d.Name + " " + d.Address, () => Task.FromResult<NmeaDevice>(new NmeaParser.BluetoothDevice(d, Maui.MainApplication.Context))));
                }
            }
#elif IOS
            string[] knownDevices = { "com.bad-elf.gps", "com.dualav.xgps150", "00001101-0000-1000-8000-00805F9B34FB" }; // If you add more, also update info.plist
            foreach (var accessory in ExternalAccessory.EAAccessoryManager.SharedAccessoryManager.ConnectedAccessories)
            {
                foreach (var str in knownDevices)
                {
                    if (accessory.ProtocolStrings.Contains(str))
                        deviceList.Add(new DeviceInfo(str, () => Task.FromResult<NmeaDevice>(new EAAccessoryDevice(accessory, str))));
                }
            }
#endif
            return deviceList;
        }

    }
}