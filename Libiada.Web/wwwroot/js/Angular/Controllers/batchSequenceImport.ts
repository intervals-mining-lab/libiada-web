interface Accession {
    value: string;
}

/**
 * Interface for the angular controller's scope
 */
interface BatchSequenceImportScope extends ng.IScope {
    accessionsField: string;
    accessions: Accession[];

    parseIds: () => void;
    deleteId: (accession: Accession) => void;
}

/**
 * Angular controller class
 */
class BatchSequenceImportHandler {
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes Angular controller
     */
    private ngOnInit(): void {
        const batchSequenceImport = ($scope: BatchSequenceImportScope): void => {
            $scope.parseIds = function (): void {
                let splitted: string[] = $scope.accessionsField.split(/[^\w.]/);
                for (let i = 0; i < splitted.length; i++) {
                    if (splitted[i]) {
                        $scope.accessions.push({ value: splitted[i] });
                    }
                }

                $scope.accessionsField = "";
            }

            $scope.deleteId = function (accession: Accession): void {
                $scope.accessions.splice($scope.accessions.indexOf(accession), 1);
            }

            $scope.accessions = [];
        };

        // Register controller in Angular module
        angular.module("libiada").controller("BatchSequenceImportCtrl", ["$scope", batchSequenceImport]);
    }
}

/**
 * Wrapper function for backward compatibility
 * @returns Instance of batch sequence import handler
 */
export default function BatchSequenceImportController(): BatchSequenceImportHandler {
    return new BatchSequenceImportHandler();
}
