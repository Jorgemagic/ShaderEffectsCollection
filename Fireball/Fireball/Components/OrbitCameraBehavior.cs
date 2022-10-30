using Evergine.Common.Input.Mouse;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Mathematics;
using System;
using System.Linq;

namespace Fireball.Components
{
    public class OrbitCameraBehavior : Behavior
    {
        [Flags]
        public enum Modes
        {
            Orbit = 0x1,
            Zoom = 0x2,
            Pan = 0x04,
        }

        /// <summary>
        /// The half pi.
        /// </summary>
        private static readonly float halfPi = (float)Math.PI * 0.5f;

        /// <summary>
        /// The camera to move.
        /// </summary>
        [BindComponent(false)]
        public Transform3D Transform = null;

        /// <summary>
        /// The child transform.
        /// </summary>
        private Transform3D childTransform;

        /// <summary>
        /// The camera transform.
        /// </summary>
        private Transform3D cameraTransform;

        /// <summary>
        /// True if the camera Is Dragging.
        /// </summary>
        private bool isRotating;

        /// <summary>
        /// The orbit_scale.
        /// </summary>
        private const float OrbitScale = 0.005f;

        /// <summary>
        /// The last mouse position.
        /// </summary>
        private Vector2 lastMousePosition;

        /// <summary>
        /// The theta.
        /// </summary>
        private float theta;

        /// <summary>
        /// The phi.
        /// </summary>
        private float phi;

        /// <summary>
        /// The is dirty.
        /// </summary>
        private bool isDirty;

        /// <summary>
        /// The current mouse state.
        /// </summary>
        private Point currentMouseState;
        private bool isPanning;
        private Vector3 panInitialPosition;
        private Vector3 cameraInitialPosition;
        private Modes currentMode;
        private MouseDispatcher mouseDispatcher;

        public OrbitCameraBehavior()
        {
            this.currentMode = Modes.Orbit;
        }

        /// <inheritdoc/>
        protected override void OnLoaded()
        {
            base.OnLoaded();

            this.phi = 0;
            this.theta = 0;

            this.isRotating = false;
            this.isPanning = false;

            this.isDirty = true;
        }

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            var child = this.Owner.ChildEntities.First();
            this.childTransform = child.FindComponent<Transform3D>();
            this.cameraTransform = child.ChildEntities.First().FindComponent<Transform3D>();

            this.cameraInitialPosition = this.cameraTransform.LocalPosition;

            return base.OnAttached();
        }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();

            this.mouseDispatcher = this.Owner.Scene.Managers.RenderManager.ActiveCamera3D?.Display?.MouseDispatcher;
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            this.HandleInput();

            if (this.isDirty)
            {
                this.CommitChanges();
                this.isDirty = false;
            }
        }

        /// <summary>
        /// Handles the input.
        /// </summary>
        private void HandleInput()
        {
            if (this.mouseDispatcher == null)
            {
                return;
            }

            // Orbit
            if (this.currentMode.HasFlag(Modes.Orbit))
            {
                if (this.mouseDispatcher.IsButtonDown(Evergine.Common.Input.Mouse.MouseButtons.Left))
                {
                    this.currentMouseState = this.mouseDispatcher.Position;

                    if (this.isRotating == false)
                    {
                        this.isRotating = true;
                    }
                    else
                    {
                        Vector2 delta = Vector2.Zero;

                        delta.X = -this.currentMouseState.X + this.lastMousePosition.X;
                        delta.Y = this.currentMouseState.Y - this.lastMousePosition.Y;

                        delta = -delta;
                        this.Orbit(delta * OrbitScale);
                    }

                    this.lastMousePosition.X = this.currentMouseState.X;
                    this.lastMousePosition.Y = this.currentMouseState.Y;
                }
                else
                {
                    this.isRotating = false;
                }
            }

            // Pan
            if (this.currentMode.HasFlag(Modes.Pan))
            {
                if (this.mouseDispatcher.IsButtonDown(Evergine.Common.Input.Mouse.MouseButtons.Middle))
                {
                    this.currentMouseState = this.mouseDispatcher.Position;

                    if (this.isPanning == false)
                    {
                        this.isPanning = true;
                        this.panInitialPosition = this.Transform.LocalPosition;
                    }
                    else
                    {
                        Vector2 delta = Vector2.Zero;
                        delta.X = -this.currentMouseState.X + this.lastMousePosition.X;
                        delta.Y = this.currentMouseState.Y - this.lastMousePosition.Y;
                        delta /= 100.0f;

                        this.Transform.LocalPosition += (this.cameraTransform.WorldTransform.Right * delta.X) + (this.cameraTransform.WorldTransform.Up * delta.Y);
                    }

                    this.lastMousePosition.X = this.currentMouseState.X;
                    this.lastMousePosition.Y = this.currentMouseState.Y;
                }
                else
                {
                    this.isPanning = false;
                }
            }

            // Zoom
            if (this.currentMode.HasFlag(Modes.Zoom))
            {
                if (this.mouseDispatcher.ScrollDelta.Y != 0)
                {
                    Vector3 direction = this.Transform.LocalPosition - this.cameraTransform.Position;
                    Vector3 increment;
                    if (direction.Length() < 5)
                    {
                        direction.Normalize();
                        increment = direction * MathHelper.Clamp((float)this.mouseDispatcher.ScrollDelta.Y, -1, 1);
                    }
                    else
                    {
                        direction.Normalize();
                        increment = direction * (float)this.mouseDispatcher.ScrollDelta.Y;
                    }

                    Vector3 desiredPosition = this.cameraTransform.Position + increment;
                    Vector3 desiredDirection = this.Transform.LocalPosition - desiredPosition;
                    if (desiredDirection.Length() > 1.5f)
                    {
                        this.cameraTransform.Position = desiredPosition;
                    }
                }
            }
        }

        /// <summary>
        /// Orbits the specified delta.
        /// </summary>
        /// <param name="delta">The delta.</param>
        public void Orbit(Vector2 delta)
        {
            this.theta += delta.X;
            this.phi += delta.Y;

            if (this.phi > halfPi)
            {
                this.phi = halfPi;
            }
            else if (this.phi < -halfPi)
            {
                this.phi = -halfPi;
            }

            this.isDirty = true;
        }

        /// <summary>
        /// Commits the changes.
        /// </summary>
        public void CommitChanges()
        {
            var rotation = this.Transform.LocalRotation;
            rotation.Y = -this.theta;

            this.Transform.LocalRotation = rotation;

            var childRotation = this.childTransform.LocalRotation;
            childRotation.X = this.phi;
            this.childTransform.LocalRotation = childRotation;
        }

        /// <summary>
        /// Reset camera position.
        /// </summary>
        public void Reset()
        {
            this.cameraTransform.LocalPosition = this.cameraInitialPosition;
            this.cameraTransform.LocalLookAt(Vector3.Zero, Vector3.Up);
            this.childTransform.LocalPosition = Vector3.Zero;
            this.childTransform.LocalRotation = Vector3.Zero;
            this.Transform.LocalPosition = Vector3.Zero;
            this.Transform.LocalRotation = Vector3.Zero;

            this.phi = 0;
            this.theta = 0;

            this.isRotating = false;
            this.isPanning = false;
        }
    }
}

