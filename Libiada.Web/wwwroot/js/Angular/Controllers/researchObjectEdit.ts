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
    Value: number;
    Text: string;
    Nature: number;
    Group: number;
    Disabled: boolean;
    Selected: boolean;
}

interface IGroup {
    Value: number;
    Text: string;
    Nature: number;
    Group: number;
    Disabled: boolean;
    Selected: boolean;
}

interface ISequenceType {
    Value: number;
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
    // Basic identifiers
    Id?: number;
    Name?: string;
    Description?: string;

    // Object classification
    Nature: number;
    Group: number;
    SequenceType: number;

    // Multisequence information
    Multisequence: any | null;
    MultisequenceId: number | null;
    MultisequenceNumber: number | null;

    // Metadata
    Created: string;
    Modified: string;

    // Location information
    CollectionCountry: string | null;
    CollectionDate: string | null;
    CollectionLocation: string | null;

    // Related sources
    Source: string | null;

    // Related collections
    Groups: IGroup[]; // Array of groups the object belongs to
    Sequences: any[]; // Array of related sequences
    ImageSequences: any[]; // Array of related sequence images

    // Parameters for organizing the interface (can be used in components)
    Selected?: boolean; // Flag of selection in the list of objects
    Visible?: boolean; // Flag of visibility in the list of objects
    Value?: number; // Value for use in form components
    Text?: string; // Text representation for form components

    // Additional properties for backward compatibility
    [key: string]: any;
}


// Updated controller class
class ResearchObjectEditor {
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
function ResearchObjectEditController(data: IResearchObjectEditData): ResearchObjectEditor   {
    return new ResearchObjectEditor  (data);
}
