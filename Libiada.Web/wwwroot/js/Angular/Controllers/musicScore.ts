import { MapModelFromJson } from "functions";
import type { MIDI } from "viewDataTypes";

declare const MIDI: MIDI;

interface Pitch {
    Alter: number;
    Instrument: number;
    MidiNumber: number;
    Octave: number;
    Step: number;
}

interface NoteDuration {
    Denominator: number;
    Numerator: number;
    OriginalDenominator: number;
    OriginalNumerator: number;
    OriginalValue: number;
    Value: number;
}

interface Note {
    Id: number;
    Priority: number;
    Tie: number;
    Triplet: boolean;
    Pitches: Pitch[];
    Duration: NoteDuration;
}

interface Line {
    line: number;
    alter: number;
    note: string;
}

interface KeySelection extends d3.Selection<SVGRectElement, null, SVGGElement, null> { }

interface NoteSelection extends d3.Selection<SVGGElement, null, HTMLElement, null> { }

/**
 * Interface for the data object passed from the server
 */
interface MusicScoreData {
    data: {
        musicNotes: Note[];
        measures: Note[][];
        sequentialTransfer: boolean;
    };
}

/**
 * Interface for the angular controller's scope
 */
interface MusicScoreScope extends ng.IScope, MusicScoreData {
    isPlaying: boolean;
    margin: number;
    verticalInterval: number;

    onLoad: () => void;
    play: () => void;
    stopAll: () => void;
}

/**
 * Angular controller for music sequence display
 */
