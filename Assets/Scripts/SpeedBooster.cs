using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum BoostCycle
{
    Idle = 0,
    Held = 1,
    Success = 2,
    HighGearFail = 3,
    NotRunning = 4,
    Overboost = 5
}

[System.Serializable]
public class BoostGear
{
    public string Name;

    public Vector2 BoostPressWindow = new Vector2(0.2f, 0.4f);
    public Vector2 BoostReleaseWindow = new Vector2(0.8f, 1.0f);

    public float MaxSpeed = 30f;

    //in seconds
    public float BoostChangeDuration = 1.0f;
    public Ease BoostEase = Ease.Linear;

    public bool IsInHeldWindow(float Value)
    {
        return Value >= BoostPressWindow.x && Value <= BoostPressWindow.y;
    }

    public bool IsInReleaseWindow(float Value)
    {
        return Value >= BoostReleaseWindow.x && Value <= BoostReleaseWindow.y;
    }

    public bool IsHeldTooLong(float Value)
    {
        return Value >= BoostReleaseWindow.y;
    }

    public bool IsReleasedTooLate(float Value)
    {
        return Value >= BoostPressWindow.y && Value <= BoostReleaseWindow.x;
    }
}

public class SpeedBooster : MonoBehaviour
{
    public Slider RowSlider;

    private PlayerMovement PlayerMovement;

    public float MaxSpeed = 30f;

    [Header("Boost Values")]
    public Color BoostFailed = Color.red;
    public Color BoostSuccess = Color.blue;
    public Color BoostInProgress = Color.yellow;
    public Color BoostOff = Color.grey;
    public Color BoostOverboost = Color.magenta;

    public BoostGear[] BoostGears;

    //moves from 0-1
    //must be released from 
    private float BoostMeterValue = 0;
    private BoostCycle BoostCycle = BoostCycle.Idle;
    Tweener BoostMeterTweener;

    private int BoostGearIndex;


