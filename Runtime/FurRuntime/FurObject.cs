using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
[ExecuteInEditMode]
public class FurObject : MonoBehaviour
{
    //存放所有激活的毛发物体
    private static HashSet<FurObject> _actives = new HashSet<FurObject>();
    //使毛发物体集合无法从外部增删
    public static IReadOnlyCollection<FurObject> actives{
        get{
            return _actives;
        }
    }
    //gpuinstancing部分
    public MaterialPropertyBlock materialPropertyBlock;
    [SerializeField]
    private Material furMat;
    [SerializeField, Range(1,100)]
    private int layerNumber = 1;
    [SerializeField,Range(0f,1)] private float offsetScale = 1;
    [SerializeField]private float offsetScaleParameter = 1;
    [SerializeField,Range(0f, 30f)]private float furUVOffsetScale = 10;
    [SerializeField] private AnimationCurve furClipCurve = AnimationCurve.Linear(0,0,1 ,1);
    private Mesh mesh;
    private Matrix4x4[] matrices;
    
    //---------蒙皮网格用
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private GraphicsBuffer m_IndexBuffer;
    private GraphicsBuffer m_DeformedDataBuffer;
    private GraphicsBuffer m_StaticDataBuffer;
    private GraphicsBuffer m_SkinningDataBuffer;
    private int m_IndexCount;
    private int m_InstanceCount;
    private Matrix4x4 m_Matrix;
    
    //-------------静态网格

    private enum PaintMode//显示模式
    {
        FurMask,
        Fur,
        FurFlowMap
    }
    //当前显示模式
    [SerializeField]private PaintMode currentPaintMode = PaintMode.Fur;
    
    [Serializable]
    public struct TextureData
    {
        [SerializeField]public Texture2D _furMaskTex;
        [SerializeField]public bool _texDirty;
        
    }
    [SerializeField, HideInInspector]public TextureData textureData;
    
    private void Start()
    {
        Initialized();
    }

    public void Initialized()
    {
        if (!furMat)
        {
            furMat = new Material(Shader.Find("Custom/Fur"));
            furMat.enableInstancing = true;
        }
        mesh = GetComponent<MeshFilter>().sharedMesh;
        // skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        // if (!skinnedMeshRenderer)
        // {
        //     Debug.LogError("skinnedMeshRenderer为空");
        // }

        SkinnedMeshSetup();
    }
    
    public void MeshSetup()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        // if (!skinnedMeshRenderer)
        // {
        //     Debug.LogError("skinnedMeshRenderer为空");
        // }
        m_DeformedDataBuffer = mesh.GetVertexBuffer(0);
        mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
        int _uvStreamID = mesh.GetVertexAttributeStream(VertexAttribute.TexCoord0);
        m_StaticDataBuffer = mesh.GetVertexBuffer(_uvStreamID);
        //m_SkinningDataBuffer = Renderer.sharedMesh.GetVertexBuffer(Renderer.sharedMesh.GetVertexAttributeStream(VertexAttribute.BlendWeight));
            
        m_IndexBuffer = mesh.GetIndexBuffer();
        m_IndexCount = m_IndexBuffer.count;
        
        furMat.SetBuffer("_DeformedData", m_DeformedDataBuffer);
        furMat.SetBuffer("_StaticData", m_StaticDataBuffer);
        
