using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AudioSystem : BaseSystem<GameEvents, GameGuiEvents, GameSetupEvents, AudioSystem>
{
    public AudioClip BackgroundMusic, DetectiveWinMusic, MrXWinMusic;

    public Transform SfxHolder;
    public AudioSource MainMusic, MusicEnd;
    Queue<AudioSource> sfxHolders = new Queue<AudioSource>();

    protected override void RegisterEvents()
    {
        SfxHolder.GetComponents<AudioSource>().ForEach((o) => sfxHolders.Enqueue(o));

        ListenTo<GameOverArgs>(GameEvents.GameEnd, GameEnd);
        ListenTo<MoveArgs>(GameEvents.DetectiveMove, PlayerMove);
        ListenTo<MoveArgs>(GameEvents.MrXMove, PlayerMove);
        ListenTo(GameEvents.MrXUseDoubleTicket, DoubleTicket);
        ListenTo(GameEvents.MrXAppear, MrXAppear);
        ListenTo(GameEvents.PlayerCannotMove, PlayerCannotMove);
        ListenTo(GameEvents.TurnEnd, TurnStart); // Use TurnEnd not TurnStart as event because in the tutorial TurnStart is broadcasted sometimes in between

        ListenTo(GameGuiEvents.TicketPopupOpened, TicketPopupOpened);
        ListenTo(GameGuiEvents.FocusPosition, Focus);
        ListenTo(GameGuiEvents.ClickedAnywhere, ClickedAnywhere);
        ListenTo(GameGuiEvents.TimeOutInThreeSeconds, TimeOutIn3);
        ListenTo(GameGuiEvents.MoveHistoryPanelClick, MoveHistoryClicked);
        ListenTo(GameGuiEvents.StationClicked, StationClicked);

        ListenTo(GameSetupEvents.PlayerChoseCard, PlayerChoseCard);

        UIButton.AnyButtonClicked += AnyButtonClicked;
        UIPlayAnimation.AnyAnimationStarted += AnyAnimationStarted;

        if (AppSetup.Instance.IsMusicEnabled)
        {
            MainMusic.volume = 1;
            MainMusic.Play();
        }
    }


    protected override void OnDestroy()
    {
        UIButton.AnyButtonClicked -= AnyButtonClicked;
        UIPlayAnimation.AnyAnimationStarted -= AnyAnimationStarted;
        base.OnDestroy();
    }

    void OnApplicationFocus(bool focus)
    {
        if (!AppSetup.Instance.IsMusicEnabled)
            return;


        if(focus)
        {
            this.WaitAndDo(new WaitForSeconds(2f), null,
                () => StartCoroutine(Helpers.CoFadeAudio(MainMusic, 1, 5, null)));
        }
        else
        {
            StopAllCoroutines();
            MainMusic.volume = 0;
        }
    }

    public void EnableMusic(bool enabled)
    {
        StopAllCoroutines();

        if(enabled)
        {
            StartCoroutine(Helpers.CoFadeAudio(MainMusic, 1, 1, null));

            if(MusicEnd != null)
                StartCoroutine(Helpers.CoFadeAudio(MusicEnd, 1, 1, null));

            MainMusic.Play();
        }
        else
        {
            StartCoroutine(Helpers.CoFadeAudio(MainMusic, 0, 1, () => MainMusic.Pause()));

            if (MusicEnd != null)
                StartCoroutine(Helpers.CoFadeAudio(MusicEnd, 0, 1, null));
        }

    }

    public void EnableSfx(bool enabled)
    {
        if(!enabled)
        {
            sfxHolders.ForEach(o => o.Stop());
        }
    }

    void AnyAnimationStarted(UIPlayAnimation obj)
    {
        if (obj.clipName == "Window_Forward")
            PlaySfx("menu_slide");
    }
    private void AnyButtonClicked(UIButton obj)
    {
        PlaySfx("ui_button_click");
    }

    private void StationClicked(BaseArgs args)
    {
        var s = args.RelatedObject.GetComponent<Station>();
        if (s != null && !s.IsHighlighted)
            PlaySfx("unreachable_station_clicked");
    }

    private void MoveHistoryClicked(BaseArgs obj)
    {
        PlaySfx("scroll_move_history");
    }

    private void TicketPopupOpened(BaseArgs obj)
    {
        PlaySfx("ticket_menu");
    }

    private void TurnStart(BaseArgs obj)
    {
        if(GameState.Instance.CurrentPlayer.MovesThisTurn == 0)
            PlaySfx("next_turn");
    }


    private void TimeOutIn3(BaseArgs obj)
    {
        PlaySfx("time_is_ending");
    }

    private void PlayerChoseCard(BaseArgs obj)
    {
        PlaySfx("card_flip");
    }

    private void PlayerCannotMove(BaseArgs args)
    {
        string sfx = "can_not_move_0" + UnityEngine.Random.Range(1, 3);
        PlaySfx(sfx);
    }

    private void Focus(BaseArgs args)
    {
        if(GameState.Instance.MrX.LastAppearance != null && args.RelatedObject == GameState.Instance.MrX.LastAppearance.gameObject)
        {
            PlaySfx("mr_x_focusing_last_position");
        }
    }

    private void MrXAppear(BaseArgs obj)
    {
        PlaySfx("mr_x_appear", 2f);
    }

    private void DoubleTicket(BaseArgs obj)
    {
        PlaySfx("double_ticket");
    }

    private void PlayerMove(MoveArgs args)
    {

        if(args.MovingPlayer.IsDetective)
        {
            if(args.To.GetStationNeighbours(TransportationType.Any).Contains(GameState.Instance.MrX.Location))
            {
                PlaySfx("detective_is_close", 1.5f);
            }
        }

        string sfx = "transportation_taxi";
        float delay = 0;
        switch(args.Ticket)
        {
            case TransportationType.Taxi:
                sfx = "transportation_taxi";
                break;
            case TransportationType.Bus:
                sfx = "transportation_bus";
                break;
            case TransportationType.Metro:
                int rnd = UnityEngine.Random.Range(1, 3);
                sfx = "transportation_metro" + rnd;
                delay = 0.25f;
                break;
            case TransportationType.Any:
                bool mrXSoundAllowed = GameSetupBehaviour.Instance.Setup.MrXSetup.Controller == PlayerController.Human && GameSetupBehaviour.Instance.Setup.DetectiveSetups.FirstOrDefault(o => o.Controller == PlayerController.Human) == null;
             

                if (mrXSoundAllowed)
                {  
                    bool b1 = args.Contains(194);
                    bool b2 = args.Contains(157);
                    bool b3 = args.Contains(115);
                    bool b4 = args.Contains(108);
                    if((b1 && b2) || (b2 && b3) || (b3 && b4))
                    {
                        sfx = "transportation_boat";
                    }
                    else
                    {
                        sfx = "transportation_black";
                    }
                    
                }
                else
                {
                    sfx = "transportation_black";
                }

                break;
        }

        PlaySfx(sfx, delay);
    }

    private void GameEnd(GameOverArgs args)
    {
        string sfx = "mr_x_lose";
        if (args.Reason == GameOverReason.MrXCaught || args.Reason == GameOverReason.MrXSurrounded)
        {
            MusicEnd.clip = DetectiveWinMusic;
            sfx = "mr_x_lose";
        }
        else
        {
            MusicEnd.clip = MrXWinMusic;
            sfx = "mr_x_win";
        }

        if (AppSetup.Instance.IsSfxEnabled)
        {
            float delay = 0.5f;
            StartCoroutine(Helpers.CoFadeAudio(MainMusic, 0, delay, () => { MainMusic.Stop(); PlaySfx(sfx); }));
            MusicEnd.PlayDelayed(delay + delay);
        }
        else
        {
            PlaySfx(sfx);
        }
    }

    private void ClickedAnywhere(BaseArgs obj)
    {
        PlaySfx("click_anywhere");
    }

    private void PlaySfx(string resourcePath) { PlaySfx(resourcePath, 0); }
    private void PlaySfx(string resourcePath, float delay)
    {
        AudioClip clip = Resources.Load("Sfx/" + resourcePath) as AudioClip;
        if(clip == null)
        {
            this.LogError("Sfx not found: " + resourcePath);
        }
        PlaySfx(clip, delay);
    }

    private void PlaySfx(AudioClip clip, float delay)
    {
        if (!AppSetup.Instance.IsSfxEnabled)
            return;

        AudioSource source = sfxHolders.Dequeue();

        source.Stop();
        source.clip = clip;

        if (delay == 0)
            source.Play();
        else
            source.PlayDelayed(delay);

        sfxHolders.Enqueue(source);
    }

}
