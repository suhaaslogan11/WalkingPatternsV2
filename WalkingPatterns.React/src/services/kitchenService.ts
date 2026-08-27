import api from "../api";

export interface KitchenAccessoryOption {
    name: string;
    price: number;
}

export interface KitchenPricing {
    materials: Record<string, number>;
    parentOptions: Record<string, KitchenAccessoryOption[]>;
}

export interface KitchenAdditionalItem {
    name: string;
    amount: number;
    quantity: number;
}

export interface KitchenItemRequest {
    parent: string;
    utilityName: string;
    width: string;
    height: string;
    depth: string;
    materials: string;
    accessories: string[];
    quantities: string[];
    additionalItems: KitchenAdditionalItem[];
    utilityNameOld: string;
}

export interface KitchenItemResponse {
    id: number;
    source: string;
    projectId: number;
    totalPrice: number;
}

const getPricing = async (): Promise<KitchenPricing> => {
    const response = await api.get("/kitchen/pricing");
    return response.data;
};

const calculateAndSave = async (
    projectId: number,
    request: KitchenItemRequest
): Promise<KitchenItemResponse> => {
    const response = await api.post(`/projects/${projectId}/kitchen-items`, request);
    return response.data;
};

const updateOrder = async (projectId: number, orderId: number, request: KitchenItemRequest): Promise<KitchenItemResponse> => {
    const response = await api.put(`/projects/${projectId}/orders/${orderId}/kitchen`, request);
    return response.data;
};

export default { getPricing, calculateAndSave, updateOrder };
