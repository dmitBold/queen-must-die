# Scene audit: NightCycle [baseline]

- time: 2026-11-14 20:23:53
- unity: 6000.2.8f1

## Overview

| key | value |
|---|---|
| GameObjects (Transforms, incl. inactive) | 43343 |
| MeshRenderer total | 8945 |
| Renderers active (Mesh + Skinned) | 5873 |
| SkinnedMeshRenderer total | 800 |
| MeshRenderer without mesh | 1 |
| MeshRenderer with >1 material | 2386 |
| Material slots | 13593 |
| Material slots with enableInstancing | 10844 |
| Unique meshes | 1404 |
| Unique mesh base names (no _LODn) | 781 |
| Unique materials (by name) | 238 |
| Unique shaders | 12 |
| Triangles: all MeshRenderers, all LODs, incl. inactive | 63845103 |
| Triangles: active MeshRenderers, all LODs | 31382233 |
| Triangles: active, LOD0 + non-LOD only (upper bound) | 16941821 |
| Triangles: SkinnedMeshRenderer (all) | 1216635 |
| Colliders / MeshColliders | 4536 / 923 |

## LOD coverage

| key | value |
|---|---|
| MeshRenderers inside a LODGroup | 6147 |
| MeshRenderers outside any LODGroup | 2798 |

## Static flags (MeshRenderer only)

| key | value |
|---|---|
| Any static flag | 1616 of 8945 |
| Static share, % | 18,1 |
| Batching Static | 1318 |
| Occluder Static | 1616 |
| Occludee Static | 1616 |
| Contribute GI | 1616 |
| Reflection Probe Static | 1616 |

## Shadow casting (MeshRenderer)

| key | value |
|---|---|
| On | 8929 |
| Two Sided | 0 |
| Shadows Only | 0 |
| Off | 16 |
| Small casters (max bounds extent < 0.25 m, cast shadows) | 2441 |
|   of them active now | 946 |

## Lights

| key | value |
|---|---|
| Light total | 410 |
| Light active | 233 |
| Directional (any) | 0 |
| Realtime active | 78 |
| Realtime active with shadows | 62 |
|   hard / soft | 28 / 34 |
| Mixed active | 1 |
| Baked active | 154 |
| LightmapSettings.lightmaps.Length | 0 |
| LightProbes count | 0 |
| MeshRenderers that have a lightmap index | 0 |
| Baked active lights with NO lightmaps in scene (effectively zero light) | 154 |
| Baked active lights when lightmaps exist (may contribute) | 0 |
| Baked active lights with zero intensity/colour | 0 |
| Realtime light budget note | URP additional lights per object limit = see Settings section |

## LOD groups

| key | value |
|---|---|
| LODGroup total | 1703 |
| LODGroup active in hierarchy | 1284 |
| Last LOD threshold > 2% screen height (object not culled until large) | 529 |
| Last LOD threshold ~0 (never culled by LOD) | 1 |
| Last LOD has no renderers (= culled state) | 0 |
| QualitySettings.lodBias | 2 |
| QualitySettings.maximumLODLevel | 0 |

## Occlusion culling

| key | value |
|---|---|
| Camera 'Main Camera' | useOcclusionCulling=True far=5000 near=0,1 enabled=True |
| Baked Umbra data size (bytes) | 0 |
| OcclusionArea | 0 |
| OcclusionPortal | 0 |

## UI / VFX / misc

| key | value |
|---|---|
| Canvas total / active | 4 / 4 |
| ParticleSystem total | 205 |
| ParticleSystem on active GameObjects | 145 |
|   playing right now (Edit Mode: usually 0) | 0 |
|   looping / playOnAwake (active ones) | 139 / 145 |
|   sum of maxParticles (active ones) | 133830 |
| ReflectionProbe | 0 |
| LightProbeGroup | 5 |
| Camera | 1 |
| Application.isPlaying | False |

## Project / render settings snapshot

