# SCP:SL Plugin Project Starter

## Terminology And Quick Reference
[Add entries only when useful.]

- RA: Remote Admin.
- RA Panel: In-game Remote Admin GUI for server management, item spawning, kicks, and related admin actions. It also has a CLI mode.
- Server-Specific Settings: SCP:SL's built-in player-facing settings panel. Prefer it over cli commands so its more intuitive.
- Player Console: In-game CLI available to players. Use it for simple player-facing commands.

## Project Snapshot
[Concise technical overview for future agents. Overwrite stale notes instead of appending long histories.]

- Current local test port: `7777`.
- Plugin target: LabAPI `net48` plugin named `EnhancedShotgun`.
- Current feature: one enhanced shotgun spawns in LCZ GR-18; normal shotgun acquisition is replaced with AKs, while Chaos Marauder spawn shotguns are replaced with A7s.
- Enhanced shotgun behavior: doubled pickup model scale, provider-backed localized pickup hint, direct internal shotgun refill without reserve ammo grants, and 4x firearm damage.
- GR-18 access: gate requires all three keycard tiers/O5-style access and bypasses SCP-2176; plugin also rejects non-O5 door interactions plus grenade/SCP-018/pry-style door damage.

## SCP:SL Plugin-Specific Principles

**General**
- When starting a plugin or checking available APIs, read `.references\AGENTS.md` in parent folder and follow its LabAPI/decompiled lookup order.
- Treat each plugin folder as a separate product unless the user explicitly asks for shared code or a multi-plugin change.
- Prefer native game/LabAPI behavior over custom implementations unless custom behavior is specifically requested. Example: invoke the native RA ragdoll cleanup instead of manually recreating it.
- Ask before changing a request into a separate plugin/shared library, adding external dependencies, or using Harmony when a LabAPI event/wrapper might work.

**Testing**
- Always test before calling work complete. If no checks exist, say so and describe what was verified.
- Prefer verifying behavior directly when possible.
- For plugin code changes that need live/manual verification, deploy to `%APPDATA%\SCP Secret Laboratory\LabAPI\plugins\<active port>` and start/restart the visible test server. Do not launch the server headless, ensure a window pops up.
- Plugin configs are under `%APPDATA%\SCP Secret Laboratory\LabAPI\configs\<active port>\<Plugin Name>\`.
- For manual verification, keep detailed logs of steps, expected results, observed results, configs, commands, errors, and likely failure points. Assume feedback may only be "it does not work."

**UI And Text**
- SCP:SL does not support custom client-side UI. For admin-facing commands, prefer RA CLI commands. For player-facing options, prefer Server-Specific Settings; use Player Console commands as a fallback.
- For player text, always use the reusable `Templates\HintDisplayProvider` pattern instead of direct `Player.SendHint` calls from feature code, unless explicitly asked not to. The pattern is: feature services depend on an `IHintDisplayProvider`; RueI is preferred when loaded; missing RueI uses a null provider by default; a config flag may opt into a conservative vanilla compatibility provider.
- Keep stable hint IDs/tags per message type so provider-backed hints can be replaced or removed explicitly. RueI-style composed overlays are preferred for clean multi-plugin display; vanilla hints should only be short-duration, throttled, and enabled as a deliberate compatibility fallback because they cannot be removed by tag and can conflict with other plugins.
- Keep a clear hint/text system instead of improvising message behavior per feature.

## General Software And Game Design Principles

**Design**
- Do not create god classes. Split files by feature/domain.
- Keep server and client performance in mind, especially event frequency, polling, allocations, networking, and repeated hint/broadcast updates.
- When a feature would be cleaner as a separate plugin, explain why and ask for confirmation.

**Documentation**
- Create and maintain a localized user-facing `README.md` with a clear explanation of what the plugin does, how to use it, config files, player/RA commands, and known plugin conflicts.
- Keep `implementation-notes.md` current for non-trivial work: decisions, spec interpretations, intentional deviations, tradeoffs, plugin-conflict risks, and open questions.
- Keep `## Project Snapshot` in `AGENTS.md` updated and concise.
- Cite exact source paths when explaining SCP:SL, LabAPI, or native game behavior.

**Localization**
- Provide user-facing docs, UI text, hints, broadcasts, and prompts in both English and Chinese.
- Show only one language at a time. Prefer matching each player’s client game language when available.
- Add a `language` config setting where `""` means match client, `"cn"` forces Chinese, and `"en"` forces English.
- Default to `language: ""`; fall back to Chinese when the client language cannot be determined.
- Keep internal development notes, `AGENTS.md`, code comments, identifiers, and CLI commands in English.
