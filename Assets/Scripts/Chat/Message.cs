using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Message
{
    private string pseudo;
    private DateTime date;
    private string content;

    public Message(string _pseudo, DateTime _date, string _content)
    {
        this.pseudo = _pseudo;
        this.date = _date;
        this.content = _content;
    }

    public string MessageFormat()
    {
        string hour_min_sec = date.ToString("HH:mm:ss");
        return $"{pseudo} [{hour_min_sec}] : {content}";
    }
}
