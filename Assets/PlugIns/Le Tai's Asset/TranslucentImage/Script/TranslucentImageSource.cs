using System;
using UnityEditor;
using UnityEngine;

namespace LeTai.Asset.TranslucentImage
{
    /// <summary>
    ///     Common source of blur for Translucent Images.
    /// </summary>
    /// <remarks>
    ///     It is an Image effect that blur the render target of the Camera it attached to, then save the result to a global
    ///     read-only  Render Texture
    /// </remarks>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Tai Le Assets/Translucent Image Source")]
    [HelpURL("http://leloctai.com/asset/translucentimage/docs/articles/customize.html#translucent-image-source")]
    public class TranslucentImageSource : MonoBehaviour
    {
        private float lastUpdate;

        protected virtual void Start()
        {
            previewMaterial = new Material(Shader.Find("Hidden/FillCrop"));

            InitializeBlurAlgorithm();
            CreateNewBlurredScreen();

            lastDownsample = Downsample;
        }

        private void OnDestroy()
        {
            // RT are not released automatically
            if (BlurredScreen)
                BlurredScreen.Release();
        }


        protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (blurAlgorithm == null || BlurConfig == null)
                goto draw_unmodified;

            if (shouldUpdateBlur())
            {
                if (BlurredScreen == null           || !BlurredScreen.IsCreated() ||
                    Downsample    != lastDownsample ||
                    !BlurRegion.Approximately(lastBlurRegion))
                {
                    CreateNewBlurredScreen();
                    lastDownsample = Downsample;
                    lastBlurRegion = BlurRegion;
                }

                blurAlgorithm.Blur(source, BlurRegion, ref blurredScreen);
            }

            if (preview)
            {
                previewMaterial.SetVector(ShaderId.CROP_REGION, BlurRegion.ToMinMaxVector());
                Graphics.Blit(BlurredScreen, destination, previewMaterial);
                return;
            }

            draw_unmodified:
            Graphics.Blit(source, destination);
        }

        private void InitializeBlurAlgorithm()
        {
            switch (BlurAlgorithmSelection)
            {
                case BlurAlgorithmType.ScalableBlur:
                    blurAlgorithm = new ScalableBlur();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(BlurAlgorithmSelection));
            }

            blurAlgorithm.Init(BlurConfig);
        }

        protected virtual void CreateNewBlurredScreen()
        {
            if (BlurredScreen)
                BlurredScreen.Release();

            BlurredScreen = new RenderTexture(Mathf.RoundToInt(Cam.pixelWidth  * BlurRegion.width)  >> Downsample,
                                              Mathf.RoundToInt(Cam.pixelHeight * BlurRegion.height) >> Downsample,
                                              0)
                            {
                                name       = $"{gameObject.name} Translucent Image Source",
                                filterMode = FilterMode.Bilinear
                            };
            BlurredScreen.Create();
        }

        public bool shouldUpdateBlur()
        {
            if (!enabled)
                return false;

            var now    = GetTrueCurrentTime();
            var should = now - lastUpdate >= MinUpdateCycle;

            if (should)
                lastUpdate = GetTrueCurrentTime();

            return should;
        }

        private static float GetTrueCurrentTime()
        {
#if UNITY_EDITOR
            return (float) EditorApplication.timeSinceStartup;
#else
            return Time.unscaledTime;
#endif
        }

        #region Public field

        /// <summary>
        ///     Maximum number of times to update the blurred image each second
        /// </summary>
        public float maxUpdateRate = float.PositiveInfinity;

        /// <summary>
        ///     Render the blurred result to the render target
        /// </summary>
        public bool preview;

        #endregion


        #region Private Field

        private IBlurAlgorithm blurAlgorithm;

        [SerializeField] private BlurAlgorithmType blurAlgorithmSelection = BlurAlgorithmType.ScalableBlur;

        [SerializeField] private BlurConfig blurConfig;

        [SerializeField] private int downsample;

        private int lastDownsample;

        [SerializeField] private Rect blurRegion = new Rect(0, 0, 1, 1);

        private Rect lastBlurRegion = new Rect(0, 0, 1, 1);

        //Disable non-sense warning from Unity
