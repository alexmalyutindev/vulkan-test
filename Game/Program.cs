using Engine;
using EngineCore;
using EngineCore.Rendering.Core;


var windowModule = new WindowModule();
windowModule.Init();
var vulkanContext = new VulkanContext();
vulkanContext.InitVulkan(windowModule.Window);

vulkanContext.Destroy();

Debug.Log("Success!");
return;

// var engine = new EngineCore();
// engine.Run();

var coreLoop = new CoreLoop();
coreLoop.Run();

coreLoop.Dispose();