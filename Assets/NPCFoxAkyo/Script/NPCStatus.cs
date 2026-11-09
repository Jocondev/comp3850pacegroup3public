
using System.Net.NetworkInformation;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]

/// <summary>
/// NPCの基本的な同期対象の情報(HP、たーげっとなど)
/// </summary>
public class NPCStatus : UdonSharpBehaviour
{

    //[SerializeField] private ParticleLaunch particle;
    // 基本的なNPC属性
    [UdonSynced(UdonSyncMode.None)][SerializeField] private float maxHP = 100f;     // MAXHP
    [UdonSynced(UdonSyncMode.None)][SerializeField] private float currentHP = 100f;     // HP
   [SerializeField] private float attackPower = 10f; //基礎攻撃力
   [SerializeField] private float speed = 1f;       //歩く速さ
   [SerializeField] private float runningSpeed = 2f;//走る速さ      

    //ステート駆動
    [UdonSynced(UdonSyncMode.None)][SerializeField] private int currentState = 0; //NPCStateHandler用状態管理

    // 追跡・ターゲット管理
    [UdonSynced(UdonSyncMode.None)][SerializeField] private int currentTargetPlayerID = 0;  // 追跡中のプレイヤーID
    //private VRCPlayerApi currentTargetPlayer;  // 追跡中のプレイヤー
    //private VRCPlayerApi previousTargetPlayer; // 前回のプレイヤー

    [UdonSynced(UdonSyncMode.None)][SerializeField] private int currentTargetObjectID = 0;  // 追跡中のオブジェクトID

    //NavMesh用
    [UdonSynced(UdonSyncMode.None)] private Vector3 currentTargetPoint;  //現在の目的地


    //視線誘導用
    [SerializeField] private float delayTime = 0.5f; // 視線誘導の遅延秒数
    [SerializeField] private float maxLookAtDistance = 5f; // 視線誘導をする最大距離
    [SerializeField] private float smoothTime = 0.5f; // 視線誘導ウェイトのスムーズさ
    [UdonSynced(UdonSyncMode.None)][SerializeField] private bool useLocalPlayerAsTarget = false; // 視線誘導のターゲットを強制的にローカルプレイヤーにするオプション
    [UdonSynced(UdonSyncMode.None)][SerializeField] private bool delayedLookAt = false; // 視線誘導がワンテンポ遅れるオプション
    [UdonSynced(UdonSyncMode.None)][SerializeField] private bool enableLookAt = true; // 視線誘導のON/OFF切り替え


    [SerializeField] private float maxChaseDistance = 20f; // 追跡可能な最大距離
    [SerializeField] private float chaseTimeLimit = 50f;   // 追跡を諦めるまでの最大時間

    // スタン状態
    [SerializeField] private float maxStunThreshold = 30f; // スタン最大耐久力
    [SerializeField] private float stunThreshold = 30f; // スタン耐久力（0になるとスタン）
    [SerializeField] private float stunTime = 5f; // スタン時間

    // 倒した時のスコア
    [SerializeField] private int scoreOnKill = 10;





    private void Start()
    {
        /*/==実験==========================================================================
        // ローカルプレイヤーのVRCPlayerApiを取得
        VRCPlayerApi localPlayer = Networking.LocalPlayer;

        // ローカルプレイヤーが存在する場合、currentTargetPlayerとpreviousTargetPlayerを設定
        if (localPlayer != null)
        {
            currentTargetPlayerID = localPlayer.playerId; // 現在のターゲットにローカルプレイヤーを設定
            previousTargetPlayerID = 0; // 以前のターゲットはまだ設定しない
            currentTargetPlayer = localPlayer;

        }

        // 現在のターゲットのヘッドボーン位置を更新
        UpdateLookatTargets();
        //============================================================================
        */
        RequestSerialization();
    }

    private void Update()
    {

       
    
    }




    /// <summary>
    /// ダメージによるHP減少,スタン値の減少(デフォルト0.0f)
    /// </summary>
    public void TakeDamage(float damage, float stunDamage = 0.0f)
    {
        //オーナ権限を委譲
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        //現在HPを減少
        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0); // HPがマイナスにならないようにする

        // スタン耐久値減少
        stunThreshold -= stunDamage;
        stunThreshold = Mathf.Max(stunThreshold, 0);  // スタン耐久値が0以下にならないようにする

        //Debug.Log($"NPC took {damage} damage. Current HP: {currentHP}");

        // HPとスタン耐久値の同期をリクエスト
        Sync();

