/// <reference types="angular" />
/// <reference types="d3" />
/// <reference path="../functions.d.ts" />
/**
* Handler for displaying subsequence calculation results
*/
class SubsequencesCalculationResultHandler {
    constructor() {
        this.initializeController();
    }
    /**
    * Initializes the Angular controller
    */
    initializeController() {
        const subsequencesCalculationResult = ($scope, $http, $sce) => {
            "use strict";
            /**
            * Fills an array of visible points
            */
            function fillVisiblePoints() {
                $scope.visiblePoints = [];
                for (let i = 0; i < $scope.points.length; i++) {
                    $scope.visiblePoints.push([]);
                    for (let j = 0; j < $scope.points[i].length; j++) {
                        if ($scope.dotVisible($scope.points[i][j])) {
                            $scope.visiblePoints[i].push($scope.points[i][j]);
                        }
                    }
                }
            }
            /**
            * Gets the attribute text for the given subsequence
            //* @param attributes Array of attribute IDs
            */
            function getAttributesText(attributes) {
                const attributesText = [];
                for (let i = 0; i < attributes.length; i++) {
                    const attributeValue = $scope.attributeValues[attributes[i]];
                    attributesText.push($scope.attributes[attributeValue.attribute] + (attributeValue.value === "" ? "" : ` = ${attributeValue.value}`));
                }
                return $sce.trustAsHtml(attributesText.join("<br/>"));
            }
            /**
            * Returns the index of an attribute by its name, if any
            * @param dot Data point
            * @param attributeName Attribute name
            */
            function getAttributeIdByName(dot, attributeName) {
                return dot.attributes.find(a => $scope.attributes[$scope.attributeValues[a].attribute] === attributeName);
            }
            /**
            * Returns true if the dot has the given attribute and its value is equal to the given value
            * @param dot The data point
            * @param attributeName The name of the attribute
            * @param expectedValue The expected value
            */
            function isAttributeEqual(dot, attributeName, expectedValue) {
                const attributeId = $scope.getAttributeIdByName(dot, attributeName);
                if (attributeId !== undefined) {
                    const product = $scope.attributeValues[attributeId].value.toUpperCase();
                    return product.indexOf(expectedValue) !== -1;
                }
                return false;
            }
            /**
            *Applies new filter
            * @param newFilter Filter string
            */
            function addFilter(newFilter) {
                d3.selectAll(".dot")
                    .attr("visibility", (d) => {
                    const filterValue = newFilter.toUpperCase();
                    let visible = $scope.isAttributeEqual(d, "product", filterValue);
                    visible = visible || $scope.isAttributeEqual(d, "locus_tag", filterValue);
                    d.filtersVisible.push(visible);
                    return $scope.dotVisible(d) ? "visible" : "hidden";
                });
                $scope.fillVisiblePoints();
            }
            /**
            * Deletes the specified filter
            * @param filter Filter string
            * @param filterIndex Filter index
            */
            function deleteFilter(filter, filterIndex) {
                d3.selectAll(".dot")
                    .attr("visibility", (d) => {
                    d.filtersVisible.splice(filterIndex, 1);
                    return $scope.dotVisible(d) ? "visible" : "hidden";
                });
                $scope.fillVisiblePoints();
            }
            /**
            * Initializes data for the gene map
            */
            function fillPoints() {
                $scope.researchObjects = [];
                $scope.points = [];
                for (let i = 0; i < $scope.sequencesData.length; i++) {
                    const sequenceData = $scope.sequencesData[i];
                    $scope.researchObjects.push({
                        id: sequenceData.ResearchObjectId,
                        name: sequenceData.ResearchObjectName,
                        visible: true,
                        colorId: i,
                        Nature: "", // Fill with suitable values 
                        Group: 0, // or get it from sequenceData 
                        SequenceType: 0,
                        Multisequence: false,
                        Matter: 0,
                        MatterIds: [],
                        RemoteId: sequenceData.RemoteId || null,
                        Notation: 0,
                        NotationValue: 0,
                        LanguageId: null,
                        TranslatorId: null,
                        Characteristics: [],
                        Description: null
                    });
                    $scope.points.push([]);
                    for (let j = 0; j < sequenceData.SubsequencesData.length; j++) {
                        const subsequenceData = sequenceData.SubsequencesData[j];
                        const point = {
                            id: subsequenceData.Id, researchObjectId: sequenceData.ResearchObjectId,
                            researchObjectName: sequenceData.ResearchObjectName,
                            sequenceRemoteId: sequenceData.RemoteId,
                            attributes: subsequenceData.Attributes,
                            partial: subsequenceData.Partial,
                            featureId: subsequenceData.FeatureId,
                            positions: subsequenceData.Starts,
                            lengths: subsequenceData.Lengths,
                            subsequenceRemoteId: subsequenceData.RemoteId,
                            rank: j + 1,
                            characteristicsValues: subsequenceData.CharacteristicsValues,
                            colorId: i,
                            featureVisible: true,
                            legendVisible: true,
                            filtersVisible: [],
                        };
                        $scope.points[i].push(point);
                    }
                }
            }
            /**
            * Filters points by subsequence feature
            * @param feature Feature to filter
            */
            function filterByFeature(feature) {
                const featureValue = parseInt(feature.Value);
                d3.selectAll(".dot")
                    .filter((dot) => dot.featureId === featureValue)
                    .attr("visibility", (d) => {
                    d.featureVisible = feature.Selected;
                    return $scope.dotVisible(d) ? "visible" : "hidden";
                });
                for (let i = 0; i < $scope.points.length; i++) {
                    for (let j = 0; j < $scope.points[i].length; j++) {
                        if ($scope.points[i][j].featureId === parseInt(feature.Value)) {
                            $scope.points[i][j].featureVisible = feature.Selected;
                        }
                    }
                }
                // TODO: optimize calls to this method
                $scope.fillVisiblePoints();
            }
            /**
            * Checks if a point is visible
            * @param dot The point to check
            */
            function dotVisible(dot) {
                const filterVisible = dot.filtersVisible.length === 0 || dot.filtersVisible.some(element => element);
                return dot.featureVisible && dot.legendVisible && filterVisible;
            }
            /**
            * Determines whether points are similar by product
            * @param d First point
            * @param dot Second dot
            */
            function dotsSimilar(d, dot) {
                if (d.featureId !== dot.featureId) {
                    return false;
                }
                switch (d.featureId) {
                    case 1: //CDS 
                    case 2: // RRNA 
                    case 3: // TRNA 
                        const firstProductId = $scope.getAttributeIdByName(d, "product");
                        const secondProductId = $scope.getAttributeIdByName(dot, "product");
                        if (firstProductId === undefined || secondProductId === undefined) {
                            return false;
                        }
                        const firstAttributeValue = $scope.attributeValues[firstProductId].value.toUpperCase();
                        const secondAttributeValue = $scope.attributeValues[secondProductId].value.toUpperCase();
                        if (firstAttributeValue !== secondAttributeValue) {
                            return false;
                        }
                        break;
                }
                return true;
            }
            /**
            * Shows a tooltip for a point or group of points
            * @param event Mouse event
            * @param d Data point
            * @param tooltip Tooltip element
            * @param svg SVG element
            */
            function showTooltip(event, d, tooltip, svg) {
                $scope.clearTooltip(tooltip);
                const tooltipHtml = [];
                tooltip.style("opacity", 0.9);
                tooltip.selectedDots = svg.selectAll(".dot")
                    .filter((dot) => {
                    if ($scope.xValue(dot) === $scope.xValue(d)
                        && $scope.yValue(dot) === $scope.yValue(d)) {
                        tooltipHtml.push($scope.fillPointTooltip(dot));
                        return true;
                    }
                    else {
                        return false;
                    }
                })
                    .attr("rx", $scope.selectedDotRadius)
                    .attr("ry", $scope.selectedDotRadius);
                tooltip.html(tooltipHtml.join("</br></br>"));
                tooltip.style("left", `${event.pageX + 10}px`)
                    .style("top", `${event.pageY - 8}px`);
                tooltip.hideTooltip = false;
            }
            /**
            * Creates a string representing the tooltip text
            * @param d Data point
            */
            function fillPointTooltip(d) {
                const tooltipContent = [];
                const genBankLink = "<a target='_blank' rel='noopener' href='https://www.ncbi.nlm.nih.gov/nuccore/";
                const header = d.sequenceRemoteId ? `${genBankLink}${d.sequenceRemoteId}'>${d.researchObjectName}</a>` : d.researchObjectName;
                tooltipContent.push(header);
                if (d.sequenceRemoteId) {
                    const peptideGenbankLink = `${genBankLink}${d.sequenceRemoteId}'>Peptide ncbi page</a>`;
                    tooltipContent.push(peptideGenbankLink);
                }
                //tooltipContent.push($scope.features[d.featureId]); 
                tooltipContent.push($scope.features[d.featureId]?.Text || $scope.features[d.featureId]?.Value || "Unknown feature");
                tooltipContent.push($scope.getAttributesText(d.attributes));
                if (d.partial) {
                    tooltipContent.push("partial");
                }
                const start = d.positions[0] + 1;
                const end = d.positions[0] + d.lengths[0];
                const positionGenbankLink = d.sequenceRemoteId ?
                    `${genBankLink}${d.sequenceRemoteId}?from=${start}&to=${end}'>${d.positions.join(", ")}</a>` :
                    d.positions.join(", ");
                tooltipContent.push(`Position: ${positionGenbankLink}`);
                tooltipContent.push(`Length: ${d.lengths.join(", ")}`);
                // TODO: show all features
                tooltipContent.push(`(${$scope.xValue(d)}, ${$scope.yValue(d)})`);
                return tooltipContent.join("</br>");
            }
            /**
            * Clears the tooltip and deselects the dots
            * @param tooltip Tooltip element
            */
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
            /**
            * Returns the X value of the data point
            * @param d Data point
            */
            function xValue(d) {
                return $scope.lineChart ? d.rank : d.characteristicsValues[+$scope.firstCharacteristic.Value];
            }
            /**
            * Returns the Y value of the data point
            * @param d Data point
            */
            function yValue(d) {
                return $scope.lineChart ? d.characteristicsValues[+$scope.firstCharacteristic.Value] : d.characteristicsValues[+$scope.secondCharacteristic.Value];
            }
            /**
            * Main method for drawing the chart
            */
            function draw() {
                $scope.loading = true;
                $scope.loadingScreenHeader = "Drawing...";
                $scope.fillPoints();
                // Remove the previous chart and tooltip, if any
                d3.select(".chart-tooltip").remove();
                d3.select(".chart-svg").remove();
                // Sort points by selected characteristic
                if ($scope.lineChart) {
                    for (let i = 0; i < $scope.points.length; i++) {
                        $scope.points[i].sort((first, second) => $scope.yValue(second) - $scope.yValue(first));
                        for (let j = 0; j < $scope.points[i].length; j++) {
                            $scope.points[i][j].rank = j + 1;
                        }
                    }
                }
                // All organisms are visible after redrawing
                $scope.researchObjects.forEach(researchObject => { researchObject.visible = true; });
                $scope.points.forEach(points => {
                    points.forEach(point => {
                        point.legendVisible = true;
                        point.featureVisible = $scope.features[point.featureId].Selected;
                    });
                });
                // Settings for chart size and indentation 
                const margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
                const width = $scope.width - margin.left - margin.right;
                const height = $scope.height - margin.top - margin.bottom;
                // Calculate boundaries for points 
                const xMinArray = [];
                const xMaxArray = [];
                const yMaxArray = [];
                const yMinArray = [];
                $scope.points.forEach(points => {
                    xMinArray.push(d3.min(points, $scope.xValue));
                    xMaxArray.push(d3.max(points, $scope.xValue));
                    yMinArray.push(d3.min(points, $scope.yValue));
                    yMaxArray.push(d3.max(points, $scope.yValue));
                });
                // Setting up the X axis
                // Calculating the bounds for the points
                const xMin = d3.min(xMinArray);
                const xMax = d3.max(xMaxArray);
                const xMargin = (xMax - xMin) * 0.05;
                const xScale = d3.scaleLinear()
                    .domain([xMin - xMargin, xMax + xMargin])
                    .range([0, width]);
                const xAxis = d3.axisBottom(xScale)
                    .tickSizeInner(-height)
                    .tickSizeOuter(0)
                    .tickPadding(10);
                $scope.xMap = (d) => xScale($scope.xValue(d));
                // Setting up the Y axis 
                const yMin = d3.min(yMinArray);
                const yMax = d3.max(yMaxArray);
                const yMargin = (yMax - yMin) * 0.05;
                const yScale = d3.scaleLinear()
                    .domain([yMin - yMargin, yMax + yMargin])
                    .range([height, 0]);
                const yAxis = d3.axisLeft(yScale)
                    .tickSizeInner(-width)
                    .tickSizeOuter(0)
                    .tickPadding(10);
                $scope.yMap = (d) => yScale($scope.yValue(d));
                // Set the fill color
                const color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.researchObjects.length]);
                // Add the chart canvas to the web page body
                const svg = d3.select("#chart").append("svg")
                    .attr("width", $scope.width)
                    .attr("height", $scope.height)
                    .attr("class", "chart-svg")
                    .append("g")
                    .attr("transform", `translate(${margin.left},${margin.top})`);
                // Add tooltip area to web page
                const tooltip = d3.select("#chart").append("div")
                    .attr("class", "chart-tooltip position-absolute bg-light font-monospace small lh-sm p-1 rounded")
                    .style("opacity", 0);
                // Prevent tooltip from hiding when clicked on
                tooltip.on("click", () => { tooltip.hideTooltip = false; });
                // Hide tooltip when clicked outside of it
                d3.select("#chart").on("click", () => { $scope.clearTooltip(tooltip); });
                // X Axis 
                svg.append("g")
                    .attr("class", "x axis")
                    .attr("transform", `translate(0,${height})`)
                    .call(xAxis);
                svg.append("text")
                    .attr("class", "label")
                    .attr("transform", `translate(${width / 2} ,${height + margin.top - $scope.legendHeight})`)
                    .style("text-anchor", "middle")
                    .text($scope.lineChart ? "Rank" : $scope.firstCharacteristic.Text)
                    .style("font-size", "12pt");
                // Y axis 
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
                const researchObjectsGroups = svg.selectAll(".researchObject")
                    .data($scope.points)
                    .enter()
                    .append("g")
                    .attr("class", "researchObject");
                // Draw points 
                researchObjectsGroups.selectAll(".dot")
                    .data((d) => d)
                    .enter()
                    .append("ellipse")
                    .attr("class", "dot")
                    .attr("rx", $scope.dotRadius)
                    .attr("ry", $scope.dotRadius)
                    .attr("cx", $scope.xMap)
                    .attr("cy", $scope.yMap)
                    .style("fill-opacity", 0.6)
                    .style("fill", (d) => color(d.colorId))
                    .style("stroke", (d) => color(d.colorId))
                    .attr("visibility", (d) => $scope.dotVisible(d) ? "visible" : "hidden")
                    .on("click", (event, d) => $scope.showTooltip(event, d, tooltip, svg));
                // Draw a legend 
                const legend = svg.selectAll(".legend")
                    .data($scope.researchObjects)
                    .enter()
                    .append("g")
                    .attr("class", "legend")
                    .attr("transform", (_d, i) => "translate(0," + i * 20 + ")")
                    .on("click", function (event, d) {
                    d.visible = !d.visible;
                    const legendEntry = d3.select(event.currentTarget);
                    legendEntry.select("text")
                        .style("opacity", () => d.visible ? 1 : 0.5);
                    legendEntry.select("rect")
                        .style("fill-opacity", () => d.visible ? 1 : 0);
                    svg.selectAll(".dot")
                        .filter((dot) => dot.researchObjectId === d.id)
                        .attr("visibility", (dot) => {
                        dot.legendVisible = d.visible;
                        return $scope.dotVisible(dot) ? "visible" : "hidden";
                    });
                });
                // Draw colored rectangles of the legend
                legend.append("rect")
                    .attr("width", 15)
                    .attr("height", 15)
                    .style("fill", (d) => color(d.colorId))
                    .style("stroke", (d) => color(d.colorId))
                    .style("stroke-width", 4)
                    .attr("transform", `translate(0, -${$scope.legendHeight})`);
                // Draw legend text 
                legend.append("text")
                    .attr("x", 24)
                    .attr("y", 9)
                    .attr("dy", ".35em")
                    .attr("transform", `translate(0, -${$scope.legendHeight})`)
                    .text((d) => d.name)
                    .style("font-size", "9pt");
                $scope.loading = false;
            }
            // Register functions in $scope 
            $scope.draw = draw;
            $scope.dotVisible = dotVisible;
            $scope.dotsSimilar = dotsSimilar;
            $scope.fillVisiblePoints = fillVisiblePoints;
            $scope.filterByFeature = filterByFeature;
            $scope.getAttributesText = getAttributesText;
            $scope.fillPoints = fillPoints;
            $scope.fillPointTooltip = fillPointTooltip;
            $scope.showTooltip = showTooltip;
            $scope.clearTooltip = clearTooltip;
            $scope.yValue = yValue;
            $scope.xValue = xValue;
            $scope.addFilter = addFilter;
            $scope.deleteFilter = deleteFilter;
            $scope.getAttributeIdByName = getAttributeIdByName;
            $scope.isAttributeEqual = isAttributeEqual;
            // Initialize $scope properties 
            $scope.dotRadius = 3;
            $scope.selectedDotRadius = $scope.dotRadius * 3;
            $scope.visiblePoints = [];
            $scope.characteristicComparers = [];
            $scope.productFilter = "";
            $scope.loadingScreenHeader = "Loading subsequences characteristics";
            $scope.loading = true;
            // Get task ID from URL 
            const location = window.location.href.split("/");
            $scope.taskId = location[location.length - 1];
            // Loading data from the server 
            $http.get(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
                .then(function (data) {
                MapModelFromJson($scope, data.data);
                $scope.legendHeight = $scope.sequencesData.length * 20;
                $scope.height = 800 + $scope.legendHeight;
                $scope.width = 800;
                $scope.firstCharacteristic = $scope.subsequencesCharacteristicsList[0];
                $scope.secondCharacteristic = $scope.subsequencesCharacteristicsList[$scope.subsequencesCharacteristicsList.length - 1];
                $scope.loading = false;
            })
                .catch(function () {
                alert("Failed loading subsequences characteristics");
                $scope.loading = false;
            });
        };
        // Registering a controller in an Angular module
        angular.module("libiada").controller("SubsequencesCalculationResultCtrl", ["$scope", "$http", "$sce", subsequencesCalculationResult]);
    }
}
/**
* Funcwrapper for backward compatibility
*/
function SubsequencesCalculationResultController() {
    return new SubsequencesCalculationResultHandler();
}
//# sourceMappingURL=subsequencesCalculationResult.js.map