using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine.UI;

public class SpawnObjectAddressable : MonoBehaviour
{

    public Slider downloadSlider;
    public AssetReference assetReference;
    //public AssetLabelReference assetLabelReference;

    // Start is called before the first frame update
    void Start()
    {
        downloadSlider.value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {

            // Addressables.LoadAssetAsync<GameObject>(assetLabelReference).Completed +=(handle)=>{
            //     if(handle.Status == AsyncOperationStatus.Succeeded)
            //     {
            //         Instantiate<GameObject>(handle.Result); 
            //         Debug.Log("Success");
            //     }
            //     else
            //     {
            //         Debug.Log("oops " + handle.Result +  "   " + handle.OperationException.Message);
            //     }
            // };

            LoadPrefab();

            // AsyncOperationHandle<GameObject> asyncHandle = Addressables.LoadAssetAsync<GameObject>(assetReference);
            // asyncHandle.Completed += OnComplete;
        }
    }

    private async void LoadPrefab()
    {
        var size = await Addressables.GetDownloadSizeAsync(assetReference);
        var handle = assetReference.LoadAssetAsync<GameObject>();
        while(handle.Status != AsyncOperationStatus.Succeeded)
        {
            var perc = handle.GetDownloadStatus().DownloadedBytes/(float)size;
            downloadSlider.value = handle.GetDownloadStatus().Percent * 100f;
            await Task.Yield();
        }
        Instantiate<GameObject>(handle.Result); 
    }

    private void OnComplete(AsyncOperationHandle<GameObject> handle)
    {
        if(handle.Status == AsyncOperationStatus.Succeeded)
        {
            Instantiate<GameObject>(handle.Result);
        }
        else
        {
            Debug.Log("Failed to load");
        }

    }
}
