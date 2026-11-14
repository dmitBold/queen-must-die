# История оптимизации QMDA (сцена NightCycle)

Unity 6000.2.8f1, URP 17.2.0 Forward+. Все замеры: `AuditReports/perf_log.csv`, 6 точек из `viewpoints.json`, Editor Play Mode (цифры индикативные, сравнивать только между собой).

## Исходное состояние (Этап 1, разведка)

- 5 крупных комнат активны одновременно (Office, Laboratory, Hall, Bedroom, Reliquary), ~5702 активных MeshRenderer, 31.4M треугольников.
- 62 активных realtime Point Light с тенями, 8929 renderer с включённым Cast Shadows.
- 154 Baked-света не работают (Lightmaps = 0), Occlusion Culling не запечён, lodBias = 2 (Quality `PC`).
- Baseline: `perf_baseline_20261114.md`, метка `baseline` в `perf_log.csv`.
- Точка Laboratory в `viewpoints.json` была за геометрией комнаты (пустой скайбокс), переставлена на (30.70, 7.79, 32.20), euler (15, 315, 0). Резерв: `viewpoints.backup_before_lab_fix.json`.

## Шаг 1. Cast Shadows = Off у мелкого декора

- Коммит `9d69a018`. Скрипт `Assets/Audit/Editor/ShadowStep1.cs`, список объектов `step1_shadow_off_list.csv`.
- Изменено 1518 из 5691 активных MeshRenderer: всё меньше 0.6 м и объекты до 4 м с именем Candle / Torch / Book / Paper. SkinnedMeshRenderer, URP-ассет, lodBias и Static-флаги не трогались.
- Метка замера: `step1_shadows_small` (`perf_compare_step1_shadows_small.md`).
- Результат: заметный выигрыш только в Office (fps 73.2 -> 89.9, GPU 12.7 -> 8.6 мс, треугольники 24.9M -> 11.2M). В остальных точках изменения в пределах шума одного прогона.

## Ручная чистка тяжёлых ассетов (сделана руками, без PerfLogger)

Основание: диагностика shadow pass (только чтение). Один Candelabrum_Base LOD0 давал 1.88M треугольников и был активен у Bedroom и Spawn, BonesAndSkillPile_Var3 LOD0 давал 1.87M. Внутри LODGroup было 21.2M из 22.4M raw-треугольников кастеров, у 96.9% LOD0 > 2000.

Что сделано: в префабах и в сцене (там, где префабы распакованы) удалён компонент LOD Group, оставлен один самый экономный LOD (LOD5).

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

## Шаг 3. Бюджет realtime-теней point-светов (в работе)

- Решение: динамика, а не статическое отключение. `ShadowLightBudget` (runtime, не привязан к сцене): тени остаются только у N ближайших к камере светов, у остальных `Light.shadows = None`, исходное значение возвращается при выключении компонента.
- Универсальные правила допуска вместо имён объектов: intensity ниже порога, range выше порога, свет дальше `QualitySettings.shadowDistance + range` от камеры. Явный opt-out: тег `IgnoreShadowBudget`.
- Hall `Point Light` (r=66.6) и 28 светов EternalLadder выведены из кандидатов правилами (range > 40; дальше shadowDistance + range), без проверки по именам.
- Итоговая запись (коммит, метка `step3_combined`, таблица baseline -> step1 -> step3_combined) будет добавлена после замера.
