using Deadpan.Enums.Engine.Components.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WildfrostHopeMod;
using WildfrostHopeMod.Configs;

namespace MoreOverload
{
    public class MoreOverload : WildfrostMod
    {
        public MoreOverload(string modDirectory) : base(modDirectory)
        {
        }


        //-------MOD CONFIGS------\\
        //RINKA HP CONFIG
        [ConfigItem(Variant.RinkaHPDefault, "", "rinkaConfig")]
        [ConfigManagerTitle("Set Rinka's HP")]
        [ConfigManagerDesc("Requires game restart and new run for changes to take effect.")]
        [ConfigOptions(new string[] { "1 HP (default)",
                                      "2 HP" },
        new object[]
        {
            new Variant[]
            {
                Variant.RinkaHPDefault,
                Variant.RinkaHPPlusOne
            }
        }
        )]
        public Variant rinkaHPVariant; 
        
        //HOKA OVERBURN CONFIG
        [ConfigItem(Variant.hokaOverburnDefault, "", "hokaConfig")]
        [ConfigManagerTitle("Set Hoka's Overburn Amount")]
        [ConfigManagerDesc("Requires game restart and new run for changes to take effect.")]
        [ConfigOptions(new string[] { "2 Overburn (default)",
                                      "1 Overburn" },
        new object[]
        {
            new Variant[]
            {
                Variant.hokaOverburnDefault,
                Variant.hokaOverburnLess
            }
        }
        )]
        public Variant hokaOverburnVariant;

        //HOKA HP CONFIG
        private int hokaHPStat;

        [ConfigItem(Variant.hokaHPDefault, "", "hokaHPConfigs")]
        [ConfigManagerTitle("Set Hoka's HP")]
        [ConfigManagerDesc("Requires game restart and new run for changes to take effect.")]
        [ConfigOptions(new string[] { "6 HP (default)",
                                      "5 HP",
                                      "4 HP" },
        new object[]
        {
            new Variant[]
            {
                Variant.hokaHPDefault,
                Variant.hokaHPMinusOne,
                Variant.hokaHPMinusTwo,
            }
        }
        )]
        public Variant hokaHPVariant;

        //AOTARO HP CONFIG
        private int aotaroHPStat;
        private int aotaroCounterStat;
        private int aotaroAttackStat;

        [ConfigItem(Variant.aotaroHPDefault, "", "aotaroHPConfigs")]
        [ConfigManagerTitle("Set Aotaro's HP")]
        [ConfigManagerDesc("Requires game restart and new run for changes to take effect.")]
        [ConfigOptions(new string[] { "8 HP (default)",
                                      "7 HP",
                                      "6 HP" },
        new object[]
        {
            new Variant[]
            {
                Variant.aotaroHPDefault,
                Variant.aotaroHPMinusOne,
                Variant.aotaroHPMinusTwo,
            }
        }
        )]
        public Variant aotaroHPVariant;

        //AOTARO ATTACK
        [ConfigItem(Variant.aotaroAttackDefault, "", "aotaroAttackConfigs")]
        [ConfigManagerTitle("Set Aotaro's Attack")]
        [ConfigManagerDesc("Requires game restart and new run for changes to take effect.")]
        [ConfigOptions(new string[] { "3 Attack (default)",
                                      "2 Attack" },
        new object[]
        {
            new Variant[]
            {
                Variant.aotaroAttackDefault,
                Variant.aotaroAttackLess
            }
        }
        )]
        public Variant aotaroAttackVariant;

        //AOTARO TURN TIMER
        [ConfigItem(Variant.aotaroCounterDefault, "", "aotaroCounterConfigs")]
        [ConfigManagerTitle("Set Aotaro's Counter")]
        [ConfigManagerDesc("Requires game restart and new run for changes to take effect.")]
        [ConfigOptions(new string[] { "4 Turn Timer (default)",
                                      "5 Turn Timer " },
        new object[]
        {
            new Variant[]
            {
                Variant.aotaroCounterDefault,
                Variant.aotaroCounterMore
            }
        }
        )]
        public Variant aotaroCounterVariant;


