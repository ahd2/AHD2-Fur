using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public partial class FurEditor : Editor
{
    //包含所有GUIContent
    private class FurStyles
    {
        public static GUIContent furMat = new GUIContent("毛发材质");
        public static GUIContent layerNumber = new GUIContent("毛发层数");
        public static GUIContent offsetScale = new GUIContent("毛发长度");
        public static GUIContent offsetScaleParameter = new GUIContent("毛发长度系数");
        public static GUIContent furUVOffsetScale = new GUIContent("毛发方向偏移程度");
        public static GUIContent currentPaintMode = new GUIContent("显示模式");
        public static GUIContent furClipCurve = new GUIContent("毛发形状曲线");
        
        //绘制画笔GUIContent
        public static GUIContent paintToggleContent = EditorGUIUtility.IconContent("ClothInspector.PaintValue");
        //flowmapGUIContent
        public static GUIContent flowMapPaintModeContent = new GUIContent("替换模式", "使用预设方向替换绘制区域的毛发方向。");
        
        //Scene视图GUIContent
        public static GUIContent savedContent = new GUIContent("已保存", "纹理已保存到磁盘。Ctrl + s 保存。");
        public static GUIContent nonSavedContent = new GUIContent("未保存", "纹理已修改但未保存到磁盘。Ctrl + s 保存。");
    }
    public void SaveTexture(Texture2D texture2D)
    {
        // 获取纹理的路径
        string path = AssetDatabase.GetAssetPath(texture2D);
        var bytes = _furObject.textureData._furMaskTex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);//刷新
    }
    /// <summary>
    /// 键盘事件回调(覆写保存快捷键)
    /// </summary>
    private void EventCallBack(Event current)
    {
        if (current.modifiers == EventModifiers.Control)
        {
            if (current.keyCode== KeyCode.S)
            {
                SaveTexture(_furObject.textureData._furMaskTex);
                _furObject.textureData._texDirty = false;
            }
        }
    }

    private void CreateFurMap()
    {
        Vector2Int imageSize = new Vector2Int(1024, 1024);
        EditorGUILayout.Vector2IntField("", imageSize);
        if (GUILayout.Button("创建贴图"))
        {
            Texture2D furMap = new Texture2D(imageSize.x, imageSize.y, TextureFormat.ARGB32, false);
            
            // 弹出文件选择窗口  
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(assetPath)) 
            {
                assetPath = "Assets";
            }
            string path = EditorUtility.OpenFolderPanel("选择路径", assetPath, "");
            
            if (path == "")
            {
                GUIUtility.ExitGUI();//提前结束绘制，不加这个报错不匹配
                return;
            }
            path = path.Substring(path.IndexOf("Assets"));//相对路径
            path += "/FurMap.png";
            var bytes = furMap.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);//刷新
            
            TextureImporter furMapImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            furMapImporter.isReadable = true;
            // 应用更改
            AssetDatabase.ImportAsset(path);
            _furObject.textureData._furMaskTex = furMap;
            _furObject.SetFurMap();
        }
    }
}
