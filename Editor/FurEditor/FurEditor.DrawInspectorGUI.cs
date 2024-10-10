using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public partial class FurEditor : Editor
{
    //所有property
    private SerializedProperty furMatProperty; 
    private SerializedProperty layerNumberProperty; 
    private SerializedProperty offsetScaleProperty;
    private SerializedProperty offsetScaleParameterProperty;
    private SerializedProperty furUVOffsetScaleProperty;
    private SerializedProperty currentPaintModeProperty;
    private SerializedProperty furClipCurveProperty;
    private void DrawUniversalGUI()
    {
        EditorGUILayout.BeginVertical();
        // 更新序列化对象的状态，这是必要的，以确保我们的SerializedProperty与当前MonoBehaviour实例同步  
        serializedObject.Update();  
        
        // 绘制自定义Inspector UI  
        EditorGUILayout.PropertyField(furMatProperty, FurStyles.furMat);
        EditorGUILayout.PropertyField(layerNumberProperty, FurStyles.layerNumber);
        EditorGUILayout.BeginVertical("HelpBox");
        EditorGUILayout.PropertyField(offsetScaleProperty, FurStyles.offsetScale);
        EditorGUILayout.PropertyField(offsetScaleParameterProperty, FurStyles.offsetScaleParameter);
        EditorGUILayout.PropertyField(furUVOffsetScaleProperty, FurStyles.furUVOffsetScale);
        EditorGUILayout.EndVertical();
        EditorGUILayout.PropertyField(currentPaintModeProperty, FurStyles.currentPaintMode);
        EditorGUILayout.PropertyField(furClipCurveProperty, FurStyles.furClipCurve);
        // 应用对序列化对象的任何更改  
        serializedObject.ApplyModifiedProperties(); 
        EditorGUILayout.EndVertical();
        //
        GUIStyle boolBtnOn = new GUIStyle(GUI.skin.GetStyle("Button"));//得到Button样式
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        FurStyles.paintToggleContent.tooltip = "点击切换绘制模式(需要打开Gizmos)";  // 设置工具提示文本
        isPaint = GUILayout.Toggle(isPaint, FurStyles.paintToggleContent, boolBtnOn, GUILayout.Width(35), GUILayout.Height(25));
        // 在图标后面添加一个标签
        GUILayout.Label("绘制模式", GUILayout.Width(70), GUILayout.Height(25)); // 你可以调整宽度以适应你的UI布局
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        _currentPaintMode = (PaintModeEnum)EditorGUILayout.Popup(_currentPaintMode.GetHashCode(),customNames);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }
    
    //初始化property
    public virtual void FindProperties()
    {
        furMatProperty = serializedObject.FindProperty("furMat"); 
        layerNumberProperty = serializedObject.FindProperty("layerNumber"); 
        offsetScaleProperty = serializedObject.FindProperty("offsetScale");
        offsetScaleParameterProperty = serializedObject.FindProperty("offsetScaleParameter");
        furUVOffsetScaleProperty = serializedObject.FindProperty("furUVOffsetScale");
        currentPaintModeProperty = serializedObject.FindProperty("currentPaintMode");
        furClipCurveProperty = serializedObject.FindProperty("furClipCurve");
    }

    private void DrawFurMaskInspector()
    {
        _brushSize = (int)EditorGUILayout.Slider("笔刷大小", _brushSize, 1, 10);//笔刷大小
        _brushStronger = EditorGUILayout.Slider("笔刷色值(仅黑白)", _brushStronger, 0, 1f);//笔刷强度
        
        // 创建一个颜色，从黑色到白色渐变
        Color colorValue = Color.white * _brushStronger;
        // 绘制颜色块
        // 禁用颜色字段，使其不可编辑
        EditorGUI.BeginDisabledGroup(true);
        Color disabledColorValue = GUI.color; // 保存当前GUI颜色
        GUI.color = colorValue; // 设置颜色字段的颜色
        EditorGUILayout.ColorField(colorValue); // 绘制不可编辑的颜色字段
        GUI.color = disabledColorValue; // 恢复GUI颜色
        EditorGUI.EndDisabledGroup();
    }

    private bool _replaceMode;
    private Vector2 _replaceFurDir;
    private void DrawFlowMapInspector()
    {
        _brushSize = (int)EditorGUILayout.Slider("笔刷大小", _brushSize, 1, 10);//笔刷大小
        EditorGUILayout.BeginVertical("HelpBox");
        _replaceMode = EditorGUILayout.Toggle(FurStyles.flowMapPaintModeContent, _replaceMode);
        _replaceFurDir = EditorGUILayout.Vector2Field("替换方向", _replaceFurDir);
        furDir = _replaceFurDir.normalized;//使用的是归一化后的方向
        if (GUILayout.Button("应用方向到全局"))
        {
            ApplyGlobalFurDir();
        }
        EditorGUILayout.EndVertical();
    }

    public void ApplyGlobalFurDir()
    {
        Texture2D srcTex = _furObject.textureData._furMaskTex;
        Undo.RegisterCompleteObjectUndo(_furObject.textureData._furMaskTex,"tex");//撤回贴图修改
        for (int i = 0 ; i < srcTex.width; i++)
        {
            for (int j = 0; j < srcTex.height; j++)
            {
                Color c = srcTex.GetPixel(i, j);
                c.r = furDir.x * 0.5f + 0.5f;//这时候应该已经归一化了
                c.g = furDir.y * 0.5f + 0.5f;
                _furObject.textureData._furMaskTex.SetPixel(i, j, c);
            }
        }
        // 应用更改
        _furObject.textureData._furMaskTex.Apply();
        SaveTexture(_furObject.textureData._furMaskTex);//自动保存一次贴图
        _furObject.textureData._texDirty = false;
    }
}
