using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ForbiddenWordData
{
    public List<string> forbiddenWords;
}

public static class ForbiddenWordsLoader
{
    private static HashSet<string> forbiddenWordsSet;

    public static HashSet<string> LoadForbiddenWords()
    {
        if (forbiddenWordsSet != null)
            return forbiddenWordsSet;

        TextAsset jsonFile = Resources.Load<TextAsset>("forbidden_words");
        if (jsonFile == null)
        {
            Debug.LogError(" forbidden_words.json 파일을 찾을 수 없습니다!");
            return new HashSet<string>();
        }

        ForbiddenWordData data = JsonConvert.DeserializeObject<ForbiddenWordData>(jsonFile.text);
        forbiddenWordsSet = new HashSet<string>(data.forbiddenWords);
        return forbiddenWordsSet;
    }
}
