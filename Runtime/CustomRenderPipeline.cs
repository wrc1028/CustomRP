using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace CustomRP
{
    public class CustomRenderPipeline : RenderPipeline
    {
        private const string k_RenderGraphName = "Custom RP Render Graph";
        private RenderGraph m_RenderGraph = null;
        private CustomRenderGraphRecord m_CustomRenderGraphRecord = null;
        /// <summary>
        /// 上下文容器, 用于传递当前上下文的帧数据信息
        /// </summary>
        private ContextContainer m_ContextContainer = null;

        public CustomRenderPipeline()
        {
            InitializeRenderGraph();
        }

        protected override void Dispose(bool disposing)
        {
            CleanupRenderGraph();
            base.Dispose(disposing);
        }

        private void InitializeRenderGraph()
        {
            // 初始化 RTHandle System
            RTHandles.Initialize(Screen.width, Screen.height);
            m_RenderGraph = new RenderGraph(k_RenderGraphName);
            m_RenderGraph.nativeRenderPassesEnabled = CustomRenderPipelineUtils.SupportsNativeRenderPasses;
            m_CustomRenderGraphRecord = new CustomRenderGraphRecord();
            m_ContextContainer = new ContextContainer();
        }

        private void CleanupRenderGraph()
        {
            m_RenderGraph?.Cleanup();
            m_RenderGraph = null;

            m_CustomRenderGraphRecord?.Dispose();
            m_CustomRenderGraphRecord = null;

            m_ContextContainer?.Dispose();
            m_ContextContainer = null;
        }

#if UNITY_2021_1_OR_NEWER
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            Render(context, new List<Camera>(cameras));      
        }
#endif

#if UNITY_2021_1_OR_NEWER
        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
#else
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
#endif
        {
            // 管理注册事件回调
            BeginContextRendering(context, cameras);

            // 逐一渲染场景中激活的相机
            for (int cameraIndex = 0; cameraIndex < cameras.Count; cameraIndex ++)
            {
                RenderSingleCamera(context, cameras[cameraIndex]);
            }

            // 当前帧结束
            m_RenderGraph.EndFrame();

            EndContextRendering(context, cameras);
        }

        /// <summary>
        /// 渲染当前相机内容
        /// </summary>
        /// <param name="context">渲染上下文</param>
        /// <param name="camera">当前相机</param>
        private void RenderSingleCamera(ScriptableRenderContext context, Camera camera)
        {
            BeginCameraRendering(context, camera);

            if (!PrepareFrameData(context, camera))
                return;


            // 为相机创建cmd用于渲染
            CommandBuffer cmd = CommandBufferPool.Get();

            // 录制和执行RenderGraph
            RecordAndExecuteRenderGraph(context, camera, cmd);

            // 提交执行cmd
            context.ExecuteCommandBuffer(cmd);
            // 释放cmd
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            // 提交渲染上下文
            context.Submit();

            EndCameraRendering(context, camera);
        }

        /// <summary>
        /// 准备并填充ContextContainer(Frame Data)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        private bool PrepareFrameData(ScriptableRenderContext context, Camera camera)
        {
            // 相机剔除
            ScriptableCullingParameters cullingParameters;
            if (!camera.TryGetCullingParameters(out cullingParameters))
                return false;
            CullingResults cullingResults = context.Cull(ref cullingParameters);

            CameraData cameraData = m_ContextContainer.GetOrCreate<CameraData>();
            cameraData.camera = camera;
            cameraData.cullingResults = cullingResults;
            return true;
        }

        /// <summary>
        /// Render Graph 的录制和执行
        /// </summary>
        /// <param name="context">渲染上下文</param>
        /// <param name="camera">当前相机</param>
        /// <param name="cmd">渲染命令</param>
        private void RecordAndExecuteRenderGraph(ScriptableRenderContext context, Camera camera, CommandBuffer cmd)
        {
            RenderGraphParameters renderGraphParameters = new RenderGraphParameters()
            {
                executionName = camera.name,
                scriptableRenderContext = context,
                commandBuffer = cmd,
                currentFrameIndex = Time.frameCount,
            };
            // 执行线被SRP隐式的执行, 录制线需要自定义
            m_RenderGraph.BeginRecording(in renderGraphParameters);
            // 开启录制
            m_CustomRenderGraphRecord.RecordRenderGraph(m_RenderGraph, m_ContextContainer);

            m_RenderGraph.EndRecordingAndExecute();
        }
    }
}