using System;
using System.Collections.Generic;
using UnityEngine;

// Pooler200209
//
// 設計思想
//
// GameObjectを手軽にプーリングする。
// 一般的に推奨されない最適化はあまりしない。
//
// 主にMicrosoft DocsやUnity Manualに載っていることに気を遣う。
// 現在把握しているもの:
// Microsoft Docs
// .NET / C# ガイド / 安全で効率的な C# コードを記述する
// Unity Manual
// Scripting / Scripting Overview / Understanding Automatic Memory Management
// Platform developmentMobile / Developer Checklist / Optimizations
//     (Practical guide to optimization for mobilesは参考程度に)
// Best practice guides / Understanding optimization in Unity
//     (Special optimizationsは参考程度に)
//
// O(1)を心がける
//
// 「(プーリング)しないよりマシ」をモットーに。
//
//
// おおまかな設計
// Object.Instantiateの代わりとなるメソッドを提供し、
// それを用いて生成されたGameObjectは、生成時にスクリプトによってアタッチされた
// コンポーネントのOnEnableとOnDisableとOnDestroyで自動的に
// プーリング状態を管理する。
// そのため、疑似削除(プールに入れる)操作はOnDisableを起こすものとする。
//
//
// 使う際の備考
// ・ApparentInstantiateにはプレハブしか渡してはならない。
// ・エディターで人の手でアタッチしてはならない。
/// <summary>
/// <see cref="GameObject"/>のプーリングを行う。
/// </summary>
/// <remarks><see cref="Pooler"/>のメソッド以外の手段でアタッチしてはならない。</remarks>
[DisallowMultipleComponent]
public class Pooler : MonoBehaviour
{
    /// <summary>
    /// 格納先の<see cref="LinkedList{GameObject}"/>と検索用の<see cref="LinkedListNode{GameObject}"/>
    /// </summary>
    readonly struct PoolLink
    {
        public readonly LinkedList<GameObject> list;
        public readonly LinkedListNode<GameObject> node;

        public PoolLink(LinkedList<GameObject> list, LinkedListNode<GameObject> node)
        {
            this.list = list;
            this.node = node;
        }
    }


    /// <summary>
    /// プーリングする全ての<see cref="GameObject"/>。
    /// 格納先の<see cref="LinkedList{GameObject}"/>と検索用の<see cref="LinkedListNode{GameObject}"/>の情報を値に持つ。
    /// </summary>
    static readonly Dictionary<GameObject, PoolLink> poolLinks = new Dictionary<GameObject, PoolLink>();

    void OnEnable()
    {
        var poolLink = poolLinks[gameObject];
        poolLink.list.Remove(poolLink.node);
    }

    void OnDisable()
    {
        var poolLink = poolLinks[gameObject];
        poolLink.list.AddFirst(poolLink.node);
    }

    void OnDestroy()
    {
        var poolLink = poolLinks[gameObject];
        poolLink.list.Remove(poolLink.node);
        poolLinks.Remove(gameObject);
    }

    /// <summary>
    /// プールから<see cref="GameObject"/>を取り出し(無ければ新たにクローンし)、アクティブにする。
    /// </summary>
    /// <remarks><paramref name="prefab"/>はプレハブでなければならない。</remarks>
    /// <param name="prefab">クローン元のプレハブ</param>
    /// <returns>取り出した<see cref="GameObject"/>。</returns>
    public static GameObject ApparentInstantiate(GameObject prefab)
    {
#if UNITY_EDITOR
        if (!prefab)
        {
            Debug.LogError("ApparentInstantiateにnullが渡されました。");
            Debug.Break();
        }
#endif

        GameObject result;

        // プールが無ければ作る。
        if (!poolLinks.ContainsKey(prefab))
        {
            poolLinks.Add(prefab, new PoolLink(new LinkedList<GameObject>(), null));
        }
#if UNITY_EDITOR
        else
        {
            if (poolLinks[prefab].node != null)
            {
                Debug.LogError($"ApparentInstantiateの引数がプレハブではありません: {prefab.name}", prefab);
                Debug.Break();
            }
        }
#endif

        var targetPool = poolLinks[prefab].list;
        if (targetPool.Count == 0)
        {
            result = Instantiate(prefab);
            var newNode = targetPool.AddLast(result);
            poolLinks.Add(result, new PoolLink(targetPool, newNode));
            result.AddComponent<Pooler>();
        }
        else
        {
            result = targetPool.First.Value;
        }
        result.SetActive(true);

        return result;
    }

    /// <summary>
    /// <see cref="ApparentInstantiate(GameObject)"/>と非アクティブにする処理を<paramref name="n"/>回ずつ行う。
    /// </summary>
    /// <remarks><paramref name="prefab"/>はプレハブでなければならない。</remarks>
    /// <param name="prefab">クローン元のプレハブ</param>
    /// <param name="n">処理の回数</param>
    public static void Prepare(GameObject prefab, int n)
    {
#if UNITY_EDITOR
        if (!prefab)
        {
            Debug.LogError("Prepareにnullが渡されました。");
            Debug.Break();
        }
#endif

        var clones = new GameObject[n];
        for (var i = 0; i < n; i++)
        {
            clones[i] = ApparentInstantiate(prefab);
        }
        foreach (var clone in clones)
        {
            clone.SetActive(false);
        }
    }

    /// <summary>
    /// <paramref name="obj"/>を非アクティブにする。
    /// </summary>
    /// <param name="obj">非アクティブにする<see cref="GameObject"/>。</param>
    [Obsolete]
    public static void ApparentDestroy(GameObject obj)
    {
#if UNITY_EDITOR
        if (!obj.GetComponent<Pooler>())
        {
            Debug.LogWarning($"ApparentDestroyの引数にPoolerがアタッチされていません: {obj.name}", obj);
        }
#endif
        obj.SetActive(false);
    }

    [Obsolete]
    public static void Clear()
    {

    }
}
