import api from "../api";
export interface HdsPricing { items: Record<string, number>; }
export interface HdsAdditionalItem { name: string; amount: number; quantity: number; }
export interface HdsItemRequest { parent: string; utilityName: string; width: string; height: string; depth: string; coreMaterial: string; shutterMaterial: string; additionalItems: HdsAdditionalItem[]; utilityNameOld: string; }
export interface HdsItemResponse { id: number; source: string; projectId: number; totalPrice: number; }
const getPricing = async (): Promise<HdsPricing> => (await api.get("/hds/pricing")).data;
const calculateAndSave = async (projectId: number, request: HdsItemRequest): Promise<HdsItemResponse> => (await api.post(`/projects/${projectId}/hds-items`, request)).data;
export default { getPricing, calculateAndSave };
