using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Mathematics;
using Grass.Effects;
using ImGuiNET;
using System;
using System.Linq;

namespace Grass.Components
{
    public class UIComponent : Behavior
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
            ImGui.Begin("Settings");

            bool postprocessing = this.Postprocessing.IsEnabled;
            ImGui.Checkbox("Postprocessing Enabled", ref postprocessing);
            this.Postprocessing.IsEnabled = postprocessing;

            ImGui.Separator();

            // Width segments
            int widthSegments = meshPlane.WidthSegments;
            ImGui.SliderInt("Width Segments", ref widthSegments, 1, 100);
            meshPlane.WidthSegments = widthSegments;

            // Height segments
            int heightSegments = meshPlane.HeightSegments;
            ImGui.SliderInt("Height Segments", ref heightSegments, 1, 100);
            meshPlane.HeightSegments = heightSegments;

            ImGui.Separator();           

            // Blade Width
            float bladeWidth = this.grassMaterial.Parameters_BladeWidth;
            ImGui.SliderFloat("Blade Width", ref bladeWidth, 0.0f, 0.02f);
            this.grassMaterial.Parameters_BladeWidth = bladeWidth;

            // Blade Width Random
            float bladeWidthRandom = this.grassMaterial.Parameters_BladeWidthRandom;
            ImGui.SliderFloat("Blade Width Random", ref bladeWidthRandom, 0.0f, 0.01f);
            this.grassMaterial.Parameters_BladeWidthRandom = bladeWidthRandom;

            // Blade Height
            float bladeHeight = this.grassMaterial.Parameters_BladeHeight;
            ImGui.SliderFloat("Blade Height", ref bladeHeight, 0.0f, 0.1f);
            this.grassMaterial.Parameters_BladeHeight = bladeHeight;

            // Blade Height Random
            float bladeHeightRandom = this.grassMaterial.Parameters_BladeHeightRandom;
            ImGui.SliderFloat("Blade Height Random", ref bladeHeightRandom, 0.0f, 0.05f);
            this.grassMaterial.Parameters_BladeHeightRandom = bladeHeightRandom;

            ImGui.Separator();

            // BendRotation Random
            float bend = this.grassMaterial.Parameters_BendRotationRandom;
            ImGui.SliderFloat("Bend Rotation Random", ref bend, 0.0f, 1.0f);
            this.grassMaterial.Parameters_BendRotationRandom = bend;

            // Blade Forward
            float bladeForward = this.grassMaterial.Parameters_BladeForward;
            ImGui.SliderFloat("Blade Forward", ref bladeForward, 0.0f, 0.1f);
            this.grassMaterial.Parameters_BladeForward = bladeForward;

            // Blade Curvature
            float bladeCurvature = this.grassMaterial.Parameters_BladeCurvature;
            ImGui.SliderFloat("Blade Curvature", ref bladeCurvature, 0.0f, 2.0f);
            this.grassMaterial.Parameters_BladeCurvature =  bladeCurvature;

            ImGui.Separator();

            // Wind Frequency
            this.grassMaterial.Parameters_WindFrenquency = SliderVector2("Wind Frequency", this.grassMaterial.Parameters_WindFrenquency, 0.0f, 0.1f);

            // Wind Strength
            float windStrength = this.grassMaterial.Parameters_WindStrength;
            ImGui.SliderFloat("Wind Strength", ref windStrength, 0.0f, 0.1f);
            this.grassMaterial.Parameters_WindStrength = windStrength;            

            ImGui.End();
        }

        private Vector2 SliderVector2(string name, Vector2 v, float min, float max)
        {
            System.Numerics.Vector2 av = new System.Numerics.Vector2(v.X, v.Y);
            ImGui.SliderFloat2(name, ref av, min, max);

            return new Vector2(av.X, av.Y);
        }
    }
}