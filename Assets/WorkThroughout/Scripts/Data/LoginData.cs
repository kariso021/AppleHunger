using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[Table("loginRecords")]
public class LoginData
{
    [PrimaryKey]
    public int loginId { get; set; }
    public int playerId { get; set; }
    public string loginTime { get; set; }
    public string ipAddress { get; set; }


    public LoginData() { }
    public LoginData(int loginId,int playerId, string loginTime,string ipAddress)
    {
        this.loginId = loginId;
        this.playerId = playerId;
        this.loginTime = loginTime;
        this.ipAddress = ipAddress;
    }
}

[Serializable]
public class LoginList
{
    public List<LoginData> loginRecords;

    public static LoginList FromJson(string jsonData)
    {
        return JsonConvert.DeserializeObject<LoginList>(jsonData);
    }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }
}
