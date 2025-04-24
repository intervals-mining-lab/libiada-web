/// <reference types="angular" />
/// <reference path="../functions.d.ts" />

// Interface for the data object that is passed to the controller
interface IResearchObjectEditData {
    nature: number;
    natures: INature[];
    groups: IGroup[];
    sequenceTypes: ISequenceType[];
    multisequences?: IMultisequence[];
    researchObject?: IResearchObject;
    sequencesCount?: number;
    [key: string]: any; // For any additional properties
}

// Interface for scope in the controller
interface IResearchObjectEditScope extends ng.IScope {
    nature: string;
    natures: INature[];
    group: number;
    groups: IGroup[];
    groupsFiltered: IGroup[];
    sequenceType: number;
    sequenceTypes: ISequenceType[];
    sequenceTypesFiltered: ISequenceType[];
    multisequences?: IMultisequence[];
    researchObject?: IResearchObject;
    sequencesCount?: number;
    filterByNature: () => void;
    [key: string]: any; // For any additional properties
}

// Auxiliary interfaces
interface INature {
    Value: number;//? type string or num
    Text: string;
    Nature: number;
    Group: number;
    Disabled: boolean;
    Selected: boolean;
}

interface IGroup {
    Value: number;//? type string or num
    Text: string;
    Nature: number;
    Group: number;
    Disabled: boolean;
    Selected: boolean;
}

interface ISequenceType {
    Value: number;//? type string or num
    Text: string;
    Nature: number;
    Group: number;
    Disabled: boolean;
    Selected: boolean;
}

interface IMultisequence {
    Id: number;
    Name: string;
    Nature: number;
}

interface IResearchObject {
    Id?: number;
    Name?: string;
    Description?: string;
    MultisequenceId?: number;
    MultisequenceNumber?: number;//скрыт
    CollectionCountry?: string;
    CollectionDate?: string;
    [key: string]: any; // For any additional properties
}

// Updated controller class
class ResearchObjectEditControllerClass {
    private data: IResearchObjectEditData;

    constructor(data: IResearchObjectEditData) {
        this.data = data;
        this.initializeController();
    }

    private initializeController(): void {
        "use strict";

        const researchObjectEdit = ($scope: IResearchObjectEditScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, this.data);
           

            function filterByNature(): void {
                const arraysForFiltration: string[] = ["groups", "sequenceTypes"];

                arraysForFiltration.forEach((arrayName: string) => {
                    if (angular.isDefined($scope[arrayName])) {
                        $scope[`${arrayName}Filtered`] = filterFilter($scope[arrayName], { Nature: $scope.nature });

                       
                    }
                });

                $scope.group = $scope.groupsFiltered[0].Value;
                $scope.sequenceType = $scope.sequenceTypesFiltered[0].Value;

                

            }

            $scope.filterByNature = filterByNature;
        };

        angular.module("libiada").controller("ResearchObjectEditCtrl", ["$scope", "filterFilter", researchObjectEdit]);
    }
}

// Wrapper function for backwards compatibility
function ResearchObjectEditController(data: IResearchObjectEditData): ResearchObjectEditControllerClass {
    return new ResearchObjectEditControllerClass(data);
}
