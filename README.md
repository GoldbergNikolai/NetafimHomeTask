Skip to content
Navigation Menu
GoldbergNikolai
/
NetafimHomeTask

Type / to search
Code
Issues
Pull requests
Actions
Projects
Security
Insights
Settings
Creating a new file in NetafimHomeTask
BreadcrumbsNetafimHomeTask
/
README.md
in
master

Edit

Preview
Indent mode

Spaces
Indent size

2
Line wrap mode

No wrap
Editing README.md file contents
Selection deleted
1
2
3
4
5
6
7
8
9
10
11
12
13
14
15
16
17
18
19
20
21
22
23
24
25
26
27
28
29
30
31
32
33
34
35
36
37
38
39
40
41
42
43
44
45
46
47
48
49
50
51
52
53
54
55
# Timer Service API

## Project Overview

This project is a webhook-based timer service built with .NET 8 and EF Core. It provides a RESTful API to manage timers that trigger webhooks upon completion. The service includes three endpoints: to set a timer, check a timer's status, and list all timers. This solution is designed for easy deployment in a Docker environment with Windows containers.

## Features

- **Set Timer**: Allows clients to create a new timer with specified hours, minutes, and seconds, and set a webhook URL that will be triggered when the timer expires.
- **Get Timer Status**: Retrieves the remaining time of a specified timer.
- **List Timers**: Lists all active timers with pagination.
- **Persistence**: Timers remain active even if the server restarts, ensuring reliable webhook execution.

## Requirements

- Run your project in Windows 10+ Pro version(In order to use Windows containers)
- [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (Windows Containers required)
- SQL Server LocalDB

## Setup Instructions

### 1. Configure Docker for Windows Containers

This project requires Docker to run in **Windows containers** mode. Follow the steps below to switch Docker:

- **Option 1**: Right-click on the Docker icon in the taskbar, then click **Switch to Windows containers**.
- **Option 2**: Run the following commands in PowerShell or Command Prompt to switch Docker to Windows containers:
  
  ```bash
  cd 'C:\Program Files\Docker\Docker'
  ./DockerCli.exe -SwitchDaemon
  
### 2. Setup you localDB

- **Step 1**: Check your local DB name if exist any:
  ```bash
  sqllocaldb i
- **Step 2**: Select local DB name, let's say its default 'MSSQLLocalDB', check it's status:
  ```bash
  sqllocaldb i MSSQLLocalDB
- **Step 3**: If status if 'Stopped', enable it:
  ```bash
  sqllocaldb start MSSQLLocalDB
- **Optional Step**: If you want to delete existing DB (Or recreate it):
  ```bash
  sqllocaldb delete MSSQLLocalDB
- **Step 4**: If local DB doesn't exists (deleted) create + start it:
- ```bash
  sqllocaldb create MSSQLLocalDB
  sqllocaldb start MSSQLLocalDB

- **Step 5**: Verify local DB created and running (Should be at status 'Running'):
```bash
  sqllocaldb i MSSQLLocalDB
Use Control + Shift + m to toggle the tab key moving focus. Alternatively, use esc then tab to move to the next interactive element on the page.
No file chosen
Attach files by dragging & dropping, selecting or pasting them.
New File at / · GoldbergNikolai/NetafimHomeTask
