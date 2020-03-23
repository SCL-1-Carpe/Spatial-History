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

    public float distance = 10f;

    [SerializeField] Text explainText;

    [SerializeField] RectTransform indicater;

    [SerializeField] GameObject nodeIndicater;

    [SerializeField] GameObject nodeSelectEffect;

    [SerializeField] GameObject _3dStartButton;
    [SerializeField] GameObject _3dRebootButton;

    [SerializeField] GameObject decideEffect;

    [SerializeField] GameObject dissolveBuilding;

    private GameObject decideEffectTargetNode;
    private GameObject freeChoiceTargetNode;


    public float shrinkTime = 0.2f;
    public float smallScale = 0.9f;
    public float largeScale = 1.2f;
    public float shrinkScale_Min = 0.23f;
    public float largeScale_Max = 1.5f;
    public float panelSpeed = 0.07f;

    [SerializeField] Transform plane_UI;
    [SerializeField] Transform planeStart;

    [SerializeField] AudioSource audio_SE;
    [SerializeField] AudioSource audio_Voice;
    [SerializeField] AudioClip select;
    [SerializeField] AudioClip preselect;
    [SerializeField] AudioClip initVoice;

    [Header("城")]
    public GameObject[] castles;
    [Header("板")]
    public GameObject[] planes;
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

    private bool isFreeChoice = false;

    [SerializeField] Text[] eraTexts;


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
            nodes[i].transform.gameObject.GetComponent<Renderer>().material = nodeInactiveMaterial;
            eraTexts[i].transform.Rotate(90, 0, 0);
            eraTexts[i].color = new Color(255f, 255f, 255f, 0);
        }
        currentNode = nodes[0];

        //パネルUIのイニシャライズ
        Utility.Alignment(plane_UI, planeStart);
        //初期舞台セット
        Utility.SetStage(currentNode.era, castles, planes);

        explainText.text = "";
        _3dStartButton.SetActive(true);
        indicater.gameObject.SetActive(false);
        nodeIndicater.SetActive(false);
        nodeSelectEffect.SetActive(false);
        //初期コンテンツを開始
        stateProcessor.SetState(ST_Start);
    }
    #endregion

    void Start()
    {
        Init();
    }

    void Update()
    {
        stateProcessor.Update();
    }

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
            Utility.Alignment(decideEffect, decideEffectTargetNode);
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
            if (Physics.Raycast(ray, out hitInfo, distance))
            {
                GameObject hitObj = hitInfo.collider.gameObject;

                if (hitObj.tag == "FreeChoiceNode" && freeChoiceTargetNode.name != hitObj.name)
                {
                    freeChoiceTargetNode = hitObj;
                    Utility.ChangeObjColor(nodeSelectEffect, Color.white);
                    audio_SE.PlayOneShot(preselect);
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
        yield return PanelLiftDown(node, 0.15f);
        if (!isFreeChoice) UpdateNode();
    }
    #endregion

    #region 1615年
    public void Event_1615() { StartCoroutine(Process_1615(currentNode)); }
    IEnumerator Process_1615(Node node)
    {
        Utility.SetStage(node.era, castles, planes);
        UpdateAudioAndUI(node);
        //着火
        node.assets[0].SetActive(true);
        yield return PanelLiftUp(node, 0.15f);
        //鎮火
        node.assets[0].SetActive(false);
        //城の非アクティブ化
        castles[1].SetActive(false);
        castles[2].SetActive(false);
        if (!isFreeChoice) UpdateNode();
    }
    #endregion

    #region 1626年
    public void Event_1626() { StartCoroutine(Process_1626(currentNode)); }
    IEnumerator Process_1626(Node node)
    {
        Utility.SetStage(node.era, castles, planes);
        UpdateAudioAndUI(node);
        yield return PanelLiftUp(node, 0.15f);
        if (!isFreeChoice) UpdateNode();
    }
    #endregion

    #region 1665年
    public void Event_1665() { StartCoroutine(Process_1665(currentNode)); }
    IEnumerator Process_1665(Node node)
    {
        Utility.SetStage(node.era, castles, planes);
        UpdateAudioAndUI(node);
        node.assets[0].GetComponent<OrbitalBeamLaser>().Start_Thunder();
        yield return new WaitForSeconds(3f);
        node.assets[0].GetComponent<OrbitalBeamLaser>().End_Thunder();
        castles[3].SetActive(false);
        if (!isFreeChoice) UpdateNode();
    }
    #endregion

    #region 1931年
    public void Event_1931() { StartCoroutine(Process_1931(currentNode)); }
    IEnumerator Process_1931(Node node)
    {
        Utility.SetStage(node.era, castles, planes);
        UpdateAudioAndUI(node);
        yield return PanelLiftUp(node, 0.15f);
        if (!isFreeChoice) UpdateNode();
    }
    #endregion

    #region サブルーチン
    IEnumerator PanelLiftUp(Node node, float speed)
    {
        while (node.panel.position.y < node.destination.position.y)
        {
            node.panel.position += new Vector3(0, Time.deltaTime * speed, 0);
            //node.panel.position = Vector3.SlerpUnclamped(node.panel.position, node.destination.position, speed);
            yield return null;
        }
    }

    IEnumerator PanelLiftDown(Node node, float speed)
    {
        while (node.panel.position.y > node.destination.position.y)
        {
            node.panel.position -= new Vector3(0, Time.deltaTime * speed, 0);
            //node.panel.position = Vector3.SlerpUnclamped(node.panel.position, node.destination.position, speed);
            yield return null;
        }
    }

    IEnumerator ActiveNextNode(Node preNode, Node node, Transform panel)
    {
        //ノードインディケーターを縮小化
        yield return Shrink();
        //ノードの色を青くする
        if (preNode.era != node.era)
            preNode.transform.gameObject.GetComponent<Renderer>().material = nodeActiveMaterial;
        //移動
        while (Vector3.Distance(panel.position, node.transform.position) > 0.007f)
        {
            //panel.position = Vector3.SlerpUnclamped(panel.position, node.transform.position, Time.deltaTime * panelSpeed);
            Vector3 vec = (node.transform.position - panel.position).normalized;
            var pos = panel.position;
            pos += vec * Time.deltaTime * panelSpeed;
            panel.position = pos;
            yield return null;
        }
        Utility.Alignment(panel, node.transform);
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
        Text eraText = eraTexts[node.myIndex];
        eraText.color = new Color(255f, 255f, 255f, 255f);
        float theta = 90f / 11f;
        while (nodeIndicater.transform.localScale.x < largeScale_Max)
        {
            nodeIndicater.transform.localScale *= largeScale;
            eraText.transform.Rotate(-theta, 0, 0);
            yield return null;
        }
        nodeIndicater.transform.localScale = Vector3.one * largeScale_Max;
    }

    IEnumerator DecideEffectGo(Node node)
    {
        decideEffectTargetNode = node.transform.gameObject;
        Utility.Alignment(decideEffect.transform, node.transform);
        decideEffect.SetActive(true);
        while (decideEffect.transform.localScale.x < 3000f)
        {
            decideEffect.transform.localScale *= 1.1f;
            yield return null;
        }
        decideEffect.SetActive(false);
        decideEffect.transform.localScale = Vector3.one * 30f;

    }
    IEnumerator InvisibleUIButton(GameObject button, Color color)
    {
        yield return new WaitForSeconds(1f);
        Utility.ChangeObjColor(button, color);
        button.SetActive(false);
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
        if (Physics.Raycast(ray, out hitInfo, distance))
        {
            string tag = hitInfo.collider.tag;

            if (tag == "NextWaitNode")
            {
                indicater.gameObject.SetActive(true);
                //決定のSEを流す
                audio_SE.PlayOneShot(select);
                //各ノードに割り振られた処理を開始
                currentNode.process.Invoke();
                //タグを変えることによる選択できないようにする
                currentNode.transform.tag = "ActiveNode";
                StartCoroutine(DecideEffectGo(currentNode));
            }
            else if (tag == "FreeChoiceNode")
            {
                //決定のSEを流す
                audio_SE.PlayOneShot(select);
                //セレクトエフェクトのカラーを決定色(Green)にする
                Utility.ChangeObjColor(nodeSelectEffect, Color.green);
                //選択したオブジェクトからノードを検索してcurrentNodeに設定
                currentNode = nodes[nodeDictionary[hitInfo.collider.transform.name]];
                currentNode.process.Invoke();
            }
            else if (tag == "Init")
            {
                //決定のSEを流す
                audio_SE.PlayOneShot(select);
                Utility.ChangeObjColor(_3dRebootButton, Color.green);
                StartCoroutine(InvisibleUIButton(_3dRebootButton, Color.white));
                audio_Voice.PlayOneShot(initVoice);
                Init();
            }
            else if (tag == "Start")
            {
                //決定のSEを流す
                audio_SE.PlayOneShot(select);
                Utility.ChangeObjColor(_3dStartButton, Color.green);
                StartCoroutine(InvisibleUIButton(_3dStartButton, Color.white));
                nodeIndicater.SetActive(true);
                //ノードインディケーターを初期位置まで移動
                StartCoroutine(ActiveNextNode(nodes[0], nodes[0], plane_UI));
                nodes[0].transform.tag = "NextWaitNode";
                stateProcessor.SetState(ST_Play);
            }
        }
    }
    public void ML_OnBumperButton()
    {
        dissolveBuilding.transform.Translate(0, -0.01f, 0);
    }
    #endregion

    #region ノードのアップデート関連
    /// <summary>
    /// 連結されているノードに更新する
    /// 最終ノードの場合はデストラクト処理
    /// </summary>
    public void UpdateNode()
    {
        int nextIndex = currentNode.myIndex + 1;
        if (nextIndex < nodes.Length)
        {
            Node preNode = currentNode;
            currentNode = nodes[nextIndex];
            currentNode.transform.tag = "NextWaitNode";
            //ノードインディケーターの移動
            StartCoroutine(ActiveNextNode(preNode, currentNode, plane_UI));
        }
        //最後のノードのイベントが終わったときはフリー選択モードとなる
        else
        {
            //パネルずらし
            plane_UI.localPosition += new Vector3(1f, 0, 0);
            currentNode.transform.gameObject.GetComponent<Renderer>().material = nodeActiveMaterial;
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
    /// <summary>
    /// ストーリーモードからフリーチョイスモードへ
    /// </summary>
    public void FreeChoiceAvtivate()
    {
        freeChoiceTargetNode = currentNode.transform.gameObject;
        //セレクトエフェクトのアクティブ化
        nodeSelectEffect.SetActive(true);
        //インディケーターは非アクティブ化
        nodeIndicater.SetActive(false);
        //イニシャライズボタンのアクティブ化
        _3dRebootButton.SetActive(true);
        //アップデートモードの変更
        stateProcessor.SetState(ST_FreeChoice);
        //タグを変える
        for (int i = 0; i < nodes.Length; i++)
            nodes[i].transform.tag = "FreeChoiceNode";
    }
    #endregion
}
