function LocalCalculationResultController() {
    "use strict";

    function localCalculationResult($scope, $http) {

        function fillLegend() {
            $scope.legend = [];

            for (let k = 0; k < $scope.characteristics.length; k++) {
                $scope.legend.push({ id: k, name: $scope.characteristics[k].MatterName, visible: true });
            }
        }

        // initializes data for chart
        function fillPoints() {
            $scope.points = [];
            let first = +$scope.firstCharacteristic.Value;
            let second = +$scope.secondCharacteristic.Value;

            for (let i = 0; i < $scope.characteristics.length; i++) {
                let characteristic = $scope.characteristics[i];
                for (let j = 0; j < characteristic.FragmentsData.length; j++) {
                    let fragmentData = characteristic.FragmentsData[j];
                    $scope.points.push({
                        id: j,
                        characteristicId: i,
                        name: fragmentData.Name,
                        x: fragmentData.Characteristics[first],
                        y: fragmentData.Characteristics[second],
                        matterName: characteristic.MatterName
                    });
                }
            }
        }

        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            let tooltipContent = [];
            tooltipContent.push(d.matterName);
            tooltipContent.push("Name: " + d.name);
            tooltipContent.push("Fragment №: " + d.id);
            let pointSharacteristics = [];
            let characteristics = $scope.characteristics[d.characteristicId].FragmentsData[d.id].Characteristics;
            for (let i = 0; i < characteristics.length; i++) {
                pointSharacteristics.push($scope.characteristicsList[i].Text + ": " + characteristics[i]);
            }

            tooltipContent.push(pointSharacteristics.join("<br/>"));

            return tooltipContent.join("</br>");
        }

        // shows tooltip for dot or group of dots
        function showTooltip(event, d, tooltip, svg) {
            $scope.clearTooltip(tooltip);

            tooltip.style("opacity", 0.9);

            let tooltipHtml = [];

            tooltip.selectedDots = svg.selectAll(".dot")
                .filter(dot => {
                    if (dot.x === d.x && dot.y === d.y) {
                        tooltipHtml.push($scope.fillPointTooltip(dot));
                        return true;
                    } else {
                        return false;
                    }
                })
                .attr("rx", $scope.selectedDotRadius)
                .attr("ry", $scope.selectedDotRadius);

            tooltip.html(tooltipHtml.join("</br></br>"));

            tooltip.style("left", (event.pageX + 10) + "px")
                .style("top", (event.pageY - 8) + "px");

            tooltip.hideTooltip = false;
        }

        // clears tooltip and unselects dots
        function clearTooltip(tooltip) {
            if (tooltip) {
                if (tooltip.hideTooltip) {
                    tooltip.html("").style("opacity", 0);

                    if (tooltip.selectedDots) {
                        tooltip.selectedDots.attr("rx", $scope.dotRadius)
                            .attr("ry", $scope.dotRadius);
                    }
                }

                tooltip.hideTooltip = true;
            }
        }

        function xValue(d) {
            return $scope.lineChart ? d.id : d.x;
        }

        function yValue(d) {
            return $scope.lineChart ? d.x : d.y;
        }

        function calculateLocalCharacteristicsSimilarityMatrix() {
            $http.get("/api/LocalCalculationWebApi/CalculateLocalCharacteristicsSimilarityMatrix", {
                params: {
                    taskId: $scope.taskId,
                    aligner: $scope.aligner.Value,
                    distanceCalculator: $scope.distanceCalculator.Value,
                    aggregator: $scope.aggregator.Value
                }
            }).then(function (result) {
                const response = result.data;
                $scope.comparisonMatrix = response.result;
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

        function draw() {
            $scope.fillPoints();

            // removing previous chart and tooltip if any
            d3.select(".chart-tooltip").remove();
            d3.select(".chart-svg").remove();

            // chart size and margin settings
            let margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
            let width = $scope.width - margin.left - margin.right;
            let height = $scope.height - margin.top - margin.bottom;

            // setup x
            // calculating margins for dots
            let xMin = d3.min($scope.points, $scope.xValue);
            let xMax = d3.max($scope.points, $scope.xValue);
            let xMargin = (xMax - xMin) * 0.05;

            let xScale = d3.scaleLinear()
                .domain([xMin - xMargin, xMax + xMargin])
                .range([0, width]);
            let xAxis = d3.axisBottom(xScale)
                .tickSizeInner(-height)
                .tickSizeOuter(0)
                .tickPadding(10);

            $scope.xMap = d => xScale($scope.xValue(d));

            // setup y
            // calculating margins for dots
            let yMax = d3.max($scope.points, $scope.yValue);
            let yMin = d3.min($scope.points, $scope.yValue);
            let yMargin = (yMax - yMin) * 0.05;

            let yScale = d3.scaleLinear()
                .domain([yMin - yMargin, yMax + yMargin])
                .range([height, 0]);
            let yAxis = d3.axisLeft(yScale)
                .tickSizeInner(-width)
                .tickSizeOuter(0)
                .tickPadding(10);

            $scope.yMap = d => yScale($scope.yValue(d));

            // setup fill color
            let color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.legend.length]);

            // add the graph canvas to the body of the webpage
            let svg = d3.select("#chart").append("svg")
                .attr("width", $scope.width)
                .attr("height", $scope.height)
                .attr("class", "chart-svg")
                .append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            // add the tooltip area to the webpage
            let tooltip = d3.select("#chart").append("div")
                .attr("class", "chart-tooltip position-absolute bg-light font-monospace small lh-sm p-1 rounded")
                .style("opacity", 0);

            // preventing tooltip hiding if dot clicked
            tooltip.on("click", () => { tooltip.hideTooltip = false; });

            // hiding tooltip
            d3.select("#chart").on("click", () => { $scope.clearTooltip(tooltip); });

            // x-axis
            svg.append("g")
                .attr("class", "x axis")
                .attr("transform", "translate(0," + height + ")")
                .call(xAxis);

            svg.append("text")
                .attr("class", "label")
                .attr("transform", "translate(" + (width / 2) + " ," + (height + margin.top - $scope.legendHeight) + ")")
                .style("text-anchor", "middle")
                .text($scope.lineChart ? "Fragment №" : $scope.firstCharacteristic.Text)
                .style("font-size", "12pt");

            // y-axis
            svg.append("g")
                .attr("class", "y axis")
                .call(yAxis);

            svg.append("text")
                .attr("class", "label")
                .attr("transform", "rotate(-90)")
                .attr("y", 0 - margin.left)
                .attr("x", 0 - (height / 2))
                .attr("dy", ".71em")
                .style("text-anchor", "middle")
                .text($scope.lineChart ? $scope.firstCharacteristic.Text : $scope.secondCharacteristic.Text)
                .style("font-size", "12pt");

            if ($scope.lineChart) {
                let line = d3.line()
                    .x($scope.xMap)
                    .y($scope.yMap);

                // Group the entries by symbol
                let dataGroups = d3.group($scope.points, d => d.matterName);

                // Loop through each symbol / key
                dataGroups.forEach(value => {
                    svg.append("path")
                        .datum(value)
                        .attr("class", "line")
                        .attr("d", line)
                        .attr("stroke", d => color(d[0].characteristicId))
                        .attr("stroke-width", 1)
                        .attr("fill", "none");
                });
            }

            // draw dots
            svg.selectAll(".dot")
                .data($scope.points)
                .enter()
                .append("ellipse")
                .attr("class", "dot")
                .attr("rx", $scope.dotRadius)
                .attr("ry", $scope.dotRadius)
                .attr("cx", $scope.xMap)
                .attr("cy", $scope.yMap)
                .style("fill-opacity", 0.6)
                .style("opacity", $scope.lineChart ? 0 : 1)
                .style("fill", d => color(d.characteristicId))
                .style("stroke", d => color(d.characteristicId))
                .on("click", (event, d) => $scope.showTooltip(event, d, tooltip, svg));

            // draw legend
            let legend = svg.selectAll(".legend")
                .data($scope.legend)
                .enter()
                .append("g")
                .attr("class", "legend")
                .attr("transform", (_d, i) => "translate(0," + i * 20 + ")")
                .on("click", function (event, d) {
                    d.visible = !d.visible;
                    let legendEntry = d3.select(event.currentTarget);
                    legendEntry.select("text")
                        .style("opacity", () => d.visible ? 1 : 0.5);
                    legendEntry.select("rect")
                        .style("fill-opacity", () => d.visible ? 1 : 0);

                    svg.selectAll(".dot")
                        .filter(dot => dot.matterName === d.name)
                        .attr("visibility", () => d.visible ? "visible" : "hidden");

                    svg.selectAll(".line")
                        .filter(line => line[0].matterName === d.name)
                        .attr("visibility", () => d.visible ? "visible" : "hidden");
                });

            // draw legend colored rectangles
            legend.append("rect")
                .attr("width", 15)
                .attr("height", 15)
                .style("fill", d => color(d.id))
                .style("stroke", d => color(d.id))
                .style("stroke-width", 4)
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")");

            // draw legend text
            legend.append("text")
                .attr("x", 24)
                .attr("y", 9)
                .attr("dy", ".35em")
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")")
                .text(d => d.name)
                .style("font-size", "9pt");
        }

        $scope.calculateLocalCharacteristicsSimilarityMatrix = calculateLocalCharacteristicsSimilarityMatrix;
        $scope.changeCharacteristicsTableVisibility = changeCharacteristicsTableVisibility;
        $scope.draw = draw;
        $scope.fillPoints = fillPoints;
        $scope.fillPointTooltip = fillPointTooltip;
        $scope.showTooltip = showTooltip;
        $scope.clearTooltip = clearTooltip;
        $scope.fillLegend = fillLegend;
        $scope.yValue = yValue;
        $scope.xValue = xValue;

        $scope.width = 800;
        $scope.dotRadius = 4;
        $scope.selectedDotRadius = $scope.dotRadius * 2;

        $scope.loadingScreenHeader = "Loading data";

        let location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $http.get(`/api/TaskManagerWebApi/GetTaskData/${$scope.taskId}`)
            .then(function (data) {
                MapModelFromJson($scope, data.data);

                $scope.fillLegend();

                $scope.firstCharacteristic = $scope.characteristicsList[0];
                $scope.secondCharacteristic = $scope.characteristicsList.length > 1 ? $scope.characteristicsList[1] : $scope.characteristicsList[0];
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
