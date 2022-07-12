using Silk.NET.Windowing;
using Silk.NET.Windowing.Sdl;

SdlWindowing.RegisterPlatform();
var window = Window.Create(WindowOptions.DefaultVulkan);
window.Run();
