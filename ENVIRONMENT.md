# XR Presence Audio Research Environment

## Unity

- Project path: `C:\Users\acd-pc67\xr-peripheral-presence-audio`
- Unity Editor: `2022.3.62f3`
- Required package currently in use: `com.meta.xr.sdk.audio@85.0.0`

## Required External Tools

- Git for Windows `2.54.0.windows.1`
- Git LFS `3.7.1`
- .NET SDK `8.0.421`, optional but useful for C# tooling
- Visual Studio 2022 Community with Unity game development workload
- .NET Framework 4.7.1 Developer Pack / Targeting Pack for Visual Studio and MSBuild reference assemblies

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

Verify the .NET Framework 4.7.1 reference assemblies:

```powershell
Test-Path "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.1\mscorlib.dll"
```

## Git Check After Installing Git

Run these commands from the project root:

```powershell
cd "C:\Users\acd-pc67\xr-peripheral-presence-audio"
git lfs install --local
git status
```

Do not commit Unity generated folders such as `Library`, `Logs`, `Temp`, `obj`, or `UserSettings`.

## Compile Check

The primary compile check for this Unity project is Unity batch mode:

```powershell
& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\acd-pc67\xr-peripheral-presence-audio" -logFile "C:\Users\acd-pc67\xr-peripheral-presence-audio\Logs\PeripheralCompileCheck.log"
```

Unity batch mode is the authoritative compile check. Do not treat direct `MSBuild.exe "xr-peripheral-presence-audio.sln"` as the project health check; Unity generated projects can fail outside the Unity compiler because of Unity-specific `NoStdLib` and NetStandard shim references.

## Unity Setup Check

1. Open the project with Unity `2022.3.62f3`.
2. Open `Assets/Scenes/SampleScene.unity`.
3. Run `Tools > Peripheral Research > Create Demo Hierarchy`.
4. Assign `XR Origin/Main Camera` or `Main Camera` to `PeripheralStateDetector.userHead` if it is empty.
5. Enter Play Mode and confirm that `peripheral_state_log.csv` is created under `Application.persistentDataPath`.
