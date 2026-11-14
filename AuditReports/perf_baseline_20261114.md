# Baseline perf, scene NightCycle (label `baseline`)

- Measured: 2026-11-14 23:07, Unity 6000.2.8f1, URP 17.2.0 Forward+, quality level `PC`.
- Method: `QMDA.Audit.PerfLogger.Start("baseline", 30, 120, true)`. For each viewpoint the camera is teleported, 30 warmup frames are skipped and 120 frames are sampled.
- Raw data: `perf_log.csv`, rows with label `baseline`.
- Viewpoints: `viewpoints.json`. Laboratory was moved in this session.

## Results per viewpoint

| viewpoint | fps_avg | fps_min | frame_ms | cpu_main_ms | cpu_render_ms | gpu_ms | draw_calls | batches | setpass | triangles | shadow_casters |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Office | 73.2 | 24.3 | 13.67 | 10.96 | 4.12 | 12.71 | 3884 | 3668 | 602 | 24,882,486 | 2997 |
| Hall | 90.8 | 12.6 | 11.01 | 8.57 | 4.01 | 10.10 | 5027 | 4084 | 477 | 19,588,862 | 2374 |
| Bedroom | 77.4 | 33.2 | 12.92 | 10.48 | 5.19 | 12.37 | 7492 | 5720 | 724 | 24,623,329 | 4258 |
| Spawn | 77.7 | 25.4 | 12.88 | 10.17 | 5.35 | 11.96 | 7244 | 5930 | 672 | 22,765,558 | 2678 |
| Reliquary | 93.1 | 21.4 | 10.74 | 8.41 | 4.30 | 6.33 | 4904 | 3473 | 549 | 9,895,375 | 4265 |
| Laboratory | 89.2 | 15.7 | 11.21 | 8.76 | 4.36 | 5.62 | 3707 | 3101 | 508 | 8,207,708 | 2287 |

Edit Mode snapshot from the first audit (20:23, unspecified camera): 7647 draw calls, 16.2M triangles, 5139 shadow casters.

## What the numbers show

- **Heaviest points:** Office, Bedroom and Spawn. They have 22–25M triangles and 11.96–12.71 ms GPU, and they are the slowest in fps (73–78).
- **Bedroom and Spawn** have the most draw calls (7.2–7.5k) and setpass calls (672–724).
- **Reliquary and Laboratory** are lighter: 8–10M triangles and 5.6–6.3 ms GPU. Their frame time is still 10.7–11.2 ms, so CPU main thread (8.4–8.8 ms) probably limits them, not the GPU.
- **Other points:** Office, Bedroom, Spawn and Hall have GPU time at or above CPU main-thread time. Their frame time is close to GPU time.
- **Triangles are high everywhere.** Triangles are counted over all passes, including shadow passes. 62 realtime point lights with shadows and 8929 shadow-casting renderers (Stage 1) are the likely cause.
- **Shadow atlas overflow:** the Editor log reports URP dropping shadow maps because they do not fit the 2048x2048 atlas. This is one more sign that the shadowed lights are over budget.

## Caveats

- These are Editor Play Mode numbers, so use them only to compare A/B steps made in the same way. Do not read them as player-build fps.
- `fps_min` and `fps_p1low` are the same value, because 120 frames give 1% = 1 frame. They show single frame hitches, mostly at the start of each point, not a stable low. Use `fps_avg`, `frame_ms` and `gpu_ms` for comparison.
- One run per point and only 120 frames. Run-to-run noise is not measured yet. Before trusting small differences, repeat the baseline 2–3 times.
- The Editor shows GPU time via FrameTimingManager. It may include the Game view overhead.
- Free RAM was low during the run (Unity used about 5–7 GB of 16 GB). Close other apps before the next A/B steps.

## Laboratory viewpoint fix

- Old point (28.70, 7.78, 33.17), looking at yaw 90 / pitch 5, was outside the room geometry (empty skybox).
- New point: position (30.70, 7.79, 32.20), euler (15, 315, 0), fov 60. It was chosen on the walkable NavMesh next to the room lights, and the preview now shows room lights and ceiling geometry.
- The preview is dark because the scene is a night cycle; this is how the room looks, not a render failure.
- Backup of the previous file: `viewpoints.backup_before_lab_fix.json`.

## Next steps (A/B)

Change one thing at a time, then run `PerfLogger.Start("<label>", 30, 120, true)` and `PerfCompare.Compare("baseline", "<label>")`.

1. Realtime shadows of point lights: limit the number of shadow-casting point lights.
2. `lodBias` 2 to 1.
3. Cast Shadows off for small renderers.
4. Turn off distant rooms.
