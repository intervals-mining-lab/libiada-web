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

        let timeouts = [];
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
                timeouts.push(setTimeout(() => this.keyOn(event, note), this.timeline * 1000));
                timeouts.push(setTimeout(() => this.noteOn(event), this.timeline * 1000));
                MIDI.chordOff(0, chord, this.velocity, this.timeline + duration);
                timeouts.push(setTimeout(() => this.keyOff(event), (this.timeline + duration) * 900));
                timeouts.push(setTimeout(() => this.noteOff(event), (this.timeline + duration) * 900));

                this.timeline += duration;
            },

            keyOn: function (event, note, min) {
                for (let i = 0; i < note.Pitches.length; i++) {
                    let pitch = note.Pitches[i];
                    let step = pitch.MidiNumber % 12;
                    let currentOctave = pitch.Octave > min ? 1 : 0;
                    let key = d3.select(`#visualization`)
                        .select(`.key_${step + currentOctave * 12}`)
                        .selectAll("rect");
                    key.style("fill", "blue")
                        .style("fill-opacity", 1);
                }
            },
            keyOff: function (event) {
                for (let i = 0; i < 24; i++) {
                    let key = d3.select(`#visualization`)
                        .select(`.key_${i}`)
                        .selectAll("rect");
                    if (getLine(i % 12).alter === 0) {
                        key.style("fill", "none");
                    } else if (getLine(i % 12).alter === 1) {
                        key.style("fill", "black");
                    }
                }
            },
            noteOn: function (event) {
                let chord = d3.select(`#notation`)
                    .select(`.chord_${chordIndex}`);
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
            noteOff: function (event) {
                let chord = d3.select(`#notation`)
                    .select(`.chord_${chordIndex}`);
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

        

        $scope.play = event => {
            $scope.isPlaying = true;
            MIDI.setVolume(0, 80);
            player.timeline = 0;

            let notes = $scope.data.musicNotes;
            let min = 9;
            chordIndex = 0;

            let totalDuration = notes.reduce((sum, note) => sum + note.Duration.Value, 0);

            notes.forEach((note, i) => {
                player.play(note, min, true, event);
            });

            let stopTimeout = setTimeout(() => {
                $scope.isPlaying = false;
                $scope.$apply();
            }, totalDuration * 1000);
            timeouts.push(stopTimeout);
        };

        $scope.stopAll = () => {
            console.log("Stopping all scheduled playbacks...");
            timeouts.forEach(timeoutID => clearTimeout(timeoutID));
            timeouts = [];
            $scope.isPlaying = false;
            $scope.$apply();
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
                default: return { line: -1, alter: -1, note: -1 };
            }
        }

        function drawKeyboard() {
            let rectPosX = 0;
            for (let i = 0; i < 24; i++) {
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
            for (let i = 0; i < 24; i++) {
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
        function drawStaff(yOffset) {
            for (let i = 3; i < 12; i++) {
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
        function drawNote(group, note, octave, iterator, x, y) {
            let step = note.Pitches[iterator].MidiNumber % 12;
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
        function drawPause(group, note, x, yOffset) {
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
        function signFromCharCode(code) {
            if (code > 0xFFFF) {
                code -= 0x10000;
                return String.fromCharCode(0xD800 + (code >> 10), 0xDC00 + (code & 0x3FF));
            }
            else {
                return String.fromCharCode(code);
            }
        }

        $scope.margin = 40;
        $scope.verticalInterval = (140 - $scope.margin * 2) / 14;
        let notation;
        let visualization;


        $(function () {
            let notes = $scope.data.musicNotes;
            let measures = $scope.data.measures;
            if (!notes || notes.length === 0) {
                console.error("No music notes found!");
                return;
            }

            let horizontalInterval = 1800 / (notes.length + 1); // Рассчитываем интервал между нотами
            let notesPerLine = 16; // Максимальное количество нот в одной строке
            let measuresPerLine = 4;
            let staffWidth = 1800; // Ширина нотного стана
            let staffHeight = 140; // Высота одной строки
            let lineCount = Math.ceil(notes.length / notesPerLine); // Количество строк

            notation = d3.select("#notation")
                .append("svg")
                .attr("width", 1800)
                .attr("height", 140 * lineCount)
                .style("display", "block")
                .style("margin", "auto");

            visualization = d3.select("#visualization")
                .append("svg")
                .attr("width", 350)
                .attr("height", 100)
                .style("display", "block")
                .style("margin", "auto");

            drawKeyboard();
            //drawStaff();

            let min = 9;
            for (let j = 0; j < notes.length; j++) {
                let note = notes[j];
                for (let k = 0; k < note.Pitches.length; k++) {
                    let pitch = note.Pitches[k];
                    min = pitch.Octave < min ? pitch.Octave : min;
                }
            }

            for (let line = 0; line < lineCount; line++) {
                let yOffset = line * staffHeight; // Смещение по Y для новой строки

                drawStaff(yOffset); // Отрисовываем нотный стан для каждой строки

                //let notesInLine = notes.slice(line * notesPerLine, (line + 1) * notesPerLine);
                let measuresInLine = measures.slice(line * measuresPerLine, (line + 1) * measuresPerLine);
                let notesInLine = 0;
                for (let measure of measuresInLine) {
                    notesInLine += measure.length;
                }
                notesInLine += measuresPerLine - 1;
                let horizontalInterval = staffWidth / (notesInLine + 1);
                let x = horizontalInterval;

                for (let i = 0; i < measuresInLine.length; i++) {
                    let measure = measuresInLine[i];

                    for (let j = 0; j < measure.length; j++) {
                        let note = measure[j];
                        let chord = notation.append("g")
                            .attr("class", `chord_${j}`);

                        

                        if (note.Pitches.length === 0) {
                            drawPause(chord, note, x, yOffset);
                        } else {
                            let lineUp = false;
                            let miny = 1000;
                            let maxy = -1000;

                            for (let k = 0; k < note.Pitches.length; k++) {
                                let pitch = note.Pitches[k];
                                let currentOctave = pitch.Octave > min ? 1 : 0;
                                let step = getLine(pitch.Step);
                                let y = $scope.margin + (13 - (step.line + 7 * currentOctave)) * $scope.verticalInterval + (line * staffHeight);

                                if (currentOctave === 0 && pitch.Step < 10) {
                                    lineUp = true;
                                }

                                miny = y < miny ? y : miny;
                                maxy = y < maxy ? maxy : y;
                                if (k == 0) {
                                    miny = y;
                                    maxy = y;
                                }

                                drawNote(chord, note, currentOctave, k, x, y);
                            }

                            let flagx, flagy;
                            if (!lineUp && note.Duration.Denominator > 1) {
                                flagx = x - 6;
                                flagy = maxy + 48;
                                chord.append("line")
                                    .attr("x1", flagx)
                                    .attr("y1", miny + 1)
                                    .attr("x2", flagx)
                                    .attr("y2", flagy)
                                    .style("stroke", "black")
                                    .style("stroke-width", 2);
                            } else if (lineUp && note.Duration.Denominator > 1) {
                                flagx = x + 6;
                                flagy = miny - 48;
                                chord.append("line")
                                    .attr("x1", flagx)
                                    .attr("y1", maxy - 1)
                                    .attr("x2", flagx)
                                    .attr("y2", flagy)
                                    .style("stroke", "black")
                                    .style("stroke-width", 2);
                            }

                            let flags = 0;
                            switch (note.Duration.Denominator) {
                                case 8: flags = 1; break;
                                case 16: flags = 2; break;
                                case 32: flags = 3; break;
                                case 64: flags = 4; break;
                                case 128: flags = 5; break;
                            }

                            if (flags > 0) {
                                for (let k = 0; k < flags; k++) {
                                    if (lineUp) {
                                        chord.append("line")
                                            .attr("x1", flagx)
                                            .attr("y1", flagy + k * 4 + 1)
                                            .attr("x2", flagx + 16)
                                            .attr("y2", flagy + k * 4 + 2)
                                            .style("stroke", "black")
                                            .style("stroke-width", 3);
                                    } else {
                                        chord.append("line")
                                            .attr("x1", flagx)
                                            .attr("y1", flagy - k * 4 - 1)
                                            .attr("x2", flagx - 16)
                                            .attr("y2", flagy - k * 4 - 2)
                                            .style("stroke", "black")
                                            .style("stroke-width", 3);
                                    }
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
                //for (let j = 0; j < notesInLine.length; j++) {
                //    let note = notesInLine[j];
                //    let chord = notation.append("g")
                //        .attr("class", `chord_${j}`);

                //    let x = (j + 1) * horizontalInterval;

                //    if (note.Pitches.length === 0) {
                //        drawPause(chord, note, x);
                //    } else {
                //        let lineUp = false;
                //        let miny = 1000;
                //        let maxy = -1000;

                //        for (let k = 0; k < note.Pitches.length; k++) {
                //            let pitch = note.Pitches[k];
                //            let currentOctave = pitch.Octave > min ? 1 : 0;
                //            let step = getLine(pitch.Step);
                //            let y = $scope.margin + (13 - (step.line + 7 * currentOctave)) * $scope.verticalInterval + (line * staffHeight);

                //            if (currentOctave === 0 && pitch.Step < 10) {
                //                lineUp = true;
                //            }

                //            miny = y < miny ? y : miny;
                //            maxy = y < maxy ? maxy : y;
                //            if (k == 0) {
                //                miny = y;
                //                maxy = y;
                //            }

                //            drawNote(chord, note, currentOctave, k, x, y);
                //        }

                //        let flagx, flagy;
                //        if (!lineUp && note.Duration.Denominator > 1) {
                //            flagx = x - 6;
                //            flagy = maxy + 48;
                //            chord.append("line")
                //                .attr("x1", flagx)
                //                .attr("y1", miny + 1)
                //                .attr("x2", flagx)
                //                .attr("y2", flagy)
                //                .style("stroke", "black")
                //                .style("stroke-width", 2);
                //        } else if (lineUp && note.Duration.Denominator > 1) {
                //            flagx = x + 6;
                //            flagy = miny - 48;
                //            chord.append("line")
                //                .attr("x1", flagx)
                //                .attr("y1", maxy - 1)
                //                .attr("x2", flagx)
                //                .attr("y2", flagy)
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
                //                        .attr("x1", flagx)
                //                        .attr("y1", flagy + k * 4 + 1)
                //                        .attr("x2", flagx + 16)
                //                        .attr("y2", flagy + k * 4 + 2)
                //                        .style("stroke", "black")
                //                        .style("stroke-width", 3);
                //                } else {
                //                    chord.append("line")
                //                        .attr("x1", flagx)
                //                        .attr("y1", flagy - k * 4 - 1)
                //                        .attr("x2", flagx - 16)
                //                        .attr("y2", flagy - k * 4 - 2)
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
