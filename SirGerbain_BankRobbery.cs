using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API;
using System.Security.Claims;
using System.Xml.Linq;

namespace SirGerbain_BankRobbery
{
    [CalloutProperties("Bank Robbery", "sirGerbain", "1.0.0")]
    public class SirGerbain_BankRobbery : FivePD.API.Callout
    {

        Vector3[] robberyLocations =
        {
            new Vector3(-107, 6466.96f, 31.63f), //Paleto Bank - pedHeading 130
        };

        Vector3[] robberyVehicleLocations =
        {
            new Vector3(-115.59f, 6458.28f, 31.19f), //Paleto Bank - vehicleHeading 100
        };

        Vector3[] robberyVehicleEntryLocations =
        {
            new Vector3(-116.45f, 6460.54f, 31.47f), //Paleto Bank - vehicleHeading 100
        };

        Ped hostage;
        List<Ped> robbers = new List<Ped>();
        List<PedHash> robberList = new List<PedHash>(); //rename robberHashList
        List<PedHash> hostageList = new List<PedHash>(); //rename hostageHashList
        Vector3 robberyLocation, robberyVehicleLocation;
        Random random = new Random();
        Vehicle robbersVehicle;
        int robberyLocationIndex, aimingRobber;
        bool arrivalOnScene = false;
        bool initiateRobbery = false;
        bool pursuit = false;
        bool startPursuit = false;

        public SirGerbain_BankRobbery()
        {
            robberyLocationIndex = random.Next(0, robberyLocations.Length - 1);
            robberyLocation = robberyLocations[robberyLocationIndex];
            robberyVehicleLocation = robberyVehicleLocations[robberyLocationIndex];

            robberList.Add(PedHash.ArmGoon02GMY);
            robberList.Add(PedHash.ArmGoon01GMM);
            robberList.Add(PedHash.ArmBoss01GMM);
            robberList.Add(PedHash.KorBoss01GMM);
            robberList.Add(PedHash.Korean01GMY);
            robberList.Add(PedHash.Korean02GMY);

            hostageList.Add(PedHash.Hooker02SFY);
            hostageList.Add(PedHash.Gay02AMY);
            hostageList.Add(PedHash.Runner01AFY);

            InitInfo(robberyLocation);
            ShortName = "Bank Robbery in progress";
            CalloutDescription = "All units, we have a bank truck robbery in progress. Suspects are armed and dangerous, shots have been fired. Use caution.";
            ResponseCode = 3;
            StartDistance = 120f;

        }

        public async override Task OnAccept()
        {
            InitBlip();
            UpdateData();
            //OnStart(Game.PlayerPed);

        }

