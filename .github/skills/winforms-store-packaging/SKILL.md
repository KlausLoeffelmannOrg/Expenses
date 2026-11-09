---
name: winforms-store-packaging
description: >-
  USE FOR: Store submission, package identity, signing, x64/ARM64 bundles, and
  full MSIX versus external-location identity after a vehicle exists. DO NOT
  USE FOR: choosing unpackaged publish versus WiX versus single-project MSIX.
  winforms-setup-and-deployment owns that choice.
---

# Packaging and Store readiness

## Durable rules

- The vehicle comes from [setup](../winforms-setup-and-deployment/SKILL.md). This skill does not invent a second packaging project when that choice already stands.
- Packaging is a deployment boundary, not a UI-stack rewrite.
- Inspect existing targets, project/build files, installer, identity, assets, native architectures and signing/version conventions before adding infrastructure.
- Full MSIX and external-location identity are different deployment models; do not present a sparse identity package as a normal full Store package.
- Add only required capabilities; package identity is a host concern, not a library silently repackaging its consumer.
- Report what was built/installed/submitted and which credential/device steps remain manual.

## Choose a model

| Goal | Preferred starting point |
|---|---|
| Conventional packaged Store desktop app; clean install/update/uninstall; architecture bundle | Full MSIX |
| Existing installer/binary location must remain; selected APIs need identity | Packaging with external location |
| Store distributes existing MSI/EXE | Separate Store installer-distribution model; not equivalent to full MSIX |

Unless installer constraints override, full MSIX is the Store shape. Use the vehicle [setup](../winforms-setup-and-deployment/SKILL.md) already chose: single-project MSIX, an existing packaging project, or external-location identity. Do not add a second packaging project from this skill.

The packaging project references the application payload; it owns manifest, identity, assets, capabilities, versions, architecture packaging and signing. Do not duplicate app source or silently change TFM.

Read [deployment checklist](references/deployment.md) before implementing project/runtime, manifest, signing or Store changes. For identity-gated map scenarios, [Maps](../winforms-windows-maps/SKILL.md) specifies its platform requirements; packaging alone does not make deprecated services viable.

## Architectures and runtime

Treat x64/ARM64 explicitly. AnyCPU alone is not the final architecture delivery story. Audit native DLLs, COM/PInvoke, graphics/capture libraries, WebView/runtime and helper executables. Build architecture-specific packages; bundle when appropriate. Include x86 only for an actual requirement.

Verify framework-dependent versus self-contained deployment/acquisition, native architecture and package size. Do not assume package dependency declarations automatically install arbitrary .NET runtimes. Check single-file/tooling compatibility rather than mixing incompatible publish settings.

## Final checks

0. The vehicle matches [setup](../winforms-setup-and-deployment/SKILL.md). This skill did not add a second packaging project.

1. Build supported unpackaged scenario and actual x64/ARM64 packaged outputs; install/launch/upgrade/uninstall/reinstall locally.
2. Exercise files/settings, shell/native/COM and identity APIs; packaged installation location is not writable application data.
3. Report architecture/signing/tooling assumptions, WACK/Store validation performed, and remaining Partner Center/manual steps. Never invent identity or claim unauthorized submission.
