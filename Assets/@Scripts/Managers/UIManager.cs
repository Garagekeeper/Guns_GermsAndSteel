using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{

    private PlayingUI _playingUI = null;

    public PlayingUI PlayingUI
    {
        set => _playingUI = value;
        get => _playingUI;
    }
}
