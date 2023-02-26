﻿using Engine.Rendering;
using Silk.NET.Windowing;

namespace Engine;

public class EngineCore
{
    public void Run()
    {
        var options = WindowOptions.DefaultVulkan with
        {
            Title = "Vulkan Test",
        };

        var window = Window.Create(options);
        window.Initialize();

        var vulkanContext = new VulkanContext();
        vulkanContext.Initialise(window);

        window.Run();
    }
}