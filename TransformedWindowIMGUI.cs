using UnityEngine;

// 派生先ではウィンドウのタイトルと中身だけ実装すれば良いようにする。
// ウィンドウのIDは自動で連番で割り振る。

/// <summary>
/// uguiのようにレイアウトできる一つのIMGUIウィンドウを提供する。
/// 派生先でのOnGUIの実装はしない。
/// </summary>
public abstract class TransformedWindowIMGUI : MonoBehaviour
{
    public Vector2 anchoredPosition;
    public bool rawEditMode = false;
    public Vector2 anchor;
    public Vector2 pivot;
    public float windowMinWidth = 0f;

    bool m_WindowIdAssigned = false;
    int m_WindowId;
    Rect m_WindowRect;
    Vector2 m_PrevScreenSize;
    Vector2 m_PrevAnchor;
    Vector2 m_PrevPivot;

    /// <summary>
    /// ウィンドウのIDを自動で連番で割り振る際の最初の番号。
    /// </summary>
    const int k_BaseWindowId = 0;

    /// <summary>
    /// ウィンドウにIDが割り振られた数。
    /// </summary>
    static int s_CreatedWindowNum;

    /// <summary>
    /// 描画されるウィンドウのID。
    /// </summary>
    public int WindowID
    {
        get
        {
            if (!m_WindowIdAssigned)
            {
                m_WindowId = k_BaseWindowId + s_CreatedWindowNum++;
                m_WindowIdAssigned = true;
            }

            return m_WindowId;
        }
    }

    /// <summary>
    /// ウィンドウのタイトルバーに表示されるテキスト。
    /// </summary>
    public abstract string WindowTitle { get; }

    void OnGUI()
    {
        var screenSize = new Vector2(Screen.width, Screen.height);

        if (!rawEditMode
            && ((anchor != m_PrevAnchor) || (pivot != m_PrevPivot)))
        {
            anchoredPosition -= (m_PrevScreenSize * (anchor - m_PrevAnchor))
                - (m_WindowRect.size * (pivot - m_PrevPivot));
        }
        Vector2 position = (screenSize * anchor)
            - (m_WindowRect.size * pivot)
            + anchoredPosition;
        m_WindowRect.position = position;
        m_WindowRect = GUILayout.Window(WindowID,
            m_WindowRect,
            WindowFunction,
            WindowTitle,
            GUILayout.Width(0f),
            GUILayout.MinWidth(windowMinWidth),
            //GUILayout.ExpandHeight(false)
            GUILayout.Height(0f));
        // 移動操作での値の変動をanchoredPositionに反映させる。
        anchoredPosition += m_WindowRect.position - position;

        m_PrevScreenSize = screenSize;
        m_PrevAnchor = anchor;
        m_PrevPivot = pivot;
    }

    /// <summary>
    /// <see cref="GUILayout.Window(int, Rect, GUI.WindowFunction, GUIContent, GUILayoutOption[])"/>に渡すメソッド。
    /// </summary>
    /// <param name="id">描画されるウィンドウのID。</param>
    private void WindowFunction(int id)
    {
        WindowContents(id);
        GUI.DragWindow();
    }

    /// <summary>
    /// ウィンドウのコンテンツを表示するメソッド。この後に自動で<see cref="GUI.DragWindow()"/>する。
    /// </summary>
    /// <param name="id">描画されるウィンドウのID。</param>
    protected abstract void WindowContents(int id);
}