| key | value |
|---|---|
| Scripting backend (Standalone) | Mono2x |
| Static Batching (Standalone Win64) | True |
| Dynamic Batching (Standalone Win64) | False |
| Graphics Jobs (PlayerSettings.graphicsJobs) | True |
| Multithreaded Rendering | True |
| GPU Skinning | True |
| Color space | Linear |
| Use Incremental GC | True |
| Auto graphics API (Win) | True |
| Graphics APIs (Win) | Direct3D12, Direct3D11 |
| Quality level | 1 = PC |
| lodBias | 2 |
| pixelLightCount | 2 |
| shadowDistance (Quality) | 40 |
| vSyncCount | 0 |
| Application.targetFrameRate | -1 |
| Anti-aliasing (Quality) | 0 |
| Texture mip limit | 0 |
| Anisotropic | ForceEnable |
| Active RP asset | PC_RPAsset (Assets/Settings/URP/PC_RPAsset.asset) |
| URP m_SupportsHDR | True |
| URP m_MSAA | Disabled |
| URP m_RenderScale | 0,7 |
| URP m_UseSRPBatcher | True |
| URP m_SupportsDynamicBatching | False |
| URP m_MainLightRenderingMode | PerPixel |
| URP m_MainLightShadowsSupported | True |
| URP m_MainLightShadowmapResolution | _2048 |
| URP m_AdditionalLightsRenderingMode | PerPixel |
| URP m_AdditionalLightsPerObjectLimit | 4 |
| URP m_AdditionalLightShadowsSupported | True |
| URP m_AdditionalLightsShadowmapResolution | _2048 |
| URP m_ShadowDistance | 45 |
| URP m_ShadowCascadeCount | 4 |
| URP m_SoftShadowsSupported | True |
| URP m_SupportsLightLayers | True |
| URP m_ReflectionProbeBlending | True |
| URP m_GPUResidentDrawerMode | Disabled |
| URP m_GPUResidentDrawerEnableOcclusionCullingInCameras | False |
| URP m_LODCrossFadeDitheringType | BlueNoise |
| URP m_UseAdaptivePerformance | True |
| Renderer[0] | PC_Renderer (default) |
|   m_RenderingMode | ForwardPlus |
|   m_DepthPrimingMode | Disabled |
|   m_CopyDepthMode | AfterOpaques |
|   m_IntermediateTextureMode | Auto |
|   m_UseNativeRenderPass | True |
|   feature[0] ScreenSpaceAmbientOcclusion | active=True |
|   feature[1] FullScreenPassRendererFeature | active=True |
|   feature[2] DecalRendererFeature | active=True |
| Renderer[1] | UI_Renderer_Data |
|   m_RenderingMode | Forward |
|   m_DepthPrimingMode | Disabled |
|   m_CopyDepthMode | AfterTransparents |
|   m_IntermediateTextureMode | Always |
|   m_UseNativeRenderPass | False |

## Top 15 meshes by instance count

| key | count | extra |
|---|---:|---|
| SM_Wall_C_LOD2 | 195 |  |
| SM_Wall_C_LOD0 | 160 |  |
| SM_Wall_C_LOD1 | 160 |  |
| SM_MDoorA_LOD0 | 133 |  |
| SM_MDoorA_LOD1 | 133 |  |
| SM_Wall_01_Chamber_Var_01_LOD0 | 119 |  |
| SM_Wall_800x800_LOD3 | 96 |  |
| SM_Prop_Candle_B_LOD0 | 88 |  |
| SM_Wall_400_LOD2 | 84 |  |
| SM_Floor_A | 82 |  |
| SM_Pillar_I_LOD3 | 76 |  |
| SM_Prop_Candle_B_LOD2 | 70 |  |
| SM_Floor_Crypt_Var_02_LOD5 | 70 |  |
| SM_Prop_Candle_B_LOD3 | 68 |  |
| SM_Prop_Candle_B_LOD1 | 68 |  |

## Top 15 mesh base names (LOD suffix stripped)

