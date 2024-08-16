﻿namespace UnitedCallouts.Callouts;

[CalloutInfo("[UC] Warrant for Arrest", CalloutProbability.Medium)]
public class WarrantForArrest : Callout
{
    private static readonly string[] WepList =
        { "WEAPON_PISTOL", "WEAPON_SMG", "WEAPON_MACHINEPISTOL", "WEAPON_PUMPSHOTGUN" };

    private static Ped _subject;
    private static Vector3 _spawnPoint;
    private static Vector3 _searcharea;
    private static Blip _blip;
    private static int _storyLine = 1;
    private static int _callOutMessage;
    private static bool _attack;
    private static bool _hasWeapon;
    private static bool _alreadySubtitleIntrod;

    public override bool OnBeforeCalloutDisplayed()
    {
        List<Vector3> list = new List<Vector3>
        {
            new(-73.264f, -28.95624f, 65.75121f),
            new(-1123.261f, 483.8483f, 82.16084f),
            new(967.7412f, -546.0015f, 59.36506f),
            new(-109.5984f, -10.19665f, 70.51959f),
            new(-10.93565f, -1434.329f, 31.11683f),
            new(-1.838376f, 523.2645f, 174.6274f),
            new(-801.5516f, 178.7447f, 72.83471f),
            new(-812.7239f, 178.7438f, 76.74079f),
            new(3.542758f, 526.8926f, 170.6218f),
            new(-1155.698f, -1519.297f, 10.63272f),

        };
        _spawnPoint = LocationChooser.ChooseNearestLocation(list);
        ShowCalloutAreaBlipBeforeAccepting(_spawnPoint, 30f);
        switch (Rndm.Next(1, 4))
        {
            case 1:
                _attack = true;
                break;
            case 2:
                break;
            case 3:
                break;
        }

        switch (Rndm.Next(1, 4))
        {
            case 1:
                CalloutMessage = "[UC]~w~ Warrant for Arrest";
                _callOutMessage = 1;
                break;
            case 2:
                CalloutMessage = "[UC]~w~ Warrant for Arrest";
                _callOutMessage = 2;
                break;
            case 3:
                CalloutMessage = "[UC]~w~ Warrant for Arrest";
                _callOutMessage = 3;
                break;
        }

        CalloutPosition = _spawnPoint;
        Functions.PlayScannerAudioUsingPosition(
            "ATTENTION_ALL_UNITS CRIME_SUSPECT_RESISTING_ARREST_01 IN_OR_ON_POSITION", _spawnPoint);
        return base.OnBeforeCalloutDisplayed();
    }

    public override bool OnCalloutAccepted()
    {
        Game.LogTrivial("UnitedCallouts Log: Warrant for Arrest callout accepted.");
        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~UnitedCallouts",
            "~y~Warrant for Arrest",
            "~b~Dispatch:~w~ Try to ~o~speak~w~ and ~b~arrest~w~ the wanted person. Respond with ~r~Code 3");

