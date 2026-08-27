import api from "../api";
export interface OtherWoodworkPricing { pricingData: Record<string, Record<string, Record<string, number>>>; }
export interface OtherWoodworkAdditionalItem { name: string; amount: number; quantity: number; }
export interface OtherWoodworkItemRequest { parent: string; utilityName: string; width: string; height: string; depth: string; coreMaterial: string; shutterMaterial: string; additionalItems: OtherWoodworkAdditionalItem[]; utilityNameOld: string; }
export interface OtherWoodworkItemResponse { id: number; source: string; projectId: number; totalPrice: number; }
const getPricing = async (): Promise<OtherWoodworkPricing> => (await api.get("/other-woodwork/pricing")).data;
const calculateAndSave = async (projectId: number, request: OtherWoodworkItemRequest): Promise<OtherWoodworkItemResponse> => (await api.post(`/projects/${projectId}/other-woodwork-items`, request)).data;
const updateOrder = async (projectId: number, orderId: number, request: OtherWoodworkItemRequest): Promise<OtherWoodworkItemResponse> => (await api.put(`/projects/${projectId}/orders/${orderId}/other-woodwork`, request)).data;
export default { getPricing, calculateAndSave, updateOrder };
