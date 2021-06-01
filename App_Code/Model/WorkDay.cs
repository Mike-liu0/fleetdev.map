using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for WorkDay
/// </summary>
[Serializable]
public class WorkDay
{
    public string day;
    public bool isWorking;

    public WorkDay(string _day)
    {
        day = _day;
        isWorking = false;
    }
}