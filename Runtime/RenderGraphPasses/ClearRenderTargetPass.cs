using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

namespace CustomRP
{
    /// <summary>
    /// Clear Render Target
    /// </summary>
    public partial class CustomRenderGraphRecord
    {
        private static readonly ProfilingSampler s_ClearRenderTargetProfilingSampler = new ProfilingSampler("Clear Render Target");

        internal class ClearRenderTargetPassData
        {
            internal RTClearFlags clearFlags;
            internal Color clearColor;
        }

        private void AddClearRenderTargetPass(RenderGraph renderGraph, CameraData cameraData)
        {
            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("Clear Render Target Pass", 
                out ClearRenderTargetPassData passData, s_ClearRenderTargetProfilingSampler))
            {
                passData.clearFlags = cameraData.GetRTClearFlags();
                passData.clearColor = cameraData.GetRTClearColor();

                if (m_BackBufferColorTexture.IsValid())
                    builder.SetRenderAttachment(m_BackBufferColorTexture, 0, AccessFlags.Write);
                
                if (m_BackBufferDepthTexture.IsValid())
                    builder.SetRenderAttachmentDepth(m_BackBufferDepthTexture, AccessFlags.Write);

                builder.SetRenderFunc((ClearRenderTargetPassData data, RasterGraphContext context) =>
                {
                    context.cmd.ClearRenderTarget(data.clearFlags, data.clearColor, depth: 1, stencil: 0);
                });

            }
        }
    }
}