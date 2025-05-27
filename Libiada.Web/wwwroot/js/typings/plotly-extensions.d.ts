interface HTMLElement {
    on(event: string, callback: (data: any) => void): this;
}

declare namespace Plotly {
    function newPlot(element: HTMLElement, data: any[], layout?: any, config?: any): HTMLElement;
    function restyle(element: HTMLElement, update: any, indices?: number[]): Promise<any>;
    function relayout(element: HTMLElement, update: any): Promise<any>;
}
