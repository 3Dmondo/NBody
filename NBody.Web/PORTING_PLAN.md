# WebGPU Porting Plan (OpenTK NBody Desktop -> Blazor WebAssembly)

## 1. Goal & MVP
Create a Blazor WebAssembly page that continuously runs the existing NBody simulation (from `NBody.Simulation`) and renders each step with WebGPU via `Evergine.Bindings.WebGPU`. Minimum viable product (MVP): a static site (`NBody.Web`) that (a) initializes WebGPU, (b) advances the simulation every frame, (c) renders bodies as colored points/quads.

## 2. Current State Summary
- Desktop app uses OpenTK + GLSL point sprites (`StarShader.vert/frag`) drawn with `GL.DrawArrays(Points)`.
- Web project (`NBody.Web`) only draws a single rotating triangle once on button click.
- No reference yet from `NBody.Web` to `NBody.Simulation`.

## 3. Architectural Porting Outline
1. Add `ProjectReference` from `NBody.Web` to `NBody.Simulation` for simulation logic reuse.
2. Replace one-shot triangle demo with dynamic simulation render loop.
3. Translate GLSL shaders to WGSL (initially simplify: fixed point size or billboard quad). WebGPU lacks `gl_PointSize`; so use:
   - Option A (MVP): Render each body as a small static-size triangle/quad.
   - Option B (Later): Expand to instanced quads sized by mass & distance (compute size logic on CPU first).
4. Create GPU buffers each frame or (better) reuse buffers and update with `wgpuQueueWriteBuffer`.
5. Maintain camera & uniforms similar to desktop version (view/projection, flags for velocity coloring, blur, mass multiplier).
6. Implement frame loop using JavaScript `requestAnimationFrame` calling back into .NET to:
   - Advance simulation step(s)
   - Update uniform data & particle buffer
   - Issue draw commands
7. Optimize: reduce allocations, use persistent arrays, partial buffer updates, consider compute pass later.

## 4. Detailed Step Plan
### Step 1: Continuous Rendering Setup (Immediate Task)
- Remove Run button from `Home.razor`.
- Auto-start rendering after first render (`OnAfterRenderAsync(firstRender)`).
- In `Home.razor.cs`:
  - Add fields: `WGPUDevice _device; WGPUQueue _queue; WGPUSwapChain _swapChain;` plus buffers & pipeline objects.
  - Extract existing initialization code into `InitializeWebGPU()`.
  - Add simulation fields: `Universe _universe;` particle CPU array (`float[] _particleData;`).
  - Add `AdvanceAndRenderFrame()` method: (a) advance simulation (`_universe.Simulate()`), (b) rebuild / update particle buffer, (c) encode + submit commands, (d) schedule next frame.
  - Add `[JSInvokable] public static Task Tick()` or instance callback pattern; JS will call this each animation frame.
- Modify `webgpu.js` (or add new script) to start a loop: `function startLoop(){ function frame(){ DotNet.invokeMethodAsync('NBody.Web','Tick'); requestAnimationFrame(frame);} requestAnimationFrame(frame); }` and call `startLoop()` automatically.
- Ensure `Tick` distinguishes first-call initialization from subsequent per-frame updates.

### Step 2: Integrate Simulation Project
- Add `<ProjectReference Include="..\NBody.Simulation\NBody.Simulation.csproj" />` to `NBody.Web.csproj`.
- Instantiate `Universe` with a subset of bodies (choose reasonable count for WASM performance, e.g. 1k) using existing constructor or a helper initializer.

### Step 3: Particle Data Layout for WebGPU
- For MVP replicate 8-float packing described in `Universe.CopyParticleAttributes8`: `pos.xyz, mass, velUnit.xyz, pad`.
- Create a single storage/vertex buffer: layout attributes:
  - location(0): position (vec3<f32>)
  - location(1): velocity unit (vec3<f32>)
  - location(2): mass (f32)
- WGSL struct example:
  ```wgsl
  struct Particle { pos: vec3<f32>, mass: f32, vel: vec3<f32>, pad: f32 }; // 32 bytes
  @group(0) @binding(0) var<uniform> Scene : SceneUniform;
  @group(0) @binding(1) var<storage, read> Particles : array<Particle>;
  ```
- MVP simplification: use vertex buffer instead of storage buffer (expand CPU-side into tightly packed vertex attributes). Later: move to storage + vertex shader `@builtin(vertex_index)` fetch.

### Step 4: WGSL Shader Draft (MVP)
- Vertex: read attributes, compute color (velocity coloring or constant), compute size surrogate (for now ignore dynamic size; fixed scaling). Output clip-space position using simple perspective uniform.
- Fragment: output color; optionally alpha for blend.
- Uniforms struct: camera matrices (4x4), camera position (vec3+pad), flags (int), mass multiplier (f32).

### Step 5: Render Pass & Command Encoding
- Reuse swap chain each frame; fetch texture view.
- Clear, set pipeline, set bind group(s), set vertex buffer, draw N points (or N * 6 indices if quads).
- Present.

### Step 6: Frame Loop Mechanics
- JS `requestAnimationFrame` triggers .NET `Tick`.
- `Tick` calls `AdvanceAndRenderFrame()`; after finishing returns a completed Task.
- Keep per-frame allocations minimal (no new arrays; reuse stack or preallocated arrays). Use `wgpuQueueWriteBuffer` to update vertex buffer contents.

### Step 7: Camera Handling
- Start with fixed camera (Identity view, orthographic or simple perspective). Later add user input (mouse drag, scroll) via JS interop.

### Step 8: Performance & Scaling Roadmap (Post-MVP)
- Switch to storage buffer + vertex indexing for more flexible particle attributes.
- Add compute pass to update positions on GPU (port simulation kernels gradually).
- Implement dynamic point size (billboard quads) using per-instance vertex expansion or geometry replacement.
- Introduce double buffering for particle data to avoid write/read hazards.
- Frustum culling (CPU pre-pass) for large body counts.

### Step 9: Shader Feature Parity (Later)
- Port blur option: implement soft falloff via fragment distance field inside quad (requires per-body quad and UV coords; mimic circle discard done in GLSL).
- Velocity coloring: replicate logic from GLSL: accumulate squared components by sign; use WGSL functions.

### Step 10: Packaging & Static Site
- Ensure app runs without server features (Blazor WASM only). Confirm `index.html` loads `webgpu.js`, which starts loop automatically.
- Consider build-time trimming; keep unsafe blocks configuration for WebGPU bindings.

## 5. Incremental Delivery Checklist
- [ ] Step 1 edits (`Home.razor` / `.cs`) continuous loop & remove button.
- [ ] Add project reference to simulation.
- [ ] Instantiate Universe with test bodies.
- [ ] Create vertex buffer & pipeline using particle layout.
- [ ] WGSL shaders (minimal).
- [ ] Frame loop stable (>=60 FPS with small body count).
- [ ] Document shader & buffer layout.
- [ ] Add velocity coloring flag uniform.
- [ ] Add mass multiplier uniform.

## 6. Risks & Mitigations
- Large body counts may exceed WASM JS->.NET frame latency: Keep bodies small initially (<=2k). Optimize later (compute shaders).
- WGSL point size limitation: accept visual downgrade initially; plan quad billboard system.
- Memory copies each frame: Use single pinned array & update in-place.

## 7. Next Action
Proceed with Step 1: modify `Home.razor` and `Home.razor.cs` as described to start automatic rendering loop.
