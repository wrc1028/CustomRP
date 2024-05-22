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
        /// ����������, ���ڴ��ݵ�ǰ�����ĵ�֡������Ϣ
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
            // ��ʼ�� RTHandle System
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
            // ����ע���¼��ص�
            BeginContextRendering(context, cameras);

            // ��һ��Ⱦ�����м�������
            for (int cameraIndex = 0; cameraIndex < cameras.Count; cameraIndex ++)
            {
                RenderSingleCamera(context, cameras[cameraIndex]);
            }

            // ��ǰ֡����
            m_RenderGraph.EndFrame();

            EndContextRendering(context, cameras);
        }

        /// <summary>
        /// ��Ⱦ��ǰ�������
        /// </summary>
        /// <param name="context">��Ⱦ������</param>
        /// <param name="camera">��ǰ���</param>
        private void RenderSingleCamera(ScriptableRenderContext context, Camera camera)
        {
            BeginCameraRendering(context, camera);

            if (!PrepareFrameData(context, camera))
                return;


            // Ϊ�������cmd������Ⱦ
            CommandBuffer cmd = CommandBufferPool.Get();

            // ¼�ƺ�ִ��RenderGraph
            RecordAndExecuteRenderGraph(context, camera, cmd);

            // �ύִ��cmd
            context.ExecuteCommandBuffer(cmd);
            // �ͷ�cmd
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            // �ύ��Ⱦ������
            context.Submit();

            EndCameraRendering(context, camera);
        }

        /// <summary>
        /// ׼�������ContextContainer(Frame Data)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        private bool PrepareFrameData(ScriptableRenderContext context, Camera camera)
        {
            // ����޳�
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
        /// Render Graph ��¼�ƺ�ִ��
        /// </summary>
        /// <param name="context">��Ⱦ������</param>
        /// <param name="camera">��ǰ���</param>
        /// <param name="cmd">��Ⱦ����</param>
        private void RecordAndExecuteRenderGraph(ScriptableRenderContext context, Camera camera, CommandBuffer cmd)
        {
            RenderGraphParameters renderGraphParameters = new RenderGraphParameters()
            {
                executionName = camera.name,
                scriptableRenderContext = context,
                commandBuffer = cmd,
                currentFrameIndex = Time.frameCount,
            };
            // ִ���߱�SRP��ʽ��ִ��, ¼������Ҫ�Զ���
            m_RenderGraph.BeginRecording(in renderGraphParameters);
            // ����¼��
            m_CustomRenderGraphRecord.RecordRenderGraph(m_RenderGraph, m_ContextContainer);

            m_RenderGraph.EndRecordingAndExecute();
        }
    }
}