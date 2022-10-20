using Evergine.Bindings.Imgui;
using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Services;
using Evergine.UI;
using System;

namespace HeightMap.Components
{
    public unsafe class UIComponent : Behavior
    {
        [BindService]
        private AssetsService assetsService;

        [BindComponent]
        private MultipleMeshPlane meshPlane;

        [BindComponent]
        private MaterialComponent materialComponent;

        private Material MeshesMaterial;
        private Material WireframeMaterial;
        private Material NormalsMaterial;
        private Material HeightMaterial;

        private int mode = 2;
        private int lastMode = 2;

        protected override bool OnAttached()
        {
            var ok = base.OnAttached();

            this.MeshesMaterial = this.assetsService.Load<Material>(EvergineContent.Materials.DebugMat);            
            this.NormalsMaterial = this.assetsService.Load<Material>(EvergineContent.Materials.Normals);
            this.HeightMaterial = this.assetsService.Load<Material>(EvergineContent.Materials.HeightMat);

            return ok;
        }

        protected override void Update(TimeSpan gameTime)
        {            
            bool open = false;
            ImguiNative.igBegin("Settings", open.Pointer(), ImGuiWindowFlags.None);

            // Mode
            string[] elements = new string[] { "Meshes", "Normals", "Shading" };
            int currentMode = this.mode;
            ImguiNative.igCombo_Str_arr("Mode", &currentMode, elements, elements.Length, 20);
            this.mode = currentMode;

            if (this.lastMode != mode)
            {
                switch (mode)
                {
                    case 0: // 
                        this.materialComponent.Material = this.MeshesMaterial;
                        break;
                    case 1:
                        this.materialComponent.Material = this.NormalsMaterial;
                        break;
                    case 2:                        
                    default:
                        this.materialComponent.Material = this.HeightMaterial;
                        break;
                }
                this.lastMode = mode;
            }

            // Width segments
            int widthSegments = meshPlane.WidthSegments;
            ImguiNative.igSliderInt("Width Segments", &widthSegments, 1, 500, string.Empty, ImGuiSliderFlags.None);
            meshPlane.WidthSegments = widthSegments;

            // Height segments
            int heightSegments = meshPlane.HeightSegments;
            ImguiNative.igSliderInt("Height Segments", &heightSegments, 1, 500, string.Empty, ImGuiSliderFlags.None);
            meshPlane.HeightSegments = heightSegments;

            ImguiNative.igEnd();
        }
    }
}
