#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace CustomRP
{
    /// <summary>
    /// Draw Gizmos
    /// </summary>
    public partial class CustomRenderGraphRecord
    {
        private static readonly ProfilingSampler s_DrawEditorGizmosProfilingSampler = new ProfilingSampler("Draw Gizmos");

        internal class DrawEditorGizmosPassData
        {
            internal RendererListHandle gizmosRendererListHandle;
        }

        private void AddDrawEditorGizmosPass(RenderGraph renderGraph, CameraData cameraData, GizmoSubset gizmoSubset)
        {
            if(!Handles.ShouldRenderGizmos() || cameraData.camera.sceneViewFilterMode == Camera.SceneViewFilterMode.ShowFiltered)
                return;
            bool renderPreGizmos = gizmoSubset == GizmoSubset.PreImageEffects;
            string passName = renderPreGizmos ? "Draw Pre Gizmos Pass" : "Draw Post Gizmos Pass";
            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(passName, 
                out DrawEditorGizmosPassData passData, s_DrawEditorGizmosProfilingSampler))
            {
                passData.gizmosRendererListHandle = renderGraph.CreateGizmoRendererList(in cameraData.camera, in gizmoSubset);

                builder.UseRendererList(passData.gizmosRendererListHandle);
                builder.AllowPassCulling(false);

                // Set Render Target
                if (m_BackBufferColorTexture.IsValid())
                    builder.SetRenderAttachment(m_BackBufferColorTexture, 0, AccessFlags.Write);
                
                if (m_BackBufferDepthTexture.IsValid())
                    builder.SetRenderAttachmentDepth(m_BackBufferDepthTexture, AccessFlags.Read);

                builder.SetRenderFunc((DrawEditorGizmosPassData data, RasterGraphContext context) =>
                {
                    context.cmd.DrawRendererList(data.gizmosRendererListHandle);
                });

            }
        }
    }
}
#endif