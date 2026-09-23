# Kerbalism Interstellar Compat

Adds support for several of KSP Interstellar: Extended's generators to Kerbalism's background simulation.

## Behavior

KSPIE generators that aren't integrated into a reactor will now generate power in the background, when the vessel is below 50% EC. This matches the in-flight behavior when KSPIE detects Kerbalism.

Reactors with integrated generators already generate their max off-screen power in the background, so those aren't modified.

## Requirements

* [Kerbalism](https://github.com/Kerbalism/Kerbalism)
* [KSP Interstellar: Extended](https://github.com/sswelm/KSP-Interstellar-Extended)