#pragma warning disable 0108
        private Camera camera;
#pragma warning restore 0108

        private Material      previewMaterial;
        private RenderTexture blurredScreen;

        #endregion


        #region Properties

        public BlurAlgorithmType BlurAlgorithmSelection
        {
            get => blurAlgorithmSelection;
            set
            {
                if (value == blurAlgorithmSelection)
                    return;
                blurAlgorithmSelection = value;
                InitializeBlurAlgorithm();
            }
        }

        public BlurConfig BlurConfig
        {
            get => blurConfig;
            set
            {
                blurConfig = value;
                InitializeBlurAlgorithm();
            }
        }

        /// <summary>
        ///     Result of the image effect. Translucent Image use this as their content (read-only)
        /// </summary>
        public RenderTexture BlurredScreen
        {
            get => blurredScreen;
            set => blurredScreen = value;
        }

        /// <summary>
        ///     The Camera attached to the same GameObject. Cached in field 'camera'
        /// </summary>
        internal Camera Cam => camera ? camera : camera = GetComponent<Camera>();

        /// <summary>
        ///     The rendered image will be shrinked by a factor of 2^{{this}} before bluring to reduce processing time
        /// </summary>
        /// <value>
        ///     Must be non-negative. Default to 0
        /// </value>
        public int Downsample
        {
            get => downsample;
            set => downsample = Mathf.Max(0, value);
        }

        /// <summary>
        ///     Define the rectangular area on screen that will be blurred.
        /// </summary>
        /// <value>
        ///     Between 0 and 1
        /// </value>
        public Rect BlurRegion
        {
            get => blurRegion;
            set
            {
                var min = new Vector2(1 / (float) Cam.pixelWidth, 1 / (float) Cam.pixelHeight);
                blurRegion.x      = Mathf.Clamp(value.x,      0,     1 - min.x);
                blurRegion.y      = Mathf.Clamp(value.y,      0,     1 - min.y);
                blurRegion.width  = Mathf.Clamp(value.width,  min.x, 1 - blurRegion.x);
                blurRegion.height = Mathf.Clamp(value.height, min.y, 1 - blurRegion.y);
            }
        }

        public Rect BlurRegionNormalizedScreenSpace
        {
            get
            {
                var camRect = camera.rect;
                return new Rect(camRect.position + BlurRegion.position * camRect.size,
                                camRect.size * BlurRegion.size);
            }

            set
            {
                var camRect = camera.rect;
                blurRegion.position = (value.position - camRect.position) / camRect.size;
                blurRegion.size     = value.size                          / camRect.size;
            }
        }

        /// <summary>
        ///     Minimum time in second to wait before refresh the blurred image.
        ///     If maxUpdateRate non-positive then just stop updating
        /// </summary>
        private float MinUpdateCycle => maxUpdateRate > 0 ? 1f / maxUpdateRate : float.PositiveInfinity;

        #endregion


#if UNITY_EDITOR

        protected virtual void OnEnable()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) Start();
        }

        protected virtual void OnGUI()
        {
            if (!preview) return;
            if (Selection.activeGameObject != gameObject) return;

            var curBlurRegionNSS = BlurRegionNormalizedScreenSpace;
            var newBlurRegionNSS = ResizableScreenRect.Draw(curBlurRegionNSS);

            if (newBlurRegionNSS != curBlurRegionNSS)
            {
                Undo.RecordObject(this, "Change Blur Region");
                BlurRegionNormalizedScreenSpace = newBlurRegionNSS;
            }

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                EditorApplication.QueuePlayerLoopUpdate();
        }
#endif
    }
}