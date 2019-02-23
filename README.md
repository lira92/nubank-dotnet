# Nubank .Net Client

Unoficial Nubank client for .Net. This project is based in this [repo](https://github.com/lmansur/nubank-ruby) and in this [post](https://rhnasc.com/automation/golang/lamba/portuguese/2018/06/25/automating-nubank.html): 

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

  ```csharp
  var nubankClient = new Nubank(login, password);
  await nubankClient.Login();
  var events = await nubankClient.GetEvents();
  ```
