function LocalCalculationResultController() {
    "use strict";

    function localCalculationResult($scope, $http) {
        function calculateLocalCharacteristicsSimilarityMatrix() {
            $http.get("/api/LocalCalculationApi/CalculateLocalCharacteristicsSimilarityMatrix", {
                params: {
                    taskId: $scope.taskId,
                    aligner: $scope.aligner.Value,
                    distanceCalculator: $scope.distanceCalculator.Value,
                    aggregator: $scope.aggregator.Value
                }
            }).then(function (result) {
                const response = result.data;
                $scope.comparisonMatrix = response.result;
                // TODO: fill this data from responce
                $scope.usedAligner = $scope.aligners[response.aligner - 1].Text;
                $scope.usedDistanceCalculator = $scope.distanceCalculators[response.distanceCalculator - 1].Text;
                $scope.usedAggregator = $scope.aggregators[response.aggregator - 1].Text;
            }, function (error) {
                alert("Failed loading alignment data");
                $scope.loading = false;
            });
        }

        $scope.isCharacteristicsTableVisible = false;

        function changeCharacteristicsTableVisibility() {
            $scope.isCharacteristicsTableVisible = true;
        }

        function fillLegend() {
            $scope.legend = [];

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

        $scope.addCharacteristic = () => $scope.chartCharacteristics.push({
            id: $scope.chartsCharacterisrticsCount++,
            value: $scope.characteristicsList.find(cl => $scope.chartCharacteristics.every(cc => cc.value !== cl))
        });

        $scope.deleteCharacteristic = characteristic => $scope.chartCharacteristics.splice($scope.chartCharacteristics.indexOf(characteristic), 1);

        // initializes data for chart
        function fillPoints() {
            $scope.points = [];

            for (let i = 0; i < $scope.characteristics.length; i++) {
                let characteristic = $scope.characteristics[i];
                $scope.points.push({
                    legendIndex: i,
                    legendId: i + 1,
                    researchObjectName: characteristic.ResearchObjectName,
                    fragmentsData: []
                });
                for (let j = 0; j < characteristic.FragmentsData.length; j++) {
                    let fragmentData = characteristic.FragmentsData[j];
                    $scope.points[i].fragmentsData.push({
                        id: j,
                        //characteristicId: i,
                        name: fragmentData.Name,
                        characteristics: fragmentData.Characteristics   
                    });
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


        function fillLinePlotData() {
            let characteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0].value);
            let characteristicsValues = $scope.points.map((p => p.fragmentsData.map(fd => fd.characteristics[characteristicIndex]))).flat();
            let min = Math.min(...characteristicsValues);
            let max = Math.max(...characteristicsValues);
            let range = Math.abs(max - min);
           
            // adding margins
            min -= Math.abs(range * 0.05);
            max += Math.abs(range * 0.05);

            $scope.layout = {
                margin: {
                    l: 50,
                    r: 20,
                    t: 10,
                    b: 20

                },
                showlegend: false,
                yaxis: {
                    range: [min, max],
                    title: {
                        text: $scope.characteristicNames[characteristicIndex],
                    }
                }
            };

            $scope.chartData = $scope.points.map(p => ({
                hoverinfo: "text+x+y",
                x: p.fragmentsData.map(fd => fd.id),
                y: p.fragmentsData.map(fd => fd.characteristics[characteristicIndex]),
                marker: {
                    color: $scope.legend[p.legendIndex].color,
                    size: 2,
                    opacity: 0.8
                },
                type: "scatter",
                line: {
                    color: $scope.legend[p.legendIndex].color,
                    width: 0.75
                },
                mode: "lines+markers",
                customdata: { legendId: p.legendId },
                name: p.researchObjectName,
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
                x: p.fragmentsData.map(fd => fd.characteristics[firstCharacteristicIndex]),     
                y: p.fragmentsData.map(fd => fd.characteristics[secondCharacteristicIndex]),
                text: p.researchObjectName,
                mode: "markers",
                marker: {
                    opacity: 0.8,
                    color: $scope.legend[p.legendIndex].color,
                    size: 3
                },
                name: p.researchObjectName,
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
                x: p.fragmentsData.map(fd => fd.characteristics[firstCharacteristicIndex]),
                y: p.fragmentsData.map(fd => fd.characteristics[secondCharacteristicIndex]),
                z: p.fragmentsData.map(fd => fd.characteristics[thirdCharacteristicIndex]),
                text: p.name,
                mode: "markers",
                marker: {
                    opacity: 0.8,
                    color: $scope.legend[p.legendIndex].color,
                    size: 3
                },
                name: p.researchObjectName,
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
                    colorscale: "Turbo",
                },

                dimensions: characteristicsIndices.map(ci => ({
                    label: $scope.characteristicNames[ci],
                    values: $scope.points.map(p => p.fragmentsData.map(fd => fd.characteristics[ci]))
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
                    $scope.fillLinePlotData();
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

        function dragbarMouseDown() {
            let right = document.getElementById("sidebar");
            let bar = document.getElementById("dragbar");

            const drag = (e) => {
                document.selection ? document.selection.empty() : window.getSelection().removeAllRanges();
                $scope.chartElement.style.width = `${e.pageX - bar.offsetWidth / 2}px`;

                Plotly.relayout($scope.chartElement, { autosize: true });
            };

            bar.addEventListener("mousedown", () => {
                document.addEventListener("mousemove", drag);
            });

            bar.addEventListener("mouseup", () => {
                document.removeEventListener("mousemove", drag);
            });
        }


        $scope.calculateLocalCharacteristicsSimilarityMatrix = calculateLocalCharacteristicsSimilarityMatrix;
        $scope.changeCharacteristicsTableVisibility = changeCharacteristicsTableVisibility;
        $scope.fillLinePlotData = fillLinePlotData;
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
        $scope.dragbarMouseDown = dragbarMouseDown;

        $scope.chartsCharacterisrticsCount = 1;
        $scope.chartElement = document.getElementById("chart");

        $scope.loadingScreenHeader = "Loading data";

        let location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $http.get(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
            .then(function (data) {
                MapModelFromJson($scope, data.data);

                $scope.fillLegend();

                $scope.chartCharacteristics = [{ id: $scope.chartsCharacterisrticsCount++, value: $scope.characteristicsList[0] }];
                $scope.aligner = $scope.aligners[0];
                $scope.distanceCalculator = $scope.distanceCalculators[0];
                $scope.aggregator = $scope.aggregators[0];

                $scope.legendHeight = $scope.legend.length * 20;
                $scope.height = 800 + $scope.legendHeight;

                $scope.loading = false;
            }, function () {
                alert("Failed loading local characteristics data");
                $scope.loading = false;
            });
    }

    angular.module("libiada").controller("LocalCalculationResultCtrl", ["$scope", "$http", localCalculationResult]);
}