| key | count | extra |
|---|---:|---|
| SM_Wall_C | 515 |  |
| SM_Prop_Candle_B | 294 |  |
| SM_MDoorA | 266 |  |
| SM_Pilaster_Brick_6m_C | 226 |  |
| SM_Prop_Candle_A | 204 |  |
| SM_PillarSection_01 | 192 |  |
| SM_Wall_800x800 | 180 |  |
| SM_Balustrade_handrails_C | 166 |  |
| SM_Roof_Arc_Crypt_Var_02 | 160 |  |
| SM_Prop_Candle_C | 132 |  |
| SM_Chain_A | 122 |  |
| SM_Column_Crypt_Var_02 | 120 |  |
| SM_Debris_02 | 120 |  |
| SM_Wall_01_Chamber_Var_01 | 119 |  |
| SM_Prop_CandleStand_E | 117 |  |

## Top 15 meshes by total triangles (tris x instances)

| key | count | extra |
|---|---:|---|
| SM_Chapel_Candelabrum_SM_Chapel_Candelabrum_Base_LOD0 | 13166384 |  |
| SM_Chapel_Candelabrum_SM_Chapel_Candelabrum_Base_LOD1 | 6583192 |  |
| SM_Chapel_Candelabrum_SM_Chapel_Candelabrum_Base_LOD2 | 3291596 |  |
| output_unwrapped | 2564760 |  |
| SM_Chapel_BonesAndSkillPile_Var3_SM_Chapel_BonesAndSkillPile_Var3_LOD0 | 1873152 |  |
| SM_Chapel_Candelabrum_SM_Chapel_Candelabrum_Base_LOD3 | 1645798 |  |
| SM_Chapel_BonesAndSkillPile_Var4_SM_Chapel_BonesAndSkillPile_Var4_LOD0 | 1353600 |  |
| SM_Prop_Candle_B_LOD0 | 1351416 |  |
| SM_Prop_Bookset_B_LOD0 | 942592 |  |
| SM_Chapel_BonesAndSkillPile_Var3_SM_Chapel_BonesAndSkillPile_Var3_LOD1 | 936576 |  |
| SM_Prop_Bookset_C_LOD0 | 888550 |  |
| SM_Chain_A_LOD0 | 877344 |  |
| SM_Prop_Bookset_A_LOD0 | 830760 |  |
| SM_Chapel_Candelabrum_SM_Chapel_Candelabrum_Base_LOD4 | 822892 |  |
| SM_PillarSection_01_LOD0 | 741936 |  |

## Top 15 materials by slot count

| key | count | extra |
|---|---:|---|
| M_Trim_A | 1065 | Shader Graphs/S_BasicTextured \| inst \| q2225 |
| M_Wall_A | 981 | Shader Graphs/S_BasicTextured \| inst \| q2225 |
| Lit | 936 | Universal Render Pipeline/Lit \| q2000 |
| M_Prop_Book_A | 628 | Shader Graphs/S_BasicTextured \| inst \| q2225 |
| M_BrickStoneWall_Disp | 589 | Shader Graphs/S_MasterBlend \| inst \| q2225 |
| M_Prop_Candle_A | 490 | Shader Graphs/S_BasicTextured \| inst \| q2225 |
| M_Bricks_A | 472 | Shader Graphs/S_General \| inst \| q2225 |
| M_Trim_B | 455 | Shader Graphs/S_BasicTextured \| inst \| q2225 |
| Golem_New | 381 | Universal Render Pipeline/Lit \| q2000 |
| M_Balustrade | 373 | Shader Graphs/S_General \| inst \| q2225 |
| M_Pillar_01 | 366 | Shader Graphs/S_Blend \| inst \| q2225 |
| M_Wall_B | 364 | Shader Graphs/S_BasicTextured \| inst \| q2225 |
| Marble | 303 | Universal Render Pipeline/Lit \| q2000 |
| M_StoneDamage | 265 | Shader Graphs/S_General \| inst \| q2225 |
| M_Stone_Floor_Tile_Disp | 255 | Shader Graphs/S_MasterBlend \| inst \| q2225 |

## Top 15 mesh+material pairs (merge/instancing candidates)

