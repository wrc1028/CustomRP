using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

namespace CustomRP
{
    /// <summary>
    /// Setup Camera Properties
    /// </summary>
    public partial class CustomRenderGraphRecord
    {
        private static readonly ProfilingSampler s_SetupCameraPropertiesProfilingSampler = new ProfilingSampler("Setup Camera Properties");

        internal class SetupCameraPropertiesPassData
        {
            internal CameraData cameraData;
        }

        private void AddSetupCameraPropertiesPass(RenderGraph renderGraph, CameraData cameraData)
        {
            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("Setup Camera Properties Pass", 
                out SetupCameraPropertiesPassData passData, s_SetupCameraPropertiesProfilingSampler))
            {
                passData.cameraData = cameraData;

                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);

                builder.SetRenderFunc((SetupCameraPropertiesPassData data, RasterGraphContext context) =>
                {
                    context.cmd.SetupCameraProperties(data.cameraData.camera);
                });

            }
        }
    }
}