        //MOD INFO
        public override string GUID => "lakhnish_monster.wildfrost.moreoverload";
        public override string[] Depends => new string[1] { "hope.wildfrost.configs" };
        public override string Title => "Even More Overburn";
        public override string Description => "Adds three new Overburn units to the Shademancer clan and 1 new pet!";

        //CARDS SETUP
        private List<CardDataBuilder> cards;
        private List<StatusEffectDataBuilder> statusEffects;
        private bool preLoaded = false;

        //HELPER METHODS
        private T TryGet<T>(string name) where T : DataFile
        {
            T data;
            if (typeof(StatusEffectData).IsAssignableFrom(typeof(T)))
                data = base.Get<StatusEffectData>(name) as T;
            else
                data = base.Get<T>(name);

            if (data == null)
                throw new Exception($"TryGet Error: Could not find a [{typeof(T).Name}] with the name [{name}] or [{Extensions.PrefixGUID(name, this)}]");

            return data;
        }

        private CardData.StatusEffectStacks SStack(string name, int amount) => new CardData.StatusEffectStacks(Get<StatusEffectData>(name), amount);

        private StatusEffectDataBuilder StatusCopy(string oldName, string newName)
        {
            StatusEffectData data = TryGet<StatusEffectData>(oldName).InstantiateKeepName();
            data.name = GUID + "." + newName;
            StatusEffectDataBuilder builder = data.Edit<StatusEffectData, StatusEffectDataBuilder>();
            builder.Mod = this;
            return builder;
        }

        //CARD & STATUS CREATION
        private void CreateModAssets()
        {
            statusEffects = new List<StatusEffectDataBuilder>();
            //STATUS EFFECTS HERE
            statusEffects.Add(
                StatusCopy("When Snow Applied To Anything Gain Equal Attack To Self", "When Overload Applied To Anything Gain Equal Attack To Self")
                .WithText("Whenever anything is <keyword=overload>'d, gain equal <keyword=attack>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXWhenYAppliedTo)data).whenAppliedTypes = new string[1] { "overload" };
                })
                );

            cards = new List<CardDataBuilder>();
            //CARDS HERE

            cards.Add(
                new CardDataBuilder(this).CreateUnit("Azuryuu", "Azuryuu") //Azul Dragon: アズル竜
                    .SetSprites("", "Azuryuu_BG.png")
                    .IsPet((ChallengeData)null, value: true)
                    .SetStats(5, 1, 3)
                    .AddPool("MagicUnitPool")
                    .SetAttackEffect(SStack("Overload", 2))
                    );

            /*
            cards.Add(
                new CardDataBuilder(this).CreateUnit("Soen", "Soen") //Twin Flames: 双炎
                    .SetSprites("", "Zula_BG.png")
                    .SetStats(6, 0, 3)
                    .AddPool("MagicUnitPool")
                    .SetAttackEffect(SStack("Demonize", 1), SStack("Overload", 1))
                    .SetStartWithEffect(SStack("MultiHit", 1)) //maybe barrage instead. Demonize + overload on aotaro is super strong
                    );
            */


            //BASE AOTARO
            CardDataBuilder aotaroCardBuilder = new CardDataBuilder(this)
            .CreateUnit("Aotaro", "Aotaro") //Blue Axe Man: Aotaro 青男
            .SetSprites("", "Aotaro_BG.png")
            .SetStartWithEffect(SStack("On Hit Equal Overload To Target", 1))
            .AddPool("MagicUnitPool");

