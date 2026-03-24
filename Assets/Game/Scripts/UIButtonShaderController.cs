using UnityEngine;
using UnityEngine.UI;

public class UIButtonShaderController : MonoBehaviour
{
    [Header("Fill Colors")]
    [SerializeField] private Color fillTop = new Color(0.95f, 0.55f, 1f, 1f);
    [SerializeField] private Color fillBottom = new Color(0.55f, 0.25f, 0.9f, 1f);

    [Header("Outline")]
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField] [Range(0f, 0.5f)] private float outlineThickness = 0.05f;
    [SerializeField] [Range(0f, 0.5f)] private float cornerRadius = 0.18f;
    [SerializeField] [Range(0f, 0.05f)] private float edgeSmoothness = 0.01f;

    [Header("Glow Effect")]
    [SerializeField] private Color glowColor = new Color(1f, 0.7f, 1f, 1f);
    [SerializeField] [Range(0f, 0.4f)] private float glowWidth = 0.05f;
    [SerializeField] [Range(0f, 10f)] private float glowSpeed = 3f;
    [SerializeField] [Range(0f, 2f)] private float glowStrength = 0.8f;

    [Header("Stripe Effect")]
    [SerializeField] private Color stripeColor = Color.white;
    [SerializeField] [Range(0f, 1f)] private float stripeWidth = 0.25f;
    [SerializeField] [Range(0f, 10f)] private float stripeSpeed = 2f;
    [SerializeField] [Range(0f, 360f)] private float stripeAngle = 45f;
    [SerializeField] [Range(0f, 2f)] private float stripeStrength = 1f;

    [Header("Shadow")]
    [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.5f);
    [SerializeField] private Vector2 shadowOffset = new Vector2(0.01f, -0.01f);
    [SerializeField] [Range(0f, 0.1f)] private float shadowBlur = 0.03f;

    [Header("Highlight")]
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private Vector2 highlightPos = new Vector2(0.15f, 0.85f);
    [SerializeField] private Vector2 highlightSize = new Vector2(0.1f, 0.06f);

    [Header("Material Settings")]
    [SerializeField] private Material baseMaterial;
    [SerializeField] private bool createMaterialInstance = true;

    private Image imageComponent;
    private Material materialInstance;

    // Property name constants (matching the shader)
    private static readonly int FillTopProperty = Shader.PropertyToID("_FillTop");
    private static readonly int FillBottomProperty = Shader.PropertyToID("_FillBottom");
    private static readonly int OutlineColorProperty = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineThicknessProperty = Shader.PropertyToID("_OutlineThickness");
    private static readonly int CornerRadiusProperty = Shader.PropertyToID("_CornerRadius");
    private static readonly int EdgeSmoothnessProperty = Shader.PropertyToID("_EdgeSmoothness");
    private static readonly int GlowColorProperty = Shader.PropertyToID("_GlowColor");
    private static readonly int GlowWidthProperty = Shader.PropertyToID("_GlowWidth");
    private static readonly int GlowSpeedProperty = Shader.PropertyToID("_GlowSpeed");
    private static readonly int GlowStrengthProperty = Shader.PropertyToID("_GlowStrength");
    private static readonly int StripeColorProperty = Shader.PropertyToID("_StripeColor");
    private static readonly int StripeWidthProperty = Shader.PropertyToID("_StripeWidth");
    private static readonly int StripeSpeedProperty = Shader.PropertyToID("_StripeSpeed");
    private static readonly int StripeAngleProperty = Shader.PropertyToID("_StripeAngle");
    private static readonly int StripeStrengthProperty = Shader.PropertyToID("_StripeStrength");
    private static readonly int ShadowColorProperty = Shader.PropertyToID("_ShadowColor");
    private static readonly int ShadowOffsetProperty = Shader.PropertyToID("_ShadowOffset");
    private static readonly int ShadowBlurProperty = Shader.PropertyToID("_ShadowBlur");
    private static readonly int HighlightColorProperty = Shader.PropertyToID("_HighlightColor");
    private static readonly int HighlightPosProperty = Shader.PropertyToID("_HighlightPos");
    private static readonly int HighlightSizeProperty = Shader.PropertyToID("_HighlightSize");

    private void Awake()
    {
        imageComponent = GetComponent<Image>();
        InitializeMaterial();
    }

    private void Start()
    {
        ApplyProperties();
    }

    private void OnValidate()
    {
        // Apply changes in editor when values are modified
        if (Application.isPlaying && materialInstance != null)
        {
            ApplyProperties();
        }
    }

    private void InitializeMaterial()
    {
        if (baseMaterial == null)
        {
            Debug.LogError($"Base material not assigned on {gameObject.name}");
            return;
        }

        if (createMaterialInstance)
        {
            // Create a unique material instance for this button
            materialInstance = new Material(baseMaterial);
            materialInstance.name = $"{baseMaterial.name}_Instance_{gameObject.name}";
            imageComponent.material = materialInstance;
        }
        else
        {
            // Use the shared material (changes will affect all buttons using this material)
            materialInstance = baseMaterial;
            imageComponent.material = materialInstance;
        }
    }

    private void ApplyProperties()
    {
        if (materialInstance == null) return;

        // Apply fill colors
        materialInstance.SetColor(FillTopProperty, fillTop);
        materialInstance.SetColor(FillBottomProperty, fillBottom);

        // Apply outline properties
        materialInstance.SetColor(OutlineColorProperty, outlineColor);
        materialInstance.SetFloat(OutlineThicknessProperty, outlineThickness);
        materialInstance.SetFloat(CornerRadiusProperty, cornerRadius);
        materialInstance.SetFloat(EdgeSmoothnessProperty, edgeSmoothness);

        // Apply glow properties
        materialInstance.SetColor(GlowColorProperty, glowColor);
        materialInstance.SetFloat(GlowWidthProperty, glowWidth);
        materialInstance.SetFloat(GlowSpeedProperty, glowSpeed);
        materialInstance.SetFloat(GlowStrengthProperty, glowStrength);

        // Apply stripe properties
        materialInstance.SetColor(StripeColorProperty, stripeColor);
        materialInstance.SetFloat(StripeWidthProperty, stripeWidth);
        materialInstance.SetFloat(StripeSpeedProperty, stripeSpeed);
        materialInstance.SetFloat(StripeAngleProperty, stripeAngle);
        materialInstance.SetFloat(StripeStrengthProperty, stripeStrength);

        // Apply shadow properties
        materialInstance.SetColor(ShadowColorProperty, shadowColor);
        materialInstance.SetVector(ShadowOffsetProperty, new Vector4(shadowOffset.x, shadowOffset.y, 0, 0));
        materialInstance.SetFloat(ShadowBlurProperty, shadowBlur);

        // Apply highlight properties
        materialInstance.SetColor(HighlightColorProperty, highlightColor);
        materialInstance.SetVector(HighlightPosProperty, new Vector4(highlightPos.x, highlightPos.y, 0, 0));
        materialInstance.SetVector(HighlightSizeProperty, new Vector4(highlightSize.x, highlightSize.y, 0, 0));
    }

    #region Public Methods for Runtime Control

    public void SetFillColors(Color top, Color bottom)
    {
        fillTop = top;
        fillBottom = bottom;
        if (materialInstance != null)
        {
            materialInstance.SetColor(FillTopProperty, fillTop);
            materialInstance.SetColor(FillBottomProperty, fillBottom);
        }
    }

    public void SetGlowEffect(Color color, float width, float speed, float strength)
    {
        glowColor = color;
        glowWidth = width;
        glowSpeed = speed;
        glowStrength = strength;
        if (materialInstance != null)
        {
            materialInstance.SetColor(GlowColorProperty, glowColor);
            materialInstance.SetFloat(GlowWidthProperty, glowWidth);
            materialInstance.SetFloat(GlowSpeedProperty, glowSpeed);
            materialInstance.SetFloat(GlowStrengthProperty, glowStrength);
        }
    }

    public void SetStripeEffect(Color color, float width, float speed, float angle, float strength)
    {
        stripeColor = color;
        stripeWidth = width;
        stripeSpeed = speed;
        stripeAngle = angle;
        stripeStrength = strength;
        if (materialInstance != null)
        {
            materialInstance.SetColor(StripeColorProperty, stripeColor);
            materialInstance.SetFloat(StripeWidthProperty, stripeWidth);
            materialInstance.SetFloat(StripeSpeedProperty, stripeSpeed);
            materialInstance.SetFloat(StripeAngleProperty, stripeAngle);
            materialInstance.SetFloat(StripeStrengthProperty, stripeStrength);
        }
    }

    public void SetCornerRadius(float radius)
    {
        cornerRadius = radius;
        if (materialInstance != null)
        {
            materialInstance.SetFloat(CornerRadiusProperty, cornerRadius);
        }
    }

    #endregion

    private void OnDestroy()
    {
        // Clean up material instance to prevent memory leaks
        if (createMaterialInstance && materialInstance != null)
        {
            if (Application.isPlaying)
            {
                Destroy(materialInstance);
            }
            else
            {
                DestroyImmediate(materialInstance);
            }
        }
    }
}