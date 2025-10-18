using Evergine.Bindings.WebGPU;
using static Evergine.Bindings.WebGPU.WebGPUNative;
using NBody.Simulation;
using Microsoft.JSInterop;
using System.Numerics;

namespace NBody.Web.Pages;

public partial class Home
{
  private WGPUDevice _device;
  private WGPUQueue _queue;
  private WGPUSwapChain _swapChain;
  private Universe _universe = default!;
  private float[] _particleData = default!; // 8 floats per body (pos.xyz, mass, vel.xyz, pad)
  private bool _initialized;
  private WGPUBuffer _particleBuffer;
  private WGPURenderPipeline _particlePipeline;
  private uint _bodyCount;
  private WGPUBuffer _sceneUniformBuffer;
  private WGPUBindGroupLayout _bindGroupLayout;
  private WGPUBindGroup _bindGroup;
  private Matrix4x4 _viewProj; // simple scaling camera for now
  private float _sceneScale = 1f;

  [JSInvokable]
  public static Task Tick() => Instance?.AdvanceAndRenderFrame() ?? Task.CompletedTask;
  private static Home? Instance;
  public Home() { Instance = this; }

  private Task AdvanceAndRenderFrame()
  {
    if (!_initialized) {
      try {
        InitializeWebGPU();
        InitSimulation();
        CreateParticleResources();
        _initialized = true;
      } catch (Exception ex) {
        Console.WriteLine($"Initialization error: {ex}");
        return Task.CompletedTask;
      }
    }

    _universe.Simulate();
    _universe.CopyParticleAttributes8(_particleData);
    UpdateSceneUniform();
    UpdateParticleBuffer();
    DrawFrame();
    return Task.CompletedTask;
  }

  private unsafe void InitializeWebGPU()
  {
    _device = emscripten_webgpu_get_device();
    _queue = wgpuDeviceGetQueue(_device);
    double width, height;
    emscripten_get_element_css_size("canvas".ToPointer(), &width, &height);

    var surfaceDescriptorFromCanvasHTMLSelector = new WGPUSurfaceDescriptorFromCanvasHTMLSelector() {
      chain = new WGPUChainedStruct() { sType = WGPUSType.SurfaceDescriptorFromCanvasHTMLSelector },
      selector = "canvas".ToPointer(),
    };
    var surfaceDescriptor = new WGPUSurfaceDescriptor() { nextInChain = (WGPUChainedStruct*)&surfaceDescriptorFromCanvasHTMLSelector };
    var surface = wgpuInstanceCreateSurface(instance: IntPtr.Zero, &surfaceDescriptor);

    var swapChainDescriptor = new WGPUSwapChainDescriptor() {
      usage = WGPUTextureUsage.RenderAttachment,
      format = WGPUTextureFormat.BGRA8Unorm,
      width = (uint)width,
      height = (uint)height,
      presentMode = WGPUPresentMode.Fifo,
    };
    _swapChain = wgpuDeviceCreateSwapChain(_device, surface, &swapChainDescriptor);
  }

  private void InitSimulation()
  {
    _bodyCount = 512; // initial WASM-friendly count
    var bodies = new Body[_bodyCount];
    _universe = new Universe(bodies);
    _particleData = new float[_bodyCount * 8];
    _universe.CopyParticleAttributes8(_particleData);
    ComputeInitialViewProjection();
  }

  private void ComputeInitialViewProjection()
  {
    float maxAbs = 1f;
    for (int i = 0; i < _particleData.Length; i += 8)
    {
      float x = _particleData[i];
      float y = _particleData[i + 1];
      float z = _particleData[i + 2];
      maxAbs = Math.Max(maxAbs, Math.Max(Math.Max(Math.Abs(x), Math.Abs(y)), Math.Abs(z)));
    }
    maxAbs *= 1.1f;
    _sceneScale = maxAbs > 0 ? 1f / maxAbs : 1f;
    _viewProj = Matrix4x4.CreateScale(_sceneScale, _sceneScale, _sceneScale);
  }

