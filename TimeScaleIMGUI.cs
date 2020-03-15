using UnityEngine;

/// <summary>
/// <see cref="Time.timeScale"/>を簡単にいじれるIMGUIを提供する。
/// </summary>
public class TimeScaleIMGUI : TransformedWindowIMGUI
{
    public float sliderMinWidth = 100f;
    [Tooltip("スライダーの両端のTextFieldのWidth。")]
    public float sliderFieldMinWidth = 32f;
    public Color warningColor = Color.yellow;
    public Color errorColor = Color.red;

    [Space]
    [SerializeField]
    float m_DefaultSliderLeftValue = 0f;
    [SerializeField]
    float m_DefaultSliderRightValue = 2f;
    [SerializeField]
    float m_DefaultResetValue = 1f;

    string m_CurrentValueText;
    string m_SliderLeftValueText;
    float m_SliderValue;
    string m_SliderRightValueText;
    string m_ResetValueText;
    bool m_ForceMode = false;
    float m_PrevTimeScale = -1f;

    public override string WindowTitle => "Time Scale";

    void Start()
    {
        m_SliderLeftValueText = m_DefaultSliderLeftValue.ToString();
        m_SliderRightValueText = m_DefaultSliderRightValue.ToString();
        m_ResetValueText = m_DefaultResetValue.ToString();
    }

    protected override void WindowContents(int id)
    {
        var currentTimeScale = Time.timeScale;
        var timeScaleChangedOutside = currentTimeScale != m_PrevTimeScale;

        if (!m_ForceMode && timeScaleChangedOutside)
        {
            m_CurrentValueText = currentTimeScale.ToString();
            m_SliderValue = currentTimeScale;
        }

        var sliderLeftValue = TimeScaleParse(m_SliderLeftValueText);
        var sliderRightValue = TimeScaleParse(m_SliderRightValueText);

        var prevCurrentValueText = m_CurrentValueText;
        var prevSliderLeftValueText = m_SliderLeftValueText;
        var prevSliderValue = m_SliderValue;
        var prevSliderRightValueText = m_SliderRightValueText;
        bool resetButtonPushed;

        GUILayout.BeginHorizontal();
        GUILayout.Label("現在の値:", GUILayout.ExpandWidth(false));
        m_CurrentValueText = TimeScaleTextField(m_CurrentValueText);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        m_SliderLeftValueText = TimeScaleTextField(m_SliderLeftValueText,
            GUILayout.MinWidth(sliderFieldMinWidth), GUILayout.ExpandWidth(false));
        m_SliderValue = GUILayout.HorizontalSlider(m_SliderValue,
            sliderLeftValue, sliderRightValue, GUILayout.MinWidth(sliderMinWidth));
        m_SliderRightValueText = TimeScaleTextField(m_SliderRightValueText,
            GUILayout.MinWidth(sliderFieldMinWidth), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        resetButtonPushed = GUILayout.Button("Set", GUILayout.ExpandWidth(false));
        m_ResetValueText = TimeScaleTextField(m_ResetValueText);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        m_ForceMode = GUILayout.Toggle(m_ForceMode, "常時反映モード");
        GUILayout.EndHorizontal();

        if (resetButtonPushed)
        {
            var newTimeScale = TimeScaleParse(m_ResetValueText);
            Time.timeScale = newTimeScale;
            m_CurrentValueText = newTimeScale.ToString();
            m_SliderValue = newTimeScale;
        }
        else if (m_CurrentValueText != prevCurrentValueText)
        {
            var newTimeScale = TimeScaleParse(m_CurrentValueText);
            Time.timeScale = newTimeScale;
            m_SliderValue = newTimeScale;
        }
        else if ((!timeScaleChangedOutside || m_ForceMode)
            && (m_SliderLeftValueText == prevSliderLeftValueText)
            && (m_SliderRightValueText == prevSliderRightValueText)
            && (m_SliderValue != prevSliderValue))
        {
            Time.timeScale = m_SliderValue;
            m_CurrentValueText = m_SliderValue.ToString();
        }
        else if (m_ForceMode)
        {
            Time.timeScale = m_PrevTimeScale;
        }

        m_PrevTimeScale = Time.timeScale;
    }

    string TimeScaleTextField(string text, params GUILayoutOption[] options)
    {
        // 無効な文字列でのみ色を変えるために元の色を確保。
        var prevColor = GUI.color;

        // 空文字列の場合はワーニングの色にする。
        if (text.Length == 0)
        {
            GUI.color = warningColor;
        }
        // 数値に変換不可か無効な数値の場合はエラーの色にする。
        else if (!float.TryParse(text, out var parseResult) || parseResult < 0f || parseResult > 100f)
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
    /// 文字列を<see cref="Time.timeScale"/>で使える値に変換する。
    /// </summary>
    /// <param name="s"><see cref="Time.timeScale"/>で使える値に変換したい文字列</param>
    /// <returns>変換結果</returns>
    float TimeScaleParse(string s)
    {
        var result = FloatParse(s);
        result = Mathf.Clamp(result, 0f, 100f);

        return result;
    }
}
