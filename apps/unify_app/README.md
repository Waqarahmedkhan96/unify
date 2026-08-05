# Unify App

Responsive Flutter client for Unify ERP.

## Run Locally

From the repository root:

```powershell
docker compose up -d --build
```

Then run the UI:

```powershell
cd apps/unify_app
..\..\flutter\bin\flutter.bat run -d chrome --dart-define=UNIFY_API_URL=http://localhost:5080
```

Development login:

```text
owner@unify.local
ChangeMe123!
```

## Build Web

```powershell
cd apps/unify_app
..\..\flutter\bin\flutter.bat build web --dart-define=UNIFY_API_URL=http://localhost:5080
```

Serve the built UI:

```powershell
cd build/web
python -m http.server 5200 --bind 127.0.0.1
```

Open:

```text
http://127.0.0.1:5200
```

## Current Targets

- Web: generated, analyzed, tested, and built.
- Android: project generated; Android SDK is still required for APK/device builds.
- Windows: project generated; full Visual Studio Desktop C++ workload is still required for native Windows builds.
