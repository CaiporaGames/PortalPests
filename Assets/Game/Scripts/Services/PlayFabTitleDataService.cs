using PlayFab;
using PlayFab.ClientModels;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class PlayFabTitleDataService : ITitleDataService 
{
    public async UniTask<string> GetContentIndexUrlAsync() {
        var tcs = new UniTaskCompletionSource<Dictionary<string,string>>();
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(),
            r => tcs.TrySetResult(r.Data ?? new Dictionary<string,string>()),
            e => tcs.TrySetException(new System.Exception(e.ErrorMessage)));

        var data = await tcs.Task;
        // Support either key name to match whatever you set in PlayFab.
        if (data.TryGetValue("contentIndexUrl", out var url)) return url;
        if (data.TryGetValue("contentIndexKey", out url)) return url;
        throw new System.Exception("TitleData missing 'contentIndexUrl' (or 'contentIndexKey').");
    }
}
