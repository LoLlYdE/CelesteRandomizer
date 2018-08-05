using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.CelesteRandomizer {
    public class RandomizerModule : EverestModule {

        public static RandomizerModule Instance;

        private List<LevelData> listOfLevels;

        public override Type SettingsType => typeof(RandomizerModuleSettings);

        public static RandomizerModuleSettings Settings => (RandomizerModuleSettings)Instance._Settings;

        public RandomizerModule() {
            Instance = this;
        }

        public override void Load() {
            On.Celeste.Level.TransitionTo += randomizeTransition;
            On.Celeste.Level.Begin += LoadLevelDatas;
        }

        public override void Unload() {
            On.Celeste.Level.TransitionTo -= randomizeTransition;
            On.Celeste.Level.Begin -= LoadLevelDatas;
        }

        private void randomizeTransition(On.Celeste.Level.orig_TransitionTo orig, Level self, LevelData next, Vector2 direction) {
            if(!Settings.Enabled) {
                orig(self, next, direction);
            }
            else {
                LevelData nextRandom = listOfLevels[0];
                Logger.Log("Randomizer", "# levels left: " + listOfLevels.Count());
                listOfLevels.Remove(nextRandom);
                setPlayerPos(self.Entities.FindFirst<Player>(), nextRandom.Spawns[0]);



                orig(self, nextRandom, direction);
            }

        }

        private void setPlayerPos(Player player, Vector2 pos) {
            player.Speed = Vector2.Zero;
            player.Position = pos;
        }

        private void LoadLevelDatas(On.Celeste.Level.orig_Begin orig, Level self) {
            if(!Settings.EndScreenAsLast) listOfLevels = randomizeOrder(new List<LevelData>(self.Session.MapData.Levels));
            if(Settings.EndScreenAsLast) listOfLevels = pseudoRandomizeOrder(new List<LevelData>(self.Session.MapData.Levels));
            orig(self);
        }

        private List<LevelData> randomizeOrder(List<LevelData> levels) {
            List<LevelData> toReturn = new List<LevelData>();
            Random rng = new Random();
            while (levels.Count != 0) {
                int next = rng.Next(levels.Count);
                toReturn.Add(levels[next]);
                levels.RemoveAt(next);
            }
            return toReturn;
        }


        private List<LevelData> pseudoRandomizeOrder(List<LevelData> levels) {
            List<LevelData> toReturn = new List<LevelData>();
            LevelData last = null;

            foreach (LevelData item in levels) {
                if (item.Name.ToLower().EndsWith("_end")) {
                    last = item;
                    levels.Remove(item);
                    break;
                }
            }

            Random rng = new Random();
            while (levels.Count != 0) {
                int next = rng.Next(levels.Count);
                Logger.Log("Randomizer", "Next: " + next);
                Logger.Log("Randomizer", "Total: " + levels.Count());
                toReturn.Add(levels[next]);
                levels.RemoveAt(next);
            }

            toReturn.Add(last);

            return toReturn;
        }
    }
}