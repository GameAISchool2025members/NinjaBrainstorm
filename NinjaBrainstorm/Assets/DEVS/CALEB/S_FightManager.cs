using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class S_FightManager : MonoBehaviour
{
    [SerializeField]
    S_PlayerController player1;
    [SerializeField] 
    S_PlayerController player2;
    
    [SerializeField]
    private int maxActions = 5;

    PlayableDirector director;
    AnimationTrack p1Track;
    AnimationTrack p2Track;
    TimelineAsset asset;


    private void Start()
    {
        director = GetComponent<PlayableDirector>();
        asset = director.playableAsset as TimelineAsset;
        ClearTimeline(asset);
        p1Track = asset.CreateTrack<AnimationTrack>("p1Track");
        p2Track = asset.CreateTrack<AnimationTrack>("p2Track");
        director.SetGenericBinding(p1Track, player1.GetComponent<Animator>());
        director.SetGenericBinding(p2Track, player2.GetComponent<Animator>());

        Fight();

        director.Play();

    }

    void ClearTimeline(TimelineAsset a)
    {
        for (int i = 0; i < a.outputTrackCount; i++)
        {
            a.DeleteTrack(a.GetOutputTrack(i));
        }
        for (int i = 0; i < a.rootTrackCount; i++)
        {
            a.DeleteTrack(a.GetRootTrack(i));
        }
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
            char p1Direction = '+';
            char p2Direction = '-';

            if (i < player1.PlayerSequence.Count)
            { p1Action = player1.PlayerSequence[i][0]; p1Direction = player1.PlayerSequence[i][2]; }
            if(i < player2.PlayerSequence.Count) 
            { p2Action = player2.PlayerSequence[i][0]; p2Direction = player2.PlayerSequence[i][2]; }


            //player1
            TimelineClip clip1 = null;
            switch (p1Action)
            {
                case 'N':
                    clip1 = p1Track.CreateClip(player1.AnimClips[0]);
                    break;
                case 'P':
                    clip1 = AnimDirection(p1Direction, player1, i, 1, 1, 1, p1Track);
                    break;
                case 'D':
                    clip1 = AnimDirection(p1Direction, player1, i, 2, 2, 2, p1Track);
                    break;
            }
            clip1.start = currentTimeTrack1;
            clip1.duration = 2f;
            currentTimeTrack1 += (clip1.duration);
            if(p1Action != 'N')
            {
                AddElementEffect(player1.PlayerSequence[i][1], clip1.start, clip1.duration, player1);
            }

            //player2
            TimelineClip clip2 = null;
            switch(p2Action)
            {
                case 'N':
                    clip2 = p2Track.CreateClip(player2.AnimClips[0]);
                    break;
                case 'P':
                    clip2 = AnimDirection(p2Direction, player2, i, 1, 1, 1, p2Track);
                    
                    break;
                case 'D':
                    clip2 = AnimDirection(p2Direction, player2, i, 2, 2, 2, p2Track);
                    break;
            }
            clip2.start = currentTimeTrack2;
            clip2.duration = 2f;
            currentTimeTrack2 += (clip2.duration);
            if (p2Action != 'N')
            {
                AddElementEffect(player2.PlayerSequence[i][1], clip2.start, clip2.duration, player2);
            }

            if (player1.IsHit)
            {
                player1.CalculateDamageAmount(GetActionAngle(player1.PlayerSequence[i]), GetActionAngle(player2.PlayerSequence[i]));
                TimelineClip clip3 = p1Track.CreateClip(player1.AnimClips[3]);
                clip3.start = currentTimeTrack1;
                clip3.duration = 2f;
                currentTimeTrack1 += (clip3.duration);
                player1.HitCharge = 0;
            }
            if(player2.IsHit)
            {
                player2.CalculateDamageAmount(GetActionAngle(player2.PlayerSequence[i]), GetActionAngle(player1.PlayerSequence[i]));
                TimelineClip clip4 = p2Track.CreateClip(player2.AnimClips[3]);
                clip4.start = currentTimeTrack2;
                clip4.duration = 2f;
                currentTimeTrack2 += (clip4.duration);
                player2.HitCharge = 1;
            }

            if(currentTimeTrack1 > currentTimeTrack2)
            {
                TimelineClip clip5 = p2Track.CreateClip(player2.AnimClips[0]);
                clip5.start = currentTimeTrack2;
                clip5.duration = currentTimeTrack1 - currentTimeTrack2;
                currentTimeTrack2 = currentTimeTrack1 ;
            }
            else if(currentTimeTrack1 < currentTimeTrack2)
            {
                TimelineClip clip6 = p1Track.CreateClip(player1.AnimClips[0]);
                clip6.start = currentTimeTrack1;
                clip6.duration = currentTimeTrack2 - currentTimeTrack1;
                currentTimeTrack1 = currentTimeTrack2;
            }
        }
    }

    void AddElementEffect(char element, double start, double duration, S_PlayerController player)
    {
        ActivationTrack track = asset.CreateTrack<ActivationTrack>("elementTrack");
        switch (element)
        {
            case 'F':
                TimelineClip clip = track.CreateDefaultClip();
                clip.start = start;
                clip.duration = duration;
                director.SetGenericBinding(track, player.elementsEffects[0]);
                break;
            case 'G':
                TimelineClip clip2 = track.CreateDefaultClip();
                clip2.start = start;
                clip2.duration = duration;
                director.SetGenericBinding(track, player.elementsEffects[1]);
                break;
            case 'W':
                TimelineClip clip3 = track.CreateDefaultClip();
                clip3.start = start;
                clip3.duration = duration;
                director.SetGenericBinding(track, player.elementsEffects[2]);
                break;
        }
    }

    TimelineClip AnimDirection(char playerDirection, S_PlayerController player, int i, int leftAnim, int rightAnim, int straightAnim, AnimationTrack track)
    {
        TimelineClip clip = null;
        if (playerDirection == '-' && player.PlayerSequence[i][3] >= 2)
        { clip = track.CreateClip(player.AnimClips[leftAnim]); }//left
        else if (playerDirection == '+' && player.PlayerSequence[i][3] >= 2)
        { clip = track.CreateClip(player.AnimClips[rightAnim]); }//right
        else if (player.PlayerSequence[i][3] < 2)
        { clip = track.CreateClip(player.AnimClips[straightAnim]); }//straight

        return clip;
    }

    int GetActionAngle(string action)
    {
        char[] chars = {action[3], action[4]};
        string angleSt = new string(chars);
        int angle = Int32.Parse(angleSt);

        return angle;
    }

    bool P1WinsElement(char p1Element, char p2Element)
    {
        bool result = false;

        switch (p1Element)
        {
            case 'W':
                if(p2Element == 'F') { result = true; }
                else if(p2Element == 'G') { result = false; }
                break;
            case 'F':
                if (p2Element == 'W') { result = false; }
                else if (p2Element == 'G') { result = true; }
                break;
            case 'G':
                if (p2Element == 'F') { result = false; }
                else if (p2Element == 'W') { result = true; }
                break;
        }

        return result;
    }
    void CompareAction(int index)
    {
        //reset hits
        player1.IsHit = false;
        player2.IsHit = false;

        char p1Type = 'N';
        char p1Action = 'N';

        if (index < player1.PlayerSequence.Count) {
            //get action and element for player 1
            p1Action = player1.PlayerSequence[index][0];
            p1Type = player1.PlayerSequence[index][1];
        }

        char p2Action = 'N';
        char p2Type = 'N';

        if (index < player2.PlayerSequence.Count) {
            //get action and element for player 2
            p2Action = player2.PlayerSequence[index][0];
            p2Type = player2.PlayerSequence[index][1];
        }


        if (p1Action == p2Action)//player 1 and 2 both do the same action
        {
            if(p1Action == 'N') { return; }//no action
            if (p1Action == 'P')//action is attack
            {
                if (p1Type == p2Type)
                {
                    player1.IsHit = true;
                    player2.IsHit = true;
                }
                else
                {
                    player1.IsHit = P1WinsElement(p1Type, p2Type) ? false : true;
                    player2.IsHit = P1WinsElement(p1Type, p2Type) ? true : false;
                }
            }
        }
        else//player 1 and 2 are doing different actions
        {
            bool p1IsDefending;
            if (p1Action == 'D') { p1IsDefending = true; } else { p1IsDefending = false; }//get if player 1 is defending
            if(p1Action == 'N' && p2Action == 'P') { player1.IsHit=true; return; }
            if(p2Action == 'N' && p1Action == 'P') { player2.IsHit=true; return; }
            if(p1Type == p2Type) { if (p1IsDefending) { player1.HitCharge++; } else { player2.HitCharge++; } }

            if (p1IsDefending)
            {
                if (P1WinsElement(p1Type, p2Type))
                {
                    print(p1Type + p2Type);
                    return;
                }
                else
                {
                    player1.IsHit = true;
                }
            }
            else
            {
                if (P1WinsElement(p1Type, p2Type))
                {
                    player2.IsHit = true;
                }
                else
                {
                    return;
                }
            }

        }
    }
}
