/// <reference types="angular" />
/// <reference types="functions" />

/**
* Interface for link
*/
interface ILink {
    id: number;
    name: string;
}

/**
* Interface for placement type
*/
interface IArrangementType {
    id: number;
    name: string;
}

/**
* Interface for characteristic type
*/
interface ICharacteristicType {
    id: number;
    name: string;
    description?: string;
    Links: ILink[];
    ArrangementTypes: IArrangementType[];
    //[key: string]: any;
}

/**
* Interface for notation
*/
interface INotation {
    id: number;
    name: string;
    Nature: number;
}

/**
* Interface for the characteristic
*/
interface ICharacteristic {
    characteristicType: ICharacteristicType;
    link?: ILink;
    arrangementType?: IArrangementType;
    notation: INotation;
    //[key: string]: any;
}

/**
* Interface for order transformation controller data
*/
interface IOrderTransformerData {
    // Display settings
    hideNotation?: boolean;

    // Filtering properties
    nature?: number;
    natures?: any[];
    notations?: INotation[];

    // Language settings
    languages?: string[];
    translators?: string[];
    pauseTreatments?: any[];

    // Characteristics
    characteristics?: ICharacteristic[];

    // Additional properties
    [key: string]: any;
}

/**
* Interface for the scope order transformation controller
*/
interface IOrderTransformerScope extends ng.IScope {
    // Display settings
    hideNotation?: boolean;

    // Properties for filtering
    nature?: number;
    natures?: any[];
    notations?: INotation[];
    notation?: INotation;

    // Language settings
    languages?: string[];
    language?: string;
    translators?: string[];
    translator?: string;
    pauseTreatments?: any[];
    pauseTreatment?: any;

    // Characteristics
    characteristics?: ICharacteristic[];

    // Methods
    filterByNature: () => void;

    // Additional properties
    [key: string]: any;
}
/**
* Controller for order transformation
*/
class OrderTransformerHandler {
    private data: IOrderTransformerData;

    /**
    * Creates an instance of the order transformation controller
    * @param data Data for initializing the controller
    */
    constructor(data: IOrderTransformerData) {
        this.data = data;
        this.initializeController();
    }

    /**
    * Initializes the Angular controller
    */
    private initializeController(): void {
        "use strict";

        const orderTransformer = ($scope: IOrderTransformerScope, filterFilter: ng.IFilterFilter): void => {
            // Initialize scope with data from the parameter
            MapModelFromJson($scope, this.data);

            /**
            * Filters notations by the selected nature
            */
            function filterByNature(): void {
                if (!$scope.hideNotation) {
                    $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];

                    // If the notation is not associated with a characteristic
                    if ($scope.characteristics) {
                        angular.forEach($scope.characteristics, (characteristic: ICharacteristic) => {
                            characteristic.notation = $scope.notation;
                        });
                    }
                }
            }

            // Assign methods to $scope
            $scope.filterByNature = filterByNature;

            // Initialize the selected values
            $scope.notation = filterFilter($scope.notations, { Nature: $scope.nature })[0];
            $scope.language = $scope.languages ? $scope.languages[0] : undefined;
            $scope.translator = $scope.translators ? $scope.translators[0] : undefined;
            $scope.pauseTreatment = $scope.pauseTreatments ? $scope.pauseTreatments[0] : undefined;
        };

        // Register the controller in Angular
        angular.module("libiada").controller("OrderTransformerCtrl", ["$scope", "filterFilter", orderTransformer]);
    }
}

/**
* Wrapper function for backward compatibility
* @param data Data for controller initialization
* @returns Order transformation controller instance
*/
function OrderTransformerController(data: IOrderTransformerData): OrderTransformerHandler {
    return new OrderTransformerHandler(data);
}