angular.module('Charts', ['Directives.TableParse'])
    .controller('ChartsCtrl', ['$scope', function ($scope) {

        function draw() {
            var margin = { top: 30, right: 20, bottom: 30, left: 200 },
                    width = 800 - margin.left - margin.right,
                    height = 600 - margin.top - margin.bottom;

            // Set the ranges
            var x = d3.scaleLinear().range([0, width]);
            var y = d3.scaleLinear().range([height, 0]);

            // Define the axes
            var xAxis = d3.svg.axis().scale(x)
                .orient("bottom").ticks(5);

            var yAxis = d3.svg.axis().scale(y)
                .orient("left").ticks(5);

            // Define the line
            var valueline = d3.svg.line()
                .x(function (d) { return x(d.date); })
                .y(function (d) { return y(d.close); });

            // Define 'div' for tooltips
            var div = d3.select("body")
                .append("div")  // declare the tooltip div
                .attr("class", "tooltip")              // apply the 'tooltip' class
                .style("opacity", 0);                  // set the opacity to nil

            // Adds the svg canvas
            var svg = d3.select("body")
                .append("svg")
                .attr("width", width + margin.left + margin.right)
                .attr("height", height + margin.top + margin.bottom)
                .append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            // Get the data
            var data = $scope.data;

            data.forEach(function (d) {
                d.date = +d.date;
                d.close = +d.close;
                //d.name = +d.name;
            });

            // Scale the range of the data
            x.domain(d3.extent(data, function (d) { return d.date; }));
            y.domain(d3.extent(data, function (d) { return d.close; }));

            // Add the valueline path.
            svg.append("path")
                .attr("class", "line")
                .attr("d", valueline(data));

            // draw the scatterplot
            svg.selectAll("dot")
                .data(data)
                .enter().append("circle")
                .attr("r", 2)
                .attr("cx", function (d) { return x(d.date); })
                .attr("cy", function (d) { return y(d.close); })
                // Tooltip stuff after this
                .on("mouseover", function (d) {
                    div.transition()
                        .duration(700)
                        .style("opacity", 0);
                    div.transition()
                        .duration(400)
                        .style("opacity", .9);
                    div.html(
                        "Name = " + (d.name) +
                        "<br/>x = " + (d.date) +
                        "<br/>y = " + d.close)
                        .style("left", (d3.event.pageX) + "px")
                        .style("top", (d3.event.pageY - 28) + "px");
                });

            // Add the X Axis
            svg.append("g")
                .attr("class", "x axis")
                .attr("transform", "translate(0," + height + ")")
                .call(xAxis);

            // Add the Y Axis
            svg.append("g")
                .attr("class", "y axis")
                .call(yAxis);
        };

        $scope.draw = draw;

        $scope.rawPaste = '';
        $scope.parsedPaste = [];
        $scope.data = [];
        $scope.i = '';
        $scope.j = '';
        $scope.l = '';
        $scope.data.date = [];
        $scope.data.close = [];
        $scope.data.name = [];

        $scope.addX = function (i, j) {
            $scope.data = [];
            for (var k = 1; k < $scope.parsedPaste.length; k++) {
                $scope.data.push({ date: $scope.parsedPaste[k][i].replace(",", "."), close: $scope.parsedPaste[k][j].replace(",", "."), name: $scope.parsedPaste[k][0] });
            }
            draw();
        };
        $scope.addY = function (l) {
            $scope.data = [];

            for (var k = 1; k < $scope.parsedPaste.length; k++) {
                $scope.data.push({ date: k, close: $scope.parsedPaste[k][l].replace(",", "."), name: $scope.parsedPaste[k][0] });
            }
            draw();
        };
    }]);