| key | count | extra |
|---|---:|---|
| SM_Wall_C_LOD0 + M_Wall_A | 200 |  |
| SM_Wall_C_LOD2 + M_Wall_A | 195 |  |
| SM_Wall_C_LOD2 + M_Trim_A | 195 |  |
| SM_Wall_C_LOD1 + M_Trim_A | 160 |  |
| SM_Wall_C_LOD1 + M_Wall_A | 160 |  |
| SM_MDoorA_LOD1 + M_MDoorA | 133 |  |
| SM_Wall_C_LOD0 + M_Trim_A | 120 |  |
| SM_Wall_01_Chamber_Var_01_LOD0 + M_WA_WallChamber | 119 |  |
| SM_Wall_400_LOD2 + M_Stone_Floor_Tile_Disp | 84 |  |
| SM_Floor_A + M_Marblefloor_D | 82 |  |
| SM_MDoorA_LOD0 + M_MDoorA | 77 |  |
| SM_Pillar_I_LOD3 + M_Wall_B | 76 |  |
| SM_Pillar_I_LOD3 + M_Trim_A | 76 |  |
| SM_Pillar_I_LOD3 + M_Wall_A | 76 |  |
| SM_Pillar_I_LOD3 + M_Trim_B | 76 |  |

## Shaders by slot count

| key | count | extra |
|---|---:|---|
| Shader Graphs/S_BasicTextured | 5845 |  |
| Universal Render Pipeline/Lit | 2714 |  |
| Shader Graphs/S_General | 2263 |  |
| Shader Graphs/S_Blend | 999 |  |
| Shader Graphs/S_MasterBlend | 898 |  |
| Shader Graphs/S_Brick | 414 |  |
| Shader Graphs/S_WorldAligned | 409 |  |
| Shader Graphs/S_LitORM | 23 |  |
| ProBuilder6/Standard Vertex Color | 11 |  |
| Shader Graphs/VolumetricSpotlight | 11 |  |
| Shader Graphs/S_Foliage | 5 |  |
| QMD/BalustradeDissolve | 1 |  |

## Top 20 mesh base names OUTSIDE any LODGroup

| key | count | extra |
|---|---:|---|
| SM_Wall_01_Chamber_Var_01 | 119 |  |
| SM_Wall_400 | 84 |  |
| SM_Floor_A | 82 |  |
| SM_Floor_Crypt_Var_02 | 76 |  |
| SM_Pillar_I | 76 |  |
| SM_Wall_800x800 | 72 |  |
| SM_Wall_F | 59 |  |
| SM_Wall_Crypt_Var_01 | 57 |  |
| SM_WallEnd_BrickA_4m_B | 38 |  |
| SM_Wall_C | 37 |  |
| Plane | 35 |  |
| lid | 34 |  |
| output_unwrapped | 33 |  |
| Paper_01 | 32 |  |
| Torch_01 | 26 |  |
| SM_ArchStone_A_ArchStone_01 | 23 |  |
| Sword | 19 |  |
| SM_Wall_Chamber_Var_03 | 19 |  |
| SM_Halberd | 19 |  |
| SM_Pilaster_Brick_6m_C | 16 |  |

## Renderer kind (name heuristic): total, extra = static count

| key | count | extra |
|---|---:|---|
| other | 3449 | static=304 casting=3433 |
| wall | 1694 | static=577 casting=1694 |
| candle/torch | 931 | static=0 casting=931 |
| book | 690 | static=0 casting=690 |
| pillar | 557 | static=164 casting=557 |
| door | 489 | static=266 casting=489 |
| ceiling | 349 | static=141 casting=349 |
| floor | 311 | static=139 casting=311 |
| prop | 243 | static=0 casting=243 |
| trim | 137 | static=25 casting=137 |
| paper | 72 | static=0 casting=72 |
| statue | 23 | static=0 casting=23 |

## Small shadow casters by kind

| key | count | extra |
|---|---:|---|
| other | 1381 |  |
| candle/torch | 652 |  |
| book | 375 |  |
| paper | 20 |  |
| prop | 8 |  |
| floor | 5 |  |

## MeshRenderers by layer

