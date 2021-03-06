# sf-dataexport

.NET Core Global Tool for for administrative tasks of Salesforce.

 
## Installation

Download the executable file here https://github.com/ste80/SF-DataExport/tree/master/SF-DataExport/Self-Contained

OR

Download the [.NET Core SDK 2.1.300](https://aka.ms/DotNetCore21) or later.
Install the [`sf-dataexport`](https://www.nuget.org/packages/sf-dataexport)
.NET Global Tool, using the command-line:

```
dotnet tool install -g sf-dataexport
```

## Version update

```
dotnet tool update -g sf-dataexport
```

## Usage

```
Usage: sf-dataexport
```

Features

 * Display org chart
 * Display all user profile photo
 * Quick search and login as any user (UI + command line)
 * Download prescheduled data export zip files (UI + command line)
 * Display org api usages and statistic (UI + command line to log data)
 * Search objects and display page in browser (under construction)
 * CSV import/export from/to Sales/Excel/CSV/SQL, auto adjust format/time zone (under construction)

## Prerequisites

 * .NET Core 2.1 or later
 * The minimum **Windows** versions supporting the WebSocket library are Windows 8 and Windows Server 2012. [Read more](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets?redirectedfrom=MSDN&view=netframework-4.7.2).
 * Mono is required on **Linux**. Read more about installing Mono [here](https://www.mono-project.com/download/stable/#download-lin-ubuntu).
 * If you have issues running Chrome on Linux, the Puppeteer repo has a [great troubleshooting guide](https://github.com/GoogleChrome/puppeteer/blob/master/docs/troubleshooting.md).

## How does it work?

This is a .NET application with [Google Chrome](https://www.google.com/chrome/) rendering capabilities, communicates with the locally-installed browser instance using the [Puppeteer](https://github.com/GoogleChrome/puppeteer/) project, and implements a remote call infrastructure for communication between Node and the browser.

All the state changes and network IO are performed by .NET CLI, chrome is only responsible for UI rendering and OAuth flow.
