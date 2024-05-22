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
            // 使用光栅化的RenderPass绘制物体
            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("Draw Opaque Pass", 
                out DrawObjectsPassData passData, s_DrawOpaqueProfilingSampler))
            {
                // 声明创建或引用的资源, 获取渲染的资源列表
                RendererListDesc opaqueRendererListDesc = new RendererListDesc(s_DefaultShaderTagIdList, cameraData.cullingResults, cameraData.camera)
                {
                    sortingCriteria = SortingCriteria.CommonOpaque,
                    renderQueueRange = RenderQueueRange.opaque,
                };
                passData.opaqueRendererListHandle = renderGraph.CreateRendererList(in opaqueRendererListDesc);
                // RenderGraph 引用渲染列表
                builder.UseRendererList(in passData.opaqueRendererListHandle);
                
                // 设置渲染的目标
                if (m_BackBufferColorTexture.IsValid())
                    builder.SetRenderAttachment(m_BackBufferColorTexture, 0, AccessFlags.Write);
                if (m_BackBufferDepthTexture.IsValid())
                    builder.SetRenderAttachmentDepth(m_BackBufferDepthTexture, AccessFlags.Write);

                // 设置渲染全局状态
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((DrawObjectsPassData data, RasterGraphContext context) =>
                {
                    // 调用渲染指令渲染
                    context.cmd.DrawRendererList(data.opaqueRendererListHandle);
                });
            }
        }

        private void AddDrawTransparentPass(RenderGraph renderGraph, CameraData cameraData)
        {
            // 使用光栅化的RenderPass绘制物体
            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("Draw Transparent Pass", 
                out DrawObjectsPassData passData, s_DrawTransparentProfilingSampler))
            {
                // 声明创建或引用的资源, 获取渲染的资源列表
                RendererListDesc transparentRendererListDesc = new RendererListDesc(s_DefaultShaderTagIdList, cameraData.cullingResults, cameraData.camera)
                {
                    sortingCriteria = SortingCriteria.CommonTransparent,
                    renderQueueRange = RenderQueueRange.transparent,
                };
                passData.transparentRendererListHandle = renderGraph.CreateRendererList(in transparentRendererListDesc);
                // RenderGraph 引用渲染列表
                builder.UseRendererList(in passData.transparentRendererListHandle);
                
                // 设置渲染的目标
                if (m_BackBufferColorTexture.IsValid())
                    builder.SetRenderAttachment(m_BackBufferColorTexture, 0, AccessFlags.Write);
                if (m_BackBufferDepthTexture.IsValid())
                    builder.SetRenderAttachmentDepth(m_BackBufferDepthTexture, AccessFlags.Write);

                // 设置渲染全局状态
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((DrawObjectsPassData data, RasterGraphContext context) =>
                {
                    // 调用渲染指令渲染
                    context.cmd.DrawRendererList(data.transparentRendererListHandle);
                });
            }
        }
    }
}
