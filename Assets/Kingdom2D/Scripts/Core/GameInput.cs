using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace Kingdom2D
{
    /// <summary>
    /// Girdi katmanı. Projede eski Input Manager da yeni Input System da
    /// etkin olsa çalışır (derleme sembolleriyle otomatik seçilir).
    /// </summary>
    public static class GameInput
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        static bool Key(Key k)
        {
            var kb = Keyboard.current;
            return kb != null && kb[k].isPressed;
        }

        static bool KeyDown(Key k)
        {
            var kb = Keyboard.current;
            return kb != null && kb[k].wasPressedThisFrame;
        }

        public static float Horizontal
        {
            get
            {
                float v = 0f;
                if (Key(Key.A) || Key(Key.LeftArrow)) v -= 1f;
                if (Key(Key.D) || Key(Key.RightArrow)) v += 1f;
                var gp = Gamepad.current;
                if (Mathf.Abs(v) < 0.1f && gp != null)
                {
                    float gx = gp.leftStick.ReadValue().x;
                    if (Mathf.Abs(gx) > 0.2f) v = gx;
                    if (gp.dpad.left.isPressed) v = -1f;
                    if (gp.dpad.right.isPressed) v = 1f;
                }
                return Mathf.Clamp(v, -1f, 1f);
            }
        }

        public static bool PayHeld
        {
            get
            {
                var gp = Gamepad.current;
                bool pad = gp != null && (gp.buttonSouth.isPressed || gp.rightTrigger.isPressed);
                return Key(Key.Space) || Key(Key.S) || Key(Key.DownArrow) || pad;
            }
        }

        public static bool RestartPressed { get { return KeyDown(Key.R); } }
        public static bool PausePressed { get { return KeyDown(Key.P); } }
        public static bool QuitPressed { get { return KeyDown(Key.Escape); } }
#else
        public static float Horizontal
        {
            get
            {
                float v = 0f;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) v -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) v += 1f;
                if (Mathf.Abs(v) < 0.1f)
                {
                    float gx = Input.GetAxisRaw("Horizontal");
                    if (Mathf.Abs(gx) > 0.2f) v = gx;
                }
                return Mathf.Clamp(v, -1f, 1f);
            }
        }

        public static bool PayHeld
        {
            get
            {
                return Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.S) ||
                       Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.JoystickButton0);
            }
        }

        public static bool RestartPressed { get { return Input.GetKeyDown(KeyCode.R); } }
        public static bool PausePressed { get { return Input.GetKeyDown(KeyCode.P); } }
        public static bool QuitPressed { get { return Input.GetKeyDown(KeyCode.Escape); } }
#endif
    }
}
