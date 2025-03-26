using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{

    private PlayingUI _playingUI = null;
    private GameOverUI _gameOverUI = null;
    private PauseUI _pauseUI = null;

    public PlayingUI PlayingUI
    {
        set => _playingUI = value;
        get => _playingUI;
    }

    public GameOverUI GameOverUI
    {
        set => _gameOverUI = value;
        get => _gameOverUI;
    }

    public PauseUI PauseUI
    {
        set => _pauseUI = value;
        get => _pauseUI;
    }
}
