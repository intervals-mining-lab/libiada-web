function ChartsController(data) {
    "use strict";

    function charts($scope) {

        function draw() {
            let margin = { top: 30, right: 20, bottom: 30, left: 200 },
                width = 800 - margin.left - margin.right,
                height = 600 - margin.top - margin.bottom;

            // Set the ranges
            let x = d3.scaleLinear().range([0, width]);
            let y = d3.scaleLinear().range([height, 0]);

            // Define the axes
            let xAxis = d3.svg.axis().scale(x)
                .orient("bottom").ticks(5);

            let yAxis = d3.svg.axis().scale(y)
                .orient("left").ticks(5);

            // Define the line
            let valueline = d3.svg.line()
                .x(d => x(d.date))
                .y(d => y(d.close));

            // Define 'div' for tooltips
            let div = d3.select("body")
                .append("div")  // declare the tooltip div
                .attr("class", "chart-tooltip position-absolute text-bg-light font-monospace small lh-sm p-1 rounded")
                .style("opacity", 0);                  // set the opacity to nil

            // Adds the svg canvas
            let svg = d3.select("body")
                .append("svg")
                .attr("width", width + margin.left + margin.right)
                .attr("height", height + margin.top + margin.bottom)
                .append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            // Get the data
            let data = $scope.data;

            data.forEach(function (d) {
                d.date = +d.date;
                d.close = +d.close;
                //d.name = +d.name;
            });

            // Scale the range of the data
            x.domain(d3.extent(data, d => d.date));
            y.domain(d3.extent(data, d => d.close));

            // Add the valueline path.
            svg.append("path")
                .attr("class", "line")
                .attr("d", valueline(data));

            // draw the scatterplot
            svg.selectAll("dot")
                .data(data)
                .enter().append("circle")
                .attr("r", 2)
                .attr("cx", d => x(d.date))
                .attr("cy", d => y(d.close))
                // Tooltip stuff after this
                .on("mouseover", (event, d) => {
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
                        .style("left", (event.pageX) + "px")
                        .style("top", (event.pageY - 28) + "px");
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

        $scope.rawPaste = "";
        $scope.parsedPaste = [];
        $scope.data = [];
        $scope.i = "";
        $scope.j = "";
        $scope.l = "";
        $scope.data.date = [];
        $scope.data.close = [];
        $scope.data.name = [];

        $scope.addX = (i, j) => {
            $scope.data = [];
            for (let k = 1; k < $scope.parsedPaste.length; k++) {
                $scope.data.push({ date: $scope.parsedPaste[k][i].replace(",", "."), close: $scope.parsedPaste[k][j].replace(",", "."), name: $scope.parsedPaste[k][0] });
            }
            draw();
        };
        $scope.addY = (l) => {
            $scope.data = [];

            for (let k = 1; k < $scope.parsedPaste.length; k++) {
                $scope.data.push({ date: k, close: $scope.parsedPaste[k][l].replace(",", "."), name: $scope.parsedPaste[k][0] });
            }
            draw();
        };
    }

    angular.module("libiada").controller("ChartsCtrl", ["$scope", charts]);
}
