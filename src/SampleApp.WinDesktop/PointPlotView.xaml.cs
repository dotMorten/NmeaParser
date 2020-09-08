using Esri.ArcGISRuntime.Location;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SampleApp.WinDesktop
{
    /// <summary>
    /// Interaction logic for PointPlotView.xaml
    /// </summary>
    public partial class PointPlotView : UserControl
    {
        private List<Point> locations = new List<Point>();

        public PointPlotView()
        {
            InitializeComponent();
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PlotMap.Width = PlotMap.Height = Math.Min(e.NewSize.Width, e.NewSize.Height);
        }
        Size size = new Size();
        private void PlotMap_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            size = e.NewSize;
            UpdatePlot();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        public void Clear()
        {
            locations.Clear();
            if(!autoFit)
                autoFitToggle.IsChecked = true;
            else
                UpdatePlot();
        }

        private struct Point
        {
            public double Latitude { get ;set; }
            public double Longitude { get ;set; }
            public double Z { get ;set; }
            public NmeaParser.Messages.Gga.FixQuality Quality { get; set; }
        }
        
        public void AddLocation(double latitude, double longitude, double altitude, NmeaParser.Messages.Gga.FixQuality quality)
        {
            if(quality == NmeaParser.Messages.Gga.FixQuality.Invalid)
                return;
            locations.Add(new Point { 
                Latitude = latitude, 
                Longitude = longitude, 
                Z = altitude, 
                Quality = quality 
            });
            UpdatePlot();
        }

        
        private void UpdatePlot()
        {
            if (size.Width == 0 || size.Height == 0 || !IsVisible)
                return;
            if (locations.Count == 0)
            {
                Dispatcher.Invoke(() =>
                {
                    Status.Text = "";
                    plot.Source = null;
                });
                return;
            }
            var measurements = locations.ToArray(); // Grab copy to avoid threading issues
            var latAvr = measurements.Select(l => l.Latitude).Average();
            var lonAvr = measurements.Select(l => l.Longitude).Average();
            
            //List<double> lonDistances = new List<double>(locations.Count);
            //List<double> latDistances = new List<double>(locations.Count);
            List<double> distances = new List<double>(measurements.Length);
            var locations2 = new List<Point>();
            foreach (var l in measurements)
            {
                var d = Vincenty.GetDistanceVincenty(latAvr, lonAvr, l.Latitude, l.Longitude);
                var dLat = Vincenty.GetDistanceVincenty(latAvr, lonAvr, l.Latitude, lonAvr);
                var dLon = Vincenty.GetDistanceVincenty(latAvr, lonAvr, latAvr, l.Longitude);
                distances.Add(d);
                //latDistances.Add(dLat);
                //lonDistances.Add(dLon);
                if (latAvr > l.Latitude) dLat = -dLat;
                if (lonAvr > l.Longitude) dLon = -dLon;
                locations2.Add(new Point() { Latitude = dLat, Longitude= dLon, Quality = l.Quality });
            }
            var latMin = locations2.Select(l => l.Latitude).Min();
            var lonMin = locations2.Select(l => l.Longitude).Min();
            var latMax = locations2.Select(l => l.Latitude).Max();
            var lonMax = locations2.Select(l => l.Longitude).Max();
            var latAvr2 = locations2.Select(l => l.Latitude).Average();
            var lonAvr2 = locations2.Select(l => l.Longitude).Average();
            var maxDifLat = Math.Max(latAvr2 - latMin, latMax - latAvr2);
            var maxDifLon = Math.Max(lonAvr2 - lonMin, lonMax - lonAvr2);
            //var maxDif = Math.Max(maxDifLat, maxDifLon);
            double maxDif = 1;
            if (autoFit)
            {
                maxDif = distances.Max();
                if (maxDif < 0.05)
                    maxDif = 0.05;
                else if (maxDif < 1)
                    maxDif = Math.Ceiling(maxDif * 10) / 10d;
                else
                    maxDif = Math.Ceiling(maxDif);
                currentScale = maxDif / (Math.Min(size.Width, size.Height) * .5);
            }
            else
            {
                maxDif = currentScale * (Math.Min(size.Width, size.Height) * .5);
            }
            double scale = currentScale;
            if (scale == 0)
                scale = 1;
             int width = (int)size.Width;
             int height = (int)size.Height;
            int stride = width * 4;
             byte[] pixels = new byte[width * height * 4];
            double[][] stamp = new double[][] {new double[] { .3, .5, .3 }, new double[] { .5, 1, .5 }, new double[] { .3, .5, .3 } };
            Color col = Colors.Red;
            for (int i = 0; i < locations2.Count; i++) 
            {
                var l = locations2[i];
                var x = (int)(width * .5 + (l.Longitude - lonAvr2) / scale);
                var y = (int)(height * .5 - (l.Latitude - latAvr2) / scale);
                var index = ((int)y) * stride + ((int)x) * 4;
                for (int r = -1; r < stamp.Length-1; r++)
                {
                    for (int c = -1; c < stamp[r + 1].Length-1; c++)
                    {
                        if (x + c >= width || x + c < 0 ||
                           y + r >= width || y + r < 0)
                            continue;
                        var p = index + r * stride + c * 4;
                        var val = stamp[r + 1][c + 1];
                        switch(l.Quality)
                        {
                            case NmeaParser.Messages.Gga.FixQuality.Estimated:
                               col = Colors.Red; break;
                            case NmeaParser.Messages.Gga.FixQuality.GpsFix:
                               col = Colors.Orange; break;
                            case NmeaParser.Messages.Gga.FixQuality.DgpsFix:
                               col = Colors.Yellow; break;
                            case NmeaParser.Messages.Gga.FixQuality.FloatRtk:
                               col = Color.FromRgb(0,255,0); break;
                            case NmeaParser.Messages.Gga.FixQuality.Rtk:
                               col = Colors.LightBlue; break;
                               default:
                               col = Colors.Gray; break;
                        }
                        pixels[p + 1] = col.B;
                        pixels[p + 1] = col.G;
                        pixels[p + 2] = col.R;
                        pixels[p + 3] = (byte)Math.Min(255, pixels[p + 3] + val * 255); //Multiply alpha
                    }
                }
            }
            var stdDevLat = Math.Sqrt(locations2.Sum(d => (d.Latitude - latAvr2) * (d.Latitude - latAvr2)) / locations2.Count);
            var stdDevLon = Math.Sqrt(locations2.Sum(d => (d.Longitude - lonAvr2) * (d.Longitude - lonAvr2)) / locations2.Count);
            var zAvr = measurements.Select(l => l.Z).Where(l => !double.IsNaN(l)).Average();
            var stdDevZ = Math.Sqrt(measurements.Select(l => l.Z).Where(l => !double.IsNaN(l)).Sum(d => (d - zAvr) * (d - zAvr)) / measurements.Select(l => l.Z).Where(l => !double.IsNaN(l)).Count());
            var meanH = distances.Average();
            var stdDevH = Math.Sqrt(distances.Sum(d => d * d) / distances.Count);
            var stdErrorH = stdDevH / Math.Sqrt(distances.Count);
            var marginOfErrorH = stdErrorH * 2;

            Dispatcher.Invoke(() =>
            {
                SecondMeterLabel.Text = $"{maxDif.ToString("0.###")}m";
                FirstMeterLabel.Text = $"{(maxDif / 2).ToString("0.###")}m";
                // Specify the area of the bitmap that changed.
                var writeableBitmap = new WriteableBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Bgra32, null);
                writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
                plot.Source = writeableBitmap;
                Status.Text = $"Measurements: {measurements.Length}\nAverage:\n - Latitude: {latAvr.ToString("0.0000000")}\n - Longitude: {lonAvr.ToString("0.0000000")}\n - Elevation: {zAvr.ToString("0.000")}m\nStandard Deviation:\n - Latitude: {stdDevLat.ToString("0.###")}m\n - Longitude: {stdDevLon.ToString("0.###")}m\n - Horizontal: {stdDevH.ToString("0.###")}m\n95% confidence: {marginOfErrorH.ToString("0.###")}m\n - Elevation: {stdDevZ.ToString("0.###")}m";
            });
        }

        internal static class Vincenty
        {
            private const double D2R = 0.01745329251994329576923690768489; //Degrees to radians

            public static double GetDistanceVincenty(double lat1, double lon1, double lat2, double lon2)
            {
                return GetDistanceVincenty(lat1, lon1, lat2, lon2, 6378137, 6356752.31424518); //Uses WGS84 values
            }

            /// <summary>
            /// Vincenty's formulae is used in geodesy to calculate the distance
            /// between two points on the surface of a spheroid.
            /// </summary>
            public static double GetDistanceVincenty(double lat1, double lon1, double lat2, double lon2, double semiMajor, double semiMinor)
            {
                var a = semiMajor;
                var b = semiMinor;
                var f = (a - b) / a; //flattening
                var L = (lon2 - lon1) * D2R;
                var U1 = Math.Atan((1 - f) * Math.Tan(lat1 * D2R));
                var U2 = Math.Atan((1 - f) * Math.Tan(lat2 * D2R));
                var sinU1 = Math.Sin(U1);
                var cosU1 = Math.Cos(U1);
                var sinU2 = Math.Sin(U2);
                var cosU2 = Math.Cos(U2);

                double lambda = L;
                double lambdaP;
                double cosSigma, cosSqAlpha, sinSigma, cos2SigmaM, sigma, sinLambda, cosLambda;

                int iterLimit = 100;
                do
                {
                    sinLambda = Math.Sin(lambda);
                    cosLambda = Math.Cos(lambda);
                    sinSigma = Math.Sqrt((cosU2 * sinLambda) * (cosU2 * sinLambda) +
                        (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda) * (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda));
                    if (sinSigma == 0)
                        return 0;  // co-incident points

                    cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;
                    sigma = Math.Atan2(sinSigma, cosSigma);
                    double sinAlpha = cosU1 * cosU2 * sinLambda / sinSigma;
                    cosSqAlpha = 1 - sinAlpha * sinAlpha;
                    cos2SigmaM = cosSigma - 2 * sinU1 * sinU2 / cosSqAlpha;
                    if (double.IsNaN(cos2SigmaM))
                        cos2SigmaM = 0;  // equatorial line: cosSqAlpha=0 (§6)
                    double C = f / 16 * cosSqAlpha * (4 + f * (4 - 3 * cosSqAlpha));
                    lambdaP = lambda;
                    lambda = L + (1 - C) * f * sinAlpha *
                        (sigma + C * sinSigma * (cos2SigmaM + C * cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM)));
                } while (Math.Abs(lambda - lambdaP) > 1e-12 && --iterLimit > 0);

                if (iterLimit == 0) return double.NaN;  // formula failed to converge

                var uSq = cosSqAlpha * (a * a - b * b) / (b * b);
                var A = 1 + uSq / 16384 * (4096 + uSq * (-768 + uSq * (320 - 175 * uSq)));
                var B = uSq / 1024 * (256 + uSq * (-128 + uSq * (74 - 47 * uSq)));
                var deltaSigma = B * sinSigma * (cos2SigmaM + B / 4 * (cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM) -
                    B / 6 * cos2SigmaM * (-3 + 4 * sinSigma * sinSigma) * (-3 + 4 * cos2SigmaM * cos2SigmaM)));
                var s = b * A * (sigma - deltaSigma);

                s = Math.Round(s, 3); // round to 1mm precision
                return s;
            }
        }
        bool autoFit = true;
        double currentScale = 1;
        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            autoFit = false;
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            autoFit = true;
            UpdatePlot();
        }

        private void plot_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var maxDif = Math.Round(currentScale * (Math.Min(size.Width, size.Height) * .5), 3);
            var dif = 1;
            if (maxDif < 1)
                maxDif = e.Delta < 0 ? maxDif * 2 : maxDif / 2;
            else
                maxDif = e.Delta < 0 ? maxDif + 1 : maxDif - 1;
            if (maxDif < 0.05)
                maxDif = 0.05;
            else if (maxDif < 1)
                maxDif = Math.Ceiling(maxDif * 10) / 10d;
            else
                maxDif = Math.Ceiling(maxDif);
            currentScale = maxDif / (Math.Min(size.Width, size.Height) * .5);

            if (autoFitToggle.IsChecked == true)
            {
                autoFitToggle.IsChecked = false;
            }
            else
            {
                UpdatePlot();
            }
        }
    }
}