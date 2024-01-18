using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
public class SongFolderText : MonoBehaviour
{
    // Start is called before the first frame update
    
    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath + "/Playlists");
        bool exists = Directory.Exists(path);
        var list = Directory.GetDirectories(path);
        var lengths = Directory.GetFiles(list[0], "Info.dat").Length;
        GetComponent<TextMeshProUGUI>().text = "No song files found, please make sure that the files are located in the '" + path + "' folder." + exists + list.Length + " info length" + lengths;
        Debug.Log("No song files found, please make sure that the files are located in the '" + path + "' folder." + exists + list.Length + " info length" + lengths);
    
    }
    void OnEnable()
    {
        }

    // Update is called once per frame
    void Update()
    {
        
    }
}
