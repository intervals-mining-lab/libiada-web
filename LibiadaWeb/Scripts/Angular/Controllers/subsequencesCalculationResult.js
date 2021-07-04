function SubsequencesCalculationResultController() {
    "use strict";

    function subsequencesCalculationResult($scope, $http, $sce) {

        // fills array of currently visible points
        function fillVisiblePoints() {
            $scope.visiblePoints = [];
            for (var i = 0; i < $scope.points.length; i++) {
                $scope.visiblePoints.push([]);
                for (var j = 0; j < $scope.points[i].length; i++) {
                    if ($scope.dotVisible($scope.points[i][j])) {
                        $scope.visiblePoints[i].push($scope.points[i][j]);
                    }
                }
            }
        }

        // gets attributes text for given subsequence
        function getAttributesText(attributes) {
            var attributesText = [];
            for (var i = 0; i < attributes.length; i++) {
                var attributeValue = $scope.attributeValues[attributes[i]];
                attributesText.push($scope.attributes[attributeValue.attribute] + (attributeValue.value === "" ? "" : " = " + attributeValue.value));
            }

            return $sce.trustAsHtml(attributesText.join("<br/>"));
        }

        // returns attribute index by its name if any
        function getAttributeIdByName(dot, attributeName) {
            return dot.attributes.find(a => $scope.attributes[$scope.attributeValues[a].attribute] === attributeName);
        }

        // returns true if dot has given attribute and its value equal to the given value
        function isAttributeEqual(dot, attributeName, expectedValue) {
            var attributeId = $scope.getAttributeIdByName(dot, attributeName);
            if (attributeId) {
                var product = $scope.attributeValues[attributeId].value.toUpperCase();
                return product.indexOf(expectedValue) !== -1;
            }

            return false;
        }

        // adds and applies new filter
        function addFilter() {
            if ($scope.newFilter.length > 0) {
                $scope.filters.push({ value: $scope.newFilter });

                d3.selectAll(".dot")
                    .attr("visibility", d => {
                        var filterValue = $scope.newFilter.toUpperCase();
                        var visible = $scope.isAttributeEqual(d, "product", filterValue);
                        visible = visible || $scope.isAttributeEqual(d, "locus_tag", filterValue);
                        d.filtersVisible.push(visible);
                        return $scope.dotVisible(d) ? "visible" : "hidden";
                    });

                $scope.fillVisiblePoints();
                $scope.newFilter = "";
            }
            // todo: add error message if filter is empty
        }

        // deletes given filter
        function deleteFilter(filter) {
            d3.selectAll(".dot")
                .attr("visibility", d => {
                    d.FiltersVisible.splice($scope.filters.indexOf(filter), 1);
                    return $scope.dotVisible(d) ? "visible" : "hidden";
                });
            $scope.filters.splice($scope.filters.indexOf(filter), 1);
            $scope.fillVisiblePoints();
        }

        // initializes data for genes map
        function fillPoints() {
            $scope.matters = [];
            $scope.points = [];
            for (var i = 0; i < $scope.sequencesData.length; i++) {
                var sequenceData = $scope.sequencesData[i];
                $scope.matters.push({ id: sequenceData.MatterId, name: sequenceData.MatterName, visible: true, colorId: i, visible: true });
                $scope.points.push([]);
                for (var j = 0; j < sequenceData.SubsequencesData.length; j++) {
                    var subsequenceData = sequenceData.SubsequencesData[j];
                    var point = {
                        id: subsequenceData.Id,
                        matterId: sequenceData.MatterId,
                        matterName: sequenceData.MatterName,
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

            var featureValue = parseInt(feature.Value);
            d3.selectAll(".dot")
                .filter(dot => dot.featureId === featureValue)
                .attr("visibility", d => {
                    d.featureVisible = feature.Selected;
                    return $scope.dotVisible(d) ? "visible" : "hidden";
                });

            for (var i = 0; i < $scope.points.length; i++) {
                for (var j = 0; j < $scope.points[i].length; j++) {
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
            var filterVisible = dot.filtersVisible.length === 0 || dot.filtersVisible.some(element => element);

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
                    var firstProductId = $scope.getAttributeIdByName(d, "product");
                    var secondProductId = $scope.getAttributeIdByName(dot, "product");
                    if ($scope.attributeValues[firstProductId].value.toUpperCase() !== $scope.attributeValues[secondProductId].value.toUpperCase()) {
                        return false;
                    }
                    break;
            }

            return true;
        }

        // shows tooltip for dot or group of dots
        function showTooltip(event, d, tooltip, svg) {
            $scope.clearTooltip(tooltip);
            var tooltipHtml = [];
            tooltip.style("opacity", 0.9);

            tooltip.selectedDots = svg.selectAll(".dot")
                .filter(dot => {
                    if ($scope.xValue(dot) === $scope.xValue(d) && $scope.yValue(dot) === $scope.yValue(d)) {
                        tooltipHtml.push($scope.fillPointTooltip(dot));
                        return true;
                    } else {
                        return false;
                    }
                })
                .attr("rx", $scope.selectedDotRadius)
                .attr("ry", $scope.selectedDotRadius);

            tooltip.html(tooltipHtml.join("</br></br>"));

            tooltip.style("background", "#eee")
                .style("color", "#000")
                .style("border-radius", "5px")
                .style("font-family", "monospace")
                .style("padding", "5px")
                .style("left", (event.pageX + 10) + "px")
                .style("top", (event.pageY - 8) + "px");

            tooltip.hideTooltip = false;
        }

        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            var tooltipContent = [];
            var genBankLink = "<a target='_blank' rel='noopener' href='https://www.ncbi.nlm.nih.gov/nuccore/";

            var header = d.remoteId ? genBankLink + d.remoteId + "'>" + d.matterName + "</a>" : d.matterName;
            tooltipContent.push(header);

            if (d.remoteId) {
                var peptideGenbankLink = genBankLink + d.remoteId + "'>Peptide ncbi page</a>";
                tooltipContent.push(peptideGenbankLink);
            }

            tooltipContent.push($scope.features[d.featureId]);
            tooltipContent.push($scope.getAttributesText(d.attributes));

            if (d.partial) {
                tooltipContent.push("partial");
            }

            var start = d.positions[0] + 1;
            var end = d.positions[0] + d.lengths[0];
            var positionGenbankLink = d.remoteId ?
                genBankLink + d.remoteId + "?from=" + start + "&to=" + end + "'>" + d.positions.join(", ") + "</a>" :
                d.positions.join(", ");
            tooltipContent.push("Position: " + positionGenbankLink);
            tooltipContent.push("Length: " + d.lengths.join(", "));
            // TODO: show all characteristics
            tooltipContent.push("(" + $scope.xValue(d) + ", " + $scope.yValue(d) + ")");

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
            d3.select(".tooltip").remove();
            d3.select(".chart-svg").remove();

            // sorting points by selected characteristic
            if ($scope.lineChart) {
                for (var i = 0; i < $scope.points.length; i++) {
                    $scope.points[i].sort((first, second) => $scope.yValue(second) - $scope.yValue(first));

                    for (var j = 0; j < $scope.points[i].length; j++) {
                        $scope.points[i][j].rank = j + 1;
                    }
                }
            }

            // all organisms are visible after redrawing
            $scope.matters.forEach(matter => { matter.visible = true; });

            $scope.points.forEach(points => {
                points.forEach(point => {
                    point.legendVisible = true;
                    point.FeatureVisible = $scope.features[point.featureId].Selected;
                });
            });

            // chart size and margin settings
            var margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
            var width = $scope.width - margin.left - margin.right;
            var height = $scope.height - margin.top - margin.bottom;

            // calculating margins for dots
            var xMinArray = [];
            var xMaxArray = [];
            var yMaxArray = [];
            var yMinArray = [];

            $scope.points.forEach(points => {
                xMinArray.push(d3.min(points, $scope.xValue));
                xMaxArray.push(d3.max(points, $scope.xValue));
                yMinArray.push(d3.min(points, $scope.yValue));
                yMaxArray.push(d3.max(points, $scope.yValue));
            });

            // setup x
            // calculating margins for dots
            var xMin = d3.min(xMinArray);
            var xMax = d3.max(xMaxArray);
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
            var yMin = d3.min(yMinArray);
            var yMax = d3.max(yMaxArray);
            var yMargin = (yMax - yMin) * 0.05;

            var yScale = d3.scaleLinear()
                .domain([yMin - yMargin, yMax + yMargin])
                .range([height, 0]);
            var yAxis = d3.axisLeft(yScale)
                .tickSizeInner(-width)
                .tickSizeOuter(0)
                .tickPadding(10);

            $scope.yMap = d => yScale($scope.yValue(d));

            // setup fill color
            var color = d3.scaleSequential(d3.interpolateTurbo).domain([0, $scope.matters.length]);

            // add the graph canvas to the body of the webpage
            var svg = d3.select("#chart").append("svg")
                .attr("width", $scope.width)
                .attr("height", $scope.height)
                .attr("class", "chart-svg")
                .append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            // add the tooltip area to the webpage
            var tooltip = d3.select("#chart").append("div")
                .attr("class", "tooltip")
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

            var mattersGroups = svg.selectAll(".matter")
                .data($scope.points)
                .enter()
                .append("g")
                .attr("class", "matter");

            // draw dots
            mattersGroups.selectAll(".dot")
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
            var legend = svg.selectAll(".legend")
                .data($scope.matters)
                .enter()
                .append("g")
                .attr("class", "legend")
                .attr("transform", (_d, i) => "translate(0," + i * 20 + ")")
                .on("click", function (_event, d) {
                    d.visible = !d.visible;
                    var legendEntry = d3.select(this);
                    legendEntry.select("text")
                        .style("opacity", () => d.visible ? 1 : 0.5);
                    legendEntry.select("rect")
                        .style("fill-opacity", () => d.visible ? 1 : 0);

                    svg.selectAll(".dot")
                        .filter(dot => dot.matterId === d.id)
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
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")");

            // draw legend text
            legend.append("text")
                .attr("x", 24)
                .attr("y", 9)
                .attr("dy", ".35em")
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")")
                .text(d => d.name)
                .style("font-size", "9pt");

            $scope.loading = false;
        }

        $scope.setCheckBoxesState = SetCheckBoxesState;

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
        $scope.filters = [];
        $scope.productFilter = "";

        $scope.loadingScreenHeader = "Loading subsequences characteristics";
        $scope.loading = true;

        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $http.get(`/api/TaskManagerWebApi/${$scope.taskId}`)
            .then(function (data) {
                MapModelFromJson($scope, JSON.parse(data.data));

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
