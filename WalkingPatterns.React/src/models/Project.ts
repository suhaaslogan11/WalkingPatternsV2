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

export interface ModuleSummary {
    projectDetailId: number;
    roomName: string;
    woodwork: number;
    accessories: number;
    services: number;
    total: number;
}

export interface ProjectDetailPage {
    projectId: number;
    clientName: string;
    projectName: string;
    projectDate: string;
    versionNumber: string;
    modules: ModuleSummary[];
}

export interface OrderDetail {
    orderId: number;
    parent?: string;
    materials?: string;
    width?: string;
    height?: string;
    depth?: string;
    accessories?: string;
    quantities?: string;
    additionalItemName?: string;
    additionalItemsAmounts?: string;
    additionalItemsQuantities?: string;
    materialTotal: number;
    accessoriesTotal: number;
    additionalItemsTotal: number;
    totalPrice: number;
    utilityNameOld?: string;
    orderDate: string;
}

export interface ProjectOrders {
    projectDetailId: number;
    roomName: string;
    orders: OrderDetail[];
}

export interface ProjectFinancials {
    grandTotal: number;
    discountAmount: number;
    discountedTotal: number;
}
