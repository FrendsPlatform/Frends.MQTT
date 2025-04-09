# Frends.MQTT.Send
The task connects to a MQTT broker, publishes a message to a given topic, then disconnects.

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)
[![Build](https://github.com/FrendsPlatform/Frends.MQTT/actions/workflows/Send_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.MQTT/actions)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.MQTT/Frends.MQTT.Send|main)

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
- **.NET 8 SDK** installed (for running tests)

## One-Time Setup

- **Start the MQTT broker with certificates**:
- 
```bash
docker-compose up -d
```

## Run the Tests

Once the broker is running, run your .NET tests:

```bash
dotnet test
```

Clean Up:

```bash
docker-compose down --volumes
```

### Create a NuGet package

`dotnet pack --configuration Release`

### Third party licenses

StyleCop.Analyzer version (unmodified version 1.1.118) used to analyze code uses Apache-2.0 license, full text and source code can be found in https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/README.md
