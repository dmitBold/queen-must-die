# Perf: baseline -> step1 -> step3_combined (ShadowLightBudget)

Editor Play Mode, Unity 6000.2.8f1, URP 17.2 Forward+, quality `PC`. Данные: `perf_log.csv`, `PerfLogger.Start(label, 30, 120, true)`.

Колонки:
- `baseline` — исходное состояние.
- `step1` — метка `step1_shadows_small` (Cast Shadows off у мелкого декора, коммит `9d69a018`).
- `step3 r1` / `step3 r2` — метки `step3_combined` и `step3_combined_r2`: step1 + ручная чистка 7 ассетов (`fda220ca`, `af80c4bf`) + ShadowLightBudget (`dac38d3a`). Два прогона подряд, чтобы видеть шум.
- `cleanup only` — метка `step3_cleanup_only`: то же, но компонент ShadowLightBudget выключен (step1 + ручная чистка без бюджета). Позволяет отделить вклад бюджета.

Как читать: `draw calls`, `shadow casters`, `triangles` почти не шумят (r1 и r2 совпадают в пределах 1–2%). `fps` и `gpu_ms` шумят заметно: r1 и r2 расходятся до 7 fps и до 2 мс GPU. Различия меньше этого — шум.

## fps_avg

| точка | baseline | step1 | step3 r1 | step3 r2 | cleanup only |
|---|--:|--:|--:|--:|--:|
| Office | 73.2 | 89.9 | 97.0 | 90.8 | 94.5 |
| Hall | 90.8 | 83.4 | 90.2 | 97.7 | 90.2 |
| Bedroom | 77.4 | 72.6 | 81.2 | 85.7 | 66.5 |
| Spawn | 77.7 | 78.7 | 69.6 | 73.6 | 71.6 |
| Reliquary | 93.1 | 93.5 | 87.6 | 94.3 | 92.9 |
| Laboratory | 89.2 | 89.8 | 90.6 | 93.8 | 89.6 |

## gpu_ms

| точка | baseline | step1 | step3 r1 | step3 r2 | cleanup only |
|---|--:|--:|--:|--:|--:|
| Office | 12.71 | 8.57 | 7.56 | 9.52 | 8.60 |
| Hall | 10.10 | 10.07 | 8.71 | 9.30 | 10.59 |
| Bedroom | 12.37 | 12.38 | 11.41 | 11.59 | 14.25 |
| Spawn | 11.96 | 12.00 | 12.40 | 13.16 | 13.35 |
| Reliquary | 6.33 | 6.31 | 4.66 | 4.91 | 8.27 |
| Laboratory | 5.62 | 4.65 | 4.00 | 4.46 | 5.79 |

## draw_calls

| точка | baseline | step1 | step3 r1 | step3 r2 | cleanup only |
|---|--:|--:|--:|--:|--:|
| Office | 3884 | 2634 | 2551 | 2571 | 2638 |
| Hall | 5027 | 4985 | 4649 | 4664 | 5017 |
| Bedroom | 7492 | 7514 | 6865 | 6807 | 7323 |
| Spawn | 7244 | 7208 | 8360 | 8393 | 7145 |
| Reliquary | 4904 | 4705 | 3259 | 3234 | 4555 |
| Laboratory | 3707 | 3658 | 2969 | 3001 | 3646 |

## shadow_casters

| точка | baseline | step1 | step3 r1 | step3 r2 | cleanup only |
|---|--:|--:|--:|--:|--:|
| Office | 2997 | 1735 | 1648 | 1662 | 1735 |
| Hall | 2374 | 2345 | 1921 | 1923 | 2345 |
| Bedroom | 4258 | 4221 | 3537 | 3538 | 4166 |
| Spawn | 2678 | 2629 | 4101 | 4162 | 2575 |
| Reliquary | 4265 | 4060 | 2411 | 2410 | 3978 |
| Laboratory | 2287 | 2228 | 1438 | 1439 | 2221 |

## triangles, млн

| точка | baseline | step1 | step3 r1 | step3 r2 | cleanup only |
|---|--:|--:|--:|--:|--:|
| Office | 24.88 | 11.18 | 10.65 | 10.73 | 11.19 |
| Hall | 19.59 | 19.56 | 17.99 | 17.93 | 19.36 |
| Bedroom | 24.62 | 24.82 | 22.49 | 22.59 | 23.70 |
| Spawn | 22.77 | 22.60 | 24.71 | 24.89 | 21.95 |
| Reliquary | 9.90 | 9.84 | 6.33 | 6.34 | 8.76 |
| Laboratory | 8.21 | 8.13 | 5.75 | 5.75 | 7.81 |