    // Start is called before the first frame update
    void Start()
    {
        PlayerMovement = GetComponent<PlayerMovement>();

        BoostCycle = BoostCycle.NotRunning;

        BoostGearIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBoostCycle(Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space), Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space));

        float NewSpeed = EvaluateBoostCycleSpeed();

        PlayerMovement.SetMaxSpeed(NewSpeed);

        if (RowSlider != null)
        {
            RowSlider.value = BoostMeterValue;
        }
    }

    private void StartCycle()
    {
        BoostMeterTweener = DOTween.To(() => BoostMeterValue, x => BoostMeterValue = x, 1.0f, GetCurrentGear().BoostChangeDuration).SetLoops(-1);
        BoostMeterTweener.OnStepComplete(OnBoostMeterComplete);
        BoostMeterTweener.SetEase(GetCurrentGear().BoostEase);

        BoostCycle = BoostCycle.Idle;

    }

    private void StopCycle()
    {
        BoostMeterTweener.Kill();
        BoostMeterTweener = null;

        BoostCycle = BoostCycle.NotRunning;
    }

    private void UpdateBoostCycle(bool isPressingMouseButton, bool isReleasingMouseButton)
    {
        if (BoostCycle == BoostCycle.Overboost)
        {
            if (isPressingMouseButton)
            {
                //lose gear if you press boost again
                FailGearChange();
            }
        }

        BoostGear CurrentGear = GetCurrentGear();

        //if boost cycle isn't running then start it when button is pressed
        if (BoostCycle == BoostCycle.NotRunning)
        {
            if (isPressingMouseButton)
            {
                //start boost at minimum press window
                StartCycle();
                //Debug.Log("Starting");
                BoostMeterValue = BoostGears[0].BoostPressWindow.x;
            }
        }

        //button is NOT held and player is starting hold
        if (BoostCycle != BoostCycle.Held && isPressingMouseButton)
        {
            //player pressing hold at proper time
            if (CurrentGear.IsInHeldWindow(BoostMeterValue))
            {
                //boost started
                //Debug.Log("Boost Started");
                BoostCycle = BoostCycle.Held;
            }

            //player pressing button at wrong time
            else
            {
                //boost set back to idle
                //Debug.Log("Boost Start Failed");
                FailGearChange();
            }
        }

        //button IS held and player is releasing
        else if (BoostCycle == BoostCycle.Held && isReleasingMouseButton)
        {
            //player releasing hold at proper time
            if (CurrentGear.IsInReleaseWindow(BoostMeterValue))
            {
                //Release successful, keep speed
                SucceedGearChange();
            }

            //player releasing hold at wrong time
            else
            {
                FailGearChange();
            }
        }

        //check if held is held too long
        else if (BoostCycle == BoostCycle.Held)
        {
            if (CurrentGear.IsHeldTooLong(BoostMeterValue))
            {
                //boost held too long
                FailGearChange();
            }
        }

        //check when success will expire
        else if (BoostCycle == BoostCycle.Success)
        {
            if (CurrentGear.IsReleasedTooLate(BoostMeterValue))
            {
                //boost held too long
                FailGearChange();
            }
        }

        //If we are in higher gear, we are allowed to loose them while idling
        else if (BoostCycle == BoostCycle.Idle && BoostGearIndex > 0)
        {
            if (CurrentGear.IsReleasedTooLate(BoostMeterValue))
            {
                //boost held too long
                FailGearChange();
            }
        }
    }

    private float EvaluateBoostCycleSpeed()
    {
        //boost cycle was done well, we can chill
        if (BoostCycle == BoostCycle.Held)
        {
            if (RowSlider != null)
            {
                Transform Handle = RowSlider.transform.Find("Handle Slide Area").Find("Handle");
                Handle.GetComponent<Image>().color = BoostInProgress;
            }

            //give a taste of the higher gear speed
            return GetHigherGearSpeed();
        }

        else if (BoostCycle == BoostCycle.Success)
        {
            if (RowSlider != null)
            {
                Transform Handle = RowSlider.transform.Find("Handle Slide Area").Find("Handle");
                Handle.GetComponent<Image>().color = BoostSuccess;
            }
            return GetCurrentGearSpeed();
        }
        else if (BoostCycle == BoostCycle.NotRunning)
        {
            if (RowSlider != null)
            {
                Transform Handle = RowSlider.transform.Find("Handle Slide Area").Find("Handle");
                Handle.GetComponent<Image>().color = BoostOff;
            }
            return GetCurrentGearSpeed();
        }

        else if (BoostCycle == BoostCycle.Overboost)
        {
            if (RowSlider != null)
            {
                Transform Handle = RowSlider.transform.Find("Handle Slide Area").Find("Handle");
                Handle.GetComponent<Image>().color = BoostOverboost;
            }

            return GetCurrentGearSpeed();
        }

        //player borked it, back to basics
        if (RowSlider != null)
        {
            Transform Handle = RowSlider.transform.Find("Handle Slide Area").Find("Handle");
            Handle.GetComponent<Image>().color = BoostFailed;
        }

        return GetCurrentGearSpeed();
    }

    private void OnBoostMeterComplete()
    {
        BoostMeterValue = 0;
        BoostMeterTweener.ChangeStartValue(0.0f, GetCurrentGear().BoostChangeDuration);
        BoostMeterTweener.SetEase(GetCurrentGear().BoostEase);

        if (BoostCycle == BoostCycle.HighGearFail)
        {
            BoostCycle = BoostCycle.Idle;
        }
    }

    private BoostGear GetCurrentGear()
    {
        return BoostGears[BoostGearIndex];
    }

    private float GetCurrentGearSpeed()
    {
        return GetCurrentGear().MaxSpeed;
    }

    private float GetHigherGearSpeed()
    {
        int index = Mathf.Min(BoostGearIndex + 1, BoostGears.Length - 1);

        return BoostGears[index].MaxSpeed;
    }

    //move up a gear
    private void SucceedGearChange()
    {
        BoostGearIndex = Mathf.Min(BoostGearIndex + 1, BoostGears.Length - 1);

        if (BoostGearIndex == BoostGears.Length - 1)
        {
            BoostCycle = BoostCycle.Overboost;
        }
        else
        {
            BoostCycle = BoostCycle.Success;
        }
    }

    private void FailGearChange()
    {
        BoostGearIndex = Mathf.Max(BoostGearIndex - 1, 0);

        if (BoostGearIndex == 0)
        {
            BoostCycle = BoostCycle.Idle;
            StopCycle();
        }
        else
        {
            BoostCycle = BoostCycle.HighGearFail;
        }
    }
}
