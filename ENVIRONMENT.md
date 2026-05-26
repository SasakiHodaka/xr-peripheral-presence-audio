# XR Presence Audio Research Environment

## Unity

- Project path: `C:\Users\acd-pc67\My project`
- Unity Editor: `2022.3.62f3`
- Required package currently in use: `com.meta.xr.sdk.audio@85.0.0`

## Required External Tools

- Git for Windows `2.54.0.windows.1`
- Git LFS `3.7.1`
- .NET SDK `8.0.421`, optional but useful for C# tooling
- Visual Studio 2022 Community with Unity game development workload

## Install / Verification

Reopen PowerShell after installing tools, then verify:

```powershell
git --version
dotnet --version
```

If PATH has not refreshed yet, use:

```powershell
& "C:\Program Files\Git\cmd\git.exe" --version
& "C:\Program Files\dotnet\dotnet.exe" --version
```

## Git Setup After Installing Git

Run these commands from the project root:

```powershell
cd "C:\Users\acd-pc67\My project"
git init
git lfs install --local
git add Assets Packages ProjectSettings .gitignore .gitattributes .vsconfig ENVIRONMENT.md
git commit -m "Initial Unity research environment"
```

Do not commit Unity generated folders such as `Library`, `Logs`, `Temp`, `obj`, or `UserSettings`.

## Compile Check

The primary compile check for this Unity project is Unity batch mode:

```powershell
& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\acd-pc67\My project" -logFile "C:\Users\acd-pc67\My project\Logs\PeripheralCompileCheck.log"
```

Direct `dotnet build Assembly-CSharp.csproj` may require the `.NET Framework 4.7.1` targeting pack because Unity generates a non-SDK-style .NET Framework project.

## Unity Setup Check

1. Open the project with Unity `2022.3.62f3`.
2. Open `Assets/Scenes/SampleScene.unity`.
3. Run `Tools > Peripheral Research > Create Demo Hierarchy`.
4. Assign `XR Origin/Main Camera` or `Main Camera` to `PeripheralStateDetector.userHead` if it is empty.
5. Enter Play Mode and confirm that `peripheral_state_log.csv` is created under `Application.persistentDataPath`.
