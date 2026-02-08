// Controller class
class MusicFilesOperator {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const musicFiles = ($scope) => {
        };
        angular.module("libiada").controller("MusicFilesCtrl", ["$scope", musicFiles]);
    }
}
// Wrapper function for backwards compatibility
export default function MusicFilesController() {
    return new MusicFilesOperator();
}
//# sourceMappingURL=musicFiles.js.map