using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Water_Volume : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private Material _material;
        private RTHandle tempRT;

        private RTHandle sourceHandle;

        public CustomRenderPass(Material mat)
        {
            _material = mat;
        }

        public void SetSource(RTHandle source)
        {
            sourceHandle = source;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor desc)
        {
            tempRT = RTHandles.Alloc(
                desc.width,
                desc.height,
                depthBufferBits: DepthBits.None,
                colorFormat: desc.graphicsFormat,
                filterMode: FilterMode.Bilinear,
                name: "_TemporaryColourTexture"
            );
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Reflection)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();

            // Blit source → temp
            Blitter.BlitCameraTexture(cmd, sourceHandle, tempRT, _material, 0);

            // Blit temp → source
            Blitter.BlitCameraTexture(cmd, tempRT, sourceHandle);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            RTHandles.Release(tempRT);
        }
    }

    [System.Serializable]
    public class _Settings
    {
        public Material material = null;
        public RenderPassEvent renderPass = RenderPassEvent.AfterRenderingSkybox;
    }

    public _Settings settings = new _Settings();

    CustomRenderPass pass;

    public override void Create()
    {
        if (settings.material == null)
            settings.material = (Material)Resources.Load("Water_Volume");

        pass = new CustomRenderPass(settings.material);
        pass.renderPassEvent = settings.renderPass;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_2022_2_OR_NEWER
        pass.SetSource(renderer.cameraColorTargetHandle);
#else
        pass.SetSource(renderer.cameraColorTarget);
#endif
        renderer.EnqueuePass(pass);
    }
}