        public async override void OnStart(Ped closest)
        {
            setupCallout();
            base.OnStart(closest);

            while (!arrivalOnScene)
            {
                await BaseScript.Delay(1000);
                float distance = Game.PlayerPed.Position.DistanceToSquared(robberyLocation);
                if (distance > 100f)
                {
                    foreach (var r in robbers)
                    {
                        r.AttachBlip();
                        r.AlwaysKeepTask = true;
                        r.BlockPermanentEvents = true;
                    }
                    arrivalOnScene = true;
                    break;
                }
            }

            while (!initiateRobbery && arrivalOnScene)
            {
                await BaseScript.Delay(5000);
                if (random.Next(0, 100) > 25 && !pursuit)
                {

                    if (random.Next(0, 100) > 15) //chance hostage gets shot
                    {
                        robbers[aimingRobber].Task.ShootAt(hostage);
                        robbers[aimingRobber].Task.ClearAll();
                    }

                    for (int i = 0; i < robbers.Count; i++)
                    {
                        float offsetX_ = 1.0f * (float)Math.Cos(i * 120.0f * (Math.PI / 180.0));
                        float offsetY_ = 1.0f * (float)Math.Sin(i * 120.0f * (Math.PI / 180.0));
                        Vector3 seatLocation = robberyVehicleEntryLocations[robberyLocationIndex] + new Vector3(offsetX_, offsetY_, 0);
                        using (TaskSequence sequence = new TaskSequence())
                        {
                            sequence.AddTask.GoTo(seatLocation);
                            if (i == 0)
                            {
                                sequence.AddTask.EnterVehicle(robbersVehicle, VehicleSeat.Driver);
                            }
                            else
                            {
                                sequence.AddTask.EnterVehicle(robbersVehicle, VehicleSeat.Any);
                            }
                            sequence.Close();
                            robbers[i].Task.PerformSequence(sequence);
                        }
                    }

                    pursuit = true;

                }

                if (pursuit && !startPursuit)
                {
                    while ((robbers[1].IsInVehicle() || !robbers[1].IsAlive) && (robbers[2].IsInVehicle() || !robbers[2].IsAlive))
                    {
                        robbers[0].Task.CruiseWithVehicle(robbersVehicle, 150f, 525116);

                        hostage.Task.ReactAndFlee(robbers[1]);
                        startPursuit = true;
                        break;
                    }
                }

                if(pursuit && startPursuit) {

                    await BaseScript.Delay(random.Next(10000, 23000));
                    int theChosenOne = random.Next(1, robbers.Count);
                    robbers[theChosenOne].Task.VehicleShootAtPed(closest);
                    await BaseScript.Delay(random.Next(1000, 5000));
                    robbers[theChosenOne].Task.ClearAll();

                }

            }
        }

        public async void setupCallout()
        {
            for (int i = 0; i < 3; i++)
            {
                float offsetX = 2.0f * (float)Math.Cos(i * 120.0f * (Math.PI / 180.0));
                float offsetY = 2.0f * (float)Math.Sin(i * 120.0f * (Math.PI / 180.0));
                Vector3 robberLocation = robberyLocation + new Vector3(offsetX, offsetY, 0);
                Ped robber = await SpawnPed(robberList[random.Next(0, robberList.Count)], robberLocation);
                robber.AlwaysKeepTask = true;
                robber.BlockPermanentEvents = true;
                robber.Weapons.Give(WeaponHash.Pistol, 250, true, true);
                robber.Heading = random.Next(120, 150);
                robber.Task.GuardCurrentPosition();
                robbers.Add(robber);
            }

            hostage = await SpawnPed(hostageList[random.Next(0, hostageList.Count)], robberyLocation);
            hostage.AlwaysKeepTask = true;
            hostage.BlockPermanentEvents = true;
            hostage.Heading = random.Next(120, 150);
            hostage.Task.HandsUp(-1);

            aimingRobber = random.Next(0, robbers.Count);
            robbers[aimingRobber].Task.AimAt(hostage, -1);

            robbersVehicle = await SpawnVehicle(VehicleHash.Kuruma, robberyVehicleLocation);
            robbersVehicle.Mods.PrimaryColor = VehicleColor.MetallicBlack;
            robbersVehicle.Mods.SecondaryColor = VehicleColor.MetallicBlack;
            robbersVehicle.Mods.PearlescentColor = VehicleColor.MetallicBlack;
            robbersVehicle.Mods.HasNeonLight(VehicleNeonLight.Right);
            robbersVehicle.Mods.HasNeonLight(VehicleNeonLight.Left);
            robbersVehicle.Mods.HasNeonLight(VehicleNeonLight.Back);
            robbersVehicle.Mods.HasNeonLight(VehicleNeonLight.Front);
            robbersVehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Right, true);
            robbersVehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Left, true);
            robbersVehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Back, true);
            robbersVehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Front, true);
            robbersVehicle.Mods.NeonLightsColor = System.Drawing.Color.FromArgb(100, 0, 255, 0);
            robbersVehicle.IsEngineRunning = true;
            robbersVehicle.Heading = random.Next(80, 120);
            robbersVehicle.AttachBlip();
            robbersVehicle.LockStatus = VehicleLockStatus.Unlocked;
        }

    }
}

