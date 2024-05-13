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

        RenderTargetIdentifier renderTargetIdentifier;
        RenderTargetIdentifier depthTargetIdentifier;
        int renderTextureWidth, renderTextureHeight;

        public RayMarchingPass(ComputeShader rayMarchingCompute, string kernelName, int renderTargetID, int depthTargetID)
        {
            this.rayMarchingCompute = rayMarchingCompute;
            this.kernelName = kernelName;
            this.renderTargetID = renderTargetID;
            this.depthTargetID = depthTargetID;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            cameraTargetDescriptor.enableRandomWrite = true;
            cmd.GetTemporaryRT(renderTargetID, cameraTargetDescriptor);
            renderTargetIdentifier = new RenderTargetIdentifier(renderTargetID);

            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(cameraTargetDescriptor.width, cameraTargetDescriptor.height, RenderTextureFormat.RFloat, 32, 0, RenderTextureReadWrite.Default);
            descriptor.enableRandomWrite = true;

            cmd.GetTemporaryRT(depthTargetID, descriptor);
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
            cmd.SetComputeIntParam(rayMarchingCompute, "_Width", renderTextureWidth);
            cmd.SetComputeIntParam(rayMarchingCompute, "_Height", renderTextureHeight);

            cmd.SetComputeFloatParam(rayMarchingCompute, "_NearClip", renderingData.cameraData.camera.nearClipPlane);
            cmd.SetComputeFloatParam(rayMarchingCompute, "_FarClip", renderingData.cameraData.camera.farClipPlane);
            cmd.SetComputeMatrixParam(rayMarchingCompute, "_CameraToWorld", renderingData.cameraData.camera.cameraToWorldMatrix);
            cmd.SetComputeMatrixParam(rayMarchingCompute, "_CameraInverseProjection", renderingData.cameraData.camera.projectionMatrix.inverse);

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

    public override void Create()
    {
        if (rayMarchingCompute == null)
        {
            initialized = false;
            return;
        }

        int renderTargetId = Shader.PropertyToID("_Result");
        int depthTargetId = Shader.PropertyToID("_Depth");
        rayMarchingPass = new RayMarchingPass(rayMarchingCompute, kernelName, renderTargetId, depthTargetId);
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
