using System;
using UnityEngine;

[Serializable]
public class ConfigData
{
    public static string SaveKey => "config";
    [SerializeField]
    private int bgmVolume = 100;
    [SerializeField]
    private int seVolume = 100;
    [SerializeField]
    private int voiceVolume = 100;

    public int BGMVolume => bgmVolume;
    public int SEVolume => seVolume;
    public int VoiceVolume => voiceVolume;

    public ConfigData() { }
    public ConfigData(int bgmVolume, int seVolume, int voiceVolume)
    {
        this.bgmVolume = bgmVolume;
        this.seVolume = seVolume;
        this.voiceVolume = voiceVolume;
    }
}
