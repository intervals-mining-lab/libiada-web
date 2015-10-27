function CalculationResultController(data) {
    "use strict";

    function calculationResult($scope) {
        MapModelFromJson($scope, data);

        function prepareDataAndDraw() {
            var points = [];
            var first = +$scope.firstCharacteristic.Value;
            var second = +$scope.secondCharacteristic.Value;
            for (var i = 0; i < $scope.characteristics.length; i++) {
                points.push({
                    name: $scope.characteristics[i].matterName,
                    x: $scope.characteristics[i].characteristics[first],
                    y: $scope.characteristics[i].characteristics[second]
                });
            }

            $scope.drawScatter(points);
        }

        function drawScatter(points) {
            var margin = { top: 30, right: 20, bottom: 30, left: 200 };
            var width = 800 - margin.left - margin.right;
            var height = 600 - margin.top - margin.bottom;

            var x = d3.scale.linear().range([0, width]);
            var y = d3.scale.linear().range([height, 0]);

            var chart = dc.seriesChart("#chart");
            var ndx = crossfilter(points);
            var runDimension = ndx.dimension(function (d, index) {
                 return [d.name, +d.x]; });
            var runGroup = runDimension.group().reduceSum(function (d) { return +d.y; });

            var symbolScale = d3.scale.ordinal().range(d3.svg.symbolTypes);
            var symbolAccessor = function (d) { return symbolScale(d.key[0]); };
            var subChart = function (c) {
                return dc.scatterPlot(c)
                    .symbol(symbolAccessor)
                    .symbolSize(8)
                    .highlightedSize(10);
            };

            chart
                .chart(subChart)
                .x(x)
                .y(y)
                .width(width)
                .height(height)
                .brushOn(false)
                .xAxisLabel($scope.firstCharacteristic.Text)
                .yAxisLabel($scope.secondCharacteristic.Text)
                .clipPadding(10)
                .elasticX(true)
                .elasticY(true)
                .dimension(runDimension)
                .group(runGroup)
                .mouseZoomable(true)
                 .seriesAccessor(function (d) { return d.key[0]; })
    .keyAccessor(function (d) { return d.key[1]; })
    .valueAccessor(function (d) { return +d.value; })
                .legend(dc.legend().x(350).y(350).itemHeight(13).gap(5).horizontal(1).legendWidth(140).itemWidth(70));
            //chart.yAxis().tickFormat(function (d) { return d3.format(",d")(d + 299500); });

            dc.renderAll();
        }

        $scope.drawScatter = drawScatter;
        $scope.prepareDataAndDraw = prepareDataAndDraw;
    }

    angular.module("CalculationResult", []).controller("CalculationResultCtrl", ["$scope", calculationResult]);
}
