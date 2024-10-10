using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FurRenderFeature : ScriptableRendererFeature
{
    class FurRenderPass : ScriptableRenderPass
    {
        private const string NameOfCommandBuffer = "Fur";//buffer名字
        private bool inReflectionPlane = false;

        public FurRenderPass(Settings settings)
        {
            this.renderPassEvent = settings.renderPassEvent;
            this.inReflectionPlane = settings.inReflectionPlane;
        }
        

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(NameOfCommandBuffer);
            try
            {
                foreach (var furObject in FurObject.actives)
                {
                    if(!furObject){
                        continue;//如果furObject为空，跳过
                    }

                    if (Shader.IsKeywordEnabled("_PLANAR_REFLECTION_CAMERA") && !inReflectionPlane)
                    {
                        continue;//平面反射中不渲染
                    }
                    furObject.SetupFurRenderCommands(cmd);
                }
                context.ExecuteCommandBuffer(cmd);
            }
            finally
            {
                CommandBufferPool.Release(cmd);
            }
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    FurRenderPass _furpass;
    [System.Serializable]
    public class Settings
    {
        //指定该RendererFeature在渲染流程的哪个时机插入
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        public bool inReflectionPlane = false;
    }
    public Settings settings = new Settings();

    public override void Create()
    {
        _furpass = new FurRenderPass(settings);
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_furpass);
    }
}