| key | count | extra |
|---|---:|---|
| Default (0) | 7884 |  |
| Environment (8) | 1009 |  |
| DoorLayer (7) | 52 |  |

## Lights by type / bake mode / shadows / active

| key | count | extra |
|---|---:|---|
| Point / Baked / shadows=None / inactive | 106 |  |
| Point / Baked / shadows=Soft / ACTIVE | 60 |  |
| Point / Baked / shadows=None / ACTIVE | 56 |  |
| Point / Baked / shadows=Soft / inactive | 52 |  |
| Point / Realtime / shadows=Soft / ACTIVE | 34 |  |
| Point / Realtime / shadows=Hard / ACTIVE | 28 |  |
| Spot / Baked / shadows=Soft / ACTIVE | 15 |  |
| Point / Realtime / shadows=None / ACTIVE | 15 |  |
| Point / Baked / shadows=Hard / ACTIVE | 9 |  |
| Spot / Baked / shadows=Hard / ACTIVE | 7 |  |
| Spot / Baked / shadows=None / ACTIVE | 7 |  |
| Point / Realtime / shadows=None / inactive | 7 |  |
| Point / Mixed / shadows=None / inactive | 4 |  |
| Point / Realtime / shadows=Hard / inactive | 2 |  |
| Point / Realtime / shadows=Soft / inactive | 2 |  |
| Spot / Baked / shadows=Hard / inactive | 2 |  |
| Spot / Baked / shadows=None / inactive | 2 |  |
| Point / Mixed / shadows=None / ACTIVE | 1 |  |
| Spot / Realtime / shadows=None / ACTIVE | 1 |  |

## Active realtime lights by room (depth 2)

| key | count | extra |
|---|---:|---|
| EternalLadder/Eternal ladder | 28 | withShadows=28 |
| Palace/Hall | 27 | withShadows=20 |
| Palace/Office | 8 | withShadows=4 |
| Palace/Library Teleport | 3 | withShadows=3 |
| Palace/Bedroom | 2 | withShadows=2 |
| Palace/Reliquary | 2 | withShadows=1 |
| Notes/Dead Guard_ Note | 2 | withShadows=0 |
| Point Light | 1 | withShadows=1 |
| Point Light (3) | 1 | withShadows=1 |
| Point Light (1) | 1 | withShadows=1 |
| Point Light (5) | 1 | withShadows=1 |
| Point Light (4) | 1 | withShadows=0 |
| Golem/GolemParts | 1 | withShadows=0 |

## Active BAKED lights by room (depth 2)

| key | count | extra |
|---|---:|---|
| Palace/Office | 47 |  |
| Palace/Hall | 34 |  |
| Palace/Bedroom | 25 |  |
| Palace/Laboratory | 15 |  |
| Palace/Reliquary | 11 |  |
| Notes/Dead Guard_ Note | 8 |  |
| PUZZLES/StatuePuzzle | 4 |  |
| Notes/Archive_Note 3 (1) | 3 |  |
| PUZZLES/PedestalPuzzle | 3 |  |
| Golem/GolemParts | 2 |  |
| Loot/Reagent | 1 |  |
| Notes/Lab_Note | 1 |  |

## LODGroup by level count

| key | count | extra |
|---|---:|---|
| 4 levels | 776 |  |
| 2 levels | 318 |  |
| 3 levels | 271 |  |
| 5 levels | 227 |  |
| 6 levels | 111 |  |

## LODGroup fade mode

| key | count | extra |
|---|---:|---|
| None / animate=False | 1703 |  |

## LOD0 -> LOD1 threshold (screen height)

| key | count | extra |
|---|---:|---|
| >=40% | 1125 |  |
| 5-15% | 345 |  |
| 15-40% | 124 |  |
| 2-5% | 109 |  |

## Last LOD threshold (screen height)

| key | count | extra |
|---|---:|---|
| 0.5-1% | 713 |  |
| 1-2% | 453 |  |
| 2-5% | 437 |  |
| 5-15% | 98 |  |
| 15-40% | 1 |  |
| 0 | 1 |  |

