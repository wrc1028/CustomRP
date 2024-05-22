using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Experimental.Rendering;

namespace CustomRP
{
    public partial class CustomRenderGraphRecord : IRenderGraphRecorder, IDisposable
    {
        private RTHandle m_CameraColorHandle = null;
        private TextureHandle m_BackBufferColorTexture = TextureHandle.nullHandle;

        private RTHandle m_CameraDepthHandle = null;
        private TextureHandle m_BackBufferDepthTexture = TextureHandle.nullHandle;
        /// <summary>
        /// 记录 RenderGraph
        /// </summary>
        /// <param name="renderGraph"></param>
        /// <param name="frameData">上下文容器, 用于传递当前上下文的帧数据信息</param>
        public void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            CameraData cameraData = frameData.Get<CameraData>();
            CreateRenderGraphCameraTargets(renderGraph, cameraData);

            AddSetupCameraPropertiesPass(renderGraph, cameraData);
            CameraClearFlags clearFlags = cameraData.camera.clearFlags;
            if (!CustomRenderPipelineUtils.SupportsNativeRenderPasses && clearFlags != CameraClearFlags.Nothing)
            {
                AddClearRenderTargetPass(renderGraph, cameraData);
            }

            AddDrawOpaquePass(renderGraph, cameraData);
            if (clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            {
                AddDrawSkyboxPass(renderGraph, cameraData);
            }
            AddDrawTransparentPass(renderGraph, cameraData);
#if UNITY_EDITOR
            AddDrawEditorGizmosPass(renderGraph, cameraData, GizmoSubset.PreImageEffects);
            AddDrawEditorGizmosPass(renderGraph, cameraData, GizmoSubset.PostImageEffects);
#endif
        }

        private void CreateRenderGraphCameraTargets(RenderGraph renderGraph, CameraData cameraData)
        {
            var cameraTargetTexture = cameraData.camera.targetTexture;
            bool isBuildInTexture = cameraTargetTexture == null;
            bool isCameraTargetOffscreenDepth = !isBuildInTexture && cameraTargetTexture.format == RenderTextureFormat.Depth;

            // Target Color
            RenderTargetIdentifier bulitinColorTarget = isBuildInTexture ? 
                BuiltinRenderTextureType.CameraTarget : new RenderTargetIdentifier(cameraTargetTexture);
            if (m_CameraColorHandle == null)
                m_CameraColorHandle = RTHandles.Alloc(bulitinColorTarget, "_CameraColorTexture");
            else if (m_CameraColorHandle.nameID != bulitinColorTarget)
                RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref m_CameraColorHandle, bulitinColorTarget);

            // Target Depth
            RenderTargetIdentifier bulitinDepthTarget = isBuildInTexture ? 
                BuiltinRenderTextureType.Depth : new RenderTargetIdentifier(cameraTargetTexture);
            if (m_CameraDepthHandle == null)
                m_CameraDepthHandle = RTHandles.Alloc(bulitinDepthTarget, "_CameraDepthTexture");
            else if (m_CameraDepthHandle.nameID != bulitinDepthTarget)
                RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref m_CameraDepthHandle, bulitinDepthTarget);

            // Render Target info (类似??RenderTextureDescriptor)
            int targetWidth = isBuildInTexture ? Screen.width : cameraTargetTexture.width;
            int targetHeight = isBuildInTexture ? Screen.height : cameraTargetTexture.height;
            int targetVolumeDepth = isBuildInTexture ? 1 : cameraTargetTexture.volumeDepth;
            int targetMsaaSamples = isBuildInTexture ? 1 : cameraTargetTexture.antiAliasing;
            RenderTargetInfo colorTargetInfo = new RenderTargetInfo()
            {
                width = targetWidth,
                height = targetHeight,
                volumeDepth = targetVolumeDepth,
                msaaSamples = targetMsaaSamples,
                format = GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Default, 
                    QualitySettings.activeColorSpace == ColorSpace.Linear),
            };
            RenderTargetInfo depthTargetInfo = colorTargetInfo;
            depthTargetInfo.format = SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil);

            // Import Resource Params (类似??ConfigureClear)
            bool clearOnFirstUse = !CustomRenderPipelineUtils.SupportsNativeRenderPasses; 
            bool discardColorBackBufferOnLastUse = !CustomRenderPipelineUtils.SupportsNativeRenderPasses;
            ImportResourceParams colorTargetParams = new ImportResourceParams()
            {
                clearOnFirstUse = clearOnFirstUse,
                discardOnLastUse = discardColorBackBufferOnLastUse,
                clearColor = CoreUtils.ConvertSRGBToActiveColorSpace(cameraData.camera.backgroundColor),
            };
            bool discardDepthBackBufferOnLastUse = !isCameraTargetOffscreenDepth;
            ImportResourceParams depthTargetParams = new ImportResourceParams()
            {
                clearOnFirstUse = clearOnFirstUse,
                discardOnLastUse = discardDepthBackBufferOnLastUse,
                clearColor = CoreUtils.ConvertSRGBToActiveColorSpace(cameraData.camera.backgroundColor),
            };

#if UNITY_EDITOR
            if (cameraData.camera.cameraType == CameraType.SceneView)
                depthTargetParams.discardOnLastUse = false;
#endif
            m_BackBufferColorTexture = renderGraph.ImportTexture(m_CameraColorHandle, colorTargetInfo, colorTargetParams);
            m_BackBufferDepthTexture = renderGraph.ImportTexture(m_CameraDepthHandle, depthTargetInfo, depthTargetParams);
        }
        
        public void Dispose()
        {
            RTHandles.Release(m_CameraColorHandle);
            RTHandles.Release(m_CameraDepthHandle);
                
            GC.SuppressFinalize(this);
        }
    }
}
