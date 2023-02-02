using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public class SpriteAtlasLoader : MonoBehaviour
{
    public AssetReferenceT<SpriteAtlas> atlasSpritereference;

    private void OnEnable()
    {
        SpriteAtlasManager.atlasRequested += AtlasRequested;
    }
    
    private void OnDisable()
    {
        SpriteAtlasManager.atlasRequested -= AtlasRequested;
    }

    // // Start is called before the first frame update
    // async UniTaskVoid Start()
    // {
    //     await atlasSpritereference.LoadAssetAsync<SpriteAtlas>();
    //     Debug.Log("loaded sprite Atlas");
    //     var atlas = (SpriteAtlas)atlasSpritereference.Asset;
    //     Debug.Log(atlas.spriteCount);
    //     Sprite[] sprites = null;
    //     var count = atlas.GetSprites(sprites);

    // }

    private async void AtlasRequested(string tag, System.Action<SpriteAtlas> atlasAction)
    {
        var handle = atlasSpritereference.LoadAssetAsync<SpriteAtlas>();
        while(!handle.IsDone)
        {
            //Debug.Log(handle.GetDownloadStatus().Percent);
            await Task.Yield();
        }
    
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            atlasAction.Invoke(handle.Result);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
