using Esri.ArcGISRuntime.Location;
using System;
using System.Collections.Generic;
using System.Linq;
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
        List<double[]> locations = new List<double[]>();
        public PointPlotView()
        {
            InitializeComponent();
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PlotMap.Width = PlotMap.Height = Math.Min(e.NewSize.Width, e.NewSize.Height);
        }

        private void PlotMap_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePlot();
        }

        public void Clear()
        {
            locations.Clear();
            UpdatePlot();
        }
        public void AddLocation(double latitude, double longitude, double altitude)
        {
            Dispatcher.Invoke(() =>
            {
                locations.Add(new double[] { latitude, longitude, altitude });
                UpdatePlot();
            });
        }
        static SolidColorBrush dotFill = new SolidColorBrush(Colors.LightGreen);
        private void UpdatePlot()
        {
            if (canvas.ActualWidth == 0 || canvas.ActualHeight == 0 || !IsVisible)
                return;
            Status.Text = "";
            canvas.Children.Clear();
            if (locations.Count == 0)
                return;
            var latAvr = locations.Select(l => l[0]).Average();
            var lonAvr = locations.Select(l => l[1]).Average();
            
            //List<double> lonDistances = new List<double>(locations.Count);
            //List<double> latDistances = new List<double>(locations.Count);
            List<double> distances = new List<double>(locations.Count);
            var locations2 = new List<double[]>();
            foreach (var l in locations)
            {
                var d = Vincenty.GetDistanceVincenty(latAvr, lonAvr, l[0], l[1]);
                var dLat = Vincenty.GetDistanceVincenty(latAvr, lonAvr, l[0], lonAvr);
                var dLon = Vincenty.GetDistanceVincenty(latAvr, lonAvr, latAvr, l[1]);
                distances.Add(d);
                //latDistances.Add(dLat);
                //lonDistances.Add(dLon);
                if (latAvr > l[0]) dLat = -dLat;
                if (lonAvr > l[1]) dLon = -dLon;
                locations2.Add(new double[] { dLat, dLon });
            }
            var latMin = locations2.Select(l => l[0]).Min();
            var lonMin = locations2.Select(l => l[1]).Min();
            var latMax = locations2.Select(l => l[0]).Max();
            var lonMax = locations2.Select(l => l[1]).Max();
            var latAvr2 = locations2.Select(l => l[0]).Average();
            var lonAvr2 = locations2.Select(l => l[1]).Average();
            var maxDifLat = Math.Max(latAvr2 - latMin, latMax - latAvr2);
            var maxDifLon = Math.Max(lonAvr2 - lonMin, lonMax - lonAvr2);
            var maxDif = Math.Max(maxDifLat, maxDifLon);
            if (maxDif < .1)
                maxDif = .1;
            if (maxDif < .25)
                maxDif = .25;
            else if (maxDif < .5)
                maxDif = .5;
            else
                maxDif = Math.Ceiling(maxDif);
            //if(maxDif < 1)
            SecondMeterLabel.Text = $"{maxDif}m";
            FirstMeterLabel.Text = $"{maxDif/2}m";
            double scale = maxDif / Math.Min(OuterRing.ActualWidth, OuterRing.ActualHeight) / .5;
            if (scale == 0)
                scale = 1;
            for (int i = Math.Max(0, locations2.Count - 1000); i < locations2.Count; i++) // Only draw the last 1000 points
            {
                var l = locations2[i];
                var x = canvas.ActualWidth * .5 + (l[1] - lonAvr2) / scale;
                var y = canvas.ActualHeight * .5 - (l[0] - latAvr2) / scale;
                Ellipse e = new Ellipse() { Width = 3, Height = 3, Fill = dotFill };

                if (canvas.Children.Count == locations2.Count - 1)
                {
                    e.Fill = new SolidColorBrush(Colors.Red);
                    e.Width = 5;
                    e.Height = 5;
                }

                Canvas.SetLeft(e, x - e.Width * .5);
                Canvas.SetTop(e, y - e.Height * .5);
                canvas.Children.Add(e);
            }
            var stdDevLat = Math.Sqrt(locations2.Sum(d => (d[0] - latAvr2) * (d[0] - latAvr2)) / locations2.Count);
            var stdDevLon = Math.Sqrt(locations2.Sum(d => (d[1] - lonAvr2) * (d[1] - lonAvr2)) / locations2.Count);
            var zAvr = locations.Select(l => l[2]).Where(l => !double.IsNaN(l)).Average();
            var stdDevZ = Math.Sqrt(locations.Select(l => l[2]).Where(l => !double.IsNaN(l)).Sum(d => (d - zAvr) * (d - zAvr)) / locations.Select(l => l[2]).Where(l => !double.IsNaN(l)).Count());
            Status.Text = $"Average:\n - Latitude: {latAvr.ToString("0.0000000")}\n - Longitude: {lonAvr.ToString("0.0000000")}\n - Elevation: {zAvr.ToString("0.000")}m\nStandard Deviation:\n - Latitude: {stdDevLat.ToString("0.###")}m\n - Longitude: {stdDevLon.ToString("0.###")}m\n - Horizontal: {distances.Average().ToString("0.###")}m\n - Elevation: {stdDevZ.ToString("0.###")}m";
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

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }
    }
}