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

        private int counter = 0;

        private Vector2 lastPos = Vector2.Zero;

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
                Player ply = self.Entities.FindFirst<Player>();

                bool eligible = (computeDistance(lastPos, ply.Position) > 20 || self.Session.LevelData.Spawns.Count == 1 || (++counter > 300));
                if (lastPos == Vector2.Zero)
                    eligible = true;

                if (!eligible) {
                    setPlayerPos(ply, lastPos);
                } else {
                    counter = 0;
                    LevelData nextRandom = listOfLevels[0];
                    listOfLevels.Remove(nextRandom);
                    Vector2 nearestSpawn = findNearestSpawn(nextRandom.Spawns, ply.Position);
                    setPlayerPos(ply, nearestSpawn);
                    lastPos = nearestSpawn;
                    orig(self, nextRandom, direction);
                }
            }

        }

        private Vector2 findNearestSpawn(List<Vector2> spawns, Vector2 playerPos) {
            Vector2 toReturn = spawns[0];
            float distance = computeDistance(toReturn, playerPos);

            foreach (Vector2 item in spawns) {
                float newDist = computeDistance(playerPos, item);
                if(newDist < distance) {
                    distance = newDist;
                    toReturn = item;
                }
            }
            return toReturn;
        }

        private float computeDistance(Vector2 pos1, Vector2 pos2) {
            //sqrt((x1-x2)^2 + (y1-y2)^2)
            return (float) (Math.Sqrt(Math.Pow(pos1.X - pos2.X, 2) + Math.Pow(pos1.Y - pos2.Y, 2)));
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

            bool found = false;

            foreach (LevelData item in levels) {
                if (item.Name.ToLower().EndsWith("_end")) {
                    last = item;
                    levels.Remove(item);
                    found = true;
                    break;
                }
            }

            Random rng = new Random();
            while (levels.Count != 0) {
                int next = rng.Next(levels.Count);
                toReturn.Add(levels[next]);
                levels.RemoveAt(next);
            }

            if(found) toReturn.Add(last);

            return toReturn;
        }
    }
}