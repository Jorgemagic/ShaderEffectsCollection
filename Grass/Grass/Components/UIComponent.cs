using Evergine.Bindings.Imgui;
using Evergine.Common.Graphics;
using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Mathematics;
using Evergine.UI;
using Grass.Effects;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Grass.Components
{
    public unsafe class UIComponent : Behavior
    {       
        [BindComponent]
        private SingleMeshPlane meshPlane;

        [BindComponent]
        private MaterialComponent materialComponent;

        private GrassEffect grassMaterial;

        private Entity Postprocessing;

        protected override bool OnAttached()
        {
            var ok = base.OnAttached();

            this.grassMaterial = new GrassEffect(this.materialComponent.Material);

            this.Postprocessing = this.Managers.EntityManager.FindAllByTag("Postprocessing").First();

            return ok;
        }

        protected override void Update(TimeSpan gameTime)
        {
            bool open = false;
            ImguiNative.igBegin("Settings", open.Pointer(), ImGuiWindowFlags.None);

            bool postprocessing = this.Postprocessing.IsEnabled;            
            ImguiNative.igCheckbox("PostProcessing Enabled", postprocessing.Pointer());
            this.Postprocessing.IsEnabled = postprocessing;
            
            ImguiNative.igSeparator();

            this.grassMaterial.Parameters_TopColor = this.ColorPicker("Blade Top", this.grassMaterial.Parameters_TopColor);
            this.grassMaterial.Parameters_BottomColor = this.ColorPicker("Blade Bottom", this.grassMaterial.Parameters_BottomColor);
            
            ImguiNative.igSeparator();

            // Width segments
            int widthSegments = meshPlane.WidthSegments;
            ImguiNative.igSliderInt("Width Segments", &widthSegments, 1, 100, string.Empty, ImGuiSliderFlags.None);
            meshPlane.WidthSegments = widthSegments;

            // Height segments
            int heightSegments = meshPlane.HeightSegments;            
            ImguiNative.igSliderInt("Height Segments", &heightSegments, 1, 100, string.Empty, ImGuiSliderFlags.None);
            meshPlane.HeightSegments = heightSegments;
            
            ImguiNative.igSeparator();

            // Blade Width
            float bladeWidth = this.grassMaterial.Parameters_BladeWidth;
            ImguiNative.igSliderFloat("Blade Width", &bladeWidth, 0.0f, 0.02f, string.Empty, ImGuiSliderFlags.None);
            this.grassMaterial.Parameters_BladeWidth = bladeWidth;

            // Blade Width Random
            float bladeWidthRandom = this.grassMaterial.Parameters_BladeWidthRandom;
            ImguiNative.igSliderFloat("Blade Width Random", &bladeWidthRandom, 0.0f, 0.01f, string.Empty, ImGuiSliderFlags.None);
            this.grassMaterial.Parameters_BladeWidthRandom = bladeWidthRandom;

            // Blade Height
            float bladeHeight = this.grassMaterial.Parameters_BladeHeight;            
            ImguiNative.igSliderFloat("Blade Height", &bladeHeight, 0.0f, 0.1f, string.Empty, ImGuiSliderFlags.None);
            this.grassMaterial.Parameters_BladeHeight = bladeHeight;

            // Blade Height Random
            float bladeHeightRandom = this.grassMaterial.Parameters_BladeHeightRandom;            
            ImguiNative.igSliderFloat("Blade Height Random", &bladeHeightRandom, 0.0f, 0.05f, string.Empty, ImGuiSliderFlags.None);
            this.grassMaterial.Parameters_BladeHeightRandom = bladeHeightRandom;

            ImguiNative.igSeparator();

            // BendRotation Random
            float bend = this.grassMaterial.Parameters_BendRotationRandom;
            ImguiNative.igSliderFloat("Bend Rotation Random", &bend, 0.0f, 1.0f, string.Empty, ImGuiSliderFlags.None);
            this.grassMaterial.Parameters_BendRotationRandom = bend;

            // Blade Forward
            float bladeForward = this.grassMaterial.Parameters_BladeForward;
            ImguiNative.igSliderFloat("Blade Forward", &bladeForward, -0.1f, 0.1f, string.Empty, ImGuiSliderFlags.None);
            this.grassMaterial.Parameters_BladeForward = bladeForward;

            // Blade Curvature
            float bladeCurvature = this.grassMaterial.Parameters_BladeCurvature;            
            ImguiNative.igSliderFloat("Blade Curvature", &bladeCurvature, 0.0f, 2.0f, string.Empty, ImGuiSliderFlags.None);
            this.grassMaterial.Parameters_BladeCurvature =  bladeCurvature;

            ImguiNative.igSeparator();

            // Wind Frequency
            Vector2 windFrenquency = this.grassMaterial.Parameters_WindFrenquency;
            ImguiNative.igSliderFloat2("Wind Frequency", &windFrenquency, 0.0f, 0.1f, string.Empty, ImGuiSliderFlags.None);
            this.grassMaterial.Parameters_WindFrenquency = windFrenquency;

            // Wind Strength
            float windStrength = this.grassMaterial.Parameters_WindStrength;
            ImguiNative.igSliderFloat("Wind Strength", &windStrength, 0.0f, 0.1f, string.Empty, ImGuiSliderFlags.None);
            this.grassMaterial.Parameters_WindStrength = windStrength;            
            
            ImguiNative.igEnd();
        }

        private Color ColorPicker(string name, Color c)
        {
            var v = c.ToVector3();
            ImguiNative.igColorEdit3(name, &v, ImGuiColorEditFlags.None);
            return Color.FromVector3(ref v);
        }
    }
}