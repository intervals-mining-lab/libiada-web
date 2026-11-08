import { MapModelFromJson } from "functions";
import { Group, Nature, SequenceType } from "viewDataTypes";

interface Multisequence {
    Id: number;
    Name: string;
    Nature: string;
    ResearchObjects: ResearchObject[];
}

interface ResearchObject {
    Id: number;
    Name: string;
    Description?: string;
    Nature: number;
    Group: number;
    SequenceType: number;
    Multisequence: Multisequence | null;
    MultisequenceId: number | null;
    MultisequenceNumber: number | null;
    Created: string;
    Modified: string;
    CollectionCountry: string | null;
    CollectionDate: string | null;
    CollectionLocation: string | null;
}

/**
 * Interface for the data object passed from the server
 */
interface ResearchObjectEditData {
    group: string;
    sequenceType: string;
    groups: Group[];
    multisequences: Multisequence[];
    nature: string;
    natures: Nature[];
    researchObject: ResearchObject;
    sequenceTypes: SequenceType[];
    sequencesCount: number;
}

/**
 * Interface for the angular controller's scope
 */
interface ResearchObjectEditScope extends ng.IScope, ResearchObjectEditData {
    groupsFiltered: Group[];
    sequenceTypesFiltered: SequenceType[];

    filterByNature: () => void;
}

// Angular controller class
class ResearchObjectEditor {
    constructor(data: ResearchObjectEditData) {
        this.ngOnInit(data);
    }

    private ngOnInit(data: ResearchObjectEditData): void {
        const researchObjectEdit = ($scope: ResearchObjectEditScope, filterFilter: ng.IFilterFilter): void => {
            MapModelFromJson($scope, data);

            function filterByNature(): void {
                if (angular.isDefined($scope.groups)) {
                    $scope.groupsFiltered = filterFilter($scope.groups, { Nature: $scope.nature });
                     $scope.group = $scope.groupsFiltered[0].Value;
                }
                if (angular.isDefined($scope.sequenceTypes)) {
                    $scope.sequenceTypesFiltered = filterFilter($scope.sequenceTypes, { Nature: $scope.nature });
                    $scope.sequenceType = $scope.sequenceTypesFiltered[0].Value;
                }
            }

            $scope.filterByNature = filterByNature;

        };

        angular.module("libiada").controller("ResearchObjectEditCtrl", ["$scope", "filterFilter", researchObjectEdit]);
    }
}

// Wrapper function for backwards compatibility
export default function ResearchObjectEditController(data: ResearchObjectEditData): ResearchObjectEditor {
    return new ResearchObjectEditor(data);
}
