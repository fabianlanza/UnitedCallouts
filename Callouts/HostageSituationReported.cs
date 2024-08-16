﻿namespace UnitedCallouts.Callouts;

[CalloutInfo("[UC] Hostage Situation Reported", CalloutProbability.Medium)]
public class HostageSituationReported : Callout
{
    private static readonly string[] WepList =
        { "weapon_bullpuprifle_mk2", "WEAPON_SMG", "WEAPON_MACHINEPISTOL", "WEAPON_PUMPSHOTGUN" };

    private static Ped _ag1;
    private static Ped _ag2;
    private static Ped Victim1 => _victimPeds[0];
    private static Ped Victim2 => _victimPeds[1];
    private static Ped Victim3 => _victimPeds[2];
    private static Ped Victim4 => _victimPeds[3];
    private static Vector3 _searchArea;
    private static Vector3 _spawnPoint;
    private static Blip _spawnLocationBlip;
    private static bool _scene1;
    private static bool _scene2;
    private static bool _notificationDisplayed;
    private static RelationshipGroup _viRelationshipGroup = new("VI");

    private static List<Ped> _victimPeds = new List<Ped>(4);

    public override bool OnBeforeCalloutDisplayed()
    {
        List<Vector3> list = new List<Vector3>
        {
            new(976.6871f, -96.42852f, 74.84537f),
            new(1250.002f, -3014.48f, 9.319259f),

        };

        _spawnPoint = LocationChooser.ChooseNearestLocation(list);
        _ag1 = new("mp_g_m_pros_01", _spawnPoint, 0f);
        _ag2 = new("mp_g_m_pros_01", _spawnPoint, 0f);

        for (int i = 0; i < _victimPeds.Count; i++)
        {
            _victimPeds[i] = new(_spawnPoint, 0f);
        }

        switch (Rndm.Next(1, 4))
        {
            case 1:
                _scene1 = true;
                break;
            case 2:
                _scene2 = true;
                break;
            case 3:
                _scene2 = true;
                break;
        }

        ShowCalloutAreaBlipBeforeAccepting(_spawnPoint, 100f);
        AddMinimumDistanceCheck(10f, _spawnPoint);
        CalloutMessage = "[UC]~w~ Hostage Situation Reported";
        CalloutPosition = _spawnPoint;
        return base.OnBeforeCalloutDisplayed();
    }

    public override bool OnCalloutAccepted()
    {
        _ag1.IsPersistent = true;
        _ag1.BlockPermanentEvents = true;
        _ag1.Inventory.GiveNewWeapon(new WeaponAsset(WepList[Rndm.Next(WepList.Length)]), 500, true);
        _ag1.Health = 200;
        _ag1.Armor = 300;

        _ag2.IsPersistent = true;
        _ag2.BlockPermanentEvents = true;
        _ag2.Inventory.GiveNewWeapon(new WeaponAsset(WepList[Rndm.Next(WepList.Length)]), 500, true);
        _ag2.Health = 200;
        _ag2.Armor = 300;

        foreach (var ped in _victimPeds)
        {
            ped.BlockPermanentEvents = true;
            ped.IsPersistent = true;
            ped.Tasks.PlayAnimation("random@arrests@busted", "idle_a", 8.0F, AnimationFlags.Loop);
            ped.RelationshipGroup = _viRelationshipGroup;
        }

        NativeFunction.Natives.TASK_AIM_GUN_AT_ENTITY(_ag1, Victim1, -1, true);
        NativeFunction.Natives.TASK_AIM_GUN_AT_ENTITY(_ag2, Victim2, -1, true);

        RelationshipGroup agRelationshipGroup = new("AG");
        _ag1.RelationshipGroup = agRelationshipGroup;
        _ag2.RelationshipGroup = agRelationshipGroup;

        agRelationshipGroup.SetRelationshipWith(_viRelationshipGroup, Relationship.Hate);
        agRelationshipGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);

        _searchArea = _spawnPoint.Around2D(1f, 2f);
        _spawnLocationBlip = new(_searchArea, 40f)
        {
            Color = Color.Yellow,
            Alpha = 0.5f
        };
        _spawnLocationBlip.EnableRoute(Color.Yellow);

        Functions.PlayScannerAudioUsingPosition("DISP_ATTENTION_UNIT WE_HAVE CRIME_ROBBERY IN_OR_ON_POSITION",
            _spawnPoint);
        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~UnitedCallouts", "~y~FIB",
            "The police department called for the ~y~Hostage Rescue Team~w~ to rescue all ~o~hostages~w~.");
        return base.OnCalloutAccepted();
    }

    public override void OnCalloutNotAccepted()
    {
        if (_ag1) _ag1.Delete();
        if (_ag2) _ag2.Delete();
        foreach (var ped in _victimPeds)
        {
            if (ped) ped.Delete();
        }

        base.OnCalloutNotAccepted();
    }

    public override void Process()
    {
        if (!_notificationDisplayed && MainPlayer.IsOnFoot && _spawnPoint.DistanceTo(MainPlayer) < 25f)
        {
            if (_spawnLocationBlip) _spawnLocationBlip.Delete();

            Game.DisplayHelp("The ~y~Hostage Rescue Team~w~ is ~g~on scene~w~.");
            _notificationDisplayed = true;
        }

        if (_ag1.DistanceTo(MainPlayer) < 14f)
        {
            if (_scene1 && !_scene2 && _ag1.DistanceTo(MainPlayer) < 18f)
            {
                Game.DisplaySubtitle("~y~Criminal~w~: shhh....I am hearing steps!");

                _ag1.Tasks.FightAgainstClosestHatedTarget(1000f);
                _ag2.Tasks.FightAgainstClosestHatedTarget(1000f);
            }

            if (_scene2 && !_scene1 && MainPlayer.DistanceTo(_ag1) < 18f)
            {
                _ag1.Tasks.FightAgainstClosestHatedTarget(1000f);
                _ag2.Tasks.FightAgainstClosestHatedTarget(1000f);
            }
        }

        if (_ag1 && _ag1.IsDead && _ag2 && _ag2.IsDead) End();
        if (_ag1 && Functions.IsPedArrested(_ag1) && _ag2 && Functions.IsPedArrested(_ag2)) End();
        if (Game.IsKeyDown(Settings.EndCall)) End();
        if (MainPlayer.IsDead) End();
        base.Process();
    }

    public override void End()
    {
        if (_ag1) _ag1.Dismiss();
        if (_ag2) _ag2.Dismiss();
        foreach (var ped in _victimPeds)
        {
            if (ped) ped.Dismiss();
        }

        if (_spawnLocationBlip) _spawnLocationBlip.Delete();
        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~UnitedCallouts",
            "~y~Hostage Situation Reported", "~b~You: ~w~Dispatch we're code 4. Show me ~g~10-8.");
        Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH ALL_UNITS_CODE4 NO_FURTHER_UNITS_REQUIRED");
        base.End();
    }
}
