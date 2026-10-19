import type * as plotly from "plotly.js";

interface SelectListItem {
    Value: string;
    Text: string;
    Selected: boolean;
    Disabled: boolean;
    Group: string | null; // TODO: check what type it could be
}

export interface SelectListItemWithNature extends SelectListItem {
    Nature: number;
}

export interface Characteristic extends SelectListItem { }

export interface SequencesGroup extends SelectListItem { }

export interface SequenceType extends SelectListItemWithNature { }

export interface Group extends SelectListItemWithNature { }

export interface Nature extends SelectListItem { }

export interface Notation extends SelectListItemWithNature { }

export interface RemoteDb extends SelectListItemWithNature { }

export interface Language extends SelectListItem { }

export interface PauseTreatment extends SelectListItem { }

export interface Trajectory extends SelectListItem { }

export interface Translator extends SelectListItem { }

export interface Feature extends SelectListItemWithNature { }

export interface ClusterizatorType extends SelectListItem { }

export interface ImageTransformer extends SelectListItem { }

export interface OrderTransformation extends SelectListItem { }

export interface SequenceGroupType extends SelectListItemWithNature { }

export interface SequenceGroup extends SelectListItemWithNature { }

export interface Multisequence extends SelectListItemWithNature { }

export interface DeviationCalculationMethod extends SelectListItem { }

export interface SegmentationCriterion extends SelectListItem { }

export interface Threshold extends SelectListItem { }

export interface Link extends SelectListItem { }

export interface Aggregator extends SelectListItem { }

export interface Aligner extends SelectListItem { }

export interface DistanceCalculator extends SelectListItem { }

export interface ArrangementType extends SelectListItem { }

export interface CharacteristicType extends SelectListItem {
    ArrangementTypes: ArrangementType[];
    Links: Link[];
}

export interface ResearchObject extends SelectListItemWithNature {
    Group: string;
    SequenceType: string;
}

export interface SequenceImportResult {
    Matter: string;
    Name: string;
    Nature: string;
    Status: string;
    ErrorMessage?: string;
}

export interface ImportResult {
    Status: string;
    ResearchObjectName: string;
    Result: string;
    Group?: string;
    SequenceType?: string;
}

export interface SequenceCharacteristics {
    ResearchObjectName: string;
    Characteristics: number[];
    SequenceGroupId?: number;
}

export interface LegendItem {
    id: number;
    name: string | number;
    visible: boolean;
    color: string;
}

export interface FragmentData {
    Characteristics: number[];
    Length: number;
    Name: string;
    Start: number;
}

export interface LocalCharacteristicsData {
    AutocorrelationData: number[][];
    DifferenceData: number[][];
    FourierData: number[][];
    FragmentsData: FragmentData[];
    ResearchObjectName: string;
}

// extension for plotly.js PlotData type to take into account parallel coordinates plot data
export interface ParallelCoordinatesPlotData extends plotly.PlotData {
    dimensions: {
        label: string;
        values: number[];
    }[]
}

export type DisplayedTable = "researchObjects" | "sequenceGroups";
