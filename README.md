# Nubank .Net Client

[![Build status](https://ci.appveyor.com/api/projects/status/hj8cik3bjee9q1j0?svg=true)](https://ci.appveyor.com/project/lira92/nubank-dotnet)
[![NuGet](https://img.shields.io/nuget/v/NubankClient.svg)](https://www.nuget.org/packages/NubankClient)

Unofficial Nubank client for .Net. This project was based in this [repo](https://github.com/lmansur/nubank-ruby) and in this [post](https://rhnasc.com/automation/golang/lamba/portuguese/2018/06/25/automating-nubank.html). Thanks [@andreroggeri](https://github.com/andreroggeri) for your QR Code implementation using Python, my implementation was based in yours.

## Installation

This package is available through Nuget Packages: https://www.nuget.org/packages/NubankClient
 
**Nuget**
```
Install-Package NubankClient
```

**.NET CLI**

```
dotnet add package NubankClient
```

## Usage

### Login (Login is required for any other request)
 ```csharp
 var nubankClient = new Nubank(login, password);
 await nubankClient.Login();
 ```

 ### Device Authorization

Login in most part of devices is asking for authorization through QR Code, Nubank Client returns a response that indicate if device authorization is needed when login is requested. See an example:
 ```csharp
 var nubankClient = new Nubank(login, password);
 var loginResponse = await nubankClient.Login();
 if (loginResponse.NeedsDeviceAuthorization) {
    var qrcode = loginResponse.GetQrCodeAsAscii();
    // Here you can get qrcode as bitmap too.
    // var qrcode = loginResponse.GetQrCodeAsBitmap();

    // Now the user needs to scan QRCode using your device.
    // The user needs to access nubank in his smartphone and navigate to menu: Nu(Seu Nome) > Perfil > Acesso pelo site.
    // After user scan QRCode:
    await nubankClient.AutenticateWithQrCode(loginResponse.Code);
 }
 ```

If you need, it is possible to make the process asynchronous like this:
 ```csharp
 var nubankClient = new Nubank(login, password);
 var loginResponse = await nubankClient.Login();
 if (loginResponse.NeedsDeviceAuthorization) {
   var qrcode = loginResponse.GetQrCodeAsAscii();

   //Do something with QrCode, save, send, whatever.
 }
 ```
 After user scan QrCode, you can create another NubankClient and you can bypass login.

```csharp
 var nubankClient = new Nubank(login, password);
 await nubankClient.AutenticateWithQrCode(previousGeneratedCode);
 // Now you can get events
 ```
  
 ### Get Events (Transactions, Bill paid, etc.)
 ```csharp
 var events = await nubankClient.GetEvents();
 ```
Note: The Nubank api returns amount of events without decimal separators, to get a decimal to represent the amount with decimal separators use CurrencyAmount property of Event Class.