        //var furObject = GameObject.Find("Model");
        //var furObject = GameObject.Find("Wolf_Generic");
        //SkinnedMeshRenderer SMrenderer = furObject.GetComponentInChildren<SkinnedMeshRenderer>();
        //m_Matrix = SMrenderer.transform.parent.Find("ch_bone").Find("Bip001").Find("Bip001 Pelvis").localToWorldMatrix;
        //m_Matrix = SMrenderer.transform.parent.Find("pm0143_00").localToWorldMatrix;
        //Debug.Log(m_Matrix);
        // 获取物体的世界坐标下的位置、旋转和缩放
        m_Matrix = transform.localToWorldMatrix;
        m_InstanceCount = Mathf.FloorToInt(furMat.GetFloat("_LayerCount"));
    }

    public void SkinnedMeshSetup()
    {
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (!skinnedMeshRenderer)
        {
            Debug.LogError("skinnedMeshRenderer为空");
        }
        m_DeformedDataBuffer = skinnedMeshRenderer.GetVertexBuffer();
        skinnedMeshRenderer.sharedMesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
        int _uvStreamID = skinnedMeshRenderer.sharedMesh.GetVertexAttributeStream(VertexAttribute.TexCoord0);
        m_StaticDataBuffer = skinnedMeshRenderer.sharedMesh.GetVertexBuffer(_uvStreamID);
        //m_SkinningDataBuffer = Renderer.sharedMesh.GetVertexBuffer(Renderer.sharedMesh.GetVertexAttributeStream(VertexAttribute.BlendWeight));
            
        m_IndexBuffer = skinnedMeshRenderer.sharedMesh.GetIndexBuffer();
        m_IndexCount = m_IndexBuffer.count;
        
        furMat.SetBuffer("_DeformedData", m_DeformedDataBuffer);
        furMat.SetBuffer("_StaticData", m_StaticDataBuffer);
        
        //var furObject = GameObject.Find("Model");
        //var furObject = GameObject.Find("Wolf_Generic");
        //SkinnedMeshRenderer SMrenderer = furObject.GetComponentInChildren<SkinnedMeshRenderer>();
        //m_Matrix = SMrenderer.transform.parent.Find("ch_bone").Find("Bip001").Find("Bip001 Pelvis").localToWorldMatrix;
        //m_Matrix = SMrenderer.transform.parent.Find("pm0143_00").localToWorldMatrix;
        //Debug.Log(m_Matrix);
        // 获取物体的世界坐标下的位置、旋转和缩放
        Vector3 position = skinnedMeshRenderer.rootBone.position;
        Quaternion rotation = skinnedMeshRenderer.rootBone.rotation;
        m_Matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
        m_InstanceCount = Mathf.FloorToInt(furMat.GetFloat("_LayerCount"));
    }

    public void DrawSMFur(CommandBuffer cmd)
    {
        //Debug.Log("开始绘制");
        cmd.DrawProcedural(m_IndexBuffer, m_Matrix, furMat, 1, MeshTopology.Triangles, m_IndexCount, m_InstanceCount);
    }
    
    public void DrawMFur(CommandBuffer cmd)
    {
        //Debug.Log("开始绘制");
        cmd.DrawProcedural(m_IndexBuffer, m_Matrix, furMat, 1, MeshTopology.Triangles, m_IndexCount, m_InstanceCount);
    }

    public void CleanUp()
    {
        m_IndexBuffer?.Dispose();
        m_DeformedDataBuffer?.Dispose();
        m_StaticDataBuffer?.Dispose();
        m_SkinningDataBuffer?.Dispose();
    }

    private void Setup() {
        
        float[] gradients = new float[layerNumber];
        //裁剪值数组(从1递增)
        float[] clips = new float[layerNumber];
        //顶点偏移数组(从1递增)
        float[] normaloffsets = new float[layerNumber];

        materialPropertyBlock = new MaterialPropertyBlock();

        for (int i = 0; i < layerNumber; i++)
        {
            clips[i] = furClipCurve.Evaluate(1.0f * i / layerNumber);
            gradients[i] = 1.0f * ( i + 1 ) / layerNumber;
            normaloffsets[i] = 1.0f * i / layerNumber * offsetScale * offsetScaleParameter;
        }

        materialPropertyBlock.SetFloatArray(ShaderProperties.s_clip, clips);
        materialPropertyBlock.SetFloatArray(ShaderProperties.s_vertexOffset, normaloffsets);
        materialPropertyBlock.SetFloatArray(ShaderProperties.s_gradient,gradients);
        materialPropertyBlock.SetFloat(ShaderProperties.s_furUVOffsetScale, furUVOffsetScale);
    }
    /// <summary>
    /// 处理毛发绘制指令
    /// </summary>
    public void SetupFurRenderCommands(ref CommandBuffer cmd)  
    {
        #if UNITY_EDITOR//编辑器下会判断状态决定绘制哪种
        if (!mesh)
        {
            mesh = GetComponent<MeshFilter>().sharedMesh;
        }
        switch (currentPaintMode)
        {
            case PaintMode.Fur:
                Setup();//放在这里，因为非编辑器模式不需要每帧Setup
                DrawFur(ref cmd);
                break;
            case PaintMode.FurMask:
                DrawFurMask(cmd);
                break;
            case PaintMode.FurFlowMap:
                DrawFurFlowMap(cmd);
                break;
        }
        #else//非编辑器下只会绘制毛发
        DrawFur(ref cmd);
        #endif
    }
    void OnEnable(){
        _actives.Add(this);
    }

    void OnDisable(){
        _actives.Remove(this);
    }
    private class ShaderProperties{
        //转换为id
        public static readonly int s_clip = Shader.PropertyToID("_Clip");
        public static readonly int s_vertexOffset = Shader.PropertyToID("_Offset");
        public static readonly int s_gradient = Shader.PropertyToID("_Gradient");
        public static readonly int s_furUVOffsetScale = Shader.PropertyToID("_FurUVOffsetScale");
        public static readonly int FurMask = Shader.PropertyToID("_FurMask");
    }

    private void DrawFurMask(CommandBuffer cmd)
    {
        cmd.DrawMesh(mesh,transform.localToWorldMatrix,furMat,0,1);
    }

    private void DrawFur(ref CommandBuffer cmd)
    {
        matrices = new Matrix4x4[layerNumber];
        for (int i = 1; i < layerNumber; i++) {//alphaTest必须从1开始，Transparent还可以从0开始
            matrices[i] = transform.localToWorldMatrix;
        }
        cmd.DrawMeshInstanced(mesh, 0, furMat, 0, matrices, layerNumber, materialPropertyBlock);
    }
    private void DrawFurFlowMap(CommandBuffer cmd)
    {
        cmd.DrawMesh(mesh,transform.localToWorldMatrix,furMat,0,2);
    }

    public void GetFurMap()
    {
        textureData._furMaskTex = furMat.GetTexture(ShaderProperties.FurMask) as Texture2D;
    }
    public void SetFurMap()
    {
        furMat.SetTexture(ShaderProperties.FurMask, textureData._furMaskTex);
    }
}
