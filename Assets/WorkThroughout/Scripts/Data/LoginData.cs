using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LoginData
{
    public int loginId;       // �α��� ID
    public int playerId;      // �÷��̾� ID
    public string loginTime;  // �α��� �ð� (JSON ��ȯ�� ���� string)
    public string ipAddress;  // �α��� �� ����� IP �ּ�

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
