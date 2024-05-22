using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    [CreateAssetMenu(fileName = "Custom RP Asset", menuName = "Custom RP/Render Pipeline Asset")]
    public class CustomRenderPipelineAsset : RenderPipelineAsset<CustomRenderPipeline>
    {
        protected override RenderPipeline CreatePipeline()
        {
            QualitySettings.antiAliasing = 1;
            return new CustomRenderPipeline();
        }
    }
}
