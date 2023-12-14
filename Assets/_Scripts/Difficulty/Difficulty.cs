using System.Collections;
using System.Collections.Generic;
using Boomlagoon.JSON;
using System.IO;

namespace dm {

class BpmEvent {
   private float timeInBeat;
   private float newBPM;

   public float TimeInBeat { get; set;}
   public float NewBPMValue { get; set;}
}

class RotationEvent {

}


class Difficulty {
        static public List<ColorNote> ParseJson(string path)
        {

            JSONObject json = JSONObject.Parse(File.ReadAllText(path));
            if (json.ContainsKey("version")) {
            return ParseJsonV3(path);
            }
            else if (json.ContainsKey("_version"))
            {
                return ParseJsonV2(path);
            }
            return null;
        }

    static private List<ColorNote> ParseJsonV2(string path) {
        List<ColorNote> list= new List<ColorNote>();
        JSONObject json = JSONObject.Parse( File.ReadAllText(path));
        // ColorNotes
        var notes = json.GetArray("_notes");
        foreach (var note in notes)
        {
            var n = new ColorNote
            {
                NoteColorType = (NoteColorType )note.Obj.GetNumber("_type"),
                CutDirection = (CutDirection)note.Obj.GetNumber("_cutDirection"),
                xColumn= (int)note.Obj.GetNumber("_lineIndex"),
                yLayer= (int)note.Obj.GetNumber("_lineLayer"),
                TimeInBeat = (note.Obj.GetNumber("_time"))
            };

            list.Add(n);
        }
        return list;
    }

    static private List<ColorNote> ParseJsonV3(string path) {
        List<ColorNote> list = new List<ColorNote>();
        var jsonString = File.ReadAllText(path);
        JSONObject json = JSONObject.Parse(jsonString);
        // ColorNotes
        var notes = json.GetArray("colorNotes");
        foreach (var note in notes)
        {
            var n = new ColorNote
            {
                TimeInBeat = note.Obj.GetNumber("b"),
                xColumn = (int)note.Obj.GetNumber("x"),
                yLayer = (int)note.Obj.GetNumber("y"),
                NoteColorType = (NoteColorType)note.Obj.GetNumber("c"),
                CutDirection = (CutDirection)note.Obj.GetNumber("d"),
            };

            list.Add(n);
        }
        return list;
    }
}


}