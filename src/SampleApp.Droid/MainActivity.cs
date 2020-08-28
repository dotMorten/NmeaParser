using Android.App;
using Android.Widget;
using Android.OS;
using Android;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Content.PM;
using System;
using System.Collections.Generic;
using System.Linq;
using NmeaParser.Messages;

namespace SampleApp.Droid
{
    [Activity(Label = "NMEA Parser SampleApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private Button startButton;
        private Button stopButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            startButton = FindViewById<Button>(Resource.Id.startButton);
            startButton.Click += StartButton_Click;

            stopButton = FindViewById<Button>(Resource.Id.stopButton);
            stopButton.Click += StopButton_Click;
            stopButton.Enabled = false;

            devices.Add("System GPS", null);
            var devicePicker = FindViewById<Spinner>(Resource.Id.device_picker);
            foreach(var d in NmeaParser.BluetoothDevice.GetBluetoothSerialDevices())
            {
                devices[d.Name + " " + d.Address] = d.Address;
            }
            devicePicker.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, devices.Keys.ToArray());
            devicePicker.SetSelection(0);
        }

        private Dictionary<string, string> devices = new Dictionary<string, string>();

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (listener?.IsOpen == true)
            {
                Stop();
            }
        }

        private void Stop()
        {
            listener.MessageReceived -= Listener_MessageReceived;
            socket?.Close();
            socket?.Dispose();
            socket = null;
            _ = listener.CloseAsync();
            listener = null;
            startButton.Enabled = !(stopButton.Enabled = false);            
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (listener?.IsOpen != true)
            {
                Start();
            }
        }

        private NmeaParser.NmeaDevice listener;
        private TextView status;
        private bool launched;
        private Android.Bluetooth.BluetoothSocket socket;

        private async void Start()
        {
            startButton.Enabled = false;
            status = FindViewById<TextView>(Resource.Id.output);
            var devicePicker = FindViewById<Spinner>(Resource.Id.device_picker);
            var id = devicePicker.SelectedItem.ToString();
            var btAddress = devices[id];
            if (btAddress == null)
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.AccessFineLocation }, 1000);
                    return;
                }
                if (launched)
                    return;

                launched = true;
                listener = new NmeaParser.SystemNmeaDevice(ApplicationContext);
            }
            else //Bluetooth
            {
                try
                {
                    status.Text = "Opening bluetooth...";
                    var adapter = Android.Bluetooth.BluetoothAdapter.DefaultAdapter;
                    var bt = Android.Bluetooth.BluetoothAdapter.DefaultAdapter.GetRemoteDevice(btAddress);
                    Java.Util.UUID SERIAL_UUID = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FB"); //UUID for Serial Device Service
                    socket = bt.CreateRfcommSocketToServiceRecord(SERIAL_UUID);
                    try
                    {
                        await socket.ConnectAsync();
                    }
                    catch(Java.IO.IOException)
                    {
                        // This sometimes fails. Use fallback approach to open socket
                        // Based on https://stackoverflow.com/a/41627149
                        socket.Dispose();
                        var m = bt.Class.GetMethod("createRfcommSocket", new Java.Lang.Class[] { Java.Lang.Integer.Type });
                        socket = m.Invoke(bt, new Java.Lang.Object[] { 1 }) as Android.Bluetooth.BluetoothSocket;
                        socket.Connect();
                    }
                    listener = new NmeaParser.StreamDevice(socket.InputStream);
                }
                catch(System.Exception ex)
                {
                    socket?.Dispose();
                    socket = null;
                    status.Text += "\nError opening Bluetooth device:\n" + ex.Message;
                }
            }

            if (listener != null)
            {
                listener.MessageReceived += Listener_MessageReceived;
                status.Text += "\nOpening device...";
                await listener.OpenAsync();
                status.Text += "\nConnected!";
                startButton.Enabled = !(stopButton.Enabled = true);
            }
            else
            {
                startButton.Enabled = !(stopButton.Enabled = false);
            }
        }

        protected override void OnDestroy()
        {
            Stop();
            base.OnDestroy();
        }

        protected override void OnResume()
        {
            base.OnResume();
            // if it was resumed by the GPS permissions dialog
            //Start();
        }

        Queue<NmeaParser.Messages.NmeaMessage> messages = new Queue<NmeaParser.Messages.NmeaMessage>(100);
        private void Listener_MessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs e)
        {
            var message = e.Message;
            RunOnUiThread(() =>
            {
                if (messages.Count == 100) messages.Dequeue();
                messages.Enqueue(message);
                status.Text = string.Join("\n", messages.Reverse().Select(n=>n.ToString()));
                if(message is Rmc rmc)
                {
                    FindViewById<TextView>(Resource.Id.latitude).Text = "Latitude = " + rmc.Latitude.ToString("0.0000000");
                    FindViewById<TextView>(Resource.Id.longitude).Text = "Longitude = " + rmc.Longitude.ToString("0.0000000");
                }
                else if (message is Gga gga)
                {
                    FindViewById<TextView>(Resource.Id.altitude).Text = "Altitude = " + gga.Altitude.ToString() + " " + gga.AltitudeUnits.ToString();
                }
            });
        }
    }
}

