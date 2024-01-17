
namespace dm {

public enum NoteColorType {
    RED = 0,
    BLUE = 1,
}

public enum CutDirection {
    UP = 0,
    DOWN = 1,
    LEFT = 2,
    RIGHT = 3,
    UPLEFT= 4,
    UPRIGHT = 5,
    DOWNLEFT = 6,
    DOWNRIGHT = 7,
    ANY = 8
}


class ColorNote {
    // The time, in beats, where this object reaches the player.
    public double TimeInBeat { get; set; }
    // An integer number, from 0 to 3, which represents the column where this note is located. The far left column is located at index 0, and increases to the far right column located at index 3.
    public int xColumn { get; set; }
    // An integer number, from 0 to 2, which represents the layer where this note is located. The bottommost layer is located at layer 0, and increases to the topmost layer located at index 2.
    public int yLayer { get; set; }
    // An integer which represents the color of the note.
    public NoteColorType NoteColorType { get; set; }
    // This indicates the cut direction for the note.
    public CutDirection CutDirection { get; set; }

    // An integer number which represents the additional counter-clockwise angle offset applied to the note's cut direction in degrees. This has no effect on angles created due to snapping (e.g. dot stack, slanted windows).
    public double angleOffset { get; set; }
}

}