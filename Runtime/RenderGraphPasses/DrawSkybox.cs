using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace CustomRP
{
    /// <summary>
    /// Draw Skybox
    /// </summary>
    public partial class CustomRenderGraphRecord
    {
        private static readonly ProfilingSampler s_DrawSkyboxProfilingSampler = new ProfilingSampler("Draw Skybox");

        internal class DrawSkyboxPassData
        {
            internal RendererListHandle skyboxRendererListHandle;
        }

        private void AddDrawSkyboxPass(RenderGraph renderGraph, CameraData cameraData)
        {
            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("Draw Skybox Pass", 
                out DrawSkyboxPassData passData, s_DrawSkyboxProfilingSampler))
            {
                passData.skyboxRendererListHandle = renderGraph.CreateSkyboxRendererList(cameraData.camera);

                // ������Ⱦ��Ŀ��
                if (m_BackBufferColorTexture.IsValid())
                    builder.SetRenderAttachment(m_BackBufferColorTexture, 0, AccessFlags.Write);
                if (m_BackBufferDepthTexture.IsValid())
                    builder.SetRenderAttachmentDepth(m_BackBufferDepthTexture, AccessFlags.Write);
                
                builder.UseRendererList(passData.skyboxRendererListHandle);
                
                // ������Ⱦȫ��״̬
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((DrawSkyboxPassData data, RasterGraphContext context) =>
                {
                    context.cmd.DrawRendererList(data.skyboxRendererListHandle);
                });

            }
        }
    }
}