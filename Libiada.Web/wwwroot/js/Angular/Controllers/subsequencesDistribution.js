import { MapModelFromJson } from "functions";
// Main controller class
class SubsequencesDistributionManager {
    constructor(data) {
        this.ngOnInit(data);
    }
    /**
     * Initializes the Angular controller
     */
    ngOnInit(data) {
        // Define the controller function
        const subsequencesDistribution = ($scope) => {
            MapModelFromJson($scope, data);
        };
        // Register the controller in the Angular module
        angular.module("libiada").controller("SubsequencesDistributionCtrl", ["$scope", subsequencesDistribution]);
    }
}
// Export the constructor for use in _AngularControllerInitializer.cshtml
export default function SubsequencesDistributionController(data) {
    return new SubsequencesDistributionManager(data);
}
;
//# sourceMappingURL=subsequencesDistribution.js.map