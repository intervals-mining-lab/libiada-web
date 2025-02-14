
function ChartsController(data) {
    "use strict";

    function charts($scope, $document) {
        function parseTabularData(text) {
            //The array we will return
            let result = [];
            try {
                //Pasted data split into rows
                let rows = text.split(/[\n\f\r]/);
                // extracting first row that contains characteristics names
                let characteristics = rows.shift().split("\t");
                // extracting sequence name column
                $scope.sequencesName = characteristics.shift();
                let rawCharacteristics = rows.map(r => r.split("\t"));
                $scope.characteristics = [];
                for (let i = 0; i < rawCharacteristics.length; i++) {

                    $scope.characteristics[i] = { Characteristics: [], ResearchObjectName: rawCharacteristics[i][0] };
                    for (let j = 1; j < rawCharacteristics[i].length; j++) {
                        $scope.characteristics[i].Characteristics[j - 1] = +rawCharacteristics[i][j].replace(",",".");
                    }
                }
                //$scope.characteristics = characteristics.map((c, i) => ({Characteristics: c }));
                $scope.characteristicsList = characteristics.map((c, i) => ({ Value: i + 1, Text: c }));
                $scope.characteristicNames = characteristics.map(c => c);
                $scope.chartCharacteristics = [{ id: $scope.chartsCharacterisrticsCount++, value: $scope.characteristicsList[0] }];
                $scope.fillLegend();
                //$scope.firstCharacteristic = $scope.chartCharacteristics[0];
                //$scope.secondCharacteristic = $scope.chartCharacteristics.length > 1 ? $scope.chartCharacteristics[1] : $scope.chartCharacteristics[0];

                rows.forEach(thisRow => {
                    let row = thisRow.trim();
                    if (row) {
                        let cols = row.split("\t").map(c => c.replace(",", "."));
                        result.push(cols);
                    }
                });
            }
            catch (err) {
                console.log("error parsing as tabular data");
                console.log(err);
                return null;
            }
            return result;
        }

        function fillLegend() {
            $scope.legend = [];
            if ($scope.sequenceGroups) {
                $scope.colorScale = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.sequenceGroups.length]);
                for (let j = 0; j < $scope.sequenceGroups.length; j++) {
                    const color = $scope.colorScale(j + 1);
                    $scope.legend.push({
                        id: parseInt($scope.sequenceGroups[j].Value),
                        name: $scope.sequenceGroups[j].Text,
                        visible: true,
                        color: color
                    });

                    // hack for the legend's dot color
                    document.styleSheets[0].insertRule(`.legend${$scope.sequenceGroups[j].Value}:after { background:${color} }`);
                }
            } else {
                $scope.colorScale = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.characteristics.length]);
                for (let k = 0; k < $scope.characteristics.length; k++) {
                    const color = $scope.colorScale(k + 1);
                    $scope.legend.push({
                        id: k + 1,
                        name: $scope.characteristics[k].ResearchObjectName,
                        visible: true,
                        color: color
                    });

                    // hack for the legend's dot color
                    document.styleSheets[0].insertRule(`.legend${k + 1}:after { background:${color} }`);
                }
            }
        }

        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            let tooltipContent = [];
            tooltipContent.push(`Name: ${d.name}`);

            let pointData = $scope.characteristics[d.id - 1].Characteristics;
            let pointsCharacteristics = [];
            for (let i = 0; i < pointData.length; i++) {
                pointsCharacteristics.push(`${$scope.characteristicsList[i].Text}: ${pointData[i]}`);
            }

            tooltipContent.push(pointsCharacteristics.join("<br/>"));

            return tooltipContent.join("</br>");
        }

        // shows tooltip for dot or group of dots
        function showTooltip(selectedPoint) {
            $("button[data-bs-target='#tooltip-tab-pane']").tab("show");

            $scope.tooltipVisible = true;
            $scope.tooltip = {
                id: selectedPoint.id,
                name: selectedPoint.name,
                characteristics: selectedPoint.characteristics
            };
            let update = {};
            switch ($scope.chartCharacteristics.length) {
                case 1:
                    break;
                case 2:
                    update = {
                        "marker.symbol": $scope.points.map(point => point === selectedPoint ? "diamond-wide" : "circle"),
                        "marker.size": $scope.points.map(point => point === selectedPoint ? 15 : 6)
                    };
                    break;
                case 3:
                    break;
                default:
            }


            Plotly.restyle($scope.chartElement, update);

            $scope.$apply();
        }


        function legendClick(legendItem) {
            if ($scope.chartData && $scope.chartData[0].customdata) {
                let index = [];
                let update = { visible: legendItem.visible ? "legendonly" : true };
                for (let i = 0; i < $scope.chartData.length; i++) {
                    if ($scope.chartData[i].customdata.legendId === legendItem.id) {
                        index.push(i);
                    }
                }

                Plotly.restyle($scope.chartElement, update, index);
            }

        }

        function legendSetVisibilityForAll(visibility) {
            if ($scope.chartData && $scope.chartData[0].customdata) {
                let update = { visible: visibility ? true : "legendonly" };
                $scope.legend.forEach(l => l.visible = visibility);
                Plotly.restyle($scope.chartElement, update);
            }
        }

        $scope.addCharacteristic = () => $scope.chartCharacteristics.push({
            id: $scope.chartsCharacterisrticsCount++,
            value: $scope.characteristicsList.find(cl => $scope.chartCharacteristics.every(cc => cc.value !== cl))
        });

        $scope.deleteCharacteristic = characteristic => $scope.chartCharacteristics.splice($scope.chartCharacteristics.indexOf(characteristic), 1);

        //processing text changes
        function textChanged() {
            let text = $("#dataPasteBox").val();
            if (text) {
                $scope.rawData = text;
                let asArray = $scope.parseTabularData(text);
                if (asArray) {
                    $scope.parsedData = asArray;
                    $scope.$apply();
                }
            }
        }

        //processing keystrokes
        function handleKeyDown(e, args) {
            if (!$scope.inFocus && e.which === $scope.keyCodes.V && (e.ctrlKey || e.metaKey)) { // CTRL + V
                //reset value of our box
                $("#dataPasteBox").val("");
                //set it in focus so that pasted text goes inside the box
                $("#dataPasteBox").focus();
            }
        }

        $document.ready(() => {
            //Handles the Ctrl + V keys for pasting


            //If this is true, we wont respond to Ctrl + V
            $("body").on("focus", "input, textarea", () => { $scope.inFocus = true; });

            //We are not on a text element so we will respond
            //to Ctrl + V
            $("body").on("blur", "input, textarea", () => { $scope.inFocus = false; });

            //Handle the key down event
            $(document).keydown($scope.handleKeyDown);

            //We will respond to when the textbox value changes
            $("#dataPasteBox").bind("input propertychange", $scope.textChanged);
        });

      
        // initializes data for chart
        function fillPoints() {
            $scope.points = [];

            for (let i = 0; i < $scope.characteristics.length; i++) {
                let characteristic = $scope.characteristics[i];
                const legendIndex = characteristic.SequenceGroupId ? $scope.legend.findIndex(l => l.id === characteristic.SequenceGroupId) : i;
                $scope.points.push({
                    id: i + 1,
                    legendIndex: legendIndex,
                    legendId: characteristic.SequenceGroupId ? characteristic.SequenceGroupId : i + 1,
                    name: characteristic.ResearchObjectName,
                    characteristics: characteristic.Characteristics
                });
            }
        }

        function fillBarPlotData() {
            let characteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0].value);
            let min = Math.min(...$scope.points.map(p => p.characteristics[characteristicIndex]));
            let max = Math.max(...$scope.points.map(p => p.characteristics[characteristicIndex]));
            let range = Math.abs(max - min);
            let maxNameLength = Math.max(...$scope.points.map(p => p.name.length));

            // adding margins
            min -= Math.abs(range * 0.05);
            max += Math.abs(range * 0.05);

            $scope.layout = {
                margin: {
                    l: 50,
                    r: 20,
                    t: 10,
                    b: Math.min(150, maxNameLength * 10)

                },
                showlegend: false,
                xaxis: { categoryorder: "total ascending" },
                yaxis: {
                    range: [min, max],
                    title: {
                        text: $scope.characteristicNames[characteristicIndex],
                    }
                }
            };

            $scope.chartData = $scope.points.map(p => ({
                hoverinfo: "text+x+y",
                x: [p.name],
                y: [p.characteristics[characteristicIndex]],
                marker: { color: $scope.legend[p.legendIndex].color },
                type: "bar",
                customdata: { legendId: p.legendId },
                name: p.name,
                visible: $scope.legend[p.legendIndex].visible ? "true" : "legendonly"
            }));
        }

        function fillScatterPlotData() {
            let firstCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0].value);
            let secondCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[1].value);

            $scope.layout = {
                margin: {
                    l: 50,
                    r: 20,
                    t: 30,
                    b: 40
                },
                showlegend: false,
                hovermode: "closest",
                xaxis: {
                    //type: $scope.plotTypeX ? "log" : "",
                    title: {
                        text: $scope.characteristicNames[firstCharacteristicIndex]
                    }
                },
                yaxis: {
                    //type: $scope.plotTypeY ? "log" : "",
                    title: {
                        text: $scope.characteristicNames[secondCharacteristicIndex]
                    }
                }
            };

            $scope.chartData = $scope.points.map(p => ({
                hoverinfo: "text+x+y",
                type: "scattergl",
                x: [p.characteristics[firstCharacteristicIndex]],
                y: [p.characteristics[secondCharacteristicIndex]],
                text: p.name,
                mode: "markers",
                marker: { opacity: 0.8, color: $scope.legend[p.legendIndex].color },
                name: p.name,
                customdata: { legendId: p.legendId },
                visible: $scope.legend[p.legendIndex].visible
            }));
        }

        function fill3dScatterPlotData() {

            let firstCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0].value);
            let secondCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[1].value);
            let thirdCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[2].value);

            $scope.chartData = $scope.points.map(p => ({
                hoverinfo: "text+x+y+z",
                x: [p.characteristics[firstCharacteristicIndex]],
                y: [p.characteristics[secondCharacteristicIndex]],
                z: [p.characteristics[thirdCharacteristicIndex]],
                text: p.name,
                mode: "markers",
                marker: { opacity: 0.8, color: $scope.legend[p.legendIndex].color },
                name: p.name,
                type: "scatter3d",
                customdata: { legendId: p.legendId },
                visible: $scope.legend[p.legendIndex].visible
            }));

            $scope.layout = {
                margin: {
                    l: 0,
                    r: 0,
                    b: 0,
                    t: 0
                },
                showlegend: false,
                scene: {
                    xaxis: {
                        //type: $scope.plotTypeX ? "log" : "",
                        title: {
                            text: $scope.characteristicNames[firstCharacteristicIndex],
                            font: {
                                size: 10
                            }
                        },
                    },
                    yaxis: {
                        //type: $scope.plotTypeY ? "log" : "",
                        title: {
                            text: $scope.characteristicNames[secondCharacteristicIndex],
                            font: {
                                size: 10
                            }
                        }
                    },
                    zaxis: {
                        //type: $scope.plotTypeY ? "log" : "",
                        title: {
                            text: $scope.characteristicNames[thirdCharacteristicIndex],
                            font: {
                                size: 10
                            }
                        }
                    }
                }
            };
        }

        function fillParallelCoordinatesPlotData() {
            let characteristicsIndices = $scope.chartCharacteristics.map(c => $scope.characteristicsList.indexOf(c.value));

            $scope.chartData = [{
                type: "parcoords",
                //pad: [80, 80, 80, 80],
                line: {
                    color: $scope.points.map(p => p.legendIndex),
                    colorscale: "Turbo"
                },

                dimensions: characteristicsIndices.map(ci => ({
                    label: $scope.characteristicNames[ci],
                    values: $scope.points.map(p => p.characteristics[ci])
                }))
            }];

            $scope.layout = {
                margin: {
                    l: 50,
                    r: 50,
                    b: 20,
                    t: 70
                },
                showlegend: false
            };
        }
        function draw() {
            $scope.fillPoints();

            switch ($scope.chartCharacteristics.length) {
                case 1:
                    $scope.fillBarPlotData();
                    break;
                case 2:
                    $scope.fillScatterPlotData();
                    break;
                case 3:
                    $scope.fill3dScatterPlotData();
                    break;
                default:
                    $scope.fillParallelCoordinatesPlotData();
            }

            Plotly.newPlot($scope.chartElement, $scope.chartData, $scope.layout, { responsive: true });

            $scope.chartElement.on("plotly_click", data => {
                $scope.selectedPointIndex = data.points[0].pointNumber;
                $scope.selectedResearchObjectIndex = data.points[0].curveNumber;
                let selectedPoint = $scope.points[data.points[0].curveNumber];
                $scope.showTooltip(selectedPoint);
            });
        }

        $scope.parseTabularData = parseTabularData;
        $scope.textChanged = textChanged;
        $scope.handleKeyDown = handleKeyDown;
        $scope.fillBarPlotData = fillBarPlotData;
        $scope.fillScatterPlotData = fillScatterPlotData;
        $scope.fill3dScatterPlotData = fill3dScatterPlotData;
        $scope.fillParallelCoordinatesPlotData = fillParallelCoordinatesPlotData;
        $scope.draw = draw;
        $scope.fillPoints = fillPoints;
        $scope.fillPointTooltip = fillPointTooltip;
        $scope.showTooltip = showTooltip;
        $scope.fillLegend = fillLegend;
        $scope.legendClick = legendClick;
        $scope.legendSetVisibilityForAll = legendSetVisibilityForAll;
        

        $scope.chartsCharacterisrticsCount = 1;
        $scope.chartElement = document.getElementById("chart");

        $scope.keyCodes = {
            "C": 67,
            "V": 86
        };

        $scope.inFocus = false;
        $scope.rawData = "";
        $scope.parsedData = [];
    }

    angular.module("libiada").controller("ChartsCtrl", ["$scope", "$document", charts]);
}