        // オーナー側での更新処理
        OnStatusUpdated();
    }

    /// <summary>
    /// 引数のIDをもとにプレイヤーを参照する
    /// </summary>
    /// <returns>VRCPlayerApi</returns>
    public VRCPlayerApi GetPlayer(int PlayerID)
    {
        return VRCPlayerApi.GetPlayerById(PlayerID);
    }

    /// <summary>
    /// HPを回復
    /// </summary>
    public void Heal(float amount)
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        currentHP += amount;
        currentHP = Mathf.Min(currentHP, maxHP); // 現在HPがmaxHPを超えないように

        //Debug.Log($"NPC healed {amount} HP. Current HP: {currentHP}");

        Sync();
        OnStatusUpdated();
    }

    /// <summary>
    /// 現在のスタン耐久値取得
    /// </summary>
    /// <returns>スタン耐久値(float)</returns>
    public float GetStunThreshold() { return stunThreshold; }

    /// <summary>
    /// スタン耐久値回復
    /// </summary>
    public void MaxStunThreshold()
    {
        stunThreshold = maxStunThreshold;
    }


    /// <summary>
    /// 同期後のオーナー側の更新処理
    /// </summary>
    private void OnStatusUpdated()
    {
        // 何かUdonSynced変数の更新に付随した処理
    }



    public void Sync()
    {
        //同期タイミング確認用パーティクル
        //particle.LaunchParticle();


        RequestSerialization();
    }


    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        Debug.Log($"プレイヤーが退室しました: {player.displayName}");

        // 例：そのプレイヤーに関連する何かを解除・初期化
        if (player.playerId == GetTargetPlayerID())
        {
            currentTargetPlayerID = -1;
            Debug.Log("ターゲットプレイヤーが退出したため、ターゲットをクリアしました。");
        }
        Sync();
    }


    //=======Getter Setter=========================================================


    /// <summary>
    /// 現在HP取得
    /// </summary>
    /// <returns>現在のHP(float)</returns>
    public float GetCurrentHP() { return currentHP; }

    /// <summary>
    /// MaxHP取得
    /// </summary>
    /// <returns>MAXHP(float)</returns>
    public float GetMaxHP() { return maxHP; }

    /// <summary>
    /// 現在HPをセット
    /// </summary>
    public void SetCurrentHP(float currentHP)
    {
        //オーナ権限を委譲
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        this.currentHP = currentHP;

        Sync();
    }

    /// <summary>
    /// MAXHPをセット
    /// </summary>
    public void SetMaxHP(float maxHP)
    {
        //オーナ権限を委譲
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        this.maxHP = maxHP;

        Sync();
    }


    /// <summary>
    /// ターゲットプレイヤーをセット
    /// </summary>
    public void SetTargetPlayer(VRCPlayerApi newTargetPlayer)
    {

        //オーナ権限を委譲
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        if (newTargetPlayer == null || !newTargetPlayer.IsValid()) {
            return;
        }
        // currentを新しいターゲットに更新
        currentTargetPlayerID = newTargetPlayer.playerId;
       // currentTargetPlayer = newTargetPlayer;

        Sync();
    }

    public int GetTargetPlayerID() {  return currentTargetPlayerID; }


    /// <summary>
    /// ステートを取得
    /// </summary>
    public int GetCurrentState()
    {
        return currentState;
    }

    /// <summary>
    /// ステートをセット
    /// IDLE:0 PATROLLING:1 CHASING:2 ACCOMPANYING:3 FLEEING:4 
    ///COMBAT:5 STUNNED:6 DEAD:7 DAMAGED:8 KNOCKED_BACK:9
    /// </summary>
    public void SetCurrentState(int currentState)
    {
        //オーナ権限を委譲
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        this.currentState = currentState;


        Sync();
    }

    /// <summary>
    /// オブジェクトのターゲットを更新
    /// </summary>
    public void SetCurrentTargetObjectID(int objectID)
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        currentTargetObjectID = objectID;

        Sync();
    }
    /// <summary>
    /// 追跡していないときの目的地を取得
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCurrentTargetPoint() {
        return currentTargetPoint;
    }



    /// <summary>
    /// 追跡していないときの目的地をセット
    /// </summary>
    /// <param name="currentTargetPoint"></param>
    public void SetCurrentTargetPoint(Vector3 currentTargetPoint)
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        this.currentTargetPoint = currentTargetPoint;


        Sync();
    }

    public void SetSpeed(float speed) {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        this.speed = speed;

        Sync();
    }
    public float GetSpeed()
    {
        return speed;
    }


    public void SetRunningSpeed(float runningSpeed)
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        this.runningSpeed = runningSpeed;

        Sync();



    }
    public float GetRunningSpeed()
    {
        return runningSpeed;
    }




    // delayedLookAt のゲッター
    public bool GetDelayedLookAt()
    {
        return delayedLookAt;
    }

    // delayedLookAt のセッター
    public void SetDelayedLookAt(bool value)
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        delayedLookAt = value;
        Sync();
    }

    // enableLookAt のゲッター
    public bool GetEnableLookAt()
    {
        return enableLookAt;
    }

    // enableLookAt のセッター
    public void SetEnableLookAt(bool value)
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        enableLookAt = value;

        Sync();
    }

    public float GetSmoothTime() { 
        return smoothTime;
    }

    public void SetSmoothTime(float smoothTime)
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        this.smoothTime = smoothTime;

        Sync();
    }

    public float GetmaxLookAtDistance() { 
        return maxLookAtDistance;
    }

    public float GetDelayTime() { 
        return delayTime;
    }
}