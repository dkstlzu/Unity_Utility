using UnityEngine;

namespace dkstlzu.Utility
{
    [CreateAssetMenu(fileName = "FrameDevToolConfig", menuName = "FrameAnimation/Dev Tool Setting", order = 10)]
    public class FrameAnimatorEditorWindowConfigScriptable : ScriptableSingleton<FrameAnimatorEditorWindowConfigScriptable>
    {
        public Sprite NoSpriteIndicator;
        
        public int BodyBoxPoolSize = 5;
        public int HitBoxPoolSize = 5;
        public int AttackBoxPoolSize = 5;
        
        public Color BodyBoxColor = Color.green;
        public Color HitBoxColor = Color.blue;
        public Color AttackBoxColor = Color.red;
        
        public int AnimationFPS = 6;
        public float AutoSaveInterval = 30;
    }
}