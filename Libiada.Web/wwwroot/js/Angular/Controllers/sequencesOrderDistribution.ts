/**
 * Interface for the angular controller's scope
 */
interface ISequencesOrderDistributionScope extends ng.IScope {
}

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
    private ngOnInit(): void {
        const sequencesOrderDistribution = (_$scope: ISequencesOrderDistributionScope): void => {
        };

        // Register the controller in Angular
        angular.module("libiada").controller("SequencesOrderDistributionCtrl", ["$scope", sequencesOrderDistribution]);
    }
}

/**
* Wrapper function for backward compatibility
* @returns Order distribution controller instance
*/
export default function SequencesOrderDistributionController(): SequencesOrderDistributionHandler {
    return new SequencesOrderDistributionHandler();
}
