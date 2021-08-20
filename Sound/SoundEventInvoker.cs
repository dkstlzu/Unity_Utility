using UnityEngine;

namespace Utility
{
    public class SoundEventInvoker : MonoBehaviour
    {
        public SoundEventArgs Args;

        public virtual void Invoke()
        {
            SoundEvent.soundEvent.Invoke(Args);
        }
    }
}