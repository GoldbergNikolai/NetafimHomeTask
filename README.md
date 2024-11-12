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
- Install SQL server on Docker

## Setup Instructions

### 1. Configure Docker for Windows Containers

This project requires Docker to run in **Windows containers** mode. Follow the steps below to switch Docker:

- **Option 1**: Right-click on the Docker icon in the taskbar, then click **Switch to Windows containers**.
- **Option 2**: Run the following commands in PowerShell or Command Prompt to switch Docker to Windows containers:
  
  ```bash
  cd 'C:\Program Files\Docker\Docker'
  ./DockerCli.exe -SwitchDaemon

### 2.To set up SQL Server on Docker

- **Step 1**: Pull the SQL Server Docker Image:
  ```bash
  docker pull mcr.microsoft.com/mssql/server:2022-latest
  ```
- **Step 2**: Run SQL Server Container: Replace YourPassword with a secure password.
  ```bash
  docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=A1s2D3f4G5h6" -p 1433:1433 --name sql1 -d mcr.microsoft.com/mssql/server:2022-latest
  ```
- **Step 3**: Check what IpAddress docker created SQL server on:
	- a. Go to Docker container you created(You can spot it by first 10 chars of long sequence you got after running command on paragragh 2.) into Inspect TAB
	- b. Search for IpAddress under the NetworkSettings
	- c. COPY it and paste in appsettings.json -> DefaultConnection-> Server=<ip_address_you_copied>
