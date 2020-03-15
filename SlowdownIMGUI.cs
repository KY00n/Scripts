using UnityEngine;

/// <summary>
/// 指定したfpsになるように処理落ちを起こすIMGUIを提供する。
/// </summary>
public class SlowdownIMGUI : TransformedWindowIMGUI
{
    public float sliderMinWidth = 100f;
    [Tooltip("スライダーの両端のTextFieldのWidth。")]
    public float sliderFieldWidth = 32f;
    public Color warningColor = Color.yellow;
    public Color errorColor = Color.red;
    [Space]
    [SerializeField]
    float m_InitSliderLeftValue = 2f;
    [SerializeField]
    float m_InitSliderRightValue = 60f;

    // 安全のため…。
    float m_MinTargetFps = 1f;

    float m_PrevRealtime = -1f;

    bool m_Slowdown = false;
    string m_CurrentValueText;
    string m_SliderLeftValueText;
    float m_SliderValue;
    string m_SliderRightValueText;

    public override string WindowTitle => "処理落ち機";

    void Start()
    {
        m_SliderLeftValueText = m_InitSliderLeftValue.ToString();
        m_SliderValue = Mathf.Max(m_InitSliderLeftValue, m_InitSliderRightValue);
        m_SliderRightValueText = m_InitSliderRightValue.ToString();
        m_CurrentValueText = m_SliderValue.ToString();
    }

    void Update()
    {
        if (m_Slowdown)
        {
            while (Time.realtimeSinceStartup < m_PrevRealtime + (1f / TargetFPSParse(m_CurrentValueText)))
            {
                // Do 虚無.
            }
        }
        m_PrevRealtime = Time.realtimeSinceStartup;
    }

    protected override void WindowContents(int id)
    {
        var sliderLeftValue = TargetFPSParse(m_SliderLeftValueText);
        var sliderRightValue = TargetFPSParse(m_SliderRightValueText);

        var prevCurrentValueText = m_CurrentValueText;
        var prevSliderLeftValueText = m_SliderLeftValueText;
        var prevSliderValue = m_SliderValue;
        var prevSliderRightValueText = m_SliderRightValueText;

        GUILayout.BeginHorizontal();
        m_Slowdown = GUILayout.Toggle(m_Slowdown, "有効");
        m_CurrentValueText = TargetFPSTextField(m_CurrentValueText);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        m_SliderLeftValueText = TargetFPSTextField(m_SliderLeftValueText,
            GUILayout.Width(sliderFieldWidth));
        m_SliderValue = GUILayout.HorizontalSlider(m_SliderValue, sliderLeftValue, sliderRightValue, GUILayout.MinWidth(sliderMinWidth));
        m_SliderRightValueText = TargetFPSTextField(m_SliderRightValueText,
            GUILayout.Width(sliderFieldWidth));
        GUILayout.EndHorizontal();

        if (m_CurrentValueText != prevCurrentValueText)
        {
            m_SliderValue = TargetFPSParse(m_CurrentValueText);
        }
        else if ((m_SliderLeftValueText == prevSliderLeftValueText)
            && (m_SliderRightValueText == prevSliderRightValueText)
            && (m_SliderValue != prevSliderValue))
        {
            m_CurrentValueText = m_SliderValue.ToString();
        }
    }

    string TargetFPSTextField(string text, params GUILayoutOption[] options)
    {
        // 無効な文字列でのみ色を変えるために元の色を確保。
        var prevColor = GUI.color;

        // 空文字列の場合はワーニングの色にする。
        if (text.Length == 0)
        {
            GUI.color = warningColor;
        }
        // 数値に変換不可か無効な数値の場合はエラーの色にする。
        else if (!float.TryParse(text, out var parseResult) || parseResult < 1f)
        {
            GUI.color = errorColor;
        }

        var resultText = GUILayout.TextField(text, options);

        // 色の設定を元に戻す。
        GUI.color = prevColor;

        return resultText;
    }

    /// <summary>
    /// <see cref="float.TryParse(string, out float)"/>での変換結果のみを返す。
    /// </summary>
    /// <param name="s"><see cref="float"/>に変換したい文字列</param>
    /// <returns>変換結果</returns>
    float FloatParse(string s)
    {
        float.TryParse(s, out var result);
        return result;
    }

    /// <summary>
    /// 文字列を<see cref="m_TargetFps"/>などで使える値に変換する。
    /// </summary>
    /// <param name="s"><see cref="m_TargetFps"/>などで使える値に変換したい文字列</param>
    /// <returns>変換結果</returns>
    float TargetFPSParse(string s)
    {
        var result = FloatParse(s);
        result = Mathf.Max(result, m_MinTargetFps);

        return result;
    }
}
