using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class WordHandler : MonoBehaviour, HttpUtil.ICallBack
{
    public static WordHandler _instance;

    [HideInInspector] public WordInfo[] infos;

    private int currentWord = 0;
    private int currentCharacter = 0;

    //输入框
    public CharacterGrid[] characterGrids;
    private int gridNums;

    //当前拼音
    public Text currentPinYin;

    //提示信息
    public GameObject tipPanel;
    public Text tipPinYin;
    public Text tipDetail;

    private string[] pinYinArray;
    private string[] characterArray;


    public ButtonStatusManager btnManager;

    private bool isCharacterFull = false;
    private bool isBtnActivite = false;


    private void Awake()
    {
        _instance = this;
    }


    // Use this for initialization
    private void Start()
    {
        gridNums = characterGrids.Length;
        ClearText();
        RequestInfo();
    }

    private void Update()
    {
        if (!isBtnActivite)
            for (var i = 0; i < gridNums; i++)
            {
                if ("".Equals(characterGrids[i].content.text)) break;

                if (i == gridNums - 1) isCharacterFull = true;
            }

        if (isCharacterFull)
        {
            btnManager.SetAllInteractable();
            if (!RoleLifeManager._instance.IsDefenseRoleAlive()) btnManager.SetDefenseBtnDisable();

            isBtnActivite = true;
            isCharacterFull = false;
        }
    }

    private void ClearText()
    {
        for (var i = 0; i < gridNums; i++)
        {
            characterGrids[i].pinyin.text = "";
            characterGrids[i].content.text = "";
            characterGrids[i].toggle.interactable = false;
        }

        currentPinYin.text = "";
        tipPinYin.text = "";
        tipDetail.text = "";
    }

    public void SetBtnStateValue(bool state)
    {
        isBtnActivite = state;
    }

    //请求网络数据
    private void RequestInfo()
    {
        HttpUtil.GetWordInfo(this, this);
    }

    //网络请求失败回调,如果请求失败则反复请求网络，直到成功
    public void OnRequestError(string error)
    {
        AndroidUtil.Toast("网络出错!\n" + error);
        StartCoroutine(LaterRequest(HttpUtil.RetryNetWorkTime));
    }

    private IEnumerator LaterRequest(float second)
    {
        yield return new WaitForSeconds(second);
        RequestInfo();
    }

    //网络请求成功回调
    public void OnRequestSuccess(long responseCode, string response)
    {
        AndroidUtil.Log(response);
        AdventureHandler._instance.isCalcTime = true;
        var infoArray = JsonUtility.FromJson<WordInfos>(GeneralUtils.JsonArrayToObject(response, "infos"));
        infos = infoArray.infos;
        for (var i = 0; i < gridNums; i++) characterGrids[i].toggle.interactable = true;

        //更新输入框中的信息
        UpdateWordInfo(infos[currentWord]);
        //显示手写识别模块
        GameSetting._instance.SetHWRModule(true);
//        AndroidUtil.Toast("该组长度:" + infos.Length);
    }

    public void UpdateWordInfo(WordInfo word)
    {
        currentCharacter = 0;
        pinYinArray = word.Pinyin.Split(' ');
        characterArray = word.Content.Split(' ');
        AndroidUtil.Log(
            "拼音: " + GeneralUtils.printArray(pinYinArray) +
            "\n内容: " + GeneralUtils.printArray(characterArray) +
            "\n详细: " + word.detail);
        for (var i = 0; i < pinYinArray.Length; i++)
        {
            characterGrids[i].pinyin.text = pinYinArray[i];
            characterGrids[i].content.text = "";
        }

        currentPinYin.text = pinYinArray[0];
        characterGrids[0].toggle.isOn = true;

        tipPinYin.text = infos[currentWord].Pinyin;
        tipDetail.text = "释义:" + infos[currentWord].Detail;
    }

    public void UpdateWordInfo()
    {
        currentWord++;
        UpdateWordInfo(infos[currentWord]);
    }


    public void SetCharacter(string character)
    {
        characterGrids[currentCharacter].content.text = character;
        if (currentCharacter < gridNums - 1)
            currentCharacter++;
        else
            currentCharacter = 0;

        characterGrids[currentCharacter].toggle.isOn = true;
        currentPinYin.text = pinYinArray[currentCharacter];
    }

    public bool JudgeResult()
    {
        for (var i = 0; i < gridNums; i++)
            if (!characterArray[i].Equals(characterGrids[i].content.text))
            {
                //TODO 提示信息
                AndroidUtil.Toast("正确书写为:" + infos[currentWord].Content);
                ScoreManager._instance.AddErrorWordList(infos[currentCharacter]);
                return false;
            }

        return true;
    }

    public bool JudgeGameOver()
    {
        SetBtnStateValue(false);
        if (currentWord == infos.Length - 1)
        {
            //TODO 过关保存数据没过关不保存
            if (ScoreManager._instance.IsGameSuccess()) UpdateLevelData();

            ClearText();
            //增加的操作
            GameOverHandler();

            return true;
        }

        return false;
    }

    private void GameOverHandler()
    {
        GameSetting._instance.SetHWRModule(false);
        var wordFrame1 = GameObject.FindGameObjectWithTag("WordFrame1");
        if (wordFrame1) wordFrame1.SetActive(false);

        var wordFrame2 = GameObject.FindGameObjectWithTag("WordFrame2");
        if (wordFrame2) wordFrame2.SetActive(false);
    }

    public void UpdateLevelData()
    {
        var selectLevel = LevelDict.Instance.SelectLevel;
        if (selectLevel != 0)
        {
            var info = LevelDict.Instance.GetLevelInfo(selectLevel);
            //获取旗子数
            info.flag = ScoreManager._instance.GetRewardFlagNum();
            if (info.state == LevelInfo.CURRENT)
            {
                info.state = LevelInfo.OK;
                LevelDict.Instance.UnlockLevel(selectLevel + 1);
            }
        }
    }

    public void ShowDetialContent()
    {
        tipPanel.SetActive(true);
    }

    public void HideDetialContent()
    {
        tipPanel.SetActive(false);
    }

    public void SelecetCharacter1()
    {
        currentCharacter = 0;
        currentPinYin.text = pinYinArray[currentCharacter];
    }

    public void SelecetCharacter2()
    {
        currentCharacter = 1;
        currentPinYin.text = pinYinArray[currentCharacter];
    }
}