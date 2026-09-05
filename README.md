# Turret Rush

A turret mounted on a car that drives itself down a desert road. The player aims;
the gun keeps its own time. Enemies wait ahead, charge when the car gets close,
and spend themselves on the impact. Reach the end of the level or lose the car.

Test task for Factura. Unity **6000.3.19f1**, URP, portrait, built for Android.

## Running it

Open the project and load `Assets/_Project/Scenes/Gameplay.unity`.

Tests: `Window → General → Test Runner` — **91 EditMode**, **15 PlayMode**.

## Stack

| | |
|---|---|
| **VContainer** | Zenject is archived; VContainer is maintained, supports Unity 6 and resolves with far less overhead. Its entry points are the reason there is no `Update()` anywhere in the gameplay code. |
| **UniTask** | The whole game loop is one awaited sequence. As a state machine it would be five states, a switch and a set of half-finished-transition flags. |
| **DOTween** | UI fades, the camera intro, recoil and hit reactions. |

Managed stripping is set to High, so `Assets/link.xml` preserves the assemblies
VContainer constructs by reflection. That failure only ever appears on device,
never in the editor.

## Architecture

One rule holds the rest together: **logic is plain C#, `MonoBehaviour` is a view.**
Systems own the rules and are driven by VContainer entry points; views hold scene
references and know how to display things.

`GameLifetimeScope` is the only place anything is registered, and the order it is
registered in is the order of a frame — the car moves, the barrel is pointed, the
gun fires down it, bullets advance, enemies react, the road catches up, the camera
looks at the result. It is also the order a restart runs in, which is what puts the
car back on the start line before the ground streamer lays its tiles around it.

```
Scripts/
  Bootstrap/   composition root, game flow, display settings
  Level/       level rules: session, progress, coins, IResettable
  Player/      car and turret - movement, aiming, views
  Combat/      health, weapon, projectiles, hit flash
  Enemies/     spawn plan, enemy system, enemy view
  World/       ground streaming, camera rig
  Vfx/  UI/    presentation, none of which gameplay knows about
```

Three interfaces exist in the whole project. `IDamageable` so a bullet does not
care what it hit, `IInputService` so nothing depends on how a finger arrives, and
`IResettable` so adding a system to the restart is an interface rather than another
line someone forgets.

## Some decisions worth pointing at

- **The turret writes a world rotation.** The car weaves as it drives; a barrel
  inheriting that rotation would wander away from wherever the player left it.
- **Bullets sweep, they do not carry colliders.** At 90 m/s a shot covers 1.5 m per
  frame against a body half a metre thick. A PlayMode test places a target entirely
  between two frames to prove it.
- **The enemy layout is planned from a seed, and stratified.** Uniform random over
  the level clumps — empty ground beside walls of enemies. Stratifying puts a floor
  under the gap between neighbours while keeping the spacing irregular.
- **Enemy health bars are drawn by one canvas, in immediate mode.** A world-space
  Canvas per body would rebuild twenty times a frame during a firefight. Nothing
  tracks which bar belongs to which enemy, so a body returning to the pool cannot
  leave one behind.
- **Pause is four lines.** Every system already steps itself by `Time.deltaTime`, so
  zeroing the time scale stops all of them at once.
- **Two hand-written shaders.** `Palette Flash` is a small lit shader with a
  per-renderer white-out for hit feedback, which URP/Lit has no parameter for.
  `Sky Gradient` takes its horizon colour from the fog colour, so the horizon stops
  being an edge.

Everything pooled resets its own state on release — the animator, the trail, the
flash, the flinch. That class of bug is invisible until an enemy comes back out of
the pool mid-stride and still glowing.

## Tests

EditMode covers the pure layer: hit points, the fire schedule, the drift curve, the
tile ring, turret aiming, the spawn planner, level progress, the wallet. PlayMode
covers the seams that only exist once physics and prefabs are assembled — the
damage chain, the layer mask, the car's footprint under rotation, and an encounter
end to end.

Configs are built by reflection in the test assembly rather than by adding setters
to shipping code.

## Time

Roughly **10.5 hours**.
