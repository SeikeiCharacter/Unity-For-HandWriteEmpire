﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChineseInfo
{
    private string pinyin;
    private string content;
    private string detail;

    public ChineseInfo(string pinyin, string content, string detail)
    {
        this.pinyin = pinyin;
        this.content = content;
        this.detail = detail;
    }

    public string Pinyin
    {
        get { return pinyin; }
        set { pinyin = value; }
    }

    public string Content
    {
        get { return content; }
        set { content = value; }
    }

    public string Detail
    {
        get { return detail; }
        set { detail = value; }
    }

    public int getLength()
    {
        return content.Length;
    }
}