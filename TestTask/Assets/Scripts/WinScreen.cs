using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScreen : MonoBehaviour
{
    [SerializeField]
    private DIContainer _dIContainer;

    [SerializeField]
    private Text _winText;
    [SerializeField]
    private Image _backgroundImage;

    [SerializeField]
    private UnityEvent _onActivate;

    private void Awake()
    {
        _dIContainer.WinScreen = this;
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        _onActivate.Invoke();
        var backgroundImageColor = _backgroundImage.color;
        backgroundImageColor.a = 0f;
        _backgroundImage.color = backgroundImageColor;
        _backgroundImage.DOFade(1f, 2f);
        _winText.transform.DOScale(1.05f, 1.2f).SetEase(Ease.InOutCirc).SetLoops(-1);
    }

    public void Replay() =>
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
