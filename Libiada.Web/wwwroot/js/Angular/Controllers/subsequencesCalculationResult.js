/// <reference types="angular" />
/// <reference types="d3" />
/// <reference types="plotly.js" />
/// <reference types="jquery" />
/// <reference path="./Interfaces/commonInterfaces.d.ts" />
/// <reference path="./Interfaces/subsequencesCalculationInterfaces.d.ts" />
/// <reference path="../../typings/bootstrap-jquery-extensions.d.ts" />
/// <reference path="../../typings/plotly-extensions.d.ts" />
/**
 * Controller for displaying subsequences calculation results
 */
class SubsequencesCalculationResultHandler {
    /**
     * Creates a new instance of the controller
     */
    constructor() {
        this.initializeController();
    }
    /**
     * Initializes the Angular controller
     */
    initializeController() {
        "use strict";
        const subsequencesCalculationResult = ($scope, $http, $sce) => {
            function fillLegend() {
                $scope.legend = [];
                $scope.colorScale = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.sequencesData.length]);
                for (let k = 0; k < $scope.sequencesData.length; k++) {
                    const color = $scope.colorScale(k + 1);
                    $scope.legend.push({
                        id: k + 1,
                        name: $scope.sequencesData[k].ResearchObjectName,
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
            $scope.deleteCharacteristic = (characteristic) => $scope.chartCharacteristics.splice($scope.chartCharacteristics.indexOf(characteristic), 1);
            // initializes data for chart
            function fillPoints() {
                $scope.points = [];
                for (let i = 0; i < $scope.sequencesData.length; i++) {
                    let sequenceData = $scope.sequencesData[i];
                    const legendIndex = i;
                    $scope.points.push({
                        legendIndex: i,
                        legendId: i + 1,
                        researchObjectName: sequenceData.ResearchObjectName,
                        name: sequenceData.ResearchObjectName,
                        researchObjectId: sequenceData.ResearchObjectId,
                        sequenceRemoteId: sequenceData.RemoteId,
                        subsequencesData: [],
                        id: i
                    });
                    for (let j = 0; j < sequenceData.SubsequencesData.length; j++) {
                        let subsequenceData = sequenceData.SubsequencesData[j];
                        let point = {
                            id: subsequenceData.Id,
                            legendIndex: i,
                            name: sequenceData.ResearchObjectName,
                            attributes: subsequenceData.Attributes,
                            partial: subsequenceData.Partial,
                            featureId: subsequenceData.FeatureId,
                            positions: subsequenceData.Starts,
                            lengths: subsequenceData.Lengths,
                            subsequenceRemoteId: subsequenceData.RemoteId,
                            rank: j + 1,
                            characteristics: subsequenceData.CharacteristicsValues,
                            featureVisible: true,
                            legendVisible: true,
                            filtersVisible: []
                        };
                        $scope.points[i].subsequencesData.push(point);
                    }
                }
            }
            // shows tooltip for dot or group of dots
            function showTooltip(selectedTrace) {
                $("button[data-bs-target='#tooltip-tab-pane']").tab("show");
                let selectedPoint = selectedTrace.subsequencesData[$scope.selectedPointIndex];
                $scope.tooltipElements.length = 0;
                let researchObjectName = selectedTrace.researchObjectName;
                $scope.tooltipElements.push(fillPointTooltip(selectedPoint, researchObjectName, $scope.pointsSimilarity.same));
                let similarPoints = [];
                for (let i = 0; i < $scope.points.length; i++) {
                    for (let j = 0; j < $scope.points[i].subsequencesData.length; j++) {
                        if (selectedPoint !== $scope.points[i].subsequencesData[j]) {
                            let similar = $scope.chartCharacteristics.every(filter => {
                                let characteristicIndex = $scope.characteristicsList.indexOf(filter.value);
                                let selectedPointValue = selectedPoint.characteristics[characteristicIndex];
                                let anotherPointValue = $scope.points[i].subsequencesData[j].characteristics[characteristicIndex];
                                return selectedPointValue == anotherPointValue;
                            });
                            if (similar) {
                                let point = $scope.points[i].subsequencesData[j];
                                similarPoints.push(point);
                                researchObjectName = $scope.points[i].researchObjectName;
                                $scope.tooltipElements.push(fillPointTooltip(point, researchObjectName, true));
                            }
                        }
                    }
                }
                let update = {};
                switch ($scope.chartCharacteristics.length) {
                    case 1:
                    case 2:
                        update = {
                            "marker.symbol": $scope.points.map(t => t.subsequencesData.map(p => p === selectedPoint || similarPoints.includes(p) ? "diamond-wide" : "circle")),
                            "marker.size": $scope.points.map(t => t.subsequencesData.map(p => p === selectedPoint || similarPoints.includes(p) ? 10 : $scope.pointSize * 1.5))
                        };
                        break;
                    case 3:
                        break;
                    default:
                }
                Plotly.restyle($scope.chartElement, update);
                $scope.$apply();
            }
            // constructs string representing tooltip text (inner html)
            function fillPointTooltip(point, researchObjectName, similarity) {
                let color = similarity === $scope.pointsSimilarity.same ? "default"
                    : similarity === $scope.pointsSimilarity.similar ? "success"
                        : similarity === $scope.pointsSimilarity.different ? "danger" : "danger";
                let tooltipElement = {
                    id: point.id,
                    name: researchObjectName,
                    sequenceRemoteId: point.sequenceRemoteId,
                    feature: $scope.features[point.featureId].Text,
                    attributes: $scope.getAttributesText(point.attributes),
                    partial: point.partial,
                    color: color,
                    characteristics: point.characteristics,
                    similarity: similarity,
                    selectedForAlignment: false,
                    position: "(",
                    length: 0,
                    positions: point.positions,
                    lengths: point.lengths
                };
                if (point.subsequenceRemoteId) {
                    tooltipElement.remoteId = point.subsequenceRemoteId;
                }
                for (let i = 0; i < point.positions.length; i++) {
                    tooltipElement.position += point.positions[i] + 1;
                    tooltipElement.position += "..";
                    tooltipElement.position += point.positions[i] + point.lengths[i];
                    tooltipElement.length += point.lengths[i];
                    if (i !== point.positions.length - 1) {
                        tooltipElement.position += ", ";
                    }
                }
                tooltipElement.position += ")";
                return tooltipElement;
            }
            function fillLinePlotData() {
                let characteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0].value);
                let characteristicsValues = $scope.points.map((p => p.subsequencesData.map(fd => fd.characteristics[characteristicIndex]))).flat();
                let min = Math.min(...characteristicsValues);
                let max = Math.max(...characteristicsValues);
                let range = Math.abs(max - min);
                // adding margins
                min -= Math.abs(range * 0.05);
                max += Math.abs(range * 0.05);
                let ranks = [];
                if ($scope.lineChart) {
                    for (let i = 0; i < $scope.points.length; i++) {
                        let y = $scope.points[i].subsequencesData.map(sd => sd.characteristics[characteristicIndex]);
                        y.sort((first, second) => second - first);
                        ranks.push({
                            //x is range from 1 to subsequencesData length
                            x: Array.from({ length: y.length }, (x, i) => i + 1),
                            y: y
                        });
                    }
                }
                else {
                    for (let i = 0; i < $scope.points.length; i++) {
                        ranks.push({
                            x: $scope.points[i].subsequencesData.map(sd => sd.rank),
                            y: $scope.points[i].subsequencesData.map(sd => sd.characteristics[characteristicIndex])
                        });
                    }
                }
                $scope.layout = {
                    margin: {
                        l: 50,
                        r: 20,
                        t: 10,
                        b: 20
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
            $scope.pointSize = 3;
                $scope.chartData = $scope.points.map((p, i) => ({
                    hoverinfo: "text+x+y",
                    x: ranks[i].x,
                    y: ranks[i].y,
                    marker: {
                        color: $scope.legend[p.legendIndex].color,
                        size: $scope.pointSize,
                        opacity: 0.8
                    },
                    type: "scatter",
                    mode: "markers",
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
            $scope.pointSize = 5;
                $scope.chartData = $scope.points.map(p => ({
                    hoverinfo: "text+x+y",
                    type: "scattergl",
                    x: p.subsequencesData.map(sd => sd.characteristics[firstCharacteristicIndex]),
                    y: p.subsequencesData.map(sd => sd.characteristics[secondCharacteristicIndex]),
                    text: p.name,
                    mode: "markers",
                    marker: {
                        opacity: 0.8,
                        color: $scope.legend[p.legendIndex].color,
                        size: $scope.pointSize
                    },
                    name: p.name,
                    customdata: { legendId: p.legendId },
                    visible: $scope.legend[p.legendIndex].visible
                }));
            }
            function fill3dScatterPlotData() {
                let firstCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[0].value);
                let secondCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[1].value);
                let thirdCharacteristicIndex = $scope.characteristicsList.indexOf($scope.chartCharacteristics[2].value);
            $scope.pointSize = 5;
                $scope.chartData = $scope.points.map(p => ({
                    hoverinfo: "text+x+y+z",
                    x: p.subsequencesData.map(sd => sd.characteristics[firstCharacteristicIndex]),
                    y: p.subsequencesData.map(sd => sd.characteristics[secondCharacteristicIndex]),
                    z: p.subsequencesData.map(sd => sd.characteristics[thirdCharacteristicIndex]),
                    text: p.name,
                    mode: "markers",
                    marker: {
                        opacity: 0.8,
                        color: $scope.legend[p.legendIndex].color,
                        size: $scope.pointSize
                    },
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
                        line: {
                            color: $scope.points.map(p => p.subsequencesData.map(sd => sd.legendIndex)).flat(),
                            colorscale: "Turbo"
                        },
                        dimensions: characteristicsIndices.map(ci => ({
                            label: $scope.characteristicNames[ci],
                            values: $scope.points.map(p => p.subsequencesData.map(sd => sd.characteristics[ci])).flat()
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
                $scope.chartElement.on("plotly_click", (data) => {
                    $scope.selectedPointIndex = data.points[0].pointNumber;
                    $scope.selectedResearchObjectIndex = data.points[0].curveNumber;
                    let selectedTrace = $scope.points[data.points[0].curveNumber];
                    $scope.showTooltip(selectedTrace);
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
                    document.selection ? document.selection.empty() : window.getSelection()?.removeAllRanges();
                    $scope.chartElement.style.width = `${e.pageX - bar.offsetWidth / 2}px`;
                    Plotly.relayout($scope.chartElement, { autosize: true });
                };
                bar?.addEventListener("mousedown", () => {
                    document.addEventListener("mousemove", drag);
                });
                bar?.addEventListener("mouseup", () => {
                    document.removeEventListener("mousemove", drag);
                });
            }
            // gets attributes text for given subsequence
            function getAttributesText(attributes) {
                let attributesText = [];
                for (let i = 0; i < attributes.length; i++) {
                    let attributeValue = $scope.attributeValues[attributes[i]];
                    attributesText.push($scope.attributes[attributeValue.attribute] + (attributeValue.value === "" ? "" : ` = ${attributeValue.value}`));
                }
                return attributesText;
            }
            // returns attribute index by its name if any
            function getAttributeIdByName(dot, attributeName) {
                return dot.attributes.find(a => $scope.attributes[$scope.attributeValues[a].attribute] === attributeName);
            }
            // returns true if dot has given attribute and its value equal to the given value
            function isAttributeEqual(dot, attributeName, expectedValue) {
                let attributeId = $scope.getAttributeIdByName(dot, attributeName);
                if (attributeId !== undefined) {
                    let product = $scope.attributeValues[attributeId].value.toUpperCase();
                    return product.indexOf(expectedValue) !== -1;
                }
                return false;
            }
            // applies new filter
            function addFilter(newFilter) {
                d3.selectAll(".dot")
                    .attr("visibility", (d) => {
                    let filterValue = newFilter.toUpperCase();
                    let visible = $scope.isAttributeEqual(d, "product", filterValue);
                    visible = visible || $scope.isAttributeEqual(d, "locus_tag", filterValue);
                    d.filtersVisible.push(visible);
                    return $scope.dotVisible(d) ? "visible" : "hidden";
                });
                $scope.fillVisiblePoints();
            }
            // removes given filter
            function deleteFilter(filter, filterIndex) {
                d3.selectAll(".dot")
                    .attr("visibility", (d) => {
                    if (d.FiltersVisible) {
                        d.FiltersVisible.splice(filterIndex, 1);
                    }
                    return $scope.dotVisible(d) ? "visible" : "hidden";
                });
                $scope.fillVisiblePoints();
            }
            // determines if dots are similar by product
            function dotsSimilar(d, dot) {
                if (d.featureId !== dot.featureId) {
                    return false;
                }
                switch (d.featureId) {
                    case 1: // CDS
                    case 2: // RRNA
                    case 3: // TRNA
                        const firstProductId = $scope.getAttributeIdByName(d, "product");
                        const secondProductId = $scope.getAttributeIdByName(dot, "product");
                        if (firstProductId !== undefined && secondProductId !== undefined) {
                            const firstAttributeValue = $scope.attributeValues[firstProductId].value.toUpperCase();
                            const secondAttributeValue = $scope.attributeValues[secondProductId].value.toUpperCase();
                            if (firstAttributeValue !== secondAttributeValue) {
                                return false;
                            }
                        }
                        break;
                }
                return true;
            }
            // Функция-заглушка для dotVisible
            function dotVisible(d) {
                return true;
            }
            // Функция-заглушка для fillVisiblePoints
            function fillVisiblePoints() {
                // Реализация отсутствует в исходном коде
            }
            $scope.fillLinePlotData = fillLinePlotData;
            $scope.fillScatterPlotData = fillScatterPlotData;
            $scope.fill3dScatterPlotData = fill3dScatterPlotData;
            $scope.fillParallelCoordinatesPlotData = fillParallelCoordinatesPlotData;
            $scope.draw = draw;
            $scope.fillPoints = fillPoints;
            $scope.fillLegend = fillLegend;
            $scope.legendClick = legendClick;
            $scope.legendSetVisibilityForAll = legendSetVisibilityForAll;
            $scope.dragbarMouseDown = dragbarMouseDown;
            $scope.showTooltip = showTooltip;
            $scope.dotVisible = dotVisible;
            $scope.fillVisiblePoints = fillVisiblePoints;
            $scope.dotsSimilar = dotsSimilar;
            $scope.getAttributesText = getAttributesText;
            $scope.addFilter = addFilter;
            $scope.deleteFilter = deleteFilter;
            $scope.getAttributeIdByName = getAttributeIdByName;
            $scope.isAttributeEqual = isAttributeEqual;
            $scope.fillPointTooltip = fillPointTooltip;
            $scope.chartElement = document.getElementById("chart");
            $scope.characteristicsTableRendering = { rendered: false };
            $scope.chartsCharacterisrticsCount = 1;
            $scope.tooltipElements = [];
            $scope.visiblePoints = [];
            $scope.characteristicComparers = [];
            $scope.productFilter = "";
            $scope.pointsSimilarity = Object.freeze({ "same": 0, "similar": 1, "different": 2 });
            $scope.loadingScreenHeader = "Loading subsequences characteristics";
            $scope.loading = true;
            let location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];
            $http.get(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                MapModelFromJson($scope, data.data);
                $scope.fillLegend();
                $scope.chartCharacteristics = [{ id: $scope.chartsCharacterisrticsCount++, value: $scope.characteristicsList[0] }];
                $scope.loading = false;
            })
                .catch(function () {
                alert("Failed loading subsequences characteristics");
                $scope.loading = false;
            });
        };
        // Register controller in Angular module
        angular.module("libiada").controller("SubsequencesCalculationResultCtrl", ["$scope", "$http", "$sce", subsequencesCalculationResult]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @returns Instance of subsequence calculation result handler
 */
function SubsequencesCalculationResultController() {
    return new SubsequencesCalculationResultHandler();
}
//# sourceMappingURL=subsequencesCalculationResult.js.map