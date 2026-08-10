import { MapModelFromJson } from "functions";
/**
* Controller class for sequence mixer
*/
class SequenceMixerHandler {
    /**
    * Creates a new controller instance.
    * @param data Data to create the controller
    */
    constructor(data) {
        this.ngOnInit(data);
    }
    /**
    * Initializes the Angular controller.
    */
    ngOnInit(data) {
        const sequenceMixer = ($scope) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("SequenceMixerCtrl", ["$scope", sequenceMixer]);
    }
}
/**
* wrapper function for backward compatibility
* @param data Data to create controller
* @returns SequenceMixerHandler instance
*/
export default function SequenceMixerController(data) {
    return new SequenceMixerHandler(data);
}
//# sourceMappingURL=sequenceMixer.js.map