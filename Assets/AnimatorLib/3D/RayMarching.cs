using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RayMarching : ScriptableRendererFeature
{
    class RayMarchingPass : ScriptableRenderPass
    {
        ComputeShader rayMarchingCompute;
        string kernelName;
        int renderTargetID;
        int depthTargetID;
        int MSAA;

        RenderTargetIdentifier renderTargetIdentifier;
        RenderTargetIdentifier depthTargetIdentifier;
        int renderTextureWidth, renderTextureHeight;

        public RayMarchingPass(ComputeShader rayMarchingCompute, string kernelName, int renderTargetID, int depthTargetID, int MSAA)
        {
            this.rayMarchingCompute = rayMarchingCompute;
            this.kernelName = kernelName;
            this.renderTargetID = renderTargetID;
            this.depthTargetID = depthTargetID;
            this.MSAA = MSAA;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            cameraTargetDescriptor.enableRandomWrite = true;
            cmd.GetTemporaryRT(renderTargetID, cameraTargetDescriptor);
            renderTargetIdentifier = new RenderTargetIdentifier(renderTargetID);

            RenderTextureDescriptor depthDescriptor = new RenderTextureDescriptor(cameraTargetDescriptor.width, cameraTargetDescriptor.height, RenderTextureFormat.RFloat, 32, 0, RenderTextureReadWrite.Default);
            depthDescriptor.enableRandomWrite = true;

            cmd.GetTemporaryRT(depthTargetID, depthDescriptor);
            depthTargetIdentifier = new RenderTargetIdentifier(depthTargetID);

            renderTextureWidth = cameraTargetDescriptor.width;
            renderTextureHeight = cameraTargetDescriptor.height;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //if (renderingData.cameraData.isSceneViewCamera) return;

            CommandBuffer cmd = CommandBufferPool.Get();
            var mainKernel = rayMarchingCompute.FindKernel(kernelName);
            rayMarchingCompute.GetKernelThreadGroupSizes(mainKernel, out uint xGroupSize, out uint yGroupSize, out _);
            cmd.Blit(renderingData.cameraData.targetTexture, renderTargetIdentifier);

            cmd.Blit(renderingData.cameraData.renderer.cameraDepthTarget, depthTargetIdentifier);

            cmd.SetComputeTextureParam(rayMarchingCompute, mainKernel, renderTargetID, renderTargetIdentifier);
            cmd.SetComputeTextureParam(rayMarchingCompute, mainKernel, depthTargetID, depthTargetIdentifier);

            cmd.SetComputeBufferParam(rayMarchingCompute, mainKernel, "_Spheres", SDFObjectManager.SphereBuffer);
            cmd.SetComputeIntParam(rayMarchingCompute, "_NumSpheres", SDFObjectManager.Spheres.Count);
            cmd.SetComputeBufferParam(rayMarchingCompute, mainKernel, "_Boxes", SDFObjectManager.BoxBuffer);
            cmd.SetComputeIntParam(rayMarchingCompute, "_NumBoxes", SDFObjectManager.Boxes.Count);
            cmd.SetComputeBufferParam(rayMarchingCompute, mainKernel, "_LineSegments", SDFObjectManager.SegmentBuffer);
            cmd.SetComputeIntParam(rayMarchingCompute, "_NumLineSegments", SDFObjectManager.Segments.Count);

            cmd.SetComputeBufferParam(rayMarchingCompute, mainKernel, "_Lights", SDFObjectManager.LightBuffer);
            cmd.SetComputeIntParam(rayMarchingCompute, "_NumLights", SDFObjectManager.Lights.Count);
            cmd.SetComputeBufferParam(rayMarchingCompute, mainKernel, "_Materials", SDFObjectManager.MaterialBuffer);

            cmd.SetComputeIntParam(rayMarchingCompute, "_Width", renderTextureWidth);
            cmd.SetComputeIntParam(rayMarchingCompute, "_Height", renderTextureHeight);
            cmd.SetComputeIntParam(rayMarchingCompute, "_MSAA", renderingData.cameraData.isSceneViewCamera ? 1 : MSAA);

            cmd.SetComputeFloatParam(rayMarchingCompute, "_NearClip", renderingData.cameraData.camera.nearClipPlane);
            cmd.SetComputeFloatParam(rayMarchingCompute, "_FarClip", renderingData.cameraData.camera.farClipPlane);
            cmd.SetComputeMatrixParam(rayMarchingCompute, "_CameraToWorld", renderingData.cameraData.camera.cameraToWorldMatrix);
            cmd.SetComputeMatrixParam(rayMarchingCompute, "_WorldToCamera", renderingData.cameraData.camera.worldToCameraMatrix);
            cmd.SetComputeMatrixParam(rayMarchingCompute, "_CameraInverseProjection", renderingData.cameraData.camera.projectionMatrix.inverse);
            cmd.SetComputeMatrixParam(rayMarchingCompute, "_CameraProjection", renderingData.cameraData.camera.projectionMatrix);

            cmd.DispatchCompute(rayMarchingCompute, mainKernel,
                Mathf.CeilToInt((float)renderTextureWidth / xGroupSize),
                Mathf.CeilToInt((float)renderTextureHeight / yGroupSize),
                1);

            cmd.Blit(renderTargetIdentifier, renderingData.cameraData.renderer.cameraColorTargetHandle);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(renderTargetID);
            cmd.ReleaseTemporaryRT(depthTargetID);
        }
    }

    RayMarchingPass rayMarchingPass;
    bool initialized;

    public ComputeShader rayMarchingCompute;
    public string kernelName = "main";
    public int MSAA = 2;

    public override void Create()
    {
        if (rayMarchingCompute == null)
        {
            initialized = false;
            return;
        }

        int renderTargetId = Shader.PropertyToID("_Result");
        int depthTargetId = Shader.PropertyToID("_Depth");
        rayMarchingPass = new RayMarchingPass(rayMarchingCompute, kernelName, renderTargetId, depthTargetId, MSAA);
        rayMarchingPass.renderPassEvent = RenderPassEvent.AfterRendering;
        initialized = true;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (initialized)
        {
            renderer.EnqueuePass(rayMarchingPass);
        }
    }
}
