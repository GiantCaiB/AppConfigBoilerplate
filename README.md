# AppConfigBoilerplate
An appconfig class for .net 

## To initiate the class, just declare it in your Main method
>public static void Main(string[] args)
>{
>    var appConfig = AppConfig.Current;
>}

## Create json files as your config file
add appsettings.json and files for other environments: appsettings.devtest.json or appsettings.UAT.json...

## Last step to add a new configurable field
add Key-value-pairs in json file and add same name property in the class

>{
> "PortNumber": 1234
>}

>public int PortNumber {get; set;}
