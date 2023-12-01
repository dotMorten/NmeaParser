using Android;
using Android.Bluetooth;
using Android.Content.PM;
using AndroidX.Core.App;

namespace SampleApp.Droid;

[Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/icon")]
public class MainActivity : Activity
{
    private Button startButton;
    private Button stopButton;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Set our view from the "main" layout resource
        SetContentView(Resource.Layout.activity_main);
        startButton = FindViewById<Button>(Resource.Id.startButton)!;
        startButton.Click += StartButton_Click;

        stopButton = FindViewById<Button>(Resource.Id.stopButton)!;
        stopButton.Click += StopButton_Click;
        stopButton.Enabled = false;

        devices.Add("System GPS", null);
        var devicePicker = FindViewById<Spinner>(Resource.Id.device_picker);

        if (this.ApplicationContext!.CheckSelfPermission(Manifest.Permission.BluetoothConnect) != Permission.Granted)
        {
            ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.BluetoothConnect }, 1000);
            return;
        }

        foreach (var d in NmeaParser.BluetoothDevice.GetBluetoothSerialDevices(ApplicationContext))
        {
            devices[d.Name + " " + d.Address] = d;
        }
        devicePicker.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, devices.Keys.ToArray());
        devicePicker.SetSelection(0);
    }

    private Dictionary<string, BluetoothDevice?> devices = new Dictionary<string, BluetoothDevice?>();

    private void StopButton_Click(object? sender, EventArgs e)
    {
        if (listener?.IsOpen == true)
        {
            Stop();
        }
    }

    private void Stop()
    {
        listener!.MessageReceived -= Listener_MessageReceived;
        monitor!.LocationChanged -= Monitor_LocationChanged;
        socket?.Close();
        socket?.Dispose();
        socket = null;
        _ = listener.CloseAsync();
        listener = null;
        startButton.Enabled = !(stopButton.Enabled = false);
    }

    private void StartButton_Click(object? sender, EventArgs e)
    {
        if (listener?.IsOpen != true)
        {
            Start();
        }
    }

    private NmeaParser.NmeaDevice? listener;
    private NmeaParser.Gnss.GnssMonitor? monitor;
    private TextView? status;
    private bool launched;
    private Android.Bluetooth.BluetoothSocket? socket;

    private async void Start()
    {
        startButton.Enabled = false;
        status = FindViewById<TextView>(Resource.Id.output);
        var devicePicker = FindViewById<Spinner>(Resource.Id.device_picker)!;
        var id = devicePicker.SelectedItem?.ToString();
        var btAddress = devices[id];
        if (btAddress == null)
        {
            if (this.ApplicationContext!.CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Permission.Granted)
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
                listener = new NmeaParser.BluetoothDevice(btAddress, ApplicationContext!);
            }
            catch (System.Exception ex)
            {
                socket?.Dispose();
                socket = null;
                status.Text += "\nError opening Bluetooth device:\n" + ex.Message;
            }
        }

        if (listener != null)
        {
            listener.MessageReceived += Listener_MessageReceived;
            status!.Text += "\nOpening device...";
            try
            {
                await listener.OpenAsync();
                status.Text += "\nConnected!";
                monitor = new NmeaParser.Gnss.GnssMonitor(listener);
                monitor.LocationChanged += Monitor_LocationChanged;
                startButton.Enabled = !(stopButton.Enabled = true);
            }
            catch (Exception ex)
            {
                status.Text += "\nError opening device:\n" + ex.Message;
                startButton.Enabled = true;
                stopButton.Enabled = false;
            }
            finally
            {
            }
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
    private void Listener_MessageReceived(object? sender, NmeaParser.NmeaMessageReceivedEventArgs e)
    {
        var message = e.Message;
        RunOnUiThread(() =>
        {
            if (messages.Count == 100) messages.Dequeue();
            messages.Enqueue(message);
            status!.Text = string.Join("\n", messages.Reverse().Select(n => n.ToString()));
        });
    }

    private void Monitor_LocationChanged(object? sender, EventArgs e)
    {
        FindViewById<TextView>(Resource.Id.latitude)!.Text = "Latitude = " + monitor!.Latitude.ToString("0.0000000");
        FindViewById<TextView>(Resource.Id.longitude)!.Text = "Longitude = " + monitor!.Longitude.ToString("0.0000000");
        FindViewById<TextView>(Resource.Id.altitude)!.Text = "Altitude = " + monitor!.Altitude.ToString();
    }
}