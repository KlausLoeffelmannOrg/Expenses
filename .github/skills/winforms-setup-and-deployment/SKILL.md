---
name: winforms-setup-and-deployment
description: >-
  USE FOR: choosing the minimal WinForms install vehicle — unpackaged publish,
  single-project MSIX, WiX MSI, or a packaging project — including .vdproj
  migration and the Visual Studio components that vehicle needs. DO NOT USE FOR:
  Store submission, signing ceremony, or x64/ARM64 identity details after the
  vehicle is chosen. Those belong to winforms-store-packaging.
---

# WinForms Setup & Deployment

This skill chooses the vehicle. [Store packaging](../winforms-store-packaging/SKILL.md) does not choose a second one. It owns identity, architectures, signing, and Store submission after the vehicle exists.

How to pick the smallest deployment vehicle that satisfies a hard requirement, and how to
implement it. Default to the least infrastructure; escalate only when nothing smaller works.

## When to use this skill

Use this skill when:

- You find an old **Visual Studio Setup/Installer project** (`.vdproj`, typically NetFX 2.0–4.0
  era) that no longer loads in current Visual Studio.
- The app needs a **WinRT/UWP/WinUI consent-gated capability** (location, radios, microphone,
  Bluetooth, background tasks…) and there is no clean `CsWin32` / C#-WinRT / P/Invoke path.
- The app needs **machine-wide install, prerequisites, services, drivers, registry, or shortcuts**.
- The app must be **submitted to the Store** or distributed as a **signed package**.

## Pick the minimal infrastructure (decision ladder)

| Requirement | Smallest vehicle that satisfies it |
| --- | --- |
| Just run the app; no special OS gate | Plain `dotnet build` / framework-dependent or self-contained publish. No installer. |
| Consent-gated WinRT capability, no native shortcut exists | **Single-project MSIX** added to the *same* project (package identity). |
| Per-machine install, prerequisites, services, drivers, custom actions, COM/registry, Start-menu shortcuts | **WiX MSI** (optionally a Burn bundle to chain prerequisites). |
| Multiple apps in one package, or packaging a project you cannot modify | **Windows Application Packaging Project (WAP, `.wapproj`)**. |
| Store submission / signed public distribution | MSIX (single-project or WAP) **+ signing cert + hosting** (see below). |

Rules of thumb:

- Be minimal. A consent capability alone → MSIX, *not* a full WAP or MSI.
- MSIX and WiX/MSI solve **different** problems (capability identity vs. arbitrary
  install-time logic). Needing real install-time work (prereqs, services, machine state) almost
  always lands you back at **WiX + MSI**.
- When no smaller compromise covers a hard requirement, choose **WiX + MSI**.

## Migrating legacy Setup Projects (.vdproj) to WiX MSI

`.vdproj` is deprecated and does not load in modern Visual Studio. There is **no automatic
converter** — the structure is mapped by hand to WiX. Use the current **WiX Toolset (v5/v6)**.

Key aspects a migration must cover:

- **Tooling**: `dotnet tool install --global wix`; author an SDK-style `.wixproj`
  (`<Project Sdk="WixToolset.Sdk/5.0.x">`); build with `wix build Package.wxs -o Package.msi`
  or `dotnet build`.
- **Map every `.vdproj` concept** to WiX: files/folders → `Directory`/`Component`/`File`;
  shortcuts → `Shortcut`; registry → `RegistryValue`; custom actions → `CustomAction`
  (prefer declarative WiX over scripts); file types/COM → dedicated WiX elements.
- **Harvest** large existing file trees with the **Heat** extension instead of listing files
  by hand; keep generated authoring separate from hand-written authoring.
- **Identity for upgrades**: set a stable `UpgradeCode`, use `MajorUpgrade`, and keep
  component GUID/`Id` rules stable so repair/upgrade/uninstall keep working.
- **Install scope**: decide per-user vs per-machine (`InstallScope`/`ALLUSERS`); per-machine
  needs elevation.
- **Prerequisites / runtime** (e.g. the .NET runtime): chain them with a **Burn `Bundle`**
  (`.exe` bootstrapper), not loose merge modules.
- **Validate** with ICE/validation, then **sign** the MSI (and the bundle) with SignTool.

## Adding MSIX to unlock WinRT / UWP / WinUI capabilities

Try the cheaper paths first — many "UWP-looking" APIs work **unpackaged**:

1. `CsWin32` (source-generated P/Invoke) or **C#/WinRT** projection / direct P/Invoke.
2. Only if the API is **consent-gated** (`RequestAccessAsync`) does it require **package
   identity**. Such APIs return `Allowed` *only* from a packaged app that declares the matching
   capability in `Package.appxmanifest` and that the user consented to. No identity = `Denied`.

