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
            play: function (pitches, duration, moveTime, event) {
                MIDI.chordOn(0, pitches, this.velocity, this.timeline);
                setTimeout(keyOn, this.timeline * 1000, event, pitches);
                MIDI.chordOff(0, pitches, this.velocity, this.timeline + this.barDuration * duration);
                setTimeout(keyOff, (this.timeline + this.barDuration * duration) * 900, event);

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
                if (notes[i].Pitches.length > 0) {
                    var pitches = [];
                    for (var j = 0; j < notes[i].Pitches.length; j++) {
                        pitches[j] = notes[i].Pitches[j].MidiNumber + octaver;
                    }
                    player.play(pitches, notes[i].Duration.Value, true, event);
                }
                else {
                    player.play(0, notes[i].Duration.Value, true, event);
                }
            }
            player.timeline = 0;
        };

        
        $scope.width = 50;
        $scope.height = 7;
        $scope.margin = 20;
        $scope.dotRadius = 5;
        var notation;
        var visualization;

        var getNote = function (step) {
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

        function drawKeyboard() {
            var rectPosX = 0;
            for (var i = 0; i < 24; i++) {

                if (getNote(i % 12).alter === 0) {
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
                if (getNote(i % 12).alter === 1) {
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
        
        function keyOn(event, pitches) {
            for (var i = 0; i < pitches.length; i++) {
                var key = d3.select("#visualization_" + $scope.data.fmotifs[event].Id)
                    .select(".key_" + (pitches[i] % 24))
                    .selectAll("rect");
                key.style("fill", "blue")
                    .style("fill-opacity", 1);
            }
            
        }

        function keyOff(event) {
            for (var i = 0; i < 24; i++) {
                var key = d3.select("#visualization_" + $scope.data.fmotifs[event].Id)
                    .select(".key_" + (i))
                    .selectAll("rect");
                if (getNote(i % 12).alter === 0) {
                    key.style("fill", "none");
                }
                else if (getNote(i % 12).alter === 1) {
                    key.style("fill", "black");
                }
            }
        }
        
        $(function () {
            for (var i = 0; i < $scope.data.fmotifs.length; i++) {
                var fmotif = $scope.data.fmotifs[i];
                var width = $scope.width * fmotif.NoteList.length;
                var height = $scope.height * 14 + $scope.margin * 2;
                notation = d3
                    .select("#notation_" + fmotif.Id)
                    .append("svg")
                    .attr("width", width)
                    .attr("height", height);
                visualization = d3
                    .select("#visualization_" + fmotif.Id)
                    .append("svg")
                    .attr("width", 350)
                    .attr("height", 100);
                drawKeyboard();               
                for (var j = 3; j < 13; j++) {
                    if (j % 2 !== 0) {
                        notation.append("line")
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
                            var type = getNote(pitch.Step);
                            var y = $scope.margin + (6 - type.line + 7 * (1 - currentOctave)) * $scope.height;
                            miny = y < miny ? y : miny;
                            maxy = y > maxy ? y : maxy;
                            notation.append("ellipse")
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
                            notation.append("line")
                                .attr("x1", $scope.width * (j + 1) - 25 - 3 * direction)
                                .attr("y1", direction < 0 ? maxy + center : miny + center)
                                .attr("x2", $scope.width * (j + 1) - 25 - 3 * direction)
                                .attr("y2", direction < 0 ? 0 + $scope.margin / 2 : height - $scope.margin / 2)
                                .style("stroke", "#000")
                                .style("stroke-width", "2");
                            notation.append("line")
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
                                notation.append("line")
                                    .attr("x1", $scope.width * (j + 1) - 25 - 2 * direction)
                                    .attr("y1", miny)
                                    .attr("x2", $scope.width * (j + 1) - 25 - 2 * direction)
                                    .attr("y2", direction < 0 ? 0 + $scope.margin / 2 : height - $scope.margin / 2)
                                    .style("stroke", "#000")
                                    .style("stroke-width", "2");
                                notation.append("line")
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
                        notation.append("rect")
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