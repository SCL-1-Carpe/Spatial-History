using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using State;
using UnityEngine.Events;
using Utils;

public class MainManager : MonoBehaviour
{
    #region フィールド
    StateProcessor stateProcessor = new StateProcessor();

    [SerializeField] Transform controllerTip;

    public float rayDistance = 10f;

    [SerializeField] Text explainText;

    [SerializeField] RectTransform indicater;

    [SerializeField] GameObject nodeIndicater;

    [SerializeField] GameObject nodeSelectEffect;

    [SerializeField] GameObject _3dStartButton;
    [SerializeField] GameObject _3dRebootButton;

    [SerializeField] GameObject dissolveBuilding;

    [SerializeField] Text AwakeText;

    [SerializeField] CharacterController characterController;

    private GameObject freeChoiceTargetNode;

    [SerializeField] Transform sphereDissolve;
    [SerializeField] GameObject firstOsakajoObj;


    public float shrinkTime = 0.2f;
    public float smallScale = 0.9f;
    public float largeScale = 1.2f;
    public float shrinkScale_Min = 0.23f;
    public float largeScale_Max = 1.5f;
    public float planeSpeed = 0.15f;

    private bool isFreeChoice = false;

    private bool isAwakePress = false;

    public float awakeWaitSpeed = 2.42f;
    public float awakeSpeed;
    public float arrowSpeed = 0.005f;
    private bool canAllInit = false;

    [SerializeField] Transform plane_UI;
    [SerializeField] Transform planeStart;

    [Header("Audio"), Header("----------")]
    [SerializeField] AudioSource audio_SE;
    [SerializeField] AudioSource audio_Voice;
    [SerializeField] AudioClip clip_Select, clip_PreSelect;
    [SerializeField] AudioClip clip_Intro, clip_OpeExplain;
    [SerializeField] AudioClip clip_worldMake;

    [Header("城")]
    public GameObject[] castles;
    [Header("板")]
    public GameObject[] planes;
    [Header("時代テキスト")]
    public Text[] eraTexts;
    public Material nodeInactiveMaterial, nodeActiveMaterial;
    
    #region 時代の構造体
    [System.Serializable]
    public struct Node
    {
        [Header("時代")]
        public int era;
        [Header("説明"), TextArea(1, 3)]
        public string explain;
        [Header("ボイス")]
        public AudioClip vc;
        [Header("サウンドエフェクト")]
        public AudioClip se;
        [Header("Nodeのトランスフォーム")]
        public Transform transform;
        [Header("ディゾルブ用のパネル")]
        public Transform panel;
        [Header("パネルのターゲット位置")]
        public Transform destination;
        [Header("アセット")]
        public GameObject[] assets;
        [NonSerialized]
        public int myIndex;
        public UnityEvent process;
    }
    public Node[] nodes;
    [NonSerialized]
    public Node currentNode;
    #endregion

    private Dictionary<string, int> nodeDictionary = new Dictionary<string, int>();
    #endregion

    /**********************************************************************************/

    #region イニシャライズ
    public void Init()
    {
        //ノードが時系列になるようにソート => 今回はいらない
        //Array.Sort(nodes, (a, b) => a.era - b.era);
        //タグと索引の初期化
        for (int i = 0; i < nodes.Length; i++)
        {
            nodes[i].transform.tag = "InActiveNode";
            nodes[i].myIndex = i;
            nodeDictionary[nodes[i].transform.name] = nodes[i].myIndex;
            //Utility.ChangeObjColor(nodes[i].transform.gameObject, Color.red);
            nodes[i].transform.gameObject.GetComponent<Renderer>().material = nodeInactiveMaterial;
            eraTexts[i].transform.Rotate(90, 0, 0);
            eraTexts[i].transform.gameObject.SetActive(false);
        }
        currentNode = nodes[0];

        //パネルUIのイニシャライズ
        Utility.Alignment(plane_UI, planeStart);
        //初期舞台セット
        Utility.SetStage(currentNode.era, castles, planes);

        explainText.text = string.Empty;
        _3dStartButton.SetActive(true);
        indicater.gameObject.SetActive(false);
        SetNodeEffect(NodeEffect.INIT);
        //初期コンテンツを開始
        stateProcessor.SetState(ST_Start);
    }
    #endregion

    void Start()
    {
        stateProcessor.SetState(ST_Awake);
    }

    void Update()
    {
        stateProcessor.Update();
    }

