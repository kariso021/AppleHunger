using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LoginData
{
    public int loginId;       // 로그인 ID
    public int playerId;      // 플레이어 ID
    public string loginTime;  // 로그인 시간 (JSON 변환을 위해 string)
    public string ipAddress;  // 로그인 시 사용한 IP 주소

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
        return JsonUtility.FromJson<LoginList>(jsonData);
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}
