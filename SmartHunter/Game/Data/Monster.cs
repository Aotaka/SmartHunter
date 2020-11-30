using System;
using System.Collections.ObjectModel;
using System.Linq;
using SmartHunter.Core;
using SmartHunter.Core.Data;
using SmartHunter.Game.Config;
using SmartHunter.Game.Helpers;

namespace SmartHunter.Game.Data
{
    public enum MonsterCrown
    {
        None,
        Mini,
        Silver,
        Gold
    }

    public class Monster : TimedVisibility
    {
        public ulong Address { get; private set; }

        string m_Id;
        public string Id
        {
            get { return m_Id; }
            set
            {
                if (SetProperty(ref m_Id, value))
                {
                    NotifyPropertyChanged(nameof(IsVisible));
                    UpdateLocalization();
                }
            }
        }

        public bool isElder
        {
            get
            {
                bool elder = false;

                MonsterConfig config = null;
                if (ConfigHelper.MonsterData.Values.Monsters.TryGetValue(Id, out config))
                {
                    elder = config.isElder;
                }

                return elder;
            }
        }

        public string Name
        {
            get
            {
                return LocalizationHelper.GetMonsterName(Id);
            }
        }

        float m_SizeScale;
        public float SizeScale
        {
            get { return m_SizeScale; }
            set
            {
                if (SetProperty(ref m_SizeScale, value))
                {
                    NotifyPropertyChanged(nameof(ModifiedSizeScale));
                    NotifyPropertyChanged(nameof(Size));
                    NotifyPropertyChanged(nameof(Crown));
                }
            }
        }

        float m_ScaleModifier;

        public float ScaleModifier
        {
            get { return m_ScaleModifier; }
            set
            {
                if (SetProperty(ref m_ScaleModifier, value))
                {
                    NotifyPropertyChanged(nameof(ModifiedSizeScale));
                    NotifyPropertyChanged(nameof(Size));
                    NotifyPropertyChanged(nameof(Crown));
                }
            }
        }

        public float ModifiedSizeScale
        {
            get
            {
                return SizeScale / ScaleModifier;
            }
        }

        public float Size
        {
            get
            {
                float size = 0;

                MonsterConfig config = null;
                if (ConfigHelper.MonsterData.Values.Monsters.TryGetValue(Id, out config))
                {
                    size = config.BaseSize * ModifiedSizeScale;
                }

                return size;
            }
        }

        public MonsterCrown Crown
        {
            get
            {
                MonsterCrown crown = MonsterCrown.None;

                MonsterConfig config = null;
                if (ConfigHelper.MonsterData.Values.Monsters.TryGetValue(Id, out config) && config.Crowns != null)
                {
                    float modifiedSizeScale = float.Parse(ModifiedSizeScale.ToString("0.00"));

                    if (modifiedSizeScale <= config.Crowns.Mini)
                    {
                        crown = MonsterCrown.Mini;
                    }
                    else if (modifiedSizeScale >= config.Crowns.Gold)
                    {
                        crown = MonsterCrown.Gold;
                    }
                    else if (modifiedSizeScale >= config.Crowns.Silver)
                    {
                        crown = MonsterCrown.Silver;
                    }
                }
                return crown;
            }
        }

        public Progress Health { get; private set; }
        public ObservableCollection<MonsterPart> Parts { get; private set; }
        public ObservableCollection<MonsterPartSoften> PartSoftens { get; private set; }
        public ObservableCollection<MonsterStatusEffect> StatusEffects { get; private set; }

        public bool IsVisible
        {
            get
            {
                return IsIncluded(Id) && IsTimeVisible(ConfigHelper.Main.Values.Overlay.MonsterWidget.ShowUnchangedMonsters, ConfigHelper.Main.Values.Overlay.MonsterWidget.HideMonstersAfterSeconds);
            }
        }

        public Monster(ulong address, string id, float maxHealth, float currentHealth, float sizeScale, float scaleModifier)
        {
            Address = address;
            m_Id = id;
            Health = new Progress(maxHealth, currentHealth);
            m_SizeScale = sizeScale;
            m_ScaleModifier = scaleModifier;

            Parts = new ObservableCollection<MonsterPart>();
            PartSoftens = new ObservableCollection<MonsterPartSoften>();
            StatusEffects = new ObservableCollection<MonsterStatusEffect>();
        }

