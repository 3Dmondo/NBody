const initWebGPU = async () => {
    const adapter = await navigator.gpu.requestAdapter();
    const device = await adapter.requestDevice();
    Module.preinitializedWebGPUDevice = device;
};

const startLoop = () => {
    const frame = () => {
        DotNet.invokeMethodAsync('NBody.Web', 'Tick');
        requestAnimationFrame(frame);
    };
    requestAnimationFrame(frame);
};

initWebGPU().then(() => {
    startLoop();
});