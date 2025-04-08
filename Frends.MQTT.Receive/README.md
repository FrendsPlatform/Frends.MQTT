# Frends.MQTT.Receive
The task connects to a MQTT broker, subscribes to a topic, and listens for incoming messages for a given amount of seconds. Afterwards (after the task ends), it returns a list of messages. This client works neither like an email client nor a queue client: messages come down at unknown intervals at the server's convenience. This means if messages arrive while the client is not yet alive, not yet succesfully subscribed, or disposed of at the end of the task, the messages will be lost (at least in QoS 0). 

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)
[![Build](https://github.com/FrendsPlatform/Frends.MQTT/actions/workflows/Receive_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.MQTT/actions)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.MQTT/Frends.MQTT.Receive|main)

## Installing

You can install the Task via frends UI Task View.

## Building

### Clone a copy of the repository

`git clone https://github.com/FrendsPlatform/Frends.MQTT.git`

### Build the project

`dotnet build`

### Run tests

## Prerequisites

- **Docker** installed  
- **.NET SDK** installed (for running tests)

---

## One-Time Setup: Generate Certificates

These commands create a Certificate Authority and sign a server certificate.

Run these in **Git Bash**, **WSL**, or **Linux/macOS terminal**.

```bash
openssl genrsa -out mosquitto/config/ca.key 2048
openssl req -new -x509 -days 365 -key mosquitto/config/ca.key -out mosquitto/config/ca.crt -subj "//CN=MQTT-Test-CA"

openssl genrsa -out mosquitto/config/server.key 2048
openssl req -new -key mosquitto/config/server.key -out mosquitto/config/server.csr -subj "//CN=localhost"
openssl x509 -req -in mosquitto/config/server.csr -CA mosquitto/config/ca.crt -CAkey mosquitto/config/ca.key -CAcreateserial -out mosquitto/config/server.crt -days 365
```

---

## Create Password File for Mosquitto

This sets up a test user for MQTT auth. Run these in **Power Shell**, in Test project directory.

```bash
New-Item -ItemType File -Path ".\mosquitto\config\passwd" -Force | Out-Null
docker run --rm -v "$(Resolve-Path .\mosquitto\config):/mosquitto/config" eclipse-mosquitto mosquitto_passwd -b /mosquitto/config/passwd testuser testpass
```

**Username:** `testuser`  
**Password:** `testpass`

---

## Set File Permissions (Linux/macOS)

```bash
chmod 644 mosquitto/config/*.crt
chmod 600 mosquitto/config/server.key
```

---

## File Structure

```
mosquitto/
└── config/
    ├── mosquitto.conf     # Broker config (included in repo)
    ├── ca.crt             # CA certificate
    ├── server.crt         # Server certificate
    ├── server.key         # Server private key
    ├── passwd             # MQTT user credentials
```

Only `mosquitto.conf` is committed to Git. All other files in `mosquitto/config` are generated locally and ignored via `.gitignore` to avoid committing sensitive data.

---

## Start the Mosquitto Broker

### On Windows (Command Prompt or PowerShell)

```powershell
docker run -p 1883:1883 -p 8883:8883 -v "C:\Full\Path\To\mosquitto\config:/mosquitto/config" eclipse-mosquitto
```

Replace `C:\Full\Path\To\...` with the actual absolute path on your system.

### On Linux/macOS or Git Bash (Windows)

```bash
docker run -d -p 1883:1883 -p 8883:8883 -v "$(pwd)/mosquitto/config:/mosquitto/config" eclipse-mosquitto
```

---

## Run the Tests

Once the broker is running, run your .NET tests:

```bash
dotnet test
```

### Create a NuGet package

`dotnet pack --configuration Release`

### Third party licenses

StyleCop.Analyzer version (unmodified version 1.1.118) used to analyze code uses Apache-2.0 license, full text and source code can be found in https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/README.md
