function CalculationResultController() {
    "use strict";

    function calculationResult($scope, $http) {

        function fillLegend() {
            $scope.legend = [];
            if ($scope.clustersCount) {
                for (var j = 0; j < $scope.clustersCount; j++) {
                    $scope.legend.push({ id: j, name: j + 1, visible: true });
                }
            } else {
                for (var k = 0; k < $scope.characteristics.length; k++) {
                    $scope.legend.push({ id: k, name: $scope.characteristics[k].MatterName, visible: true });
                }
            }
        }

        // initializes data for chart
        function fillPoints() {
            $scope.points = [];
            var first = +$scope.firstCharacteristic.Value;
            var second = +$scope.secondCharacteristic.Value;

            for (var i = 0; i < $scope.characteristics.length; i++) {
                var characteristic = $scope.characteristics[i];
                $scope.points.push({
                    id: i,
                    name: characteristic.MatterName,
                    x: characteristic.Characteristics[first],
                    y: characteristic.Characteristics[second],
                    cluster: characteristic.cluster ? characteristic.cluster : characteristic.MatterName
                });
            }
        }


        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            var tooltipContent = [];
            tooltipContent.push("Name: " + d.name);


            var pointsCharacteristics = [];
            for (var i = 0; i < $scope.characteristics[d.id].Characteristics.length; i++) {
                pointsCharacteristics.push($scope.characteristicsList[i].Text + ": " + $scope.characteristics[d.id].Characteristics[i]);
            }

            tooltipContent.push(pointsCharacteristics.join("<br/>"));

            return tooltipContent.join("</br>");
        }

        // shows tooltip for dot or group of dots
        function showTooltip(event, d, tooltip, svg) {
            $scope.clearTooltip(tooltip);

            tooltip.style("opacity", 0.9);

            var tooltipHtml = [];

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
                        tooltip.selectedDots.attr("rx", $scope.dotRadius)
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
            $scope.fillPoints();

            // removing previous chart and tooltip if any
            d3.select(".chart-tooltip").remove();
            d3.select(".chart-svg").remove();

            var actualLegendHeight = $scope.legendSettings.show ? $scope.legendHeight : 0;

            // chart size and margin settings
            var margin = { top: 30 + actualLegendHeight, right: 30, bottom: 35, left: 50 };
            var width = $scope.width - margin.left - margin.right;
            var height = $scope.height + actualLegendHeight - margin.top - margin.bottom;

            // setup x
            // calculating margins for dots
            var xMin = d3.min($scope.points, $scope.xValue);
            var xMax = d3.max($scope.points, $scope.xValue);
            var xMargin = (xMax - xMin) * 0.05;

            var xScale = d3.scaleLinear()
                .domain([xMin - xMargin, xMax + xMargin])
                .range([0, width]);
            var xAxis = d3.axisBottom(xScale)
                .tickSizeInner(-height)
                .tickSizeOuter(0)
                .tickPadding(10);

            $scope.xMap = d => xScale($scope.xValue(d));

            // setup y
            // calculating margins for dots
            var yMax = d3.max($scope.points, $scope.yValue);
            var yMin = d3.min($scope.points, $scope.yValue);
            var yMargin = (yMax - yMin) * 0.05;

            var yScale = d3.scaleLinear()
                .domain([yMin - yMargin, yMax + yMargin])
                .range([height, 0]);
            var yAxis = d3.axisLeft(yScale)
                .tickSizeInner(-width)
                .tickSizeOuter(0)
                .tickPadding(10);

            $scope.yMap = function (d) { return yScale($scope.yValue(d)); };

            // setup fill color
            var color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.legend.length]);

            // add the graph canvas to the body of the webpage
            var svg = d3.select("#chart").append("svg")
                .attr("width", $scope.width)
                .attr("height", $scope.height + actualLegendHeight)
                .attr("class", "chart-svg")
                .append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            // add the tooltip area to the webpage
            var tooltip = d3.select("#chart").append("div")
                .attr("class", "chart-tooltip position-absolute text-bg-light font-monospace small lh-sm p-1 rounded")
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
                var legend = svg.selectAll(".legend")
                    .data($scope.legend)
                    .enter()
                    .append("g")
                    .attr("class", "legend")
                    .attr("transform", (_d, i) => "translate(0," + i * 20 + ")")
                    .on("click", (event, d) => {
                        d.visible = !d.visible;
                        var legendEntry = d3.select(event.currentTarget);
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

        async function exportToExcel() {

            const workbook = new ExcelJS.Workbook();
            const worksheet = workbook.addWorksheet("My Sheet");
            const font = { name: 'Courier New' };
            const border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } }
            var columns = [
                { header: '№', key: 'id', width: 10, style: { font: font, border: border } },
                { header: 'Sequence name', key: 'name', width: 32, style: { font: font, border: border } }
            ];

            columns = columns.concat($scope.characteristicNames.map(cn => ({ header: cn, key: cn, width: 20, style: { font: font, border: border } })));

            worksheet.columns = columns;

            for (var i = 0; i < $scope.characteristics.length; i++) {
                var row = { id: i + 1, name: $scope.characteristics[i].MatterName };
                $scope.characteristics[i].Characteristics.forEach((cv, j) => row[$scope.characteristicNames[j]] = cv);
                worksheet.addRow(row).commit();
            }

            const buffer = await workbook.xlsx.writeBuffer();

            const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8' });

            // TODO: rewrite it using browser file API
            var saveBlobAsFile = function (blob) {
                var a = document.createElement("a");
                document.body.appendChild(a);
                a.style = "display: none";
                var url = window.URL.createObjectURL(blob);
                a.href = url;
                a.download = $scope.excelFileName || "Results";
                a.click();
                window.URL.revokeObjectURL(url);
                a.remove();
            };

            saveBlobAsFile(blob);
        }

        function renderResultsTable() {
            if (!$scope.characteristicsTableRendering.rendered) {
                if ($("#calculationResults").length > 0) {
                    $("#calculationResults").append($scope.characteristics.map((c, i) =>
                        `<tr id="resultRow${i}">
                        <td>${i + 1}</td>
                        <td>${c.MatterName}</td>
                        ${c.Characteristics.map(c => `<td>${c}</td>`).join()}`
                    ).join());
                }
                $scope.characteristicsTableRendering.rendered = true;
            }
        }

        $scope.draw = draw;
        $scope.fillPoints = fillPoints;
        $scope.fillPointTooltip = fillPointTooltip;
        $scope.showTooltip = showTooltip;
        $scope.clearTooltip = clearTooltip;
        $scope.fillLegend = fillLegend;
        $scope.yValue = yValue;
        $scope.xValue = xValue;
        $scope.exportToExcel = exportToExcel;
        $scope.renderResultsTable = renderResultsTable;

        $scope.width = 800;
        $scope.dotRadius = 3;
        $scope.selectedDotRadius = $scope.dotRadius * 2;
        $scope.legendSettings = { show: true };
        $scope.characteristicsTableRendering = { rendered: false };

        $scope.loadingScreenHeader = "Loading data";

        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $scope.loading = true;

        $http.get(`/api/TaskManagerWebApi/${$scope.taskId}`)
            .then(function (data) {
                MapModelFromJson($scope, JSON.parse(data.data));

                $scope.fillLegend();

                $scope.firstCharacteristic = $scope.characteristicsList[0];
                $scope.secondCharacteristic = $scope.characteristicsList.length > 1 ? $scope.characteristicsList[1] : $scope.characteristicsList[0];

                $scope.legendHeight = $scope.legend.length * 20;
                $scope.height = 800;

                $scope.loading = false;
            }, function () {
                alert("Failed loading characteristic data");
                $scope.loading = false;
            });
    }

    angular.module("libiada").controller("CalculationResultCtrl", ["$scope", "$http", calculationResult]);
}
