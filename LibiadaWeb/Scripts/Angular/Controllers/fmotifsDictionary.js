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

    }

    angular.module("libiada").controller("FmotifsDictionaryCtrl", ["$scope", fmotifsDictionary]);
}