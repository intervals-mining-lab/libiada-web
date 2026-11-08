/**
* Controller for sequence order distribution
*/
class SequencesOrderDistributionHandler {
    constructor() {
        this.ngOnInit();
    }
    /**
    * Initializes the Angular controller
    */
    ngOnInit() {
        const sequencesOrderDistribution = (_$scope) => {
        };
        // Register the controller in Angular
        angular.module("libiada").controller("SequencesOrderDistributionCtrl", ["$scope", sequencesOrderDistribution]);
    }
}
/**
* Wrapper function for backward compatibility
* @returns Order distribution controller instance
*/
export default function SequencesOrderDistributionController() {
    return new SequencesOrderDistributionHandler();
}
//# sourceMappingURL=sequencesOrderDistribution.js.map