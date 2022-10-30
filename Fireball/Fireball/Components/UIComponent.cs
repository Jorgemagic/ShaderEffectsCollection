using Evergine.Bindings.Imgui;
using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Services;
using Evergine.Platform;
using Evergine.UI;
using System;

namespace Fireball.Components
{
    public unsafe class UIComponent : Behavior
    {
        [BindService]
        private AssetsService assetsService = null;

        [BindComponent]
        private MaterialComponent materialComponent = null;

        private Material explosionMaterial;
        private Material WireframeMaterial;        

        private int mode = 0;
        private int lastMode = 0;

        protected override bool OnAttached()
        {
            var ok = base.OnAttached();

            this.explosionMaterial = this.assetsService.Load<Material>(EvergineContent.Materials.FireballMat);
            this.WireframeMaterial = this.assetsService.Load<Material>(EvergineContent.Materials.WireframeMat);            

            return ok;
        }

        protected override void Update(TimeSpan gameTime)
        {
            if (DeviceInfo.PlatformType == Evergine.Common.PlatformType.Web)
            {
                return;
            }

            bool open = false;
            ImguiNative.igBegin("Settings", open.Pointer(), ImGuiWindowFlags.None);

            // Mode
            string[] elements = new string[] { "Shading", "Wireframe" };
            int currentMode = this.mode;
            ImguiNative.igCombo_Str_arr("Mode", &currentMode, elements, elements.Length, 20);
            this.mode = currentMode;

            if (this.lastMode != mode)
            {
                switch (mode)
                {
                    case 1:
                        this.materialComponent.Material = this.WireframeMaterial;
                        break;
                    case 0: // 
                    default:
                        this.materialComponent.Material = this.explosionMaterial;
                        break;
                                          
                }
                this.lastMode = mode;
            }

            ImguiNative.igEnd();
        }
    }
}