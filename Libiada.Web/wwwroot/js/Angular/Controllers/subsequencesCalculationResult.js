﻿function SubsequencesCalculationResultController() {
    "use strict";

    function subsequencesCalculationResult($scope, $http, $sce) {

        // fills array of currently visible points
        function fillVisiblePoints() {
            $scope.visiblePoints = [];
            for (let i = 0; i < $scope.points.length; i++) {
                $scope.visiblePoints.push([]);
                for (let j = 0; j < $scope.points[i].length; i++) {
                    if ($scope.dotVisible($scope.points[i][j])) {
                        $scope.visiblePoints[i].push($scope.points[i][j]);
                    }
                }
            }
        }

        // gets attributes text for given subsequence
        function getAttributesText(attributes) {
            let attributesText = [];
            for (let i = 0; i < attributes.length; i++) {
                let attributeValue = $scope.attributeValues[attributes[i]];
                attributesText.push($scope.attributes[attributeValue.attribute] + (attributeValue.value === "" ? "" : ` = ${attributeValue.value}`));
            }

            return $sce.trustAsHtml(attributesText.join("<br/>"));
        }

        // returns attribute index by its name if any
        function getAttributeIdByName(dot, attributeName) {
            return dot.attributes.find(a => $scope.attributes[$scope.attributeValues[a].attribute] === attributeName);
        }

        // returns true if dot has given attribute and its value equal to the given value
        function isAttributeEqual(dot, attributeName, expectedValue) {
            let attributeId = $scope.getAttributeIdByName(dot, attributeName);
            if (attributeId) {
                let product = $scope.attributeValues[attributeId].value.toUpperCase();
                return product.indexOf(expectedValue) !== -1;
            }

            return false;
        }

        // applies new filter
        function addFilter(newFilter) {
            d3.selectAll(".dot")
                .attr("visibility", d => {
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
                .attr("visibility", d => {
                    d.FiltersVisible.splice(filterIndex, 1);
                    return $scope.dotVisible(d) ? "visible" : "hidden";
                });

            $scope.fillVisiblePoints();
        }

        // initializes data for genes map
        function fillPoints() {
            $scope.researchObjects = [];
            $scope.points = [];
            for (let i = 0; i < $scope.sequencesData.length; i++) {
                let sequenceData = $scope.sequencesData[i];
                $scope.researchObjects.push({ id: sequenceData.ResearchObjectId, name: sequenceData.ResearchObjectName, visible: true, colorId: i, visible: true });
                $scope.points.push([]);
                for (let j = 0; j < sequenceData.SubsequencesData.length; j++) {
                    let subsequenceData = sequenceData.SubsequencesData[j];
                    let point = {
                        id: subsequenceData.Id,
                        researchObjectId: sequenceData.ResearchObjectId,
                        researchObjectName: sequenceData.ResearchObjectName,
                        sequenceRemoteId: sequenceData.RemoteId,
                        attributes: subsequenceData.Attributes,
                        partial: subsequenceData.Partial,
                        featureId: subsequenceData.FeatureId,
                        attributes: subsequenceData.Attributes,
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

        // filters dots by subsequences feature
        function filterByFeature(feature) {

            let featureValue = parseInt(feature.Value);
            d3.selectAll(".dot")
                .filter(dot => dot.featureId === featureValue)
                .attr("visibility", d => {
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

            // TODO: optimize this method calls
            $scope.fillVisiblePoints();
        }

        // checks if dot is visible
        function dotVisible(dot) {
            let filterVisible = dot.filtersVisible.length === 0 || dot.filtersVisible.some(element => element);

            return dot.featureVisible && dot.legendVisible && filterVisible;
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
                    const firstAttributeValue = $scope.attributeValues[firstProductId].value.toUpperCase();
                    const secondAttributeValue = $scope.attributeValues[secondProductId].value.toUpperCase();
                    if (firstAttributeValue !== secondAttributeValue) {
                        return false;
                    }
                    break;
            }

            return true;
        }

        // shows tooltip for dot or group of dots
        function showTooltip(event, d, tooltip, svg) {
            $scope.clearTooltip(tooltip);
            let tooltipHtml = [];
            tooltip.style("opacity", 0.9);

            tooltip.selectedDots = svg.selectAll(".dot")
                .filter(dot => {
                    if ($scope.xValue(dot) === $scope.xValue(d)
                     && $scope.yValue(dot) === $scope.yValue(d)) {
                        tooltipHtml.push($scope.fillPointTooltip(dot));
                        return true;
                    } else {
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

        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            let tooltipContent = [];
            let genBankLink = "<a target='_blank' rel='noopener' href='https://www.ncbi.nlm.nih.gov/nuccore/";

            let header = d.remoteId ? `${genBankLink}${d.remoteId}'>${d.researchObjectName}</a>` : d.researchObjectName;
            tooltipContent.push(header);

            if (d.remoteId) {
                let peptideGenbankLink = `${genBankLink}${d.remoteId}'>Peptide ncbi page</a>`;
                tooltipContent.push(peptideGenbankLink);
            }

            tooltipContent.push($scope.features[d.featureId]);
            tooltipContent.push($scope.getAttributesText(d.attributes));

            if (d.partial) {
                tooltipContent.push("partial");
            }

            let start = d.positions[0] + 1;
            let end = d.positions[0] + d.lengths[0];
            let positionGenbankLink = d.remoteId ?
                `${genBankLink}${d.remoteId}?from=${start}&to=${end}'>${d.positions.join(", ")}</a>` :
                d.positions.join(", ");
            tooltipContent.push(`Position: ${positionGenbankLink}`);
            tooltipContent.push(`Length: ${d.lengths.join(", ")}`);
            // TODO: show all characteristics
            tooltipContent.push(`(${$scope.xValue(d)}, ${$scope.yValue(d)})`);

            return tooltipContent.join("</br>");
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
            return $scope.lineChart ? d.rank : d.characteristicsValues[+$scope.firstCharacteristic.Value];
        }

        function yValue(d) {
            return $scope.lineChart ? d.characteristicsValues[+$scope.firstCharacteristic.Value] : d.characteristicsValues[+$scope.secondCharacteristic.Value];
        }

        // main drawing method
        function draw() {
            $scope.loading = true;
            $scope.loadingScreenHeader = "Drawing...";
            $scope.fillPoints();
            // removing previous chart and tooltip if any
            d3.select(".chart-tooltip").remove();
            d3.select(".chart-svg").remove();

            // sorting points by selected characteristic
            if ($scope.lineChart) {
                for (let i = 0; i < $scope.points.length; i++) {
                    $scope.points[i].sort((first, second) => $scope.yValue(second) - $scope.yValue(first));

                    for (let j = 0; j < $scope.points[i].length; j++) {
                        $scope.points[i][j].rank = j + 1;
                    }
                }
            }

            // all organisms are visible after redrawing
            $scope.researchObjects.forEach(researchObject => { researchObject.visible = true; });

            $scope.points.forEach(points => {
                points.forEach(point => {
                    point.legendVisible = true;
                    point.FeatureVisible = $scope.features[point.featureId].Selected;
                });
            });

            // chart size and margin settings
            let margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
            let width = $scope.width - margin.left - margin.right;
            let height = $scope.height - margin.top - margin.bottom;

            // calculating margins for dots
            let xMinArray = [];
            let xMaxArray = [];
            let yMaxArray = [];
            let yMinArray = [];

            $scope.points.forEach(points => {
                xMinArray.push(d3.min(points, $scope.xValue));
                xMaxArray.push(d3.max(points, $scope.xValue));
                yMinArray.push(d3.min(points, $scope.yValue));
                yMaxArray.push(d3.max(points, $scope.yValue));
            });

            // setup x
            // calculating margins for dots
            let xMin = d3.min(xMinArray);
            let xMax = d3.max(xMaxArray);
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
            let yMin = d3.min(yMinArray);
            let yMax = d3.max(yMaxArray);
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
            let color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.researchObjects.length]);

            // add the graph canvas to the body of the webpage
            let svg = d3.select("#chart").append("svg")
                .attr("width", $scope.width)
                .attr("height", $scope.height)
                .attr("class", "chart-svg")
                .append("g")
                .attr("transform", `translate(${margin.left},${margin.top})`);

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
                .attr("transform", `translate(0,${height})`)
                .call(xAxis);

            svg.append("text")
                .attr("class", "label")
                .attr("transform", `translate(${width / 2} ,${height + margin.top - $scope.legendHeight})`)
                .style("text-anchor", "middle")
                .text($scope.lineChart ? "Rank" : $scope.firstCharacteristic.Text)
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

            let researchObjectsGroups = svg.selectAll(".researchObject")
                .data($scope.points)
                .enter()
                .append("g")
                .attr("class", "researchObject");

            // draw dots
            researchObjectsGroups.selectAll(".dot")
                .data(d => d)
                .enter()
                .append("ellipse")
                .attr("class", "dot")
                .attr("rx", $scope.dotRadius)
                .attr("ry", $scope.dotRadius)
                .attr("cx", $scope.xMap)
                .attr("cy", $scope.yMap)
                .style("fill-opacity", 0.6)
                .style("fill", d => color(d.colorId))
                .style("stroke", d => color(d.colorId))
                .attr("visibility", d => $scope.dotVisible(d) ? "visible" : "hidden")
                .on("click", (event, d) => $scope.showTooltip(event, d, tooltip, svg));

            // draw legend
            let legend = svg.selectAll(".legend")
                .data($scope.researchObjects)
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
                        .filter(dot => dot.researchObjectId === d.id)
                        .attr("visibility", dot => {
                            dot.legendVisible = d.visible;
                            return $scope.dotVisible(dot) ? "visible" : "hidden";
                        });
                });

            // draw legend colored rectangles
            legend.append("rect")
                .attr("width", 15)
                .attr("height", 15)
                .style("fill", d => color(d.colorId))
                .style("stroke", d => color(d.colorId))
                .style("stroke-width", 4)
                .attr("transform", `translate(0, -${$scope.legendHeight})`);

            // draw legend text
            legend.append("text")
                .attr("x", 24)
                .attr("y", 9)
                .attr("dy", ".35em")
                .attr("transform", `translate(0, -${$scope.legendHeight})`)
                .text(d => d.name)
                .style("font-size", "9pt");

            $scope.loading = false;
        }

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

        $scope.dotRadius = 3;
        $scope.selectedDotRadius = $scope.dotRadius * 3;
        $scope.visiblePoints = [];
        $scope.characteristicComparers = [];
        $scope.productFilter = "";

        $scope.loadingScreenHeader = "Loading subsequences characteristics";
        $scope.loading = true;

        let location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $http.get(`/api/TaskManagerApi/GetTaskData/${$scope.taskId}`)
            .then(function (data) {
                MapModelFromJson($scope, data.data);

                $scope.legendHeight = $scope.sequencesData.length * 20;
                $scope.height = 800 + $scope.legendHeight;
                $scope.width = 800;

                $scope.firstCharacteristic = $scope.subsequencesCharacteristicsList[0];
                $scope.secondCharacteristic = $scope.subsequencesCharacteristicsList[$scope.subsequencesCharacteristicsList.length - 1];

                $scope.loading = false;
            }, function () {
                alert("Failed loading subsequences characteristics");
                $scope.loading = false;
            });
    }

    angular.module("libiada").controller("SubsequencesCalculationResultCtrl", ["$scope", "$http", "$sce", subsequencesCalculationResult]);
}
