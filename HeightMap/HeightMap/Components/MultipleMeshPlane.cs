using Evergine.Common.Attributes;
using Evergine.Common.Graphics;
using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Mathematics;
using System;
using System.Collections.Generic;

namespace HeightMap.Components
{
    public class MultipleMeshPlane : MeshComponent
    {
        [BindService]
        private GraphicsContext graphicsContext = null;

        protected List<Mesh> meshCollection = new List<Mesh>();        

        protected int widthSegments;
        protected int heightSegments;
        protected Random random = new Random();

        [RenderPropertyAsInput(1, 500, AsSlider = true, DesiredChange = 1, DesiredLargeChange = 2, DefaultValue = 1)]
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

        [RenderPropertyAsInput(1, 500, AsSlider = true, DesiredChange = 1, DesiredLargeChange = 2, DefaultValue = 1)]
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

        [DontRenderProperty]
        public new Model Model
        {
            get => this.modelLink.Value;

            private set
            {
                this.modelLink.Value = value;

                this.ThrowRefreshEvent();
            }
        }

        [DontRenderProperty]
        public new string ModelMeshName => this.modelMeshName;

        public MultipleMeshPlane()
        {
            this.widthSegments = 1;
            this.heightSegments = 1;
        }

        protected int GetPrimitiveHashCode()
        {
            int hashCode = 934837179;
            hashCode = (hashCode * -1521134295) + this.widthSegments.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.HeightSegments.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            this.Build();

            return true;
        }

        protected void NotifyPropertyChange()
        {
            if (this.State == AttachableObjectState.Activated)
            {
                this.UnloadModel();

                this.Build();
                this.ThrowRefreshEvent();
            }
        }


        protected void Build()
        {
            this.meshCollection.Clear();
            List<MeshBuilder> meshBuilders = new List<MeshBuilder>();

            Color meshColor = Color.Black;
            float wCeiling = (float)Math.Ceiling(this.widthSegments / 100.0f);
            for (int i = 0; i < wCeiling; i++)
            {
                float hCeiling = (float)Math.Ceiling(this.heightSegments / 100.0f);
                int quadWIndex = i * 100;
                int quadWRest = quadWIndex + 100;
                if (i + 1 == wCeiling)
                {
                    if (widthSegments % 100 != 0)
                    {
                        quadWRest = quadWIndex + (widthSegments % 100);
                    }
                }

                for (int j = 0; j < hCeiling; j++)
                {
                    MeshBuilder builder = new MeshBuilder();
                    meshBuilders.Add(builder);
                    int index = 0;

                    meshColor = new Color((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), 1.0f);                   

                    for (int w = quadWIndex; w < quadWRest; w++)
                    {
                        float widthStep = w * (1.0f / widthSegments);
                        float widthNextStep = ((w + 1) * (1.0f / widthSegments));

                        int quadHIndex = j * 100;
                        int quadHRest = quadHIndex + 100;
                        if (j + 1 == hCeiling)
                        {
                            if (heightSegments % 100 != 0)
                            {
                                quadHRest = quadHIndex + (heightSegments % 100);
                            }
                        }

                        for (int h = quadHIndex; h < quadHRest; h++)
                        {
                            float heightStep = h * (1.0f / heightSegments);
                            float heightNextStep = ((h + 1) * (1.0f / heightSegments));

                            // First Triangle
                            builder.AddVertex(
                                              new Vector3(-0.5f + widthStep, 0, -0.5f + heightStep), // Position
                                              Vector3.Up, // Normal,
                                              Vector3.Zero, // Tangent
                                              new Vector2(widthStep, heightStep), // UV
                                              meshColor// Color
                                              );


                            builder.AddVertex(
                                              new Vector3(-0.5f + widthNextStep, 0, -0.5f + heightNextStep), // Position
                                              Vector3.Up, // Normal,
                                              Vector3.Zero, // Tangent
                                              new Vector2(widthNextStep, heightNextStep), // UV
                                              meshColor // Color
                                              );

                            builder.AddVertex(
                                              new Vector3(-0.5f + widthStep, 0, -0.5f + heightNextStep), // Position
                                              Vector3.Up, // Normal,
                                              Vector3.Zero, // Tangent
                                              new Vector2(widthStep, heightNextStep), // UV
                                              meshColor // Color
                                              );

                            builder.AddIndex(index++);
                            builder.AddIndex(index++);
                            builder.AddIndex(index++);

                            // Second Triangle
                            builder.AddVertex(
                                              new Vector3(-0.5f + widthStep, 0, -0.5f + heightStep), // Position
                                              Vector3.Up, // Normal,
                                              Vector3.Zero, // Tangent
                                              new Vector2(widthStep, heightStep), // UV
                                              meshColor // Color
                                              );

                            builder.AddVertex(
                                              new Vector3(-0.5f + widthNextStep, 0, -0.5f + heightStep), // Position
                                              Vector3.Up, // Normal,
                                              Vector3.Zero, // Tangent
                                              new Vector2(widthNextStep, heightStep), // UV
                                              meshColor // Color
                                              );

                            builder.AddVertex(
                                             new Vector3(-0.5f + widthNextStep, 0, -0.5f + heightNextStep), // Position
                                             Vector3.Up, // Normal,
                                             Vector3.Zero, // Tangent
                                             new Vector2(widthNextStep, heightNextStep), // UV
                                             meshColor // Color
                                             );

                            builder.AddIndex(index++);
                            builder.AddIndex(index++);
                            builder.AddIndex(index++);
                        }
                    }

                    var mesh = builder.CreateMesh(graphicsContext, PrimitiveTopology.TriangleList);
                    this.meshCollection.Add(mesh);
                }
            }

            // Create MeshContainer
            var meshContainer = new MeshContainer()
            {
                Name = "MultiplesMeshes",
                Meshes = this.meshCollection,
            };

            // Create RootNode
            var rootNode = new NodeContent()
            {
                Name = "MultiplesMeshes",
                Mesh = meshContainer,
                Children = new NodeContent[0],
                ChildIndices = new int[0],
            };

            // Create material List
            var materialCollection = new List<(string, System.Guid)>()
            {
                ("Default", Guid.Empty),
            };

            this.Model = new Model()
            {
                MeshContainers = new[] { meshContainer },
                Materials = materialCollection,
                AllNodes = new[] { rootNode },
                RootNodes = new[] { 0 },
            };

            this.Model.RefreshBoundingBox();

            foreach(MeshBuilder meshBuilder in meshBuilders)
            {
                meshBuilder.Clear();
            }
        }
    }
}
