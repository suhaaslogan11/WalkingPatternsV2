import api from "../api";
import type { Client } from "../models/Client";

const getClients = async (): Promise<Client[]> => {
    const response = await api.get("/Clients");
    return response.data;
};

const addClient = async (client: Client): Promise<Client> => {
    const response = await api.post("/Clients", client);
    return response.data;
};

export default {
    getClients,
    addClient
};