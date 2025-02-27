function MusicScoreController(data) {
    "use strict";

    function musicScore($scope) {
        MapModelFromJson($scope, data);

        // Инициализация MIDI-плеера
        $scope.onLoad = () => {
            MIDI.loadPlugin({
                soundfontUrl: "../../js/",
                instrument: "acoustic_grand_piano",
                onprogress: (state, progress) => {
                    console.log(state, progress);
                },
                onsuccess: () => {
                    console.log("MIDI loaded successfully");
                }
            });
        };

        let chordIndex;
        let player = {
            timeline: 0,
            velocity: 127,

            play: function (note, event) {
                let chord = [];
                let duration = note.Duration.Value;

                for (let i = 0; i < note.Pitches.length; i++) {
                    chord[i] = note.Pitches[i].MidiNumber;
                }

                MIDI.chordOn(0, chord, this.velocity, this.timeline);
                setTimeout(() => this.keyOn(event, note), this.timeline * 1000);
                setTimeout(() => this.noteOn(event), this.timeline * 1000);
                MIDI.chordOff(0, chord, this.velocity, this.timeline + duration);
                setTimeout(() => this.keyOff(event), (this.timeline + duration) * 900);
                setTimeout(() => this.noteOff(event), (this.timeline + duration) * 900);

                this.timeline += duration;
            },

            keyOn: function (event, note) {
                note.Pitches.forEach(pitch => {
                    let step = pitch.MidiNumber % 12;
                    d3.select(`#visualization`).select(`.key_${step}`).selectAll("rect")
                        .style("fill", "blue")
                        .style("fill-opacity", 1);
                });
            },

            keyOff: function () {
                for (let i = 0; i < 24; i++) {
                    let key = d3.select(`#visualization`).select(`.key_${i}`).selectAll("rect");
                    key.style("fill", "none");
                }
            },

            noteOn: function (event) {
                let chord = d3.select(`#notation`).select(`.chord_${chordIndex}`);
                chord.selectAll("ellipse").style("fill", "blue");
                chordIndex++;
            },

            noteOff: function (event) {
                let chord = d3.select(`#notation`).select(`.chord_${chordIndex}`);
                chord.selectAll("ellipse").style("fill", "black");
            }
        };

        $scope.play = () => {
            $scope.isPlaying = true;
            MIDI.setVolume(0, 80);
            player.timeline = 0;

            let notes = $scope.data.musicNotes;
            chordIndex = 0;

            let totalDuration = notes.reduce((sum, note) => sum + note.Duration.Value, 0);

            notes.forEach((note, i) => {
                setTimeout(() => player.play(note, i), player.timeline * 1000);
            });

            setTimeout(() => {
                $scope.isPlaying = false;
                $scope.$apply();
            }, totalDuration * 1000);
        };

        function drawStaff() {
            let staff = d3.select(`#notation`).append("svg")
                .attr("width", 900)
                .attr("height", 140);

            for (let i = 3; i < 12; i++) {
                if (i % 2 !== 0) {
                    staff.append("line")
                        .attr("x1", 0)
                        .attr("y1", $scope.margin + i * $scope.verticalInterval)
                        .attr("x2", 900)
                        .attr("y2", $scope.margin + i * $scope.verticalInterval)
                        .style("stroke", "#000")
                        .style("stroke-width", 2);
                }
            }
        }

        function drawKeyboard() {
            let keyboard = d3.select(`#visualization`).append("svg")
                .attr("width", 350)
                .attr("height", 100);

            for (let i = 0; i < 24; i++) {
                keyboard.append("g")
                    .attr("class", `key_${i}`)
                    .append("rect")
                    .attr("width", 25)
                    .attr("height", 100)
                    .style("fill", "none")
                    .style("stroke-width", 1)
                    .style("stroke", "black");
            }
        }

        $scope.margin = 40;
        $scope.verticalInterval = 10;

        drawKeyboard();
        drawStaff();
    }

    angular.module("libiada").controller("MusicScoreCtrl", ["$scope", musicScore]);
}