export default function MusicScoreController(data: MusicScoreData): void {
    function musicScore($scope: MusicScoreScope): void {
        MapModelFromJson($scope, data);

        $scope.margin = 40;
        $scope.verticalInterval = (140 - $scope.margin * 2) / 14;
        let notation: d3.Selection<SVGSVGElement, null, HTMLElement, null>;
        let visualization: d3.Selection<SVGSVGElement, null, HTMLElement, null>;

        // MIDI player variables
        let timeouts: ReturnType<typeof setTimeout>[] = [];
        let chordIndex: number;

        // MIDI-player initialization
        $scope.onLoad = (): void => {
            MIDI.loadPlugin({
                soundfontUrl: "../../js/",
                instrument: "acoustic_grand_piano",
                onprogress: (state: string, progress: number): void => {
                    console.log(state, progress);
                },
                onsuccess: (): void => {
                    console.log("MIDI loaded successfully");
                }
            });
        };

        const player = {
            timeline: 0,
            velocity: 127,

            play: function (note: Note, minOctave: number): void {
                const chord: number[] = [];
                const duration: number = note.Duration.Value;

                for (let i: number = 0; i < note.Pitches.length; i++) {
                    chord[i] = note.Pitches[i].MidiNumber;
                }

                MIDI.chordOn(0, chord, this.velocity, this.timeline);
                timeouts.push(setTimeout(() => this.keyOn(note, minOctave), this.timeline * 1000));
                timeouts.push(setTimeout(() => this.noteOn(), this.timeline * 1000));
                MIDI.chordOff(0, chord, this.velocity, this.timeline + duration);
                timeouts.push(setTimeout(() => this.keyOff(), (this.timeline + duration) * 900));
                timeouts.push(setTimeout(() => this.noteOff(), (this.timeline + duration) * 900));

                this.timeline += duration;
            },
            keyOn: function (note: Note, minOctave: number): void {
                for (let i: number = 0; i < note.Pitches.length; i++) {
                    const pitch: Pitch = note.Pitches[i];
                    const step: number = pitch.MidiNumber % 12;
                    const currentOctave: number = pitch.Octave > minOctave ? 1 : 0;
                    const key: KeySelection = d3.select<HTMLElement, null>(`#visualization`)
                        .select<SVGGElement>(`.key_${step + currentOctave * 12}`)
                        .selectAll<SVGRectElement, null>("rect");
                    key.style("fill", "blue")
                        .style("fill-opacity", 1);
                }
            },
            keyOff: function (): void {
                for (let i: number = 0; i < 24; i++) {
                    const key: KeySelection = d3.select<HTMLElement, null>(`#visualization`)
                        .select<SVGGElement>(`.key_${i}`)
                        .selectAll<SVGRectElement, null>("rect");
                    if (getLine(i % 12).alter === 0) {
                        key.style("fill", "none");
                    } else if (getLine(i % 12).alter === 1) {
                        key.style("fill", "black");
                    }
                }
            },
            noteOn: function (): void {
                const chord: NoteSelection = d3.select<SVGSVGElement, null>(`#notation`)
                    .select<SVGGElement>(`.chord_${chordIndex}`);
                chord.selectAll("ellipse")
                    .style("fill", "blue");
                chord.selectAll("rect")
                    .style("fill", "blue");
                chord.selectAll("text")
                    .style("fill", "blue");
                chord.selectAll("line")
                    .style("stroke", "blue");
                chord.selectAll(".white")
                    .style("fill", "white");
                chord.selectAll(".blackline")
                    .style("stroke", "black");
            },
            noteOff: function (): void {
                const chord: NoteSelection = d3.select<SVGSVGElement, null>(`#notation`)
                    .select<SVGGElement>(`.chord_${chordIndex}`);
                chord.selectAll("ellipse")
                    .style("fill", "black");
                chord.selectAll("rect")
                    .style("fill", "black");
                chord.selectAll("text")
                    .style("fill", "black");
                chord.selectAll("line")
                    .style("stroke", "black");
                chord.selectAll(".white")
                    .style("fill", "white");
                chordIndex++;
            }
        };

        $scope.play = (): void => {
            $scope.isPlaying = true;
            MIDI.setVolume(0, 80);
            player.timeline = 0;

            const notes: Note[] = $scope.data.musicNotes;
            let min: number = 9;
            chordIndex = 0;

            const totalDuration: number = notes.reduce(
                (sum: number, note: Note): number => sum + note.Duration.Value,
                0
            );

            notes.forEach((note: Note): void => {
                player.play(note, min);
            });

            timeouts.push(setTimeout((): void => {
                $scope.isPlaying = false;
                $scope.$apply();
            }, totalDuration * 1000));
        };

        $scope.stopAll = (): void => {
            console.log("Stopping all scheduled playbacks...");
            timeouts.forEach((timeoutID: ReturnType<typeof setTimeout>): void => clearTimeout(timeoutID));
            timeouts = [];
            $scope.isPlaying = false;
            $scope.$apply();
        };

        function getLine(step: number): Line {
            switch (step) {
                case 0: return { line: 0, alter: 0, note: "C" };
                case 1: return { line: 0, alter: 1, note: "C#" };
                case 2: return { line: 1, alter: 0, note: "D" };
                case 3: return { line: 1, alter: 1, note: "D#" };
                case 4: return { line: 2, alter: 0, note: "E" };
                case 5: return { line: 3, alter: 0, note: "F" };
                case 6: return { line: 3, alter: 1, note: "F#" };
                case 7: return { line: 4, alter: 0, note: "G" };
                case 8: return { line: 4, alter: 1, note: "G#" };
                case 9: return { line: 5, alter: 0, note: "A" };
                case 10: return { line: 5, alter: 1, note: "A#" };
                case 11: return { line: 6, alter: 0, note: "B" };
                default: return { line: -1, alter: -1, note: "-1" };
            }
        }

        function drawKeyboard(): void {
            let rectPosX: number = 0;
            for (let i: number = 0; i < 24; i++) {
                if (getLine(i % 12).alter === 0) {
                    visualization.append("g")
                        .attr("class", `key_${i}`)
                        .append("rect")
                        .attr("width", 25)
                        .attr("height", 100)
                        .attr("x", rectPosX)
                        .style("fill", "none")
                        .style("stroke-width", 1)
                        .style("stroke", "black");
                    rectPosX += 25;
                }
            }
            for (let i: number = 0; i < 24; i++) {
                if (getLine(i % 12).alter === 1) {
                    visualization.append("g")
                        .attr("class", `key_${i}`)
                        .append("rect")
                        .attr("width", 17)
                        .attr("height", 70)
                        .attr("x", 350 / 24 + (i - 1) * 350 / 24)
                        .style("fill", "black")
                        .style("stroke-width", 1)
                        .style("stroke", "black");
                }
            }
        }

        // draws notation staff
        function drawStaff(yOffset: number): void {
            for (let i: number = 3; i < 12; i++) {
                if (i % 2 !== 0) {
                    notation.append("line")
                        .attr("x1", 0)
                        .attr("y1", yOffset + $scope.margin + i * $scope.verticalInterval)
                        .attr("x2", 1800)
                        .attr("y2", yOffset + $scope.margin + i * $scope.verticalInterval)
                        .style("stroke", "#000")
                        .style("stroke-width", 2);
                }
            }
            notation.append("line")
                .attr("x1", 0)
                .attr("y1", yOffset + $scope.margin + 3 * $scope.verticalInterval)
                .attr("x2", 0)
                .attr("y2", yOffset + $scope.margin + 11 * $scope.verticalInterval)
                .style("stroke", "#000")
                .style("stroke-width", 5);
            notation.append("line")
                .attr("x1", 1800)
                .attr("y1", yOffset + $scope.margin + 3 * $scope.verticalInterval)
                .attr("x2", 1800)
                .attr("y2", yOffset + $scope.margin + 11 * $scope.verticalInterval)
                .style("stroke", "#000")
                .style("stroke-width", 5);
        }

        // draws note
        function drawNote(
            group: d3.Selection<SVGGElement, null, HTMLElement, null>,
            note: Note,
            octave: number,
            iterator: number,
            x: number,
            y: number
        ): void {
            const step: number = note.Pitches[iterator].MidiNumber % 12;
            if (octave === 0 && (step === 0 || step === 1)) {
                group.append("line")
                    .attr("class", "blackline")
                    .attr("x1", x - 15)
                    .attr("y1", y)
                    .attr("x2", x + 15)
                    .attr("y2", y)
                    .style("stroke", "#000")
                    .style("stroke-width", 2);
            }
            if (octave === 1) {
                switch (step) {
                    case 9:
                    case 10:
                        group.append("line")
                            .attr("class", "blackline")
                            .attr("x1", x - 15)
                            .attr("y1", y)
                            .attr("x2", x + 15)
                            .attr("y2", y)
                            .style("stroke", "#000")
                            .style("stroke-width", 2);
                        break;
                    case 11:
                        group.append("line")
                            .attr("class", "blackline")
                            .attr("x1", x - 15)
                            .attr("y1", y + 5)
                            .attr("x2", x + 15)
                            .attr("y2", y + 5)
                            .style("stroke", "#000")
                            .style("stroke-width", 2);
                        break;
                    default: break;
                }
            }
            if (note.Duration.Denominator === 1 || note.Duration.Denominator === 2) {
                group.append("ellipse")
                    .attr("rx", 7)
                    .attr("ry", 5)
                    .attr("cx", x)
                    .attr("cy", y)
                    .attr("transform", "rotate(350 " + x + " " + y + ")")
                    .style("fill-opacity", 1)
                    .style("fill", "black");
                group.append("ellipse")
                    .attr("class", "white")
                    .attr("rx", 4)
                    .attr("ry", 2)
                    .attr("cx", x)
                    .attr("cy", y)
                    .attr("transform", "rotate(330 " + x + " " + y + ")")
                    .style("fill-opacity", 1)
                    .style("fill", "white");
            }
            else {
                group.append("ellipse")
                    .attr("rx", 7)
                    .attr("ry", 5)
                    .attr("cx", x)
                    .attr("cy", y)
                    .attr("transform", "rotate(350 " + x + " " + y + ")")
                    .style("fill-opacity", 1)
                    .style("fill", "black");
            }
            if (getLine(step).alter === 1) {
                group.append("text")
                    .attr("x", x - 25)
                    .attr("y", y + 6)
                    .attr("font-size", "25px")
                    .text("\u266F");
            }
        }

        // draws pause
        function drawPause(
            group: d3.Selection<SVGGElement, null, HTMLElement, null>,
            note: Note,
            x: number,
            yOffset: number
        ): void {
            switch (note.Duration.Denominator) {
                case 1:
                    group.append("rect")
                        .attr("width", 10)
                        .attr("height", 5)
                        .attr("x", x)
                        .attr("y", yOffset + $scope.margin + 6 * $scope.verticalInterval)
                        .style("fill", "black");
                    break;
                case 2:
                    group.append("rect")
                        .attr("width", 10)
                        .attr("height", 5)
                        .attr("x", x)
                        .attr("y", yOffset + $scope.margin + 5 * $scope.verticalInterval)
                        .style("fill", "black");
                    break;
                case 4:
                    group.append("text")
                        .attr("x", x)
                        .attr("y", yOffset + $scope.margin + 10 * $scope.verticalInterval)
                        .attr("font-size", "35px")
                        .text(signFromCharCode(0x1D13D));
                    break;
                case 8:
                    group.append("text")
                        .attr("x", x)
                        .attr("y", yOffset + $scope.margin + 11 * $scope.verticalInterval)
                        .attr("font-size", "35px")
                        .text(signFromCharCode(0x1D13E));
                    break;
                case 16:
                    group.append("text")
                        .attr("x", x)
                        .attr("y", yOffset + $scope.margin + 10 * $scope.verticalInterval)
                        .attr("font-size", "35px")
                        .text(signFromCharCode(0x1D13F));
                    break;
                case 32:
                    group.append("text")
                        .attr("x", x)
                        .attr("y", yOffset + $scope.margin + 10 * $scope.verticalInterval)
                        .attr("font-size", "35px")
                        .text(signFromCharCode(0x1D140));
                    break;
                case 64:
                    group.append("text")
                        .attr("x", x)
                        .attr("y", yOffset + $scope.margin + 9 * $scope.verticalInterval)
                        .attr("font-size", "35px")
                        .text(signFromCharCode(0x1D141));
                    break;
                case 128:
                    group.append("text")
                        .attr("x", x)
                        .attr("y", yOffset + $scope.margin + 9 * $scope.verticalInterval)
                        .attr("font-size", "35px")
                        .text(signFromCharCode(0x1D142));
                    break;
                default:
                    group.append("rect")
                        .attr("width", 10)
                        .attr("height", 5)
                        .attr("x", x)
                        .attr("y", yOffset + $scope.margin + 6 * $scope.verticalInterval)
                        .style("fill", "black");
                    break;
            }
        }

        // draws symbol from char code
        function signFromCharCode(code: number): string {
            if (code > 0xFFFF) {
                code -= 0x10000;
                return String.fromCharCode(0xD800 + (code >> 10), 0xDC00 + (code & 0x3FF));
            }
            else {
                return String.fromCharCode(code);
            }
        }

        // draws everything
        $((): void => {
            const notes: Note[] = $scope.data.musicNotes;
            const measures: Note[][] = $scope.data.measures;
            if (!notes || notes.length === 0) {
                console.error("No music notes found!");
                return;
            }

            let horizontalInterval: number = 1800 / (notes.length + 1); // calculating interval between notes
            const notesPerLine: number = 16; // Maximum number of notes per line
            const measuresPerLine: number = 4;
            const staffWidth: number = 1800; // Width of the staff
            const staffHeight: number = 140; // Height of one line
            const lineCount: number = Math.ceil(notes.length / notesPerLine); // Number of lines

            notation = d3.select<SVGSVGElement, null>("#notation")
                .append("svg")
                .attr("width", 1800)
                .attr("height", 140 * lineCount)
                .style("display", "block")
                .style("margin", "auto");

            visualization = d3.select<SVGSVGElement, null>("#visualization")
                .append("svg")
                .attr("width", 350)
                .attr("height", 100)
                .style("display", "block")
                .style("margin", "auto");

            drawKeyboard();
            //drawStaff();

            let min: number = 9;
            for (let j: number = 0; j < notes.length; j++) {
                const note: Note = notes[j];
                for (let k: number = 0; k < note.Pitches.length; k++) {
                    const pitch: Pitch = note.Pitches[k];
                    min = pitch.Octave < min ? pitch.Octave : min;
                }
            }

            for (let line: number = 0; line < lineCount; line++) {
                const yOffset: number = line * staffHeight; // Y offset for the new line

                drawStaff(yOffset); // Draw the staff for each line

                //let notesInLine = notes.slice(line * notesPerLine, (line + 1) * notesPerLine);
                const measuresInLine: Note[][] = measures.slice(line * measuresPerLine, (line + 1) * measuresPerLine);
                let notesInLine: number = 0;
                for (const measure of measuresInLine) {
                    notesInLine += measure.length;
                }
                notesInLine += measuresPerLine - 1;
                const horizontalInterval: number = staffWidth / (notesInLine + 1);
                let x: number = horizontalInterval;

                for (let i: number = 0; i < measuresInLine.length; i++) {
                    const measure: Note[] = measuresInLine[i];

                    for (let j: number = 0; j < measure.length; j++) {
                        const note: Note = measure[j];
                        const chord: NoteSelection = notation.append("g")
                            .attr("class", `chord_${j}`);



                        if (note.Pitches.length === 0) {
                            drawPause(chord, note, x, yOffset);
                        } else {
                            let lineUp: boolean = false;
                            let minY: number = 1000;
                            let maxY: number = -1000;

                            for (let k: number = 0; k < note.Pitches.length; k++) {
                                const pitch: Pitch = note.Pitches[k];
                                const currentOctave: number = pitch.Octave > min ? 1 : 0;
                                const step: Line = getLine(pitch.Step);
                                const y: number = $scope.margin + (13 - (step.line + 7 * currentOctave)) * $scope.verticalInterval + (line * staffHeight);

                                if (currentOctave === 0 && pitch.Step < 10) {
                                    lineUp = true;
                                }

                                minY = y < minY ? y : minY;
                                maxY = y < maxY ? maxY : y;
                                if (k == 0) {
                                    minY = y;
                                    maxY = y;
                                }

                                drawNote(chord, note, currentOctave, k, x, y);
                            }

                            if (note.Duration.Denominator == 1) {
                                continue;
                            }

                            let flagX: number;
                            let flagY: number;

                            if (lineUp) {
                                flagX = x + 6;
                                flagY = minY - 48;
                                chord.append("line")
                                    .attr("x1", flagX)
                                    .attr("y1", maxY - 1)
                                    .attr("x2", flagX)
                                    .attr("y2", flagY)
                                    .style("stroke", "black")
                                    .style("stroke-width", 2);
                            } else {
                                flagX = x - 6;
                                flagY = maxY + 48;
                                chord.append("line")
                                    .attr("x1", flagX)
                                    .attr("y1", minY + 1)
                                    .attr("x2", flagX)
                                    .attr("y2", flagY)
                                    .style("stroke", "black")
                                    .style("stroke-width", 2);
                            }

                            let flags: number = 0;
                            switch (note.Duration.Denominator) {
                                case 8: flags = 1; break;
                                case 16: flags = 2; break;
                                case 32: flags = 3; break;
                                case 64: flags = 4; break;
                                case 128: flags = 5; break;
                                case 256: flags = 6; break;
                                case 512: flags = 7; break;
                                case 1024: flags = 8; break;
                                case 2048: flags = 9; break;
                            }

                            for (let k: number = 0; k < flags; k++) {
                                if (lineUp) {
                                    chord.append("line")
                                        .attr("x1", flagX)
                                        .attr("y1", flagY + k * 4 + 1)
                                        .attr("x2", flagX + 16)
                                        .attr("y2", flagY + k * 4 + 2)
                                        .style("stroke", "black")
                                        .style("stroke-width", 3);
                                } else {
                                    chord.append("line")
                                        .attr("x1", flagX)
                                        .attr("y1", flagY - k * 4 - 1)
                                        .attr("x2", flagX - 16)
                                        .attr("y2", flagY - k * 4 - 2)
                                        .style("stroke", "black")
                                        .style("stroke-width", 3);
                                }
                            }
                        }
                        x = x + horizontalInterval;
                    }
                    notation.append("line")
                        .attr("x1", x)
                        .attr("y1", $scope.margin + 3 * $scope.verticalInterval + (line * staffHeight))
                        .attr("x2", x)
                        .attr("y2", $scope.margin + 11 * $scope.verticalInterval + (line * staffHeight))
                        .style("stroke", "#000")
                        .style("stroke-width", 2);
                    x = x + horizontalInterval;
                };
                //for (let j: number = 0; j < notesInLine.length; j++) {
                //    let note: Note = notesInLine[j];
                //    let chord = notation.append("g")
                //        .attr("class", `chord_${j}`);

                //    let x: number = (j + 1) * horizontalInterval;

                //    if (note.Pitches.length === 0) {
                //        drawPause(chord, note, x);
                //    } else {
                //        let lineUp: boolean = false;
                //        let minY: number = 1000;
                //        let maxY: number = -1000;

                //        for (let k = 0; k < note.Pitches.length; k++) {
                //            let pitch = note.Pitches[k];
                //            let currentOctave = pitch.Octave > min ? 1 : 0;
                //            let step = getLine(pitch.Step);
                //            let y = $scope.margin + (13 - (step.line + 7 * currentOctave)) * $scope.verticalInterval + (line * staffHeight);

                //            if (currentOctave === 0 && pitch.Step < 10) {
                //                lineUp = true;
                //            }

                //            minY = y < minY ? y : minY;
                //            maxY = y < maxY ? maxY : y;
                //            if (k == 0) {
                //                minY = y;
                //                maxY = y;
                //            }

                //            drawNote(chord, note, currentOctave, k, x, y);
                //        }

                //        let flagX, flagY;
                //        if (!lineUp && note.Duration.Denominator > 1) {
                //            flagX = x - 6;
                //            flagY = maxY + 48;
                //            chord.append("line")
                //                .attr("x1", flagX)
                //                .attr("y1", minY + 1)
                //                .attr("x2", flagX)
                //                .attr("y2", flagY)
                //                .style("stroke", "black")
                //                .style("stroke-width", 2);
                //        } else if (lineUp && note.Duration.Denominator > 1) {
                //            flagX = x + 6;
                //            flagY = minY - 48;
                //            chord.append("line")
                //                .attr("x1", flagX)
                //                .attr("y1", maxY - 1)
                //                .attr("x2", flagX)
                //                .attr("y2", flagY)
                //                .style("stroke", "black")
                //                .style("stroke-width", 2);
                //        }

                //        let flags = 0;
                //        switch (note.Duration.Denominator) {
                //            case 8: flags = 1; break;
                //            case 16: flags = 2; break;
                //            case 32: flags = 3; break;
                //            case 64: flags = 4; break;
                //            case 128: flags = 5; break;
                //        }

                //        if (flags > 0) {
                //            for (let k = 0; k < flags; k++) {
                //                if (lineUp) {
                //                    chord.append("line")
                //                        .attr("x1", flagX)
                //                        .attr("y1", flagY + k * 4 + 1)
                //                        .attr("x2", flagX + 16)
                //                        .attr("y2", flagY + k * 4 + 2)
                //                        .style("stroke", "black")
                //                        .style("stroke-width", 3);
                //                } else {
                //                    chord.append("line")
                //                        .attr("x1", flagX)
                //                        .attr("y1", flagY - k * 4 - 1)
                //                        .attr("x2", flagX - 16)
                //                        .attr("y2", flagY - k * 4 - 2)
                //                        .style("stroke", "black")
                //                        .style("stroke-width", 3);
                //                }
                //            }
                //        }
                //    }
                //}
            }


        });
    }

    angular.module("libiada").controller("MusicScoreCtrl", ["$scope", musicScore]);
}
