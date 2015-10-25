function CalculationResultController(data) {
    "use strict";

    function calculationResult($scope) {
        MapModelFromJson($scope, data);

        function prepareDataAndDraw() {
            var points = [];
            var first = +$scope.firstCharacteristic.Value;
            var second = +$scope.secondCharacteristic.Value;
            for(var i = 0; i < $scope.characteristics.length; i++) {
                points.push({ x: $scope.characteristics[i][first], y: $scope.characteristics[i][second] });
            }

            $scope.drawScatter(points);
        }

        function drawScatter(points) {
            var chart = dc.seriesChart("#chart");
            var ndx = crossfilter(points);
            var runDimension = ndx.dimension(function (d) { return [+d.x, +d.y]; });
            var runGroup = runDimension.group().reduceSum(function (d) { return +d.x; });

            var symbolScale = d3.scale.ordinal().range(d3.svg.symbolTypes);
            var symbolAccessor = function (d) { return symbolScale(d.key[0]); };
            var subChart = function (c) {
                return dc.scatterPlot(c)
                    .symbol(symbolAccessor)
                    .symbolSize(8)
                    .highlightedSize(10);
            };

            chart
                .width(768)
                .height(480)
                .chart(subChart)
                .x(d3.scale.linear().domain([0, 20]))
                .brushOn(false)
                .yAxisLabel("Measured Speed km/s")
                .xAxisLabel("Run")
                .clipPadding(10)
                .elasticY(true)
                .dimension(runDimension)
                .group(runGroup)
                .mouseZoomable(true)
                .seriesAccessor(function (d) { return "Expt: " + d.key[0]; })
                .keyAccessor(function (d) { return +d.key[1]; })
                .valueAccessor(function (d) { return +d.value - 500; })
                .legend(dc.legend().x(350).y(350).itemHeight(13).gap(5).horizontal(1).legendWidth(140).itemWidth(70));
            chart.yAxis().tickFormat(function (d) { return d3.format(",d")(d + 299500); });
            chart.margins().left += 40;

            dc.renderAll();
        }

        $scope.drawScatter = drawScatter;
        $scope.prepareDataAndDraw = prepareDataAndDraw;
    }

    angular.module("CalculationResult", []).controller("CalculationResultCtrl", ["$scope", calculationResult]);
}
