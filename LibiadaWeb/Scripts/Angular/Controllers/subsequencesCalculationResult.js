(function () {
    'use strict';

    angular
        .module('app')
        .controller('subsequencesCalculationResult', subsequencesCalculationResult);

    subsequencesCalculationResult.$inject = ['$scope']; 

    function subsequencesCalculationResult($scope) {
        $scope.title = 'subsequencesCalculationResult';

        activate();

        function activate() { }
    }
})();
