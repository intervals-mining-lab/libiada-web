// Angular controller class
class MusicFilesOperator {
    constructor() {
        this.ngOnInit();
    }
    ngOnInit() {
        const musicFiles = () => {
        };
        angular.module("libiada").controller("MusicFilesCtrl", [musicFiles]);
    }
}
// Wrapper function for backwards compatibility
export default function MusicFilesController() {
    return new MusicFilesOperator();
}
//# sourceMappingURL=musicFiles.js.map