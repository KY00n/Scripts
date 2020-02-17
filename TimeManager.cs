using System.Collections;
using UnityEngine;

/// <summary>
/// クラシックなSTGの処理落ちを再現するための独自delta timeなどを管理する。
/// </summary>
/// <remarks>
/// ・アタッチしたものを(始めの)シーンに一つ置くだけで良い。複数のシーンに配置していても多重処理しないので安心。
/// ・ExecutionOrderは<see cref="Update"/>で<see cref="Time.deltaTime"/>を使う処理より早く。
/// </remarks>
[DefaultExecutionOrder(-1)]
public class TimeManager : MonoBehaviour
{
    /// <summary>
    /// 希望のフレームレートに近づけるの処理の種類。
    /// </summary>
    public enum FPSAdjustmentType
    {
        None,
        FromFPSCheck,
    }


    [Delayed]
    [SerializeField]
    [Tooltip("希望のフレームレート")]
    int m_TargetFrameRate = k_DefaultTargetFrameRate;
    [SerializeField]
    [Tooltip("フレームレートを計算する間隔(秒)")]
    float m_FPSCheckIntervalTime = 1f;
    [SerializeField]
    [Tooltip("希望のフレームレートに近づけるための処理")]
    FPSAdjustmentType m_FPSAdjustment = FPSAdjustmentType.None;

#if UNITY_EDITOR
    int m_PrevTargetFrameRate;
    float m_PrevFPSCheckIntervalTime;
    FPSAdjustmentType m_PrevFPSAdjustment;
#endif

    /// <summary>
    /// 前回のフレームレートチェックから何フレーム経ったか
    /// </summary>
    int m_FrameCountSinceCheck = 0;
    /// <summary>
    /// 前回のフレームレートチェックから何秒経ったか
    /// </summary>
    float m_TimeSinceCheck = 0f;

    /// <summary>
    /// フレームレートを1つに決めてるプロジェクトで<see cref="WaitForFrames"/>で第2引数を省略するための定数。
    /// </summary>
    const int k_DefaultTargetFrameRate = 60;

    /// <summary>
    /// <see cref="Object.DontDestroyOnLoad(Object)"/>のシングルトン
    /// </summary>
    static GameObject s_Instance;

    /// <summary>
    /// 希望のフレームレート
    /// </summary>
    public static int TargetFrameRate { get; set; }
    /// <summary>
    /// フレームレートを計算する間隔(秒)
    /// </summary>
    public static float FPSCheckIntervalTime { get; set; }
    /// <summary>
    /// 希望のフレームレートに近づけるための処理
    /// </summary>
    public static FPSAdjustmentType FPSAdjustment { get; set; }
    /// <summary>
    /// 現在のFPS。<see cref="m_FPSCheckIntervalTime"/>秒置きに更新される。
    /// </summary>
    public static float CurrentFPS { get; private set; }
    /// <summary>
    /// STGなオブジェクトを制御するコンテキストで扱うdelta time。
    /// </summary>
    public static float DeltaTime { get; private set; }
    /// <summary>
    /// STGなオブジェクトを制御するコンテキストで扱うtime scale。
    /// </summary>
    public static float TimeScale { get; private set; }

    void Awake()
    {
        // シングルトン
        if (s_Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            s_Instance = gameObject;
            DontDestroyOnLoad(gameObject);

            TargetFrameRate = m_TargetFrameRate;
            FPSCheckIntervalTime = m_FPSCheckIntervalTime;
            FPSAdjustment = m_FPSAdjustment;

            Application.targetFrameRate = TargetFrameRate;

#if UNITY_EDITOR
            m_PrevTargetFrameRate = m_TargetFrameRate;
            m_PrevFPSCheckIntervalTime = m_FPSCheckIntervalTime;
            m_PrevFPSAdjustment = m_FPSAdjustment;
#endif
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        // インスペクターからの値反映や
        // インスペクターへの表示

        if (m_TargetFrameRate == m_PrevTargetFrameRate)
        {
            m_TargetFrameRate = TargetFrameRate;
            m_PrevTargetFrameRate = m_TargetFrameRate;
        }
        else
        {
            TargetFrameRate = m_TargetFrameRate;
            m_PrevTargetFrameRate = m_TargetFrameRate;
        }

        if (m_FPSCheckIntervalTime == m_PrevFPSCheckIntervalTime)
        {
            m_FPSCheckIntervalTime = FPSCheckIntervalTime;
            m_PrevFPSCheckIntervalTime = m_FPSCheckIntervalTime;
        }
        else
        {
            FPSCheckIntervalTime = m_FPSCheckIntervalTime;
            m_PrevFPSCheckIntervalTime = m_FPSCheckIntervalTime;
        }

        if (m_FPSAdjustment == m_PrevFPSAdjustment)
        {
            m_FPSAdjustment = FPSAdjustment;
            m_PrevFPSAdjustment = m_FPSAdjustment;
        }
        else
        {
            FPSAdjustment = m_FPSAdjustment;
            m_PrevFPSAdjustment = m_FPSAdjustment;
        }
#endif

        // DesiredFrameRateの更新

        if (TargetFrameRate <= 0)
        {
            DeltaTime = Time.deltaTime;
        }
        else
        {
            DeltaTime = (1f / TargetFrameRate) * Time.timeScale;
        }


        // CurrentFPSの更新

        m_FrameCountSinceCheck++;
        m_TimeSinceCheck += Time.unscaledDeltaTime;
        if (m_TimeSinceCheck >= m_FPSCheckIntervalTime)
        {
            CurrentFPS = m_FrameCountSinceCheck / m_TimeSinceCheck;
            m_FrameCountSinceCheck = 0;
            m_TimeSinceCheck = 0f;
        }


        // WaitForFramesで使うtime scaleを確保
        TimeScale = Time.timeScale;
    }

    // 他のオブジェクトがLateUpdateでこのクラスの独自delta timeを使うことがある場合、
    // 別MonoBehaviourへの分離を考える。
    void LateUpdate()
    {
        // FPS調整処理

        switch (m_FPSAdjustment)
        {
            case FPSAdjustmentType.None:
                if (m_TargetFrameRate != Application.targetFrameRate)
                {
                    Application.targetFrameRate = m_TargetFrameRate;
                }
                break;
            case FPSAdjustmentType.FromFPSCheck:
                if (m_FrameCountSinceCheck == 0)
                {
                    if (CurrentFPS <= 0)
                        break;

                    Application.targetFrameRate =
                        (int)(Application.targetFrameRate * m_TargetFrameRate / CurrentFPS);
                }
                break;
        }
    }

    /// <summary>
    /// <paramref name="frames"/>フレーム待たせる。
    /// </summary>
    public static IEnumerator WaitForFrames(int frames, int frameRate = k_DefaultTargetFrameRate)
    {
        if (frames <= 0)
        {
            yield break;
        }

        var finishTime = frameRate * frames;
        var progressTime = 0f;

        for (var progressFrames = 0; frameRate == TargetFrameRate && TimeScale == 1f; progressFrames++)
        {
            if (progressFrames >= frames)
            {
                yield break;
            }
            yield return null;
            progressTime += DeltaTime;
        }

        while (progressTime < finishTime)
        {
            yield return null;
            progressTime += DeltaTime;
        }
    }
}