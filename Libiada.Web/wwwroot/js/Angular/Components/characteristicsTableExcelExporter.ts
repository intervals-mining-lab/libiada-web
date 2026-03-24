//import type ExcelJS from "../../../lib/exceljs/exceljs.js";
import type { SequenceCharacteristics, SequencesGroup } from "viewDataTypes";

interface ExcelRow {
    id: number;
    name: string;
    sequenceGroup?: number;
    [key: string]: string | number | undefined;
}

interface CharacteristicsTableExcelExporterComponentController extends ng.IController {
    excelFileName: string;
    sequenceGroups?: SequencesGroup[];
    characteristics: SequenceCharacteristics[];
    characteristicNames: string[];

    exportToExcel: () => void;
}

function CharacteristicsTableExcelExporterController(this: CharacteristicsTableExcelExporterComponentController) {
    const ctrl: CharacteristicsTableExcelExporterComponentController = this;

    ctrl.exportToExcel = async function exportToExcel() {
        const workbook = new ExcelJS.Workbook();
        const worksheet = workbook.addWorksheet("My Sheet");
        const font = { name: "Courier New" };
        const border = { top: { style: "thin" }, left: { style: "thin" }, bottom: { style: "thin" }, right: { style: "thin" } }
        let columns = [
            { header: "№", key: "id", width: 10, style: { font: font, border: border } },
            { header: "Sequence name", key: "name", width: 32, style: { font: font, border: border } }
        ];

        if (ctrl.sequenceGroups) columns.push({ header: "Sequences group", key: "sequenceGroup", width: 10, style: { font: font, border: border } });

        columns = columns.concat(ctrl.characteristicNames.map(cn => ({ header: cn, key: cn, width: 20, style: { font: font, border: border } })));

        worksheet.columns = columns;

        for (let i = 0; i < ctrl.characteristics.length; i++) {
            let row: ExcelRow = { id: i + 1, name: ctrl.characteristics[i].ResearchObjectName };

            if (ctrl.sequenceGroups) row.sequenceGroup = ctrl.characteristics[i].SequenceGroupId;

            ctrl.characteristics[i].Characteristics.forEach((cv, j) => row[ctrl.characteristicNames[j]] = cv);
            worksheet.addRow(row).commit();
        }

        const buffer = await workbook.xlsx.writeBuffer();

        const blob: Blob = new Blob([buffer], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8" });

        // TODO: rewrite it using browser file API
        let saveBlobAsFile = function (blob: Blob) {
            let a: HTMLAnchorElement = document.createElement("a");
            document.body.appendChild(a);
            a.style = "display: none";
            let url: string = window.URL.createObjectURL(blob);
            a.href = url;
            a.download = ctrl.excelFileName ?? "Results";
            a.click();
            window.URL.revokeObjectURL(url);
            a.remove();
        };

        saveBlobAsFile(blob);
    }
}

angular.module("libiada").component("characteristicsTableExcelExporter", {
    templateUrl: `/AngularTemplates/_CharacteristicsTableExcelExporter`,
    controller: CharacteristicsTableExcelExporterController,
    bindings: {
        characteristicNames: "<",
        characteristics: "<",
        sequenceGroups: "<?"
    }
});
