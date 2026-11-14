# История оптимизации QMDA (сцена NightCycle)

Unity 6000.2.8f1, URP 17.2.0 Forward+. Все замеры: `AuditReports/perf_log.csv`, 6 точек из `viewpoints.json`, Editor Play Mode (цифры индикативные, сравнивать только между собой).

## Исходное состояние (Этап 1, разведка)

- Все комнаты активны одновременно, в сцене 5691 активный MeshRenderer и 170 SkinnedMeshRenderer (снимок перед шагом 1).
- Света: 233 активных, 153 с тенями (Point 131, Spot 22). Realtime из них только 62 (все Point), остальные 91 Baked.
- lodBias = 2 (Quality `PC`), URP shadowDistance 45, QualitySettings.shadowDistance 40, дополнительные тени 2048.
- По логу Editor URP отбрасывает часть shadow map: они не помещаются в атлас 2048x2048.
- Baseline: `perf_baseline_20261114.md`, метка `baseline` в `perf_log.csv`.
- Точка Laboratory в `viewpoints.json` была за геометрией комнаты (пустой скайбокс), переставлена на (30.70, 7.79, 32.20), euler (15, 315, 0). Резерв: `viewpoints.backup_before_lab_fix.json`.

## Шаг 1. Cast Shadows = Off у мелкого декора

- Коммит `9d69a018`. Скрипт `Assets/Audit/Editor/ShadowStep1.cs`, список объектов `step1_shadow_off_list.csv`.
- Изменено 1518 из 5691 активных MeshRenderer: всё меньше 0.6 м и объекты до 4 м с именем Candle / Torch / Book / Paper. SkinnedMeshRenderer, URP-ассет, lodBias и Static-флаги не трогались.
- Метка замера: `step1_shadows_small` (`perf_compare_step1_shadows_small.md`).
- Результат: заметный выигрыш только в Office (fps 73.2 -> 89.9, GPU 12.7 -> 8.6 мс, треугольники 24.9M -> 11.2M). В остальных точках изменения в пределах шума одного прогона.

## Ручная чистка тяжёлых ассетов (сделана руками, без PerfLogger)

Основание: диагностика shadow pass (только чтение). Один Candelabrum_Base LOD0 (Bedroom) давал 1.88M треугольников, BonesAndSkillPile_Var3 LOD0 (Office) давал 1.87M. Внутри LODGroup было 21.2M из 22.4M raw-треугольников кастеров, а 96.9% всех raw-треугольников приходилось на меши, у которых LOD0 больше 2000 треугольников.

Что сделано: у этих ассетов удалён компонент LOD Group, оставлен один самый экономный меш (LOD5). Правки лежат в префабах и в `NightCycle.unity`.

Ассеты: Candelabrum_Base, BonesAndSkillPile_Var1, Var3, Var4, PillarSection_01, Chain_A, Corridor_Ceiling_A.

| Коммит | Автор | Состав |
|---|---|---|
| `fda220ca` heavy assets cleanup. LOD 5 | DIMASHARDBASS111 | Префабы Candelabrum_Base, BonesAndSkillPile Var1/Var3/Var4, `NightCycle.unity` |
| `af80c4bf` heavy assets cleanup 2 | DIMASHARDBASS111 | Префабы SM_Chain_A, SM_Corridor_Ceiling_A, SM_PillarSection_01, `NightCycle.unity` |

Замечание: в `fda220ca` попал `Assets/Visual/Fonts/Alagard(rus)/alagard-12px-unicode SDF.asset` (шрифт TMP, Unity перегенерирует его сам). К чистке он не относится, по решению автора оставлен как есть.

Эффект на shadow pass (повторная диагностика, чтение, `script eval`, 2026-11-15 00:57):

| Показатель | До чистки | После чистки |
|---|--:|--:|
| Активные shadow casters (Mesh/Skinned) | 4343 | 4046 |
| Raw-треугольники кастеров (все LOD-уровни считаются одновременно) | 22,427,053 | 9,188,673 (-59%) |
| LODGroup в сцене | 1284 | 1190 |

Оговорка: «raw» до чистки включал все LOD-уровни каждой группы сразу, поэтому реальный рендеримый объём меньше обеих цифр. Отдельного прогона PerfLogger для чистки нет, её вклад измеряется вместе со следующим шагом (метка `step3_combined`).

## Шаг 3. Бюджет realtime-теней point-светов (сделан, есть регресс в Spawn)

- Решение: динамика, а не статическое отключение. `ShadowLightBudget` (runtime, не привязан к сцене): тени остаются только у N ближайших к камере светов, у остальных `Light.shadows = None`, исходное значение возвращается при выключении компонента.
- Универсальные правила допуска вместо имён объектов: intensity ниже порога, range выше порога, свет дальше `QualitySettings.shadowDistance + range` от камеры. Явный opt-out: тег `IgnoreShadowBudget`.
- Hall `Point Light` (r=66.6) и 28 светов EternalLadder выведены из кандидатов правилами (range > 40; дальше shadowDistance + range), без проверки по именам.
- Коммит `dac38d3a`: `ShadowLightBudget.cs` (namespace `Core`, `Assets/Scripts/World/Lights/`), GameObject `ShadowLightBudget` в `NightCycle.unity`, тег `IgnoreShadowBudget` в `ProjectSettings/TagManager.asset`. Параметры по умолчанию: бюджет 6, intensity >= 3, range <= 40, отсечение по shadowDistance, пересчёт каждые 0.2 с, поиск огней каждые 2 с.
- Из 62 realtime-огней под управление берутся все 62 (тегом пока не исключён ни один), претендовать на слот могут 8–32 огня в зависимости от точки, тени получают 6.

### Замеры

Полные таблицы: `perf_compare_step3_combined.md`. Метки в `perf_log.csv`: `step3_combined` и `step3_combined_r2` (два прогона подряд), `step3_cleanup_only` (то же, но ShadowLightBudget выключен; нужен, чтобы отделить вклад бюджета от ручной чистки).

- Лучше по gpu_ms, draw calls и shadow casters (относительно `cleanup only`): Hall, Bedroom, Reliquary, Laboratory. Самый заметный выигрыш в Reliquary (GPU 8.27 -> 4.66/4.91 мс, draw calls 4555 -> 3259/3234) и Laboratory (5.79 -> 4.00/4.46 мс).
- Office: в пределах шума (GPU 8.60 -> 7.56/9.52 мс).
- **Spawn стал хуже**: shadow casters 2575 -> 4101/4162, draw calls 7145 -> 8360/8393, треугольники 21.9M -> 24.7M/24.9M, GPU 13.35 -> 12.40/13.16 мс (в пределах шума), fps 71.6 -> 69.6/73.6 (шум). Рост casters и draw calls не шум, он повторился в обоих прогонах.
- Гипотеза (не проверена): раньше при переполнении атласа URP сам выбрасывал большую часть shadow map (в Editor.log «URP removed N shadow maps»), в том числе у части огней рядом со Spawn. Бюджет гарантирует тени 6 ближайшим, среди них оба света Bedroom с разрешением 2048 (Point Light (1) r=16.6 и Point Light r=5.0), и их shadow pass добавил casters. Проверить можно, выключив тени у этих двух огней или подняв порог.
- Правило отката из плана («хуже step1 в большинстве точек по gpu_ms и fps_avg») не сработало: хуже только Spawn, коммит `dac38d3a` оставлен.
- Ограничения: Editor Play Mode, один прогон на метку (step3 — два), шум fps до 7 fps и GPU до 2 мс между соседними прогонами.
