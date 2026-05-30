# Realistic A320 Fenix Failures

Realistic A320 Fenix Failures is a background application designed to improve failure realism for the Fenix A320 in Microsoft Flight Simulator 2024.

The purpose of the project is to create a more immersive and believable failure system by combining simulator context with aircraft-specific system data, allowing failures to occur in a way that feels operationally coherent instead of purely random.

## Vision

This project aims to bring a deeper level of realism to the Fenix A320 by generating failures that make sense in context, feel plausible during operation, and add meaningful challenge to each flight.

## Project Goal

The main goal is to develop a tool that can:
- run in the background
- read relevant simulator and aircraft data
- understand the current flight context
- trigger realistic failures in the Fenix A320
- provide a more dynamic and authentic failure experience than the default options

## Core Concept

The application is based on a hybrid approach:

- **SimConnect** is used to read general simulator context
- **Fenix integration** is used to access aircraft-specific systems and failure logic

This is necessary because the Fenix A320 does not rely entirely on the default simulator aircraft logic. Many important aircraft systems are simulated externally, so simulator variables alone are not always enough to reflect the real aircraft state.

## Why SimConnect

SimConnect is the official API for Microsoft Flight Simulator and is the preferred interface for this project.

It can be used to obtain information such as:
- altitude
- position
- speed
- heading
- phase-of-flight context
- environmental and general aircraft state data

For this project, SimConnect is a better fit than FSUIPC because:
- it is the official Microsoft interface
- it is better aligned with MSFS 2020 and MSFS 2024
- it avoids unnecessary third-party dependency layers
- it provides the simulator context needed by the failure engine

## Why Not Use Only SimConnect

Although SimConnect is very useful, it should not be treated as the only source of truth for the Fenix A320.

The Fenix aircraft models many systems independently from the default simulator. Because of that, some values exposed by the simulator may not fully represent the actual internal aircraft state.

For that reason, the intended architecture is:

- **SimConnect** for simulator context
- **Fenix-side integration** for real aircraft systems and failure control

## Planned Features

- Context-aware failure triggering
- Failures based on flight phase
- Probabilistic failure logic
- More realistic operational scenarios
- Support for configurable realism profiles
- Possibility of progressive degradation and non-instant failures
- Background monitoring with low user interaction

## Technical Direction

The current technical direction is centered around:
- **C#**
- **Microsoft Flight Simulator SDK**
- **SimConnect**
- background app or service architecture
- modular logic for future rule expansion

## SimConnect SDK

The recommended way to work with SimConnect is through the official Microsoft Flight Simulator SDK.

Typical managed DLL path:

`MSFS SDK\SimConnect SDK\lib\managed\Microsoft.FlightSimulator.SimConnect.dll`

To install the SDK:
1. Open Microsoft Flight Simulator
2. Enable Developer Mode
3. Open the Developer menu
4. Go to Help
5. Launch the SDK Installer

### Dependencias externas

- Microsoft SimConnect (MSFS SDK) — no incluido. Obtenerlo desde el SDK oficial.  
- Fenix Simulations — software comercial; no redistribuido ni incluido.

## Licencia
Este proyecto está bajo la Licencia Apache 2.0. Consulta el archivo [LICENSE](LICENSE) para más detalles.

## Donaciones
Si este software te resulta útil y quieres apoyar su mejora continua, puedes realizar una donación voluntaria.