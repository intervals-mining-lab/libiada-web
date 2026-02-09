import { MapModelFromJson } from "functions";
class AccordanceHandler {
    /**
     * Creates a new controller instance
     * @param data Data for controller initialization
     */
    constructor(data) {
        this.ngOnInit(data);
    }
    /**
     * Initializes Angular controller
     * @param data Data for controller initialization
     */
    ngOnInit(data) {
        const accordance = ($scope, filterFilter) => {
            MapModelFromJson($scope, data);
        };
        angular.module("libiada").controller("AccordanceCtrl", ["$scope", "filterFilter", accordance]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of accordance handler
 */
export default function AccordanceController(data) {
    return new AccordanceHandler(data);
}
//# sourceMappingURL=accordance.js.map