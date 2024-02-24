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

                $scope.characteristicsList = characteristics.map((c, i) => ({ Value: i+1, Text: c }));
                $scope.firstCharacteristic = $scope.characteristicsList[0];
                $scope.secondCharacteristic = $scope.characteristicsList.length > 1 ? $scope.characteristicsList[1] : $scope.characteristicsList[0];

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

        function fillLegend() {
            $scope.legend = [];
            for (let k = 0; k < $scope.parsedData.length; k++) {
                $scope.legend.push({ id: k, name: $scope.parsedData[k][0], visible: true });
            }
            $scope.legendHeight = $scope.legend.length * 20;
        }

        // initializes data for chart
        function fillPoints() {

            $scope.points = [];

            for (let i = 0; i < $scope.parsedData.length; i++) {
                let characteristic = $scope.parsedData[i];
                $scope.points.push({
                    id: i,
                    name: characteristic[0],
                    x: +characteristic[$scope.firstCharacteristic.Value],
                    y: +characteristic[$scope.secondCharacteristic.Value]
                });
            }
        }


        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            let tooltipContent = [];
            tooltipContent.push("Name: " + d.name);


            let pointsCharacteristics = [];
            for (let i = 0; i < $scope.characteristicsList.length; i++) {
                pointsCharacteristics.push($scope.characteristicsList[i].Text + ": " + $scope.parsedData[d.id][i+1]);
            }

            tooltipContent.push(pointsCharacteristics.join("<br/>"));

            return tooltipContent.join("</br>");
        }

        // shows tooltip for dot or group of dots
        function showTooltip(event, d, tooltip, svg) {
            $scope.clearTooltip(tooltip);

            tooltip.style("opacity", 0.9);

            let tooltipHtml = [];

            tooltip.selectedDots = svg.selectAll(".dot")
                .filter((dot) => {
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
                        tooltip.selectedDots
                            .attr("rx", $scope.dotRadius)
                            .attr("ry", $scope.dotRadius);
                    }
                }

                tooltip.hideTooltip = true;
            }
        }

        function xValue(d) {
            return d.x;
        }

        function yValue(d) {
            return d.y;
        }

        function draw() {
            $scope.fillLegend();
            $scope.fillPoints();

            // removing previous chart and tooltip if any
            d3.select(".chart-tooltip").remove();
            d3.select(".chart-svg").remove();

            let actualLegendHeight = $scope.legendSettings.show ? $scope.legendHeight : 0;

            // chart size and margin settings
            let margin = { top: 30 + actualLegendHeight, right: 30, bottom: 35, left: 50 };
            let width = $scope.width - margin.left - margin.right;
            let height = $scope.height + actualLegendHeight - margin.top - margin.bottom;

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

            $scope.yMap = function (d) { return yScale($scope.yValue(d)); };

            // setup fill color
            let color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.legend.length]);

            // add the graph canvas to the body of the webpage
            let svg = d3.select("#chart").append("svg")
                .attr("width", $scope.width)
                .attr("height", $scope.height + actualLegendHeight)
                .attr("class", "chart-svg")
                .append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            // add the tooltip area to the webpage
            let tooltip = d3.select("#chart").append("div")
                .attr("class", "chart-tooltip position-absolute bg-light font-monospace small lh-sm p-1 rounded")
                .style("opacity", 0);

            // preventing tooltip hiding if dot clicked
            tooltip.on("click", function () { tooltip.hideTooltip = false; });

            // hiding tooltip
            d3.select("#chart").on("click", function () { $scope.clearTooltip(tooltip); });

            // x-axis
            svg.append("g")
                .attr("class", "x axis")
                .attr("transform", "translate(0," + height + ")")
                .call(xAxis);

            svg.append("text")
                .attr("class", "label")
                .attr("transform", "translate(" + (width / 2) + " ," + (height + margin.top - actualLegendHeight) + ")")
                .style("text-anchor", "middle")
                .text($scope.firstCharacteristic.Text)
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
                .text($scope.secondCharacteristic.Text)
                .style("font-size", "12pt");

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
                .style("fill", d => color(d.id))
                .style("stroke", d => color(d.id))
                .on("click", (event, d) => $scope.showTooltip(event, d, tooltip, svg));

            if ($scope.legendSettings.show) {
                // draw legend
                let legend = svg.selectAll(".legend")
                    .data($scope.legend)
                    .enter()
                    .append("g")
                    .attr("class", "legend")
                    .attr("transform", (_d, i) => "translate(0," + i * 20 + ")")
                    .on("click", (event, d) => {
                        d.visible = !d.visible;
                        let legendEntry = d3.select(event.currentTarget);
                        legendEntry.select("text")
                            .style("opacity", () => d.visible ? 1 : 0.5);
                        legendEntry.select("rect")
                            .style("fill-opacity", () => d.visible ? 1 : 0);

                        svg.selectAll(".dot")
                            .filter(dot => dot.cluster === d.name)
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
                    .text(d => ($scope.clustersCount ? "Cluster " : "") + d.name)
                    .style("font-size", "9pt");
            }
        }

        $scope.parseTabularData = parseTabularData;
        $scope.textChanged = textChanged;
        $scope.handleKeyDown = handleKeyDown;
        $scope.draw = draw;
        $scope.fillPoints = fillPoints;
        $scope.fillPointTooltip = fillPointTooltip;
        $scope.showTooltip = showTooltip;
        $scope.clearTooltip = clearTooltip;
        $scope.fillLegend = fillLegend;
        $scope.yValue = yValue;
        $scope.xValue = xValue;

        $scope.width = 800;
        $scope.height = 800;
        $scope.dotRadius = 3;
        $scope.selectedDotRadius = $scope.dotRadius * 2;
        $scope.legendSettings = { show: true };
        $scope.characteristicsTableRendering = { rendered: false };

        $scope.keyCodes = {
            'C': 67,
            'V': 86
        };

        $scope.inFocus = false;
        $scope.rawData = "";
        $scope.parsedData = [];
    }

    angular.module("libiada").controller("ChartsCtrl", ["$scope", "$document", charts]);
}