        public MonsterPart UpdateAndGetPart(ulong address, bool isRemovable, float maxHealth, float currentHealth, int timesBrokenCount)
        {
            MonsterPart part = Parts.SingleOrDefault(collectionPart => collectionPart.Address == address);
            if (part != null)
            {
                if (!float.IsNaN(currentHealth / maxHealth))
                {
                    part.IsRemovable = isRemovable;
                    part.Health.Max = maxHealth;
                    part.Health.Current = currentHealth;
                    part.TimesBrokenCount = timesBrokenCount;
                }
            }
            else
            {
                part = new MonsterPart(this, address, isRemovable, maxHealth, currentHealth, timesBrokenCount);
                part.Changed += PartOrStatusEffect_Changed;

                Parts.Add(part);
            }

            part.NotifyPropertyChanged(nameof(MonsterPart.IsVisible));

            return part;
        }

        public MonsterPartSoften UpdateAndGetPartSoften(ulong address, float maxTime, float currentTime, uint timesCount, uint partID)
        {
            MonsterPartSoften partSoften = PartSoftens.SingleOrDefault(collectionPartSoften => collectionPartSoften.Address == address);
            if (partSoften != null)
            {
                partSoften.Time.Max = maxTime;
                partSoften.Time.Current = currentTime;
                partSoften.TimesCount = timesCount;
                if (partSoften.PartID != partID)
                {
                    partSoften.PartID = partID;
                    partSoften.NotifyPropertyChanged(nameof(MonsterPartSoften.Name));
                }
            }
            else
            {
                partSoften = new MonsterPartSoften(this, address, maxTime, currentTime, timesCount, partID);
                partSoften.Changed += PartOrStatusEffect_Changed;

                PartSoftens.Add(partSoften);
            }

            partSoften.NotifyPropertyChanged(nameof(MonsterPartSoften.IsVisible));

            return partSoften;
        }

        public MonsterStatusEffect UpdateAndGetStatusEffect(ulong address, int index, float maxBuildup, float currentBuildup, float maxDuration, float currentDuration, int timesActivatedCount)
        {
            MonsterStatusEffect statusEffect = StatusEffects.SingleOrDefault(collectionStatusEffect => collectionStatusEffect.Index == index); // TODO: check address???

            if (statusEffect != null)
            {
                if (!float.IsNaN(currentDuration / maxDuration))
                {
                    statusEffect.Duration.Max = maxDuration;
                    statusEffect.Duration.Current = currentDuration;
                }
                if (!float.IsNaN(currentBuildup / maxBuildup))
                {
                    statusEffect.Buildup.Max = maxBuildup;
                    statusEffect.Buildup.Current = currentBuildup;
                }
                statusEffect.TimesActivatedCount = timesActivatedCount;
            }
            else
            {
                statusEffect = new MonsterStatusEffect(this, address, index, maxBuildup, currentBuildup, maxDuration, currentDuration, timesActivatedCount);
                statusEffect.Changed += PartOrStatusEffect_Changed;

                StatusEffects.Add(statusEffect);
            }

            statusEffect.NotifyPropertyChanged(nameof(MonsterStatusEffect.IsVisible));

            return statusEffect;
        }

        public void UpdateLocalization()
        {
            NotifyPropertyChanged(nameof(Name));

            foreach (var part in Parts)
            {
                part.NotifyPropertyChanged(nameof(MonsterPart.Name));
            }
            foreach (var statusEffect in StatusEffects)
            {
                statusEffect.NotifyPropertyChanged(nameof(MonsterStatusEffect.Name));
            }
            foreach (var partSoften in PartSoftens)
            {
                partSoften.NotifyPropertyChanged(nameof(MonsterPartSoften.Name));
            }
        }

        public static bool IsIncluded(string monsterId)
        {
            return ConfigHelper.Main.Values.Overlay.MonsterWidget.MatchIncludeMonsterIdRegex(monsterId);
        }

        private void PartOrStatusEffect_Changed(object sender, GenericEventArgs<DateTimeOffset> e)
        {
            UpdateLastChangedTime();
        }
    }
}
