﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection.Emit;
using DragonBones;
using UnityEngine;
using UnityEngine.UI;

public class AdventureHandler : MonoBehaviour
{
    public static AdventureHandler _instance;

    //书写冷却时间
    public double WriteFireTime;
    private double writeFireTime;

    //敌人攻击冷却时间
    public double BossFireTime;

    private double bossFireTime;

    //Boss攻击倒计时显示
    public Text timeText;


    private bool isStartWrite = false;
    private bool isUp = false;

    public UnityArmatureComponent attackRole;

    //是否是新的识别
    private bool isNewRec = false;

    //是否是新的动画
    private bool isNewAnim = false;
    private string newAnimName = "";

    //Boss攻击是否计时
    [HideInInspector] public bool isCalcTime = false;

    //演示显示游戏结束面板的时间
    public float delayShowGameOverTime = 2f;

    //当动画冲突时两个动画的播放间隔时间
    public float delayAnimPlayTime = 1f;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        bossFireTime = BossFireTime;

        writeFireTime = WriteFireTime;


        //设置动画监听
        attackRole.AddDBEventListener(EventObject.COMPLETE, OnAnimationEventHandler);
    }


    private void Update()
    {
        if (isStartWrite && isUp)
        {
            writeFireTime -= Time.deltaTime;
            if (writeFireTime <= 0)
            {
                writeFireTime = WriteFireTime;
                isStartWrite = false;
                isUp = false;
                HWRRec();
            }
        }
        else
        {
            writeFireTime = WriteFireTime;
        }

        if (isCalcTime)
        {
            //Boss攻击
            bossFireTime -= Time.deltaTime;
            if (bossFireTime <= 0)
            {
                isCalcTime = false;
                bossFireTime = BossFireTime;
                FadeInRoleAnim(attackRole, "behurt");
            }
        }

        timeText.text = bossFireTime.ToString("Boss : 0.00s");
    }


    public void SetTimerStart(string state)
    {
        if ("Down".Equals(state))
        {
            //当按下的时候表示开始写字
            isStartWrite = true;
        }
        else if ("Move".Equals(state))
        {
            isUp = false;
            writeFireTime = WriteFireTime;
        }
        else if ("Up".Equals(state))
        {
            isUp = true;
        }
    }

    //获取Android手写识别后的结果
    public void CallHWRRec()
    {
        AndroidUtil.Call("hwrRec");
    }

    //手写识别的功能
    public void HWRRec()
    {
        //TODO 限制输入的个数以及实现修改功能
        CallHWRRec();
    }

    //手写模块识别结果回调，在Anroid那边调用
    public void OnGetRecResult(string results)
    {
        AndroidUtil.Log("识别到的结果:" + results);
        if (results != null && results.Length >= 1)
        {
            var resultArr = results.Split(';');
            if (resultArr.Length > 0) WordHandler._instance.SetCharacter(resultArr[0]);
        }
    }

    //用于Unity下调试手写识别结果反馈
    public void TestHWRRec(string results)
    {
        AndroidUtil.Log("识别到的结果:" + results);
        if (results != null && results.Length >= 1) WordHandler._instance.SetCharacter(results.Substring(0, 1));
    }

    public void JudgeResult(string btnName)
    {
        if (!WordHandler._instance.JudgeResult())
        {
            //TODO 错误效果
            FadeInRoleAnim(attackRole, "fail");
//            attackRole.animation.Play("normal");
        }
        else
        {
            //TODO 攻击效果
            if ("AttachBtn".Equals(btnName))
            {
                FadeInRoleAnim(attackRole, "attack");
//              attackRole.animation.Play("normal");
                AndroidUtil.Toast("攻击效果!!!");
            }
            else if ("CureBtn".Equals(btnName))
            {
                //TODO 需要修改成对应对象的动画
                FadeInRoleAnim(attackRole, "attack");

                AndroidUtil.Toast("治疗效果!!!");
            }
            else if ("DefensenBtn".Equals(btnName))
            {
                //TODO 需要修改成对应对象的动画
                attackRole.animation.FadeIn("attack", 0.2f, 1);

                AndroidUtil.Toast("防御效果!!!");
            }
        }

        isNewRec = true;
        isCalcTime = false;
    }

    //更新为新的单词
    private void SetNewWord()
    {
        if (WordHandler._instance.JudgeGameOver())
            StartCoroutine(DelayShowGameOverPanel(delayShowGameOverTime));
        else
            WordHandler._instance.UpdateWordInfo();

        isNewRec = false;
    }

    private IEnumerator DelayShowGameOverPanel(float second)
    {
        yield return new WaitForSeconds(second);
        GameSetting._instance.SetGameOver(true);
    }


    //动画完成后切换回默认动画
    private void OnAnimationEventHandler(string type, EventObject eventObject)
    {
        if (isNewAnim)
        {
            isNewAnim = false;
            if (!"".Equals(newAnimName))
            {
                StartCoroutine(DelayAnimPlay(attackRole, newAnimName, delayAnimPlayTime));
                newAnimName = "";
            }
        }
        else
        {
            PlayRoleAnim(attackRole, "normal");
            if (isNewRec)
            {
                isNewRec = false;
                SetNewWord();
            }

            isCalcTime = true;
        }
    }

    public void FadeInRoleAnim(UnityArmatureComponent role, string animName)
    {
        if (role.animation.lastAnimationName != "normal")
        {
            isNewAnim = true;
            newAnimName = animName;
        }
        else
        {
            role.animation.FadeIn(animName, 0.2f, 1);
        }
    }

    public void PlayRoleAnim(UnityArmatureComponent role, string animName)
    {
        role.animation.Play(animName, 1);
    }

    private IEnumerator DelayAnimPlay(UnityArmatureComponent role, string animName, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        PlayRoleAnim(role, animName);
    }
}