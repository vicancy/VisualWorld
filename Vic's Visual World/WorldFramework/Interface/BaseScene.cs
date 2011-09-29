using System;

namespace WorldFramework.Interface
{
    //basic interface for scenes 
    public abstract class BaseScene : IObject
    {
        public bool IsInitialized { get; set; }

        public BaseScene()
        {
            IsInitialized = false;
        }

        public virtual void Initialize()
        {
            IsInitialized = true;
        }

        public virtual void Draw()
        {
        }

        public virtual void Destroy()
        {
        }
    }
}
