# Hikvision-Set

Hik-Set is a simple utility to change Day/Night/Auto mode of 1-2 Hikvision camera. 

- 2020-11-19 1.2.0 Log with fewer rows
- 2020-11-18 1.1.0 Using .ini and writing .log
- 2020-11-08 1.0.0 First version.

![HIK-Set](HIK-Set.png)

It is based on https://www.hikvision.com/en/support/download/sdk/device-network-sdk--for-windows-64-bit-/ version V6.1.6.3_build20200925 and sample 1-Preview-PreviewDemo. CHCNetSDK.cs is from SDK. From CHCNetSDK.cs all ..\bin paths are removed:
```
...
[DllImport(@"HCNetSDK.dll")]
public static extern bool NET_DVR_Init();
...
```
 All necessary DLLs (6.1.6.3 and libeay, ssleay 1.0.2.20) from SKDs\lib are included in project \bin folder
 ```
 HCCore.dll
 HCNetSDK.dll
 libeay32.dll
 ssleay32.dll
 HCNetSDKCom\HCCoreDevCfg.dll
 HCNetSDKCom\HCPreview.dll
 ```
 Visual Studio 2019 version 16.8.1 was used to compile this for x64 and .NET4.5 (you must have it installed). You can download project also as zip from https://github.com/jussivirkkala/Hikvision-Set/archive/main.zip) and start HIK-Set.exe from unzipped bin folder. Remember first to set correct parameters (IP, username, port,password) in HIK-Set.ini.
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
 In .ini all rows starting with # are ignored. If you only have one camera set port number of camera 2 to 0. You can also rename HIK-Set.exe and HIK-Set.ini to more descriptive name e.g CameraSet. Log file .log is appended automatically. When application is closed Auto mode command is transmitted. Application stays always on top and has opacity of 5%. Preset commands are: 39 Day mode (IR cut filter in), 40 Night mode (IR cut filter out), 46 Day/Night Auto Mode.


Tested with DS-2DE2204IW-DE3 https://www.hikvision.com/en/products/IP-Products/PTZ-Cameras/Value-Series/DS-2DE2204IW-DE3-W/ with V5.6.11 build 190416 with Stratus software. Please provide feedback by making an issue or through tweet https://twitter.com/jussivirkkala.
