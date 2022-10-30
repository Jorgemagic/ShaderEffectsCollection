using Evergine.Framework;
using Evergine.Platform;
using Evergine.UI;

namespace Fireball
{
    public class MyScene : Scene
    {
        public override void RegisterManagers()
        {
            base.RegisterManagers();
            this.Managers.AddManager(new global::Evergine.Bullet.BulletPhysicManager3D());
            if (DeviceInfo.PlatformType != Evergine.Common.PlatformType.Web)
            {
                this.Managers.AddManager(new ImGuiManager());
            }
        }

        protected override void CreateScene()
        {
        }
    }
}