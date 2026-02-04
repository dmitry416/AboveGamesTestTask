using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkbox : MonoBehaviour
{
    [SerializeField] private Color _baseColor;
    [SerializeField] private Color _checkedColor;
    [SerializeField] private Color _subBaseColor;
    [SerializeField] private Color _subCheckedColor;
    [SerializeField] private Sprite _baseIcon;
    [SerializeField] private Sprite _checkedIcon;
    [Space]
    [SerializeField] private Checker[] _checkers;

    public void Check(int checkerID)
    {
        for (int i = 0; i < _checkers.Length; ++i)
        {
            if (checkerID == i)
                _checkers[i].SetChecker(_checkedIcon, _checkedColor, _subCheckedColor);
            else
                _checkers[i].SetChecker(_baseIcon, _baseColor, _subBaseColor);
        }
    }
}
