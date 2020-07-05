# AppConfigBoilerplate
An appconfig class for .net 


public static void Main(string[] args)
{
    var appConfig = AppConfig.Current;
}

add appsettings.json and files for other environments: appsettings.devtest.json or appsettings.UAT.json...

add property in json file with the same property name in AppConfig.cs
