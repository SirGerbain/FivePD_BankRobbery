using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API;
using System.Security.Cryptography.X509Certificates;

namespace SirGerbain_BankRobbery
{
    [CalloutProperties("SG_BankRobbery", "sirGerbain", "1.0.0")]
    public class SirGerbain_BankRobbery : FivePD.API.Callout
    {

        public SirGerbain_BankRobbery()
        {

            InitInfo(spawnPlace);
            ShortName = "";
            CalloutDescription = "";
            ResponseCode = 3;
            StartDistance = 500f;

        }

        public async override Task OnAccept()
        {
            InitBlip();
            UpdateData();

        }

        public async override void OnStart(Ped player)
        {
            base.OnStart(player);

        }

    }
}

