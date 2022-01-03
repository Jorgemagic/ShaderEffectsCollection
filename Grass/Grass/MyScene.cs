using Evergine.Framework;
using Evergine.UI;

namespace Grass
{
    public class MyScene : Scene
    {
        public override void RegisterManagers()
        {
            this.Managers.AddManager(new ImGuiManager());
            base.RegisterManagers();
            this.Managers.AddManager(new Evergine.Bullet.BulletPhysicManager3D());
        }

        protected override void CreateScene()
        {
        }
    }
}