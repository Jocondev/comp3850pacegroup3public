# VRChat Pet Project

A VRChat world where players can adopt, customise and interact with networked pets, built with Unity and UdonSharp.

Created by Apogee VR as a Macquarie University project for TFOM, Semester 1, 2026.

---

## Features

- **Networked pets** – each player's pet is synced, so everyone in the instance sees the same thing
- **Customisation** – change your pet's field colour, ears and tail rings
- **Interests** – toggle the pet's interests to change how it behaves

---

## Requirements

| Tool | Version |
|---|---|
| Unity | 2022.3.22f1 |

You also need a VRChat account that is allowed to upload worlds.

---

## Getting started

### 1. Get the project
Clone the repo, or download it as a ZIP with **Code → Download ZIP**:

### 2. Open the main scene
Open `Assets/World.unity`.

---

## Testing locally

1. Open the VRChat SDK panel: **VRChat SDK → Show Control Panel**.
2. Log in, then go to the **Builder** tab.
3. Click **Build & Test New Build**. To test networking, set the number of clients to 2 or more.

You can also press **Play** in the Unity editor for quick tests. Use **ClientSim**, which comes with the SDK, to simulate a player.

---

## Uploading the world to your own account

> **Important:** the scene is still linked to the original upload's world ID. Before uploading to a different account, you must detach it, or the upload will fail.

1. In the SDK Control Panel → **Builder**, fill in the world name, description and thumbnail.
2. Click **Build and Upload**.

---

## Third-party assets

| Asset | Author | Licence |
|---|---|---|
| Pixel UI - https://assetstore.unity.com/packages/2d/gui/pixel-ui-sci-fi-free-hud-kit-sampler-412096 | heyheythere | Standard Unity Asset Store EULA |
| Crate - https://sketchfab.com/3d-models/sci-fi-crate-0e60a62a8f1e4048b638a32eb0661015 | Darren McNerney 3D | CC Attribution |
| Table - https://sketchfab.com/3d-models/sci-fi-table-c212f516053e4375b70c871e4d423df4 | shtran | CC Attribution |
| 3D modelling of ears | VRChat user @psychVR | CC Attribution |
| Some texture images | freepik  | CC Attribution |

---

## Credits

| Name | Role |
|---|---|
| Felicity | Team leader, Lead designer & artist |
| Brendan | Lead developer |
| John | Lead cyber security analyst |
| Elliott | Lead World developer |
| Mohammad | Lead Software Analyst |

Client / sponsor: TFOM

---

## Licence

- **Code** is licensed under the [MIT Licence](LICENSE).
- **Documentation, art and media** are licensed under [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/).

If you reuse or build on this project, please credit Apogee VR.
