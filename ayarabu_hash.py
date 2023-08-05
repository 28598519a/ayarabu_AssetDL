import os
import urllib.request
import hashlib
import UnityPy #requirement 'pip install unitypy==1.6.7.2'

filetype = ['DmmR18Web', 'DmmAndroid']
ayakashi_cdn_header = f'https://game.ayarabu.com/AssetBundles/{filetype[0]}/'

HASH_SALT = 'aesydreoi8714dfgqw3nyanpksdgnu'
HASH_COUNT = 14
XOR_VAL = b'\x2C'

def makeSha256Hash(text, salt = HASH_SALT, loopHashCount = HASH_COUNT):
    text_salt = f'{text}{salt}'
    b_text_salt = text_salt.encode('utf-8')
    for i in range(loopHashCount):
        if i >= 1:
            b_text_salt = baxor(b_text_salt, XOR_VAL * len(b_text_salt))
        b_text_salt = hashlib.sha256(b_text_salt).digest()
    hashRes = res = ''.join(format(x, '02x') for x in b_text_salt)
    return hashRes

def baxor(ba1, ba2):
    return bytes(a ^ b for a,b in zip(ba1, ba2))

def dump_filelist(manifest, output):
    env = UnityPy.load(manifest)
    for o in env.objects:
        data = o.read()
        if data.name == 'AssetBundleManifest':
            parsed_list = []
            for key in data.type_tree['AssetBundleNames']:
                parsed_list.append(data.type_tree['AssetBundleNames'][key])
            parsed_list.sort()
            with open(output, 'w', encoding='utf-8-sig') as f:
                for p in parsed_list:
                    dirname, basename = os.path.split(p)
                    filename, fileext = os.path.splitext(basename)
                    hashname = makeSha256Hash(filename)
                    f.write(f'{p},{ayakashi_cdn_header}{dirname}/{hashname}{fileext}\n')

def main():
    response = urllib.request.urlopen(f'{ayakashi_cdn_header}{filetype[0]}').read()
    dump_filelist(response, 'manifest.csv')

if __name__ == '__main__':
    main()
