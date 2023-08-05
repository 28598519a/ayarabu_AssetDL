# ayarabu_AssetDL
Use to download all あやかしランブル！ X指定 (Ayakashi Rumble) resources [DMM ver.]

這遊戲啟動的時候會先抓一個叫做 DmmAndroid / DmmR18Web的檔案，沒加密，但是他資料不是以檔案的形式存在AssetBundle中，而是直接把那些資料套個AssetBundle的殼，所以不能用AssetStudio處理，要用UnityPy直接讀裡面的資訊，拿到檔案表後再以SHA256算出url

## Usage
1. 使用ayarabu_hash.py產生manifest.csv (可省略，直接用我附在Release的檔案就行)
2. 選取manifest.csv開始下載 [總共59.5GB；有夠大...]

## Thanks
[sh0wer1ee](https://gist.github.com/sh0wer1ee)