## Hierarchy: root objects (extra = active | descendants | renderers | tris(all LOD) | lights | LODGroups)

| key | count | extra |
|---|---:|---|
| Palace | 23754 | ON \| 23754 \| 7027 \| 61289798 \| 286 \| 1401 |
| Day/Night Connection | 6332 | ON \| 6332 \| 368 \| 430478 \| 50 \| 5 |
| DeathCutscenes | 4267 | ON \| 4267 \| 810 \| 74217 \| 3 \| 0 |
| Knight_Statues | 2080 | ON \| 2080 \| 26 \| 72134 \| 0 \| 0 |
| Main scripts | 1756 | ON \| 1756 \| 231 \| 208327 \| 11 \| 3 |
| Statues_after_Golem | 1750 | OFF \| 1750 \| 9 \| 23910 \| 0 \| 0 |
| EternalLadder | 1198 | ON \| 1198 \| 831 \| 1516690 \| 30 \| 232 |
| Golem | 668 | ON \| 668 \| 205 \| 114748 \| 3 \| 3 |
| Start events | 410 | ON \| 410 \| 167 \| 598884 \| 0 \| 31 |
| OLD_Statues | 401 | OFF \| 401 \| 22 \| 64372 \| 0 \| 0 |
| PUZZLES | 256 | ON \| 256 \| 141 \| 566904 \| 7 \| 26 |
| Eyes | 212 | ON \| 212 \| 68 \| 54264 \| 0 \| 0 |
| Dead Guard Library | 155 | ON \| 155 \| 25 \| 33022 \| 0 \| 1 |
| Notes | 31 | ON \| 31 \| 14 \| 11332 \| 14 \| 1 |
| Decal2 | 17 | ON \| 17 \| 0 \| 0 \| 0 \| 0 |
| Loot | 7 | ON \| 7 \| 4 \| 2646 \| 1 \| 0 |
| TeleportTriggers | 7 | ON \| 7 \| 0 \| 0 \| 0 \| 0 |
| GlobalVolume | 5 | ON \| 5 \| 0 \| 0 \| 0 \| 0 |
| Quests | 5 | ON \| 5 \| 0 \| 0 \| 0 \| 0 |
| CheckPoints | 4 | ON \| 4 \| 0 \| 0 \| 0 \| 0 |
| TutorialHints | 4 | ON \| 4 \| 0 \| 0 \| 0 \| 0 |
| LevelLoader_Library | 3 | ON \| 3 \| 0 \| 0 \| 0 \| 0 |
| SavedEvents | 3 | ON \| 3 \| 0 \| 0 \| 0 \| 0 |
| ESSENCE | 2 | ON \| 2 \| 1 \| 0 \| 0 \| 0 |
| Завал для уборки | 2 | ON \| 2 \| 1 \| 12 \| 0 \| 0 |
| --Sound-- | 2 | ON \| 2 \| 0 \| 0 \| 0 \| 0 |
| LadderTrigger | 2 | OFF \| 2 \| 0 \| 0 \| 0 \| 0 |
| Point Light | 1 | ON \| 1 \| 0 \| 0 \| 1 \| 0 |
| Point Light (1) | 1 | ON \| 1 \| 0 \| 0 \| 1 \| 0 |
| Point Light (3) | 1 | ON \| 1 \| 0 \| 0 \| 1 \| 0 |
| Point Light (5) | 1 | ON \| 1 \| 0 \| 0 \| 1 \| 0 |
| Point Light (4) | 1 | ON \| 1 \| 0 \| 0 \| 1 \| 0 |
| LadderTrigger_Reliq | 1 | OFF \| 1 \| 0 \| 0 \| 0 \| 0 |
| BloodPuddle | 1 | ON \| 1 \| 0 \| 0 \| 0 \| 0 |
| BloodPuddle (1) | 1 | ON \| 1 \| 0 \| 0 \| 0 \| 0 |
| LockDoorQuest | 1 | ON \| 1 \| 0 \| 0 \| 0 \| 0 |
| Player Spawn Point | 1 | ON \| 1 \| 0 \| 0 \| 0 \| 0 |

