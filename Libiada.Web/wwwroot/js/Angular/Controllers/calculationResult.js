function CalculationResultController() {
    "use strict";

    function calculationResult($scope, $http) {

        function fillLegend() {
            $scope.legend = [];
            if ($scope.clustersCount) {
                $scope.colorScale = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.clustersCount]);
                for (let j = 0; j < $scope.clustersCount; j++) {
                    $scope.legend.push({ id: j + 1, name: j + 1, visible: true });

                    // hack for the legend's dot color
                    document.styleSheets[0].insertRule(".legend" + (j + 1) + ":after { background:" + $scope.colorScale(j) + "}");

                }
            } else {
                $scope.colorScale = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.characteristics.length]);
                for (let k = 0; k < $scope.characteristics.length; k++) {
                    $scope.legend.push({ id: k + 1, name: $scope.characteristics[k].MatterName, visible: true });

                    // hack for the legend's dot color
                    document.styleSheets[0].insertRule(".legend" + (k + 1) + ":after { background:" + $scope.colorScale(k) + "}");
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

        function fillBarPlotData() {
            let characteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0].value);
            let min = Math.min(...$scope.points.map(p => p.characteristics[characteristicIndex]));
            let max = Math.max(...$scope.points.map(p => p.characteristics[characteristicIndex]));
            let range = Math.abs(max - min);
            let maxNameLength = Math.max(...$scope.points.map(p => p.name.length));
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
                xaxis: { categoryorder: 'total ascending' },
                yaxis: {
                    range: [min,max]
                },

            };

            $scope.chartData = $scope.points.map(p => ({
                hoverinfo: 'text+x+y',
                x: [p.name],
                y: [p.characteristics[characteristicIndex]],
                marker: { color: $scope.colorScale(p.id) },
                type: 'bar',
                customdata: { id: p.id },
                name: p.name,
                visible: $scope.legend.find(l => l.id == p.id).visible ? "true" : "legendonly" 
            }));
        }

        function fillScatterPlotData() {
            $scope.layout = {
                margin: {
                    l: 50,
                    r: 20,
                    t: 30,
                    b: 20

                },
                showlegend: false,
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

            $scope.chartData = $scope.points.map(p => ({
                hoverinfo: 'text+x+y',
                type: 'scattergl',
                x: [p.characteristics[firstCharacteristicIndex]],
                y: [p.characteristics[secondCharacteristicIndex]],
                text: p.name,
                mode: "markers",
                marker: { opacity: 0.8, color: $scope.colorScale(p.id) },
                name: p.name,
                customdata: { id: p.id },
                visible: $scope.legend.find(l => l.id == p.id).visible
            }));


            //$scope.plot.on("plotly_click", data => {
            //    $scope.selectedPointIndex = data.points[0].pointNumber;
            //    $scope.selectedMatterIndex = data.points[0].curveNumber;
            //    let selectedPoint = $scope.points[data.points[0].curveNumber][data.points[0].pointNumber];
            //    //$scope.showTooltip(selectedPoint);
            //});
        }

        function fill3dScatterPlotData() {

            let firstCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0].value);
            let secondCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[1].value);
            let thirdCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[2].value);
            $scope.chartData = $scope.points.map(p => ({
                hoverinfo: 'text+x+y+z',
                x: [p.characteristics[firstCharacteristicIndex]],
                y: [p.characteristics[secondCharacteristicIndex]],
                z: [p.characteristics[thirdCharacteristicIndex]],
                text: p.name,
                mode: "markers",
                marker: {
                    opacity: 0.8, color: $scope.colorScale(p.id)
                    //                    line: {
                    //    width: 0.5
                    //},
                },
                name: p.name,
                type: 'scatter3d',
                customdata: { id: p.id },
                visible: $scope.legend.find(l => l.id == p.id).visible
            }));
            $scope.layout = {
                margin: {
                    l: 0,
                    r: 0,
                    b: 0,
                    t: 0
                },
                showlegend: false
            };
        }

        function fillParallelCoordinatesPlotData() {
            $scope.chartData = [{
                type: 'parcoords',
                //pad: [80, 80, 80, 80],
                line: {
                    color: $scope.points.map(p => p.id),
                    colorscale: 'Turbo'
                },

                dimensions: $scope.chartCharacteristics.map(c => ({
                    label: c.value.Text,
                    values: $scope.points.map(p => p.characteristics[$scope.characteristicsList.indexOf(c.value)])
                }))
                //    [{
                //    range: [2, 4.5],
                //    label: 'sepal_width',
                //    values: unpack(rows, 'sepal_width')
                //}]
            }];

            $scope.layout = {
                margin: {
                    l: 50,
                    r: 50,
                    b: 20,
                    t: 70
                },
               // width: $scope.width,
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

            Plotly.newPlot($scope.chartElementId, $scope.chartData, $scope.layout, { responsive: true });
            $scope.chartDisplayed = true;
        }

        function legendClick(legendItem) {
            if ($scope.chartData && $scope.chartData[0].customdata) {
                let index;
                let update = { visible: legendItem.visible ? "legendonly" : true  };
                for (let i = 0; i < $scope.chartData.length; i++) {
                    if ($scope.chartData[i].customdata.id == legendItem.id) {
                        index = i;
                        break;
                    }
                }

                Plotly.restyle($scope.chartElementId, update, index);
            }
            
        }

        function legendSetVisibilityForAll(visibility) {
            if ($scope.chartData && $scope.chartData[0].customdata) {
                let update = { visible: visibility ? true : "legendonly" };
                $scope.legend.forEach(l => l.visible = visibility);
                Plotly.restyle($scope.chartElementId, update);
            }
        }

        function dragbarMouseDown() {
            let chart = document.getElementById($scope.chartElementId);
            let right = document.getElementById('sidebar');
            let bar = document.getElementById('dragbar');

            const drag = (e) => {
                document.selection ? document.selection.empty() : window.getSelection().removeAllRanges();
                chart.style.width = (e.pageX - bar.offsetWidth / 2) + 'px';

                Plotly.relayout($scope.chartElementId, { autosize: true });
            };

            bar.addEventListener('mousedown', () => {
                document.addEventListener('mousemove', drag);
            });

            bar.addEventListener('mouseup', () => {
                document.removeEventListener('mousemove', drag);
            });
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

        $scope.fillBarPlotData = fillBarPlotData;
        $scope.fillScatterPlotData = fillScatterPlotData;
        $scope.fill3dScatterPlotData = fill3dScatterPlotData;
        $scope.fillParallelCoordinatesPlotData = fillParallelCoordinatesPlotData;
        $scope.draw = draw;
        $scope.fillPoints = fillPoints;
        $scope.fillPointTooltip = fillPointTooltip;
        $scope.showTooltip = showTooltip;
        $scope.clearTooltip = clearTooltip;
        $scope.fillLegend = fillLegend;
        $scope.legendClick = legendClick;
        $scope.legendSetVisibilityForAll = legendSetVisibilityForAll;
        $scope.dragbarMouseDown = dragbarMouseDown;
        $scope.exportToExcel = exportToExcel;
        $scope.renderResultsTable = renderResultsTable;

        $scope.dotRadius = 4;
        $scope.selectedDotRadius = $scope.dotRadius * 2;
        $scope.chartElementId = "chart";
        $scope.legendSettings = { show: true };
        $scope.characteristicsTableRendering = { rendered: false };
        $scope.chartDisplayed = false;
        $scope.chartsCharacterisrticsCount = 1;

        $scope.loadingScreenHeader = "Loading data";

        let location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        // initialyzing tooltips for tabs
        $('[data-bs-toggle="tooltip"]').tooltip();

        $scope.loading = true;

        $http.get(`/api/TaskManagerWebApi/GetTaskData/${$scope.taskId}`)
            .then(function (data) {
                MapModelFromJson($scope, data.data);

                $scope.fillLegend();

                $scope.chartCharacteristics = [{ id: $scope.chartsCharacterisrticsCount++, value: $scope.characteristicsList[0] }];

                $scope.loading = false;
            }, function () {
                alert("Failed loading characteristic data");
                $scope.loading = false;
            });
    }

    angular.module("libiada").controller("CalculationResultCtrl", ["$scope", "$http", calculationResult]);
}
