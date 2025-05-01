using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace appleHunger
{
    public static class AppleHungerTools
    {
        public static T FindByName<T>(T[] list, string targetName) where T : Component
        {
            foreach (var item in list)
            {
                if (item.gameObject.name == targetName && item.gameObject.scene.IsValid())
                {
                    return item;
                }
            }
            return null;
        }

    }
    // ClientNetworkManager
    [System.Serializable]
    public class LoginResponse
    {
        public bool success;
        public LoginData records;
    }

    [System.Serializable]
    public class PlayerStatsResponse
    {
        public bool success;
        public PlayerStatsData playerStats;
    }

    [System.Serializable]
    public class RankingDataResponse
    {
        public bool success;
        public PlayerRankingData[] topRankings;
    }

    [System.Serializable]
    public class PurchaseResponse
    {
        public bool success;
        public string message;
        public int remainingCurrency;
        public string error;
    }

    // ServerToAPI
    [System.Serializable]
    public class LoginRecordData
    {
        public int loginId;
        public int playerId;
        public string loginTime;
        public string ipAddress;
    }

    [System.Serializable]
    public class LoginRecordList
    {
        public List<LoginRecordData> records;
    }
    // JSON 파싱을 위한 클래스
    [System.Serializable]
    public class MatchHistoryResponse
    {
        public bool success;
        public MatchHistoryData[] matches;
    }

    [System.Serializable]
    public class PlayerItemsResponse
    {
        public bool success;
        public PlayerItemData[] items;
    }

    [System.Serializable]
    public class LoginUpdateRequest
    {
        public int playerId;
        public string ipAddress;

        public LoginUpdateRequest(int playerId, string ipAddress)
        {
            this.playerId = playerId;
            this.ipAddress = ipAddress;
        }
    }
    [System.Serializable]
    public class RankingShouldUpdateResponse
    {
        public bool shouldUpdate;
    }

    [System.Serializable]
    public class NicknameUpdateRequest
    {
        public int playerId;
        public string playerName;

        public NicknameUpdateRequest(int id, string nickname)
        {
            playerId = id;
            playerName = nickname;
        }
    }
    [System.Serializable]
    public class NicknameDuplicateResponse
    {
        public bool isDuplicate;
    }

    [System.Serializable]
    public class UnityTokenResponse
    {
        public string idToken;
        public string sessionToken;
    }

    [System.Serializable]
    public class GoogleIdUpdateRequest
    {
        public int playerId;
        public string googleId;

        public GoogleIdUpdateRequest(int playerId, string googleId)
        {
            this.playerId = playerId;
            this.googleId = googleId;
        }
    }
    [System.Serializable]
    public class PlayerSessionRequest
    {
        public int playerId;
        public int isInGame; // bool로 보내고 싶다면 1/0으로 변환해서 넣기
    }
    [System.Serializable]
    public class IsInGameResponse
    {
        public int isInGame;
    }
    [System.Serializable]
    public class AuthMappingRequest
    {
        public string deviceId;
        public string googleId;

        public AuthMappingRequest(string deviceId, string googleId)
        {
            this.deviceId = deviceId;
            this.googleId = googleId;
        }
    }
}
