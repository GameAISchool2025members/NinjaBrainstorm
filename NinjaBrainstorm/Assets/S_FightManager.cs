using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class S_FightManager : MonoBehaviour
{
    [SerializeField]
    Animator p1Animator;
    [SerializeField] 
    Animator p2Animator;

    [Space][SerializeField]
    AnimationClip[] clips;
    
    [Space][SerializeField]
    private List<string> player1Sequence = new List<string>();
    [SerializeField]
    private List<string> player2Sequence = new List<string>();
    [SerializeField]
    private int maxActions = 5;

    PlayableDirector director;
    AnimationTrack p1Track;
    AnimationTrack p2Track;

    bool p1IsHit = false;
    bool p2IsHit = false;

    private void Start()
    {
        director = GetComponent<PlayableDirector>();
        TimelineAsset asset = director.playableAsset as TimelineAsset;
        p1Track = asset.CreateTrack<AnimationTrack>("p1Track");
        p2Track = asset.CreateTrack<AnimationTrack>("p2Track");
        director.SetGenericBinding(p1Track, p1Animator);
        director.SetGenericBinding(p2Track, p2Animator);

        Fight();

        director.Play();

    }

    void ClearTimeline()
    {
        //tomorrow find how to delete tracks
    }

    void Fight()
    {
        double currentTimeTrack1 = 0f;
        double currentTimeTrack2 = 0f;
        //read first action
        for(int i = 0; i < maxActions; i++)
        {
            CompareAction(i);
            char p1Action = 'N';
            char p2Action = 'N';

            if (i < player1Sequence.Count)
            { p1Action = player1Sequence[i][0]; }
            if(i < player2Sequence.Count) 
            { p2Action = player2Sequence[i][0]; }

            //print("Player1: " + p1Action + " , isHit: " + p1IsHit);
            //print("Player2: " + p2Action + " , isHit: " + p2IsHit);

            //player1
            TimelineClip clip1 = null;
            switch (p1Action)
            {
                case 'N':
                    clip1 = p1Track.CreateClip(clips[0]);
                    break;
                case 'P':
                    clip1 = p1Track.CreateClip(clips[1]);
                    break;
                case 'D':
                    clip1 = p1Track.CreateClip(clips[2]);
                    break;
            }
            print(clip1 + "info");
            clip1.start = currentTimeTrack1;
            clip1.duration = 2f;
            currentTimeTrack1 += (clip1.duration);

            //player2
            TimelineClip clip2 = null;
            switch(p2Action)
            {
                case 'N':
                    clip2 = p2Track.CreateClip(clips[0]);
                    break;
                case 'P':
                    clip2 = p2Track.CreateClip(clips[1]);
                    break;
                case 'D':
                    clip2 = p2Track.CreateClip(clips[2]);
                    break;
            }
            print(clip2 + "info");
            clip2.start = currentTimeTrack2;
            clip2.duration = 2f;
            currentTimeTrack2 += (clip2.duration);

            if(p1IsHit)
            {
                TimelineClip clip3 = p1Track.CreateClip(clips[3]);
                clip3.start = currentTimeTrack1;
                clip3.duration = 2f;
                currentTimeTrack1 += (clip3.duration);
            }
            if(p2IsHit)
            {
                TimelineClip clip4 = p2Track.CreateClip(clips[3]);
                clip4.start = currentTimeTrack2;
                clip4.duration = 2f;
                currentTimeTrack2 += (clip4.duration);
            }

            if(currentTimeTrack1 > currentTimeTrack2)
            {
                TimelineClip clip5 = p2Track.CreateClip(clips[0]);
                clip5.start = currentTimeTrack2;
                clip5.duration = currentTimeTrack1 - currentTimeTrack2;
                currentTimeTrack2 = currentTimeTrack1 ;
            }
            else if(currentTimeTrack1 < currentTimeTrack2)
            {
                TimelineClip clip6 = p1Track.CreateClip(clips[0]);
                clip6.start = currentTimeTrack1;
                clip6.duration = currentTimeTrack2 - currentTimeTrack1;
                currentTimeTrack1 = currentTimeTrack2;
            }
        }
    }
    void CompareAction(int index)
    {
        //reset hits
        p1IsHit = false;
        p2IsHit = false;

        char p1Type = 'N';
        char p1Action = 'N';

        if (index < player1Sequence.Count) {
            //get action and element for player 1
            p1Action = player1Sequence[index][0];
            p1Type = player1Sequence[index][1];
        }

        char p2Action = 'N';
        char p2Type = 'N';

        if (index < player2Sequence.Count) {
            //get action and element for player 2
            p2Action = player2Sequence[index][0];
            p2Type = player2Sequence[index][1];
        }


        if (p1Action == p2Action)//player 1 and 2 both do the same action
        {
            if(p1Action == 'N') { return; }//no action
            if (p1Action == 'P')//action is attack
            {
                if (p1Type == p2Type)
                {
                    p1IsHit = true;
                    p2IsHit = true;
                }
                else
                {
                    switch (p1Type)
                    {
                        case 'G':
                            if (p2Type == 'F') { p1IsHit = true; }
                            if (p2Type == 'W') { p2IsHit = true; }
                            break;
                        case 'F':
                            if (p2Type == 'W') { p2IsHit = true; }
                            if (p2Type == 'G') { p1IsHit = true; }
                            break;
                        case 'W':
                            if (p2Type == 'F') { p2IsHit = true; }
                            if (p2Type == 'G') { p1IsHit = true; }
                            break;
                    }
                }
            }
        }
        else//player 1 and 2 are doing different actions
        {
            bool p1Defence;
            if (p1Action == 'D') { p1Defence = true; } else { p1Defence = false; }//get if player 1 is defending
            if(p1Action == 'N' && p2Action == 'P') { p1IsHit=true; return; }
            if(p2Action == 'N' && p1Action == 'P') { p2IsHit=true; return; }

            switch (p1Type)
            {
                case 'G':
                    if (p2Type == 'F') { p1IsHit = p1Defence ? true:false; }//if p1 is defending from fire with grass they get hit
                    if (p2Type == 'W') { p2IsHit = p1Defence ? false:true; }//if p2 is defending from grass with water they get hit
                    break;
                case 'F':
                    if (p2Type == 'W') { p1IsHit = p1Defence ? true:false; }//if p1 is defending from water with fire they get hit
                    if (p2Type == 'G') { p2IsHit = p1Defence ? false:true; }//if p2 is defending from fire with grass they get hit
                    break;
                case 'W':
                    if (p2Type == 'F') { p2IsHit = p1Defence ? false:true; }//if p2 is defending from fire with water they get hit
                    if (p2Type == 'G') { p1IsHit = p1Defence ? true:false; }//if p1 is defending from grass with water they get hit
                    break;
            }

        }
    }
}