## Hierarchy: children of big roots (extra = active | descendants | renderers | tris | lights | LODGroups)

| key | count | extra |
|---|---:|---|
| Palace/OLD Arms Corridor | 10760 | OFF \| 10760 \| 399 \| 514472 \| 55 \| 0 |
| Day/Night Connection/Office_statues | 5396 | OFF \| 5396 \| 74 \| 185232 \| 50 \| 1 |
| Palace/Office | 4591 | ON \| 4591 \| 2411 \| 16976291 \| 66 \| 508 |
| Palace/Hall | 2619 | ON \| 2619 \| 1016 \| 2634473 \| 63 \| 123 |
| Palace/Laboratory | 1919 | ON \| 1919 \| 1156 \| 5445468 \| 49 \| 280 |
| Main scripts/Media_Shit | 1729 | OFF \| 1729 \| 231 \| 208327 \| 11 \| 3 |
| DeathCutscenes/CS_Reliq_DEATH | 1420 | OFF \| 1420 \| 270 \| 24739 \| 1 \| 0 |
| DeathCutscenes/CS_Pillar_DEATH | 1420 | OFF \| 1420 \| 270 \| 24739 \| 1 \| 0 |
| DeathCutscenes/CS_Office_DEATH | 1420 | OFF \| 1420 \| 270 \| 24739 \| 1 \| 0 |
| Knight_Statues/GEAR_STATUE | 1276 | ON \| 1276 \| 10 \| 31868 \| 0 \| 0 |
| EternalLadder/Eternal ladder | 1194 | ON \| 1194 \| 830 \| 1516678 \| 30 \| 232 |
| Palace/Archive  | 1163 | OFF \| 1163 \| 547 \| 24023114 \| 2 \| 103 |
| Palace/Bedroom | 962 | ON \| 962 \| 611 \| 5614181 \| 27 \| 127 |
| Day/Night Connection/DeadGolems | 914 | OFF \| 914 \| 277 \| 232950 \| 0 \| 4 |
| Palace/Reliquary | 782 | ON \| 782 \| 435 \| 2792847 \| 13 \| 109 |
| Palace/New arms corridor | 614 | OFF \| 614 \| 331 \| 3256124 \| 0 \| 149 |
| Knight_Statues/RELIQ_STATUE | 587 | ON \| 587 \| 3 \| 7970 \| 0 \| 0 |
| Statues_after_Golem/ActiveStatue3 | 583 | ON \| 583 \| 3 \| 7970 \| 0 \| 0 |
| Statues_after_Golem/ActiveStatue2 | 583 | ON \| 583 \| 3 \| 7970 \| 0 \| 0 |
| Statues_after_Golem/ActiveStatue1 | 583 | ON \| 583 \| 3 \| 7970 \| 0 \| 0 |
| Golem/Golem with door | 497 | ON \| 497 \| 151 \| 82846 \| 0 \| 3 |
| Palace/Reliq/Bedroom secret path | 260 | OFF \| 260 \| 101 \| 9188 \| 8 \| 0 |
| Golem/GOLEM_RIGGED_BROKEN | 157 | ON \| 157 \| 49 \| 26064 \| 0 \| 0 |
| Knight_Statues/CEIL_STATUE | 105 | ON \| 105 \| 6 \| 16148 \| 0 \| 0 |
| Knight_Statues/Reliq.statue copy2 | 53 | ON \| 53 \| 3 \| 8074 \| 0 \| 0 |
| Knight_Statues/Harakiri statue | 53 | ON \| 53 \| 4 \| 8074 \| 0 \| 0 |
| Palace/BloodTrail | 44 | ON \| 44 \| 0 \| 0 \| 0 \| 0 |
| Palace/Library Teleport | 39 | ON \| 39 \| 20 \| 23640 \| 3 \| 2 |
| Day/Night Connection/Toy | 21 | OFF \| 21 \| 17 \| 12296 \| 0 \| 0 |

## Canvas by render mode

| key | count | extra |
|---|---:|---|
| ScreenSpaceOverlay / ACTIVE | 4 |  |

