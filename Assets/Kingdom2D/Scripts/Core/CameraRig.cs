using UnityEngine;

namespace Kingdom2D
{
    /// <summary>Kralı yumuşak takip eden, dünya sınırlarına kilitlenen kamera.</summary>
    public class CameraRig : MonoBehaviour
    {
        GameConfig cfg;
        Camera cam;
        Transform target;
        float shake;
        float lastTargetX;

        public Camera Cam { get { return cam; } }

        public void Init(GameConfig config, Camera camera, Transform follow)
        {
            cfg = config;
            cam = camera;
            target = follow;
            cam.orthographic = true;
            cam.orthographicSize = cfg.cameraSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Pal.SkyNightTop;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 100f;
            if (target != null) lastTargetX = target.position.x;
            transform.position = new Vector3(ClampX(lastTargetX), cfg.cameraHeight, -10f);
        }

        public void Shake(float amount)
        {
            shake = Mathf.Min(0.6f, shake + amount);
        }

        public float HalfWidth
        {
            get { return cam != null ? cam.orthographicSize * cam.aspect : 10f; }
        }

        float ClampX(float x)
        {
            float limit = Mathf.Max(0f, cfg.worldHalfWidth - HalfWidth);
            return Mathf.Clamp(x, -limit, limit);
        }

        void LateUpdate()
        {
            if (cfg == null || cam == null) return;

            float tx = lastTargetX;
            if (target != null)
            {
                float vel = (target.position.x - lastTargetX) / Mathf.Max(0.0001f, Time.deltaTime);
                lastTargetX = target.position.x;
                tx = target.position.x + Mathf.Clamp(vel, -cfg.kingSpeed, cfg.kingSpeed) / cfg.kingSpeed * cfg.cameraLookAhead;
            }

            float x = Mathf.Lerp(transform.position.x, ClampX(tx), 1f - Mathf.Exp(-cfg.cameraLerp * Time.deltaTime));
            float y = cfg.cameraHeight;

            if (shake > 0.0005f)
            {
                x += Random.Range(-shake, shake);
                y += Random.Range(-shake, shake);
                shake = Mathf.Lerp(shake, 0f, 8f * Time.deltaTime);
            }

            transform.position = new Vector3(x, y, -10f);
        }
    }
}
