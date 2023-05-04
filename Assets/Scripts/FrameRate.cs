using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace GameU
{
    public class FrameRate : MonoBehaviour
    {
        [SerializeField, Range(0, 240)]
        int targetFPS;

        [SerializeField, Range(1, 10)]
        int frameTimeGraphRange_frames = 3;

        [SerializeField]
        bool showFrameStats;

        [SerializeField]
        Vector2 screenPosition = Vector2.one * 8;

        [SerializeField, Range(0, 100)]
        public int testDelay;

        [SerializeField, Range(0.01f, 10f)]
        float timeScale = 1f;

        private void Awake()
        {
            style.alignment = TextAnchor.UpperLeft;
            historyTexture = new Texture2D(128, 16);
            historyTexture.wrapMode = TextureWrapMode.Clamp;
            history = new Color32[historyTexture.width * historyTexture.height];
            Array.Fill(history, new Color32(0, 0, 0, byte.MaxValue));

            if (targetFPS <= 0)
            {
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = Screen.currentResolution.refreshRate;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = targetFPS;
            }
            print($"Target frame rate {Application.targetFrameRate} FPS, Display refresh rate {Screen.currentResolution.refreshRate} Hz, V-sync ({QualitySettings.vSyncCount})");
        }

        private float _smoothDeltaTime;
        private Color32[] history;
        private Texture2D historyTexture;
        private readonly GUIStyle style = new();

        public void Update()
        {
            Time.timeScale = timeScale;
            if (testDelay > 0)
            {
                System.Threading.Thread.Sleep(testDelay);
            }
        }

        private void LateUpdate()
        {
            _smoothDeltaTime += (Time.unscaledDeltaTime - _smoothDeltaTime) * 0.1f;
            // Copy stat history to the left
            for (int y = 0; y < historyTexture.height; y++)
            {
                for (int x = 0; x < historyTexture.width - 1; x++)
                {
                    int n = x + y * historyTexture.width;
                    history[n] = history[n + 1];
                }
            }
            // Fill column with current stat
            float frameTime = Time.unscaledDeltaTime * 1000f; // ms/f
            float targetFrameTime = 1000f / Application.targetFrameRate; // ms/f
            float frameTimeRange = frameTimeGraphRange_frames * 0.5f * targetFrameTime;
            float deltaFromTarget = frameTime - targetFrameTime;
            float percent = Mathf.Clamp01((deltaFromTarget + frameTimeRange / 2f) / frameTimeRange);
            float cr = Mathf.Clamp01(1f - Mathf.Abs(percent - 1.0f));
            float cg = Mathf.Clamp01(0.5f - 2f * Mathf.Abs(percent - 0.5f));
            float cb = Mathf.Clamp01(1f - Mathf.Abs(percent - 0.0f));
            Color clr = new(cr, cg, cb, 1f);
            float height = (float)(historyTexture.height - 1);
            int samplePoint = Mathf.RoundToInt(percent * height);
            int centerLine = Mathf.RoundToInt(0.5f * height);
            for (int y = 0; y < historyTexture.height; y++)
            {
                int x = historyTexture.width - 1;
                int n = x + y * historyTexture.width;
                if (samplePoint == centerLine && y == centerLine)
                {
                    float q = Mathf.Abs(percent * height - (samplePoint + 0.5f));
                    history[n] = Color.Lerp(Color.grey, Color.yellow, q);
                }
                else if (samplePoint > centerLine && y >= centerLine && y <= samplePoint ||
                         samplePoint < centerLine && y <= centerLine && y >= samplePoint)
                {
                    history[n] = clr;
                }
                else
                {
                    history[n] = Color.black;
                }
            }
            historyTexture.SetPixels32(history);
            historyTexture.Apply();
        }

        void OnGUI()
        {
            if (!showFrameStats) { return; }
            if (!Event.current.type.Equals(EventType.Repaint)) return;

            style.fontSize = Screen.height / 48;
            float msec = _smoothDeltaTime * 1000.0f;
            float fps = 1f / _smoothDeltaTime;
            string text = $"{fps:0.0} FPS ({msec:0.0}ms)";

            // Draw the stats drop shadow
            style.normal.textColor = Color.black;

            var rect = new Rect(screenPosition.x, screenPosition.y, 8 + style.fontSize * text.Length, style.fontSize + 8);
            GUI.Label(rect, text, style);

            rect.position += Vector2.one * 2f;
            GUI.Label(rect, text, style);

            // Draw the stats
            style.normal.textColor = Color.yellow;

            rect.position -= Vector2.one;
            GUI.Label(rect, text, style);

            // Draw the history
            rect.position += Vector2.up * (style.fontSize + 4);
            rect.size = new Vector2(historyTexture.width, historyTexture.height) * 2f;
            GUI.DrawTexture(rect, historyTexture);
        }
    }
}
