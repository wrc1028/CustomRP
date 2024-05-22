using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

namespace CustomRP
{
    /// <summary>
    /// Draw Objects
    /// </summary>
    public partial class CustomRenderGraphRecord
    {
        private static readonly ProfilingSampler s_DrawOpaqueProfilingSampler = new ProfilingSampler("Draw Opaque");
        private static readonly ProfilingSampler s_DrawTransparentProfilingSampler = new ProfilingSampler("Draw Transparent");
        private static ShaderTagId s_DefaultShaderTagIdList = new ShaderTagId("SRPDefaultUnlit");
        internal class DrawObjectsPassData
        {
            internal RendererListHandle opaqueRendererListHandle;
            internal RendererListHandle transparentRendererListHandle;
            internal TextureHandle backBufferHandle;
        }

        private void AddDrawOpaquePass(RenderGraph renderGraph, CameraData cameraData)
        {
            // ʹ�ù�դ����RenderPass��������
            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("Draw Opaque Pass", 
                out DrawObjectsPassData passData, s_DrawOpaqueProfilingSampler))
            {
                // �������������õ���Դ, ��ȡ��Ⱦ����Դ�б�
                RendererListDesc opaqueRendererListDesc = new RendererListDesc(s_DefaultShaderTagIdList, cameraData.cullingResults, cameraData.camera)
                {
                    sortingCriteria = SortingCriteria.CommonOpaque,
                    renderQueueRange = RenderQueueRange.opaque,
                };
                passData.opaqueRendererListHandle = renderGraph.CreateRendererList(in opaqueRendererListDesc);
                // RenderGraph ������Ⱦ�б�
                builder.UseRendererList(in passData.opaqueRendererListHandle);
                
                // ������Ⱦ��Ŀ��
                if (m_BackBufferColorTexture.IsValid())
                    builder.SetRenderAttachment(m_BackBufferColorTexture, 0, AccessFlags.Write);
                if (m_BackBufferDepthTexture.IsValid())
                    builder.SetRenderAttachmentDepth(m_BackBufferDepthTexture, AccessFlags.Write);

                // ������Ⱦȫ��״̬
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((DrawObjectsPassData data, RasterGraphContext context) =>
                {
                    // ������Ⱦָ����Ⱦ
                    context.cmd.DrawRendererList(data.opaqueRendererListHandle);
                });
            }
        }

        private void AddDrawTransparentPass(RenderGraph renderGraph, CameraData cameraData)
        {
            // ʹ�ù�դ����RenderPass��������
            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("Draw Transparent Pass", 
                out DrawObjectsPassData passData, s_DrawTransparentProfilingSampler))
            {
                // �������������õ���Դ, ��ȡ��Ⱦ����Դ�б�
                RendererListDesc transparentRendererListDesc = new RendererListDesc(s_DefaultShaderTagIdList, cameraData.cullingResults, cameraData.camera)
                {
                    sortingCriteria = SortingCriteria.CommonTransparent,
                    renderQueueRange = RenderQueueRange.transparent,
                };
                passData.transparentRendererListHandle = renderGraph.CreateRendererList(in transparentRendererListDesc);
                // RenderGraph ������Ⱦ�б�
                builder.UseRendererList(in passData.transparentRendererListHandle);
                
                // ������Ⱦ��Ŀ��
                if (m_BackBufferColorTexture.IsValid())
                    builder.SetRenderAttachment(m_BackBufferColorTexture, 0, AccessFlags.Write);
                if (m_BackBufferDepthTexture.IsValid())
                    builder.SetRenderAttachmentDepth(m_BackBufferDepthTexture, AccessFlags.Write);

                // ������Ⱦȫ��״̬
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((DrawObjectsPassData data, RasterGraphContext context) =>
                {
                    // ������Ⱦָ����Ⱦ
                    context.cmd.DrawRendererList(data.transparentRendererListHandle);
                });
            }
        }
    }
}
