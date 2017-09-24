function SubsequencesCalculationResultController() {
    "use strict";

    function subsequencesCalculationResult($scope, $http, $sce) {

        // shows modal window with progressbar and given text
        function showModalLoadingWindow(headerText) {
            $scope.loadingScreenHeader = headerText;
            $scope.loadingModalWindow.modal("show");
            $scope.loading = true;
        }

        // hides modal window
        function hideModalLoadingWindow() {
            $scope.loading = false;
            $scope.loadingModalWindow.modal("hide");
        }

        // fills array of currently visible points
        function fillVisiblePoints() {
            $scope.visiblePoints = [];
            for (var i = 0; i < $scope.sequencesData.length; i++) {
                for (var j = 0; j < $scope.sequencesData[i].SubsequencesData.length; j++) {
                    if ($scope.dotVisible($scope.sequencesData[i].SubsequencesData[j])) {
                        $scope.visiblePoints.push($scope.sequencesData[i].SubsequencesData[j]);
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

        // returns product attribute index if any
        function getProductAttributeId(dot) {
            return dot.Attributes.find(function (a) {
                return $scope.attributes[$scope.attributeValues[a].attribute] === "product";
            });
        }

        // adds and applies new filter
        function addFilter() {
            if ($scope.newFilter.length > 0) {
                $scope.filters.push({ value: $scope.newFilter });

                d3.selectAll(".dot")
                    .attr("visibility",
                    function (d) {
                        var productId = getProductAttributeId(d);
                        d.FiltersVisible.push(productId && $scope.attributeValues[productId].value.toUpperCase().indexOf($scope.newFilter.toUpperCase()) !== -1);
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
                .attr("visibility", function (d) {
                    d.FiltersVisible.splice($scope.filters.indexOf(filter), 1);
                    return $scope.dotVisible(d) ? "visible" : "hidden";
                });
            $scope.filters.splice($scope.filters.indexOf(filter), 1);
            $scope.fillVisiblePoints();
        }

        // initializes data for genes map
        function fillPoints() {
            for (var i = 0; i < $scope.sequencesData.length; i++) {
                $scope.sequencesData[i].Visible = true;

                for (var j = 0; j < $scope.sequencesData[i].SubsequencesData.length; j++) {
                    var subsequenceData = $scope.sequencesData[i].SubsequencesData[j];
                    subsequenceData.Matter = $scope.sequencesData[i];
                    subsequenceData.FeatureVisible = true;
                    subsequenceData.MatterVisible = true;
                    subsequenceData.FiltersVisible = [];
                    subsequenceData.Rank = j + 1;
                }
            }
        }

        // filters dots by subsequences feature
        function filterByFeature(feature) {
            d3.selectAll(".dot")
                .filter(function (dot) {
                    return dot.FeatureId === parseInt(feature.Value);
                })
                .attr("visibility", function (d) {
                    d.FeatureVisible = feature.Selected;
                    return $scope.dotVisible(d) ? "visible" : "hidden";
                });

            for (var i = 0; i < $scope.sequencesData.length; i++) {
                for (var j = 0; j < $scope.sequencesData[i].SubsequencesData.length; j++) {
                    if ($scope.sequencesData[i].SubsequencesData[j].FeatureId === parseInt(feature.Value)) {
                        $scope.sequencesData[i].SubsequencesData[j].FeatureVisible = feature.Selected;
                    }
                }
            }

            // optimize this method calls
            $scope.fillVisiblePoints();
        }

        // checks if dot is visible
        function dotVisible(dot) {
            var filterVisible = dot.FiltersVisible.length === 0 || dot.FiltersVisible.some(function (element) {
                return element;
            });

            return dot.FeatureVisible && dot.MatterVisible && filterVisible;
        }

        // determines if dots are similar by product
        function dotsSimilar(d, dot) {
            if (d.FeatureId !== dot.FeatureId) {
                return false;
            }

            switch (d.FeatureId) {
                case 1: // CDS
                case 2: // RRNA
                case 3: // TRNA
                    var firstProductId = getProductAttributeId(d);
                    var secondProductId = getProductAttributeId(dot);
                    if ($scope.attributeValues[firstProductId].value.toUpperCase() !== $scope.attributeValues[secondProductId].value.toUpperCase()) {
                        return false;
                    }
                    break;
            }

            return true;
        }

        // shows tooltip for dot or group of dots
        function showTooltip(selectedPoints, tooltip, svg) {
            $scope.clearTooltip(tooltip);

            tooltip.style("opacity", 0.9);

            var tooltipHtml = [];

            tooltip.selectedPoints = selectedPoints;
            var point = selectedPoints[0];
            tooltip.selectedDots = svg.selectAll(".dot")
                .filter(function (dot) {
                    if ($scope.dotVisible(dot)) {
                        if (dot.MatterId === point.MatterId && $scope.yValue(dot) === $scope.yValue(point)) { // if dots are in the same position
                            tooltipHtml.push($scope.fillPointTooltip(dot));
                            return true;
                        }
                    }

                    return false;
                })
                .attr("rx", $scope.selectedDotRadius);

            tooltip.html(tooltipHtml.join("</br></br>"));

            var matrix = tooltip.selectedDots.nodes()[0].parentNode.getScreenCTM()
                .translate($scope.xMap(point), $scope.yMap(point));

            tooltip.style("background", "#000")
                .style("color", "#fff")
                .style("border-radius", "5px")
                .style("font-family", "monospace")
                .style("padding", "5px")
                .style("left", (window.pageXOffset + matrix.e + 15) + "px")
                .style("top", (window.pageYOffset + matrix.f + 15) + "px");
        }

        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            var tooltipContent = [];
            var genBankLink = "<a target='_blank' rel='noopener' href='https://www.ncbi.nlm.nih.gov/nuccore/";

            var header = d.Matter.RemoteId ? genBankLink + d.Matter.RemoteId + "'>" + d.Matter.MatterName + "</a>" : d.Matter.MatterName;
            tooltipContent.push(header);

            if (d.RemoteId) {
                var peptideGenbankLink = genBankLink + d.RemoteId + "'>Peptide ncbi page</a>";
                tooltipContent.push(peptideGenbankLink);
            }

            tooltipContent.push($scope.features[d.FeatureId]);
            tooltipContent.push($scope.getAttributesText(d.Attributes));

            if (d.Partial) {
                tooltipContent.push("partial");
            }

            var start = d.Starts[0] + 1;
            var end = d.Starts[0] + d.Lengths[0];
            var positionGenbankLink = d.Matter.RemoteId ?
                                      genBankLink + d.Matter.RemoteId + "?from=" + start + "&to=" + end + "'>" + d.Starts.join(", ") + "</a>" :
                                      d.Starts.join(", ");
            tooltipContent.push("Position: " + positionGenbankLink);
            tooltipContent.push("Length: " + d.Lengths.join(", "));
            // TODO: show all characteristics
            tooltipContent.push("(" + $scope.xValue(d) + ", " + $scope.yValue(d) + ")");

            return tooltipContent.join("</br>");
        }

        // clears tooltip and unselects dots
        function clearTooltip(tooltip) {
            if (tooltip) {
                tooltip.html("").style("opacity", 0);

                if (tooltip.selectedDots) {
                    tooltip.selectedDots.attr("rx", $scope.dotRadius);
                }
            }
        }

        function isKeyLeftOrRight(keyCode) {
            return keyCode === 37 || keyCode === 39;
        }

        function xValue(d) {
            return $scope.lineChart ? d.Rank : d.CharacteristicsValues[+$scope.firstCharacteristic.Value];
        }

        function yValue(d) {
            return $scope.lineChart ? d.CharacteristicsValues[+$scope.firstCharacteristic.Value] : d.CharacteristicsValues[+$scope.secondCharacteristic.Value];
        }

        // main drawing method
        function draw() {
            $scope.showModalLoadingWindow("Drawing...");

            // removing previous chart and tooltip if any
            d3.select(".tooltip").remove();
            d3.select(".chart-svg").remove();

            // sorting points by selected characteristic
            if ($scope.lineChart) {
                for (var i = 0; i < $scope.sequencesData.length; i++) {
                    $scope.sequencesData[i].SubsequencesData.sort(function (first, second) {
                        return $scope.yValue(second) - $scope.yValue(first);
                    });

                    for (var j = 0; j < $scope.sequencesData[i].SubsequencesData.length; j++) {
                        $scope.sequencesData[i].SubsequencesData[j].Rank = j + 1;
                    }
                }
            }

            // all organisms are visible after redrawing
            $scope.sequencesData.forEach(function (matter) {
                matter.Visible = true;
                matter.SubsequencesData.forEach(function (point) {
                    point.MatterVisible = true;
                    //point.FeatureVisible = $scope.features[point.featureId].Selected;
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

            $scope.sequencesData.forEach(function (matter) {
                xMinArray.push(d3.min(matter.SubsequencesData, $scope.xValue));
                xMaxArray.push(d3.max(matter.SubsequencesData, $scope.xValue));
                yMinArray.push(d3.min(matter.SubsequencesData, $scope.yValue));
                yMaxArray.push(d3.max(matter.SubsequencesData, $scope.yValue));
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

            $scope.xMap = function (d) { return xScale($scope.xValue(d)); };

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

            $scope.yMap = function (d) { return yScale($scope.yValue(d)); };

            // setup fill color
            var cValue = function (d) { return d.Matter.MatterId; };
            var color = d3.scaleOrdinal(d3.schemeCategory20);

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
                .data($scope.sequencesData)
                .enter()
                .append("g")
                .attr("class", "matter");

            if ($scope.lineChart) {
                var line = d3.line()
                    .x($scope.xMap)
                    .y($scope.yMap);

                $scope.sequencesData.forEach(function (matter) {
                    // Nest the entries by symbol
                    var dataNest = d3.nest()
                        .key(function (d) { return d.Matter.MatterId })
                        .entries(matter.SubsequencesData);

                    // Loop through each symbol / key
                    dataNest.forEach(function (d) {
                        svg.append("path")
                            .datum(d.values)
                            .attr("class", "line")
                            .attr("d", line)
                            .attr('stroke', function (d) { return color(cValue(d[0])); })
                            .attr('stroke-width', 1)
                            .attr('fill', 'none');
                    });
                });
            }

            // draw dots
            mattersGroups.selectAll(".dot")
                .data(function (d) {
                    return d.SubsequencesData;
                })
                .enter()
                .append("ellipse")
                .attr("class", "dot")
                .attr("rx", $scope.dotRadius)
                .attr("ry", $scope.dotRadius)
                .attr("cx", $scope.xMap)
                .attr("cy", $scope.yMap)
                .style("fill-opacity", 0.6)
                .style("fill", function (d) { return color(cValue(d)); })
                .style("stroke", function (d) { return color(cValue(d)); })
                .attr("visibility", function (dot) {
                    return $scope.dotVisible(dot) ? "visible" : "hidden";
                });

            // draw legend
            var legend = svg.selectAll(".legend")
                .data($scope.sequencesData)
                .enter().append("g")
                .attr("class", "legend")
                .attr("transform", function (d, i) { return "translate(0," + i * 20 + ")"; })
                .on("click", function (d) {
                    d.Visible = !d.Visible;
                    var legendEntry = d3.select(this);
                    legendEntry.select("text")
                        .style("opacity", function () { return d.Visible ? 1 : 0.5; });
                    legendEntry.select("rect")
                        .style("fill-opacity", function () { return d.Visible ? 1 : 0; });

                    mattersGroups.filter(function (matter) {
                        return matter.MatterId === d.MatterId;
                    })
                        .selectAll(".dot")
                        .attr("visibility", function (dot) {
                            dot.MatterVisible = d.Visible;
                            return $scope.dotVisible(dot) ? "visible" : "hidden";
                        });

                    svg.selectAll(".line")
                        .filter(function (line) { return line[0].Matter.MatterId === d.MatterId; })
                        .attr("visibility", function (line) {
                            return d.visible ? "visible" : "hidden";
                        });
                });

            // draw legend colored rectangles
            legend.append("rect")
                .attr("width", 15)
                .attr("height", 15)
                .style("fill", function (d) { return color(d.MatterId); })
                .style("stroke", function (d) { return color(d.MatterId); })
                .style("stroke-width", 4)
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")");

            // draw legend text
            legend.append("text")
                .attr("x", 24)
                .attr("y", 9)
                .attr("dy", ".35em")
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")")
                .text(function (d) { return d.MatterName; })
                .style("font-size", "9pt");

            // tooltip event bind
            d3.select("body").on("click", function () {
                var selectedPoints = mattersGroups.selectAll(".dot").filter(function () {
                    return this === d3.event.target;
                }).data();

                if (selectedPoints.length === 0) {
                    $scope.clearTooltip(tooltip);
                } else {
                    $scope.showTooltip(selectedPoints, tooltip, svg);
                }
            });

            // tooltip show on key up or key down
            d3.select("body")
                .on("keydown", function () {
                    var keyCode = d3.event.keyCode;
                    if (tooltip.selectedPoints && $scope.isKeyLeftOrRight(keyCode)) {
                        $scope.clearTooltip(tooltip);

                        var selectedPoint = tooltip.selectedPoints[0];
                        var subsequencesData = $scope.sequencesData
                            .find(function (p) { return p.MatterId === selectedPoint.Matter.MatterId; })
                            .SubsequencesData;

                        var indexOfPoint = subsequencesData.indexOf(selectedPoint);
                        switch (keyCode) {
                            case 37: // left
                                indexOfPoint--;
                                break;
                            case 39: // right
                                indexOfPoint++;
                                break;
                        }

                        var nextSlectedPoint = subsequencesData[indexOfPoint];
                        if (nextSlectedPoint) {
                            $scope.showTooltip([nextSlectedPoint], tooltip, svg);
                        }
                    }
                });

            // preventing scroll in key up and key down
            window.addEventListener("keydown", function (e) {
                if ($scope.isKeyLeftOrRight(e.keyCode)) {
                    e.preventDefault();
                }
            }, false);

            $scope.hideModalLoadingWindow();
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
        $scope.isKeyLeftOrRight = isKeyLeftOrRight;
        $scope.yValue = yValue;
        $scope.xValue = xValue;
        $scope.addFilter = addFilter;
        $scope.deleteFilter = deleteFilter;
        $scope.getProductAttributeId = getProductAttributeId;
        $scope.showModalLoadingWindow = showModalLoadingWindow;
        $scope.hideModalLoadingWindow = hideModalLoadingWindow;

        $scope.dotRadius = 3;
        $scope.selectedDotRadius = $scope.dotRadius * 3;
        $scope.visiblePoints = [];
        $scope.characteristicComparers = [];
        $scope.filters = [];
        $scope.productFilter = "";
        $scope.loadingModalWindow = $("#loadingDialog");

        $scope.showModalLoadingWindow("Loading subsequences characteristics");

        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $http.get("/api/TaskManagerWebApi/" + $scope.taskId)
            .then(function (data) {
                MapModelFromJson($scope, JSON.parse(data.data));

                $scope.legendHeight = $scope.sequencesData.length * 20;
                $scope.height = 800 + $scope.legendHeight;
                $scope.width = 800;

                $scope.firstCharacteristic = $scope.subsequencesCharacteristicsList[0];
                $scope.secondCharacteristic = $scope.subsequencesCharacteristicsList[$scope.subsequencesCharacteristicsList.length - 1];

                $scope.fillPoints();

                $scope.hideModalLoadingWindow();
            }, function () {
                alert("Failed loading subsequences characteristics");
            });
    }

    angular.module("libiada", []).controller("SubsequencesCalculationResultCtrl", ["$scope", "$http", "$sce", subsequencesCalculationResult]);
}