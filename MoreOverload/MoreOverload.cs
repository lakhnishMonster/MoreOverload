using Deadpan.Enums.Engine.Components.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MoreOverload
{
    public class MoreOverload : WildfrostMod
    {
        public MoreOverload(string modDirectory) : base(modDirectory)
        {
        }

        //MOD INFO
        public override string GUID => "lakhnish_monster.wildfrost.moreoverload";

        public override string[] Depends => new string[0];

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

            //----------SHADEMANCER COMPANIONS-------\\
            //THIS NEEDS SOMETHING MORE??? Aotaro is always a clear pick over this.
            //Also need to keep in mind what happens in the chance there are no overburn units.
            //This looks like old school Dimona too, which is probably why it feels bad.
            //Need to also make sure it's not too close to the pet or I could just scrap it all together.
            /*
            cards.Add(
                new CardDataBuilder(this).CreateUnit("Soen", "Soen") //Twin Flames: 双炎
                    .SetSprites("", "Zula_BG.png")
                    .SetStats(6, 0, 3)
                    .AddPool("MagicUnitPool")
                    .SetAttackEffect(SStack("Demonize", 1), SStack("Overload", 1))
                    .SetStartWithEffect(SStack("MultiHit",1)) //maybe barrage instead. Demonize + overload on aotaro is super strong
                    );
            */  
            //I love this unit, but it's also really strong. Maybe needs a 5 turn timer? I like the HP stat. 1 less attack might also be better?
            cards.Add(
                new CardDataBuilder(this).CreateUnit("Aotaro", "Aotaro")//Blue Axe Man: Aotaro 青男
                    .SetSprites("", "Aotaro_BG.png")
                    .SetStats(8, 3, 4)
                    .AddPool("MagicUnitPool")
                    .SetStartWithEffect(SStack("On Hit Equal Overload To Target", 1))
                    );

            //Nice alternative to spike for the 1st 3 fights, can help get more multikills in fights 2 & 3 compared to Spike.
            //Doesn't fair too well afterwards and I don't see it being a carry for end game like Booshu, but I like it.
            cards.Add(
                new CardDataBuilder(this).CreateUnit("Azuryuu", "Azuryuu") //Azul Dragon: アズル竜
                    .SetSprites("", "Azuryuu_BG.png")
                    .IsPet((ChallengeData)null, value: true)
                    .SetStats(5, 1, 3)
                    .AddPool("MagicUnitPool")
                    .SetAttackEffect(SStack("Overload", 2))
                    );
            //1 Overload seems too little, 2 seems good but when lumin'd, it scales too fast w/ Hoka.
            //Needs more testing
            cards.Add(
                new CardDataBuilder(this).CreateUnit("Hoka", "Hoka") //Arson: 放火
                    .SetSprites("", "Zula_BG.png")
                    .SetStats(6, null, 4) //6hp tp survive a 5 damage Grok hit
                    .AddPool("MagicUnitPool")
                    .SetStartWithEffect(SStack("On Turn Apply Overload To Enemies", 2))
                    );
            //Yuki alternate, but scales too fast with Hoka, probably because there isn't an overload guard like there is for Snow.
            //Needs more testing
            cards.Add(
                new CardDataBuilder(this).CreateUnit("Rinka", "Rinka") //Will-o'-the Wisp: 燐火
                    .SetSprites("", "Zula_BG.png")
                    .SetStats(1, 1, 4)
                    .AddPool("MagicUnitPool")
                    .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                    {
                        data.startWithEffects = new CardData.StatusEffectStacks[1]
                        {
                           SStack("When Overload Applied To Anything Gain Equal Attack To Self",1)
                        };
                    }));
          
            preLoaded = true;
        }
        

        protected override void Load()
        {
            if (!preLoaded) { CreateModAssets(); }
            base.Load();
        }

        protected override void Unload()
        {
            base.Unload();
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