function CalculationResultController() {
    "use strict";

    function calculationResult($scope, $http) {

        function fillLegend() {
            $scope.legend = [];
            if ($scope.clustersCount) {
                for (let j = 0; j < $scope.clustersCount; j++) {
                    $scope.legend.push({ id: j + 1, name: j + 1, visible: true });
                }
            } else {
                for (let k = 0; k < $scope.characteristics.length; k++) {
                    $scope.legend.push({ id: k + 1, name: $scope.characteristics[k].MatterName, visible: true });
                }
            }
        }

        $scope.addCharacteristic = () => $scope.chartCharacteristics.push({ id: $scope.chartsCharacterisrticsCount++, value: $scope.characteristicsList[0] });

        $scope.deleteCharacteristic = characteristic => $scope.chartCharacteristics.splice($scope.chartCharacteristics.indexOf(characteristic), 1);

        // initializes data for chart
        function fillPoints() {
            $scope.points = [];

            for (let i = 0; i < $scope.characteristics.length; i++) {
                let characteristic = $scope.characteristics[i];
                $scope.points.push({
                    id: i + 1,
                    name: characteristic.MatterName,
                    characteristics: characteristic.Characteristics,
                    cluster: characteristic.cluster ? characteristic.cluster : characteristic.MatterName
                });
            }
        }


        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            let tooltipContent = [];
            tooltipContent.push("Name: " + d.name);

            let pointData = $scope.characteristics[d.id - 1].Characteristics;
            let pointsCharacteristics = [];
            for (let i = 0; i < pointData.length; i++) {
                pointsCharacteristics.push($scope.characteristicsList[i].Text + ": " + pointData[i]);
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

        function drawBarPlot() {
            let characteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0].value);
            let colorScale = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.legend.length]);
            let data = [{
                x: $scope.points.map(p => p.name),
                y: $scope.points.map(p => p.characteristics[characteristicIndex]),
                marker: { color: $scope.points.map(p => colorScale(p.id)) },
                type: 'bar'
            }];

            Plotly.newPlot('chart', data);
        }

        function drawScatterPlot() {
            $scope.layout = {
                //showlegend: false,
                hovermode: "closest",
                xaxis: {
                    //type: $scope.plotTypeX ? 'log' : '',
                    title: {
                        text: $scope.chartCharacteristics[0].value,
                        font: {
                            family: 'Courier New, monospace',
                            size: 12
                        }
                    }
                },
                yaxis: {
                    //type: $scope.plotTypeY ? 'log' : '',
                    title: {
                        text: $scope.chartCharacteristics[1].value,
                        font: {
                            family: 'Courier New, monospace',
                            size: 12
                        }
                    }
                }
            };

            let firstCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0].value);
            let secondCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[1].value);
            let colorScale = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.legend.length]);
            let data = [{
                hoverinfo: 'text+x+y',
                type: 'scattergl',
                x: $scope.points.map(p => p.characteristics[firstCharacteristicIndex]),
                y: $scope.points.map(p => p.characteristics[secondCharacteristicIndex]),
                text: $scope.points.map(p => p.name),
                mode: "markers",
                marker: { opacity: 0.8, color: $scope.points.map(p => colorScale(p.id) ) },
                name: $scope.points.map(p => p.name)
            }];

            Plotly.newPlot("chart", data, $scope.layout, { responsive: true });

            //$scope.plot.on("plotly_click", data => {
            //    $scope.selectedPointIndex = data.points[0].pointNumber;
            //    $scope.selectedMatterIndex = data.points[0].curveNumber;
            //    let selectedPoint = $scope.points[data.points[0].curveNumber][data.points[0].pointNumber];
            //    //$scope.showTooltip(selectedPoint);
            //});
        }

        function draw3dScatterPlot() {
            let colorScale = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.legend.length]);
            let firstCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0].value);
            let secondCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[1].value);
            let thirdCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[2].value);
            let data = [{
                x: $scope.points.map(p => p.characteristics[firstCharacteristicIndex]),
                y: $scope.points.map(p => p.characteristics[secondCharacteristicIndex]),
                z: $scope.points.map(p => p.characteristics[thirdCharacteristicIndex]),
                text: $scope.points.map(p => p.name),
                mode: "markers",
                marker: {
                    opacity: 0.8, color: $scope.points.map(p => colorScale(p.id))
                    //                    line: {
                    //    color: 'rgba(217, 217, 217, 0.14)',
                    //    width: 0.5
                    //},
                },
                name: $scope.points.map(p => p.name),
                type: 'scatter3d'
            }];
            //var layout = {
            //    margin: {
            //        l: 0,
            //        r: 0,
            //        b: 0,
            //        t: 0
            //    }
            //};
            Plotly.newPlot('chart', data);
        }

        function drawParallelCoordinatesPlot() {


            let data = [{
                type: 'parcoords',
                //pad: [80, 80, 80, 80],
                //line: {
                //    color: unpack(rows, 'species_id'),
                //    colorscale: [[0, 'red'], [0.5, 'green'], [1, 'blue']]
                //},

                dimensions: $scope.chartCharacteristics.map(c => ({
                    label: c.value.Text,
                    values: $scope.points.map(p => p.characteristics[$scope.characteristicsList.indexOf(c.value)] )
                }))
                //    [{
                //    range: [2, 4.5],
                //    label: 'sepal_width',
                //    values: unpack(rows, 'sepal_width')
                //}, {
                //    constraintrange: [5, 6],
                //    range: [4, 8],
                //    label: 'sepal_length',
                //    values: unpack(rows, 'sepal_length')
                //}, {
                //    label: 'petal_width',
                //    range: [0, 2.5],
                //    values: unpack(rows, 'petal_width')
                //}, {
                //    label: 'petal_length',
                //    range: [1, 7],
                //    values: unpack(rows, 'petal_length')
                //}]
            }];

            let layout = {
                width: $scope.width
            };

            Plotly.newPlot('chart', data, layout);
        }

        function draw() {
            $scope.fillPoints();

            switch ($scope.chartCharacteristics.length) {
                case 1:
                    $scope.drawBarPlot();
                    break;
                case 2:
                    $scope.drawScatterPlot();
                    break;
                case 3:
                    $scope.draw3dScatterPlot();
                    break;
                default:
                    $scope.drawParallelCoordinatesPlot();
            }



            //// removing previous chart and tooltip if any
            //d3.select(".chart-tooltip").remove();
            //d3.select(".chart-svg").remove();

            //let actualLegendHeight = $scope.legendSettings.show ? $scope.legendHeight : 0;

            //// chart size and margin settings
            //let margin = { top: 30 + actualLegendHeight, right: 30, bottom: 35, left: 50 };
            //let width = $scope.width - margin.left - margin.right;
            //let height = $scope.height + actualLegendHeight - margin.top - margin.bottom;

            //// setup x
            //// calculating margins for dots
            //let xMin = d3.min($scope.points, $scope.xValue);
            //let xMax = d3.max($scope.points, $scope.xValue);
            //let xMargin = (xMax - xMin) * 0.05;

            //let xScale = d3.scaleLinear()
            //    .domain([xMin - xMargin, xMax + xMargin])
            //    .range([0, width]);
            //let xAxis = d3.axisBottom(xScale)
            //    .tickSizeInner(-height)
            //    .tickSizeOuter(0)
            //    .tickPadding(10);

            //$scope.xMap = d => xScale($scope.xValue(d));

            //// setup y
            //// calculating margins for dots
            //let yMax = d3.max($scope.points, $scope.yValue);
            //let yMin = d3.min($scope.points, $scope.yValue);
            //let yMargin = (yMax - yMin) * 0.05;

            //let yScale = d3.scaleLinear()
            //    .domain([yMin - yMargin, yMax + yMargin])
            //    .range([height, 0]);
            //let yAxis = d3.axisLeft(yScale)
            //    .tickSizeInner(-width)
            //    .tickSizeOuter(0)
            //    .tickPadding(10);

            //$scope.yMap = function (d) { return yScale($scope.yValue(d)); };

            //// setup fill color
            //let color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.legend.length]);

            //// add the graph canvas to the body of the webpage
            //let svg = d3.select("#chart").append("svg")
            //    .attr("width", $scope.width)
            //    .attr("height", $scope.height + actualLegendHeight)
            //    .attr("class", "chart-svg")
            //    .append("g")
            //    .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            //// add the tooltip area to the webpage
            //let tooltip = d3.select("#chart").append("div")
            //    .attr("class", "chart-tooltip position-absolute bg-light font-monospace small lh-sm p-1 rounded")
            //    .style("opacity", 0);

            //// preventing tooltip hiding if dot clicked
            //tooltip.on("click", function () { tooltip.hideTooltip = false; });

            //// hiding tooltip
            //d3.select("#chart").on("click", function () { $scope.clearTooltip(tooltip); });

            //// x-axis
            //svg.append("g")
            //    .attr("class", "x axis")
            //    .attr("transform", "translate(0," + height + ")")
            //    .call(xAxis);

            //svg.append("text")
            //    .attr("class", "label")
            //    .attr("transform", "translate(" + (width / 2) + " ," + (height + margin.top - actualLegendHeight) + ")")
            //    .style("text-anchor", "middle")
            //    .text($scope.firstCharacteristic.Text)
            //    .style("font-size", "12pt");

            //// y-axis
            //svg.append("g")
            //    .attr("class", "y axis")
            //    .call(yAxis);

            //svg.append("text")
            //    .attr("class", "label")
            //    .attr("transform", "rotate(-90)")
            //    .attr("y", 0 - margin.left)
            //    .attr("x", 0 - (height / 2))
            //    .attr("dy", ".71em")
            //    .style("text-anchor", "middle")
            //    .text($scope.secondCharacteristic.Text)
            //    .style("font-size", "12pt");

            //// draw dots
            //svg.selectAll(".dot")
            //    .data($scope.points)
            //    .enter()
            //    .append("ellipse")
            //    .attr("class", "dot")
            //    .attr("rx", $scope.dotRadius)
            //    .attr("ry", $scope.dotRadius)
            //    .attr("cx", $scope.xMap)
            //    .attr("cy", $scope.yMap)
            //    .style("fill-opacity", 0.6)
            //    .style("fill", d => color($scope.clustersCount ? d.cluster : d.id))
            //    .style("stroke", d => color($scope.clustersCount ? d.cluster : d.id))
            //    .on("click", (event, d) => $scope.showTooltip(event, d, tooltip, svg));

            //if ($scope.legendSettings.show) {
            //    // draw legend
            //    let legend = svg.selectAll(".legend")
            //        .data($scope.legend)
            //        .enter()
            //        .append("g")
            //        .attr("class", "legend")
            //        .attr("transform", (_d, i) => "translate(0," + i * 20 + ")")
            //        .on("click", (event, d) => {
            //            d.visible = !d.visible;
            //            let legendEntry = d3.select(event.currentTarget);
            //            legendEntry.select("text")
            //                .style("opacity", () => d.visible ? 1 : 0.5);
            //            legendEntry.select("rect")
            //                .style("fill-opacity", () => d.visible ? 1 : 0);

            //            svg.selectAll(".dot")
            //                .filter(dot => dot.cluster === d.name)
            //                .attr("visibility", () => d.visible ? "visible" : "hidden");
            //        });

            //    // draw legend colored rectangles
            //    legend.append("rect")
            //        .attr("width", 15)
            //        .attr("height", 15)
            //        .style("fill", d => color(d.id))
            //        .style("stroke", d => color(d.id))
            //        .style("stroke-width", 4)
            //        .attr("transform", "translate(0, -" + $scope.legendHeight + ")");

            //    // draw legend text
            //    legend.append("text")
            //        .attr("x", 24)
            //        .attr("y", 9)
            //        .attr("dy", ".35em")
            //        .attr("transform", "translate(0, -" + $scope.legendHeight + ")")
            //        .text(d => $scope.clustersCount ? `Cluster ${d.name}` : d.name)
            //        .style("font-size", "9pt");
            //}

            $scope.chartDisplayed = true;
        }

        function legendSetVisibilityForAll(visibility) {
            let legend = d3.selectAll(".legend");

            legend.each(l => l.visible = visibility);

            legend.selectAll("rect")
                .style("fill-opacity", () => visibility ? 1 : 0);

            legend.selectAll("text")
                .style("opacity", () => visibility ? 1 : 0.5);

            d3.select(".chart-svg")
                .selectAll(".dot")
                .attr("visibility", () => visibility ? "visible" : "hidden");
        }

        async function exportToExcel() {

            const workbook = new ExcelJS.Workbook();
            const worksheet = workbook.addWorksheet("My Sheet");
            const font = { name: 'Courier New' };
            const border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } }
            let columns = [
                { header: '№', key: 'id', width: 10, style: { font: font, border: border } },
                { header: 'Sequence name', key: 'name', width: 32, style: { font: font, border: border } }
            ];

            columns = columns.concat($scope.characteristicNames.map(cn => ({ header: cn, key: cn, width: 20, style: { font: font, border: border } })));

            worksheet.columns = columns;

            for (let i = 0; i < $scope.characteristics.length; i++) {
                let row = { id: i + 1, name: $scope.characteristics[i].MatterName };
                $scope.characteristics[i].Characteristics.forEach((cv, j) => row[$scope.characteristicNames[j]] = cv);
                worksheet.addRow(row).commit();
            }

            const buffer = await workbook.xlsx.writeBuffer();

            const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8' });

            // TODO: rewrite it using browser file API
            let saveBlobAsFile = function (blob) {
                let a = document.createElement("a");
                document.body.appendChild(a);
                a.style = "display: none";
                let url = window.URL.createObjectURL(blob);
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
        
        $scope.drawBarPlot = drawBarPlot;
        $scope.drawScatterPlot = drawScatterPlot;
        $scope.draw3dScatterPlot = draw3dScatterPlot;
        $scope.drawParallelCoordinatesPlot = drawParallelCoordinatesPlot;
        $scope.draw = draw;
        $scope.fillPoints = fillPoints;
        $scope.fillPointTooltip = fillPointTooltip;
        $scope.showTooltip = showTooltip;
        $scope.clearTooltip = clearTooltip;
        $scope.fillLegend = fillLegend;
        $scope.yValue = yValue;
        $scope.xValue = xValue;
        $scope.legendSetVisibilityForAll = legendSetVisibilityForAll;
        $scope.exportToExcel = exportToExcel;
        $scope.renderResultsTable = renderResultsTable;

        $scope.width = 800;
        $scope.height = 800;
        $scope.dotRadius = 4;
        $scope.selectedDotRadius = $scope.dotRadius * 2;
        $scope.legendSettings = { show: true };
        $scope.characteristicsTableRendering = { rendered: false };
        $scope.chartDisplayed = false;
        $scope.chartsCharacterisrticsCount = 1;

        $scope.loadingScreenHeader = "Loading data";

        let location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $scope.loading = true;

        $http.get(`/api/TaskManagerWebApi/GetTaskData/${$scope.taskId}`)
            .then(function (data) {
                MapModelFromJson($scope, data.data);

                $scope.fillLegend();

                $scope.chartCharacteristics = [{ id: $scope.chartsCharacterisrticsCount++, value: $scope.characteristicsList[0] }];

                $scope.legendHeight = $scope.legend.length * 20;

                $scope.loading = false;
            }, function () {
                alert("Failed loading characteristic data");
                $scope.loading = false;
            });
    }

    angular.module("libiada").controller("CalculationResultCtrl", ["$scope", "$http", calculationResult]);
}
