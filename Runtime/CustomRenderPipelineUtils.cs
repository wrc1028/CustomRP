

using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    public static class CustomRenderPipelineUtils
    {
        public static bool SupportsNativeRenderPasses
        {
            get 
            {
                return SystemInfo.graphicsDeviceType != GraphicsDeviceType.Direct3D12 && 
                   SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3 && 
                   SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore;
            }
        }
    }
}