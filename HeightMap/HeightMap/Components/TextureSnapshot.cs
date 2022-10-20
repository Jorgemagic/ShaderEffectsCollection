using Evergine.Common.Graphics;
using Evergine.Common.Input;
using Evergine.Common.Input.Keyboard;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Services;
using System;
using System.IO;

namespace HeightMap.Components
{
    public class TextureSnapshot : Drawable3D
    {
        [BindService]
        protected GraphicsPresenter graphicsPresenter;

        [BindService]
        protected GraphicsContext graphicsContext;

        //protected override unsafe void Update(TimeSpan gameTime)
        public override unsafe void Draw(DrawContext drawContext)
        {
            KeyboardDispatcher keyboardDispatcher = this.graphicsPresenter.FocusedDisplay?.KeyboardDispatcher;

            if (keyboardDispatcher?.ReadKeyState(Keys.Space) == ButtonState.Pressing)
            {
                var frameBuffer = this.Managers.RenderManager.ActiveCamera3D.DrawContext.IntermediateFrameBuffer;
                int width = (int)frameBuffer.Width;
                int height = (int)frameBuffer.Height;

                // Color Staging Texture
                var color = frameBuffer.ColorTargets[0].Texture;

                var colorStaging = color.Description;
                colorStaging.Flags = TextureFlags.None;
                colorStaging.CpuAccess = ResourceCpuAccess.Read;
                colorStaging.Usage = ResourceUsage.Staging;
                var colorStagingTexture = graphicsContext.Factory.CreateTexture(ref colorStaging);


                // Depth Staging Texture
                var depth = frameBuffer.DepthStencilTarget.Value.Texture;

                var depthStaging = depth.Description;
                depthStaging.Flags = TextureFlags.None;
                depthStaging.CpuAccess = ResourceCpuAccess.Read;
                depthStaging.Usage = ResourceUsage.Staging;
                var depthStagingTexture = graphicsContext.Factory.CreateTexture(ref depthStaging);

                // Copy to staging textures
                var queue = graphicsContext.Factory.CreateCommandQueue();
                var command = queue.CommandBuffer();

                command.Begin();
                command.CopyTextureDataTo(color, colorStagingTexture);
                command.CopyTextureDataTo(depth, depthStagingTexture);
                command.End();
                command.Commit();
                queue.Submit();
                queue.WaitIdle();

                // Write color data to file
                var colorMappedResource = graphicsContext.MapMemory(colorStagingTexture, MapMode.Read);

                var colorResourceSpan = new Span<byte>(colorMappedResource.Data.ToPointer(), (int)colorMappedResource.RowPitch * height);
                byte[] colorData = colorResourceSpan.ToArray();
                File.WriteAllBytes("Color.raw", colorData);

                graphicsContext.UnmapMemory(depthStagingTexture);

                // Write depth data to file
                var depthMappedResource = graphicsContext.MapMemory(depthStagingTexture, MapMode.Read);

                var depthResourceSpan = new Span<byte>(depthMappedResource.Data.ToPointer(), (int)depthMappedResource.RowPitch * height);
                byte[] depthData = depthResourceSpan.ToArray();
                File.WriteAllBytes("Depth.raw", depthData);

                graphicsContext.UnmapMemory(depthStagingTexture);

            }
        }
    }
}
