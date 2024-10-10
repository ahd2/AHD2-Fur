using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public partial class FurEditor : Editor
{
    private delegate void OnDrawMouseAction(PaintParameters parameters);

    private OnDrawMouseAction _drawMouseAction;
    void OnSceneGUI()
    {
        if (isPaint)
        {
            _drawMouseAction = null;//清空委托
            switch (_currentPaintMode)
            {
                case PaintModeEnum.FurMask:
                    _drawMouseAction += DrawFurMask;
                    break;
                case PaintModeEnum.FlowMap:
                    if (_replaceMode)
                    {
                        _drawMouseAction += ReplaceFurFlowMap;
                    }
                    else
                    {
                        _drawMouseAction += DrawFurFlowMap;
                    }
                    break;
            }
            DrawTexture(_drawMouseAction);
        }
    }
    //给委托传递参数
    private class PaintParameters
    {
        public Event e;
        public float size;
        public RaycastHit hit;
        // 构造函数
        public PaintParameters(Event e, float size, RaycastHit hit)
        {
            this.e = e;
            this.size = size;
            this.hit = hit;
        }
    }
    
    private void DrawTexture(OnDrawMouseAction onDrawMouseAction)
    {
        float size = 0.01f * _brushSize;//方形区域半径（单位是0-1的uv上的）
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (_furObject.textureData._texDirty)
            {
                Handles.color = Color.red;
            }
            else
            {
                Handles.color = Color.green;
            }
            Handles.DrawWireDisc(hit.point, hit.normal, size * 0.8f);
            PaintParameters paintParameters = new PaintParameters(e, size, hit);
            onDrawMouseAction?.Invoke(paintParameters);//仅委托不为空时调用
        }
        //获取当前事件的类型
        if (e.rawType == EventType.KeyDown)
        {
            EventCallBack(e);
        }
        // 绘制自定义图标和标签
        DrawCustomIcon();
        HandleUtility.Repaint();
    }

    private Texture2D _savedIconTexture;
    private Texture2D _nonSavedIconTexture;
    private void DrawCustomIcon()
    {
        if (!_savedIconTexture || !_nonSavedIconTexture)
        {
             _nonSavedIconTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/Editor/FurEditor/NonSaved.png", typeof(Texture2D));
             _savedIconTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/Editor/FurEditor/Saved.png", typeof(Texture2D));
        }
    
        if (_savedIconTexture != null)
        {
            Handles.BeginGUI();
            // 获取 Scene 视图的尺寸
            Rect sceneViewRect = SceneView.currentDrawingSceneView.position;
            float iconSize = 64; // 图标大小
            Rect iconRect = new Rect(sceneViewRect.width - iconSize, sceneViewRect.height - iconSize - 32, iconSize, iconSize);
            
            // 创建自定义GUIStyle
            GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
            labelStyle.fontSize = 25; // 设置字体大小
            labelStyle.fontStyle = FontStyle.Bold; // 设置字体为粗体
            labelStyle.alignment = TextAnchor.MiddleLeft; // 设置文本对齐方式
            
            if (_furObject.textureData._texDirty)
            {
                GUI.DrawTexture(iconRect, _nonSavedIconTexture);
                
                labelStyle.normal.textColor = Color.red;
                // 绘制文本
                GUI.Label(new Rect(sceneViewRect.width - iconSize - 80, sceneViewRect.height - iconSize - 20,80,64), FurStyles.nonSavedContent, labelStyle);
            }
            else
            {
                GUI.DrawTexture(iconRect, _savedIconTexture);
                
                labelStyle.normal.textColor = Color.green;
                // 绘制文本
                GUI.Label(new Rect(sceneViewRect.width - iconSize - 80, sceneViewRect.height - iconSize - 20,80,64), FurStyles.savedContent, labelStyle);
            }
            Handles.EndGUI();
        }
    }
    

    private void DrawFurMask(PaintParameters parameters)
    {
        if (parameters.e.type == EventType.MouseDrag && parameters.e.button == 0 ||parameters.e.type == EventType.MouseDown && parameters.e.button == 0)
        {
            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
            parameters.e.Use();
            Vector2 pixelUV = parameters.hit.textureCoord;
            // 这里添加你的绘制逻辑
            //贴图UV坐标以右上角为原点
            int width = _furObject.textureData._furMaskTex.width;
            int height = _furObject.textureData._furMaskTex.height;
            Undo.RegisterCompleteObjectUndo(_furObject.textureData._furMaskTex,"tex");//撤回贴图修改
            for (int i = (int)((pixelUV.x - parameters.size) * width); i < (int)((pixelUV.x + parameters.size) * width); i++)
            {
                for (int j = (int)((pixelUV.y - parameters.size) * height); j < (int)((pixelUV.y + parameters.size) * height); j++)
                {
                    // 计算当前像素点到笔刷中心点的距离的平方  
                    float dx = (i / (float)width) - pixelUV.x;  
                    float dy = (j / (float)height) - pixelUV.y;  
                    float distanceSquared = dx * dx + dy * dy;  
                    //if(i < 0 || j <0 || i >= width || j >= height) continue;
                    if(distanceSquared > parameters.size * parameters.size) continue;
                    Color c = _furObject.textureData._furMaskTex.GetPixel(i, j);
                    c.b = _brushStronger;
                    _furObject.textureData._furMaskTex.SetPixel(i, j, c);
                }
            }
            _furObject.textureData._furMaskTex.Apply();
            _furObject.textureData._texDirty = true;//修改后未保存，标记为脏
        }
        else if (parameters.e.type == EventType.MouseUp && parameters.e.button == 0)
        {
            //SaveTexture();//绘制结束保存Control贴图
            GUIUtility.hotControl = 0;
        }
    }
    
    private Vector2 currentPixelUV;
    private Vector2 lastFramePixelUV;
    private Vector2 furDir;
    private void DrawFurFlowMap(PaintParameters parameters)
    {
        if (parameters.e.type == EventType.MouseDrag && parameters.e.button == 0 ||parameters.e.type == EventType.MouseDown && parameters.e.button == 0)
        {
            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
            parameters.e.Use();
            currentPixelUV = parameters.hit.textureCoord;
            Vector2 lastfurDir = furDir;
            furDir = (currentPixelUV - lastFramePixelUV);
            if (furDir.magnitude < 0.001f)
            {
                furDir = lastfurDir;
            }
            furDir = furDir.normalized;
            // 这里添加你的绘制逻辑
            //贴图UV坐标以右上角为原点
            int width = _furObject.textureData._furMaskTex.width;
            int height = _furObject.textureData._furMaskTex.height;
            Undo.RegisterCompleteObjectUndo(_furObject.textureData._furMaskTex,"tex");//撤回贴图修改
            for (int i = (int)((currentPixelUV.x - parameters.size) * width); i < (int)((currentPixelUV.x + parameters.size) * width); i++)
            {
                for (int j = (int)((currentPixelUV.y - parameters.size) * height); j < (int)((currentPixelUV.y + parameters.size) * height); j++)
                {
                    // 计算当前像素点到笔刷中心点的距离的平方  
                    float dx = (i / (float)width) - currentPixelUV.x;  
                    float dy = (j / (float)height) - currentPixelUV.y;  
                    float distanceSquared = dx * dx + dy * dy;  
                    //if(i < 0 || j <0 || i >= width || j >= height) continue;
                    if(distanceSquared > parameters.size * parameters.size) continue;
                    Color c = _furObject.textureData._furMaskTex.GetPixel(i, j);
                    float finalpaintWeight = distanceSquared / (parameters.size * parameters.size);
                    // c.r = (c.r * (1 - _brushStronger) + furDir.x * _brushStronger);
                    // c.g = (c.g * (1 - _brushStronger) + furDir.y * _brushStronger);
                    c.r = (c.r * finalpaintWeight + (furDir.x * 0.5f + 0.5f ) * (1 - finalpaintWeight)) ;//软笔刷，但是效果不太好
                    c.g = (c.g * finalpaintWeight + (furDir.y * 0.5f + 0.5f ) * (1 - finalpaintWeight));
                    _furObject.textureData._furMaskTex.SetPixel(i, j, c);
                }
            }
            _furObject.textureData._furMaskTex.Apply();
            _furObject.textureData._texDirty = true;//修改后未保存，标记为脏
        }
        else if (parameters.e.type == EventType.MouseUp && parameters.e.button == 0)
        {
            //SaveTexture();//绘制结束保存Control贴图
            GUIUtility.hotControl = 0;
        }
        lastFramePixelUV = currentPixelUV;
    }
    
    private void ReplaceFurFlowMap(PaintParameters parameters)
    {
        if (parameters.e.type == EventType.MouseDrag && parameters.e.button == 0 ||parameters.e.type == EventType.MouseDown && parameters.e.button == 0)
        {
            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
            parameters.e.Use();
            currentPixelUV = parameters.hit.textureCoord;
            //贴图UV坐标以右上角为原点
            int width = _furObject.textureData._furMaskTex.width;
            int height = _furObject.textureData._furMaskTex.height;
            Undo.RegisterCompleteObjectUndo(_furObject.textureData._furMaskTex,"tex");//撤回贴图修改
            for (int i = (int)((currentPixelUV.x - parameters.size) * width); i < (int)((currentPixelUV.x + parameters.size) * width); i++)
            {
                for (int j = (int)((currentPixelUV.y - parameters.size) * height); j < (int)((currentPixelUV.y + parameters.size) * height); j++)
                {
                    // 计算当前像素点到笔刷中心点的距离的平方  
                    float dx = (i / (float)width) - currentPixelUV.x;  
                    float dy = (j / (float)height) - currentPixelUV.y;  
                    float distanceSquared = dx * dx + dy * dy;  
                    //if(i < 0 || j <0 || i >= width || j >= height) continue;
                    if(distanceSquared > parameters.size * parameters.size) continue;
                    Color c = _furObject.textureData._furMaskTex.GetPixel(i, j);
                    c.r = furDir.x * 0.5f + 0.5f;
                    c.g = furDir.y * 0.5f + 0.5f;
                    _furObject.textureData._furMaskTex.SetPixel(i, j, c);
                }
            }
            _furObject.textureData._furMaskTex.Apply();
            _furObject.textureData._texDirty = true;//修改后未保存，标记为脏
        }
        else if (parameters.e.type == EventType.MouseUp && parameters.e.button == 0)
        {
            //SaveTexture();//绘制结束保存Control贴图
            GUIUtility.hotControl = 0;
        }
    }
}
