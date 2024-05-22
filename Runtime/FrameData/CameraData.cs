using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

namespace CustomRP
{
    public class CameraData : ContextItem
    {
        public Camera camera;
        public CullingResults cullingResults;
        public override void Reset()
        {
            camera = null;
            cullingResults = default;
        }

        internal RTClearFlags GetRTClearFlags()
        {
            CameraClearFlags clearFlags = camera.clearFlags;
            if (clearFlags == CameraClearFlags.Depth)
                return RTClearFlags.DepthStencil;
            else if (clearFlags == CameraClearFlags.Nothing)
                return RTClearFlags.None;
            
            return RTClearFlags.All;
        }

        internal Color GetRTClearColor()
        {
            return CoreUtils.ConvertSRGBToActiveColorSpace(camera.backgroundColor);    
        }
    }
}