using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(FurObject))]
//[CanEditMultipleObjects]
public partial class FurEditor : Editor
{
    bool isPaint;
    private UnityEditor.SceneView sv;
    [SerializeField]private float _brushSize;
    [SerializeField]private float _brushStronger;
    private FurObject _furObject;
    
    private enum PaintModeEnum  
    {  
        FurMask,  
        FlowMap
    }
    // 创建一个字符串数组，包含你想要显示的自定义名称
    string[] customNames = new string[]
    {
        "遮罩绘制",
        "生长方向绘制"
    };

    private PaintModeEnum _currentPaintMode = PaintModeEnum.FurMask;
    private void OnEnable()
    {
        if (SceneView.sceneViews.Count > 0)
        {
            sv = SceneView.sceneViews[0] as SceneView;
        }
        FindProperties();
        // 从EditorPrefs加载
        _brushSize = EditorPrefs.GetFloat("FurEditorBrushSize", 1.0f);
        _brushStronger = EditorPrefs.GetFloat("FurEditorBrushStronger", 1.0f);
    }
    public override void OnInspectorGUI()
    {
        _furObject = (FurObject)target;
        DrawUniversalGUI();
        if (isPaint && sv)//sv不为空，也就是说场景中有scene视图。否则不执行指令
        {
            sv.drawGizmos = true;//直接打开drawGizmos，以防忘记打开，OnSceneGUI失效 补：这个会报错，currentDrawingSceneView为空，奇怪
            _furObject.GetFurMap();
            if (!_furObject.textureData._furMaskTex)
            {
                //报错
                EditorGUILayout.HelpBox("FurMap贴图为空，请为材质导入贴图或点击创建新帖图。", MessageType.Error);
                //弹出创建贴图按钮
                CreateFurMap();
            }
        }
        switch (_currentPaintMode)
        {
            case PaintModeEnum.FurMask:
                DrawFurMaskInspector();
                break;
            case PaintModeEnum.FlowMap:
                DrawFlowMapInspector();
                break;
        }
    }

    private void OnDestroy()
    {
        // 保存_brushSize到EditorPrefs
        EditorPrefs.SetFloat("FurEditorBrushSize", _brushSize);
        EditorPrefs.SetFloat("FurEditorBrushStronger", _brushStronger);
    }
}
