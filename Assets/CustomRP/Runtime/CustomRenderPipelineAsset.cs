using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//�����ʲ�������̳�RenderPipelineAsset
//��Ҫ���ṩһ�ַ�������ȡ��Ⱦ�Ĺ��ߵ�ʵ��
[CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline();
    }
}
