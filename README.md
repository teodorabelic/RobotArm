# RobotArm (Tema 4) — Server + 3 klijenta (cross‑platform)

Ovo je kompletan skeleton koji pokrećeš na **macOS** ili **Windows**.

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
# server na https://localhost:7043 (primer)
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

## Publish (za predaju)
Primera radi za macOS (Apple Silicon) i Windows x64:
```bash
# Server
dotnet publish Server -c Release -r osx-arm64 --self-contained true -o bin/Server-osx
dotnet publish Server -c Release -r win-x64   --self-contained true -o bin/Server-win

# Klijenti (ponovi za K2 i K3)
dotnet publish Client.K1.Desktop -c Release -r osx-arm64 --self-contained true -o bin/Client.K1-osx
dotnet publish Client.K1.Desktop -c Release -r win-x64   --self-contained true -o bin/Client.K1-win
```

## Šta demonstriraš profesoru
- Prioritet: K1 > (K2=K3), FIFO u okviru istog reda
- Permisije: K2 ne može rotate; K3 ne može pomeranje
- Granice 5×5, rotacije 0/90/180/270
- Svaki pokušaj upisan u SQLite `OperationLog` sa vremenskim pečatom