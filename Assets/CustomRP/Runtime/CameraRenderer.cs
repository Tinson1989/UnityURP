using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

//相机渲染
public partial class CameraRenderer
{
    private static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    
    private ScriptableRenderContext context;
    private Camera camera;

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;
#if UNITY_EDITOR
        PrepareBuffer();
        PrepareFowSceneWindow();
#endif
        if (!Cull())
        {
            return;
        }

        SetUp();
        DrawVisibleGeometry();
#if UNITY_EDITOR
        DrawUnsupporttedShaders();
        //图标相关的东西
        DrawGizmos();
#endif
        Submit();
    }

    void SetUp()
    {
        //设置mvp
        context.SetupCameraProperties(camera);
        //清除渲染目标
        buffer.ClearRenderTarget(true, true, Color.clear);
        buffer.BeginSample(bufferName);
        //这里为啥要先执行一次呢？
        ExecuteBuffer();
    }

    void DrawVisibleGeometry()
    {
        //先绘制不透明的对象，然后是skybox，然后再绘制透明的对象
        var sortingSetting = new SortingSettings(camera)
        {
            //需要了解这里是按照什么顺序
            criteria = SortingCriteria.CommonOpaque
        };
        //绘图设置
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSetting);
        //筛选设置
        var filteringSetting = new FilteringSettings(RenderQueueRange.opaque);
        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSetting);
        //绘制天空盒
        context.DrawSkybox(camera);

        sortingSetting.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSetting;
        filteringSetting.renderQueueRange = RenderQueueRange.transparent;

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSetting);
    }

    

    void Submit()
    {
        buffer.EndSample(bufferName);
        ExecuteBuffer();
        context.Submit();
    }

    //命令缓冲区
    private const string bufferName = "RenderCamera";

    private CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    //执行缓冲区
    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    //剔除相机看不到的物体
    private CullingResults cullingResults;
    bool Cull()
    {
        //通过这里获取相机是否需要剔除
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }

        return false;
    }
}