  private unsafe void CreateParticleResources()
  {
    // Vertex buffer
    ulong sizeBytes = (ulong)(_particleData.Length * sizeof(float));
    var bufferDesc = new WGPUBufferDescriptor() {
      size = sizeBytes,
      usage = WGPUBufferUsage.CopyDst | WGPUBufferUsage.Vertex,
      mappedAtCreation = false,
    };
    _particleBuffer = wgpuDeviceCreateBuffer(_device, &bufferDesc);

    // Uniform buffer (viewProj matrix)
    var sceneDesc = new WGPUBufferDescriptor() {
      size = 64,
      usage = WGPUBufferUsage.CopyDst | WGPUBufferUsage.Uniform,
      mappedAtCreation = false,
    };
    _sceneUniformBuffer = wgpuDeviceCreateBuffer(_device, &sceneDesc);
    UpdateSceneUniform();

    // Bind group layout
    var bgleEntry = new WGPUBindGroupLayoutEntry() {
      binding = 0,
      visibility = WGPUShaderStage.Vertex,
      buffer = new WGPUBufferBindingLayout() { type = WGPUBufferBindingType.Uniform },
    };
    var bglDesc = new WGPUBindGroupLayoutDescriptor() { entryCount = 1, entries = &bgleEntry };
    _bindGroupLayout = wgpuDeviceCreateBindGroupLayout(_device, &bglDesc);

    string wgsl = 
      """
      struct Scene {
        viewProj : mat4x4<f32>,
      };
      @group(0) @binding(0) var<uniform> uScene : Scene;
      
      struct VSOut {
        @builtin(position) pos : vec4<f32>,
        @location(0) col    : vec3<f32>,
      };
      
      // Attributes: location(0)=pos.xyz, location(1)=mass, location(2)=vel.xyz
      @vertex
      fn vs_main(
          @location(0) inPos  : vec3<f32>,
          @location(1) inMass : f32,
          @location(2) inVel  : vec3<f32>) -> VSOut {
          var o : VSOut;
          o.pos = uScene.viewProj * vec4<f32>(inPos, 1.0);
          let mag = length(inVel);
          if (mag < 1e-5) {
              o.col = vec3<f32>(1.0, 1.0, 1.0);
          } else {
              let v = normalize(inVel);
              // Map [-1,1] to [0,1]
              o.col = 0.5 + 0.5 * v;
          }
          return o;
      }
      
      @fragment
      fn fs_main(@location(0) col : vec3<f32>) -> @location(0) vec4<f32> {
          return vec4<f32>(col, 1.0);
      }
      """;

    var shader = CreateShaderModule(wgsl, _device);

    // Vertex layout
    WGPUVertexAttribute* attrs = stackalloc WGPUVertexAttribute[3];
    attrs[0] = new WGPUVertexAttribute { format = WGPUVertexFormat.Float32x3, offset = 0, shaderLocation = 0 };
    attrs[1] = new WGPUVertexAttribute { format = WGPUVertexFormat.Float32, offset = 12, shaderLocation = 1 };
    attrs[2] = new WGPUVertexAttribute { format = WGPUVertexFormat.Float32x3, offset = 16, shaderLocation = 2 };
    var vbl = new WGPUVertexBufferLayout {
      arrayStride = 32,
      attributeCount = 3,
      attributes = attrs,
      stepMode = WGPUVertexStepMode.Vertex
    };

    var vertexState = new WGPUVertexState {
      module = shader,
      entryPoint = "vs_main".ToPointer(),
      bufferCount = 1,
      buffers = &vbl
    };

    var blend = new WGPUBlendState {
      color = new WGPUBlendComponent {
        srcFactor = WGPUBlendFactor.One,
        dstFactor = WGPUBlendFactor.One,
        operation = WGPUBlendOperation.Add
      },
      alpha = new WGPUBlendComponent {
        srcFactor = WGPUBlendFactor.One,
        dstFactor = WGPUBlendFactor.One,
        operation = WGPUBlendOperation.Add
      }
    };

    var target = new WGPUColorTargetState {
      format = WGPUTextureFormat.BGRA8Unorm,
      blend = &blend,
      writeMask = WGPUColorWriteMask.All
    };

    var fragment = new WGPUFragmentState {
      module = shader,
      entryPoint = "fs_main".ToPointer(),
      targetCount = 1,
      targets = &target
    };

    // Pipeline layout
    var layout = _bindGroupLayout; // take address via local
    var layoutDesc = new WGPUPipelineLayoutDescriptor() {
      bindGroupLayoutCount = 1,
      bindGroupLayouts = &layout
    };
    var pipelineLayout = wgpuDeviceCreatePipelineLayout(_device, &layoutDesc);

    var rpDesc = new WGPURenderPipelineDescriptor {
      layout = pipelineLayout,
      vertex = vertexState,
      primitive = new WGPUPrimitiveState {
        topology = WGPUPrimitiveTopology.PointList,
        frontFace = WGPUFrontFace.CCW,
        cullMode = WGPUCullMode.None
      },
      fragment = &fragment,
      multisample = new WGPUMultisampleState { count = 1, mask = 0xFFFFFFFF, alphaToCoverageEnabled = false },
      depthStencil = null
    };

    _particlePipeline = wgpuDeviceCreateRenderPipeline(_device, &rpDesc);

    // Bind group
    var bgEntry = new WGPUBindGroupEntry { binding = 0, buffer = _sceneUniformBuffer, offset = 0, size = 64 };
    var bgDesc = new WGPUBindGroupDescriptor { layout = _bindGroupLayout, entryCount = 1, entries = &bgEntry };
    _bindGroup = wgpuDeviceCreateBindGroup(_device, &bgDesc);

    wgpuShaderModuleRelease(shader);
    wgpuPipelineLayoutRelease(pipelineLayout);
  }

