# RobotArm

A cross-platform client-server application for simulating and controlling a robotic arm, developed using **C#**, **.NET 8**, **ASP.NET Core**, and **Avalonia UI**.

RobotArm was developed as a team project for the Monitoring and Control Systems Software course to demonstrate multi-client communication, asynchronous command processing, priority-based scheduling, and role-based access control.

The application simulates a robotic arm operating within a 5×5 grid. Three desktop clients communicate with a central server, each with different permissions and command priorities.

## Features

- **Robotic Arm Simulation** – Control the position and rotation of a robotic arm within a 5×5 grid.
- **Multi-Client Architecture** – Three desktop clients communicate with a central ASP.NET Core server through HTTP API requests.
- **Role-Based Access Control** – Each client has a predefined set of permitted operations.
- **Client Authentication** – Validate client identities using API keys and Client IDs.
- **Asynchronous Command Processing** – Process commands through high- and normal-priority queues.
- **Priority-Based Scheduling** – Give higher processing priority to commands submitted by client K1.
- **Concurrency Control** – Synchronize access to the shared robotic arm state.
- **Operation Logging** – Store successful and rejected commands, timestamps, execution results, and state changes in SQLite.

## Client Roles

| Client | Allowed Operations | Priority |
|---|---|---|
| K1 | Left, Right, Up, Down, Rotate | High |
| K2 | Left, Right, Up, Down | Normal |
| K3 | Rotate | Normal |

Commands submitted by K1 are assigned to the high-priority queue, while commands from K2 and K3 are assigned to the normal-priority queue.

## Technologies

| Component | Technology |
|---|---|
| Programming Language | C# |
| Framework | .NET 8 |
| Backend | ASP.NET Core Web API |
| Desktop UI | Avalonia UI |
| Database | SQLite |
| ORM | Entity Framework Core |
| Communication | HTTP/HTTPS API |
| Authentication | API Keys and Client ID |

## Architecture

The application follows a client-server architecture consisting of a central backend and three desktop clients.

### Server

The server is responsible for:
- Authenticating clients and validating permissions.
- Maintaining the robotic arm's current position and rotation.
- Receiving and scheduling commands based on priority.
- Executing commands asynchronously through a background service.
- Synchronizing access to shared state.
- Persisting operation logs using Entity Framework Core and SQLite.

### Desktop Clients

Each desktop client is developed using Avalonia UI and provides:
- Controls for permitted robotic arm operations.
- Communication with the server through HTTP API requests.
- Client identification and API key authentication.
- Display of the current robotic arm state.
- Command results and HTTP response information.

## Project Structure

```text
RobotArm/
├── Server/                 # ASP.NET Core backend
├── Client.K1.Desktop/      # High-priority desktop client
├── Client.K2.Desktop/      # Movement-only desktop client
├── Client.K3.Desktop/      # Rotation-only desktop client
├── config/                 # Client authentication configuration
├── RobotArm.sln            # Visual Studio solution
└── README.md
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Entity Framework Core CLI tools (optional if already installed)
- HTTPS development certificate

Install the EF Core CLI tools if needed:

```bash
dotnet tool install --global dotnet-ef
```

Trust the HTTPS development certificate:

```bash
dotnet dev-certs https --trust
```

### Setup and Run the Server

Clone the repository:

```bash
git clone https://github.com/teodorabelic/RobotArm.git
cd RobotArm
```

Restore dependencies, apply database migrations, and start the server:

```bash
dotnet restore
cd Server
dotnet ef database update
dotnet run
# Server: https://localhost:5001
```

Alternatively, start the server with explicit development environment and URL settings:

**Linux/macOS:**

```bash
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS="https://localhost:5001;http://localhost:5000" dotnet run
```

**Windows PowerShell:**

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "https://localhost:5001;http://localhost:5000"
dotnet run
```

### Run the Desktop Clients

Open a new terminal for each client and navigate to the repository root directory.

Run the following commands separately:

```bash
dotnet run --project Client.K1.Desktop
dotnet run --project Client.K2.Desktop
dotnet run --project Client.K3.Desktop
```

### Client Configuration

Client authentication settings are defined in `config/clients.json`.

Each client has:
- `ClientId` – unique client identifier.
- `ApiKey` – authentication key.
- `Role` – determines permitted operations.

The included API keys are intended for local development and demonstration purposes only.

## Project Documentation

Additional technical details about the system architecture, command processing, authentication, and database logging are available in `dokumentacija.doc`.
