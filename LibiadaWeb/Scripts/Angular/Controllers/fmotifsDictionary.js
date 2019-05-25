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
            barDuration: 4,
            timeline: 0,
            velocity: 127,
            playNote: function (pitch, duration, moveTime) {
                MIDI.noteOn(0, pitch, this.velocity, this.timeline);
                MIDI.noteOff(0, pitch, this.velocity, this.timeline + this.barDuration * duration);

                if (typeof moveTime !== 'undefined' && moveTime === true) {
                    this.move(duration);
                }
            },
            playChord: function (pitches, duration, moveTime) {
                MIDI.chordOn(0, pitches, this.velocity, this.timeline);
                MIDI.chordOff(0, pitches, this.velocity, this.timeline + this.barDuration * duration);

                if (typeof moveTime !== 'undefined' && moveTime === true) {
                    this.move(duration);
                }
            },
            move: function (duration) {
                this.timeline += this.barDuration * duration;
            }
        };

        $scope.play = function (event) {
            MIDI.setVolume(0, 80);
            var notes = $scope.data.fmotifs[event].NoteList;
            var octaver = $scope.data.sequentialTransfer ? 60 : 0;
            for (var i = 0; i < notes.length; i++) {
                if (notes[i].Pitches.length > 1) {
                    var pitches = [];
                    for (var j = 0; j < notes[i].Pitches.length; j++) {
                        pitches[j] = notes[i].Pitches[j].MidiNumber + octaver;
                    }
                    player.playChord(pitches, notes[i].Duration.Value, true);
                }
                else if (notes[i].Pitches.length === 1) {
                    player.playNote(notes[i].Pitches[0].MidiNumber + octaver, notes[i].Duration.Value, true);
                }
                else {
                    player.playNote(0, notes[i].Duration.Value, true);
                }
            }
            player.timeline = 0;
        };

        
        $scope.width = 50;
        $scope.height = 7;
        $scope.margin = 20;
        $scope.dotRadius = 5;

        $scope.getLine = function (step) {
            switch (step) {
                case 0: return { line: 0, alter: 0 };
                case 1: return { line: 0, alter: 1 };
                case 2: return { line: 1, alter: 0 };
                case 3: return { line: 1, alter: 1 };
                case 4: return { line: 2, alter: 0 };
                case 5: return { line: 3, alter: 0 };
                case 6: return { line: 3, alter: 1 };
                case 7: return { line: 4, alter: 0 };
                case 8: return { line: 4, alter: 1 };
                case 9: return { line: 5, alter: 0 };
                case 10: return { line: 5, alter: 1 };
                case 11: return { line: 6, alter: 0 };
                default: return {line: -1, alter: -1};
                    
            }
        }

        $(function () {
            var color = d3.scaleOrdinal(d3.schemeCategory10);
            for (var i = 0; i < $scope.data.fmotifs.length; i++) {
                var fmotif = $scope.data.fmotifs[i];
                var width = $scope.width * fmotif.NoteList.length;
                var height = $scope.height * 14 + $scope.margin * 2;
                var svg = d3
                    .select("#notation_" + fmotif.Id)
                    .append("svg")
                    .attr("width", width)
                    .attr("height", height);
                for (var j = 3; j < 13; j++) {
                    if (j % 2 != 0) {
                        svg.append("line")
                            .attr("x1", 0)
                            .attr("y1", $scope.margin + j * $scope.height)
                            .attr("x2", width)
                            .attr("y2", $scope.margin + j * $scope.height)
                            .style("stroke", "#000")
                            .style("stroke-width", "2");
                    }
                }
                var min = 9;
                for (var j = 0; j < fmotif.NoteList.length; j++) {
                    var note = fmotif.NoteList[j];
                    for (var k = 0; k < note.Pitches.length; k++) {
                        var pitch = note.Pitches[k];
                        min = pitch.Octave < min ? pitch.Octave : min;
                    }
                }
                for (var j = 0; j < fmotif.NoteList.length; j++) {
                    var note = fmotif.NoteList[j];
                    var center = $scope.margin + (6) * $scope.height;
                    if (note.Pitches.length > 0) {
                        var maxy = 0;
                        var miny = $scope.width * 2 + $scope.height * 20;
                        for (var k = 0; k < note.Pitches.length; k++) {
                            var pitch = note.Pitches[k];
                            var currentOctave = pitch.Octave > min ? 1 : 0;
                            var type = $scope.getLine(pitch.Step);
                            var y = $scope.margin + (6 - type.line + 7 * (1 - currentOctave)) * $scope.height;
                            miny = y < miny ? y : miny;
                            maxy = y > maxy ? y : maxy;
                            svg.append("ellipse")
                                .attr("rx", $scope.dotRadius)
                                .attr("ry", $scope.dotRadius)
                                .attr("cx", $scope.width * (j + 1) - 25)
                                .attr("cy", y)
                                .style("fill-opacity", 1)
                                .style("fill", "black")
                                .style("stroke", "black");
                        }
                        if (note.Pitches.length > 1) {
                            miny -= center;
                            maxy -= center;
                            var direction = 0;
                            if (Math.sign(miny) === Math.sign(maxy)) {
                                direction = Math.sign(miny);
                            }
                            else {
                                direction = Math.abs(miny) > Math.abs(maxy) ? -1 : 1;
                            }
                            svg.append("line")
                                .attr("x1", $scope.width * (j + 1) - 25 - 3 * direction)
                                .attr("y1", direction < 0 ? maxy + center : miny + center)
                                .attr("x2", $scope.width * (j + 1) - 25 - 3 * direction)
                                .attr("y2", direction < 0 ? 0 + $scope.margin / 2 : height - $scope.margin / 2)
                                .style("stroke", "#000")
                                .style("stroke-width", "2");
                            svg.append("line")
                                .attr("x1", $scope.width * (j + 1) - 25 - 2 * direction)
                                .attr("y1", direction < 0 ? 0 + $scope.margin / 2 : height - $scope.margin / 2)
                                .attr("x2", $scope.width * (j + 1) - 25 - 10 * direction)
                                .attr("y2", direction < 0 ? 0 + $scope.margin / 2 : height - $scope.margin / 2)
                                .style("stroke", "#000")
                                .style("stroke-width", "2");
                        }
                        else {
                            var direction = miny - center < 0 ? -1 : 1;
                            if (note.Duration.OriginalValue < 1) {
                                svg.append("line")
                                    .attr("x1", $scope.width * (j + 1) - 25 - 2 * direction)
                                    .attr("y1", miny)
                                    .attr("x2", $scope.width * (j + 1) - 25 - 2 * direction)
                                    .attr("y2", direction < 0 ? 0 + $scope.margin / 2 : height - $scope.margin / 2)
                                    .style("stroke", "#000")
                                    .style("stroke-width", "2");
                                svg.append("line")
                                    .attr("x1", $scope.width * (j + 1) - 25 - 2 * direction)
                                    .attr("y1", direction < 0 ? 0 + $scope.margin / 2 : height - $scope.margin / 2)
                                    .attr("x2", $scope.width * (j + 1) - 25 - 10 * direction)
                                    .attr("y2", direction < 0 ? 0 + $scope.margin / 2 : height - $scope.margin / 2)
                                    .style("stroke", "#000")
                                    .style("stroke-width", "2");
                            }
                        }


                    }
                    else {
                        svg.append("rect")
                            .attr("width", $scope.dotRadius * 2)
                            .attr("height", $scope.dotRadius * 2)
                            .attr("x", $scope.width * (j + 1) - 25 - $scope.dotRadius)
                            .attr("y", center - $scope.dotRadius)
                            .style("fill", "black");
                    }
                }
            }
        });
    }

    angular.module("libiada").controller("FmotifsDictionaryCtrl", ["$scope", fmotifsDictionary]);
}