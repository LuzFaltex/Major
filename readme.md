# Major

Major is a Discord Interactive bot designed in collaboration with [Naka-Kon](https://naka-kon.com/) and LuzFaltex.

Development:

Before you begin development, please look over our [code contribution guide](https://docs.luzfaltex.com/contribute/code/contributing.html). This will cover coding styles, policies and procedures, and additional information related to your contribution. 

After you have read this guide, follow the steps below to configure your environment for development.

1. Fork and clone the code
1. Create Postgresql database (a docker instance will work fine)
1. Build the project in debug mode, then add `appsettings.json` to your debug output folder (`MajorInteractiveBot/bin/Debug/netcoreapp2.2/appsettings.json`) with the following text:
```json
{  
  "LogLevel": 2,
  "BotOwner": "Your Discord user Id"
}
```
4. In Visual Studio, right click on the `MajorInteractiveBot` project and select Manage User Secrets. Provide the following:
```json
{
  "DiscordToken": "Your Token",
  "ConnectionStrings": {
    "MajorDb": "User ID=username;Password=password;Host=server;Database=database"
  }
}
```

From this point forward, you should be able to launch the app and have it run as expected. If you wish to add a feature to the bot, please [create an issue](https://github.com/LuzFaltex/Major/issues/new) or post in an open issue to volunteer to make the requested changes. This allows the change to be reviewed and approved.