        _subject = new Ped(_spawnPoint)
        {
            Position = _spawnPoint,
            IsPersistent = true,
            BlockPermanentEvents = true
        };
        var subjectPersona = Functions.GetPersonaForPed(_subject);
        subjectPersona.Wanted = true;

        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~UnitedCallouts",
            "~y~Dispatch", "Loading ~g~Information~w~ of the ~y~LSPD Database~w~...");
        Functions.DisplayPedId(_subject, true);

        _searcharea = _spawnPoint.Around2D(1f, 2f);
        _blip = new Blip(_searcharea, 30f);
        _blip.Color = Color.Yellow;
        _blip.EnableRoute(Color.Yellow);
        _blip.Alpha = 0.5f;
        return base.OnCalloutAccepted();
    }

    public override void OnCalloutNotAccepted()
    {
        if (_subject) _subject.Delete();
        if (_blip) _blip.Delete();
        base.OnCalloutNotAccepted();
    }

    public override void Process()
    {
        if (_subject.DistanceTo(MainPlayer) < 20f)
        {
            Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH OFFICERS_ARRIVED_ON_SCENE");
            if (_attack && !_hasWeapon)
            {
                _subject.Inventory.GiveNewWeapon(new WeaponAsset(WepList[Rndm.Next(WepList.Length)]), 500, true);
                _hasWeapon = true;
                _subject.Tasks.FightAgainst(MainPlayer);
            }

            if (!_attack && !_alreadySubtitleIntrod && _subject.DistanceTo(MainPlayer) < 10f && MainPlayer.IsOnFoot)
            {
                Game.DisplaySubtitle("Press ~y~Y ~w~to speak with the person.", 5000);
                _alreadySubtitleIntrod = true;
            }

            if (!_attack && Game.IsKeyDown(Settings.Dialog) && _subject.DistanceTo(MainPlayer) < 2f)
            {
                _subject.Face(MainPlayer);
                switch (_storyLine)
                {
                    case 1:
                        Game.DisplaySubtitle("~y~Suspect: ~w~Hello Officer! Can I help you? (1/5)", 5000);
                        _storyLine++;
                        break;
                    case 2:
                        Game.DisplaySubtitle("~b~You: ~w~We have a warrant for your arrest. (2/5)", 5000);
                        _storyLine++;
                        break;
                    case 3:
                        Game.DisplaySubtitle("~y~Suspect: ~w~...me? Are you sure? (3/5)", 5000);
                        _storyLine++;
                        break;
                    case 4:
                        switch (_callOutMessage)
                        {
                            case 1:
                                Game.DisplaySubtitle(
                                    "~b~You: ~w~I have to arrest you because we have an arrest warrant against you. You need to come with me. (4/5)",
                                    5000);
                                break;
                            case 2:
                                Game.DisplaySubtitle("~b~You: ~w~Tell that to the court. Don't make this hard! (4/5)",
                                    5000);
                                break;
                            case 3:
                                Game.DisplaySubtitle(
                                    "~b~You: ~w~Tell that to the court. Don't make this harder than what it needs to be! (4/5)",
                                    5000);
                                break;
                        }
                        _storyLine++;
                        break;
                    case 5:
                        switch (_callOutMessage)
                        {
                            case 1:
                                _subject.Tasks.PutHandsUp(-1, MainPlayer);
                                Game.DisplaySubtitle("~y~Suspect: ~w~Okay, fine. (5/5)", 5000);
                                break;
                            case 2:
                                Game.DisplaySubtitle("~y~Suspect: ~w~You're not taking me in, you pig! (5/5)", 5000);
                                _subject.Inventory.GiveNewWeapon("WEAPON_PISTOL", 500, true);
                                NativeFunction.Natives.TASK_COMBAT_PED(_subject, MainPlayer, 0, 16);
                                break;
                            case 3:
                                Game.DisplaySubtitle(
                                    "~y~Suspect: ~w~I'm not going with you... I'm sorry but I can't go back to prison! (5/5)",
                                    5000);
                                _subject.Inventory.GiveNewWeapon("WEAPON_KNIFE", 500, true);
                                NativeFunction.Natives.TASK_COMBAT_PED(_subject, MainPlayer, 0, 16);
                                break;
                        }
                        _storyLine++;
                        break;
                }
            }
        }

        if (MainPlayer.IsDead) End();
        if (Game.IsKeyDown(Settings.EndCall)) End();
        if (_subject && _subject.IsDead) End();
        if (_subject && Functions.IsPedArrested(_subject)) End();
        base.Process();
    }

    public override void End()
    {
        if (_subject) _subject.Dismiss();
        if (_blip) _blip.Delete();
        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~UnitedCallouts",
            "~y~Warrant for Arrest", "~b~You: ~w~Dispatch we're code 4. Show me ~g~10-8.");
        Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH ALL_UNITS_CODE4 NO_FURTHER_UNITS_REQUIRED");
        base.End();
    }
}
