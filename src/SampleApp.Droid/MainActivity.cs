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
using NmeaParser.Nmea.Gps;
using NmeaParser.Nmea;

namespace SampleApp.Droid
{
    [Activity(Label = "SampleApp.Droid", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Start();
        }

        private NmeaParser.SystemNmeaDevice listener;
        private TextView status;
        private bool launched;

        private async void Start()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.AccessFineLocation }, 1000);
                return;
            }
            if (launched)
                return;

            launched = true;

            listener = new NmeaParser.SystemNmeaDevice();
            listener.MessageReceived += Listener_MessageReceived;
            status = FindViewById<TextView>(Resource.Id.output);
            status.Text = "Opening device...";
            await listener.OpenAsync();
        }

        protected override void OnResume()
        {
            base.OnResume();
            // if it was resumed by the GPS permissions dialog
            Start();
        }

        Queue<NmeaParser.Nmea.NmeaMessage> messages = new Queue<NmeaParser.Nmea.NmeaMessage>(100);
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
                    FindViewById<TextView>(Resource.Id.latitude).Text = "Latitude = " + rmc.Latitude.ToString();
                    FindViewById<TextView>(Resource.Id.longitude).Text = "Longitude = " + rmc.Longitude.ToString();
                }
                else if (message is Gga gga)
                {
                    FindViewById<TextView>(Resource.Id.altitude).Text = "Altitude = " + gga.Altitude.ToString() + " " + gga.AltitudeUnits.ToString();
                }
            });
        }
    }
}

