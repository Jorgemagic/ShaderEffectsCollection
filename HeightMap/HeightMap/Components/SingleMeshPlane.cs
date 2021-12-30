using Evergine.Common.Attributes;
using Evergine.Components.Graphics3D;
using Evergine.Components.Primitives;
using Evergine.Mathematics;
using System;

namespace HeightMap.Components
{
    public class SingleMeshPlane : PrimitiveBaseMesh
    {
        private int widthSegments;
        private int heightSegments;

        [RenderPropertyAsInput(1, 100, AsSlider = true, DesiredChange = 1, DesiredLargeChange = 2, DefaultValue = 1)]
        public int WidthSegments
        {
            get => widthSegments;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.WidthSegments));
                }

                this.widthSegments = value;
                this.NotifyPropertyChange();
            }
        }

        [RenderPropertyAsInput(1, 100, AsSlider = true, DesiredChange = 1, DesiredLargeChange = 2, DefaultValue = 1)]
        public int HeightSegments
        {
            get => heightSegments;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.HeightSegments));
                }

                this.heightSegments = value;
                this.NotifyPropertyChange();
            }
        }

        public SingleMeshPlane()
        {
            this.widthSegments = 1;
            this.HeightSegments = 1;
        }

        protected override int GetPrimitiveHashCode()
        {
            int hashCode = 934837179;
            hashCode = (hashCode * -1521134295) + this.widthSegments.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.HeightSegments.GetHashCode();
            return hashCode;
        }

        protected override void Build(PrimitiveModelBuilder builder)
        {
            int index = 0;
            for (int w = 0; w < this.widthSegments; w++)
            {
                for (int h = 0; h < this.HeightSegments; h++)
                {
                    float widthStep = w * (1.0f / widthSegments);
                    float widthNextStep = ((w + 1) * (1.0f / widthSegments));
                    float heightStep = h * (1.0f / heightSegments);
                    float heightNextStep = ((h + 1) * (1.0f / heightSegments));

                    // First Triangle
                    builder.AddVertex(
                                      new Vector3(-0.5f + widthStep, 0, -0.5f + heightStep), // Position
                                      Vector3.Up, // Normal,
                                      new Vector2(widthStep, heightStep) // UV
                                      );


                    builder.AddVertex(
                                      new Vector3(-0.5f + widthNextStep, 0, -0.5f + heightNextStep), // Position
                                      Vector3.Up, // Normal,
                                      new Vector2(widthNextStep, heightNextStep) // UV
                                      );

                    builder.AddVertex(
                                      new Vector3(-0.5f + widthStep, 0, -0.5f + heightNextStep), // Position
                                      Vector3.Up, // Normal,
                                      new Vector2(widthStep, heightNextStep) // UV
                                      );

                    builder.AddIndex(index++);
                    builder.AddIndex(index++);
                    builder.AddIndex(index++);

                    // Second Triangle
                    builder.AddVertex(
                                      new Vector3(-0.5f + widthStep, 0, -0.5f + heightStep), // Position
                                      Vector3.Up, // Normal,
                                      new Vector2(widthStep, heightStep) // UV
                                      );

                    builder.AddVertex(
                                      new Vector3(-0.5f + widthNextStep, 0, -0.5f + heightStep), // Position
                                      Vector3.Up, // Normal,
                                      new Vector2(widthNextStep, heightStep) // UV
                                      );

                    builder.AddVertex(
                                     new Vector3(-0.5f + widthNextStep, 0, -0.5f + heightNextStep), // Position
                                     Vector3.Up, // Normal,
                                     new Vector2(widthNextStep, heightNextStep) // UV
                                     );

                    builder.AddIndex(index++);
                    builder.AddIndex(index++);
                    builder.AddIndex(index++);
                }
            }
        }
    }
}