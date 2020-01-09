using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BeatDetection : MonoBehaviour
{
    public int ringBufferSize = 120;
    public int bufferSize;
    public int samplingRate = 44100;
    public bool limitBeats;
    public int limitedAmount;
    public float beatIndicationThreshold;

    public OnBeatEvent onBeat;

    private const int bands = 12;
    private const int maximumLag = 100;
    private const float smoothDecay = 0.997f;

    private AudioSource audioSource;
    private AudioData audioData;

    private int framesSinceBeat;
    private float framePeriod;

    private int currentRingBufferPosition;

    private float[] spectrum;
    private float[] previousSpectrum;

    private float[] averagePowerPerBand;
    private float[] onsets;
    private float[] notations;
    

    private void Awake()
    {
        onsets = new float[ringBufferSize];
        notations = new float[ringBufferSize];
        spectrum = new float[bufferSize];
        averagePowerPerBand = new float[bands];

        audioSource = GetComponent<AudioSource>();
        samplingRate = audioSource.clip.frequency;

        framePeriod = (float)bufferSize / samplingRate;

        previousSpectrum = new float[bands];
        for(int i = 0; i < bands; i++)
        {
            previousSpectrum[i] = 100f;
        }

        audioData = new AudioData(maximumLag, smoothDecay, framePeriod, BandWidth() * 2);
    }

    private float BandWidth()
    {
        return (2f / bufferSize) * (samplingRate / 2f) * .5f;
    }

    private void Update()
    {
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
        for(int i = 0; i<bands; i++)
        {
            float averagePower = 0;
            int lowFrequencyIndex = (i == 0) ? 0 : Mathf.RoundToInt((samplingRate * .5f) / Mathf.Pow(2, bands - i));
            int highFrequencyIndex = Mathf.RoundToInt((samplingRate * .5f) / Mathf.Pow(2, bands - 1 - i));
            int lowBound = FrequencyByIndex(lowFrequencyIndex);
            int highBound = FrequencyByIndex(highFrequencyIndex);
            for(int j = lowBound; j <= highBound; j++)
            {
                averagePower += spectrum[j];
            }
            averagePower /= (highBound - lowBound + 1);
            averagePowerPerBand[i] = averagePower;
        }

        float onset = 0;
        for(int i = 0; i <bands; i++)
        {
            float spectrumValue = Mathf.Max(-100f,20f*Mathf.Log10(averagePowerPerBand[i]+160f));
            spectrumValue *= 0.025f;

            float dbIncrement = spectrumValue - previousSpectrum[i];
            previousSpectrum[i] = spectrumValue;

            onset += dbIncrement;
        }

        onsets[currentRingBufferPosition] = onset;
        audioData.UpdateAudioData(onset);

        float maxDelay = 0f;
        int tempo = 0;
        for (int i = 0; i < maximumLag; i++)
        {
            float delayVal = Mathf.Sqrt(audioData.DelayAtIndex(i));
            if(delayVal > maxDelay)
            {
                maxDelay = delayVal;
                tempo = i;
            }
        }

        float maximumNotation = -9999;
        int maximumNotationIndex = 0;

        for(int i = Mathf.RoundToInt(tempo*.5f); i<Mathf.Min(ringBufferSize,2*tempo); i++)
        {
            float notationValue = onset + notations[(currentRingBufferPosition - i + ringBufferSize) % ringBufferSize] - (beatIndicationThreshold * 100f) * Mathf.Pow(Mathf.Log(i / tempo),2);
            if (notationValue > maximumNotation)
            {
                maximumNotation = notationValue;
                maximumNotationIndex = i;
            }
        }

        notations[currentRingBufferPosition] = maximumNotation;
        float minimumNotation = notations[0];
        for(int i = 0; i < ringBufferSize; i++)
        {
            if (notations[i] < minimumNotation)
            {
                minimumNotation = notations[i];
            }
        }

        for(int i = 0; i<ringBufferSize; i++)
        {
            notations[i] -= minimumNotation;
        }

        framesSinceBeat++;

        if(maximumNotationIndex == currentRingBufferPosition)
        {
            if (limitBeats)
            {
                if (framesSinceBeat > tempo / limitedAmount)
                {
                    onBeat.Invoke();
                    framesSinceBeat = 0;
                }
            }
            else {
                onBeat.Invoke();
            }
        }
        currentRingBufferPosition++;
        if(currentRingBufferPosition>ringBufferSize){
            currentRingBufferPosition = 0;
        }
    }

    public int FrequencyByIndex(int frequencyIndex)
    {
        if (frequencyIndex < BandWidth())
        {
            return 0;
        }
        if(frequencyIndex > samplingRate*.5f - BandWidth())
        {
            return bufferSize / 2;
        }

        float fraction = frequencyIndex / samplingRate;
        return Mathf.RoundToInt(bufferSize * fraction);
    }

}

public class AudioData
{
    private int index;
    private int delayLength;
    private float decay;

    private float[] delays;
    private float[] outputValues;
    private float[] weights;
    private float[] bpms;

    private float octaveWidth;
    private float framePeriod;

    public AudioData(int delayLength, float decay, float framePeriod, float octaveWidth)
    {
        this.octaveWidth = octaveWidth;
        this.decay = decay;
        this.delayLength = delayLength;

        delays = new float[delayLength];
        outputValues = new float[delayLength];

        index = 0;

        bpms = new float[delayLength];
        weights = new float[delayLength];

        AddWeigths();
    }

    private void AddWeigths()
    {
        for(int i=0; i<delayLength; i++)
        {
            bpms[i] = 60f / (framePeriod * i);
            weights[i] = Mathf.Exp(-.5f*Mathf.Pow(Mathf.Log(bpms[i]/120f)/Mathf.Log(2f/octaveWidth),2f));
        }
    }

    public void UpdateAudioData(float updateOnset)
    {
        delays[index] = updateOnset;
        for(int i=0; i < delayLength; i++)
        {
            int delayIndex = (index - i + delayLength) % delayLength;
            outputValues[i] += (1f - decay) * (delays[index] * delays[delayIndex] - outputValues[i]);
        }

        index++;
        if(index >= delayLength)
        {
            index = 0;
        }
    }

    public float DelayAtIndex(int delayIndex)
    {
        return weights[delayIndex] * outputValues[delayIndex];
    }

}

[System.Serializable]
public class OnBeatEvent : UnityEvent
{

}

