# Nubank .Net Client

Unofficial Nubank client for .Net. This project was based in this [repo](https://github.com/lmansur/nubank-ruby) and in this [post](https://rhnasc.com/automation/golang/lamba/portuguese/2018/06/25/automating-nubank.html).

## Instalation

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
  
 ### Get Events (Transactions, Bill paid, etc.)
 ```csharp
 var events = await nubankClient.GetEvents();
 ```