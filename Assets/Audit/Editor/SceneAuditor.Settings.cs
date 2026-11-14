using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

namespace QMDA.Audit
{
    public static partial class SceneAuditor
    {
        static void SettingsSection(AuditReport rep)
        {
            var s = rep.Sec("Project / render settings snapshot");

            // Player
            s.Add("Scripting backend (Standalone)", PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone));
            // GetBatchingForPlatform is internal in PlayerSettings, so read it by reflection.
            var gb = typeof(PlayerSettings).GetMethod("GetBatchingForPlatform",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (gb != null)
            {
                var args = new object[] { BuildTarget.StandaloneWindows64, 0, 0 };
                gb.Invoke(null, args);
                s.Add("Static Batching (Standalone Win64)", (int)args[1] == 1);
                s.Add("Dynamic Batching (Standalone Win64)", (int)args[2] == 1);
            }
            else s.Add("Static/Dynamic Batching", "API not found (see ProjectSettings.asset m_BuildTargetBatching)");
            s.Add("Graphics Jobs (PlayerSettings.graphicsJobs)", PlayerSettings.graphicsJobs);
            s.Add("Multithreaded Rendering", PlayerSettings.MTRendering);
            s.Add("GPU Skinning", PlayerSettings.gpuSkinning);
            s.Add("Color space", PlayerSettings.colorSpace);
            s.Add("Use Incremental GC", PlayerSettings.gcIncremental);
            s.Add("Auto graphics API (Win)", PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64));
            var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
            s.Add("Graphics APIs (Win)", string.Join(", ", apis));

            // Quality
            int q = QualitySettings.GetQualityLevel();
            s.Add("Quality level", q + " = " + QualitySettings.names[q]);
            s.Add("lodBias", QualitySettings.lodBias);
            s.Add("pixelLightCount", QualitySettings.pixelLightCount);
            s.Add("shadowDistance (Quality)", QualitySettings.shadowDistance);
            s.Add("vSyncCount", QualitySettings.vSyncCount);
            s.Add("Application.targetFrameRate", Application.targetFrameRate);
            s.Add("Anti-aliasing (Quality)", QualitySettings.antiAliasing);
            s.Add("Texture mip limit", QualitySettings.globalTextureMipmapLimit);
            s.Add("Anisotropic", QualitySettings.anisotropicFiltering);

            // URP asset (read through SerializedObject to avoid an assembly reference to URP)
            var rp = QualitySettings.renderPipeline != null ? QualitySettings.renderPipeline : GraphicsSettings.defaultRenderPipeline;
            s.Add("Active RP asset", rp != null ? rp.name + " (" + AssetDatabase.GetAssetPath(rp) + ")" : "null (Built-in)");
            if (rp == null) return;

            var so = new SerializedObject(rp);
            string[] props =
            {
                "m_SupportsHDR", "m_MSAA", "m_RenderScale", "m_UseSRPBatcher", "m_SupportsDynamicBatching",
                "m_MainLightRenderingMode", "m_MainLightShadowsSupported", "m_MainLightShadowmapResolution",
                "m_AdditionalLightsRenderingMode", "m_AdditionalLightsPerObjectLimit", "m_AdditionalLightShadowsSupported",
                "m_AdditionalLightsShadowmapResolution", "m_ShadowDistance", "m_ShadowCascadeCount", "m_SoftShadowsSupported",
                "m_SupportsLightLayers", "m_ReflectionProbeBlending", "m_GPUResidentDrawerMode",
                "m_GPUResidentDrawerEnableOcclusionCullingInCameras", "m_LODCrossFadeDitheringType", "m_UseAdaptivePerformance"
            };
            foreach (var p in props)
            {
                var sp = so.FindProperty(p);
                if (sp == null) continue;
                s.Add("URP " + p, Read(sp));
            }

            // Renderer data(s) and their features: is SSAO / Decal really enabled?
            var list = so.FindProperty("m_RendererDataList");
            if (list == null) return;
            for (int i = 0; i < list.arraySize; i++)
            {
                var rd = list.GetArrayElementAtIndex(i).objectReferenceValue;
                if (rd == null) continue;
                var rso = new SerializedObject(rd);
                s.Add("Renderer[" + i + "]", rd.name + (i == so.FindProperty("m_DefaultRendererIndex")?.intValue ? " (default)" : ""));
                foreach (var p in new[] { "m_RenderingMode", "m_DepthPrimingMode", "m_CopyDepthMode", "m_IntermediateTextureMode", "m_UseNativeRenderPass" })
                {
                    var sp = rso.FindProperty(p);
                    if (sp != null) s.Add("  " + p, Read(sp));
                }
                var feats = rso.FindProperty("m_RendererFeatures");
                if (feats == null) continue;
                for (int f = 0; f < feats.arraySize; f++)
                {
                    var fo = feats.GetArrayElementAtIndex(f).objectReferenceValue;
                    if (fo == null) { s.Add("  feature[" + f + "]", "MISSING"); continue; }
                    var fso = new SerializedObject(fo);
                    var active = fso.FindProperty("m_Active");
                    s.Add("  feature[" + f + "] " + fo.GetType().Name, "active=" + (active != null && active.boolValue));
                }
            }
        }

        static string Read(SerializedProperty sp)
        {
            switch (sp.propertyType)
            {
                case SerializedPropertyType.Boolean: return sp.boolValue.ToString();
                case SerializedPropertyType.Float: return sp.floatValue.ToString("0.###");
                case SerializedPropertyType.Enum: return sp.enumValueIndex >= 0 && sp.enumValueIndex < sp.enumNames.Length ? sp.enumNames[sp.enumValueIndex] : sp.intValue.ToString();
                default: return sp.intValue.ToString();
            }
        }
    }
}
