import api from "../api";

export interface BedroomPricing {
    pricingData: Record<string, Record<string, Record<string, number>>>;
}
export interface BedroomAdditionalItem { name: string; amount: number; quantity: number; }
export interface BedroomItemRequest {
    parent: string; utilityName: string; width: string; height: string; depth: string;
    coreMaterial: string; shutterMaterial: string; additionalItems: BedroomAdditionalItem[]; utilityNameOld: string;
}
export interface BedroomItemResponse { id: number; source: string; projectId: number; totalPrice: number; }

const getPricing = async (): Promise<BedroomPricing> => (await api.get("/bedroom/pricing")).data;
const calculateAndSave = async (projectId: number, request: BedroomItemRequest): Promise<BedroomItemResponse> =>
    (await api.post(`/projects/${projectId}/bedroom-items`, request)).data;
const updateOrder = async (projectId: number, orderId: number, request: BedroomItemRequest): Promise<BedroomItemResponse> =>
    (await api.put(`/projects/${projectId}/orders/${orderId}/bedroom`, request)).data;

export default { getPricing, calculateAndSave, updateOrder };
