// Declaring global variables and functions
/// <reference types="angular" />
// Main controller class
class SubsequencesDistributionManager {
    constructor(data) {
        this.data = data;
        this.initialize();
    }
    initialize() {
        // Define the controller function
        const subsequencesDistribution = ($scope) => {
            MapModelFromJson($scope, this.data);
        };
        // Register the controller in the Angular module
        angular.module("libiada")
            .controller("SubsequencesDistributionCtrl", ["$scope", subsequencesDistribution]);
    }
}
// Export the constructor for use in _AngularControllerInitializer.cshtml
function SubsequencesDistributionController(data) {
    return new SubsequencesDistributionManager(data);
}
;
//# sourceMappingURL=subsequencesDistribution.js.map