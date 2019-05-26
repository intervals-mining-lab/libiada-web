function FmotifsDictionaryController(data) {
    "use strict";

    function fmotifsDictionary($scope) {
        MapModelFromJson($scope, data);

        console.log($scope);

        $scope.onload = function () {
            MIDI.loadPlugin({
                soundfontUrl: "../../Scripts/midijs/soundfont/",
                instrument: "acoustic_grand_piano",
                onprogress: function (state, progress) {
                    console.log(state, progress);
                },
                onsuccess: function () {
                    //MIDI.programChange(0, MIDI.GM.byName["acoustic_guitar_steel"].program);
                }
            });

        };

        var player = {
            barDuration: 8,
            timeline: 0,
            velocity: 127,
            play: function (note, minOctave, moveTime, event) {
                var chord = [];
                var duration = note.Duration.Value;
                for (var i = 0; i < note.Pitches.length; i++) {
                    if ($scope.data.sequentialTransfer) {
                        chord[i] = note.Pitches[i].MidiNumber + 60;
                    }
                    else {
                        chord[i] = note.Pitches[i].MidiNumber;
                    }
                }
                MIDI.chordOn(0, chord, this.velocity, this.timeline);
                setTimeout(this.keyOn, this.timeline * 1000, event, note, minOctave);
                MIDI.chordOff(0, chord, this.velocity, this.timeline + this.barDuration * duration);
                setTimeout(this.keyOff, (this.timeline + this.barDuration * duration) * 900, event);

                if (typeof moveTime !== 'undefined' && moveTime === true) {
                    this.move(duration);
                }
            },
            move: function (duration) {
                this.timeline += this.barDuration * duration;
            },
            keyOn: function (event, note, min) {
                for (var i = 0; i < note.Pitches.length; i++) {
                    var pitch = note.Pitches[i];
                    var step = pitch.Step;
                    var currentOctave = pitch.Octave > min ? 1 : 0;
                    var key = d3.select("#visualization_" + $scope.data.fmotifs[event].Id)
                        .select(".key_" + (step + currentOctave * 12))
                        .selectAll("rect");
                    key.style("fill", "blue")
                        .style("fill-opacity", 1);
                }
            },
            keyOff: function (event) {
                for (var i = 0; i < 24; i++) {
                    var key = d3.select("#visualization_" + $scope.data.fmotifs[event].Id)
                        .select(".key_" + (i))
                        .selectAll("rect");
                    if (getLine(i % 12).alter === 0) {
                        key.style("fill", "none");
                    }
                    else if (getLine(i % 12).alter === 1) {
                        key.style("fill", "black");
                    }
                }
            }
        }

        $scope.play = function (event) {
            MIDI.setVolume(0, 80);
            player.timeline = 0;
            var notes = $scope.data.fmotifs[event].NoteList;
            var min = 9;
            for (var i = 0; i < notes.length; i++) {
                if (notes[i].Pitches.length > 0) {
                    for (var j = 0; j < notes[i].Pitches.length; j++) {
                        min = notes[i].Pitches[j].Octave < min ? notes[i].Pitches[j].Octave : min;
                    }
                }
            }
            for (var i = 0; i < notes.length; i++) {
                if (notes[i].Pitches.length > 0) {
                    player.play(notes[i], min, true, event);
                }
                else {
                    player.play(0, min, true, event);
                }
            }
        };

        function getLine(step) {
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
                default: return {line: -1, alter: -1, note: -1};
            }
        }

        function getHorizontalInterval(fmotif) {
            return 450 / (fmotif.NoteList.length + 1);
        }

        function drawKeyboard() {
            var rectPosX = 0;
            for (var i = 0; i < 24; i++) {
                if (getLine(i % 12).alter === 0) {
                    visualization.append("g")
                        .attr("class", "key_" + i)
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
            for (var i = 0; i < 24; i++) {
                if (getLine(i % 12).alter === 1) {
                    visualization.append("g")
                        .attr("class", "key_" + i)
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

        function drawStaff() {
            for (var i = 3; i < 12; i++) {
                if (i % 2 !== 0) {
                    notation.append("line")
                        .attr("x1", 0)
                        .attr("y1", $scope.margin + i * $scope.verticalInterval)
                        .attr("x2", 450)
                        .attr("y2", $scope.margin + i * $scope.verticalInterval)
                        .style("stroke", "#000")
                        .style("stroke-width", 2);
                }
            }
            notation.append("line")
                .attr("x1", 0)
                .attr("y1", $scope.margin + 3 * $scope.verticalInterval)
                .attr("x2", 0)
                .attr("y2", $scope.margin + 11 * $scope.verticalInterval)
                .style("stroke", "#000")
                .style("stroke-width", 5);
            notation.append("line")
                .attr("x1", 450)
                .attr("y1", $scope.margin + 3 * $scope.verticalInterval)
                .attr("x2", 450)
                    .attr("y2", $scope.margin + 11 * $scope.verticalInterval)
                .style("stroke", "#000")
                .style("stroke-width", 5);
        }

        function drawNotes(group, note, octave, iterator, x, y) {
            var step = note.Pitches[iterator].Step;
            if (octave === 0 && (step === 0 || step === 1)) {
                group.append("line")
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
                            .attr("x1", x - 15)
                            .attr("y1", y)
                            .attr("x2", x + 15)
                            .attr("y2", y)
                            .style("stroke", "#000")
                            .style("stroke-width", 2);
                        break;
                    case 11:
                        group.append("line")
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
            if (note.Duration.OriginalDenominator === 1 || note.Duration.OriginalDenominator === 2) {
                group.append("ellipse")
                    .attr("rx", 7)
                    .attr("ry", 5)
                    .attr("cx", x)
                    .attr("cy", y)
                    .attr("transform", "rotate(350 " + x + " " + y + ")")
                    .style("fill-opacity", 1)
                    .style("fill", "black");
                group.append("ellipse")
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
        }

        function drawPause(group, note, x) {
            if (note.Duration.OriginalDenominator === 1) {
                group.append("rect")
                    .attr("width", 10)
                    .attr("height", 5)
                    .attr("x", x)
                    .attr("y", $scope.margin + 6 * $scope.verticalInterval)
                    .style("fill", "black");
            }
            else {
                group.append("rect")
                    .attr("width", 10)
                    .attr("height", 5)
                    .attr("x", x)
                    .attr("y", $scope.margin + (6 - 1) * $scope.verticalInterval)
                    .style("fill", "black");
            }
        }

        $scope.margin = 40;
        $scope.verticalInterval = (140 - $scope.margin * 2) / 14;
        var notation;
        var visualization;

        $(function () {
            for (var i = 0; i < $scope.data.fmotifs.length; i++) {
                var fmotif = $scope.data.fmotifs[i];
                var horizontalInterval = getHorizontalInterval(fmotif);
                notation = d3
                    .select("#notation_" + fmotif.Id)
                    .append("svg")
                    .attr("width", 450)
                    .attr("height", 140);
                visualization = d3
                    .select("#visualization_" + fmotif.Id)
                    .append("svg")
                    .attr("width", 350)
                    .attr("height", 100);
                drawKeyboard();   
                drawStaff();
                var min = 9;
                for (var j = 0; j < fmotif.NoteList.length; j++) {
                    var note = fmotif.NoteList[j];
;                    for (var k = 0; k < note.Pitches.length; k++) {
                        var pitch = note.Pitches[k];
                        min = pitch.Octave < min ? pitch.Octave : min;
                    }
                }
                for (var j = 0; j < fmotif.NoteList.length; j++) {
                    var note = fmotif.NoteList[j];
                    var chord = notation.append("g");
                    var x = (j + 1) * horizontalInterval;
                    if (note.Pitches.length === 0) {
                        drawPause(chord, note, x);
                    }
                    else {
                        var lineUp = false;
                        var miny = 1000;
                        var maxy = -1000;
                        for (var k = 0; k < note.Pitches.length; k++) {
                            var pitch = note.Pitches[k];
                            var currentOctave = pitch.Octave > min ? 1 : 0;
                            var step = getLine(pitch.Step);
                            var y = $scope.margin + (13 - (step.line + 7 * currentOctave)) * $scope.verticalInterval;
                            if (currentOctave === 0 && pitch.Step < 10) {
                                lineUp = true;
                            }
                            console.log(lineUp);
                            miny = y < miny ? y : miny;
                            maxy = y < maxy ? maxy : y;
                            drawNotes(chord, note, currentOctave, k, x, y);
                        }
                        var flagx, flagy;
                        if (!lineUp && note.Duration.OriginalDenominator > 1) {
                            flagx = x - 6;
                            flagy = maxy + 40;
                            chord.append("line")
                                .attr("x1", flagx)
                                .attr("y1", miny + 1)
                                .attr("x2", flagx)
                                .attr("y2", flagy)
                                .style("stroke", "black")
                                .style("stroke-width", 2);
                        }
                        else if (lineUp && note.Duration.OriginalDenominator > 1) {
                            flagx = x + 6;
                            flagy = miny - 40;
                            chord.append("line")
                                .attr("x1", flagx)
                                .attr("y1", maxy - 1)
                                .attr("x2", flagx)
                                .attr("y2", flagy)
                                .style("stroke", "black")
                                .style("stroke-width", 2);
                        }
                        var flags = 0;
                        switch (note.Duration.OriginalDenominator) {
                            case 8: flags = 1; break;
                            case 16: flags = 2; break;
                            case 32: flags = 3; break;
                            case 64: flags = 4; break;
                        }
                        if (flags > 0) {
                            for (var k = 0; k < flags; k++) {
                                if (lineUp) {
                                    chord.append("line")
                                        .attr("x1", flagx)
                                        .attr("y1", flagy + k * 4)
                                        .attr("x2", flagx + 16)
                                        .attr("y2", flagy + k * 4 + 4)
                                        .style("stroke", "black")
                                        .style("stroke-width", 2);
                                }
                                else if (!lineUp) {
                                    chord.append("line")
                                        .attr("x1", flagx)
                                        .attr("y1", flagy - k * 4)
                                        .attr("x2", flagx - 16)
                                        .attr("y2", flagy - k * 4 - 4)
                                        .style("stroke", "black")
                                        .style("stroke-width", 2);
                                }
                            }
                        }
                    }
                }
            }
        });
    }

    angular.module("libiada").controller("FmotifsDictionaryCtrl", ["$scope", fmotifsDictionary]);
}