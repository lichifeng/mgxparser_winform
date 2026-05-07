using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using MgxNative;

namespace mgxparser
{
    public class GameInfo
    {
        public string parser { get; set; }
        public string md5 { get; set; }
        public string filename { get; set; }
        public long filesize { get; set; }
        public long lastmod { get; set; }
        public string guid { get; set; }
        public int verlog { get; set; }
        public string ver { get; set; }
        public string verraw { get; set; }
        public double versave { get; set; }
        public double? versave2 { get; set; }
        public double verscenario { get; set; }
        public bool include_ai { get; set; }
        public int speed_raw { get; set; }
        public string speed { get; set; }
        public int recorder { get; set; }
        public int totalplayers { get; set; }
        public int mapsize_raw { get; set; }
        public string mapsize { get; set; }
        public int revealmap_raw { get; set; }
        public string revealmap { get; set; }
        public int mapx { get; set; }
        public int mapy { get; set; }
        public bool fogofwar { get; set; }
        public bool instantbuild { get; set; }
        public bool enablecheats { get; set; }
        public int restoretime { get; set; }
        public bool ismultiplayer { get; set; }
        public bool isconquest { get; set; }
        public int relics2win { get; set; }
        public int explored2win { get; set; }
        public bool anyorall { get; set; }
        public int victorytype_raw { get; set; }
        public string victorytype { get; set; }
        public int score2win { get; set; }
        public int time2win_raw { get; set; }
        public string time2win { get; set; }
        public string scenariofilename { get; set; }
        public string instructions { get; set; }
        public long duration { get; set; }
        public List<object> chat { get; set; }
        public int mapid { get; set; }
        public string mapname { get; set; }
        public int difficulty_raw { get; set; }
        public string difficulty { get; set; }
        public bool lockteams { get; set; }
        public int poplimit { get; set; }
        public int gametype_raw { get; set; }
        public string gametype { get; set; }
        public bool lockdiplomacy { get; set; }
        public bool haswinner { get; set; }
        public List<int> matchup { get; set; }
        public List<List<int>> teams { get; set; }
        public List<PlayerInfo> players { get; set; }
    }

    public class PlayerInfo
    {
        public int slot { get; set; }
        public int index { get; set; }
        public int playertype { get; set; }
        public string name { get; set; }
        public int? teamid { get; set; }
        public bool? ismainop { get; set; }
        public double? initx { get; set; }
        public double? inity { get; set; }
        public int? civ_raw { get; set; }
        public string civ { get; set; }
        public int? colorid { get; set; }
        public bool? disconnected { get; set; }
        public int? resigned { get; set; }
        public int? feudaltime { get; set; }
        public int? castletime { get; set; }
        public int? imperialtime { get; set; }
        public double? initage_raw { get; set; }
        public string initage { get; set; }
        public double? initfood { get; set; }
        public double? initwood { get; set; }
        public double? initstone { get; set; }
        public double? initgold { get; set; }
        public double? initpop { get; set; }
        public double? initcivilian { get; set; }
        public double? initmilitary { get; set; }
        public string modversion { get; set; }
        public bool? winner { get; set; }
    }

    public static class MgxParser
    {
        public static GameInfo ParseGameInfo(string filePath)
        {
            string json = Parser.ParseFile(filePath);
            if (json == null)
                throw new Exception("解析录像文件失败，文件可能已损坏或格式不支持");

            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<GameInfo>(json);
        }
    }
}
