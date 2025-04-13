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
                for (let j = 0; j < characteristic.FragmentsData.length; j++) {
                    let fragmentData = characteristic.FragmentsData[j];
                    $scope.points.push({
                        id: j,
                        characteristicId: i,
                        legendIndex: i,
                        legendId: i + 1,
                        name: fragmentData.Name,
                        characteristics: fragmentData.Characteristics,
                        researchObjectName: characteristic.ResearchObjectName
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
                yaxis: {
                    range: [min, max],
                    title: {
                        text: $scope.characteristicNames[characteristicIndex],
                    }
                }
            };

            $scope.chartData = $scope.points.map(p => ({
                hoverinfo: "text+x+y",
                x: [p.id],
                y: [p.characteristics[characteristicIndex]],
                marker: { color: $scope.legend[p.legendIndex].color },
                type: "scatter",
                line: {
                    color: $scope.legend[p.legendIndex].color,
                    width: 3
                },
                mode: "lines+markers",
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

        //function draw() {
        //    $scope.fillPoints();

        //    // removing previous chart and tooltip if any
        //    d3.select(".chart-tooltip").remove();
        //    d3.select(".chart-svg").remove();

        //    // chart size and margin settings
        //    let margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
        //    let width = $scope.width - margin.left - margin.right;
        //    let height = $scope.height - margin.top - margin.bottom;

        //    // setup x
        //    // calculating margins for dots
        //    let xMin = d3.min($scope.points, $scope.xValue);
        //    let xMax = d3.max($scope.points, $scope.xValue);
        //    let xMargin = (xMax - xMin) * 0.05;

        //    let xScale = d3.scaleLinear()
        //        .domain([xMin - xMargin, xMax + xMargin])
        //        .range([0, width]);
        //    let xAxis = d3.axisBottom(xScale)
        //        .tickSizeInner(-height)
        //        .tickSizeOuter(0)
        //        .tickPadding(10);

        //    $scope.xMap = d => xScale($scope.xValue(d));

        //    // setup y
        //    // calculating margins for dots
        //    let yMax = d3.max($scope.points, $scope.yValue);
        //    let yMin = d3.min($scope.points, $scope.yValue);
        //    let yMargin = (yMax - yMin) * 0.05;

        //    let yScale = d3.scaleLinear()
        //        .domain([yMin - yMargin, yMax + yMargin])
        //        .range([height, 0]);
        //    let yAxis = d3.axisLeft(yScale)
        //        .tickSizeInner(-width)
        //        .tickSizeOuter(0)
        //        .tickPadding(10);

        //    $scope.yMap = d => yScale($scope.yValue(d));

        //    // setup fill color
        //    let color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.legend.length]);

        //    // add the graph canvas to the body of the webpage
        //    let svg = d3.select("#chart").append("svg")
        //        .attr("width", $scope.width)
        //        .attr("height", $scope.height)
        //        .attr("class", "chart-svg")
        //        .append("g")
        //        .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

        //    // add the tooltip area to the webpage
        //    let tooltip = d3.select("#chart").append("div")
        //        .attr("class", "chart-tooltip position-absolute bg-light font-monospace small lh-sm p-1 rounded")
        //        .style("opacity", 0);

        //    // preventing tooltip hiding if dot clicked
        //    tooltip.on("click", () => { tooltip.hideTooltip = false; });

        //    // hiding tooltip
        //    d3.select("#chart").on("click", () => { $scope.clearTooltip(tooltip); });

        //    // x-axis
        //    svg.append("g")
        //        .attr("class", "x axis")
        //        .attr("transform", `translate(0,${height})`)
        //        .call(xAxis);

        //    svg.append("text")
        //        .attr("class", "label")
        //        .attr("transform", `translate(${width / 2} ,${height + margin.top - $scope.legendHeight})`)
        //        .style("text-anchor", "middle")
        //        .text($scope.lineChart ? "Fragment №" : $scope.firstCharacteristic.Text)
        //        .style("font-size", "12pt");

        //    // y-axis
        //    svg.append("g")
        //        .attr("class", "y axis")
        //        .call(yAxis);

        //    svg.append("text")
        //        .attr("class", "label")
        //        .attr("transform", "rotate(-90)")
        //        .attr("y", 0 - margin.left)
        //        .attr("x", 0 - (height / 2))
        //        .attr("dy", ".71em")
        //        .style("text-anchor", "middle")
        //        .text($scope.lineChart ? $scope.firstCharacteristic.Text : $scope.secondCharacteristic.Text)
        //        .style("font-size", "12pt");

        //    if ($scope.lineChart) {
        //        let line = d3.line()
        //            .x($scope.xMap)
        //            .y($scope.yMap);

        //        // Group the entries by symbol
        //        let dataGroups = d3.group($scope.points, d => d.researchObjectName);

        //        // Loop through each symbol / key
        //        dataGroups.forEach(value => {
        //            svg.append("path")
        //                .datum(value)
        //                .attr("class", "line")
        //                .attr("d", line)
        //                .attr("stroke", d => color(d[0].characteristicId))
        //                .attr("stroke-width", 1)
        //                .attr("fill", "none");
        //        });
        //    }

        //    // draw dots
        //    svg.selectAll(".dot")
        //        .data($scope.points)
        //        .enter()
        //        .append("ellipse")
        //        .attr("class", "dot")
        //        .attr("rx", $scope.dotRadius)
        //        .attr("ry", $scope.dotRadius)
        //        .attr("cx", $scope.xMap)
        //        .attr("cy", $scope.yMap)
        //        .style("fill-opacity", 0.6)
        //        .style("opacity", $scope.lineChart ? 0 : 1)
        //        .style("fill", d => color(d.characteristicId))
        //        .style("stroke", d => color(d.characteristicId))
        //        .on("click", (event, d) => $scope.showTooltip(event, d, tooltip, svg));

        //    // draw legend
        //    let legend = svg.selectAll(".legend")
        //        .data($scope.legend)
        //        .enter()
        //        .append("g")
        //        .attr("class", "legend")
        //        .attr("transform", (_d, i) => `translate(0,${i * 20})`)
        //        .on("click", function (event, d) {
        //            d.visible = !d.visible;
        //            let legendEntry = d3.select(event.currentTarget);
        //            legendEntry.select("text")
        //                .style("opacity", () => d.visible ? 1 : 0.5);
        //            legendEntry.select("rect")
        //                .style("fill-opacity", () => d.visible ? 1 : 0);

        //            svg.selectAll(".dot")
        //                .filter(dot => dot.researchObjectName === d.name)
        //                .attr("visibility", () => d.visible ? "visible" : "hidden");

        //            svg.selectAll(".line")
        //                .filter(line => line[0].researchObjectName === d.name)
        //                .attr("visibility", () => d.visible ? "visible" : "hidden");
        //        });

        //    // draw legend colored rectangles
        //    legend.append("rect")
        //        .attr("width", 15)
        //        .attr("height", 15)
        //        .style("fill", d => color(d.id))
        //        .style("stroke", d => color(d.id))
        //        .style("stroke-width", 4)
        //        .attr("transform", `translate(0, -${$scope.legendHeight})`);

        //    // draw legend text
        //    legend.append("text")
        //        .attr("x", 24)
        //        .attr("y", 9)
        //        .attr("dy", ".35em")
        //        .attr("transform", `translate(0, -${$scope.legendHeight})`)
        //        .text(d => d.name)
        //        .style("font-size", "9pt");
        //}

        $scope.calculateLocalCharacteristicsSimilarityMatrix = calculateLocalCharacteristicsSimilarityMatrix;
        $scope.changeCharacteristicsTableVisibility = changeCharacteristicsTableVisibility;
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