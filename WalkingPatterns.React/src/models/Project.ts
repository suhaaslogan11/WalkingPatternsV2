export interface Project {
    id: number;
    projectName: string;
    projectDate: string;
    clientId: number;
    clientName: string;
    versionNumber: string;
    grandTotal: number;
    discountAmount: number;
    discountedTotal: number;
}

export interface AddProjectRequest {
    projectName: string;
    projectDate: string;
    versionNumber?: string;
}
