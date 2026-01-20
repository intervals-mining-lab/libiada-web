import { MapModelFromJson } from "functions";
/**
 * Controller for genes import functionality
 */
class GenesImportHandler {
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
        const genesImport = ($scope) => {
            MapModelFromJson($scope, data);
        };
        // Register controller in Angular module
        angular.module("libiada").controller("GenesImportCtrl", ["$scope", genesImport]);
    }
}
/**
 * Wrapper function for backward compatibility
 * @param data Data for controller initialization
 * @returns Instance of genes import handler
 */
export default function GenesImportController(data) {
    return new GenesImportHandler(data);
}
//# sourceMappingURL=genesImport.js.map