  private unsafe void UpdateSceneUniform()
  {
    Span<float> m = stackalloc float[16];
    m[0] = _viewProj.M11; m[1] = _viewProj.M12; m[2] = _viewProj.M13; m[3] = _viewProj.M14;
    m[4] = _viewProj.M21; m[5] = _viewProj.M22; m[6] = _viewProj.M23; m[7] = _viewProj.M24;
    m[8] = _viewProj.M31; m[9] = _viewProj.M32; m[10] = _viewProj.M33; m[11] = _viewProj.M34;
    m[12] = _viewProj.M41; m[13] = _viewProj.M42; m[14] = _viewProj.M43; m[15] = _viewProj.M44;
    fixed (float* ptr = m)
    {
      wgpuQueueWriteBuffer(_queue, _sceneUniformBuffer, 0, ptr, 64);
    }
  }

  private unsafe void UpdateParticleBuffer()
  {
    fixed (float* p = _particleData)
    {
      uint sizeBytes = (uint)(_particleData.Length * sizeof(float));
      wgpuQueueWriteBuffer(_queue, _particleBuffer, 0, p, sizeBytes);
    }
  }

  private unsafe void DrawFrame()
  {
    var backbufferView = wgpuSwapChainGetCurrentTextureView(_swapChain);
    if (backbufferView == IntPtr.Zero)
      return;

    var colorAttachment = new WGPURenderPassColorAttachment() {
      view = backbufferView,
      loadOp = WGPULoadOp.Clear,
      storeOp = WGPUStoreOp.Store,
      clearValue = new WGPUColor { r = 0.02, g = 0.02, b = 0.05, a = 1.0 },
    };
    var passDesc = new WGPURenderPassDescriptor {
      colorAttachmentCount = 1,
      colorAttachments = &colorAttachment
    };

    var encoder = wgpuDeviceCreateCommandEncoder(_device, null);
    var pass = wgpuCommandEncoderBeginRenderPass(encoder, &passDesc);
    wgpuRenderPassEncoderSetPipeline(pass, _particlePipeline);
    wgpuRenderPassEncoderSetBindGroup(pass, 0, _bindGroup, 0, (uint*)0);
    wgpuRenderPassEncoderSetVertexBuffer(pass, 0, _particleBuffer, 0, WGPU_WHOLE_SIZE);
    wgpuRenderPassEncoderDraw(pass, _bodyCount, 1, 0, 0);
    wgpuRenderPassEncoderEnd(pass);

    var cmd = wgpuCommandEncoderFinish(encoder, null);
    wgpuQueueSubmit(_queue, 1, &cmd);

    wgpuRenderPassEncoderRelease(pass);
    wgpuCommandEncoderRelease(encoder);
    wgpuCommandBufferRelease(cmd);
    wgpuTextureViewRelease(backbufferView);
  }

  private unsafe WGPUShaderModule CreateShaderModule(string wgsl, WGPUDevice device)
  {
    var wgslDesc = new WGPUShaderModuleWGSLDescriptor {
      chain = new WGPUChainedStruct { sType = WGPUSType.ShaderModuleWGSLDescriptor },
      source = wgsl.ToPointer()
    };
    var desc = new WGPUShaderModuleDescriptor { nextInChain = (WGPUChainedStruct*)&wgslDesc };
    return wgpuDeviceCreateShaderModule(device, &desc);
  }
}
