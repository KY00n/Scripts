using System.Collections;
using UnityEngine;

public enum DirectionType { Player, Absolute, Sequence }

public class StgAct : MonoBehaviour
{
    #region フィールド、プロパティ

    //[Tooltip("デフォルトの角度調整")]
    //[SerializeField]
    //private float correctionAngle = 0f;
    [SerializeField]
    [Tooltip("移動方向にオブジェクトの向きを自動的に合わせるかどうか")]
    private bool m_DefaultMoveAngleLink = true;
    [SerializeField]
    [Tooltip("弾を発射するときに用いる半径")]
    private float m_FireRadius = 0f;
    [SerializeField]
    [Tooltip("出現などで考慮する半径")]
    private float m_BodyRadius = 0f;

    private float m_Angle;
    private float m_BodyAngle;

    public bool DefaultMoveAngleLink => m_DefaultMoveAngleLink;
    public float FireRadius => m_FireRadius;
    public float BodyRadius => m_BodyRadius;
    public float Speed { get; set; }
    public float Acceleration { get; set; }
    public float SpeedLimit { get; set; }
    public float Angle
    {
        get
        {
            return m_Angle;
        }
        set
        {
            m_Angle = value;
            if (MoveAngleLink)
            {
                BodyAngle = m_Angle;
            }
        }
    }
    public float AngleSpeed { get; set; }
    public float AngleAcceleration { get; set; }
    public float AngleSpeedLimit { get; set; }
    public bool MoveAngleLink { get; set; }
    public DirectionType FireDirectionType { get; set; }
    public float BodyAngle
    {
        get
        {
            return m_BodyAngle;
        }
        set
        {
            m_BodyAngle = value;
            transform.rotation = Quaternion.Euler(0, 0, m_BodyAngle + 90f);
        }
    }
    public float X
    {
        get
        {
            Vector3 pos = transform.position;
            return pos.x;
        }
        set
        {
            Vector3 pos = transform.position;
            pos.x = value;
            transform.position = pos;
        }
    }
    public float Y
    {
        get
        {
            Vector3 pos = transform.position;
            return pos.y;
        }
        set
        {
            Vector3 pos = transform.position;
            pos.y = value;
            transform.position = pos;
        }
    }
    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
        set
        {
            transform.position = value;
        }
    }
    public float Angle2Player
    {
        get
        {
            //return Mathf.Atan2(Player.y - y, Player.x - x) * Mathf.Rad2Deg;
            return Vector2.SignedAngle(Vector2.right, Player.Position - transform.position);
        }
    }

    #endregion

    #region MonoBehaviourメッセージ

    void OnEnable()
    {
        Speed = 0f;
        Acceleration = 0f;
        SpeedLimit = 0f;
        Angle = 0f;
        AngleSpeed = 0f;
        AngleAcceleration = 0f;
        AngleSpeedLimit = 0f;
        MoveAngleLink = DefaultMoveAngleLink;
        FireDirectionType = DirectionType.Absolute;
    }

    void Update()
    {
        var deltaTime = TimeManager.DeltaTime;

        Angle += AngleSpeed * deltaTime;
        //rb.velocity = Mathfd.Direction(angle) * speed;
        transform.position += (Vector3)(Mathfd.Direction(Angle) * Speed * deltaTime);

        if (MoveAngleLink)
        {
            BodyAngle = Angle;
        }

        // ★臨時回転処理
        //transform.rotation = Quaternion.Euler(0, 0, bodyAngle + 90);
        //rb.MoveRotation(angle + 90);

        if (Acceleration > 0 && Speed < SpeedLimit)
        {
            Speed = Mathf.Min(Speed + Acceleration * deltaTime, SpeedLimit);
        }
        else if (Acceleration < 0 && Speed > SpeedLimit)
        {
            Speed = Mathf.Max(Speed + Acceleration * deltaTime, SpeedLimit);
        }

        if (AngleAcceleration > 0 && AngleSpeed < AngleSpeedLimit)
        {
            AngleSpeed = Mathf.Min(AngleSpeed + AngleAcceleration * deltaTime, AngleSpeedLimit);
        }
        else if (AngleAcceleration < 0 && AngleSpeed > AngleSpeedLimit)
        {
            AngleSpeed = Mathf.Max(AngleSpeed + AngleAcceleration * deltaTime, AngleSpeedLimit);
        }
    }

    #endregion

    #region その他

    public void SetMovement(float speed, float angle)
    {
        this.Speed = speed;
        this.Angle = angle;
    }

    public void SetMovement11(float speed, float acceleration, float speedLimit, float angle, float angleSpeed, float angleAcceleration, float angleSpeedLimit)
    {
        this.Speed = speed;
        this.Acceleration = acceleration;
        this.SpeedLimit = speedLimit;
        this.Angle = angle;
        this.AngleSpeed = angleSpeed;
        this.AngleAcceleration = angleAcceleration;
        this.AngleSpeedLimit = angleSpeedLimit;

        FireDirectionType = DirectionType.Absolute;
    }

    public void SetDestAtTime(float x, float y, float t)
    {
        SetDestAtTime(new Vector2(x, y), t);
    }

    public void SetDestAtTime(Vector2 position, float t)
    {
        Vector2 s = position - (Vector2)transform.position;

        Acceleration = -2 * s.magnitude / Mathf.Pow(t, 2);
        Speed = -Acceleration * t;
        SpeedLimit = 0;
        Angle = Vector2.SignedAngle(Vector2.right, s);
    }

    public void SetRangedDest(float minX, float maxX, float minY, float maxY, float t)
    {
        var newX = Random.Range(Mathf.Max(StgFrame.left, minX), Mathf.Min(StgFrame.right, minX));
        var newY = Random.Range(Mathf.Max(StgFrame.bottom, minY), Mathf.Min(StgFrame.top, maxY));
        SetDestAtTime(new Vector2(newX, newY), t);
    }

    public void SetRangedDestS(float xS, float yS, float t)
    {
        SetRangedDest(X - xS, X + xS, Y - yS, Y + yS, t);
    }


    public IEnumerator WaitForStop()
    {
        while (Speed != 0)
        {
            yield return null;
        }
    }


    public StgAct Fire(GameObject original, float speed, float angle)
    {
        //StgAct clone = StgAct.ApparentInstantiate(original);
        //clone.transform.position = transform.position + (Vector3)(fireR * new Vector2(Mathfd.Cos(angle), Mathfd.Sin(angle)));
        //clone.speed = speed;
        //clone.angle = angle;

        return FireR(original, FireRadius, speed, angle);
    }

    public StgAct FireR(GameObject original, float r, float speed, float angle)
    {
        StgAct clone = StgAct.ApparentInstantiate(original);
        clone.transform.position = transform.position + (Vector3)(r * new Vector2(Mathfd.Cos(angle), Mathfd.Sin(angle)));
        clone.Speed = speed;
        clone.Angle = angle;

        return clone;
    }

    //public StgAct Fire10(float speed, float acceleration, float speedLimit, float angle)
    //{
    //    return null;
    //}

    //public StgAct Fire10(float r, float speed, float acceleration, float speedLimit, float angle)
    //{
    //    return null;
    //}

    //public StgAct Fire01(float speed, float angle, float angleAcceleration, float angleSpeedLimit)
    //{
    //    return null;
    //}

    //public StgAct Fire01(float r, float speed, float angle, float angleAcceleration, float angleSpeedLimit)
    //{
    //    return null;
    //}

    //public StgAct Fire11(float speed, float acceleration, float speedLimit, float angle, float angleAcceleration, float angleSpeedLimit)
    //{
    //    return null;
    //}

    //public StgAct Fire11(float r, float speed, float acceleration, float speedLimit, float angle, float angleAcceleration, float angleSpeedLimit)
    //{
    //    return null;
    //}


    public void FireWay(GameObject original, int way, float angleInterval, float speed, float angle)
    {
        angle -= angleInterval * (way - 1) / 2;
        for (var i = 0; i < way; i++)
        {
            Fire(original, speed, angle);
            angle += angleInterval;
        }
    }

    public void FireLine(GameObject original, int num, float speedInterval, float topSpeed, float angle)
    {
        for (var i = 0; i < num; i++)
        {
            Fire(original, Speed, angle);
            Speed -= speedInterval;
        }
    }

    public void FireRound(GameObject original, int num, float speed, float angle)
    {
        for (var i = 0; i < num; i++)
        {
            Fire(original, speed, angle);
            angle += 360f / num;
        }
    }

    public void FireRoundWay(GameObject originalShot, int roundN, int wayN, float wayInterval, float speed, float angle)
    {
        for (var i = 0; i < roundN; i++)
        {
            FireWay(originalShot, wayN, wayInterval, speed, angle + 360 / roundN * i);
        }
    }

    public StgAct FireR2(GameObject original, float lR, float lAngle, float sR, float speed, float sAngle)
    {
        StgAct clone = StgAct.ApparentInstantiate(original);
        clone.transform.position = transform.position + (Vector3)(lR * Mathfd.Direction(lAngle) + sR * Mathfd.Direction(sAngle));
        clone.Speed = speed;
        clone.Angle = sAngle;

        return clone;
    }

    public StgAct FireR22Player(GameObject original, float lR, float lAngle, float sR, float speed, float sAngle)
    {
        StgAct clone = FireR2(original, lR, lAngle, sR, speed, sAngle);
        clone.Angle += Player.Angle2Player(clone.transform);

        return clone;
    }

    public StgAct FireR210(GameObject original, float lR, float lAngle, float sR, float speed, float sAngle, float acceleration, float speedLimit)
    {
        StgAct clone = FireR2(original, lR, lAngle, sR, speed, sAngle);
        clone.Acceleration = acceleration;
        clone.SpeedLimit = speedLimit;

        return clone;
    }

    public StgAct FireR2102Player(GameObject original, float lR, float lAngle, float sR, float speed, float sAngle, float acceleration, float speedLimit)
    {
        StgAct clone = FireR22Player(original, lR, lAngle, sR, speed, sAngle);
        clone.Acceleration = acceleration;
        clone.SpeedLimit = speedLimit;

        return clone;
    }

    public void FireRoundR2(GameObject original, int num, float lR, float lAngle, float sR, float speed, float sAngle)
    {
        float interval = 360f / num;
        for (var i = 0; i < num; i++)
        {
            FireR2(original, lR, lAngle, sR, speed, sAngle);
            sAngle += interval;
        }
    }

    public void FireRoundR210(GameObject original, int num, float lR, float lAngle, float sR, float speed, float sAngle, float acceleration, float speedLimit)
    {
        float interval = 360f / num;
        for (var i = 0; i < num; i++)
        {
            FireR210(original, lR, lAngle, sR, speed, sAngle, acceleration, speedLimit);
            sAngle += interval;
        }
    }

    public void FireRoundR2102Player(GameObject original, int num, float lR, float lAngle, float sR, float speed, float sAngle, float acceleration, float speedLimit)
    {
        float interval = 360f / num;
        for (var i = 0; i < num; i++)
        {
            FireR2102Player(original, lR, lAngle, sR, speed, sAngle, acceleration, speedLimit);
            sAngle += interval;
        }
    }


    public static StgAct ApparentInstantiate(GameObject prefub)
    {
        return Pooler.ApparentInstantiate(prefub).GetComponent<StgAct>();
    }




    IEnumerator BehaveA(float advance, GameObject originalShot, float awayAngle)
    {
        SetMovement(StgFrame.height / 1.2f, -90);

        {
            var _adv = 0f;

            while (_adv < advance)
            {
                yield return null;
                _adv += Speed;
            }
        }

        Acceleration = -Speed / 0.5f;
        while (Speed != 0)
        {
            yield return null;
        }

        FireWay(originalShot, 3, 5, StgFrame.height / 2.5f, -90);
        yield return Yielder.WaitForFixedSeconds(0.5f);
        Angle = awayAngle;
        Acceleration = StgFrame.height / 3;
        SpeedLimit = StgFrame.height / 2.5f;
    }

    public static StgAct CreateBehaveAEnemy(GameObject original, float x, float advance, GameObject originalShot, float awayAngle)
    {
        StgAct clone = StgAct.ApparentInstantiate(original);

        clone.transform.position = new Vector2(x, StgFrame.topS);
        clone.StartCoroutine(clone.BehaveA(advance, originalShot, awayAngle));

        return clone;
    }


    IEnumerator BehaveB(float x, float arrivalY, float t, GameObject originalShot, float awayAcceleration, float awaySpeedLimit, float awayAngle)
    {
        transform.position = new Vector3(x, StgFrame.topS + BodyRadius);

        Acceleration = -2 * -(arrivalY - Y) / Mathf.Pow(t, 2);
        Speed = -Acceleration * t;
        SpeedLimit = 0;
        Angle = -90;
        MoveAngleLink = false;

        while (Speed != 0)
        {
            BodyAngle = Angle2Player;

            yield return null;
        }

        //FireWay(originalShot, 3, 5, StgFrame.height / 2.5f, bodyAngle);
        Fire(originalShot, StgFrame.height / 2.8f, BodyAngle);
        yield return Yielder.WaitForFixedSeconds(0.5f);
        MoveAngleLink = true;
        Angle = awayAngle;
        Acceleration = StgFrame.height / 3;
        SpeedLimit = StgFrame.height / 2.5f;
    }

    public static StgAct CreateBehaveBEnemy(GameObject original, float x, float arrivalY, float t, GameObject originalShot, float awayAcceleration, float awaySpeedLimit, float awayAngle)
    {
        StgAct clone = StgAct.ApparentInstantiate(original);

        clone.StartCoroutine(clone.BehaveB(x, arrivalY, t, originalShot, awayAcceleration, awaySpeedLimit, awayAngle));

        return clone;
    }

    /// <summary>
    /// 横に並ぶ奴
    /// </summary>
    /// <param name="original"></param>
    /// <param name="n"></param>
    /// <param name="firstX"></param>
    /// <param name="intervalX"></param>
    /// <param name="arrivalY"></param>
    /// <param name="firstT"></param>
    /// <param name="intervalT"></param>
    /// <param name="originalShot"></param>
    /// <param name="awayAcceleration"></param>
    /// <param name="awaySpeedLimit"></param>
    /// <param name="awayAngle"></param>
    /// <returns></returns>
    public static IEnumerator CreateFormationB(GameObject original, int n, float firstX, float intervalX, float arrivalY, float firstT, float intervalT, GameObject originalShot, float awayAcceleration, float awaySpeedLimit, float awayAngle)
    {
        for (var i = 0; i < n; i++)
        {
            StgAct clone = StgAct.ApparentInstantiate(original);
            clone.StartCoroutine(clone.BehaveB(firstX + intervalX * i, arrivalY, firstT - intervalT * i, originalShot, awayAcceleration, awaySpeedLimit, awayAngle));
            yield return Yielder.WaitForFixedSeconds(intervalT);
        }
    }

    public IEnumerator Behave001(float x, GameObject originalShot, float awayAngleSpeed)
    {
        transform.position = new Vector3(x, StgFrame.topS + BodyRadius);

        Speed = StgFrame.height / 2.7f;
        Angle = -90f;

        yield return Yielder.WaitForFixedSeconds(1f);

        FireRound(originalShot, 14, StgFrame.height / 2f, Mathfd.RandomAngle());
        yield return Yielder.WaitForFixedSeconds(0.4f);

        AngleSpeed = awayAngleSpeed;
    }

    public static StgAct CreateBehave001Enemy(GameObject originalEnemy, float x, GameObject originalShot, float awayAngleSpeed)
    {
        StgAct clone = StgAct.ApparentInstantiate(originalEnemy);

        clone.StartCoroutine(clone.Behave001(x, originalShot, awayAngleSpeed));

        return clone;
    }

    public IEnumerator Behave002(float x, GameObject originalShot)
    {
        transform.position = new Vector3(x, StgFrame.topS + BodyRadius);
        Speed = StgFrame.height / 2.2f;
        Angle = -90;

        yield return Yielder.WaitForFixedSeconds(0.5f);

        MoveAngleLink = false;
        StartCoroutine(BodyRoll(370f));
        while (true)
        {
            yield return Yielder.WaitForFixedSeconds(0.12f);
            FireWay(originalShot, 3, 1.2f, StgFrame.height / 3, BodyAngle);
        }
    }

    public IEnumerator BodyRoll(float bodyAngleSpeed)
    {
        while (true)
        {
            BodyAngle += bodyAngleSpeed * TimeManager.DeltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// くるくる
    /// </summary>
    /// <param name="originalEnemy"></param>
    /// <param name="x"></param>
    /// <param name="originalShot"></param>
    /// <returns></returns>
    public static StgAct CreateBehave002Enemy(GameObject originalEnemy, float x, GameObject originalShot)
    {
        StgAct clone = StgAct.ApparentInstantiate(originalEnemy);

        clone.StartCoroutine(clone.Behave002(x, originalShot));

        return clone;
    }

    public IEnumerator Behave003(float x, float speed, float angle, float fireTime, GameObject originalShot)
    {
        transform.position = new Vector3(x, StgFrame.topS + BodyRadius);
        this.Speed = speed;
        this.Angle = angle;

        yield return Yielder.WaitForFixedSeconds(fireTime);

        FireRound(originalShot, 5, StgFrame.height / 1.8f, Angle2Player);
    }

    public static StgAct CreateBehave003Enemy(GameObject originalEnemy, float x, float speed, float angle, float fireTime, GameObject originalShot)
    {
        StgAct clone = StgAct.ApparentInstantiate(originalEnemy);

        clone.StartCoroutine(clone.Behave003(x, speed, angle, fireTime, originalShot));

        return clone;
    }


    /// <summary>
    /// ドバーッ
    /// </summary>
    /// <param name="originalEnemy"></param>
    /// <param name="x"></param>
    /// <param name="firstSpeed"></param>
    /// <param name="intervalSpeed"></param>
    /// <param name="angle"></param>
    /// <param name="fireTime"></param>
    /// <param name="originalShot"></param>
    /// <param name="n"></param>
    /// <param name="createIntervalTime"></param>
    /// <returns></returns>
    public static IEnumerator CreateFormation003(GameObject originalEnemy, float x, float firstSpeed, float intervalSpeed, float angle, float fireTime, GameObject originalShot, int n, float createIntervalTime)
    {
        for (var i = 0; i < n; i++)
        {
            CreateBehave003Enemy(originalEnemy, x, firstSpeed - intervalSpeed * i, angle, fireTime, originalShot);

            yield return Yielder.WaitForFixedSeconds(createIntervalTime);
        }
    }

    public IEnumerator Fire2PlayerBehave(GameObject originalShot, float speed)
    {
        Fire(originalShot, speed, Angle2Player);
        yield return Yielder.WaitForFixedSeconds(0f);
    }

    public IEnumerator Behave004(float lr, float y, float arrivalX, float arrivalT, IEnumerator fire, float wait, float awayAcceleration, float awaySpeedLimit, bool lookAtPlayer = true)
    {
        Coroutine lap = null;

        {
            float s;

            if (lr < 0)
            {
                transform.position = new Vector3(StgFrame.leftS - BodyRadius, y);
                s = arrivalX - X;
                Angle = 0;
            }
            else
            {
                transform.position = new Vector3(StgFrame.rightS + BodyRadius, y);
                s = X - arrivalX;
                Angle = -180;
            }

            Acceleration = -2 * s / Mathf.Pow(arrivalT, 2);
        }
        Speed = -Acceleration * arrivalT;
        SpeedLimit = 0;

        while (Speed != 0)
            yield return null;

        if (lookAtPlayer)
            lap = StartCoroutine(LookAtPlayer());

        yield return fire;

        yield return Yielder.WaitForFixedSeconds(wait);

        if (lookAtPlayer)
            StopCoroutine(lap);

        Angle = BodyAngle;
        MoveAngleLink = true;
        Acceleration = awayAcceleration;
        SpeedLimit = awaySpeedLimit;
    }

    public IEnumerator LookAtPlayer()
    {
        MoveAngleLink = false;

        while (true)
        {
            BodyAngle = Angle2Player;
            yield return null;
        }
    }

    public static StgAct CreateBehave004EnemyA(GameObject originalEnemy, float lr, float y, float arrivalX, float arrivalT, GameObject originalShot, float wait, float awayAcceleration, float awaySpeedLimit)
    {
        StgAct clone = StgAct.ApparentInstantiate(originalEnemy);

        clone.StartCoroutine(clone.Behave004(lr, y, arrivalX, arrivalT, clone.Fire2PlayerBehave(originalShot, StgFrame.height / 2f), wait, awayAcceleration, awaySpeedLimit));

        return clone;
    }

    /// <summary>
    /// 横から出てきてFireRound
    /// </summary>
    /// <param name="originalEnemy"></param>
    /// <param name="lr"></param>
    /// <param name="y"></param>
    /// <param name="arrivalX"></param>
    /// <param name="originalShot"></param>
    /// <returns></returns>
    public static StgAct CreateBehave004EnemyB(GameObject originalEnemy, float lr, float y, float arrivalX, GameObject originalShot)
    {
        StgAct clone = StgAct.ApparentInstantiate(originalEnemy);

        clone.StartCoroutine(clone.Behave004(lr, y, arrivalX, 0.8f, clone.FireRoundBehave(originalShot, 10, StgFrame.height / 3f, Mathfd.RandomAngle()), 0.5f, StgFrame.height / 3, StgFrame.height / 2));

        return clone;
    }

    public IEnumerator FireRoundBehave(GameObject original, int num, float speed, float angle)
    {
        FireRound(original, num, speed, angle);
        yield return null;
    }

    public IEnumerator FireBehave002(GameObject original, int roundN, int lineN, float speed, float lineIntervalT, int roundCount)
    {
        for (var i = 0; i < roundCount; i++)
        {
            float angle = Mathfd.RandomAngle();
            for (var j = 0; j < lineN; j++)
            {
                FireRound(original, roundN, speed, angle);
                yield return Yielder.WaitForFixedSeconds(lineIntervalT);
            }
        }
    }

    public static StgAct CreateBehave004EnemyC(GameObject originalEnemy, float lr, float y, float arrivalX, GameObject originalShot, int roundN, int lineN, float speed, float lineIntervalT, int roundCount)
    {
        StgAct clone = StgAct.ApparentInstantiate(originalEnemy);

        clone.StartCoroutine(clone.Behave004(lr, y, arrivalX, 0.8f, clone.FireBehave002(originalShot, roundN, lineN, speed, lineIntervalT, roundCount), 0.5f, StgFrame.height / 3, StgFrame.height / 2));

        return clone;
    }


    public IEnumerator Behave101(GameObject originalShot)
    {
        while (true)
        {
            FireRoundR210(originalShot, 5, Random.Range(0, StgFrame.halfWidth / 3), Mathfd.RandomAngle(), StgFrame.halfWidth / 40, StgFrame.height / 2, Mathfd.RandomAngle(), -StgFrame.height / 1, StgFrame.height / 2.5f);
            yield return Yielder.WaitForFixedSeconds(0.2f);
        }
    }

    public IEnumerator Behave007(float x, float y, float angle, GameObject originalShot)
    {
        return Behave007(new Vector2(x, y), angle, originalShot);
    }

    public IEnumerator Behave007(Vector2 position, float angle, GameObject originalShot)
    {
        transform.position = position;
        Speed = StgFrame.height / 2f;
        this.Angle = angle;

        while (true)
        {
            yield return Yielder.WaitForFixedSeconds(0.06f);
            Fire(originalShot, StgFrame.height / 1.5f, angle + 180f + Mathfd.Shake(30f));
        }
    }

    public IEnumerator Behave007(Transform transform, float angle, GameObject originalShot)
    {
        return Behave007(transform.position, angle, originalShot);
    }

    public StgAct CreateBehave007Actor(GameObject originalBody, float x, float y, float angle, GameObject originalShot)
    {
        return CreateBehave007Actor(originalBody, new Vector2(x, y), angle, originalShot);
    }

    public StgAct CreateBehave007Actor(GameObject originalBody, Vector2 position, float angle, GameObject originalShot)
    {
        var clone = StgAct.ApparentInstantiate(originalBody);

        clone.StartCoroutine(clone.Behave007(position, angle, originalShot));

        return clone;
    }

    public StgAct CreateBehave007Actor(GameObject originalBody, Transform transform, float angle, GameObject originalShot)
    {
        return CreateBehave007Actor(originalBody, transform.position, angle, originalShot);
    }

    // ボス1号挙動
    public IEnumerator Behave100Enemy(GameObject originalShot)
    {
        var enemy = GetComponent<Enemy>();

        transform.position = new Vector3(StgFrame.PX(0), StgFrame.topS + BodyRadius);
        SetDestAtTime(new Vector2(X, StgFrame.PY(0.6f)), 1f);
        yield return WaitForStop();
        MoveAngleLink = false;

        StartCoroutine(Behave101(originalShot));

        while (true)
        {
            for (var lr = -1; lr <= 1; lr += 2)
            {
                SetDestAtTime(StgFrame.PX(Random.Range(0.4f, 0.7f) * lr), StgFrame.PY(Random.Range(0.4f, 0.7f)), 2.5f);

                if (enemy.GetHPP() < 0.5f)
                {
                    yield return Yielder.WaitForFixedSeconds(1f);
                    for (var lr2 = -1; lr2 <= 1; lr2 += 2)
                    {
                        CreateBehave007Actor(originalShot, transform, Angle2Player + 30f * lr2, originalShot);
                        CreateBehave007Actor(originalShot, transform, Angle2Player + 42f * lr2, originalShot);
                    }
                }

                yield return WaitForStop();

                CreateBehave007Actor(originalShot, transform, Angle2Player, originalShot);

                yield return Yielder.WaitForFixedSeconds(0.5f);
            }
        }
    }


    public IEnumerator Behave008(float x, float arrivalY, GameObject originalShot, float awayAngle = -90f, bool another = false)
    {
        transform.position = new Vector3(x, StgFrame.topS + BodyRadius);
        SetDestAtTime(x, arrivalY, 1);
        yield return WaitForStop();

        Angle = Angle2Player;
        FireWay(originalShot, 3, 5, StgFrame.height / 1.8f, Angle);
        yield return Yielder.WaitForFixedSeconds(0.5f);
        if (another)
            yield return Yielder.WaitForFixedSeconds(0.5f);

        Acceleration = StgFrame.height / 2f;
        SpeedLimit = StgFrame.height / 1f;
        Angle = awayAngle;

        if (another)
        {
            for (var i = 0; i < 3; i++)
            {
                yield return Yielder.WaitForFixedSeconds(0.3f);
                FireRoundWay(originalShot, 6, 2, 3f, StgFrame.height / 2.8f, Mathfd.RandomAngle());
            }
        }
    }

    /// <summary>
    /// 上からまっすぐarrivalYまで降りる→3wayでoriginalShot発射→awayAngleの方へ去る。
    /// anotherをtrueで去りながら弾発射
    /// </summary>
    /// <param name="originalEnemy"></param>
    /// <param name="x">出現位置x</param>
    /// <param name="arrivalY"></param>
    /// <param name="originalShot"></param>
    /// <param name="awayAngle"></param>
    /// <param name="another"></param>
    /// <returns></returns>
    public static StgAct CreateBehave008Enemy(GameObject originalEnemy, float x, float arrivalY, GameObject originalShot, float awayAngle = -90f, bool another = false)
    {
        var clone = StgAct.ApparentInstantiate(originalEnemy);

        clone.StartCoroutine(clone.Behave008(x, arrivalY, originalShot, awayAngle, another));

        return clone;
    }

    public IEnumerator Behave009(float x, float arrivalY, float sX, float t, GameObject originalShot)
    {
        transform.position = new Vector3(x, StgFrame.topS + BodyRadius);
        SetDestAtTime(this.X, arrivalY, t);

        {
            var curT = 0f;
            while (Speed != 0)
            {
                yield return null;
                curT += TimeManager.DeltaTime;
                this.X = x + sX / 2 + sX / 2 * Mathfd.Cos(-180f + 180f * curT / t);
            }
        }

        for (var i = 0; i < 3; i++)
        {
            Fire(originalShot, StgFrame.height / 1.8f, -90f);
            yield return Yielder.WaitForFixedSeconds(0.25f);
        }

        Acceleration = StgFrame.height / 4;
        SpeedLimit = StgFrame.height / 1;
    }

    /// <summary>
    /// 広がるのに使うやつ
    /// </summary>
    /// <param name="originalEnemy"></param>
    /// <param name="x"></param>
    /// <param name="arrivalY"></param>
    /// <param name="sX"></param>
    /// <param name="t"></param>
    /// <param name="originalShot"></param>
    /// <returns></returns>
    public static StgAct CreateBehave009Enemy(GameObject originalEnemy, float x, float arrivalY, float sX, float t, GameObject originalShot)
    {
        var clone = StgAct.ApparentInstantiate(originalEnemy);

        clone.StartCoroutine(clone.Behave009(x, arrivalY, sX, t, originalShot));

        return clone;
    }

    /// <summary>
    /// 広がるやつ
    /// </summary>
    /// <param name="originalEnemy"></param>
    /// <param name="num"></param>
    /// <param name="x"></param>
    /// <param name="arrivalY"></param>
    /// <param name="sX"></param>
    /// <param name="t"></param>
    /// <param name="originalShot"></param>
    /// <returns></returns>
    public static void CreateFormation009(GameObject originalEnemy, int num, float x, float arrivalY, float sX, float t, GameObject originalShot)
    {
        for (var i = 0; i < num; i++)
        {
            CreateBehave009Enemy(originalEnemy, x, arrivalY, -sX + sX * 2 / (num - 1) * i, t, originalShot);
        }
    }

    //public IEnumerator Behave010(float lr,float fromY,float toX,float toY,float t,bool another = false)
    //{
    //    transform.position = new Vector3(StgFrame.centerX + (StgFrame.halfWidth + StgFrame.shakeRange + bodyR) * lr, fromY);
    //    SetDestAtTime(toX, toY, t);
    //    //move
    //    while (speed != 0)
    //    {

    //    }
    //}

    public IEnumerator Behave200(GameObject originalShot)
    {
        var enemy = GetComponent<Enemy>();

        transform.position = new Vector3(StgFrame.centerX, StgFrame.topS + BodyRadius);
        SetDestAtTime(X, StgFrame.PY(0.7f), 1);
        yield return WaitForStop();

        while (enemy.GetHPP() > 0.6f)
        {
            for (var lr = -1; lr <= 1; lr += 2)
            {
                for (var i = 0; i < 3; i++)
                {
                    FireWay(originalShot, 15, 1, StgFrame.height / 2.5f, Angle2Player);
                    yield return Yielder.WaitForFixedSeconds(0.46f);
                }
                yield return Behave201(originalShot, lr, true);
                yield return Yielder.WaitForFixedSeconds(1);
                FireRound(originalShot, 30, StgFrame.height / 3.2f, Mathfd.RandomAngle());
                yield return Yielder.WaitForFixedSeconds(0.8f);
                yield return Behave201(originalShot, lr);
                yield return Behave201(originalShot, -lr, true);
                yield return Yielder.WaitForFixedSeconds(1);
            }
        }

        while (enemy.GetHPP() > 0f)
        {
            for (var lr = -1; lr <= 1 && enemy.GetHPP() > 0f; lr += 2)
            {
                for (var lr2 = -1; lr2 <= 1; lr2 += 2)
                {
                    SetDestAtTime(StgFrame.PX(Random.Range(0.4f, 0.7f) * lr * lr2), StgFrame.PY(Random.Range(0.7f, 0.8f)), 2);
                    for (var i = 0; i < 6; i++)
                    {
                        yield return Yielder.WaitForFixedSeconds(0.2f);
                        FireRoundWay(originalShot, 3, 3, 3, StgFrame.height / 2.2f, Mathfd.RandomAngle());
                    }
                    yield return WaitForStop();
                    SetDestAtTime(X, StgFrame.PY(-0.8f), 0.7f);
                    yield return WaitForStop();
                    for (var i = 0; i < 2; i++)
                    {
                        CreateBehave007Actor(originalShot, transform, 90 + Mathfd.Shake(2), originalShot);
                        yield return Yielder.WaitForFixedSeconds(0.5f);
                    }
                }

                SetDestAtTime(StgFrame.centerX, StgFrame.centerY, 3);
                yield return WaitForStop();
                Angle = -90;

                {
                    float speed = StgFrame.height * 0.8f;
                    float interval = 1.4f;
                    for (var i = 0; i < 20; i++)
                    {
                        for (var j = 0; j < 8; j++)
                        {
                            Fire(originalShot, Random.Range(speed, speed * 0.6f), Angle2Player + Mathfd.Shake(4));
                        }
                        yield return Yielder.WaitForFixedSeconds(interval);
                        speed = Mathf.Min(StgFrame.height * 1.4f, speed + StgFrame.height * 0.2f);
                        interval = Mathf.Max(0.2f, interval - 0.3f);
                    }
                }
                yield return Yielder.WaitForFixedSeconds(1f);


            }
        }
    }

    public IEnumerator Behave201(GameObject originalShot, float lr = 1, bool another = false)
    {
        float lAngle = Mathfd.RandomAngle();
        var sAngle = 0f;
        var num = 4;
        var t = 1f;
        var interval = t / 40;

        if (another)
            sAngle = 360 / num / 2;

        for (var i = 0; i < 30; i++)
        {
            FireRoundR2102Player(originalShot, num, StgFrame.halfWidth * 0.45f, lAngle, StgFrame.halfWidth * 0.05f, StgFrame.height / 10, sAngle, StgFrame.height / 0.8f, StgFrame.height / 1.5f);
            yield return Yielder.WaitForFixedSeconds(interval);
            lAngle += 11 * lr;
            sAngle -= 360 / num / 3 * lr;
        }
    }

    #endregion
}