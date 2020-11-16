# Hikvsion-Set

This is a simple utility to change Day/Night/Auto mode of 1/2 Hikvision camera. 

![HIK-Set](HIK-Set.png)

It is based on https://www.hikvision.com/en/support/download/sdk/device-network-sdk--for-windows-64-bit-/ version V6.1.6.3_build20200925 sample 1-Preview-PreviewDemo. CHCNetSDK.cs is from SDK. ..\bin is removed from DllImport.
```
        [DllImport(@"HCNetSDK.dll")]
        public static extern bool NET_DVR_Init();
```
 From this SDK necessary DLLs from SKDs\lib are included in project \bin folder. Visual Studio 2019 16.8.1 was used to compile this for x64 and .NET4.5. You can download project also as zip from https://github.com/jussivirkkala/Hikvision-Set/archive/main.zip) and start HIK-Set.exe from unzipped bin folder. Remember first to set correct parameters in HIK-Set.ini.

```
# Camera 1
192.168.106.5
8000
admin
password
# Camera 2
192.168.106.6
8000
admin
password
```

 You can also rename HIK-Set.exe and HIK-Set.ini to more descriptive name e.g CameraSet. Log file .log is also created.

Please provide feedback by making an issue or through tweet https://twitter.com/jussivirkkala.
