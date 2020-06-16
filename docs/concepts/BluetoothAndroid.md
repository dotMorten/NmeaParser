# Creating a Bluetooth device in an Android app

First ensure you have the necessary permissions in the AndroidManifest.xml file:

```xml
	<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
	<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
	<uses-permission android:name="android.permission.BLUETOOTH" />
```

Next see MainActivity.cs in the Android sample as a reference:

https://github.com/dotMorten/NmeaParser/blob/main/src/SampleApp.Droid/MainActivity.cs

