/*import * as d3 from "d3";*/
import type * as plotly from "plotly.js";
import type {
    Characteristic,
    LegendItem,
    ParallelCoordinatesPlotData,
    SequenceCharacteristics,
    SequencesGroup
} from "viewDataTypes";
import { getArrayMinMax, arrayMax, throwHelper } from "functions";

interface Point {
    name: string;
    legendIndex: number;
    characteristics: number[];
}

interface Tooltip {
    name: string;
    characteristics: number[];
}

/**
 * Interface for the angular controller's scope
 */
interface ChartsScope extends ng.IScope {
    characteristicsList: Characteristic[];
    characteristicNames: string[];
    characteristics: SequenceCharacteristics[];
    sequenceGroups?: SequencesGroup[];

    sequencesColumnName: string;
    rawData: string;
    parsedData: string[][];
    inFocus: boolean;

    //const key codes
    keyCodes: { C: 67, V: 86 };

    parseTabularData: (text: string) => string[][] | null;
    textChanged: () => void;
    handleKeyDown: (e: JQuery.KeyDownEvent) => void;
}

/**
 * Controller for custom data charts visualization
 */
class ChartsControllerHandler {
    constructor() {
        this.ngOnInit();
    }

    /**
     * Initializes the Angular controller
     */
    private ngOnInit(): void {
        const charts = ($scope: ChartsScope): void => {
            /**
             * Parses tabular data from text
             * @param text Text with tab-delimited data
             * @returns Array of parsed data rows or null if parsing fails
             */
            function parseTabularData(text: string): string[][] | null {
                //The array we will return
                let result: string[][] = [];
                try {
                    //Pasted data split into rows
                    let rows: string[] = text.split(/[\n\f\r]/);

                    // extracting first row that contains characteristics names
                    let row: string = rows.shift() ?? throwHelper("No data to parse");
                    let characteristics: string[] = row.split("\t");

                    // extracting sequence name column
                    $scope.sequencesColumnName = characteristics.shift() ?? throwHelper("No characteristics to parse");
                    let rawCharacteristics: string[][] = rows.map<string[]>(r => r.split("\t"));
                    $scope.characteristics = [];
                    for (let i = 0; i < rawCharacteristics.length; i++) {
                        $scope.characteristics[i] = {
                            Characteristics: [],
                            ResearchObjectName: rawCharacteristics[i][0]
                        };
                        for (let j = 1; j < rawCharacteristics[i].length; j++) {
                            $scope.characteristics[i].Characteristics[j - 1] = +rawCharacteristics[i][j].replace(",", ".");
                        }
                    }
                    $scope.characteristicsList = characteristics.map<Characteristic>((c, i) => ({
                        Value: i.toString(),
                        Text: c,
                        Selected: false,
                        Disabled: false
                     }));
                    $scope.characteristicNames = characteristics.map<string>(c => c);

                    rows.forEach(thisRow => {
                        let row: string = thisRow.trim();
                        if (row) {
                            let cols = row.split("\t").map(c => c.replace(",", "."));
                            result.push(cols);
                        }
                    });
                }
                catch (err: any) {
                    console.log("error parsing as tabular data");
                    console.log(err);
                    return null;
                }
                return result;
            }

            /**
             * Handle text changes in data paste box
             */
            function textChanged(): void {
                let text: string = $("#dataPasteBox").val() as string;
                if (text) {
                    $scope.rawData = text;
                    let parsedDataAsArray: string[][] | null = $scope.parseTabularData(text);
                    if (parsedDataAsArray) {
                        $scope.parsedData = parsedDataAsArray;
                        $scope.$apply();

                        $("button[data-bs-target='#charts-tab-pane']").tab("show");
                    }
                }
            }

            /**
             * Handle key down events for copy/paste
             * @param e Key event
             */
            function handleKeyDown(e: JQuery.KeyDownEvent): void {
                if (!$scope.inFocus && e.which === $scope.keyCodes.V && (e.ctrlKey || e.metaKey)) { // CTRL + V
                    //reset value of our box
                    $("#dataPasteBox").val("");
                    //set it in focus so that pasted text goes inside the box
                    $("#dataPasteBox").trigger("focus");
                }
            }

            // Assign functions to scope
            $scope.parseTabularData = parseTabularData;
            $scope.textChanged = textChanged;
            $scope.handleKeyDown = handleKeyDown;

            $scope.keyCodes = {
                C: 67,
                V: 86
            };
            $scope.inFocus = false;
            $scope.rawData = "";

            $(() => {
                //Handles the Ctrl + V keys for pasting

                //If this is true, we wont respond to Ctrl + V
                $("body").on("focus", "input, textarea", () => { $scope.inFocus = true; });

                //We are not on a text element so we will respond
                //to Ctrl + V
                $("body").on("blur", "input, textarea", () => { $scope.inFocus = false; });

                //Handle the key down event
                $(document).on("keydown", $scope.handleKeyDown);

                //We will respond to when the textbox value changes
                $("#dataPasteBox").on("input propertychange", $scope.textChanged);
            });
        };

        // Register controller
        angular.module("libiada").controller("ChartsCtrl", ["$scope", charts]);
    }
}

/**
 * Factory function for creating a charts controller
 * @param data Initial data for controller
 * @returns New instance of ChartsControllerHandler
 */
export default function ChartsController(): ChartsControllerHandler {
    return new ChartsControllerHandler();
}
