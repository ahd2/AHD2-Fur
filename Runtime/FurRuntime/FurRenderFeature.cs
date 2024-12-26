using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FurRenderFeature : ScriptableRendererFeature
{
    class FurRenderPass : ScriptableRenderPass
    {
        ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Render Fur");
        private bool inReflectionPlane = false;

        public FurRenderPass(Settings settings)
        {
            this.renderPassEvent = settings.renderPassEvent;
            this.inReflectionPlane = settings.inReflectionPlane;
        }
        
        
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            foreach (var furObject in FurObject.actives)
            {
                if(!furObject){
                    continue;//如果furObject为空，跳过
                }

                if (Shader.IsKeywordEnabled("_PLANAR_REFLECTION_CAMERA") && !inReflectionPlane || renderingData.cameraData.camera.cameraType == CameraType.Preview)
                {
                    continue;//平面反射中不渲染
                }
                //furObject.SkinnedMeshSetup();
                furObject.MeshSetup();
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                foreach (var furObject in FurObject.actives)
                {
                    if(!furObject){
                        continue;//如果furObject为空，跳过
                    }

                    if (Shader.IsKeywordEnabled("_PLANAR_REFLECTION_CAMERA") && !inReflectionPlane || renderingData.cameraData.camera.cameraType == CameraType.Preview)
                    {
                        continue;//平面反射中不渲染
                    }
                    //furObject.SetupFurRenderCommands(ref cmd);
                    //Debug.Log(furObject);
                    //furObject.DrawSMFur(cmd);
                    furObject.DrawMFur(cmd);
                }
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            foreach (var furObject in FurObject.actives)
            {
                if(!furObject){
                    continue;//如果furObject为空，跳过
                }
                furObject.CleanUp();
            }
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