    #region アウェイクステート
    public void ST_Awake(bool isFirst)
    {
        if (isFirst)
        {
            StartCoroutine(GameAwake());
        }
        else
        {
            //ここに「Press」の点滅とか
            if (!isAwakePress)
            {
                sphereDissolve.localScale += Vector3.one * 0.0006f * Mathf.Sin(Time.realtimeSinceStartup * awakeWaitSpeed);
                AwakeText.color = new Color(AwakeText.color.r, AwakeText.color.g, AwakeText.color.b, 0.5f + 0.35f * Mathf.Sin(Time.realtimeSinceStartup * 1.5f));
            }
        }
    }
    #endregion

    #region スタートステート
    public void ST_Start(bool isFirst)
    {
        //初期処理
        if (isFirst)
        {
            isFreeChoice = false;
        }
        //継続処理
        else
        {
            //他に何かあれば
            stateProcessor.SetState(ST_Play);
        }
    }
    #endregion

    #region プレイステート
    public void ST_Play(bool isFirst)
    {
        //初期処理
        if (isFirst)
        {

        }
        //継続処理
        else
        {
            //説明の赤印の回転
            indicater.Rotate(new Vector3(2f, 0, 0));
        }
    }
    #endregion

    #region フリーチョイスステート
    public void ST_FreeChoice(bool isFirst)
    {
        //初期処理
        if (isFirst)
        {
            isFreeChoice = true;
        }
        //継続処理
        else
        {
            //説明の赤印の回転
            indicater.Rotate(new Vector3(2f, 0, 0));
            Utility.Alignment(nodeSelectEffect, freeChoiceTargetNode);
            //Rayはコントローラの先端から照射
            Ray ray = new Ray(controllerTip.position, controllerTip.forward);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, rayDistance))
            {
                GameObject hitObj = hitInfo.collider.gameObject;

                if (hitObj.tag == "FreeChoiceNode" && freeChoiceTargetNode.name != hitObj.name)
                {
                    freeChoiceTargetNode = hitObj;
                    Utility.ChangeObjColor(nodeSelectEffect, Color.white);
                    audio_SE.PlayOneShot(clip_PreSelect);
                    explainText.text = nodes[nodeDictionary[freeChoiceTargetNode.name]].explain;
                }
            }
        }
    }
    #endregion

    #region 1585年
    public void Event_1585() { StartCoroutine(Process_1585(currentNode)); }
    IEnumerator Process_1585(Node node)
    {
        Utility.SetStage(node.era, castles, planes);
        UpdateAudioAndUI(node);
        yield return PlaneLiftDown(node, 0.15f);
        if (!isFreeChoice) UpdateNode();
    }
    #endregion

    #region 1615年
    public void Event_1615() { StartCoroutine(Process_1615(currentNode)); }
    IEnumerator Process_1615(Node node)
    {
        GameObject fire = node.assets[0], warAnim = node.assets[1];
        Utility.SetStage(node.era, castles, planes);
        UpdateAudioAndUI(node);
        //開戦
        warAnim.SetActive(true);
        yield return ReleaseArrow(node.assets[2], node.assets[3]);

        //着火
        fire.SetActive(true);
        yield return PlaneLiftDown(node, 0.2f);

        //鎮火、非アクティブ化
        fire.SetActive(false);
        warAnim.SetActive(false);

        if (!isFreeChoice) UpdateNode();
    }
    #endregion

    #region 1626年
    public void Event_1626() { StartCoroutine(Process_1626(currentNode)); }
    IEnumerator Process_1626(Node node)
    {
        Utility.SetStage(node.era, castles, planes);
        UpdateAudioAndUI(node);
        yield return PlaneLiftUp(node, 0.15f);
        if (!isFreeChoice) UpdateNode();
    }
    #endregion

    #region 1665年
    public void Event_1665() { StartCoroutine(Process_1665(currentNode)); }
    IEnumerator Process_1665(Node node)
    {
        GameObject lightnig = node.assets[0], cloud = node.assets[1], fire = node.assets[2];
        Transform dissolveShere = node.panel;
        Utility.SetStage(node.era, castles, planes);
        UpdateAudioAndUI(node);
        //落雷
        lightnig.SetActive(true);
        cloud.SetActive(true);
        //落ちるまで待つ
        yield return new WaitForSeconds(1.74f);
        dissolveShere.localScale = Vector3.one * 0.334f;
        //火災
        fire.SetActive(true);
        yield return new WaitForSeconds(2f);
        //非アクティブなど
        fire.SetActive(false);
        dissolveShere.localScale = Vector3.one * 0.0001f;
        lightnig.SetActive(false);
        cloud.SetActive(false);
        castles[4].SetActive(false);
        if (!isFreeChoice) UpdateNode();
    }
    #endregion

    #region 1931年
    public void Event_1931() { StartCoroutine(Process_1931(currentNode)); }
    IEnumerator Process_1931(Node node)
    {
        Utility.SetStage(node.era, castles, planes);
        UpdateAudioAndUI(node);
        yield return PlaneLiftUp(node, 0.15f);
        if (!isFreeChoice) UpdateNode();
    }
    #endregion

    #region サブルーチン

    IEnumerator GameAwake()
    {
        audio_Voice.PlayOneShot(clip_Intro);
        yield return new WaitForSeconds(5f);
        AwakeText.transform.parent.gameObject.SetActive(true);
        
        //トリガー待ち
        while (!isAwakePress) yield return null;

        audio_SE.PlayOneShot(clip_worldMake);
        //ジオラマの形成
        while (sphereDissolve.localScale.x < 6f)
        {
            sphereDissolve.localScale += Vector3.one * awakeSpeed;
            yield return null;
        }
        castles[0].SetActive(true);
        firstOsakajoObj.SetActive(false);
        Init();
        audio_Voice.PlayOneShot(clip_OpeExplain);
    }

    IEnumerator PlaneLiftUp(Node node, float speed)
    {
        while (node.panel.position.y < node.destination.position.y)
        {
            node.panel.position += new Vector3(0, Time.deltaTime * speed, 0);
            yield return null;
        }
    }

    IEnumerator PlaneLiftDown(Node node, float speed)
    {
        while (node.panel.position.y > node.destination.position.y)
        {
            node.panel.position -= new Vector3(0, Time.deltaTime * speed, 0);
            yield return null;
        }
    }

    IEnumerator ActiveNextNode(Node node, Transform plane)
    {
        //縮小化
        yield return Shrink();
        //移動
        while (Vector3.Distance(plane.position, node.transform.position) > 0.007f)
        {
            Vector3 vec = (node.transform.position - plane.position).normalized;
            plane.position += vec * Time.deltaTime * planeSpeed;
            yield return null;
        }
        Utility.Alignment(plane, node.transform);
        //拡大化
        yield return EnLarge(node);
    }

    IEnumerator Shrink()
    {
        while (nodeIndicater.transform.localScale.x > shrinkScale_Min)
        {
            nodeIndicater.transform.localScale *= smallScale;
            yield return null;
        }
        nodeIndicater.transform.localScale = Vector3.one * shrinkScale_Min;
    }

    IEnumerator EnLarge(Node node)
    {
        eraTexts[node.myIndex].transform.gameObject.SetActive(true);
        float theta = 90f / 11f;
        while (nodeIndicater.transform.localScale.x < largeScale_Max)
        {
            nodeIndicater.transform.localScale *= largeScale;
            eraTexts[node.myIndex].transform.Rotate(-theta, 0, 0);
            yield return null;
        }
        nodeIndicater.transform.localScale = Vector3.one * largeScale_Max;
    }

    IEnumerator InvisibleUIButton(GameObject button, Color color)
    {
        yield return new WaitForSeconds(1f);
        Utility.ChangeObjColor(button, color);
        button.SetActive(false);
    }

    IEnumerator ReleaseArrow(GameObject arrow1, GameObject arrow2)
    {
        Vector3 arrow1Pos = arrow1.transform.position;
        Vector3 arrow2Pos = arrow2.transform.position;
        yield return new WaitForSeconds(2f);
        arrow1.SetActive(true);
        arrow2.SetActive(true);
        for (int i = 0; i < 20; i++)
        {
            arrow1.transform.Translate(0, arrowSpeed, arrowSpeed);
            arrow2.transform.Translate(0, arrowSpeed, arrowSpeed);
            yield return null;
        }
        arrow1.SetActive(false);
        arrow2.SetActive(false);
        arrow1.transform.position = arrow1Pos;
        arrow2.transform.position = arrow2Pos;

    }
    IEnumerator SetAllInit()
    {
        isAwakePress = false;
        _3dStartButton.transform.gameObject.SetActive(false);
        castles[0].SetActive(false);
        firstOsakajoObj.SetActive(true);
        audio_SE.PlayOneShot(clip_worldMake);
        while (sphereDissolve.localScale.x > 0.09f)
        {
            sphereDissolve.localScale -= Vector3.one * awakeSpeed;
            yield return null;
        }
        stateProcessor.SetState(ST_Awake);
    }
    #endregion

    #region コントローラ関連
    /// <summary>
    /// マジックリープのトリガーが引かれた瞬間に処理がされる
    /// </summary>
    public void ML_OnTriggerDown()
    {
        //Rayはコントローラの先端から照射
        Ray ray = new Ray(controllerTip.position, controllerTip.forward);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, rayDistance))
        {
            string tag = hitInfo.collider.tag;

            if (tag == "NextWaitNode")
            {
                indicater.gameObject.SetActive(true);
                //決定のSEを流す
                audio_SE.PlayOneShot(clip_Select);
                //各ノードに割り振られた処理を開始
                currentNode.process.Invoke();
                //タグを変えることによる選択できないようにする
                currentNode.transform.tag = "ActiveNode";
            }
            else if (tag == "FreeChoiceNode")
            {
                //決定のSEを流す
                audio_SE.PlayOneShot(clip_Select);
                //セレクトエフェクトのカラーを決定色(Green)にする
                Utility.ChangeObjColor(nodeSelectEffect, Color.green);
                //選択したオブジェクトからノードを検索してcurrentNodeに設定
                currentNode = nodes[nodeDictionary[hitInfo.collider.transform.name]];
                currentNode.process.Invoke();
            }
            else if (tag == "Init")
            {
                canAllInit = true;
                //決定のSEを流す
                audio_SE.PlayOneShot(clip_Select);
                Utility.ChangeObjColor(_3dRebootButton, Color.green);
                StartCoroutine(InvisibleUIButton(_3dRebootButton, Color.white));
                Init();
            }
            else if (tag == "Start")
            {
                canAllInit = false;
                //決定のSEを流す
                audio_SE.PlayOneShot(clip_Select);
                Utility.ChangeObjColor(_3dStartButton, Color.green);
                StartCoroutine(InvisibleUIButton(_3dStartButton, Color.white));
                SetNodeEffect(NodeEffect.AUTO);
                //ノードインディケーターを初期位置まで移動
                StartCoroutine(ActiveNextNode(nodes[0], plane_UI));
                nodes[0].transform.tag = "NextWaitNode";
                stateProcessor.SetState(ST_Play);
            }
            else WaitStart();
        }
        else WaitStart();
    }
    public void ML_OnBumperButton() {dissolveBuilding.transform.Translate(0, -0.01f, 0);}
    public void AllInit(){ if (canAllInit) StartCoroutine(SetAllInit());}
    #endregion

    #region ノードのアップデート関連
    /// <summary>
    /// 連結されているノードに更新する
    /// 最終ノードの場合はデストラクト処理
    /// </summary>
    public void UpdateNode()
    {
        //Utility.ChangeObjColor(currentNode.transform.gameObject, Color.blue);
        currentNode.transform.gameObject.GetComponent<Renderer>().material = nodeActiveMaterial;
        int nextIndex = currentNode.myIndex + 1;
        if (nextIndex < nodes.Length)
        {
            currentNode = nodes[nextIndex];
            currentNode.transform.tag = "NextWaitNode";
            //ノードインディケーターの移動
            StartCoroutine(ActiveNextNode(currentNode, plane_UI));
        }
        //最後のノードのイベントが終わったときはフリー選択モードとなる
        else
        {
            //パネルずらし
            plane_UI.localPosition += Vector3.right;
            FreeChoiceAvtivate();
        }
    }

    /// <summary>
    /// オーディと説明テキストのアップデート
    /// </summary>
    /// <param name="node"></param>
    public void UpdateAudioAndUI(Node node)
    {
        audio_SE.PlayOneShot(node.se);
        audio_Voice.PlayOneShot(node.vc);
        explainText.text = node.explain;
    }
    #endregion

    #region その他の関数
    public void WaitStart()
    {
        if (!isAwakePress)
        {
            isAwakePress = true;
            AwakeText.transform.parent.gameObject.SetActive(false);
        }
        else
            StartCoroutine(characterController.Wave());
    }
    /// <summary>
    /// ストーリーモードからフリーチョイスモードへ
    /// </summary>
    public void FreeChoiceAvtivate()
    {
        freeChoiceTargetNode = currentNode.transform.gameObject;
        SetNodeEffect(NodeEffect.FREE);
        //イニシャライズボタンのアクティブ化
        _3dRebootButton.SetActive(true);
        //アップデートモードの変更
        stateProcessor.SetState(ST_FreeChoice);
        //タグを変える
        for (int i = 0; i < nodes.Length; i++)
            nodes[i].transform.tag = "FreeChoiceNode";
    }

    public enum NodeEffect { INIT, AUTO, FREE }
    public void SetNodeEffect(NodeEffect ne)
    {
        switch (ne)
        {
            case NodeEffect.INIT:
                nodeIndicater.SetActive(false);
                nodeSelectEffect.SetActive(false);
                break;
            case NodeEffect.AUTO:
                nodeIndicater.SetActive(true);
                nodeSelectEffect.SetActive(false);
                break;
            case NodeEffect.FREE:
                nodeIndicater.SetActive(false);
                nodeSelectEffect.SetActive(true);
                break;
            default:
                break;
        }
    }
    #endregion
}
