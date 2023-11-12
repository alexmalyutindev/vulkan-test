using Engine;
using EngineCore;
using EngineCore.Rendering.Core;
using Silk.NET.Windowing;


var windowModule = new WindowModule();
windowModule.Init();
var vulkanContext = new VulkanContext();
vulkanContext.InitVulkan(windowModule.Window);
vulkanContext.Render();

windowModule.Window.Run();

vulkanContext.Destroy();

Log.Info("Success!");
return;

// var engine = new EngineCore();
// engine.Run();

var coreLoop = new CoreLoop();
coreLoop.Run();

coreLoop.Dispose();