            //AOTARO HP
            switch (aotaroHPVariant)
            {
                case Variant.aotaroHPDefault:
                    aotaroHPStat = 8;
                    break;
                case Variant.aotaroHPMinusOne:
                    aotaroHPStat = 7;
                    break;
                case Variant.aotaroHPMinusTwo:
                    aotaroHPStat = 6;
                    break;
            }
            //AOTARO ATTACK
            switch (aotaroAttackVariant)
            {
                case Variant.aotaroAttackDefault:
                    aotaroAttackStat = 3;
                    break;

                case Variant.aotaroAttackLess:
                    aotaroAttackStat = 2;
                    break;
            }
            //AOTARO COUNTER
            switch (aotaroCounterVariant)
            {
                case Variant.aotaroCounterDefault:
                    aotaroCounterStat = 4;
                    break;

                case Variant.aotaroCounterMore:
                    aotaroCounterStat = 5;
                    break;

            }
            aotaroCardBuilder.SetStats(aotaroHPStat, aotaroAttackStat, aotaroCounterStat);
            cards.Add(aotaroCardBuilder);

            //BASE HOKA
            CardDataBuilder hokaCardBuilder = new CardDataBuilder(this)
            .CreateUnit("Hoka", "Hoka") //Arson: 放火
            .SetSprites("", "Zula_BG.png")
            //.SetStats(6, null, 4) //6hp tp survive a 5 damage Grok hit
            .AddPool("MagicUnitPool");
            switch (hokaOverburnVariant)
            {
                case Variant.hokaOverburnDefault:
                    hokaCardBuilder.SetStartWithEffect(SStack("On Turn Apply Overload To Enemies", 2));
                    break;

                case Variant.hokaOverburnLess:
                    hokaCardBuilder.SetStartWithEffect(SStack("On Turn Apply Overload To Enemies", 1));
                    break;

            }
            switch (hokaHPVariant)
            {
                case Variant.hokaHPDefault:
                    hokaHPStat = 6;
                    break;
                case Variant.hokaHPMinusOne:
                    hokaHPStat = 5;
                    break;
                case Variant.hokaHPMinusTwo:
                    hokaHPStat = 4;
                    break;
            }
            hokaCardBuilder.SetStats(hokaHPStat, null, 4);
            cards.Add(hokaCardBuilder);
 
            //BASE RINKA
            CardDataBuilder rinkaCardBuilder = new CardDataBuilder(this)
            .CreateUnit("Rinka", "Rinka") //Will-o'-the Wisp: 燐火
            .SetSprites("", "Zula_BG.png")
            .SubscribeToAfterAllBuildEvent(delegate (CardData data)
            {
                data.startWithEffects = new CardData.StatusEffectStacks[1]
                {
                    SStack("When Overload Applied To Anything Gain Equal Attack To Self",1)
                };
            });
            switch (rinkaHPVariant)
            {
                case Variant.RinkaHPDefault:
                    rinkaCardBuilder.SetStats(1, 1, 4);
                    break;

                case Variant.RinkaHPPlusOne:
                    rinkaCardBuilder.SetStats(2, 1, 4);
                    break;

            }
            cards.Add(rinkaCardBuilder);

            preLoaded = true;
        }


        protected override void Load()
        {
            if (!preLoaded) { CreateModAssets(); }

            ConfigManager.OnModLoaded += HandleModLoaded;
            base.Load();
        }

        protected override void Unload()
        {
            base.Unload();
        }

        private void HandleModLoaded(WildfrostMod mod)
        {
            if (!(mod.GUID != "hope.wildfrost.configs"))
            {
                ConfigSection configSection = ConfigManager.GetConfigSection(this);
                if (configSection != null)
                {
                    configSection.OnConfigChanged += HandleConfigChange;
                }
            }
        }

        private void HandleConfigChange(ConfigItem item, object value)
        {
            Debug.Log("[MoreOverload] config changed!!");
        }


        public override List<T> AddAssets<T, Y>()
        {
            var typeName = typeof(T).Name;
            switch (typeName)
            {
                case nameof(CardDataBuilder):
                    return cards.Cast<T>().ToList();
                case nameof(StatusEffectDataBuilder):
                    return statusEffects.Cast<T>().ToList();
                default:
                    return null;
            }
        }


    }
}