When identity is genuinely required:

- Prefer **single-project MSIX** over a separate WAP: in the app `.csproj` set
  `EnableMsixTooling=true` and `WindowsPackageType=MSIX`, and add a `Package.appxmanifest`
  (capabilities) plus the logo/image assets. The AppxPackage targets import only under Visual
  Studio MSBuild, so plain `dotnet build` still produces the *unpackaged* exe for the inner loop.
- Declare only the capabilities actually used; document the ones that need none.
- In-repo reference: `samples\cs\Sensored` (location/microphone).

### Visual Studio tooling prerequisites

To keep loading, building, and **debugging** the packaged app in Visual Studio, these must be
installed (component IDs are stable across VS 2022/2026 editions):

| Need | Official component | ID |
| --- | --- | --- |
| The WinForms/.NET app itself | .NET desktop development | `Microsoft.VisualStudio.Workload.ManagedDesktop` |
| Single-project MSIX (load/deploy/debug packaged) | MSIX Packaging Tools | `Microsoft.VisualStudio.Component.EnableMsixTooling` |
| WinUI / Windows App SDK ("Win UI development tools") | Windows App SDK C# Templates | `Microsoft.VisualStudio.ComponentGroup.WindowsAppSDK.Cs` |
| Build target | matching Windows SDK | e.g. `Microsoft.VisualStudio.Component.Windows11SDK.22000` |

In current VS 2022 there is **no separate "MSIX" marketplace extension** — the
`EnableMsixTooling` component supplies the single-project MSIX support.

### Detect + install script (self-elevating)

Offer to create and run this on the user's behalf; it self-elevates, detects with `vswhere`,
and installs only what is missing. It does **not** touch the machine when everything is present.

```powershell
#requires -Version 5.1
param(
    [string[]] $ComponentIds = @(
        'Microsoft.VisualStudio.Component.EnableMsixTooling',
        'Microsoft.VisualStudio.ComponentGroup.WindowsAppSDK.Cs'
    )
)

$identity  = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal] $identity
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Start-Process powershell.exe -Verb RunAs -ArgumentList (
        @('-NoProfile','-ExecutionPolicy','Bypass','-File',$PSCommandPath) + $ComponentIds)
    return
}

$installerDir = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer'
$vswhere = Join-Path $installerDir 'vswhere.exe'
if (-not (Test-Path $vswhere)) { throw 'vswhere.exe not found. Is Visual Studio 2017+ installed?' }

$installPath = & $vswhere -latest -products * -property installationPath
if (-not $installPath) { throw 'No Visual Studio installation found.' }

$missing = foreach ($id in $ComponentIds) {
    if (-not (& $vswhere -products * -requires $id -property installationPath)) { $id }
}
if (-not $missing) { Write-Host 'All required components already installed.'; return }

$addArgs = $missing | ForEach-Object { '--add', $_ }
$setup = Join-Path $installerDir 'setup.exe'
Write-Host "Installing: $($missing -join ', ')"
& $setup modify --installPath $installPath @addArgs --quiet --norestart --wait
```

## When a packaging project or Store submission is required

- **Single-project MSIX** covers local sideloading and the dev/debug loop for one app.
- Escalate to a **WAP (`.wapproj`)** when you must bundle several apps, package a project you
  cannot edit, or a pipeline specifically needs the packaging project.
- **Store submission / signed public distribution** additionally needs a **code-signing
  certificate** and, for non-Store delivery, a **hosting location** for the `.msix`/`.msixbundle`
  and the `.appinstaller` update manifest. A self-signed dev cert only enables *local* sideload.
  These are environment secrets/assets the agent cannot supply — **explicitly tell the user they
  must provide the signing key and the hosting space**, and stop until they do.

## Minimalism rule

Add the least setup infrastructure the requirement allows. Let the *required* context dictate the
effort: if a smaller compromise (unpackaged P/Invoke, then single-project MSIX) cannot meet a hard
requirement (prereqs, services, machine state, Store), escalate — and when nothing smaller works,
you are back at **WiX + MSI**.

## Validation

1. Name the vehicle and the requirement that ruled the smaller ones out.
2. Do not add a packaging project when single-project MSIX or unpackaged publish already meets the requirement.
3. Hand identity, architectures, and Store submission to [store packaging](../winforms-store-packaging/SKILL.md) instead of duplicating them here.

## Related skills

- **winforms-development** — project setup and solution structure.
- **winforms-designer-code** — Designer file rules for any setup-related UI.
