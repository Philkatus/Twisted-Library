using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceManager : MonoBehaviour
{
    public static VoiceManager Instance;
    [SerializeField] SoundType Idle;
    [SerializeField] SoundType Walking;
    [SerializeField] SoundType Jumping;
    [SerializeField] SoundType HighSpeedSliding;
    [SerializeField] SoundType Achievement;
    [SerializeField] SoundType smallAchievement;
    [SerializeField] SoundType swinging;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else 
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        Idle.cooldown = Idle.minCooldown;
        Walking.cooldown = Walking.minCooldown;
        Jumping.cooldown = Jumping.minCooldown;
        HighSpeedSliding.cooldown = HighSpeedSliding.minCooldown;
        Achievement.cooldown = Achievement.minCooldown;
    }



    public void TryToIdle() 
    {
       
        Idle.timer += Time.deltaTime;
        if (soundTimerandChance(Idle,false)) 
        {
            
            Idle.timer = 0;
            Idle.cooldown = UnityEngine.Random.Range(Idle.minCooldown, Idle.maxCooldown);
            AudioManager.Instance.PlayRandom("idleVoice");
        }
    }
    public void TryToWalkSound()
    {
        
        Walking.timer += Time.deltaTime;
        if (soundTimerandChance(Walking,false))
        {
            
            Walking.timer = 0;
            Walking.cooldown = UnityEngine.Random.Range(Walking.minCooldown, Walking.maxCooldown);
            AudioManager.Instance.PlayRandom("WalkingVoice");
        }
    }
    public void TryToJumpSound()
    {
        
        if (soundTimerandChance(Jumping,true))
        {
            
            Jumping.timer = 0;
            Jumping.cooldown = UnityEngine.Random.Range(Jumping.minCooldown, Jumping.maxCooldown);
            AudioManager.Instance.PlayRandom("JumpingVoice");
        }
    }
    public void TryToHighSpeedSound() 
    {

        HighSpeedSliding.timer += Time.deltaTime;
        if (soundTimerandChance(HighSpeedSliding,false))
        {
            
            HighSpeedSliding.timer = 0;
            HighSpeedSliding.cooldown = UnityEngine.Random.Range(HighSpeedSliding.minCooldown, HighSpeedSliding.maxCooldown);
            AudioManager.Instance.PlayRandom("SlidingVoice");
        }
    }
    public void TryToAchievementSound() 
    {
        
        if (soundTimerandChance(Achievement,true))
        {
            
            Achievement.timer = 0;
            Achievement.cooldown = UnityEngine.Random.Range(Achievement.minCooldown, Achievement.maxCooldown);
            AudioManager.Instance.PlayRandom("AchievementVoice");
        }
    }
    public void TryToSmallAchievementSound()
    {
       
        if (soundTimerandChance(Achievement, true))
        {
            
            smallAchievement.timer = 0;
            smallAchievement.cooldown = UnityEngine.Random.Range(smallAchievement.minCooldown, smallAchievement.maxCooldown);
            AudioManager.Instance.PlayRandom("SmallAchievementVoice");
        }
    }
    public void TryToSwigningSound()
    {
       
        if (soundTimerandChance(Achievement, true))
        {
           
            swinging.timer = 0;
            swinging.cooldown = UnityEngine.Random.Range(swinging.minCooldown, swinging.maxCooldown);
            AudioManager.Instance.PlayRandom("swingingVoice");
        }
    }

    public void resetIdleTimer()
    {
        Idle.timer = 0;
    }
    public void resetWalkTimer()
    {
        Walking.timer =0;
    }
    public void resetJumpingTimer()
    {
        Jumping.timer = 0;
    }
    public void resetHighSpeedTimer()
    {
        HighSpeedSliding.timer = 0;
    }
    public void resetAchievementTimer()
    {
        Achievement.timer = 0;
    }
    public void resetSmallAchievementTimer()
    {
        smallAchievement.timer = 0;
    }
    public void resetSwingingTimer()
    {
        swinging.timer = 0;
    }

    bool soundTimerandChance(SoundType sound,bool withCoolDown) 
    {
      
        float randFloat = UnityEngine.Random.Range(0, 100);
        if (randFloat <= sound.chance) 
        {
            if (sound.coroutine == null)
            {
                if (withCoolDown)
                {
                    sound.coroutine = StartCoroutine(startCoolDown(sound));
                }
            }
            if (sound.timer >= sound.cooldown)
            {
                sound.timer = 0;
                if (sound.coroutine == null&&withCoolDown)
                {
                    sound.coroutine = StartCoroutine(startCoolDown(sound));
                }
                return true;
            }
        }

        return false;
    }

    IEnumerator startCoolDown(SoundType sound) 
    {
        WaitForEndOfFrame delay = new WaitForEndOfFrame();
        while (sound.timer <= sound.cooldown) 
        {
            sound.timer += Time.deltaTime;
            
            yield return delay;
        }
        sound.coroutine = null;
    }

    [Serializable]
    class SoundType
    {
        [Range(0, 100)] public float chance;
        public float minCooldown;
        public float maxCooldown;
        public float timer;
        public float cooldown;
        [HideInInspector] public Coroutine coroutine;
    }

}

