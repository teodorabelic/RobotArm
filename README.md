# RobotArm (Tema 4) — Server + 3 klijenta (cross‑platform)


## Preuslovi
- .NET 8 SDK
- (Opc.) EF Core CLI: `dotnet tool install --global dotnet-ef`
- Dev sertifikat: `dotnet dev-certs https --trust`

## Priprema
```bash
dotnet restore
cd Server
dotnet ef database update
dotnet run
# server na https://localhost:5001 
```

U drugom terminalu pokreni klijente (svaki posebno):
```bash
dotnet run --project Client.K1.Desktop
dotnet run --project Client.K2.Desktop
dotnet run --project Client.K3.Desktop
```

### API ključevi / Role
Podešeni u `config/clients.json`:
- K1 — KEY_K1_123 — role K1 (sve komande)
- K2 — KEY_K2_123 — role K2 (samo left/right/up/down)
- K3 — KEY_K3_123 — role K3 (samo rotate)

### komanda za server
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS="https://localhost:5001;http://localhost:5000" dotnet run